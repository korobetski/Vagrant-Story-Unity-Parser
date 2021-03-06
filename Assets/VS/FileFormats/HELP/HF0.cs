﻿using System.IO;
using UnityEngine;
using VS.Utils;

namespace VS.FileFormats.HELP
{

    // HF -> Ingame Help 
    public class HF0
    {
        public string Filename;

        public void ParseFromFile(string filepath)
        {
            FileParser fp = new FileParser();
            fp.Read(filepath);

            // in SMALL/HELP**.HF0
            if (fp.Ext == "HF0")
            {
                Filename = fp.FileName+ "HF0";
                ParseFromBuffer(fp.buffer, fp.FileSize);
            }

            fp.Close();
        }

        public void ParseFromBuffer(BinaryReader buffer, long limit)
        {
            ToolBox.ColorScaleHexa(buffer, Filename, 64);
            ToolBox.GreyScaleHexa(buffer, Filename, 16);
            ToolBox.GreyScaleHexa(buffer, Filename, 32);
            ToolBox.GreyScaleHexa(buffer, Filename, 64);
            ToolBox.GreyScaleHexa(buffer, Filename, 128);
            ToolBox.GreyScaleHexa(buffer, Filename, 192);

            buffer.BaseStream.Position = 0;

            string[] subs = L10n.Translate(buffer.ReadBytes((int)buffer.BaseStream.Length - 0)).Split('|');
            foreach (string s in subs)
            {

                Debug.Log(s);
            }
        }
    }
}
