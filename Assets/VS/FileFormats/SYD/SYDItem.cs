using System;
using VS.FileFormats.STATS;

namespace VS.FileFormats.SYD
{
    [Serializable]
    public class SYDItem
    {
        public string name;
        public byte id;
        public byte wepId;
        public Enums.Item.Type type;
        public byte gemSlots;
        public Enums.Damage.Type damageType;
        public byte risk;
        public Caracteristics caracteristics = new Caracteristics();
        public byte range;

        public void SetArmorDatas(byte[] b)
        {
            id = b[0];
            wepId = b[1];
            type = (Enums.Item.Type) (10 + b[2]);
            gemSlots = b[3];
            caracteristics.STR = (sbyte)b[4];
            caracteristics.INT = (sbyte)b[5];
            caracteristics.AGI = (sbyte)b[6];
        }

        public void SetBladeDatas(byte[] b)
        {
            id = b[0];
            wepId = b[1];
            type = (Enums.Item.Type) b[2];

            damageType = (Enums.Damage.Type)b[4];
            risk = b[5];

            caracteristics.STR = (sbyte)b[8];
            caracteristics.INT = (sbyte)b[9];
            caracteristics.AGI = (sbyte)b[10];

            range = b[12];
        }
    }
}
