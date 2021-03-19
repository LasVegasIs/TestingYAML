namespace Crey.Misc
{
    public class XorEncrypt
    {
        public byte outEncodingByte = 0xCA;
        public byte outIncrement = 0x5B;

        public byte inDecodingByte = 0xAC;
        public byte inIncrement = 0xB5;

        public XorEncrypt SwitchToClientMode()
        {
            ObjectHelper.Swap(ref inIncrement, ref outIncrement);
            ObjectHelper.Swap(ref inDecodingByte, ref outEncodingByte);
            return this;
        }

        public void OnProcessReceivedRawData(byte[] data, int length)
        {
            ProcessData(data, length, ref inDecodingByte, inIncrement);
        }

        public void OnProcessOutputPacketData(byte[] data, int length)
        {
            ProcessData(data, length, ref outEncodingByte, outIncrement);
        }

        private static void ProcessData(byte[] data, int length, ref byte code, byte increment)
        {
            unchecked
            {
                for (var i = 0; i < length; i++)
                {
                    data[i] ^= code;
                    code += increment;
                }
            }
        }
    }
}
