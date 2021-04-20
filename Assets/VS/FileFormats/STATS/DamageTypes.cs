using System;

namespace VS.FileFormats.STATS
{
    [Serializable]
    public class DamageTypes
    {
        public short Blunt = 0;
        public short Edged = 0;
        public short Piercing = 0;

        public DamageTypes(byte[] vs)
        {
            Blunt = (sbyte)vs[0];
            Edged = (sbyte)vs[1];
            Piercing = (sbyte)vs[2];
        }
    }
}
