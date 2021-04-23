using System;

namespace VS.FileFormats.TIM
{
    [Serializable]
    public class GIMFrame
    {
        public ushort fwidth;
        public ushort fheigth;
        public byte numBlocks;
        public byte unk1;
        public short numPalette256;
        public short additionalFrames;
        public ushort[] hbt;
    }
}