using System;
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
    public class PKOCControlPanel
    {
        private static readonly byte[] PSIAVendorCode = { 0x1A, 0x90, 0x21};
        private readonly ControlPanel _panel;
        private byte[] _data = null;
        private AuthenticationResponseData _authenticationResponseData = null;
        private static SemaphoreSlim _lock = new SemaphoreSlim(0, 1);
        
        public PKOCControlPanel(ControlPanel panel)
        {
            _panel = panel;
        }

        /// <summary>
        /// Initializes the PKOC device.
        /// </summary>
        /// <param name="settings">The device identification settings.</param>
        /// <returns>Returns a boolean indicating the success of the initialization.</returns>
        public async Task<bool> InitializePKOC(DeviceIdentification settings)
        {
            bool success = await _panel.ACUReceivedSize(settings.ConnectionId, settings.Address, 1024);
            success &= await _panel.KeepReaderActive(settings.ConnectionId, settings.Address, 3000);

            if (success)
            {
                _panel.ManufacturerSpecificReplyReceived += (_, eventArgs) =>
                {
                    if (!IsPSIAVendorCode(eventArgs.ManufacturerSpecific.VendorCode)) return;
                    
                    // Only process matching replies
                    if (eventArgs.ConnectionId != settings.ConnectionId || eventArgs.Address != settings.Address) return;
                
                    if (IdentifyMessage(eventArgs.ManufacturerSpecific.Data) == PKOCMessageIdentifier.CardPresentResponse)
                    {
                        InvokeCardPresented();
                    }
                    else if (IdentifyMessage(eventArgs.ManufacturerSpecific.Data) ==
                             PKOCMessageIdentifier.AuthenticationResponse)
                    {
                            ProcessAuthenticationResponse(eventArgs);
                    }
                    else if (IdentifyMessage(eventArgs.ManufacturerSpecific.Data) == PKOCMessageIdentifier.ReaderErrorResponse)
                    {
                        
                    }
                };
            }
            
            return success;
        }

        public async Task<AuthenticationResponseData> AuthenticationRequest(DeviceIdentification settings)
        {
            _data = null;
            _authenticationResponseData = null;

            await _panel.ManufacturerSpecificCommand(settings.ConnectionId, settings.Address,
                new ManufacturerSpecific(PSIAVendorCode, new AuthenticationRequestData(new byte[] { 0x01, 0x00 },
                    Enumerable.Range(0x00, 32).Select(i => (byte)i).ToArray(),
                    Enumerable.Range(0x00, 16).Select(i => (byte)i).ToArray(), 0x00).BuildData().ToArray()), 128,
                TimeSpan.FromSeconds(10), CancellationToken.None);


            await _lock.WaitAsync(TimeSpan.FromSeconds(10));
            return _authenticationResponseData;
        }

        public event EventHandler<CardPresentedEventArgs> CardPresented;

        private void InvokeCardPresented()
        {
            CardPresented?.Invoke(this, new CardPresentedEventArgs());
        }

        private PKOCMessageIdentifier IdentifyMessage(IEnumerable<byte> data)
        {
            return (PKOCMessageIdentifier)data.First();
        }
        
        private void ProcessAuthenticationResponse(ControlPanel.ManufacturerSpecificReplyEventArgs eventArgs)
        {
            var manufacturerSpecificData =
                DataFragmentResponse.ParseData(eventArgs.ManufacturerSpecific.Data.Skip(1).ToArray());

            if (_data == null)
            {
                _data = new byte[manufacturerSpecificData.WholeMessageLength];
            }

            if (Utilities.BuildMultiPartMessageData(
                    manufacturerSpecificData.WholeMessageLength,
                    manufacturerSpecificData.Offset,
                    manufacturerSpecificData.LengthOfFragment,
                    manufacturerSpecificData.Data,
                    _data))
            {
                try
                {
                    _authenticationResponseData = AuthenticationResponseData.ParseData(
                        new[] { (byte)PKOCMessageIdentifier.AuthenticationResponse }.Concat(_data).ToArray());
                }
                finally
                {
                    _lock.Release();
                }
            }
        }
        
        private static bool IsPSIAVendorCode(IEnumerable<byte> manufacturerSpecificVendorCode)
        {
            return manufacturerSpecificVendorCode.SequenceEqual(PSIAVendorCode);
        }
    }
}