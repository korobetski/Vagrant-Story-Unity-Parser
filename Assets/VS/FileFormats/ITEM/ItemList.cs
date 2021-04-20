using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VS.Utils;

namespace VS.FileFormats.ITEM
{
    public class ItemList:ScriptableObject
    {
        public string[] Names;
        public string[] Descriptions;

        public ItemList()
        {
            // blade names          000 - 089
            // grip names           090 - 120
            // shield names         121 - 136
            // armor names          137 - 200
            // accessory names      201 - 231
            // material names       232 - 238
            // gem names            239 - 285
            // misc item names      286 - 319
            // grimoire names       320 - 369
            // key names            370 - 401

            Names = new string[511];
            Descriptions = new string[700];

            // blade descs          000 - 089
            // grip descs           090 - 120
            // gem desc             121 - 166
            // misc item descs      167 - 200
            // grimoire descs       201 - 278
            // key descs            279 - 310
            // menu strings etc...
            // accessory descs      547 - 577
        }



        public void ParseFromFile(string filepath)
        {
            FileParser fp = new FileParser();
            fp.Read(filepath);

            if ((fp.FileName == "ITEMNAME" || fp.FileName == "ITEMHELP") && fp.Ext == "BIN")
            {
                switch (fp.FileName)
                {
                    case "ITEMNAME":
                        ParseNamesFromBuffer(fp.buffer, fp.FileSize);
                        break;
                    case "ITEMHELP":
                        ParseDescsFromBuffer(fp.buffer, fp.FileSize);
                        break;
                }
                
            }

            fp.Close();
        }

        public void ParseNamesFromBuffer(BinaryReader buffer, long limit)
        {
            buffer.BaseStream.Position = 0x18;
            List<byte> bname = new List<byte>();
            uint numNames = (uint)((buffer.BaseStream.Length - 0x18) / 24);
            for (uint i = 0; i < numNames; i++)
            {
                Names[i] = L10n.CleanTranslate(buffer.ReadBytes(24));
            }
        }

        public void ParseDescsFromBuffer(BinaryReader buffer, long limit)
        {
            // pointers are wrong in SLES
            uint i = 0;
            buffer.BaseStream.Position = 0x054E;
            List<byte> bname = new List<byte>();
            while (buffer.BaseStream.Position < buffer.BaseStream.Length)
            {
                byte b = buffer.ReadByte();
                if (b == 0xE7)
                {
                    bname.Add(b);
                    string str = L10n.CleanTranslate(bname.ToArray());
                    if (str != "")
                    {
                        Descriptions[i] = str;
                        i++;
                        bname = new List<byte>();
                    }
                }
                else
                {
                    bname.Add(b);
                }
            }
        }

        public string GetName(int id)
        {
            id -= 1;
            if (id >= 0 && id < Names.Length)
            {
                return Names[id];
            } else
            {
                return "";
            }
        }

        public string GetDescription(int id)
        {
            id -= 1;
            if (id >= 0 && id < Descriptions.Length)
            {
                if (id >= 0 && id < 121)
                {
                    // blade and grip, description id is the same
                    return Descriptions[id];
                } else if (id >= 239 && id < 402)
                {
                    // gem,  mics items, grimoire and keys
                    return Descriptions[id - 118];
                }
                else if (id >= 201 && id < 232)
                {
                    // accessories
                    return Descriptions[id + 346];
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
        }
    }
}
