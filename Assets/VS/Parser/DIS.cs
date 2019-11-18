using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VS.Utils;

namespace VS.Parser
{


    // DIS seems to be pixel text and fonts
    public class DIS : FileParser
    {
        public void Parse(string filePath)
        {
            if (!filePath.EndsWith(".DIS"))
            {
                return;
            }

            PreParse(filePath);


            int width = 256;
            int height = (int)(FileSize - 512 - 20) / width;

            byte[] header = buffer.ReadBytes(20);
            Debug.Log(string.Concat(FileName, "   header : ", BitConverter.ToString(header)));

            Color[] col = new Color[256];
            for (int i = 0; i < 256; ++i)
            {
                col[i] = ToolBox.BitColorConverter(buffer.ReadUInt16());
            }

            List<Color> cluts = new List<Color>();
            for (uint x = 0; x < height; x++)
            {
                List<Color> cl2 = new List<Color>();
                for (uint y = 0; y < width; y++)
                {
                    cl2.Add(col[buffer.ReadByte()]);
                }
                cl2.Reverse();
                cluts.AddRange(cl2);
            }
            cluts.Reverse();
            Texture2D tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
            tex.SetPixels(cluts.ToArray());
            tex.Apply();

            byte[] bytes = tex.EncodeToPNG();
            ToolBox.DirExNorCreate(Application.dataPath + "/../Assets/Resources/Textures/DIS/");
            File.WriteAllBytes(Application.dataPath + "/../Assets/Resources/Textures/DIS/" + FileName + ".png", bytes);
        }
    }
}
