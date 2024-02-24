// ReSharper disable ClassNeverInstantiated.Global
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace PKOC.Net.MessageData
{
    public enum PKOCMessageIdentifier
    {
        CardPresentResponse = 0xE0,
        AuthenticationRequest = 0xE1,
        AuthenticationResponse = 0xE2,
        NextTransactionRequest = 0xE3,
        TransactionRefreshResponse = 0xE4,
        ReaderErrorResponse = 0xFE
    }
}