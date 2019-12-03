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


            int width = 128;
            int height = (int)(FileSize - 512 - 20) / width;

            byte[] header;
            header = buffer.ReadBytes(20);
            //Debug.Log(string.Concat(FileName, "   header : ", BitConverter.ToString(header)));



            if (FileName.StartsWith("ENDSCR"))
            {
                width = 320;
                height = (int)(FileSize - 16) / width / 2;
                List<Color> gim = new List<Color>();

                for (uint x = 0; x < height; x++)
                {
                    List<Color> cl2 = new List<Color>();
                    for (uint y = 0; y < width; y++)
                    {
                        cl2.Add(ToolBox.BitColorConverter(buffer.ReadUInt16()));
                    }
                    cl2.Reverse();
                    gim.AddRange(cl2);
                }
                gim.Reverse();
                Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
                texture.SetPixels(gim.ToArray());
                texture.Apply();
                byte[] bytes = texture.EncodeToPNG();
                ToolBox.DirExNorCreate(Application.dataPath + "/../Assets/Resources/Textures/DIS/");
                File.WriteAllBytes(Application.dataPath + "/../Assets/Resources/Textures/DIS/" + FileName + ".png", bytes);

            }
            else
            {
                Color[][] pallets16 = new Color[16][];
                for (int i = 0; i < 16; i++)
                {
                    Color[] col = new Color[16];
                    for (int j = 0; j < 16; j++)
                    {
                        col[j] = ToolBox.BitColorConverter(buffer.ReadUInt16());
                    }
                    pallets16[i] = col;
                }

                long pixPtr = buffer.BaseStream.Position;
                for (int j = 0; j < pallets16.Length; j++)
                {
                    buffer.BaseStream.Position = pixPtr;
                    List<Color> gim = new List<Color>();

                    for (uint x = 0; x < height; x++)
                    {
                        List<Color> cl2 = new List<Color>();
                        for (uint y = 0; y < width; y++)
                        {
                            byte id = buffer.ReadByte();
                            byte l = (byte)Mathf.RoundToInt(id / 16);
                            byte r = (byte)(id % 16);
                            cl2.Add(pallets16[j][r]);
                            cl2.Add(pallets16[j][l]);
                        }
                        cl2.Reverse();
                        gim.AddRange(cl2);
                    }
                    gim.Reverse();
                    Texture2D texture = new Texture2D(width * 2, height, TextureFormat.ARGB32, false);
                    texture.SetPixels(gim.ToArray());
                    texture.Apply();
                    byte[] bytes = texture.EncodeToPNG();
                    ToolBox.DirExNorCreate(Application.dataPath + "/../Assets/Resources/Textures/DIS/");
                    File.WriteAllBytes(Application.dataPath + "/../Assets/Resources/Textures/DIS/" + FileName + "_4b_" + j + ".png", bytes);
                }
            }
        }
    }
}
