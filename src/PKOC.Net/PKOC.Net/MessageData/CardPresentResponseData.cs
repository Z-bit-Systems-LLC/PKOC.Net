using System;
using System.Collections.Generic;
using System.Linq;

namespace PKOC.Net.MessageData
{
    /// <summary>
    /// Represents a response data for a card present transaction.
    /// </summary>
    public class CardPresentResponseData : MessageDataBase
    {
        /// <summary>
        /// Represents the response data for a card present transaction.
        /// </summary>
        /// <param name="protocolVersions">An array of bytes representing the protocol versions.</param>
        /// <param name="error">An array of bytes representing the error.</param>
        /// <param name="transactionIdentifier">An array of bytes representing the transaction identifier.</param>
        public CardPresentResponseData(byte[] protocolVersions, byte[] error, byte[] transactionIdentifier)
        {
            ProtocolVersions = protocolVersions;
            Error = error;
            TransactionIdentifier = transactionIdentifier;
        }

        /// <inheritdoc />
        internal override ReadOnlySpan<byte> BuildData()
        {
            List<byte> data =
                new List<byte>
                {
                    (byte)PKOCMessageIdentifier.CardPresentResponse,
                    (byte)TLVCode.CardPresentPayload,
                    0x00
                };

            AddToData(data, TLVCode.SupportedProtocol, ProtocolVersions.OrderByDescending(b => b).ToArray(), true);
            AddToData(data, TLVCode.Error, Error, false);
            AddToData(data, TLVCode.TransactionIdentifier, TransactionIdentifier, true);

            data[2] = (byte)(data.Count - 3);

            return data.ToArray();
        }

        /// <summary>
        /// Parses the provided byte data and returns a CardPresentResponseData object.
        /// </summary>
        /// <param name="data">The byte data to be parsed.</param>
        /// <returns>A CardPresentResponseData object containing the parsed data.</returns>
        internal static CardPresentResponseData ParseData(ReadOnlySpan<byte> data)
        {
            if (data.Length < 2)
            {
                throw new Exception("Data length is less than 2");
            }
        
            if (data[0] != (byte)PKOCMessageIdentifier.CardPresentResponse)
            {
                throw new Exception("Not a Card Present response data type");
            }
        
            if (data[1] != (byte)TLVCode.CardPresentPayload)
            {
                throw new Exception("The first TLV should be ");
            }

            var cardPresentTLVData = GetTLVData(data.Slice(1, data.Length - 1));

            byte[] protocolVersions = Array.Empty<byte>();
            byte[] errorCode = Array.Empty<byte>();
            byte[] transactionIdentifier = Array.Empty<byte>();
        
            int index = 0;
            while (index < cardPresentTLVData.Length - 2)
            {
                var tlvData = GetTLVData(cardPresentTLVData.Data.Skip(index).ToArray());
                index += tlvData.Length;
            
                switch (tlvData.TLVCode)
                {
                    case TLVCode.SupportedProtocol:
                        protocolVersions = tlvData.Data;
                        break;
                    case TLVCode.Error:
                        errorCode = tlvData.Data;
                        break;
                    case TLVCode.TransactionIdentifier:
                        transactionIdentifier = tlvData.Data;
                        break;
                }
            }
        
            return new CardPresentResponseData(protocolVersions, errorCode, transactionIdentifier);
        }
        
        public byte[] ProtocolVersions { get; }

        /// <summary>
        /// Gets the error message as a byte array.
        /// </summary>
        /// <returns>The error message as a byte array.</returns>
        public byte[] Error { get; }

        /// <summary>
        /// Gets the transaction identifier.
        /// </summary>
        /// <remarks>
        /// The transaction identifier is a unique identifier associated with the transaction.
        /// It is represented as a byte array.
        /// </remarks>
        /// <returns>
        /// The transaction identifier as a byte array.
        /// </returns>
        public byte[] TransactionIdentifier { get; }
    }
}