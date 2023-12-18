using OSDP.Net;

namespace PKOC.Net;

public class PKOCControlPanel(ControlPanel panel)
{
    /// <summary>
    /// Initializes the PKOC device.
    /// </summary>
    /// <param name="settings">The device identification settings.</param>
    /// <returns>Returns a boolean indicating the success of the initialization.</returns>
    public async Task<bool> InitializePKOC(DeviceIdentification settings)
    {
        bool success = await panel.ACUReceivedSize(settings.ConnectionId, settings.Address, 1024);
        success &= await panel.KeepReaderActive(settings.ConnectionId, settings.Address, 3000);

        if (success)
        {
            panel.ManufacturerSpecificReplyReceived += (_, eventArgs) =>
            {
                if (eventArgs.ManufacturerSpecific.Data.ToArray()[0] == 0xE0)
                {
                    InvokeCardPresented();
                }
            };
        }
            
        return success;
    }
    
    public event EventHandler<CardPresentedEventArgs>? CardPresented;

    protected virtual void InvokeCardPresented()
    {
        CardPresented?.Invoke(this, new CardPresentedEventArgs());
    }
}