using System.IO;

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

        }
    }
}
