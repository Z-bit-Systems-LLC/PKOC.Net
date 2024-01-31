using System;
using System.Linq;
using OSDP.Net.Model.ReplyData;
using PKOC.Net.MessageData;

namespace PKOC.Net
{
    public class DeviceSettings : IEquatable<DeviceSettings>
    {
        private const ushort MaximumReceiveSizeDefault = 1024;
        private const ushort MaximumFragmentSendSizeDefault = 128;
        private const double CardReadTimeoutDefault = 3;
        
        private byte[] _incomingData;
        private byte[] _transactionId;

        public DeviceSettings(Guid connectionId, byte address) : this(connectionId, address,
            MaximumFragmentSendSizeDefault,
            MaximumReceiveSizeDefault, TimeSpan.FromSeconds(CardReadTimeoutDefault))
        {
        }

        public DeviceSettings(Guid connectionId, byte address,  ushort maximumFragmentSendSize, 
            ushort maximumReceiveSize, TimeSpan cardReadTimeout)
        {
            ConnectionId = connectionId;
            Address = address;
            MaximumFragmentSendSize = maximumFragmentSendSize;
            MaximumReceiveSize = maximumReceiveSize;
            CardReadTimeout = cardReadTimeout;

            ReaderIdentifier = Utilities.GenerateRandomBytes(32);
        }

        public Guid ConnectionId { get; }

        public byte Address { get; }
        
        public ushort MaximumFragmentSendSize { get; }

        public ushort MaximumReceiveSize { get; }

        public TimeSpan CardReadTimeout { get; }
        
        public byte[] ReaderIdentifier { get; }

        public bool Equals(DeviceSettings other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return ConnectionId.Equals(other.ConnectionId) && Address == other.Address;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DeviceSettings)obj);
        }

        public bool Equals(Guid connectionId, byte address)
        {
            return ConnectionId.Equals(connectionId) && Address == address;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + ConnectionId.GetHashCode();
                hash = hash * 31 + Address.GetHashCode();
                return hash;
            }
        }

        internal void ClearIncomingData()
        {
            _incomingData = null;
        }

        internal bool IsDataCleared()
        {
            return _incomingData == null;
        }

        internal void AllocateIncomingData(ushort wholeMessageLength)
        {
            _incomingData = new byte[wholeMessageLength];
        }

        internal void ConcatIncomingData(DataFragmentResponse dataFragment)
        {
            dataFragment.Data.CopyTo(_incomingData.AsSpan(dataFragment.Offset, dataFragment.LengthOfFragment));
        }

        internal AuthenticationResponseData AuthenticationResponseData()
        {
            var authenticationResponseData = MessageData.AuthenticationResponseData.ParseData(
                new[] { (byte)PKOCMessageIdentifier.AuthenticationResponse }
                    .Concat(_incomingData).ToArray());

            if (authenticationResponseData.TransactionIdentifier == null)
            {
                authenticationResponseData.UpdateTransactionId(_transactionId);
            }

            return authenticationResponseData;
        }

        public byte[] CreateRandomTransactionId()
        {
            _transactionId =  Utilities.GenerateRandomBytes(16);
            return _transactionId;
        }
    }
}