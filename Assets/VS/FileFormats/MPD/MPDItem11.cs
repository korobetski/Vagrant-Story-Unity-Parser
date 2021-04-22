using System;

namespace VS.FileFormats.MPD
{
    [Serializable]
    public class MPDItem11
    {
        public byte[] vs;

        public MPDItem11(byte[] vs)
        {
            this.vs = vs;
        }
    }
}