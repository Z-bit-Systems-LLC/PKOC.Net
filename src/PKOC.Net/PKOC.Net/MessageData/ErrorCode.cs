namespace PKOC.Net.MessageData
{
    public class ErrorCode
    {
        public const byte NoError = 0x00;
        public const byte ISO7816Status = 0x01;
        public const byte TimeoutAccessingCard = 0x02;
        public const byte ReservedForFutureUse = 0x03;
        public const byte MissingTLVInData = 0x04;
        public const byte TLVOutOfBounds = 0x05;
        public const byte MissingDataToCompleteRequest = 0x06;
        public const byte InvalidData = 0x07;
        public const byte MultipartOutOfSequence = 0x08;
        public const byte MultipartOutOfBounds = 0x09;
    }
}