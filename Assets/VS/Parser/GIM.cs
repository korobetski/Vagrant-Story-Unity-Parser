using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VS.Utils;

namespace VS.Parser
{

    public class GIM : FileParser
    {
        public Texture2D texture;

        public GIM(string path)
        {
            UseDebug = true;
            Parse(path);
        }


        public void Parse(string filePath)
        {
            if (!filePath.EndsWith(".GIM"))
            {
                return;
            }
            PreParse(filePath);

            if (FileName == "BLACK")
            {

                return;
            }


            ushort timH = buffer.ReadUInt16();
            buffer.ReadUInt16();
            int ptr = buffer.ReadInt16();
            int ptr2 = buffer.ReadInt16();
            Debug.Log(FileName + "  " + ptr);

            int numCol = 256;



            if (ptr == 0 && ptr2 == 0)
            {
                buffer.BaseStream.Position = 0xA8;
                numCol = 16;
            }
            else if (ptr == 0 && ptr2 == 1)
            {
                if (FileName == "DADDY")
                {
                    buffer.BaseStream.Position = 0x100;
                }
                else if (FileName == "JO")
                {
                    buffer.BaseStream.Position = 0x170;
                }
                else if (FileName == "VKP_1")
                {
                    buffer.BaseStream.Position = 0x1A0;
                }
                else
                {
                    buffer.BaseStream.Position = 0x150;
                }
                numCol = 32;
            }
            else if (ptr == 0 && ptr2 == 2)
            {
                if (FileName == "EPILOGUE")
                {
                    buffer.BaseStream.Position = 0x1F8;
                }
                else if (FileName == "VKP_0")
                {
                    buffer.BaseStream.Position = 0x270;
                }
                else if (FileName == "VKP_2")
                {
                    buffer.BaseStream.Position = 0x298;
                }
                else
                {
                    buffer.BaseStream.Position = 0x118;
                }
                numCol = 48;
            }
            else if (ptr == 1 && ptr2 == 0)
            {
                buffer.BaseStream.Position = 0x128;
            }
            else if (ptr == 1 && ptr2 == 1)
            {
                if (FileName == "HEADQUAT" || FileName == "ENKEI2")
                {
                    buffer.BaseStream.Position = 0x150;
                } else
                {
                    buffer.BaseStream.Position = 0x1D0;
                }
            }
            else if (ptr == 1 && ptr2 == 2)
            {
                if (FileName == "ENKEI")
                {
                    buffer.BaseStream.Position = 0x1F8;
                }
                else
                {
                    buffer.BaseStream.Position = 0x1F8;
                }
            }
            else if (ptr == 2 && ptr2 == 1)
            {
                buffer.BaseStream.Position = 0x220;
                numCol = 512;
            }
            else if (ptr == 2 && ptr2 == 2)
            {
                buffer.BaseStream.Position = 0x280;
                numCol = 512;
            }


            Color[] col = new Color[numCol];
            for (int i = 0; i < numCol; ++i)
            {
                col[i] = ToolBox.BitColorConverter(buffer.ReadUInt16());
            }

            int width = 128;
            int height = 15;

            int numBlocks = (int)(FileSize - buffer.BaseStream.Position) / (width * height);
            Debug.Log(FileName + "  numBlocks : " + numBlocks);
            List<Color> gim = new List<Color>();
            for (int i = 0; i < numBlocks; i++) {
                List<Color> block = new List<Color>();
                for (uint x = 0; x < height; x++)
                {
                    List<Color> cl2 = new List<Color>();
                    for (uint y = 0; y < width; y++)
                    {
                        byte id = buffer.ReadByte();
                        if (id < col.Length)
                        {
                            cl2.Add(col[id]);
                        } else
                        {
                            cl2.Add(new Color32(id, id, id, 255));
                        }
                    }
                    cl2.Reverse();
                    block.AddRange(cl2);
                }
                gim.AddRange(block);
            }
            gim.Reverse();
            texture = new Texture2D(width, height*numBlocks, TextureFormat.ARGB32, false);
            texture.SetPixels(gim.ToArray());
            texture.Apply();

            byte[] bytes = texture.EncodeToPNG();
            ToolBox.DirExNorCreate(Application.dataPath + "/../Assets/Resources/Textures/GIM/");
            File.WriteAllBytes(Application.dataPath + "/../Assets/Resources/Textures/GIM/" + FileName + ".png", bytes);
            
        }

    }
}
