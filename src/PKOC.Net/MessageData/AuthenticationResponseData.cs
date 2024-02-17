using System;
using System.Collections.Generic;

namespace PKOC.Net.MessageData
{
    public class AuthenticationResponseData : MessageDataBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationResponseData"/> class.
        /// </summary>
        /// <param name="publicKey">The public key of the credential.</param>
        /// <param name="digitalSignature">The digital signature of the credential.</param>
        /// <param name="transactionIdentifier">The transaction identifier (optional).</param>
        /// <param name="transactionSequence">The transaction sequence (optional).</param>
        /// <param name="error">The error (optional).</param>
        public AuthenticationResponseData(byte[] publicKey, byte[] digitalSignature,
            byte[] transactionIdentifier = null,
            byte? transactionSequence = null, byte[] error = null)
        {
            PublicKey = publicKey;
            DigitalSignature = digitalSignature;
            TransactionIdentifier = transactionIdentifier;
            TransactionSequence = transactionSequence;
            Error = error;
        }

        internal override ReadOnlySpan<byte> BuildData()
        {
            var data =
                new List<byte>
                {
                    (byte)PKOCMessageIdentifier.AuthenticationResponse,
                };

            AddToData(data, TLVCode.PublicKey, PublicKey, true);
            AddToData(data, TLVCode.DigitalSignature, DigitalSignature, true);
            if (TransactionIdentifier != null)
                AddToData(data, TLVCode.TransactionIdentifier, TransactionIdentifier, true);
            if (TransactionSequence.HasValue)
                AddToData(data, TLVCode.TransactionSequence, new[] { TransactionSequence.Value }, false);
            if (Error != null)
                AddToData(data, TLVCode.Error, Error, false);

            return data.ToArray();
        }

        public static AuthenticationResponseData ParseData(ReadOnlySpan<byte> data)
        {
            if (data.Length < 1)
            {
                throw new Exception("Data length is less than 1");
            }

            if (data[0] != (byte)PKOCMessageIdentifier.AuthenticationResponse)
            {
                throw new Exception("Not a Authentication request data type");
            }

            byte[] publicKey = Array.Empty<byte>();
            byte[] digitalSignature = Array.Empty<byte>();
            byte[] transactionIdentifier = null;
            byte? transactionSequence = null;
            byte[] error = null;
            
            int index = 1;
            while (index < data.Length)
            {
                var tlvData = GetTLVData(data.Slice(index));
                index += tlvData.Length;

                switch (tlvData.TLVCode)
                {
                    case TLVCode.PublicKey:
                        publicKey = tlvData.Data;
                        break;
                    case TLVCode.DigitalSignature:
                        digitalSignature = tlvData.Data;
                        break;
                    case TLVCode.TransactionIdentifier:
                        transactionIdentifier = tlvData.Data;
                        break;
                    case TLVCode.TransactionSequence:
                        transactionSequence = tlvData.Data[0];
                        break;
                    case TLVCode.Error:
                        error = tlvData.Data;
                        break;
                }
            }
            
            if (publicKey.Length != 65)
            {
                throw new Exception("An invalid protocol version TLV was found in the data");
            }

            if (digitalSignature.Length != 64)
            {
                throw new Exception("An invalid reader identifier TLV was found in the data");
            }
            
            if (transactionIdentifier != null && transactionIdentifier.Length != 0 && (transactionIdentifier.Length < 16 ||
                    transactionIdentifier.Length > 65))
            {
                throw new Exception("An invalid transaction identifier TLV was found in the data");
            }

            return new AuthenticationResponseData(publicKey, digitalSignature, transactionIdentifier,
                transactionSequence, error);
        }
        
        /// <summary>
        /// Gets the public key associated with the credential.
        /// </summary>
        /// <returns>
        /// An array of bytes representing the public key.
        /// </returns>
        public byte[] PublicKey { get; }

        /// <summary>
        /// Gets the digital signature associated with the credential.
        /// </summary>
        /// <returns>
        /// An array of bytes representing the digital signature.
        /// </returns>
        public byte[] DigitalSignature { get; }
        
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
        public byte[] TransactionIdentifier { get; private set; }

        /// <summary>
        /// Gets the transaction sequence number.
        /// </summary>
        public byte? TransactionSequence { get; }
        
        /// <summary>
        /// Gets the error message.
        /// </summary>
        /// <returns>An array of bytes representing the error message.</returns>
        public byte[] Error { get; }

        /// <summary>
        /// TODO: This is not valid method for validating cards, a random transaction ID needs to be used
        /// </summary>
        /// <returns></returns>
        public bool IsValidSignature()
        {
            return Utilities.ValidateSignature(TransactionIdentifier, PublicKey, DigitalSignature);
        }

        internal void UpdateTransactionId(byte[] transactionIdentifier)
        {
            TransactionIdentifier = transactionIdentifier;
        }
    }
}