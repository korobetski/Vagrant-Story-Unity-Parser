using System.IO;
using UnityEngine;
using VS.Utils;

namespace VS.FileFormats.SHP
{
    public class ESQ:ScriptableObject
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
            ToolBox.GreyScaleHexa(buffer, Filename, 128);
        }
    }
}
