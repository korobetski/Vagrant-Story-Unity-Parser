using System;
using UnityEngine;

namespace VS.FileFormats.MPD
{

    [Serializable]
    public class MPDGroup
    {
        public byte visibility; // bitwise flags NW-W-SW-S-SE-E-NE-N
        public byte scaleFlag;
        public byte scale = 8;
        public ushort overlapping;
        public short decX;
        public ushort unk1;
        public short decY;
        public ushort unk2;
        public short decZ;
        public ushort unk3;
        public byte[] unkBytes;

        public uint numTriangles;
        public uint numQuads;
        public MPDFace[] faces;

        public Vector3 position { get => new Vector3(decX, decY, decZ); }
    }
}
