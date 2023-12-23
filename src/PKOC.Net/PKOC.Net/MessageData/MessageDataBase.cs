using System;
using System.Collections.Generic;

namespace PKOC.Net.MessageData
{
    public abstract class MessageDataBase
    {
        internal abstract ReadOnlySpan<byte> BuildData();

        protected static TLVData GetTLVData(ReadOnlySpan<byte> payload)
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
                    if (errorCode == ErrorCode.ISO7816Status && payload.Length > 4)
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
    
        protected static void AddToData(List<byte> data, TLVCode tlvCode, byte[] bytesToAdd, bool includeLength)
        {
            if (bytesToAdd.Length <= 0) return;
            
            data.Add((byte)tlvCode);
            if (includeLength)
            {
                data.Add((byte)(bytesToAdd.Length));
            }
            data.AddRange(bytesToAdd);
        }
    }
}