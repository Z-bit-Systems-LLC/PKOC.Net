using System;

namespace PKOC.Net
{
    public class DeviceIdentification
    {
        public DeviceIdentification(Guid connectionId, byte address)
        {
            ConnectionId = connectionId;
            Address = address;
        }

        public Guid ConnectionId { get; }

        public byte Address { get; }
    }
}