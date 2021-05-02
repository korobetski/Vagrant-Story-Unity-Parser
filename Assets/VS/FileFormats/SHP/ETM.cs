using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VS.FileFormats.TIM;
using VS.Utils;

namespace VS.FileFormats.SHP
{
    
    public class ETM:ScriptableObject
    {

        public string Filename;

        public byte[] header;
        public byte[] clut;

        public Palette palette;

        private int width = 128;
        private int height;

        public void ParseFromFile(string filepath)
        {
            FileParser fp = new FileParser();
            fp.Read(filepath);

            if (fp.Ext == "ETM")
            {
                Filename = fp.FileName;
                ParseFromBuffer(fp.buffer, fp.FileSize);
            }

            fp.Close();
        }

        public void ParseFromBuffer(BinaryReader buffer, long limit)
        {
            header = buffer.ReadBytes(4);

            height = (int)((buffer.BaseStream.Length-4) / width);
            List<byte> cluts = new List<byte>();
            for (uint x = 0; x < height; x++)
            {
                List<byte> cl2 = new List<byte>();
                for (uint y = 0; y < width; y++)
                {
                    byte b = buffer.ReadByte();
                    cl2.Add(b);
                }
                cl2.Reverse();
                cluts.AddRange(cl2);
            }
            cluts.Reverse();
            clut = cluts.ToArray();
        }

        internal void BuildTextureWithSHPTIM(TIM.TIM shptim, string name)
        {
            Texture2D tex = new Texture2D(1, 1);
            List<Color> colors = new List<Color>();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte c = clut[(int)((y * width) + x)];
                    if (c < shptim.numColors)
                    {
                        colors.Add(shptim.palettes[0].colors[c]);
                    } else
                    {
                        colors.Add(shptim.palettes[1].colors[c-shptim.numColors]);
                    }
                    
                }
            }
            tex = new Texture2D((int)width, (int)height, TextureFormat.ARGB32, false);
            tex.SetPixels(colors.ToArray());
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.Apply();

            byte[] bytes = tex.EncodeToPNG();
            ToolBox.DirExNorCreate("Assets/Resources/Textures/ETM/");
            File.WriteAllBytes("Assets/Resources/Textures/ETM/" + Filename +"_"+ name+".png", bytes);
        }

    }
}
