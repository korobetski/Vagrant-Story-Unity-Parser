using System;
using System.Collections.Generic;
using UnityEngine;

namespace VS.FileFormats.GEOM
{
    [Serializable]
    public class Face
    {
        public byte type;
        public byte size;
        public byte side;
        public byte alpha;
        public byte verticesCount;
        public List<ushort> vertices;
        public List<Color32> colors;
        public List<Vector2> uv;

        public Face()
        {
            vertices = new List<ushort>();
            uv = new List<Vector2>();
            colors = new List<Color32>();
        }
    }
}