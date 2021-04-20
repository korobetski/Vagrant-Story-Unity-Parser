using System;
using System.IO;
using UnityEngine;
using VS.FileFormats;
using VS.Utils;

namespace VS.FileFormats.ZUD
{
    public class ZUD:ScriptableObject
    {
        public string Filename;
        public string ZoneId;
        public string UnitId;

        public byte idCharacter;
        public byte idWeapon;
        public Enums.Weapon.Type weaponCategory;
        public Enums.Material.Type weaponMaterial;
        public byte idShield;
        public Enums.Material.Type shieldMaterial;
        public byte unk1;
        public byte pad;

        public uint ptrCharacterSHP;
        public uint lenCharacterSHP;
        public uint ptrWeaponWEP;
        public uint lenWeaponWEP;
        public uint ptrShieldWEP;
        public uint lenShieldWEP;
        public uint ptrCommonSEQ;
        public uint lenCommonSEQ;
        public uint ptrBattleSEQ;
        public uint lenBattleSEQ;

        public SHP.SHP zudShape;
        public WEP.WEP zudWeapon;
        public WEP.WEP zudShield;
        public SEQ.SEQ zudComSeq;
        public SEQ.SEQ zudBatSeq;




        public void ParseFromFile(string filepath)
        {
            FileParser fp = new FileParser();
            fp.Read(filepath);

            // in MAP/Z***U**.ZUD
            if (fp.Ext == "ZUD")
            {
                Filename = fp.FileName;
                ZoneId = fp.FileName.Substring(1, 3);
                UnitId = fp.FileName.Substring(5, 2);
                ParseFromBuffer(fp.buffer, fp.FileSize);
            }

            fp.Close();
        }

        public void ParseFromBuffer(BinaryReader buffer, long limit)
        {

            idCharacter = buffer.ReadByte();
            idWeapon = buffer.ReadByte();
            weaponCategory = (Enums.Weapon.Type)buffer.ReadByte();
            weaponMaterial = (Enums.Material.Type)buffer.ReadByte();
            idShield = buffer.ReadByte();
            shieldMaterial = (Enums.Material.Type)buffer.ReadByte();
            unk1 = buffer.ReadByte();
            pad = buffer.ReadByte();

            // pointers
            ptrCharacterSHP = buffer.ReadUInt32();
            lenCharacterSHP = buffer.ReadUInt32();
            ptrWeaponWEP = buffer.ReadUInt32();
            lenWeaponWEP = buffer.ReadUInt32();
            ptrShieldWEP = buffer.ReadUInt32();
            lenShieldWEP = buffer.ReadUInt32();
            ptrCommonSEQ = buffer.ReadUInt32();
            lenCommonSEQ = buffer.ReadUInt32();
            ptrBattleSEQ = buffer.ReadUInt32();
            lenBattleSEQ = buffer.ReadUInt32();

            // shape section
            if (lenCharacterSHP > 0)
            {
                if (buffer.BaseStream.Position != ptrCharacterSHP)
                {
                    Debug.LogWarning("le pointeur ptrCharacterSHP n'est pas à la bonne place : " + buffer.BaseStream.Position + " != " + ptrCharacterSHP);
                    buffer.BaseStream.Position = ptrCharacterSHP;
                }
                zudShape = ScriptableObject.CreateInstance<SHP.SHP>();
                zudShape.Filename = Filename + "_ZSHP";
                zudShape.ParseFromBuffer(buffer, buffer.BaseStream.Position + lenCharacterSHP);
            }

            // weapon section
            if (lenWeaponWEP > 0)
            {
                if (buffer.BaseStream.Position != ptrWeaponWEP)
                {
                    Debug.LogWarning("le pointeur ptrWeaponWEP n'est pas à la bonne place : " + buffer.BaseStream.Position + " != " + ptrWeaponWEP);
                    buffer.BaseStream.Position = ptrWeaponWEP;
                }
                zudWeapon = ScriptableObject.CreateInstance<WEP.WEP>();
                zudWeapon.Filename = Filename + "_WEP";
                zudWeapon.WEPId = (byte)idWeapon;
                zudWeapon.material = (Enums.Material.Type)weaponMaterial;
                zudWeapon.ParseFromBuffer(buffer, buffer.BaseStream.Position + lenWeaponWEP);
            }

            // shield section
            if (lenShieldWEP > 0)
            {
                if (buffer.BaseStream.Position != ptrShieldWEP)
                {
                    Debug.LogWarning("le pointeur ptrShieldWEP n'est pas à la bonne place : " + buffer.BaseStream.Position + " != " + ptrShieldWEP);
                    buffer.BaseStream.Position = ptrShieldWEP;
                }
                zudShield = ScriptableObject.CreateInstance<WEP.WEP>();
                zudShield.Filename = Filename+"_WEP2";
                zudShield.WEPId = (byte)idShield;
                zudShield.material = (Enums.Material.Type)shieldMaterial;
                zudShield.ParseFromBuffer(buffer, buffer.BaseStream.Position + lenShieldWEP);
            }

            // common anim section
            if (lenCommonSEQ > 0)
            {
                if (buffer.BaseStream.Position != ptrCommonSEQ)
                {
                    Debug.LogWarning("le pointeur ptrCommonSEQ n'est pas à la bonne place : " + buffer.BaseStream.Position + " != " + ptrCommonSEQ);
                    buffer.BaseStream.Position = ptrCommonSEQ;
                }
                zudComSeq = ScriptableObject.CreateInstance<SEQ.SEQ>();
                zudComSeq.Filename = Filename + "_COM_SEQ";
                zudComSeq.ParseFromBuffer(buffer, buffer.BaseStream.Position + lenCommonSEQ);
            }

            // battle anim section
            if (lenBattleSEQ > 0)
            {
                if (buffer.BaseStream.Position != ptrBattleSEQ)
                {
                    Debug.LogWarning("le pointeur ptrBattleSEQ n'est pas à la bonne place : " + buffer.BaseStream.Position + " != " + ptrBattleSEQ);
                    buffer.BaseStream.Position = ptrBattleSEQ;
                }
                zudBatSeq = ScriptableObject.CreateInstance<SEQ.SEQ>();
                zudBatSeq.Filename = Filename + "_BAT_SEQ";
                zudBatSeq.ParseFromBuffer(buffer, buffer.BaseStream.Position + lenBattleSEQ);
            }
        }
    }
}
