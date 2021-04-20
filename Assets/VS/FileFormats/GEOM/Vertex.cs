using System;
using UnityEngine;


namespace VS.FileFormats.GEOM
{
    [Serializable]
    public class Vertex
    {
        public uint index;
        public byte group;
        public Vector4 position;
        public Color32 color;

        private Bone _bone;
        private BoneWeight _boneWeight;

        public Bone bone { get => _bone; set => _bone = value; }
        public BoneWeight boneWeight { get => _boneWeight; set => _boneWeight = value; }

        public Vector3 GetAbsPosition()
        {
            Vector3 pos = new Vector3(position.x, position.y, position.z);
            if (_bone != null) pos.x -= _bone.GetDec();
            return pos;
        }

        internal Vertex Negate()
        {
            position = -position;
            return this;
        }
    }
}
