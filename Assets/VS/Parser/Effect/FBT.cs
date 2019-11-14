using System.IO;
using UnityEngine;
using VS.Utils;

namespace VS.Parser.Effect
{
    public class FBT:FileParser
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

            Color[] clut = new Color[size];
            for (int i = 0; i < size; ++i) clut[i] = _pallets[0, buffer.ReadByte()];

            texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
            texture.SetPixels(clut);
            texture.Apply();

            //Effects
            /*
            */

        }
    }
}
