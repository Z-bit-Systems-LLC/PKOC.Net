using Microsoft.Extensions.Configuration;
using OSDP.Net;
using OSDP.Net.Connections;
using PKOC.Net;

var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true);
var config = builder.Build();
var osdpSection = config.GetSection("OSDP");

var portName = osdpSection["PortName"];
var baudRate = int.Parse(osdpSection["BaudRate"] ?? "9600");
var deviceAddress = byte.Parse(osdpSection["DeviceAddress"] ?? "0");

var panel = new ControlPanel();
var pkocPanel = new PKOCControlPanel(panel);

var connectionId = panel.StartConnection(new SerialPortOsdpConnection(portName, baudRate), TimeSpan.FromMilliseconds(20), true);
PKOCDevice? pkocDevice = null;

panel.ConnectionStatusChanged += (_, eventArgs) =>
{
    if (!eventArgs.IsConnected) return;
    
    Console.WriteLine("The OSDP reader is connected and will attempt to initialize for PKOC.");

    Task.Run(async () =>
    {
        pkocDevice = new PKOCDevice(eventArgs.ConnectionId, eventArgs.Address);
        bool successfulInitialization = await pkocPanel.InitializePKOC(pkocDevice);
        Console.WriteLine(successfulInitialization
            ? "The OSDP reader has been successfully initialized for PKOC."
            : "The OSDP reader has not been successfully initialized for PKOC.");
    });
};

pkocPanel.CardPresented += (_, eventArgs) =>
{
    Console.WriteLine("A PKOC card has been presented to the reader.");
    
    if (eventArgs.CardPresentResponseData.Error is not { Length: 0 })
    {
        Console.WriteLine("An error occured while a PKOC card has been presented to the reader.");
    }

    Task.Run(async () =>
    {
        var result = await pkocPanel.AuthenticationRequest(pkocDevice);
        
        Console.WriteLine(result.IsValidSignature() ? "Valid credential found" : "Invalid credential found");
        Console.WriteLine($"Raw Public Key - {BitConverter.ToString(result.PublicKey)}");
    });
};

pkocPanel.ReaderErrorReported += (_, _) =>
{
    Console.WriteLine("An error occured while a PKOC card has been presented to the reader.");
};

panel.AddDevice(connectionId, deviceAddress, true, false);

Console.ReadLine();
