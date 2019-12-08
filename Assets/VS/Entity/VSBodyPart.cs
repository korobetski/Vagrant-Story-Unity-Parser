using System.IO;

namespace VS.Entity
{
    internal class VSBodyPart
    {
        public ushort HP;
        public sbyte AGIBonus;
        public sbyte evasion;

        // 1 byte pad
        public sbyte blunt;
        public sbyte edged;
        public sbyte piercing;
        public sbyte Human;
        public sbyte Beast;
        public sbyte Undead;
        public sbyte Phantom;
        public sbyte Dragon;
        public sbyte Evil;
        // 2 byte pad

        public VSBodyPart(BinaryReader reader)
        {
            HP = reader.ReadUInt16();
            AGIBonus = reader.ReadSByte();
            evasion = reader.ReadSByte();
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

            VSSkill[] skills = new VSSkill[4];
            for (int i = 0; i < 4; i++)
            {
                skills[i] = new VSSkill(reader.ReadBytes(4));
            }

            VSEquipDatas armorDatas = new VSEquipDatas(reader);
            byte armorMaterial = reader.ReadByte();
            byte armorDropChance = reader.ReadByte(); // drop chance/255
            reader.ReadByte(); // ? always 1 for armours?
            reader.ReadByte();

            byte[] damageDistrib = reader.ReadBytes(6); // damage distribution % across each of six body parts

            reader.ReadByte();
            reader.ReadByte();
        }
    }
}