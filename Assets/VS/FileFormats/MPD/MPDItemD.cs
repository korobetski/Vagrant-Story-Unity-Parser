using System;

namespace VS.FileFormats.MPD
{
    [Serializable]
    public class MPDItemD
    {
        public byte[] vs;

        public MPDItemD(byte[] vs)
        {
            this.vs = vs;
        }
    }
}