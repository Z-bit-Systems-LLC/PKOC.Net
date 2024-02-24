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
        private readonly ConcurrentBag<PKOCDevice> _deviceSettings = new ConcurrentBag<PKOCDevice>();
        
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
        /// <param name="pkocDevice">The device identification settings.</param>
        /// <returns>Returns a boolean indicating the success of the initialization.</returns>
        public async Task<bool> InitializePKOC(PKOCDevice pkocDevice)
        {
            await _initializeLock.WaitAsync();

            try
            {
                if (_deviceSettings.Any(setting => setting.Equals(pkocDevice)))
                    throw new Exception(
                        "Device is already initialized for PKOC at the requested connection ID and address.");

                bool success = await _panel.ACUReceivedSize(pkocDevice.ConnectionId, pkocDevice.Address,
                    pkocDevice.MaximumReceiveSize);
                success &= await _panel.KeepReaderActive(pkocDevice.ConnectionId, pkocDevice.Address,
                    (ushort)pkocDevice.CardReadTimeout.TotalMilliseconds);

                if (success)
                {
                    _deviceSettings.Add(pkocDevice);
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
        /// <param name="pkocDevice">The device settings.</param>
        /// <returns>The authentication response.</returns>
        public async Task<AuthenticationResponseData> AuthenticationRequest(PKOCDevice pkocDevice)
        {
            pkocDevice.ClearIncomingData();

            DateTime startReadTime = DateTime.UtcNow;

            await _panel.ManufacturerSpecificCommand(pkocDevice.ConnectionId, pkocDevice.Address,
                new ManufacturerSpecific(PSIAVendorCode, new AuthenticationRequestData(
                        new byte[] { 0x01, 0x00 },
                        pkocDevice.ReaderIdentifier, pkocDevice.CreateRandomTransactionId(), 0x00).BuildData()
                    .ToArray()), pkocDevice.MaximumFragmentSendSize,
                pkocDevice.CardReadTimeout, CancellationToken.None);


            await _lock.WaitAsync(pkocDevice.CardReadTimeout - (DateTime.UtcNow - startReadTime));
            
            return pkocDevice.AuthenticationResponseData();
        }

        /// <summary>
        /// Event that is triggered when a card is presented to the reader.
        /// </summary>
        public event EventHandler<CardPresentedEventArgs> CardPresented;

        private void InvokeCardPresented(Guid connectionId, byte address,
            CardPresentResponseData cardPresentResponseData)
        {
            CardPresented?.Invoke(this, new CardPresentedEventArgs(connectionId, address, cardPresentResponseData));
        }

        /// <summary>
        /// Represents the event argument for the ReaderErrorReported event.
        /// </summary>
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
        
        private void ProcessAuthenticationResponse(DataFragmentResponse manufacturerSpecificData, PKOCDevice pkocDevice)
        {
            if (pkocDevice.IsDataCleared())
            {
                pkocDevice.AllocateIncomingData(manufacturerSpecificData.WholeMessageLength);
            }

            if (Utilities.BuildMultiPartMessageData(manufacturerSpecificData, pkocDevice))
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

        /// <inheritdoc />
        public void Dispose()
        {
            _panel.ManufacturerSpecificReplyReceived -= OnPanelOnManufacturerSpecificReplyReceived;
            _lock?.Dispose();
        }
    }
}