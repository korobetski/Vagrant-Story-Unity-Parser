using System;
using UnityEngine;

namespace VS.FileFormats.MPD
{
    [Serializable]
    public class MPDRoomDoor
    {
        // 12 bytes
        public byte zoneId;
        public byte roomId;
        public ushort tileIndex;
        public uint doorId;
        public Vector2Int destination;
    }
}
