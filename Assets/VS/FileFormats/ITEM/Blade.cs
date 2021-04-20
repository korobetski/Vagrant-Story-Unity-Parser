using System;
using System.IO;
using UnityEngine;
using VS.FileFormats.STATS;

namespace VS.FileFormats.ITEM
{
    [Serializable]
    public class Blade
    {
        public string name;
        [TextArea]
        public string description;
        public uint check;
        public byte nameId;
        public byte id;
        public byte wepId;
        public Enums.Blade.Category bladeType;
        public ushort DPmax;
        public ushort PPmax;
        public ushort DP;
        public ushort PP;
        public Caracteristics caracteristics;
        public byte RISK;
        public Enums.Damage.Type damageType;
        public byte rawDamages;
        // 3 bytes pad
        public Range range;
        public Classes classes;
        public Affinities affinities;
        public Enums.Material.Type material;


        internal Blade SetDatasFromMPD(BinaryReader buffer)
        {
            // must be 44 bytes
            check = buffer.ReadUInt32();
            // 5353 5309
            nameId = buffer.ReadByte();
            id = buffer.ReadByte();
            wepId = buffer.ReadByte();
            bladeType = (Enums.Blade.Category)buffer.ReadByte();
            // C819 9600
            DPmax = buffer.ReadUInt16();
            PPmax = buffer.ReadUInt16();
            // C819 0000
            DP = buffer.ReadUInt16();
            PP = buffer.ReadUInt16();
            // 0B01 FC02
            caracteristics = new Caracteristics(buffer.ReadBytes(3));
            RISK = buffer.ReadByte();
            // 0B00 0000
            rawDamages = buffer.ReadByte();
            buffer.ReadBytes(3);
            // 0908 0901
            range = new Range(new UnityEngine.Vector3(buffer.ReadByte(), buffer.ReadByte(), buffer.ReadByte()), buffer.ReadByte());
            // FFFF FEFF FFFB 0000
            classes = new Classes(buffer.ReadBytes(8));
            // 0805 05FD FD02 0200
            affinities = new Affinities(buffer.ReadBytes(8));
            // 0300
            material = (Enums.Material.Type)buffer.ReadUInt16();
            // 0000
            buffer.ReadBytes(2);
            return this;
        }
    }
}
