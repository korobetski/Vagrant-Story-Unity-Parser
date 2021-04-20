using System;
using System.IO;
using UnityEngine;
using VS.FileFormats.STATS;

namespace VS.FileFormats.ITEM
{
    [Serializable]
    public class Gem
    {
        public string name;
        public string description;
        public ushort nameId;
        public uint check;
        public byte gemId;
        public byte gemEffect;
        public Caracteristics caracteristics;
        public Classes classes;
        public Affinities affinities;

        public Gem()
        {
            caracteristics = new Caracteristics();
            classes = new Classes();
            affinities = new Affinities();
        }

        public Gem SetDatasFromMPD(BinaryReader buffer)
        {
            nameId = buffer.ReadUInt16();
            gemId = buffer.ReadByte();
            buffer.ReadByte();

            gemEffect = buffer.ReadByte();
            caracteristics.Set(buffer.ReadBytes(3));

            classes.Set(buffer.ReadBytes(8));

            affinities.Set(buffer.ReadBytes(8));

            buffer.ReadBytes(4);

            return this;
        }
    }
}
