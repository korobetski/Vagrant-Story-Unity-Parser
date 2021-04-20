using System.IO;
using UnityEngine;

namespace VS.FileFormats.HELP
{
    public class HF1
    {


        // HF -> Ingame Help 
        public class HF0
        {

            public void ParseFromFile(string filepath)
            {
                FileParser fp = new FileParser();
                fp.Read(filepath);

                // in SMALL/HELP**.HF1
                if (fp.Ext == "HF1")
                {
                    ParseFromBuffer(fp.buffer, fp.FileSize);
                }

                fp.Close();
            }

            public void ParseFromBuffer(BinaryReader buffer, long limit)
            {

            }
        }
    }
}