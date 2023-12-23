namespace PKOC.Net.MessageData
{
    public class TLVData
    {
        public TLVData(TLVCode tlvCode, int length, byte[] data)
        {
            TLVCode = tlvCode;
            Length = length;
            Data = data;
        }

        public TLVCode TLVCode { get; }
        
        public int Length { get; }

        public byte[] Data { get; }
    }
}