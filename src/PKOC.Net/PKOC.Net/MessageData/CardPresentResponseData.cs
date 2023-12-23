using System;
using System.Collections.Generic;
using System.Linq;

namespace PKOC.Net.MessageData
{
    public class CardPresentResponseData : MessageDataBase
    {
        public CardPresentResponseData(byte[] protocolVersions, byte[] error, byte[] transactionIdentifier)
        {
            ProtocolVersions = protocolVersions;
            Error = error;
            TransactionIdentifier = transactionIdentifier;
        }

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

        public byte[] Error { get; }
        
        public byte[] TransactionIdentifier { get; }
    }
}