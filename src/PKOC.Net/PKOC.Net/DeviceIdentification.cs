namespace PKOC.Net;

public class DeviceIdentification(Guid connectionId, byte address)
{
    public Guid ConnectionId { get; init; } = connectionId;

    public byte Address { get; init; } = address;
}