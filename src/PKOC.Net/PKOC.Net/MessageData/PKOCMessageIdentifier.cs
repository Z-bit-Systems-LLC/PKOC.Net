namespace PKOC.Net.MessageData
{
    public enum PKOCMessageIdentifier
    {
        CardPresentResponse = 0xE0,
        AuthenticationRequest = 0xE1,
        AuthorizationResponse = 0xE2,
        NextTransactionRequest = 0xE3,
        TransactionRefreshResponse = 0xE4,
        ReaderErrorResponse = 0xFE
    }
}