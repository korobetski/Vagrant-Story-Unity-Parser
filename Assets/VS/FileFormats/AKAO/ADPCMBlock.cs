namespace VS.FileFormats.AKAO
{
    // https://en.wikipedia.org/wiki/Adaptive_differential_pulse-code_modulation
    public class ADPCMBlock
    {
        public int range = 4;
        public int filter = 4;
        public int flagEnd = 1;
        public bool flagLooping = false;
        public bool flagLoop = false;
        public byte[] brr;

        public ADPCMBlock(byte a, byte b)
        {
            range = a & 0xF;
            filter = (a & 0xF0) >> 4;
            flagEnd = b & 0x1;
            flagLooping = (b & 0x2) > 0;
            flagLoop = (b & 0x4) > 0;
            brr = new byte[14];
        }

    }
}
