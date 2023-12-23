using System;
using System.Collections.Generic;

namespace PKOC.Net.MessageData
{
    public class ReaderErrorResponseData : MessageDataBase
    {
        public ReaderErrorResponseData(byte[] error)
        {
            Error = error;
        }

        internal override ReadOnlySpan<byte> BuildData()
        {
            List<byte> data = new List<byte> { (byte)PKOCMessageIdentifier.ReaderErrorResponse };
            data.AddRange(Error);

            return data.ToArray();
        }

        public static ReaderErrorResponseData ParseData(ReadOnlySpan<byte> data)
        {
            if (data.Length < 2)
            {
                throw new Exception("Data length is less than 2");
            }
        
            if (data[0] != (byte)PKOCMessageIdentifier.ReaderErrorResponse)
            {
                throw new Exception("Not a Card Present response data type");
            }

            return new ReaderErrorResponseData(data.Slice(1, data.Length - 1).ToArray());
        }
    
        public byte[] Error { get; }
    }
}