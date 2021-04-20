using System;
using UnityEngine;

namespace VS.FileFormats.MPD
{
    [Serializable]
    public class MPDTrap
    {
        public Vector2Int position;
        //public ushort pad;
        public ushort skillId;
        public byte unk1;
        public byte save;
        public byte saveIndex;
        public byte zero;
    }
}
