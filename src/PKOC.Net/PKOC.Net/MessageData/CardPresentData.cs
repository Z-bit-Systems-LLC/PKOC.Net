using System;
using System.Collections.Generic;
using System.Linq;

namespace PKOC.Net.MessageData
{
    public class CardPresentData
    {
        public CardPresentData(byte[] protocolVersions, byte[] error, byte[] transactionIdentifier)
        {
            ProtocolVersions = protocolVersions;
            Error = error;
            TransactionIdentifier = transactionIdentifier;
        }

        internal ReadOnlySpan<byte> BuildData()
        {
            List<byte> data =
            [
                (byte)PKOCMessageIdentifier.CardPresent,
                (byte)TLVCode.CardPresentPayload,
                0x00,
                (byte)TLVCode.SupportedProtocol,
                (byte)ProtocolVersions.Length
            ];

            data.AddRange(ProtocolVersions.OrderByDescending(b => b));

            if (Error.Length > 0)
            {
                data.Add((byte)TLVCode.Error);
                data.AddRange(Error);
            }

            if (TransactionIdentifier.Length > 0)
            {
                data.Add((byte)TLVCode.TransactionIdentifier);
                data.Add((byte)TransactionIdentifier.Length);
                data.AddRange(TransactionIdentifier);
            }

            data[2] = (byte)(data.Count - 3);

            return data.ToArray();
        }
        
        internal static CardPresentData ParseData(ReadOnlySpan<byte> data)
        {
            if (data.Length < 2)
            {
                throw new Exception("Data length is less than 2");
            }
        
            if (data[0] != (byte)PKOCMessageIdentifier.CardPresent)
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
        
            return new CardPresentData(protocolVersions, errorCode, transactionIdentifier);
        }

        private static TLVData GetTLVData(ReadOnlySpan<byte> payload)
        {
            if (payload.Length < 2)
            {
                throw new Exception("TLV payload length is less than 2");
            }

            var code = (TLVCode)payload[0];
            byte[] data;
            int length;

            switch (code)
            {
                case TLVCode.TransactionSequence:
                    data = new[] { payload[1] };
                    length = 2;
                    break;
                case TLVCode.Error:
                    byte errorCode = payload[1];
                    if (errorCode == 0x01 && payload.Length > 4)
                    {
                        data = payload.Slice(1, 3).ToArray();
                        length = 4;
                    }
                    else
                    {
                        data = new[] { payload[1] };
                        length = 2;
                    }
                    break;
                default:
                {
                    var dataLength = payload[1];
                    if (payload.Length < dataLength + 2)
                    {
                        throw new Exception("TLV data length is not correct");
                    }

                    data = payload.Slice(2, dataLength).ToArray();

                    length = dataLength + 2;
                    break;
                }
            }

            return new TLVData(code, length, data);
        }
    
        public byte[] ProtocolVersions { get; }

        public byte[] Error { get; }
        
        public byte[] TransactionIdentifier { get; }

        private class TLVData
        {
            public TLVData(TLVCode tlvCode, int length, byte[] data)
            {
                TLVCode = tlvCode;
                Length = length;
                Data = data;
            }

            public TLVCode TLVCode { get; }
        
            public int Length { get; }

            public byte[] Data { get; }
        }
    }
}