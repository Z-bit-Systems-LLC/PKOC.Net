using System;
using PKOC.Net.MessageData;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace PKOC.Net
{
    /// <summary>
    /// Represents the event arguments for when a error is reported while processing PKOC.
    /// </summary>
    public class ReaderErrorReportedEventArgs : EventArgs
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="ReaderErrorReportedEventArgs"/> class.
        /// </summary>
        /// <param name="connectionId">The unique identifier for the connection.</param>
        /// <param name="address">The address of the device.</param>
        /// <param name="readerErrorResponseData"></param>
        public ReaderErrorReportedEventArgs(Guid connectionId, byte address,
            ReaderErrorResponseData readerErrorResponseData)
        {
            ConnectionId = connectionId;
            Address = address;
            ReaderErrorResponseData = readerErrorResponseData;
        }
        
        /// <summary>
        /// Gets the unique identifier for the connection.
        /// </summary>
        public Guid ConnectionId { get; }

        /// <summary>
        /// Gets the address of the device.
        /// </summary>
        public byte Address { get; }

        /// <summary>
        /// Represents the response data of a reader error.
        /// </summary>
        public ReaderErrorResponseData ReaderErrorResponseData { get; }
    }
}