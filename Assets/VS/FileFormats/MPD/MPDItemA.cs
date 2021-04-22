using System;

namespace VS.FileFormats.MPD
{
    [Serializable]
    public class MPDItemA
    {
        public byte[] vs;

        public MPDItemA(byte[] vs)
        {
            this.vs = vs;
        }
    }
}