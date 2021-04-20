using System;
using System.IO;
using UnityEngine;

namespace VS.FileFormats.ITEM
{
    [Serializable]
    public class VSEquipDatas
    {
        // http://datacrystal.romhacking.net/wiki/Vagrant_Story:equip_data

        public ushort nameId;
        public byte id;
        public byte wepId;
        public byte category; // blade, grip, armor
        public sbyte STR;
        public sbyte INT;
        public sbyte AGI;
        public ushort DP;
        public ushort DPMax;
        public ushort PP;
        public ushort PPMax;
        public byte damageType;
        public byte costType;
        public byte cost;
        public byte material;
        // 1 byte pad
        public byte gem;
        public byte gemEffects;
        public byte ramId;
        public Range range;
        // 1 byte pad
        public sbyte blunt;
        public sbyte edged;
        public sbyte piercing;
        public sbyte Human = 0;
        public sbyte Beast = 0;
        public sbyte Undead = 0;
        public sbyte Phantom = 0;
        public sbyte Dragon = 0;
        public sbyte Evil = 0;
        // 2 byte pad
        public sbyte Physical = 0;
        public sbyte Air = 0;
        public sbyte Fire = 0;
        public sbyte Earth = 0;
        public sbyte Water = 0;
        public sbyte Light = 0;
        public sbyte Dark = 0;
        // 1 byte pad


        public VSEquipDatas()
        {

        }

        public VSEquipDatas(BinaryReader reader)
        {

            nameId = reader.ReadUInt16();
            id = reader.ReadByte();
            wepId = reader.ReadByte();
            category = reader.ReadByte();
            STR = reader.ReadSByte();
            INT = reader.ReadSByte();
            AGI = reader.ReadSByte();
            DP = reader.ReadUInt16();
            DPMax = reader.ReadUInt16();
            PP = reader.ReadUInt16();
            PPMax = reader.ReadUInt16();
            damageType = reader.ReadByte();
            costType = reader.ReadByte();
            cost = reader.ReadByte();
            material = reader.ReadByte();
            // 1 byte pad
            reader.ReadByte();
            gem = reader.ReadByte();
            gemEffects = reader.ReadByte();
            ramId = reader.ReadByte();
            Range range = new Range(new Vector3(reader.ReadByte(), reader.ReadByte(), reader.ReadByte()), reader.ReadByte());
            // 1 byte pad
            reader.ReadByte();
            blunt = reader.ReadSByte();
            edged = reader.ReadSByte();
            piercing = reader.ReadSByte();
            Human = reader.ReadSByte();
            Beast = reader.ReadSByte();
            Undead = reader.ReadSByte();
            Phantom = reader.ReadSByte();
            Dragon = reader.ReadSByte();
            Evil = reader.ReadSByte();
            // 2 byte pad
            reader.ReadByte();
            reader.ReadByte();
            sbyte Physical = reader.ReadSByte();
            sbyte Air = reader.ReadSByte();
            sbyte Fire = reader.ReadSByte();
            sbyte Earth = reader.ReadSByte();
            sbyte Water = reader.ReadSByte();
            sbyte Light = reader.ReadSByte();
            sbyte Dark = reader.ReadSByte();
            // 1 byte pad
            reader.ReadByte();
        }
    }
}