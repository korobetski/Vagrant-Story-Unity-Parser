using System;

namespace VS.FileFormats.STATS
{
    [Serializable]
    public class Classes
    {
        public int Human = 0;
        public int Beast = 0;
        public int Undead = 0;
        public int Phantom = 0;
        public int Dragon = 0;
        public int Evil = 0;
        public Classes()
        {

        }

        public Classes(byte[] vs)
        {
            Human = (sbyte)vs[0];
            Beast = (sbyte)vs[1];
            Undead = (sbyte)vs[2];
            Phantom = (sbyte)vs[3];
            Dragon = (sbyte)vs[4];
            Evil = (sbyte)vs[5];
        }

        public void Set(byte[] vs)
        {
            Human = (sbyte)vs[0];
            Beast = (sbyte)vs[1];
            Undead = (sbyte)vs[2];
            Phantom = (sbyte)vs[3];
            Dragon = (sbyte)vs[4];
            Evil = (sbyte)vs[5];
        }
    }
}
