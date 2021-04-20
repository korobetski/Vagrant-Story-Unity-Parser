using System;
using System.IO;

namespace VS.FileFormats.ITEM
{
    [Serializable]
    public class MiscItem
    {
        public ushort nameId;
        public string name;
        public string description;
        public byte flag;
        public byte quantity;

        internal MiscItem SetDatasFromMPD(BinaryReader buffer)
        {
            nameId = buffer.ReadUInt16();
            flag = buffer.ReadByte();
            quantity = buffer.ReadByte();
            return this;
        }
    }
}
