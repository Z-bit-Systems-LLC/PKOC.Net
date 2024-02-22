using System;
using PKOC.Net.MessageData;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace PKOC.Net
{
    public class ReaderErrorReportedEventArgs : EventArgs
    {
        public Guid ConnectionId { get; }
        
        public byte Address { get; }

        public ReaderErrorResponseData ReaderErrorResponseData { get; }

        public ReaderErrorReportedEventArgs(Guid connectionId, byte address, ReaderErrorResponseData readerErrorResponseData)
        {
            ConnectionId = connectionId;
            Address = address;
            ReaderErrorResponseData = readerErrorResponseData;
        }
    }
}