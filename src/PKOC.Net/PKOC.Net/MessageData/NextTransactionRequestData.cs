using System;
using System.Collections.Generic;

namespace PKOC.Net.MessageData
{
    /// <summary>
    /// Represents the request data for a next transaction message.
    /// </summary>
    public class NextTransactionRequestData : MessageDataBase
    {
        /// <summary>
        /// Represents the request data for a next transaction transaction.
        /// </summary>
        /// <param name="transactionIdentifier">The transaction identifier.</param>
        /// <param name="transactionSequence">The transaction sequence.</param>
        public NextTransactionRequestData(byte[] transactionIdentifier, byte transactionSequence)
        {
            TransactionIdentifier = transactionIdentifier;
            TransactionSequence = transactionSequence;
        }

        /// <inheritdoc />
        internal override ReadOnlySpan<byte> BuildData()
        {
            var data =
                new List<byte>
                {
                    (byte)PKOCMessageIdentifier.NextTransactionRequest
                };

            AddToData(data, TLVCode.TransactionIdentifier, TransactionIdentifier, true);
            AddToData(data, TLVCode.TransactionSequence, new[] { TransactionSequence }, false);

            return data.ToArray();
        }
        
        /// <summary>
        /// Parses the provided byte data and returns a NextTransactionRequestData object.
        /// </summary>
        /// <param name="data">The byte data to be parsed.</param>
        /// <returns>A NextTransactionRequestData object containing the parsed data.</returns>
        internal static NextTransactionRequestData ParseData(ReadOnlySpan<byte> data)
        {
            if (data.Length < 1)
            {
                throw new Exception("Data length is less than 1");
            }
        
            if (data[0] != (byte)PKOCMessageIdentifier.NextTransactionRequest)
            {
                throw new Exception("Not a Next Transaction request data type");
            }

            byte[] transactionIdentifier = Array.Empty<byte>();
            byte transactionSequence = 0x00;

            int index = 1;
            while (index < data.Length - 1)
            {
                var tlvData = GetTLVData(data.Slice(index));
                index += tlvData.Length;
            
                switch (tlvData.TLVCode)
                {
                    case TLVCode.TransactionIdentifier:
                        transactionIdentifier = tlvData.Data;
                        break;
                    case TLVCode.TransactionSequence:
                        transactionSequence = tlvData.Data[0];
                        break;
                }
            }

            if (transactionIdentifier.Length < 16 || transactionIdentifier.Length > 66)
            {
                throw new Exception("An invalid transaction identifier TLV was found in the data");
            }
        
            return new NextTransactionRequestData(transactionIdentifier,  transactionSequence);
        }
            
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