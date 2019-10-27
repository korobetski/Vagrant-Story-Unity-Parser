using System.Collections.Generic;
using UnityEngine;
using VS.Data;


// MENU/ARMOR.SYD
// MENU/BLADE.SYD
// MENU/SHIELD.SYD


namespace VS.Parser
{
    public class SYD : FileParser
    {
        public bool UseDebug = false;
        public bool Parsed = false;
        public void Parse(string filePath)
        {
            PreParse(filePath);
            // Header
            if (UseDebug)
            {
                Debug.Log("SYD Parse :: " + FileName + "   File len : " + buffer.BaseStream.Length);
            }

            uint n1 = buffer.ReadByte();
            buffer.ReadBytes(3);
            uint n2 = buffer.ReadByte();
            uint n3 = buffer.ReadByte();
            buffer.ReadBytes(2);
            uint n4 = buffer.ReadByte();
            uint n5 = buffer.ReadByte();
            buffer.ReadBytes(6);
            if (UseDebug)
            {
                Debug.Log(n1 + ", " + n2 + ", " + n3 + ", " + n4 + ", " + n5);
            }
            // SECTION 1 : List of Ids inside Byte (never > 16 for shield.SYD) this is maybe combinaison datas how to mix items to get a superior item
            // num of items
            // SECTION 2 : List of crafting material Ids inside Byte (never > 7) this is maybe combinaison datas how to mix materials to get damascus for example
            // SECTION 3 : DATAS  num of items * 8 bytes for shields and armor  // 16 bytes for blades

            switch (FileName)
            {


                case "SHIELD":
                    //Debug.Log("VS/MENU/SHIELD.SYD");
                    Shield.list = new List<Shield>();
                    buffer.BaseStream.Position = 0x0178;
                    for (var i = 0; i < 16; i++)
                    {
                        //ID|ID.WEP|armor type 01(shield)|gem slots|STR|INT|AGI|always 00
                        Shield.list.Add(new Shield(buffer.ReadBytes(8)));
                    }
                    break;
                case "ARMOR":
                    //Debug.Log("VS/MENU/ARMOR.SYD");
                    Armor.list = new List<Armor>();
                    buffer.BaseStream.Position = 0x1498;
                    for (var i = 0; i < 80; i++)
                    {
                        //ID|ID.WEP|armor type[01-05]|00|STR|INT|AGI|always 00
                        Armor.list.Add(new Armor(buffer.ReadBytes(8)));
                    }
                    break;
                case "BLADE":
                    //Debug.Log("VS/MENU/BLADE.SYD");
                    Blade.list = new List<Blade>();
                    Blade.list.Add(new Blade()); // Empty Blade, index start at 1
                    buffer.BaseStream.Position = 0x2DE0;
                    for (var i = 0; i < 90; i++)
                    {
                        // Damage types : 1 = Blunt - 2 = Edged  -  3 = Piercing
                        //22   22         03          02    02  02  0000 23  00  FA  00  06  05   06       01       Holy Win
                        //ID|ID.WEP|weapon type|damage type|02|Risk|0000|STR|INT|AGI|00|Range|?|range|always 01
                        Blade.list.Add(new Blade(buffer.ReadBytes(16)));
                    }
                    break;
            }
        }
    }
}
