using System;
using System.Collections.Generic;

namespace PKOC.Net.MessageData
{
    /// <summary>
    /// Represents a base class for message data.
    /// </summary>
    public abstract class MessageDataBase
    {
        internal abstract ReadOnlySpan<byte> BuildData();

        /// <summary>
        /// Gets the TLV data from the given payload.
        /// </summary>
        /// <param name="payload">The payload containing the TLV data.</param>
        /// <returns>The TLVData object.</returns>
        /// <exception cref="System.Exception">Thrown when the TLV payload length is less than 2 or the TLV data length is not correct.</exception>
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

        /// <summary>
        /// Adds the specified byte array to the given data list using the provided TLV code and options.
        /// </summary>
        /// <param name="data">The list to which the byte array will be added.</param>
        /// <param name="tlvCode">The TLV code specifying the data type.</param>
        /// <param name="bytesToAdd">The byte array to be added to the data list.</param>
        /// <param name="includeLength">A flag indicating whether to include the length of the byte array in the data list.</param>
        /// <remarks>
        /// If the length of the byte array is less than or equal to 0, no elements will be added to the data list.
        /// The TLV code will be cast to byte and added to the data list first.
        /// If the 'includeLength' flag is true, the length of the byte array will be added as a byte after the TLV code.
        /// Finally, all the elements in the byte array will be added to the data list.
        /// </remarks>
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