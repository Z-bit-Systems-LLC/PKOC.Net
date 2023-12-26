using System;
using System.Collections.Generic;

namespace PKOC.Net.MessageData
{
    /// <summary>
    /// Represents the response data of a reader error.
    /// </summary>
    public class ReaderErrorResponseData : MessageDataBase
    {
        /// <summary>
        /// Represents the error response data from a reader.
        /// </summary>
        /// <param name="error">The error data.</param>
        public ReaderErrorResponseData(byte[] error)
        {
            Error = error;
        }

        /// <inheritdoc />
        internal override ReadOnlySpan<byte> BuildData()
        {
            List<byte> data = new List<byte> { (byte)PKOCMessageIdentifier.ReaderErrorResponse };
            data.AddRange(Error);

            return data.ToArray();
        }

        /// <summary>
        /// Parses the provided byte data and returns a ReaderErrorResponseData object.
        /// </summary>
        /// <param name="data">The byte data to be parsed.</param>
        /// <returns>A ReaderErrorResponseData object containing the parsed data.</returns>
        public static ReaderErrorResponseData ParseData(ReadOnlySpan<byte> data)
        {
            if (data.Length < 2)
            {
                throw new Exception("Data length is less than 2");
            }
        
            if (data[0] != (byte)PKOCMessageIdentifier.ReaderErrorResponse)
            {
                throw new Exception("Not a Reader Error response data type");
            }

            return new ReaderErrorResponseData(data.Slice(1, data.Length - 1).ToArray());
        }
    
        /// <summary>
        /// Gets the error message as a byte array.
        /// </summary>
        /// <returns>The error message as a byte array.</returns>
        public byte[] Error { get; }
    }
}