using System;
using System.Linq;

namespace PKOC.Net.MessageData
{
    public class CardPresentData
    {
        private CardPresentData(byte[] protocolVersions, byte errorCode)
        {
            ProtocolVersions = protocolVersions;
            ErrorCode = errorCode;
        }

        public static CardPresentData ParseData(ReadOnlySpan<byte> data)
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
            byte errorCode = 0x00;
        
            int index = 0;
            while (index < cardPresentTLVData.Length - 2)
            {
                var TLVData = GetTLVData(cardPresentTLVData.Data.Skip(index).ToArray());
                index += TLVData.Length;
            
                switch (TLVData.TLVCode)
                {
                    case TLVCode.SupportedProtocol:
                        protocolVersions = TLVData.Data;
                        break;
                    case TLVCode.Error:
                        errorCode = TLVData.Data[0];
                        break;
                }
            }
        
            return new CardPresentData(protocolVersions, errorCode);
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
                    data = new[] { payload[1] };
                    length = 2;
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

        public byte ErrorCode { get; }

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