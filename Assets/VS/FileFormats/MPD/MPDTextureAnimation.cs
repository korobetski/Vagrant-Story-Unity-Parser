using System;

namespace VS.FileFormats.MPD
{
    [Serializable]
    public class MPDTextureAnimation
    {
        public byte type;
        public byte[] datas;
        // type is datas[6]
        // type 01 is a sprite sheet animation, 20 bytes length
        // type 02 is a sprite translation, 32 bytes length
        // type 06 is 36 bytes length
    }
}
