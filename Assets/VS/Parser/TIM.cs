using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VS.Utils;

namespace VS.Parser
{
    public class TIM : FileParser
    {
        public List<Texture2D> textures;
        public int numPallets = 0;

        public int width;
        public int height;
        public int numColors;


        public void Parse(string filePath)
        {
            if (!filePath.EndsWith(".TIM"))
            {
                return;
            }

            PreParse(filePath);

            ushort timH = buffer.ReadUInt16();
            ushort timTag = buffer.ReadUInt16();
            byte[] b = buffer.ReadBytes(12);


            Color[] col = new Color[256];
            for (int i = 0; i < 256; ++i)
            {
                col[i] = ToolBox.BitColorConverter(buffer.ReadUInt16());
            }

            width = 256;
            height = Mathf.FloorToInt((FileSize - 512 - 20) / width);

            List<Color> cluts = new List<Color>();
            for (uint x = 0; x < height; x++)
            {
                List<Color> cl2 = new List<Color>();
                for (uint y = 0; y < width; y++)
                {
                    cl2.Add(col[buffer.ReadByte()]);
                }
                //cl2.Reverse();
                cluts.AddRange(cl2);
            }
            //cluts.Reverse();
            Texture2D tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
            tex.SetPixels(cluts.ToArray());
            tex.Apply();

            byte[] bytes = tex.EncodeToPNG();
            ToolBox.DirExNorCreate(Application.dataPath + "/../Assets/Resources/Textures/TIM/");
            File.WriteAllBytes(Application.dataPath + "/../Assets/Resources/Textures/TIM/" + FileName + ".png", bytes);
        }

        public void ParseIllust(string filePath)
        {

            if (!filePath.EndsWith(".BIN"))
            {
                return;
            }

            PreParse(filePath);
            // i don't know how to decode this for the moment

            byte[] by = buffer.ReadBytes(20);
            //Debug.Log(BitConverter.ToString(by));

            Color[] col = new Color[16];
            for (int i = 0; i < 16; ++i)
            {
                col[i] = ToolBox.BitColorConverter(buffer.ReadUInt16());
            }

            width = 128;
            int height = 15;
            int numBlocks = (int)(FileSize - buffer.BaseStream.Position) / (width * height);
            List<Color> cluts = new List<Color>();
            for (int i = 0; i < numBlocks; i++)
            {
                for (uint x = 0; x < height; x++)
                {
                    List<Color> cl2 = new List<Color>();
                    for (uint y = 0; y < width; y++)
                    {
                        byte id = buffer.ReadByte();
                        //cl2.Add(col[id]);
                        //cl2.Add(new Color32(id, id, id, 255));

                        byte l = (byte)Mathf.RoundToInt(id / 16);
                        byte r = (byte)(id % 16);
                        cl2.Add(col[r]);
                        cl2.Add(col[l]);
                    }
                    cl2.Reverse();
                    cluts.AddRange(cl2);
                }
                cluts.Reverse();
            }
            Texture2D tex = new Texture2D(width * 2, height * numBlocks, TextureFormat.ARGB32, false);
            tex.SetPixels(cluts.ToArray());
            tex.Apply();

            byte[] bytes = tex.EncodeToPNG();
            ToolBox.DirExNorCreate(Application.dataPath + "/../Assets/Resources/Textures/ILLUST/");
            File.WriteAllBytes(Application.dataPath + "/../Assets/Resources/Textures/ILLUST/" + FileName + ".png", bytes);

        }

