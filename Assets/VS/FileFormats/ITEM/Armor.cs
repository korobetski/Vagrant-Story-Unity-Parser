using System;
using System.IO;
using VS.FileFormats.STATS;

namespace VS.FileFormats.ITEM
{
    [Serializable]
    public class Armor
    {
        public string name;
        public string description;
        public uint check;
        public byte nameId;
        public byte itemDescId;
        public byte id;
        public byte wepId = 0;
        public Enums.Armor.Type type;
        public ushort DPmax;
        public ushort PPmax;
        public ushort DP;
        public ushort PP;
        public byte GemSlots = 0;
        public Caracteristics caracteristics;
        public DamageTypes damageTypes;
        public Classes classes;
        public Affinities affinities;
        public Enums.Material.Type material;


        internal Armor SetDatasFromMPD(BinaryReader buffer)
        {
            // 0300 0000
            check = buffer.ReadUInt32();
            // C143 0005
            nameId = buffer.ReadByte();
            id = buffer.ReadByte();
            wepId = buffer.ReadByte();
            type = (Enums.Armor.Type)buffer.ReadByte();
            // 420E 0000
            DPmax = buffer.ReadUInt16();
            PPmax = buffer.ReadUInt16();
            // 420E 0000
            DP = buffer.ReadUInt16();
            PP = buffer.ReadUInt16();
            // 0002 0900
            GemSlots = buffer.ReadByte();
            caracteristics = new Caracteristics(buffer.ReadBytes(3));
            // 0005 0003
            buffer.ReadByte();
            damageTypes = new DamageTypes(buffer.ReadBytes(3));
            // 0000 0000 0000 0000
            classes = new Classes(buffer.ReadBytes(8));
            // 0205 05FF FFFB FB00
            affinities = new Affinities(buffer.ReadBytes(8));
            // 0200 0000
            material = (Enums.Material.Type)buffer.ReadUInt16();
            buffer.ReadBytes(2);
            return this;
        }
    }
}
