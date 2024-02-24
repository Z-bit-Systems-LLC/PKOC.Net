# PKOC.Net

[![Build Status](https://dev.azure.com/Z-bitSystems/PKOC.Net/_apis/build/status%2FZ-bit-Systems-LLC.PKOC.Net?branchName=develop)](https://dev.azure.com/Z-bitSystems/PKOC.Net/_build/latest?definitionId=2&branchName=develop)
[![NuGet](https://img.shields.io/nuget/v/PKOC.Net.svg?style=flat)](https://www.nuget.org/packages/PKOC.Net/)

PKOC.Net is a NET implementation of the Public Key Open Credential (PKOC) using Open Supervised Device Protocol (OSDP) for ACUs.
The [OSDP.Net library](https://github.com/bytedreamer/OSDP.Net) is used to allow the Access Control Unit (ACU) the ablity to process and validate PKOC issued credentials. The PKOC specification details can be found on the [PSIA PKOC hompage](https://psialliance.org/securecredentials/). 
The OSDP messages are defined on the following [GitHub page](https://github.com/smithee-solutions/openbadger/blob/main/discussions/PKOC/pkoc-osdp-acu.pdf).

## Getting Started

The PKOC.Net library provides a Nuget package to quickly add PKOC over OSDP capability to a .NET Framework or Core project.
You can install it using the NuGet Package Console window:

```shell
PM> Install-Package PKOC.Net
``` 

There is a dependecy on OSDP.Net for handling OSDP communications. 
See [OSDP.Net README](https://github.com/bytedreamer/OSDP.Net?tab=readme-ov-file) for information regarding the use of this library.

There is a separate PKOC control panel for interfacing with PKOC related transactions.
To start, create a PKOCControl panel object that includes a reference to the OSDP.Net control panel object.
Each device needs a persistent PKOCDevice object to track the PKOC related transactions.

```c#
var panel = new ControlPanel();
var pkocPanel = new PKOCControlPanel(panel);

var connectionId = panel.StartConnection(new SerialPortOsdpConnection(portName, baudRate));
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
    });
};

pkocPanel.ReaderErrorReported += (_, _) =>
{
    Console.WriteLine("An error occured while a PKOC card has been presented to the reader.");
};

panel.AddDevice(connectionId, deviceAddress, true, false);
```

## Contributing

Contact me through my consulting company [Z-bit System, LLC](https://z-bitco.com), if interested in further collaboration with the PKOC.Net library.
