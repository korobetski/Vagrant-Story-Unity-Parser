using System;
using VS.FileFormats.ITEM;

namespace VS.FileFormats.ZND
{
    [Serializable]
    public class ZNDWeapon
    {
        public VSEquipDatas blade;
        public VSEquipDatas grip;
        public VSEquipDatas[] gems = new VSEquipDatas[3];
        public Enums.Material.Type material;
        public byte dropChance;
        internal string name;
        // 2 bytes padding
    }
}