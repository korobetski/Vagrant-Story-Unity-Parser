using System;

namespace VS.FileFormats.STATS
{
    [Serializable]
    public class Caracteristics
    {
        public short STR;
        public short INT;
        public short AGI;
        public Caracteristics()
        {

        }
        public Caracteristics(byte[] vs)
        {
            STR = (sbyte)vs[0];
            INT = (sbyte)vs[1];
            AGI = (sbyte)vs[2];
        }

        public void Set(byte[] vs)
        {
            STR = (sbyte)vs[0];
            INT = (sbyte)vs[1];
            AGI = (sbyte)vs[2];
        }
    }
}