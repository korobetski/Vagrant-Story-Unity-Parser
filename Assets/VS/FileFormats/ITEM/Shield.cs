using System;
using System.IO;
using VS.FileFormats.STATS;

namespace VS.FileFormats.ITEM
{
    [Serializable]
    public class Shield
    {
        public string name;
        public uint check;
        public byte nameId;
        public byte itemDescId;
        public byte id;
        public byte wepId;
        public Enums.Armor.Type Type = Enums.Armor.Type.SHIELD;
        public ushort DPmax;
        public ushort PPmax;
        public ushort DP;
        public ushort PP;
        public byte GemSlots;
        public Caracteristics caracteristics;
        public DamageTypes damageTypes;
        public Classes classes;
        public Affinities affinities;
        public Enums.Material.Type material;
        public Gem[] gems = new Gem[3];


        internal Shield SetDatasFromMPD(BinaryReader buffer)
        {

            check = buffer.ReadUInt32();

            nameId = buffer.ReadByte();
            id = buffer.ReadByte();
            wepId = buffer.ReadByte();
            Type = (Enums.Armor.Type)buffer.ReadByte();

            DPmax = buffer.ReadUInt16();
            PPmax = buffer.ReadUInt16();

            DP = buffer.ReadUInt16();
            PP = buffer.ReadUInt16();

            GemSlots = buffer.ReadByte();
            caracteristics = new Caracteristics(buffer.ReadBytes(3));

            buffer.ReadByte();
            damageTypes = new DamageTypes(buffer.ReadBytes(3));

            classes = new Classes(buffer.ReadBytes(8));

            affinities = new Affinities(buffer.ReadBytes(8));

            material = (Enums.Material.Type)buffer.ReadUInt16();
            buffer.ReadBytes(6);

            return this;
        }
    }
}
