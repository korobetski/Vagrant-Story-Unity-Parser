using System;
using System.IO;
using VS.FileFormats.ITEM;
using VS.FileFormats.STATS;

namespace VS.FileFormats.ZND
{
    [Serializable]
    public class BodyPart
    {
        public ushort HP;
        public sbyte AGIBonus;
        public sbyte evasion;

        public DamageTypes damageTypes;
        public Affinities affinities;

        public Skill[] skills = new Skill[4];
        public ZNDArmor armor;

        public byte[] damageDistrib;

        public BodyPart(BinaryReader reader)
        {
            HP = reader.ReadUInt16();
            AGIBonus = reader.ReadSByte();
            evasion = reader.ReadSByte();
            damageTypes = new DamageTypes(reader.ReadBytes(4));
            affinities = new Affinities(reader.ReadBytes(8));
            for (int i = 0; i < 4; i++)
            {
                skills[i] = new Skill(reader.ReadBytes(4));
            }
            armor = new ZNDArmor();
            armor.item = new VSEquipDatas(reader);
            armor.material = (Enums.Material.Type) reader.ReadByte();
            armor.dropChance = reader.ReadByte();
            reader.ReadByte(); // ? always 1 for armours?
            reader.ReadByte();

            damageDistrib = reader.ReadBytes(6); // damage distribution % across each of six body parts

            reader.ReadByte();
            reader.ReadByte();
        }
    }
}