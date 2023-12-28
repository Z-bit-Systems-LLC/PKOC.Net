namespace PKOC.Net.MessageData
{
    public enum TLVCode
    {
        TransactionIdentifier = 0x4C,
        ReaderIdentifier = 0x4D,
        ProtocolVersion = 0x5C,
        Error = 0xFB,
        CardPresentPayload = 0xFC,
        TransactionSequence = 0xFD
    }
}