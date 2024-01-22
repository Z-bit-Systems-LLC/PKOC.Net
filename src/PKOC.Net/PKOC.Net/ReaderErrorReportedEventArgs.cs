using System;
using PKOC.Net.MessageData;

namespace PKOC.Net
{
    public class ReaderErrorReportedEventArgs : EventArgs
    {
        public ReaderErrorResponseData ReaderErrorResponseData { get; }

        public ReaderErrorReportedEventArgs(ReaderErrorResponseData readerErrorResponseData)
        {
            ReaderErrorResponseData = readerErrorResponseData;
        }
    }
}