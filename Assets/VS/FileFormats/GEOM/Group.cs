using System;

namespace VS.FileFormats.GEOM
{
    [Serializable]
    public class Group
    {
        private Bone _bone;
        public short boneIndex;
        public ushort numVertices;


        public Bone bone { get => _bone; set => _bone = value; }
    }
}