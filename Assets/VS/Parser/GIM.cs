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
            int ptr = buffer.ReadUInt16();
            int pal = buffer.ReadInt16(); // num of 256 col pallets (0 means no pallet)
            int pal2 = buffer.ReadInt16(); // num of 16 col pallets (0 means 1 if there is no other pallet)
            pal2++;
            Debug.Log(FileName + "  timH : " + timH + "  ptr : " + ptr + "   pal : " + pal + "   pal2 : " + pal2);

            buffer.BaseStream.Position = 0xA8;
            switch (FileName)
            {
                case "019XXX01":
                    buffer.BaseStream.Position = 0xE8;
                    break;
                case "1142":
                    buffer.BaseStream.Position = 0xA8;
                    break;
                case "DADDY":
                    buffer.BaseStream.Position = 0x100;
                    break;
                case "DEMO_001":
                    buffer.BaseStream.Position = 0x150;
                    break;
                case "DEMO_002":
                    buffer.BaseStream.Position = 0x150;
                    break;
                case "DEMO_003":
                    buffer.BaseStream.Position = 0x150;
                    break;
                case "DEMO_004":
                    buffer.BaseStream.Position = 0x150;
                    break;
                case "DEMO_005":
                    buffer.BaseStream.Position = 0x150;
                    break;
                case "DEMO_006":
                    buffer.BaseStream.Position = 0x150;
                    break;
                case "DEMO_007":
                    buffer.BaseStream.Position = 0x150;
                    break;
                case "DEMO_008":
                    buffer.BaseStream.Position = 0x150;
                    break;
                case "DEMOENK":
                    buffer.BaseStream.Position = 0x178;
                    break;
                case "DEMOFIN":
                    buffer.BaseStream.Position = 0x1F8;
                    break;
                case "ENKEI":
                    buffer.BaseStream.Position = 0x178;
                    break;
                case "ENKEI2":
                    buffer.BaseStream.Position = 0xF0;
                    break;
                case "EPI_INF":
                    buffer.BaseStream.Position = 0x118;
                    break;
                case "EPILOGUE":
                    buffer.BaseStream.Position = 0x1F8;
                    break;
                case "EV_MASK":
                    buffer.BaseStream.Position = 0xA8;
                    break;
                case "GAMEOVER":
                    buffer.BaseStream.Position = 0xE8;
                    break;
                case "HEADQUAT":
                    buffer.BaseStream.Position = 0xF0;
                    break;
                case "JO":
                    buffer.BaseStream.Position = 0x178;
                    break;
                case "JYO":
                    buffer.BaseStream.Position = 0x1A0;
                    break;
                case "LOGO_G22":
                    buffer.BaseStream.Position = 0xE8;
                    break;
                case "MAIN03":
                    buffer.BaseStream.Position = 0xE8;
                    break;
                case "N_LEA":
                    buffer.BaseStream.Position = 0x170;
                    break;
                case "STAFF001":
                    buffer.BaseStream.Position = 0xE8;
                    break;
                case "STAFF002":
                    buffer.BaseStream.Position = 0xE8;
                    break;
                case "STAFF003":
                    buffer.BaseStream.Position = 0xE8;
                    break;
                case "STAFF004":
                    buffer.BaseStream.Position = 0xE8;
                    break;
                case "STAFF005":
                    buffer.BaseStream.Position = 0xE8;
                    break;
                case "STAFF006":
                    buffer.BaseStream.Position = 0xE8;
                    break;
                case "STAFF007":
                    buffer.BaseStream.Position = 0xE8;
                    break;
                case "STAFF008":
                    buffer.BaseStream.Position = 0xE8;
                    break;
                case "STAFF009":
                    buffer.BaseStream.Position = 0xE8;
                    break;
                case "STAFF010":
                    buffer.BaseStream.Position = 0xE8;
                    break;
                case "STAFF011":
                    buffer.BaseStream.Position = 0xE8;
                    break;
                case "STAFF012":
                    buffer.BaseStream.Position = 0xE8;
                    break;
                case "STAFF013":
                    buffer.BaseStream.Position = 0xE8;
                    break;
                case "SUNRISE":
                    buffer.BaseStream.Position = 0xE8;
                    break;
                case "VKP_0":
                    buffer.BaseStream.Position = 0x270;
                    break;
                case "VKP_1":
                    buffer.BaseStream.Position = 0x1A0;
                    break;
                case "VKP_2":
                    buffer.BaseStream.Position = 0x298;
                    break;
            }

            // First 16 col pallets
            Color[][] pallets16 = new Color[pal2][];
            for (int i = 0; i < pal2; i++)
            {
                Color[] col = new Color[16];
                for (int j = 0; j < 16; j++)
                {
                    col[j] = ToolBox.BitColorConverter(buffer.ReadUInt16());
                }
                pallets16[i] = col;
            }
            buffer.ReadBytes(32); // padding between 16 col & 256 col pallets
            if (pal > 1)
            {
                buffer.ReadBytes(32);
            }
            // Second 256 col pallets
            Color[][] pallets256 = new Color[pal][];
            for (int i = 0; i < pal; i++)
            {
                Color[] col = new Color[256];
                for (int j = 0; j < 256; j++)
                {
                    col[j] = ToolBox.BitColorConverter(buffer.ReadUInt16());
                }
                pallets256[i] = col;
            }


            int width = 128;
            int height = 15;
            int numBlocks = (int)(FileSize - buffer.BaseStream.Position) / (width * height);
            long pixPtr = buffer.BaseStream.Position;
            if (pal2 > 1 || pal == 0)
            {
                for (int j = 0; j < pallets16.Length; j++)
                {
                    buffer.BaseStream.Position = pixPtr;
                    List<Color> gim = new List<Color>();
                    for (int i = 0; i < numBlocks; i++)
                    {
                        List<Color> block = new List<Color>();
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
                            block.AddRange(cl2);
                        }
                        gim.AddRange(block);
                    }
                    gim.Reverse();
                    texture = new Texture2D(width * 2, height * numBlocks, TextureFormat.ARGB32, false);
                    texture.SetPixels(gim.ToArray());
                    texture.Apply();
                    byte[] bytes = texture.EncodeToPNG();
                    ToolBox.DirExNorCreate(Application.dataPath + "/../Assets/Resources/Textures/GIM/");
                    File.WriteAllBytes(Application.dataPath + "/../Assets/Resources/Textures/GIM/" + FileName + "_4b_" + j + ".png", bytes);
                }
            }

            for (int j = 0; j < pallets256.Length; j++)
            {
                buffer.BaseStream.Position = pixPtr;
                List<Color> gim = new List<Color>();
                for (int i = 0; i < numBlocks; i++)
                {
                    List<Color> block = new List<Color>();
                    for (uint x = 0; x < height; x++)
                    {
                        List<Color> cl2 = new List<Color>();
                        for (uint y = 0; y < width; y++)
                        {
                            byte id = buffer.ReadByte();
                            cl2.Add(pallets256[j][id]);
                        }
                        cl2.Reverse();
                        block.AddRange(cl2);
                    }
                    gim.AddRange(block);
                }
                gim.Reverse();


                texture = new Texture2D(width, height * numBlocks, TextureFormat.ARGB32, false);
                texture.SetPixels(gim.ToArray());
                texture.Apply();

                byte[] bytes = texture.EncodeToPNG();
                ToolBox.DirExNorCreate(Application.dataPath + "/../Assets/Resources/Textures/GIM/");
                File.WriteAllBytes(Application.dataPath + "/../Assets/Resources/Textures/GIM/" + FileName + "_8b_" + j + ".png", bytes);
            }
        }
    }
}
