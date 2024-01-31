using System;
using System.Collections.Generic;

namespace PKOC.Net.MessageData
{
    /// <summary>
    /// Represents the request data for an authentication message.
    /// </summary>
    public class AuthenticationRequestData : MessageDataBase
    {
        /// <summary>
        /// Represents the request data for an authentication message.
        /// </summary>
        /// <param name="protocolVersion">The protocol version.</param>
        /// <param name="readerIdentifier">The reader identifier.</param>
        /// <param name="transactionIdentifier">The transaction identifier.</param>
        /// <param name="transactionSequence">The transaction sequence.</param>
        public AuthenticationRequestData(byte[] protocolVersion, byte[] readerIdentifier, byte[] transactionIdentifier,
            byte transactionSequence)
        {
            ProtocolVersion = protocolVersion;
            ReaderIdentifier = readerIdentifier;
            TransactionIdentifier = transactionIdentifier;
            TransactionSequence = transactionSequence;
        }

        /// <inheritdoc />
        internal override ReadOnlySpan<byte> BuildData()
        {
            var data =
                new List<byte>
                {
                    (byte)PKOCMessageIdentifier.AuthenticationRequest,
                    0x00,
                    0x01
                };

            AddToData(data, TLVCode.ProtocolVersion, ProtocolVersion, true);
            AddToData(data, TLVCode.ReaderIdentifier, ReaderIdentifier, true);
            AddToData(data, TLVCode.TransactionIdentifier, TransactionIdentifier, true);
            AddToData(data, TLVCode.TransactionSequence, new[] { TransactionSequence }, false);

            return data.ToArray();
        }

        /// <summary>
        /// Parses the provided byte data and returns a AuthenticationRequestData object.
        /// </summary>
        /// <param name="data">The byte data to be parsed.</param>
        /// <returns>A AuthenticationRequestData object containing the parsed data.</returns>
        internal static AuthenticationRequestData ParseData(ReadOnlySpan<byte> data)
        {
            if (data.Length < 3)
            {
                throw new Exception("Data length is less than 3");
            }

            if (data[0] != (byte)PKOCMessageIdentifier.AuthenticationRequest)
            {
                throw new Exception("Not a Authentication request data type");
            }
            
            if (data[1] != 0x00 && data[2] != 0x01 )
            {
                throw new Exception("Incorrect authentication command parameters");
            }

            byte[] protocolVersion = Array.Empty<byte>();
            byte[] readerIdentifier = Array.Empty<byte>();
            byte[] transactionIdentifier = Array.Empty<byte>();
            byte transactionSequence = 0x00;

            int index = 3;
            while (index < data.Length)
            {
                var tlvData = GetTLVData(data.Slice(index));
                index += tlvData.Length;

                switch (tlvData.TLVCode)
                {
                    case TLVCode.ProtocolVersion:
                        protocolVersion = tlvData.Data;
                        break;
                    case TLVCode.ReaderIdentifier:
                        readerIdentifier = tlvData.Data;
                        break;
                    case TLVCode.TransactionIdentifier:
                        transactionIdentifier = tlvData.Data;
                        break;
                    case TLVCode.TransactionSequence:
                        transactionSequence = tlvData.Data[0];
                        break;
                }
            }

            if (protocolVersion.Length != 2)
            {
                throw new Exception("An invalid protocol version TLV was found in the data");
            }

            if (readerIdentifier.Length != 32)
            {
                throw new Exception("An invalid reader identifier TLV was found in the data");
            }

            if (transactionIdentifier.Length != 0 && (transactionIdentifier.Length < 16 ||
                transactionIdentifier.Length > 65))
            {
                throw new Exception("An invalid transaction identifier TLV was found in the data");
            }

            return new AuthenticationRequestData(protocolVersion, readerIdentifier, transactionIdentifier,
                transactionSequence);
        }

        /// <summary>
        /// Gets the protocol version.
        /// </summary>
        /// <returns>An array of 2 bytes representing the protocol version.</returns>
        public byte[] ProtocolVersion { get; }

        /// <summary>
        /// Gets the reader identifier.
        /// </summary>
        /// <remarks>
        /// The reader identifier is a unique identifier associated with the device.
        /// It is represented as a byte array of 32 in length.
        /// </remarks>
        /// <returns>An array of bytes representing the reader identifier.</returns>
        public byte[] ReaderIdentifier { get; }

        /// <summary>
        /// Gets the transaction identifier.
        /// </summary>
        /// <remarks>
        /// The transaction identifier is a unique identifier associated with the transaction.
        /// It is represented as a byte array between 16 and 65 in length.
        /// </remarks>
        /// <returns>
        /// The transaction identifier as a byte array.
        /// </returns>
        public byte[] TransactionIdentifier { get; }

        /// <summary>
        /// Gets the transaction sequence number.
        /// </summary>
        public byte TransactionSequence { get; }
    }
}