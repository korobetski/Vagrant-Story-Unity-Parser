using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VS.Utils;

namespace VS.FileFormats.BIN
{
    // Program file
    public class BIN : ScriptableObject
    {
        private string Filename;

        public void ParseFromFile(string filepath)
        {
            FileParser fp = new FileParser();
            fp.Read(filepath);

            // ***.BIN
            if (fp.Ext == "BIN")
            {
                Filename = fp.FileName;
                ParseFromBuffer(fp.buffer, fp.FileSize);
            }

            fp.Close();
        }

        public void ParseFromBuffer(BinaryReader buffer, long limit)
        {
            // MENU/MENU12.BIN
            // contains strings for workshop crafting
            buffer.BaseStream.Position = 0;
            string[] subs = L10n.Translate(buffer.ReadBytes((int)buffer.BaseStream.Length- 0)).Split('|');
            foreach(string s in subs)
            {

                Debug.Log(s);
            }
            buffer.BaseStream.Position = 0;
            GreyScaleHexa(buffer, 128);
        }

        public void GreyScaleHexa(BinaryReader buffer, int _w = 128)
        {
            int width = _w;
            int height = (int)buffer.BaseStream.Length / width;
            List<Color> cluts = new List<Color>();
            for (uint x = 0; x < height; x++)
            {
                List<Color> cl2 = new List<Color>();
                for (uint y = 0; y < width; y++)
                {
                    byte b = buffer.ReadByte();
                    cl2.Add(new Color32(b, b, b, 255));
                }
                cl2.Reverse();
                cluts.AddRange(cl2);
            }
            cluts.Reverse();
            Texture2D tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
            tex.SetPixels(cluts.ToArray());
            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            ToolBox.DirExNorCreate(Application.dataPath + "/../Assets/Resources/Textures/Ex/");
            File.WriteAllBytes(Application.dataPath + "/../Assets/Resources/Textures/Ex/" + Filename + ".png", bytes);
        }
    }
}