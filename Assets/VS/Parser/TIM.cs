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
            for (uint x = 0; x < width; x++)
            {
                for (uint y = 0; y < height; y++)
                {
                    cluts.Add(buffer.ReadByte());
                }
            }

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
        public void ParseSHP(BinaryReader buffer)
        {
            numPallets = 2;
            uint texMapSize = buffer.ReadUInt32();
            buffer.ReadByte();
            width = buffer.ReadByte() * 2;
            height = buffer.ReadByte() * 2;
            numColors = buffer.ReadByte();
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
            for (uint x = 0; x < width; x++)
            {
                for (uint y = 0; y < height; y++)
                {
                    cluts.Add(buffer.ReadByte());
                }
            }

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

        public Texture2D DrawSHP(bool CreatePNG = false)
        {
            Texture2D tex = new Texture2D(width * 2, height, TextureFormat.ARGB32, false);
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
                File.WriteAllBytes("Assets/Resources/Textures/Models/" + FileName + "_tex.png", bytes);
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
                ToolBox.DirExNorCreate("Assets/Resources/Textures/Weapons/");
                File.WriteAllBytes("Assets/Resources/Textures/Weapons/" + FileName + "_tex.png", bytes);
            }
            tex.Compress(true);
            return tex;
        }
    }
}
