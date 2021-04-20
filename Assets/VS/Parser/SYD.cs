/*
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VS.Utils;

// MENU/ARMOR.SYD
// MENU/BLADE.SYD
// MENU/SHIELD.SYD


namespace VS.Parser
{
    public class SYD : FileParser
    {
        public void Parse(string filePath)
        {
            PreParse(filePath);

            KeyValuePair<int, int> bladeIdx = new KeyValuePair<int, int>(0, 0);
            KeyValuePair<int, int> gripIdx = new KeyValuePair<int, int>(90, 90);
            KeyValuePair<int, int> shieldIdx = new KeyValuePair<int, int>(121, 0); // shields have no descs  ?
            KeyValuePair<int, int> armorIdx = new KeyValuePair<int, int>(121, 0); // armors have no descs  ?


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
                    var so1 = ScriptableObject.CreateInstance<Serializable.ShieldWorkshop>();
                    buffer.BaseStream.Position = 0x0178;
                    for (var i = 0; i < 16; i++)
                    {
                        //ID|ID.WEP|armor type 01(shield)|gem slots|STR|INT|AGI|always 00
                        so1.Shields[i] = new Serializable.Shield();
                        so1.Shields[i].SetDatasFromSYD(buffer.ReadBytes(8));
                    }

                    ToolBox.DirExNorCreate("Assets/");
                    ToolBox.DirExNorCreate("Assets/Resources/");
                    ToolBox.DirExNorCreate("Assets/Resources/Serialized/");
                    ToolBox.DirExNorCreate("Assets/Resources/Serialized/Datas/");
                    AssetDatabase.DeleteAsset("Assets/Resources/Serialized/Datas/SHIELD.SYD.yaml.asset");
                    AssetDatabase.CreateAsset(so1, "Assets/Resources/Serialized/Datas/SHIELD.SYD.yaml.asset");
                    AssetDatabase.SaveAssets();

                    break;
                case "ARMOR":
                    var so2 = ScriptableObject.CreateInstance<Serializable.ArmorWorkshop>();
                    buffer.BaseStream.Position = 0x1498;
                    for (var i = 0; i < 80; i++)
                    {
                        //ID|ID.WEP|armor type[01-05]|00|STR|INT|AGI|always 00
                        so2.Armors[i] = new Serializable.Armor();
                        so2.Armors[i].SetDatasFromSYD(buffer.ReadBytes(8));
                    }

                    ToolBox.DirExNorCreate("Assets/");
                    ToolBox.DirExNorCreate("Assets/Resources/");
                    ToolBox.DirExNorCreate("Assets/Resources/Serialized/");
                    ToolBox.DirExNorCreate("Assets/Resources/Serialized/Datas/");
                    AssetDatabase.DeleteAsset("Assets/Resources/Serialized/Datas/ARMOR.SYD.yaml.asset");
                    AssetDatabase.CreateAsset(so2, "Assets/Resources/Serialized/Datas/ARMOR.SYD.yaml.asset");
                    AssetDatabase.SaveAssets();

                    break;
                case "BLADE":
                    var so3 = ScriptableObject.CreateInstance<Serializable.BladeWorkshop>();
                    buffer.BaseStream.Position = 0x2DE0;
                    for (var i = 0; i < 90; i++)
                    {
                        // Damage types : 1 = Blunt - 2 = Edged  -  3 = Piercing
                        //22   22         03          02    02  02  0000 23  00  FA  00  06  05   06       01       Holy Win
                        //ID|ID.WEP|weapon type|damage type|02|Risk|0000|STR|INT|AGI|00|Range|?|range|always 01
                        so3.Blades[i] = new Serializable.Blade();
                        so3.Blades[i].SetDatasFromSYD(buffer.ReadBytes(16));
                    }

                    ToolBox.DirExNorCreate("Assets/");
                    ToolBox.DirExNorCreate("Assets/Resources/");
                    ToolBox.DirExNorCreate("Assets/Resources/Serialized/");
                    ToolBox.DirExNorCreate("Assets/Resources/Serialized/Datas/");
                    AssetDatabase.DeleteAsset("Assets/Resources/Serialized/Datas/BLADE.SYD.yaml.asset");
                    AssetDatabase.CreateAsset(so3, "Assets/Resources/Serialized/Datas/BLADE.SYD.yaml.asset");
                    AssetDatabase.SaveAssets();

                    break;
            }
        }
    }
}
*/