using System.IO;
using UnityEngine;
using VS.Utils;

namespace VS.FileFormats.SHP
{
    public class ESQ
    {

        private string Filename;

        public void ParseFromFile(string filepath)
        {
            FileParser fp = new FileParser();
            fp.Read(filepath);

            if (fp.Ext == "ESQ")
            {
                Filename = fp.FileName;
                ParseFromBuffer(fp.buffer, fp.FileSize);
            }

            fp.Close();
        }

        public void ParseFromBuffer(BinaryReader buffer, long limit)
        {
            ToolBox.ColorScaleHexa(buffer, Filename, 32);
            ToolBox.GreyScaleHexa(buffer, Filename, 32);
            ToolBox.GreyScaleHexa(buffer, Filename, 64);
            ToolBox.GreyScaleHexa(buffer, Filename, 128);
            ToolBox.GreyScaleHexa(buffer, Filename, 192);
            ToolBox.GreyScaleHexa(buffer, Filename, 256);
            ToolBox.GreyScaleHexa(buffer, Filename, 320);
        }
    }
}
