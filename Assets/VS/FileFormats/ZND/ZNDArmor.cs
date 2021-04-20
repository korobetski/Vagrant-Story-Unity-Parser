using System;
using VS.FileFormats.ITEM;

namespace VS.FileFormats.ZND
{
    [Serializable]
    public class ZNDArmor
    {
        public VSEquipDatas item;
        public Enums.Material.Type material;
        public byte dropChance;
        // 2 bytes padding
    }
}