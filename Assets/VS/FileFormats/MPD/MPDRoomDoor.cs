using System;

namespace VS.FileFormats.MPD
{
    [Serializable]
    public class MPDRoomDoor
    {
        // 12 bytes
        public byte zoneId;
        public byte roomId;
        public ushort tileIndex;
        public ushort[] destination;
        public uint doorId;
    }
}
