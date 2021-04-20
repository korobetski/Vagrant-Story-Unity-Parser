using System;

namespace VS.FileFormats.MPD
{
    [Serializable]
    public class MPDTile
    {
        public byte floorHeight;
        public byte floorMode;
        public byte ceilHeight;
        public byte ceilMode;
        public byte[] properties;
    }
}
