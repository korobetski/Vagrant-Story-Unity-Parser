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
        public Vector2Int unk1;
        public short decY;
        public Vector2Int unk2; // when unk2 = 255-255 it seems that this group contains a sprite sheet animation
        public short decZ;
        public Vector2Int unk3;
        public byte[] unkBytes;

        public uint numTriangles;
        public uint numQuads;
        public MPDFace[] faces;

        public Vector3 position { get => new Vector3(decX, decY, decZ); }
        public Vector3 positionYInv { get => new Vector3(decX, -decY, decZ); }
    }
}
