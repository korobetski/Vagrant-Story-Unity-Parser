using System;
using System.IO;
using VS.Utils;

namespace VS.Entity
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
        public uint MPDId;

        public ZNDMonster(byte[] v)
        {
            MemoryStream memStream = new MemoryStream(v);
            BinaryReader reader = new BinaryReader(memStream);

            reader.ReadUInt16();
            reader.ReadByte(); // location in table for 3d model special effect
            reader.ReadByte();
            name = L10n.Translate(reader.ReadBytes(18));
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

            VSEquipDatas weaponBladeDatas = new VSEquipDatas(reader);
            VSEquipDatas weaponGripDatas = new VSEquipDatas(reader);
            VSEquipDatas weaponGem1Datas = new VSEquipDatas(reader);
            VSEquipDatas weaponGem2Datas = new VSEquipDatas(reader);
            VSEquipDatas weaponGem3Datas = new VSEquipDatas(reader);
            byte weaponMaterial = reader.ReadByte();
            byte weaponDropChance = reader.ReadByte(); // drop chance/255
            reader.ReadByte();
            reader.ReadByte();
            string weaponName = L10n.Translate(reader.ReadBytes(18));

            VSEquipDatas shieldDatas = new VSEquipDatas(reader);
            VSEquipDatas shieldGem1Datas = new VSEquipDatas(reader);
            VSEquipDatas shieldGem2Datas = new VSEquipDatas(reader);
            VSEquipDatas shieldGem3Datas = new VSEquipDatas(reader);
            byte shieldMaterial = reader.ReadByte();
            byte shieldDropChance = reader.ReadByte(); // drop chance/255
            reader.ReadByte();
            reader.ReadByte();

            VSEquipDatas accessoryDatas = new VSEquipDatas(reader);
            byte accessoryDropChance = reader.ReadByte(); // drop chance/255
            reader.ReadByte();
            reader.ReadByte();
            reader.ReadByte();

            VSBodyPart[] bodyParts = new VSBodyPart[6];
            for (int i = 0; i < 6; i++)
            {
                bodyParts[i] = new VSBodyPart(reader);
            }

            MPDId = reader.ReadUInt32();
        }
    }
}