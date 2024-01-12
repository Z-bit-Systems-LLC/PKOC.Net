using OSDP.Net;
using OSDP.Net.Connections;
using PKOC.Net;

var panel = new ControlPanel();
var pkocPanel = new PKOCControlPanel(panel);

var connectionId = panel.StartConnection(new SerialPortOsdpConnection("COM3", 9600));
DeviceSettings? deviceIdentification = null;

panel.ConnectionStatusChanged += async (_, eventArgs) =>
{
    if (eventArgs.IsConnected)
    {
        Console.WriteLine("The OSDP reader is connected and will attempt to initialize for PKOC.");

        deviceIdentification = new DeviceSettings(eventArgs.ConnectionId, eventArgs.Address);
        bool successfulInitialization = await pkocPanel.InitializePKOC(deviceIdentification);
        Console.WriteLine(successfulInitialization
            ? "The OSDP reader has been successfully initialized for PKOC."
            : "The OSDP reader has not been successfully initialized for PKOC.");
    }
};

pkocPanel.CardPresented += (_, _) =>
{
    Console.WriteLine("A PKOC card has been presented to the reader.");

    Task.Run(async () =>
    {
        var result = await pkocPanel.AuthenticationRequest(deviceIdentification);
        Console.WriteLine(result.IsValidSignature() ? "Valid credential found" : "Invalid credential found");
    });
};

panel.AddDevice(connectionId, 0, true, false);

Console.ReadLine();
