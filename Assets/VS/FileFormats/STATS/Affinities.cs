using System;
namespace VS.FileFormats.STATS
{
    [Serializable]
    public class Affinities
    {
        public int Physical = 0;
        public int Air = 0;
        public int Fire = 0;
        public int Earth = 0;
        public int Water = 0;
        public int Light = 0;
        public int Dark = 0;
        public Affinities()
        {

        }
        public Affinities(byte[] vs)
        {
            Physical = (sbyte)vs[0];
            Air = (sbyte)vs[1];
            Fire = (sbyte)vs[2];
            Earth = (sbyte)vs[3];
            Water = (sbyte)vs[4];
            Light = (sbyte)vs[5];
            Dark = (sbyte)vs[6];
        }

        public void Set(byte[] vs)
        {
            Physical = (sbyte)vs[0];
            Air = (sbyte)vs[1];
            Fire = (sbyte)vs[2];
            Earth = (sbyte)vs[3];
            Water = (sbyte)vs[4];
            Light = (sbyte)vs[5];
            Dark = (sbyte)vs[6];
        }
    }
}