        public void ParseBG(string filePath)
        {
            PreParse(filePath);

            switch (FileName)
            {
                case ("MAPBG"):
                    width = 320;
                    break;
                case ("MENUBG"):
                    width = 192;
                    break;

            }
            height = Mathf.FloorToInt((FileSize - 512) / width);

            Color[] col = new Color[256];
            for (int i = 0; i < 256; ++i)
            {
                col[i] = ToolBox.BitColorConverter(buffer.ReadUInt16());
            }


            if (FileName == "MENUBG")
            {
                List<Color> cluts = new List<Color>();
                List<Color> cl2 = new List<Color>();
                while (buffer.BaseStream.Position + 4 < buffer.BaseStream.Length)
                {
                    byte[] b = buffer.ReadBytes(4);
                    byte lt = b[0];
                    byte rt = b[2];
                    int blank = lt * 4;
                    int pix = rt * 4;
                    if (buffer.BaseStream.Position + pix <= buffer.BaseStream.Length)
                    {

                        for (uint y = 0; y < blank; y++)
                        {
                            cl2.Add(Color.black);
                        }

                        for (uint y = 0; y < pix; y++)
                        {
                            cl2.Add(col[buffer.ReadByte()]);
                        }
                    }
                    if (buffer.BaseStream.Position == buffer.BaseStream.Length)
                    {
                        break;
                    }
                }
                for (uint y = 0; y < width; y++)
                {
                    List<Color> line = cl2.GetRange((int)y * width, width);
                    line.Reverse();
                    cluts.AddRange(line);
                }


                cluts.Reverse();

                height = cluts.Count / width;
                Texture2D tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
                tex.SetPixels(cluts.ToArray());
                tex.Apply();

                byte[] bytes = tex.EncodeToPNG();
                ToolBox.DirExNorCreate(Application.dataPath + "/../Assets/Resources/Textures/BG/");
                File.WriteAllBytes(Application.dataPath + "/../Assets/Resources/Textures/BG/" + FileName + width + ".png", bytes);
            }
            else
            {

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
                ToolBox.DirExNorCreate(Application.dataPath + "/../Assets/Resources/Textures/BG/");
                File.WriteAllBytes(Application.dataPath + "/../Assets/Resources/Textures/BG/" + FileName + ".png", bytes);

            }

        }

        public void ParseWEP(BinaryReader buffer)
        {
            numPallets = 7;
            uint texMapSize = buffer.ReadUInt32();
            buffer.ReadByte();
            width = buffer.ReadByte() * 2;
            height = buffer.ReadByte() * 2;
            numColors = buffer.ReadByte();
            List<List<Color32>> pallets = new List<List<Color32>>();
            if (numColors > 0)
            {
                List<Color32> handleColors = new List<Color32>();
                for (uint i = 0; i < (int)(numColors / 3); i++)
                {
                    handleColors.Add(ToolBox.BitColorConverter(buffer.ReadUInt16()));
                }

                for (uint h = 0; h < numPallets; h++)
                {
                    List<Color32> colors = new List<Color32>();
                    colors.AddRange(handleColors);
                    for (uint i = 0; i < (int)(numColors / 3) * 2; i++)
                    {
                        colors.Add(ToolBox.BitColorConverter(buffer.ReadUInt16()));
                    }

                    pallets.Add(colors);
                }
            }
            List<int> cluts = new List<int>();
            for (uint x = 0; x < height; x++)
            {
                List<int> cl2 = new List<int>();
                for (uint y = 0; y < width; y++)
                {
                    cl2.Add(buffer.ReadByte());
                }
                cl2.Reverse();
                cluts.AddRange(cl2);
            }
            cluts.Reverse();

            textures = new List<Texture2D>();
            for (int h = 0; h < numPallets; h++)
            {
                List<Color> colors = new List<Color>();
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (cluts[(int)((y * width) + x)] < numColors)
                        {
                            colors.Add(pallets[h][cluts[(int)((y * width) + x)]]);
                        }
                        else
                        {
                            colors.Add(Color.clear);
                        }
                    }
                }

