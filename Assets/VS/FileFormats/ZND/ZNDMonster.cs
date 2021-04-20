using System;
using System.IO;
using UnityEngine;
using VS.FileFormats.ITEM;
using VS.Utils;

namespace VS.FileFormats.ZND
{
    [Serializable]
    public class ZNDMonster
    {
        public string name;
        public ushort HP;
        public ushort MP;
        public byte STR;
        public byte INT;
        public byte AGI;
        public byte speed;
        public ZNDWeapon weapon;
        public ZNDShield shield;
        public ZNDArmor accessory;
        public BodyPart[] bodyParts = new BodyPart[6];
        public uint MPDId;


        public ZNDMonster(BinaryReader reader)
        {
            reader.ReadUInt16();
            reader.ReadByte(); // location in table for 3d model special effect
            reader.ReadByte();
            name = L10n.CleanTranslate(reader.ReadBytes(24));
            HP = reader.ReadUInt16();
            MP = reader.ReadUInt16();
            STR = reader.ReadByte();
            INT = reader.ReadByte();
            AGI = reader.ReadByte();
            reader.ReadByte();
            reader.ReadByte();
            reader.ReadByte(); // walking speed whilst carrrying crates, monster don't use this :D
            reader.ReadByte();
            speed = reader.ReadByte();
            reader.ReadByte();
            reader.ReadByte();
            reader.ReadByte();
            reader.ReadByte();
            reader.ReadByte(); // 0x10
            reader.ReadByte(); // 0x11
            reader.ReadByte();
            reader.ReadByte();
            reader.ReadByte();
            reader.ReadByte();
            reader.ReadByte();
            reader.ReadByte();

            weapon = new ZNDWeapon();
            weapon.blade = new VSEquipDatas(reader);
            weapon.grip = new VSEquipDatas(reader);
            weapon.gems[0] = new VSEquipDatas(reader);
            weapon.gems[1] = new VSEquipDatas(reader);
            weapon.gems[2] = new VSEquipDatas(reader);
            weapon.material = (Enums.Material.Type)reader.ReadByte();
            weapon.dropChance = reader.ReadByte(); // drop chance/255
            reader.ReadByte();
            reader.ReadByte();
            weapon.name = L10n.CleanTranslate(reader.ReadBytes(24));

            shield = new ZNDShield();
            shield.shield = new VSEquipDatas(reader);
            shield.gems[0] = new VSEquipDatas(reader);
            shield.gems[1] = new VSEquipDatas(reader);
            shield.gems[2] = new VSEquipDatas(reader);
            shield.material = (Enums.Material.Type)reader.ReadByte();
            shield.dropChance = reader.ReadByte(); // drop chance/255
            reader.ReadByte();
            reader.ReadByte();

            accessory = new ZNDArmor();
            accessory.item = new VSEquipDatas(reader);
            accessory.dropChance = reader.ReadByte(); // drop chance/255
            reader.ReadByte();
            reader.ReadByte();
            reader.ReadByte();

            for (int i = 0; i < 6; i++)
            {
                bodyParts[i] = new BodyPart(reader);
            }

            MPDId = reader.ReadUInt32();
        }
    }
}