namespace PKOC.Net.MessageData
{
    /// <summary>
    /// Represents a TLV (Tag-Length-Value) data structure.
    /// </summary>
    public class TLVData
    {
        /// <summary>
        /// Constructor for creating a TLVData object.
        /// </summary>
        /// <param name="tlvCode">The TLVCode to be assigned to the object.</param>
        /// <param name="length">The length to be assigned to the object.</param>
        /// <param name="data">The data to be assigned to the object.</param>
        public TLVData(TLVCode tlvCode, int length, byte[] data)
        {
            TLVCode = tlvCode;
            Length = length;
            Data = data;
        }

        /// <summary>
        /// Gets the TLV (Tag-Length-Value) code associated with the property.
        /// TLV is a common format used to encode data in computer systems.
        /// </summary>
        /// <remarks>
        /// TLVCode represents the code or tag of the TLV entry.
        /// </remarks>
        /// <returns>The TLV code as an instance of TLVCode enumeration.</returns>
        public TLVCode TLVCode { get; }

        /// <summary>
        /// Gets the total length of the object.
        /// </summary>
        /// <returns>The total length of the object.</returns>
        /// <remarks>
        /// The Length property returns the total length of the object.
        /// </remarks>
        public int Length { get; }

        /// <summary>
        /// Gets the data associated with this property.
        /// </summary>
        /// <returns>
        /// The data as an array of bytes.
        /// </returns>
        public byte[] Data { get; }
    }
}