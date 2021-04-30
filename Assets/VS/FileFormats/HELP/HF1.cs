using System.IO;
using UnityEngine;
using VS.Utils;

namespace VS.FileFormats.HELP
{
    public class HF1
    {
        public string Filename;
        public void ParseFromFile(string filepath)
        {
            FileParser fp = new FileParser();
            fp.Read(filepath);

            // in SMALL/HELP**.HF1
            if (fp.Ext == "HF1")
            {
                Filename = fp.FileName+ "HF1";
                ParseFromBuffer(fp.buffer, fp.FileSize);
            }

            fp.Close();
        }

        public void ParseFromBuffer(BinaryReader buffer, long limit)
        {
            if ( buffer.BaseStream.Length > 8)
            {
                /*
                ToolBox.ColorScaleHexa(buffer, Filename, 64);
                ToolBox.GreyScaleHexa(buffer, Filename, 64);
                ToolBox.GreyScaleHexa(buffer, Filename, 128);
                */
            }
            // considering outputs of greyscales, i suspect these files to be a kind layout for .HF0
        }
    }
}