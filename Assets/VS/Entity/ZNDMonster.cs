using System;
using System.IO;
using UnityEngine;
using VS.Utils;

namespace VS.Entity
{

    public class ZNDMonster: MonoBehaviour
    {
        [SerializeField]
        public new string name;
        [SerializeField]
        public ushort HP;
        [SerializeField]
        public ushort MP;
        [SerializeField]
        public byte STR;
        [SerializeField]
        public byte INT;
        [SerializeField]
        public byte AGI;
        [SerializeField]
        public byte speed;
        [SerializeField]
        public uint MPDId;

        public ZNDMonster(BinaryReader reader)
        {
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