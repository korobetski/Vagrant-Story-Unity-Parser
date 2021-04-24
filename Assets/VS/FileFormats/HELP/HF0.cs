using System.IO;
using UnityEngine;
using VS.Utils;

namespace VS.FileFormats.HELP
{


    // HF -> Ingame Help 
    public class HF0
    {

        public void ParseFromFile(string filepath)
        {
            FileParser fp = new FileParser();
            fp.Read(filepath);

            // in SMALL/HELP**.HF0
            if (fp.Ext == "HF0")
            {
                ParseFromBuffer(fp.buffer, fp.FileSize);
            }

            fp.Close();
        }

        public void ParseFromBuffer(BinaryReader buffer, long limit)
        {

            string[] subs = L10n.Translate(buffer.ReadBytes((int)buffer.BaseStream.Length - 0)).Split('|');
            foreach (string s in subs)
            {

                Debug.Log(s);
            }
        }
    }
}
