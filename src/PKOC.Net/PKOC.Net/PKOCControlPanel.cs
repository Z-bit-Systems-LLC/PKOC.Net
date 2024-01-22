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
    public class PKOCControlPanel : IDisposable
    {
        private static readonly byte[] PSIAVendorCode = { 0x1A, 0x90, 0x21};
        
        private readonly SemaphoreSlim _initializeLock = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(0, 1);
        private readonly ControlPanel _panel;
        private readonly ConcurrentBag<DeviceSettings> _deviceSettings = new ConcurrentBag<DeviceSettings>();
        
        public PKOCControlPanel(ControlPanel panel)
        {
            _panel = panel;
            
            _panel.ManufacturerSpecificReplyReceived += OnPanelOnManufacturerSpecificReplyReceived;
        }

        /// <summary>
        /// Initializes the PKOC device.
        /// </summary>
        /// <param name="deviceSettings">The device identification settings.</param>
        /// <returns>Returns a boolean indicating the success of the initialization.</returns>
        public async Task<bool> InitializePKOC(DeviceSettings deviceSettings)
        {
            await _initializeLock.WaitAsync();

            try
            {
                if (_deviceSettings.Any(setting => setting.Equals(deviceSettings)))
                    throw new Exception(
                        "Device is already initialized for PKOC at the requested connection ID and address.");

                bool success = await _panel.ACUReceivedSize(deviceSettings.ConnectionId, deviceSettings.Address,
                    deviceSettings.MaximumReceiveSize);
                success &= await _panel.KeepReaderActive(deviceSettings.ConnectionId, deviceSettings.Address,
                    (ushort)deviceSettings.CardReadTimeout.TotalMilliseconds);

                if (success)
                {
                    _deviceSettings.Add(deviceSettings);
                }

                return success;
            }
            finally
            {
                _initializeLock.Release();
            }
        }

        public async Task<AuthenticationResponseData> AuthenticationRequest(DeviceSettings deviceSettings)
        {
            deviceSettings.ClearIncomingData();

            DateTime startReadTime = DateTime.UtcNow;

            await _panel.ManufacturerSpecificCommand(deviceSettings.ConnectionId, deviceSettings.Address,
                new ManufacturerSpecific(PSIAVendorCode, new AuthenticationRequestData(
                        new byte[] { 0x01, 0x00 },
                        deviceSettings.ReaderIdentifier, deviceSettings.CreateRandomTransactionId(), 0x00).BuildData()
                    .ToArray()), deviceSettings.MaximumFragmentSendSize,
                deviceSettings.CardReadTimeout, CancellationToken.None);


            await _lock.WaitAsync(deviceSettings.CardReadTimeout - (DateTime.UtcNow - startReadTime));
            
            return deviceSettings.AuthenticationResponseData();
        }

        public event EventHandler<CardPresentedEventArgs> CardPresented;

        private void InvokeCardPresented(CardPresentResponseData cardPresentResponseData)
        {
            CardPresented?.Invoke(this, new CardPresentedEventArgs(cardPresentResponseData));
        }
        
        public event EventHandler<ReaderErrorReportedEventArgs> ReaderErrorReported;

        private void InvokeReaderErrorReported(ReaderErrorResponseData readerErrorResponseData)
        {
            ReaderErrorReported?.Invoke(this, new ReaderErrorReportedEventArgs(readerErrorResponseData));
        }

        private PKOCMessageIdentifier IdentifyMessage(IEnumerable<byte> data)
        {
            return (PKOCMessageIdentifier)data.First();
        }
        
        private void ProcessAuthenticationResponse(DataFragmentResponse manufacturerSpecificData, DeviceSettings deviceSettings)
        {
            if (deviceSettings.IsDataCleared())
            {
                deviceSettings.AllocateIncomingData(manufacturerSpecificData.WholeMessageLength);
            }

            if (Utilities.BuildMultiPartMessageData(manufacturerSpecificData, deviceSettings))
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
                InvokeCardPresented(CardPresentResponseData.ParseData(eventArgs.ManufacturerSpecific.Data.ToArray()));
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
                InvokeReaderErrorReported(
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