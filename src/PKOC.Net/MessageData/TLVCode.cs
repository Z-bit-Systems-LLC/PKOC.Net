namespace PKOC.Net.MessageData
{
    public enum TLVCode
    {
        TransactionIdentifier = 0x4C,
        ReaderIdentifier = 0x4D,
        PublicKey = 0x5A,
        ProtocolVersion = 0x5C,
        DigitalSignature = 0x9E,
        Error = 0xFB,
        CardPresentPayload = 0xFC,
        TransactionSequence = 0xFD,
    }
}