using System;

namespace PKOC.Net.MessageData
{
    /// <summary>
    /// Represents a response data object for a transaction refresh message.
    /// </summary>
    public class TransactionRefreshResponseData : MessageDataBase
    {
        /// <inheritdoc />
        internal override ReadOnlySpan<byte> BuildData()
        {
            return new [] { (byte)PKOCMessageIdentifier.TransactionRefreshResponse };
        }

        /// <summary>
        /// Parses the provided byte data and returns a TransactionRefreshResponseData object.
        /// </summary>
        /// <param name="data">The byte data to be parsed.</param>
        /// <returns>A TransactionRefreshResponseData object containing the parsed data.</returns>
        public static TransactionRefreshResponseData ParseData(ReadOnlySpan<byte> data)
        {
            if (data.Length != 1)
            {
                throw new Exception("Data length is not equal to 1");
            }
        
            if (data[0] != (byte)PKOCMessageIdentifier.TransactionRefreshResponse)
            {
                throw new Exception("Not a Transaction Refresh response data type");
            }

            return new TransactionRefreshResponseData();
        }
    }
}