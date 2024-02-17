using System;
using PKOC.Net.MessageData;

namespace PKOC.Net
{
    public class ReaderErrorReportedEventArgs : EventArgs
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public ReaderErrorResponseData ReaderErrorResponseData { get; }

        public ReaderErrorReportedEventArgs(ReaderErrorResponseData readerErrorResponseData)
        {
            ReaderErrorResponseData = readerErrorResponseData;
        }
    }
}