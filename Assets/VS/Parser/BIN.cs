/*
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VS.Data;
using VS.Serializable;
using VS.Utils;
using L10n = VS.Utils.L10n;

namespace VS.Parser
{
    public class BIN : FileParser
    {


        public BIN()
        {
        }


        public void Explore(string filePath)
        {
            PreParse(filePath);

            List<byte> bname = new List<byte>();
            while (buffer.BaseStream.Position < buffer.BaseStream.Length)
            {
                byte b = buffer.ReadByte();
                if (b == 0xE7)
                {
                    string inam = L10n.Translate(bname.ToArray());
                    Debug.Log(string.Concat(inam) + " at : "+ buffer.BaseStream.Position);




                    bname = new List<byte>();
                }
                else
                {
                    bname.Add(b);
                }
            }

        }

        public void Parse(string filePath)
        {
            if (!filePath.EndsWith(".BIN"))
            {
                return;
            }

            PreParse(filePath);

            if (FileName == "MON")
            {
                MONList so = ScriptableObject.CreateInstance<MONList>();

                for (int j = 0; j < 150; j++)
                {
                    Serializable.MON monster = new Serializable.MON();
                    monster.header = new ushort[] { buffer.ReadUInt16(), buffer.ReadUInt16(), buffer.ReadUInt16(), buffer.ReadUInt16() };
                    // ZUD Id are maybe incremented by looping ZNDs
                    // header[0] = ZUDID to display in the bestiary
                    // header[1] = monster type (human, beast, etc...)
                    // header[2] = ZUDID to beat before appearing in the bestiary
                    // header[4] = number of monster to kill before appearing in the bestiary ?
                    buffer.ReadBytes(8); // padding
                    string inam = L10n.Translate(buffer.ReadBytes(28));
                    string[] subs = inam.Split('|');
                    monster.name = subs[0];
                    so.Monsters[j] = monster;
                }

                for (int j = 0; j < 150; j++)
                {
                    so.Monsters[j].PtrDesc = buffer.ReadUInt16();
                }

                //buffer.BaseStream.Position = 0x1AF4;

                List<byte> bname = new List<byte>();
                int i = 0;
                while (buffer.BaseStream.Position < buffer.BaseStream.Length)
                {
                    byte b = buffer.ReadByte();
                    //if (i == 150) break;
                    if (b == 0xE7)
                    {
                        string inam = L10n.Translate(bname.ToArray());
                        so.Monsters[i].description = inam;
                        bname = new List<byte>();
                        i++;
                    }
                    else
                    {
                        bname.Add(b);
                    }
                }

                ToolBox.DirExNorCreate("Assets/");
                ToolBox.DirExNorCreate("Assets/Resources/");
                ToolBox.DirExNorCreate("Assets/Resources/Serialized/");
                ToolBox.DirExNorCreate("Assets/Resources/Serialized/Datas/");
                AssetDatabase.DeleteAsset("Assets/Resources/Serialized/Datas/MON.BIN.yaml.asset");
                AssetDatabase.CreateAsset(so, "Assets/Resources/Serialized/Datas/MON.BIN.yaml.asset");
                AssetDatabase.SaveAssets();
            }
            else if (FileName == "ITEMNAME")
            {
                ItemList so = ScriptableObject.CreateInstance<ItemList>();

                List<string> itemNames = new List<string>();
                buffer.BaseStream.Position = 0x18;
                List<byte> bname = new List<byte>();
                while (buffer.BaseStream.Position < buffer.BaseStream.Length)
                {
                    byte b = buffer.ReadByte();
                    if (b == 0xE7)
                    {
                        string inam = L10n.Translate(bname.ToArray());
                        inam = inam.Replace("0", "");
                        inam = inam.Replace("\r", "");
                        inam = inam.Replace("\n", "");
                        if (inam != "untitled")
                        {
                            Debug.Log(string.Concat(itemNames.Count, " : ", inam));
                            so.Names[itemNames.Count] = inam;
                            itemNames.Add(inam);
                        }
                        bname = new List<byte>();
                    }
                    else
                    {
                        bname.Add(b);
                    }
                }



                ToolBox.DirExNorCreate("Assets/");
                ToolBox.DirExNorCreate("Assets/Resources/");
                ToolBox.DirExNorCreate("Assets/Resources/Serialized/");
                ToolBox.DirExNorCreate("Assets/Resources/Serialized/Datas/");
                AssetDatabase.DeleteAsset("Assets/Resources/Serialized/Datas/ITEM.BIN.yaml.asset");
                AssetDatabase.CreateAsset(so, "Assets/Resources/Serialized/Datas/ITEM.BIN.yaml.asset");
                AssetDatabase.SaveAssets();

                Parse(FilePath.Replace("ITEMNAME", "ITEMHELP"));
            }
            else if (FileName == "ITEMHELP")
            {
                ItemList so = ScriptableObject.CreateInstance<ItemList>();
                ItemList so2 = Resources.Load("Serialized/ITEM.BIN.yaml") as ItemList;
                so.Names = so2.Names;
                List<string> itemDescs = new List<string>();
                List<ushort> descsPtr = new List<ushort>();
                uint numDescs = 0x054E / 2;
                List<byte> bname = new List<byte>();
                for (int i = 0; i< numDescs; i++)
                {
                    ushort ptr = buffer.ReadUInt16();
                    Debug.Log(string.Concat(descsPtr.Count, " : ", ptr));
                    descsPtr.Add(ptr);
                }
                buffer.BaseStream.Position = 0x054E;

                while (buffer.BaseStream.Position < buffer.BaseStream.Length)
                {
                    byte b = buffer.ReadByte();
                    if (b == 0xE7)
                    {
                        string idesc = L10n.Translate(bname.ToArray());
                        idesc = idesc.Replace("\r", "");
                        idesc = idesc.Replace("\n", "");
                        if (idesc != "")
                        {
                            //Debug.Log(string.Concat(itemDescs.Count, " : ", idesc));
                            so.Descriptions[itemDescs.Count] = idesc;
                            itemDescs.Add(idesc);
                        }
                        bname = new List<byte>();
                    }
                    else
                    {
                        bname.Add(b);
                    }
                }
                AssetDatabase.DeleteAsset("Assets/Resources/Serialized/Datas/ITEM.BIN.yaml.asset");
                AssetDatabase.CreateAsset(so, "Assets/Resources/Serialized/Datas/ITEM.BIN.yaml.asset");
                AssetDatabase.SaveAssets();
            }
        }

    }
}
*/