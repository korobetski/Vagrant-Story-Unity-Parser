using UnityEngine;


namespace VS.Entity
{
    public class VSBone
    {
        public uint index;
        public string name;
        public int length;
        public VSBone parent;
        public int parentIndex;
        public Vector3 offset;
        public int mode;

        public VSBone()
        {

        }
    }
}
