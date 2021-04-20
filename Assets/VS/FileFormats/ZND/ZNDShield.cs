using System;
using VS.FileFormats.ITEM;

namespace VS.FileFormats.ZND
{
    [Serializable]
    public class ZNDShield
    {
        public VSEquipDatas shield;
        public VSEquipDatas[] gems = new VSEquipDatas[3];
        public Enums.Material.Type material;

        public byte dropChance;
        // 2 bytes padding
    }
}