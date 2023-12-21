namespace PKOC.Net.MessageData
{
    public enum TLVCode
    {
        TransactionIdentifier = 0x4C,
        SupportedProtocol = 0x5C,
        Error = 0xFB,
        CardPresentPayload = 0xFC,
        TransactionSequence = 0xFD,
    }
}