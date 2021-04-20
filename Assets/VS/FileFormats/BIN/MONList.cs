using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VS.Utils;

namespace VS.FileFormats.BIN
{
    public class MONList : ScriptableObject
    {
        public MON[] Monsters;

        // 0x0C0E6D18 in SLES-02755.bin

        public MONList()
        {
            Monsters = new MON[150];
        }


        public void ParseFromFile(string filepath)
        {
            FileParser fp = new FileParser();
            fp.Read(filepath);

            // the bestiary list in SMALL/MON.BIN
            if (fp.FileName == "MON" && fp.Ext == "BIN")
            {
                ParseFromBuffer(fp.buffer, fp.FileSize);
            }

            fp.Close();
        }

        public void ParseFromBuffer(BinaryReader buffer, long limit)
        {

            for (int j = 0; j < 150; j++)
            {
                MON monster = new MON();
                monster.zudToDisplay = buffer.ReadUInt16();
                monster.type = (Enums.Class.Type)buffer.ReadUInt16();
                monster.zudToKill = buffer.ReadUInt16();
                monster.unk = buffer.ReadUInt16();
                buffer.ReadBytes(8); // padding
                monster.name = L10n.CleanTranslate(buffer.ReadBytes(28));
                Monsters[j] = monster;
            }

            for (int j = 0; j < 150; j++)
            {
                Monsters[j].PtrDesc = buffer.ReadUInt16();
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
                    string inam = L10n.CleanTranslate(bname.ToArray());
                    Monsters[i].description = inam;
                    bname = new List<byte>();
                    i++;
                }
                else
                {
                    bname.Add(b);
                }
            }
        }
    }
}
