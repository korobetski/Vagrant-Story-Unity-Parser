using System;
using System.IO;
using VS.FileFormats.STATS;

namespace VS.FileFormats.ITEM
{
    [Serializable]
    public class Grip
    {
        public string name;
        public string description;
        public ushort nameId;
        public uint check;
        public byte gripId;
        public Enums.Grip.Type type;
        public byte gemSlots;
        public Caracteristics caracteristics;
        public DamageTypes damageTypes;

        internal Grip SetDatasFromMPD(BinaryReader buffer)
        {
            //check = buffer.ReadUInt32();
            // 7900 1A04
            nameId = buffer.ReadUInt16();
            gripId = buffer.ReadByte();
            type = (Enums.Grip.Type)buffer.ReadByte();
            // 0001 00FF
            gemSlots = buffer.ReadByte();
            caracteristics = new Caracteristics(buffer.ReadBytes(3));
            // 0001 000A
            buffer.ReadByte();
            damageTypes = new DamageTypes(buffer.ReadBytes(3));
            buffer.ReadBytes(4);
            return this;
        }
    }
}