                Texture2D tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
                tex.SetPixels(colors.ToArray());
                tex.Apply();
                textures.Add(tex);
            }
            textures.Add(textures[0]);
        }
        public void ParseSHP(BinaryReader buffer, bool vc = false)
        {
            numPallets = 2;
            uint texMapSize = buffer.ReadUInt32();
            byte unk = buffer.ReadByte();
            width = buffer.ReadByte() * 2;
            height = buffer.ReadByte() * 2;
            numColors = buffer.ReadByte();


            if (UseDebug)
            {
                Debug.LogWarning(string.Concat("Parse SHP Texture => texMapSize : ", texMapSize, "   unk : ", unk,
                "   width : ", width, "   height : ", height, "   numColors : ", numColors));
            }

            // Parse SHP 37 Texture => texMapSize : 33156   unk : 1   width : 128   height : 256   numColors : 160
            // Parse SHP 65 Texture => texMapSize : 32804   unk : 16   width : 128   height : 256   numColors : 16
            if (!vc)
            {
                List<List<Color32>> pallets = new List<List<Color32>>();
                if (numColors > 0)
                {
                    for (uint h = 0; h < numPallets; h++)
                    {
                        List<Color32> colors = new List<Color32>();
                        for (uint i = 0; i < numColors; i++)
                        {
                            colors.Add(ToolBox.BitColorConverter(buffer.ReadUInt16()));
                        }

                        pallets.Add(colors);
                    }
                }
                List<int> cluts = new List<int>();
                for (uint x = 0; x < height; x++)
                {
                    List<int> cl2 = new List<int>();
                    for (uint y = 0; y < width; y++)
                    {
                        cl2.Add(buffer.ReadByte());
                    }
                    cl2.Reverse();
                    cluts.AddRange(cl2);
                }
                cluts.Reverse();

                textures = new List<Texture2D>();
                for (int h = 0; h < numPallets; h++)
                {
                    List<Color> colors = new List<Color>();
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            if (cluts[(int)((y * width) + x)] < numColors)
                            {
                                colors.Add(pallets[h][cluts[(int)((y * width) + x)]]);
                            }
                            else
                            {
                                colors.Add(Color.clear);
                            }
                        }
                    }

                    Texture2D tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
                    tex.SetPixels(colors.ToArray());
                    tex.Apply();
                    textures.Add(tex);
                }
            }
            else
            {
                numPallets = 2;
                List<List<Color32>> pallets = new List<List<Color32>>();
                if (numColors > 0)
                {
                    for (uint h = 0; h < numPallets; h++)
                    {
                        List<Color32> colors = new List<Color32>();
                        for (uint i = 0; i < numColors; i++)
                        {
                            colors.Add(ToolBox.BitColorConverter(buffer.ReadUInt16()));
                        }

                        pallets.Add(colors);
                    }
                }

                List<int> cluts = new List<int>();
                for (uint x = 0; x < height; x++)
                {
                    List<int> cl2 = new List<int>();
                    for (uint y = 0; y < width; y++)
                    {
                        byte id = buffer.ReadByte();
                        byte l = (byte)Mathf.RoundToInt(id / 16);
                        byte r = (byte)(id % 16);
                        cl2.Add(r);
                        cl2.Add(l);
                    }
                    cl2.Reverse();
                    cluts.AddRange(cl2);
                }
                cluts.Reverse();

                textures = new List<Texture2D>();
                for (int h = 0; h < numPallets; h++)
                {
                    List<Color> colors = new List<Color>();
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width * 2; x++)
                        {
                            if (cluts[(int)((y * width * 2) + x)] < numColors)
                            {
                                colors.Add(pallets[h][cluts[(int)((y * width * 2) + x)]]);
                            }
                            else
                            {
                                colors.Add(Color.clear);
                            }
                        }
                    }

                    Texture2D tex = new Texture2D(width * 2, height, TextureFormat.ARGB32, false);
                    tex.SetPixels(colors.ToArray());
                    tex.Apply();
                    textures.Add(tex);
                }


                width *= 2;
            }

        }
        public Texture2D DrawSHP(bool CreatePNG = false)
        {
            Texture2D tex = new Texture2D(width * numPallets, height, TextureFormat.ARGB32, false);
            tex.PackTextures(textures.ToArray(), 0);
            tex.filterMode = FilterMode.Trilinear;
            tex.anisoLevel = 4;
            tex.wrapMode = TextureWrapMode.Repeat;
#if UNITY_EDITOR
            tex.alphaIsTransparency = true;
#endif

            if (CreatePNG)
            {
                byte[] bytes = tex.EncodeToPNG();
                ToolBox.DirExNorCreate("Assets/Resources/Textures/Models/");
                File.WriteAllBytes("Assets/Resources/Textures/Models/SHP_" + FileName + "_tex.png", bytes);
            }
            tex.Compress(true);
            return tex;
        }

        public Texture2D DrawPack(bool CreatePNG = false)
        {
            Texture2D tex = new Texture2D(width * 2, height * 4, TextureFormat.ARGB32, false);
            tex.PackTextures(textures.ToArray(), 0);
            tex.filterMode = FilterMode.Trilinear;
            tex.anisoLevel = 4;
            tex.wrapMode = TextureWrapMode.Repeat;
#if UNITY_EDITOR
            tex.alphaIsTransparency = true;
#endif

            if (CreatePNG)
            {
                byte[] bytes = tex.EncodeToPNG();
                ToolBox.DirExNorCreate("Assets/Resources/Weapon/");
                File.WriteAllBytes("Assets/Resources/Weapon/WEP_" + FileName + "_tex.png", bytes);
            }
            tex.Compress(true);
            return tex;
        }
    }
}
