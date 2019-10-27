using System.Collections.Generic;
using UnityEngine;

namespace VS.Entity
{
    public class VSFace
    {
        public int type;
        public int size;
        public int side;
        public int alpha;
        public uint verticesCount;
        public List<int> vertices;
        public List<Vector2> uv;

        public VSFace()
        {
        }
    }
}