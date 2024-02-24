using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OSDP.Net;
using OSDP.Net.Model.ReplyData;
using PKOC.Net.MessageData;
using ManufacturerSpecific = OSDP.Net.Model.CommandData.ManufacturerSpecific;

namespace PKOC.Net
{
    /// <summary>
    /// Represents a control panel for PKOC devices.
    /// </summary>
    public class PKOCControlPanel : IDisposable
    {
        private static readonly byte[] PSIAVendorCode = { 0x1A, 0x90, 0x21};
        
        private readonly SemaphoreSlim _initializeLock = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(0, 1);
        private readonly ControlPanel _panel;
        private readonly ConcurrentBag<DevicePKOCSettings> _deviceSettings = new ConcurrentBag<DevicePKOCSettings>();
        
        /// <summary>
        /// Initialize a new instance of the <see cref="PKOCControlPanel"/> class.
        /// </summary>
        /// <param name="panel">The control panel to maintain a communication session with OSDP devices.</param>
        public PKOCControlPanel(ControlPanel panel)
        {
            _panel = panel;
            
            _panel.ManufacturerSpecificReplyReceived += OnPanelOnManufacturerSpecificReplyReceived;
        }

        /// <summary>
        /// Initializes the PKOC device.
        /// </summary>
        /// <param name="devicePKOCSettings">The device identification settings.</param>
        /// <returns>Returns a boolean indicating the success of the initialization.</returns>
        public async Task<bool> InitializePKOC(DevicePKOCSettings devicePKOCSettings)
        {
            await _initializeLock.WaitAsync();

            try
            {
                if (_deviceSettings.Any(setting => setting.Equals(devicePKOCSettings)))
                    throw new Exception(
                        "Device is already initialized for PKOC at the requested connection ID and address.");

                bool success = await _panel.ACUReceivedSize(devicePKOCSettings.ConnectionId, devicePKOCSettings.Address,
                    devicePKOCSettings.MaximumReceiveSize);
                success &= await _panel.KeepReaderActive(devicePKOCSettings.ConnectionId, devicePKOCSettings.Address,
                    (ushort)devicePKOCSettings.CardReadTimeout.TotalMilliseconds);

                if (success)
                {
                    _deviceSettings.Add(devicePKOCSettings);
                }

                return success;
            }
            finally
            {
                _initializeLock.Release();
            }
        }

        /// <summary>
        /// Sends an authentication request to a PKOC control panel.
        /// </summary>
        /// <param name="devicePKOCSettings">The device settings.</param>
        /// <returns>The authentication response.</returns>
        public async Task<AuthenticationResponseData> AuthenticationRequest(DevicePKOCSettings devicePKOCSettings)
        {
            devicePKOCSettings.ClearIncomingData();

            DateTime startReadTime = DateTime.UtcNow;

            await _panel.ManufacturerSpecificCommand(devicePKOCSettings.ConnectionId, devicePKOCSettings.Address,
                new ManufacturerSpecific(PSIAVendorCode, new AuthenticationRequestData(
                        new byte[] { 0x01, 0x00 },
                        devicePKOCSettings.ReaderIdentifier, devicePKOCSettings.CreateRandomTransactionId(), 0x00).BuildData()
                    .ToArray()), devicePKOCSettings.MaximumFragmentSendSize,
                devicePKOCSettings.CardReadTimeout, CancellationToken.None);


            await _lock.WaitAsync(devicePKOCSettings.CardReadTimeout - (DateTime.UtcNow - startReadTime));
            
            return devicePKOCSettings.AuthenticationResponseData();
        }

        public event EventHandler<CardPresentedEventArgs> CardPresented;

        private void InvokeCardPresented(Guid connectionId, byte address,
            CardPresentResponseData cardPresentResponseData)
        {
            CardPresented?.Invoke(this, new CardPresentedEventArgs(connectionId, address, cardPresentResponseData));
        }

        public event EventHandler<ReaderErrorReportedEventArgs> ReaderErrorReported;

        private void InvokeReaderErrorReported(Guid connectionId, byte address,
            ReaderErrorResponseData readerErrorResponseData)
        {
            ReaderErrorReported?.Invoke(this,
                new ReaderErrorReportedEventArgs(connectionId, address, readerErrorResponseData));
        }

        private PKOCMessageIdentifier IdentifyMessage(IEnumerable<byte> data)
        {
            return (PKOCMessageIdentifier)data.First();
        }
        
        private void ProcessAuthenticationResponse(DataFragmentResponse manufacturerSpecificData, DevicePKOCSettings devicePKOCSettings)
        {
            if (devicePKOCSettings.IsDataCleared())
            {
                devicePKOCSettings.AllocateIncomingData(manufacturerSpecificData.WholeMessageLength);
            }

            if (Utilities.BuildMultiPartMessageData(manufacturerSpecificData, devicePKOCSettings))
            {
                _lock.Release();
            }
        }

        private void OnPanelOnManufacturerSpecificReplyReceived(object _,
            ControlPanel.ManufacturerSpecificReplyEventArgs eventArgs)
        {
            if (!IsPSIAVendorCode(eventArgs.ManufacturerSpecific.VendorCode)) return;

            // Only process replies that have initialized a device
            var deviceSettings =
                _deviceSettings.SingleOrDefault(setting => setting.Equals(eventArgs.ConnectionId, eventArgs.Address));
            if (deviceSettings == null) return;

            if (IdentifyMessage(eventArgs.ManufacturerSpecific.Data) == PKOCMessageIdentifier.CardPresentResponse)
            {
                InvokeCardPresented(eventArgs.ConnectionId, eventArgs.Address, CardPresentResponseData.ParseData(eventArgs.ManufacturerSpecific.Data.ToArray()));
            }
            else if (IdentifyMessage(eventArgs.ManufacturerSpecific.Data) ==
                     PKOCMessageIdentifier.AuthenticationResponse)
            {
                ProcessAuthenticationResponse(
                    DataFragmentResponse.ParseData(eventArgs.ManufacturerSpecific.Data.Skip(1).ToArray()),
                    deviceSettings);
            }
            else if (IdentifyMessage(eventArgs.ManufacturerSpecific.Data) == PKOCMessageIdentifier.ReaderErrorResponse)
            {
                InvokeReaderErrorReported(eventArgs.ConnectionId, eventArgs.Address,
                    ReaderErrorResponseData.ParseData(eventArgs.ManufacturerSpecific.Data.ToArray()));
            }
            else if (IdentifyMessage(eventArgs.ManufacturerSpecific.Data) ==
                     PKOCMessageIdentifier.TransactionRefreshResponse)
            {
            }
        }

        private static bool IsPSIAVendorCode(IEnumerable<byte> manufacturerSpecificVendorCode)
        {
            return manufacturerSpecificVendorCode.SequenceEqual(PSIAVendorCode);
        }

        public void Dispose()
        {
            _panel.ManufacturerSpecificReplyReceived -= OnPanelOnManufacturerSpecificReplyReceived;
            _lock?.Dispose();
        }
    }
}