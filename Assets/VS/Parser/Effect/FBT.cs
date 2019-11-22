using System.Collections.Generic;
using UnityEngine;

namespace VS.Parser.Effect
{
    public class FBT : FileParser
    {
        private Color32[,] _pallets;
        public Texture2D texture;

        public FBT(string path, Color32[,] pallets)
        {
            _pallets = pallets;
            Parse(path);
        }

        public void Parse(string filePath)
        {
            if (!filePath.EndsWith(".FBT"))
            {
                return;
            }

            PreParse(filePath);

            int width = 128;
            int height = 128;
            int size = width * height;
            int pad = Mathf.FloorToInt(FileSize / size);
            height *= pad;
            size = width * height;

            List<Color> cluts = new List<Color>();
            for (uint x = 0; x < height; x++)
            {
                List<Color> cl2 = new List<Color>();
                for (uint y = 0; y < width; y++)
                {
                    cl2.Add(_pallets[0, buffer.ReadByte()]);
                }
                cl2.Reverse();
                cluts.AddRange(cl2);
            }
            cluts.Reverse();

            texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
            texture.SetPixels(cluts.ToArray());
            texture.Apply();
        }
    }
}
