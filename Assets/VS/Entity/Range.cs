using UnityEngine;

namespace VS.Entity
{
    public class Range
    {
        // http://datacrystal.romhacking.net/wiki/Vagrant_Story:range
        public Vector3 origin;
        public byte shape;
        public byte angle;

        public Range(Vector3 rangeOrigin, byte option)
        {
            origin = rangeOrigin;
            shape = (byte)(option << 6);
            angle = (byte)(option >> 2);
        }
    }
}