using System;
using System.Collections.Generic;
using UnityEngine;
using VS.Data;
using VS.Utils;

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

                List<string> monsterNames = new List<string>();
                string bytes  = "";

                int i = 0;
                while (buffer.BaseStream.Position < 0x19C8)
                {
                    long ptr = buffer.BaseStream.Position;
                    byte[] monsterHeader = buffer.ReadBytes(16); // Monster variations 4*4 bytes (model id - ?? - model var - ??)
                    string inam = L10n.Translate(buffer.ReadBytes(28));
                    //Debug.Log(string.Concat(monsterNames.Count, " : ", inam, "   H : ", BitConverter.ToString(monsterHeader), "    at : ", ptr));

                    inam = inam.Split("\r\n"[0])[0];
                    monsterNames.Add(inam);

                    Monster mon = new Monster(monsterHeader, (uint)i+1, inam);
                    Monster.list.Add(mon);

                    bytes = string.Concat(bytes, "\r\n", BitConverter.ToString(monsterHeader), "    ", inam, "   AT : ", ptr);
                    i++;
                }

                // true end of monsters names is 0x19C8 , the last entries are empty

                i = 0;
                /*
                List<int> pointers = new List<int>();
                while (i < 150)
                {
                    pointers.Add(0x19C8+buffer.ReadUInt16());
                    i++;
                }
                pointers.Add((int)buffer.BaseStream.Length);
                */

                buffer.BaseStream.Position = 0x1AF4;
                /*
                i = 0;
                while (buffer.BaseStream.Position < buffer.BaseStream.Length)
                {
                    int descriptionLength = 0;
                    if (i < 149)
                    {
                        descriptionLength = pointers[i + 1] - pointers[i];
                    } else
                    {
                        descriptionLength = (int) (buffer.BaseStream.Length - buffer.BaseStream.Position);
                    }

                    Debug.Log("descriptionLength : "+ descriptionLength);
                    string description = L10n.Translate(buffer.ReadBytes(descriptionLength));
                    Debug.Log(description);
                    Monster.list[i].desc = description;
                    i++;
                }
                */

                List<byte> bname = new List<byte>();
                i = 0;
                while (buffer.BaseStream.Position < buffer.BaseStream.Length)
                {
                    byte b = buffer.ReadByte();
                    if (b == 0xE7)
                    {
                        string inam = L10n.Translate(bname.ToArray());
                        Monster.list[i].desc = inam;
                        bname = new List<byte>();
                        i++;
                    }
                    else
                    {
                        bname.Add(b);
                    }
                }



                ToolBox.DirExNorCreate("Assets/Resources/JSON/");
                ToolBox.WriteJSON("Assets/Resources/JSON/MONSTER.json", Monster.JSONlist());
            }
        }


        public List<string>[] BuildItems(string itemNamePath, string itemDescPath)
        {
            List<string> itemNames = new List<string>();
            List<string> itemDescs = new List<string>();

            PreParse(itemNamePath);
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
                        itemNames.Add(inam);
                    }
                    bname = new List<byte>();
                }
                else
                {
                    bname.Add(b);
                }
            }
            // blade names
            // grip names
            // shield names
            // armor names
            // accessory names
            // material names
            // gem names
            // misc item names
            // grimoire names
            // key names

            Debug.Log("### ----------------------------------------------------------------------------");

            PreParse(itemDescPath);
            // list of 2 bytes, the second seems to be incremental in a certain way from 0x02 to 0x31
            // lots of things i don't know at the begining so let's jump.
            buffer.BaseStream.Position = 0x054E;
            bname = new List<byte>();
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
                        Debug.Log(string.Concat(itemDescs.Count, " : ", idesc));
                        itemDescs.Add(idesc);
                    }
                    bname = new List<byte>();
                }
                else
                {
                    bname.Add(b);
                }
            }
            // blade descs
            // grip descs
            // gem desc
            // misc item descs
            // grimoire descs
            // key descs
            // accessory descs

            return new List<string>[2] { itemNames, itemDescs };
        }
    }
}
