using System;
using System.Linq;
using OSDP.Net.Model.ReplyData;
using PKOC.Net.MessageData;

namespace PKOC.Net
{
    /// <summary>
    /// Represents a device that can process PKOC over OSDP. It contains state information needed to process PKOC transactions.
    /// </summary>
    public class PKOCDevice : IEquatable<PKOCDevice>
    {
        private const ushort MaximumReceiveSizeDefault = 1024;
        private const ushort MaximumFragmentSendSizeDefault = 128;
        private const double CardReadTimeoutDefault = 3;
        
        private byte[] _incomingData;
        private byte[] _transactionId;

        /// <summary>
        /// Initialize a new instance of the <see cref="PKOCDevice"/> class.
        /// </summary>
        /// <param name="connectionId">The unique identifier for the connection.</param>
        /// <param name="address">Represents a address of the device.</param>
        public PKOCDevice(Guid connectionId, byte address) : this(connectionId, address,
            MaximumFragmentSendSizeDefault,
            MaximumReceiveSizeDefault, TimeSpan.FromSeconds(CardReadTimeoutDefault))
        {
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="PKOCDevice"/> class.
        /// </summary>
        /// <param name="connectionId">The unique identifier for the connection.</param>
        /// <param name="address">Represents a address of the device.</param>
        /// <param name="maximumFragmentSendSize">the maximum size of a fragment to be sent during PKOC transactions.</param>
        /// <param name="maximumReceiveSize">The maximum receive size for PKOC transactions.</param>
        /// <param name="cardReadTimeout">The amount of time to wait for a card read to complete.</param>
        public PKOCDevice(Guid connectionId, byte address,  ushort maximumFragmentSendSize, 
            ushort maximumReceiveSize, TimeSpan cardReadTimeout)
        {
            ConnectionId = connectionId;
            Address = address;
            MaximumFragmentSendSize = maximumFragmentSendSize;
            MaximumReceiveSize = maximumReceiveSize;
            CardReadTimeout = cardReadTimeout;

            ReaderIdentifier = Utilities.GenerateRandomBytes(32);
        }

        /// <summary>
        /// Gets the unique identifier for the connection.
        /// </summary>
        public Guid ConnectionId { get; }

        /// <summary>
        /// Gets the address of the device.
        /// </summary>
        public byte Address { get; }

        /// <summary>
        /// Gets the maximum size of a fragment to be sent during PKOC transactions.
        /// </summary>
        /// <remarks>
        /// This property determines the maximum size of a data fragment that can be sent during PKOC transactions.
        /// A data fragment is a part of the entire data being sent. If the data is larger than the maximum fragment size, it will be split into multiple fragments.
        /// </remarks>
        /// <value>
        /// The maximum fragment send size in bytes.
        /// </value>
        public ushort MaximumFragmentSendSize { get; }

        /// <summary>
        /// Gets the maximum receive size for PKOC transactions.
        /// </summary>
        public ushort MaximumReceiveSize { get; }
        
        /// <summary>
        /// Get the amount of time to wait for a card read to complete.
        /// </summary>
        public TimeSpan CardReadTimeout { get; }

        /// <summary>
        /// Gets the unique identifier for the device, which is used to validate credentials.
        /// </summary>
        public byte[] ReaderIdentifier { get; }
        
        /// <inheritdoc />
        public bool Equals(PKOCDevice other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return ConnectionId.Equals(other.ConnectionId) && Address == other.Address;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PKOCDevice)obj);
        }

        /// <summary>
        /// Determines whether the current DevicePKOCSettings object is equal to another DevicePKOCSettings object.
        /// </summary>
        /// <param name="connectionId">The unique identifier for the connection.</param>
        /// <param name="address">Represents a address of the device.</param>
        /// <returns>True if the current DevicePKOCSettings object is equal to the other DevicePKOCSettings object; otherwise, false.</returns>
        public bool Equals(Guid connectionId, byte address)
        {
            return ConnectionId.Equals(connectionId) && Address == address;
        }

        /// <inheritdoc />
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

        internal byte[] CreateRandomTransactionId()
        {
            _transactionId =  Utilities.GenerateRandomBytes(16);
            return _transactionId;
        }
    }
}