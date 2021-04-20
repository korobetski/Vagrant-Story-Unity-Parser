using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VS.FileFormats.ITEM;

namespace VS.FileFormats.SYD
{
    public class SYD:ScriptableObject
    {
        // SYD files manage workshop crafting
        // there is 3 SYD files in MENU/ : ARMOR.SYD, BLADE.SYD, SHIELD.SYD
        // ARMOR -> 80 entries
        // BLADE -> 90 entries
        // SHIELD -> 16 entries

        public string Filename;
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
            // TODO : investigations on headers
            // it must be workshop associations
            switch(Filename)
            {
                case "ARMOR":
                    buffer.BaseStream.Position = 0x1498;
                    items = new SYDItem[80];
                    for (var i = 0; i < 80; i++)
                    {
                        //ID|ID.WEP|armor type[01-05]|00|STR|INT|AGI|always 00
                        items[i] = new SYDItem();
                        items[i].SetArmorDatas(buffer.ReadBytes(8));
                    }
                    break;
                case "BLADE":
                    buffer.BaseStream.Position = 0x2DE0;
                    items = new SYDItem[90];
                    for (var i = 0; i < 90; i++)
                    {
                        // Damage types : 1 = Blunt - 2 = Edged  -  3 = Piercing
                        //22   22         03          02    02  02  0000 23  00  FA  00  06  05   06       01       Holy Win
                        //ID|ID.WEP|weapon type|damage type|02|Risk|0000|STR|INT|AGI|00|Range|?|range|always 01
                        items[i] = new SYDItem();
                        items[i].SetBladeDatas(buffer.ReadBytes(16));
                    }
                    break;
                case "SHIELD":
                    buffer.BaseStream.Position = 0x0178;
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
