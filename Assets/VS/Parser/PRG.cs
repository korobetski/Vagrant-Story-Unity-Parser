using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VS.Utils;

namespace VS.Parser
{

    public class PRG : FileParser
    {

        public void Parse(string filePath)
        {
            PreParse(filePath);

            if (FileName == "TITLE")
            {
                buffer.BaseStream.Position = 0xA70C;
                Color[] colors = new Color[16];
                for (int i = 0; i < 16; ++i)
                {
                    colors[i] = ToolBox.BitColorConverter(buffer.ReadUInt16());
                }

                int width = 128;
                int height = 54;// (int)buffer.BaseStream.Length / width;


                List<Color> cluts = new List<Color>();
                for (uint x = 0; x < height; x++)
                {
                    List<Color> cl2 = new List<Color>();
                    for (uint y = 0; y < width; y++)
                    {
                        byte b = buffer.ReadByte();
                        byte l = (byte)Mathf.RoundToInt(b / 16);
                        byte r = (byte)(b % 16);
                        cl2.Add(colors[r]);
                        cl2.Add(colors[l]);
                    }
                    cl2.Reverse();
                    cluts.AddRange(cl2);
                }
                cluts.Reverse();
                Texture2D tex = new Texture2D(width * 2, height, TextureFormat.ARGB32, false);
                tex.SetPixels(cluts.ToArray());
                tex.Apply();
                byte[] bytes = tex.EncodeToPNG();
                ToolBox.DirExNorCreate(Application.dataPath + "/../Assets/Resources/Textures/Ex/");
                File.WriteAllBytes(Application.dataPath + "/../Assets/Resources/Textures/Ex/Published.png", bytes);
                Debug.Log(buffer.BaseStream.Position);

                width = 64;
                height = 13;
                cluts = new List<Color>();
                for (uint x = 0; x < height; x++)
                {
                    List<Color> cl2 = new List<Color>();
                    for (uint y = 0; y < width; y++)
                    {
                        byte b = buffer.ReadByte();
                        byte l = (byte)Mathf.RoundToInt(b / 16);
                        byte r = (byte)(b % 16);
                        cl2.Add(colors[r]);
                        cl2.Add(colors[l]);
                    }
                    cl2.Reverse();
                    cluts.AddRange(cl2);
                }
                cluts.Reverse();
                tex = new Texture2D(width * 2, height, TextureFormat.ARGB32, false);
                tex.SetPixels(cluts.ToArray());
                tex.Apply();
                bytes = tex.EncodeToPNG();
                ToolBox.DirExNorCreate(Application.dataPath + "/../Assets/Resources/Textures/Ex/");
                File.WriteAllBytes(Application.dataPath + "/../Assets/Resources/Textures/Ex/Squaresoft.png", bytes);





                buffer.BaseStream.Position = 0x46E48 + 0x20;
                width = 128;
                height = 220;
                cluts = new List<Color>();
                for (uint x = 0; x < height; x++)
                {
                    List<Color> cl2 = new List<Color>();
                    for (uint y = 0; y < width; y++)
                    {
                        byte b = buffer.ReadByte();
                        byte l = (byte)Mathf.RoundToInt(b / 16);
                        byte r = (byte)(b % 16);
                        cl2.Add(colors[r]);
                        cl2.Add(colors[l]);
                    }
                    cl2.Reverse();
                    cluts.AddRange(cl2);
                }
                cluts.Reverse();
                tex = new Texture2D(width * 2, height, TextureFormat.ARGB32, false);
                tex.SetPixels(cluts.ToArray());
                tex.Apply();
                bytes = tex.EncodeToPNG();
                ToolBox.DirExNorCreate(Application.dataPath + "/../Assets/Resources/Textures/Ex/");
                File.WriteAllBytes(Application.dataPath + "/../Assets/Resources/Textures/Ex/font.png", bytes);
                Debug.Log(buffer.BaseStream.Position);

                // two pallets blocks with empty pixels

                // icons
                buffer.BaseStream.Position = 0x59B68;
                Color[][] pallets = new Color[16][];
                for (int i = 0; i < 16; i++)
                {
                    pallets[i] = new Color[16];
                    for (int j = 0; j < 16; j++)
                    {
                        pallets[i][j] = ToolBox.BitColorConverter(buffer.ReadUInt16());
                    }
                }
                long texPtr = buffer.BaseStream.Position;
                width = 128;
                height = 480;
                for (int i = 0; i < 16; i++)
                {
                    buffer.BaseStream.Position = texPtr;
                    cluts = new List<Color>();
                    for (uint x = 0; x < height; x++)
                    {
                        List<Color> cl2 = new List<Color>();
                        for (uint y = 0; y < width; y++)
                        {
                            byte b = buffer.ReadByte();
                            byte l = (byte)Mathf.RoundToInt(b / 16);
                            byte r = (byte)(b % 16);
                            cl2.Add(pallets[i][r]);
                            cl2.Add(pallets[i][l]);
                        }
                        cl2.Reverse();
                        cluts.AddRange(cl2);
                    }
                    cluts.Reverse();
                    tex = new Texture2D(width * 2, height, TextureFormat.ARGB32, false);
                    tex.SetPixels(cluts.ToArray());
                    tex.Apply();
                    bytes = tex.EncodeToPNG();
                    ToolBox.DirExNorCreate(Application.dataPath + "/../Assets/Resources/Textures/Ex/");
                    File.WriteAllBytes(Application.dataPath + "/../Assets/Resources/Textures/Ex/icons" + i + ".png", bytes);
                }
                Debug.Log(buffer.BaseStream.Position);


                // menu bg
                colors = new Color[256];
                for (int i = 0; i < 256; ++i)
                {
                    colors[i] = ToolBox.BitColorConverter(buffer.ReadUInt16());
                }
                width = 64 * 3;
                height = 240;
                cluts = new List<Color>();
                for (uint x = 0; x < height; x++)
                {
                    List<Color> cl2 = new List<Color>();
                    for (uint y = 0; y < width; y++)
                    {
                        byte b = buffer.ReadByte();
                        cl2.Add(colors[b]);
                    }
                    cl2.Reverse();
                    cluts.AddRange(cl2);
                }
                cluts.Reverse();
                tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
                tex.SetPixels(cluts.ToArray());
                tex.Apply();
                bytes = tex.EncodeToPNG();
                ToolBox.DirExNorCreate(Application.dataPath + "/../Assets/Resources/Textures/Ex/");
                File.WriteAllBytes(Application.dataPath + "/../Assets/Resources/Textures/Ex/foot.png", bytes);
            }
            else if (FileName == "ENDING")
            {
                buffer.BaseStream.Position = 0x76F0;

                Color[] colors = new Color[16];
                for (int i = 0; i < 16; ++i)
                {
                    colors[i] = ToolBox.BitColorConverter(buffer.ReadUInt16());
                }
                buffer.ReadBytes(12);
                int width = 128;
                int height = 224;// (int)buffer.BaseStream.Length / width;

                List<Color> cluts = new List<Color>();
                for (uint x = 0; x < height; x++)
                {
                    List<Color> cl2 = new List<Color>();
                    for (uint y = 0; y < width; y++)
                    {
                        byte b = buffer.ReadByte();

                        byte l = (byte)Mathf.RoundToInt(b / 16);
                        byte r = (byte)(b % 16);
                        cl2.Add(colors[r]);
                        cl2.Add(colors[l]);
                    }
                    cl2.Reverse();
                    cluts.AddRange(cl2);
                }
                cluts.Reverse();
                Texture2D tex = new Texture2D(width * 2, height, TextureFormat.ARGB32, false);
                tex.SetPixels(cluts.ToArray());
                tex.Apply();
                byte[] bytes = tex.EncodeToPNG();
                ToolBox.DirExNorCreate(Application.dataPath + "/../Assets/Resources/Textures/Ex/");
                File.WriteAllBytes(Application.dataPath + "/../Assets/Resources/Textures/Ex/Ending_font.png", bytes);
                Debug.Log(buffer.BaseStream.Position);


                for (int j = 0; j < 5; j++)
                {
                    buffer.ReadBytes(16);
                    colors = new Color[256];
                    for (int i = 0; i < 256; ++i)
                    {
                        colors[i] = ToolBox.BitColorConverter(buffer.ReadUInt16());
                    }
                    buffer.ReadBytes(16);
                    width = 320;
                    height = 224;

                    cluts = new List<Color>();
                    for (uint x = 0; x < height; x++)
                    {
                        List<Color> cl2 = new List<Color>();
                        for (uint y = 0; y < width; y++)
                        {
                            byte b = buffer.ReadByte();
                            cl2.Add(colors[b]);
                        }
                        cl2.Reverse();
                        cluts.AddRange(cl2);
                    }
                    cluts.Reverse();
                    tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
                    tex.SetPixels(cluts.ToArray());
                    tex.Apply();
                    bytes = tex.EncodeToPNG();
                    ToolBox.DirExNorCreate(Application.dataPath + "/../Assets/Resources/Textures/Ex/");
                    File.WriteAllBytes(Application.dataPath + "/../Assets/Resources/Textures/Ex/Ending_Illus_" + j + ".png", bytes);
                    Debug.Log(buffer.BaseStream.Position);
                }


                buffer.ReadBytes(16);
                colors = new Color[16];
                for (int i = 0; i < 16; ++i)
                {
                    colors[i] = ToolBox.BitColorConverter(buffer.ReadUInt16());
                }
                buffer.ReadBytes(16);

                width = 160;
                height = 224;//(int)(buffer.BaseStream.Length-buffer.BaseStream.Position)/ width;

                cluts = new List<Color>();
                for (uint x = 0; x < height; x++)
                {
                    List<Color> cl2 = new List<Color>();
                    for (uint y = 0; y < width; y++)
                    {
                        byte b = buffer.ReadByte();
                        byte l = (byte)Mathf.RoundToInt(b / 16);
                        byte r = (byte)(b % 16);
                        cl2.Add(colors[r]);
                        cl2.Add(colors[l]);
                        //cl2.Add(new Color32(b,b,b,255));
                    }
                    cl2.Reverse();
                    cluts.AddRange(cl2);
                }
                cluts.Reverse();
                tex = new Texture2D(width * 2, height, TextureFormat.ARGB32, false);
                tex.SetPixels(cluts.ToArray());
                tex.Apply();
                bytes = tex.EncodeToPNG();
                ToolBox.DirExNorCreate(Application.dataPath + "/../Assets/Resources/Textures/Ex/");
                File.WriteAllBytes(Application.dataPath + "/../Assets/Resources/Textures/Ex/Ending_Illus_6.png", bytes);
                Debug.Log(buffer.BaseStream.Position);


                buffer.BaseStream.Position += 6096 + 32;

                colors = new Color[16];
                for (int i = 0; i < 16; ++i)
                {
                    colors[i] = ToolBox.BitColorConverter(buffer.ReadUInt16());
                }
                buffer.ReadBytes(16);

                width = 80;
                height = 80;

                cluts = new List<Color>();
                for (uint x = 0; x < height; x++)
                {
                    List<Color> cl2 = new List<Color>();
                    for (uint y = 0; y < width; y++)
                    {
                        byte b = buffer.ReadByte();

                        byte l = (byte)Mathf.RoundToInt(b / 16);
                        byte r = (byte)(b % 16);
                        cl2.Add(colors[r]);
                        cl2.Add(colors[l]);

                        //cl2.Add(new Color32(b,b,b,255));
                    }
                    cl2.Reverse();
                    cluts.AddRange(cl2);
                }
                cluts.Reverse();
                tex = new Texture2D(width * 2, height, TextureFormat.ARGB32, false);
                tex.SetPixels(cluts.ToArray());
                tex.Apply();
                bytes = tex.EncodeToPNG();
                ToolBox.DirExNorCreate(Application.dataPath + "/../Assets/Resources/Textures/Ex/");
                File.WriteAllBytes(Application.dataPath + "/../Assets/Resources/Textures/Ex/Ending_Illus_7.png", bytes);
                Debug.Log(buffer.BaseStream.Position);

            }
            else
            {
                int width = 128;
                int height = (int)FileSize / width;
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
                File.WriteAllBytes(Application.dataPath + "/../Assets/Resources/Textures/Ex/" + FileName + ".png", bytes);
            }




            buffer.BaseStream.Close();
        }
    }
}

