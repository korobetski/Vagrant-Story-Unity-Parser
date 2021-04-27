using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VS.FileFormats.ITEM;

namespace VS.FileFormats.SYD
{
    [Serializable]
    public class SYDAssoc
    {
        public byte[] d;
    }

    public class SYD:ScriptableObject
    {
        // SYD files manage workshop crafting
        // there is 3 SYD files in MENU/ : ARMOR.SYD, BLADE.SYD, SHIELD.SYD
        // ARMOR -> 80 entries
        // BLADE -> 96 entries
        // SHIELD -> 16 entries

        public string Filename;
        public uint pointer1;
        public uint pointer2;
        public int itemCount;
        public SYDAssoc[] section0; // item associations
        public SYDAssoc[] section1; // material associations ~half count of section0
        public SYDItem[] items;



        public void ParseFromFile(string filepath)
        {
            FileParser fp = new FileParser();
            fp.Read(filepath);

            // in MENU/*.SYD
            List<string> validFiles = new List<string>{ "ARMOR", "BLADE", "SHIELD" };
            if (validFiles.Contains(fp.FileName) && fp.Ext == "SYD")
            {
                Filename = fp.FileName;
                ParseFromBuffer(fp.buffer, fp.FileSize);
            }

            fp.Close();
        }

        public void ParseFromBuffer(BinaryReader buffer, long limit)
        {
            buffer.ReadUInt32(); // header length = 0x0C000000 for the 3 SYD
            pointer1 = buffer.ReadUInt32(); // pointer to material associations
            pointer2 = buffer.ReadUInt32(); // pointer to item stats

            itemCount = -1;
            while (buffer.ReadByte() == 0) itemCount++;
            buffer.BaseStream.Position -= 2;

            // shields associations are skipped in ARMOR.SYD
            section0 = new SYDAssoc[itemCount];
            section0[0] = new SYDAssoc();
            section0[0].d = new byte[itemCount];
            for (uint i = 1; i < itemCount; i++)
            {
                section0[i] = new SYDAssoc();
                section0[i].d = buffer.ReadBytes(itemCount);
            }
            int rem = (int)buffer.BaseStream.Position % 4;
            if (rem != 0) buffer.ReadBytes(4 - rem);

            buffer.BaseStream.Position = pointer1;
            switch (Filename)
            {
                case "ARMOR":
                    //buffer.BaseStream.Position = 0x1498;
                    section1 = new SYDAssoc[32];
                    for (uint i = 0; i < 32; i++)
                    {
                        section1[i] = new SYDAssoc();
                        section1[i].d = buffer.ReadBytes(32);
                    }

                    buffer.ReadBytes(8); // first is empty
                    items = new SYDItem[80];
                    for (var i = 0; i < 80; i++)
                    {
                        //ID|ID.WEP|armor type[01-05]|00|STR|INT|AGI|always 00
                        items[i] = new SYDItem();
                        items[i].SetArmorDatas(buffer.ReadBytes(8));
                    }
                    break;
                case "BLADE":
                    //buffer.BaseStream.Position = 0x2DE0;

                    // the same patern is repeated 5 times, maybe each time per possible blade material (excluding wood & leather)
                    section1 = new SYDAssoc[50];
                    for (uint i = 0; i < 50; i++)
                    {
                        section1[i] = new SYDAssoc();
                        section1[i].d = buffer.ReadBytes(50);
                    }

                    buffer.BaseStream.Position = pointer2;
                    buffer.ReadBytes(16); // first is empty

                    items = new SYDItem[95];
                    for (var i = 0; i < 95; i++)
                    {
                        // Damage types : 1 = Blunt - 2 = Edged  -  3 = Piercing
                        //22   22         03          02    02  02  0000 23  00  FA  00  06  05   06       01       Holy Win
                        //ID|ID.WEP|weapon type|damage type|02|Risk|0000|STR|INT|AGI|00|Range|?|range|always 01
                        items[i] = new SYDItem();
                        items[i].SetBladeDatas(buffer.ReadBytes(16));
                    }
                    break;
                case "SHIELD":

                    section1 = new SYDAssoc[8];
                    for (uint i = 0; i < 8; i++)
                    {
                        section1[i] = new SYDAssoc();
                        section1[i].d = buffer.ReadBytes(8); // 0 padding + num of smithing materials
                    }

                    buffer.BaseStream.Position = pointer2;
                    buffer.ReadBytes(8); // first is empty
                    items = new SYDItem[16];
                    for (var i = 0; i < 16; i++)
                    {
                        //ID|ID.WEP|armor type 01(shield)|gem slots|STR|INT|AGI|always 00
                        items[i] = new SYDItem();
                        items[i].SetArmorDatas(buffer.ReadBytes(8));
                    }
                    break;
            }
        }

        public void SetNames(ItemList itemStr)
        {
            int dec = 0;
            switch(Filename)
            {
                case "ARMOR":
                    // ARMOR also contains shields, but gem slots arn't set with the good value
                    dec = 126;
                    break;
                case "BLADE":
                    dec = 0;
                    break;
                case "SHIELD":
                    dec = 126;
                    break;
            }

            for (var i = 0; i < items.Length; i++)
            {
                items[i].name = itemStr.GetName(items[i].id + dec);
            }
        }
    }
}
