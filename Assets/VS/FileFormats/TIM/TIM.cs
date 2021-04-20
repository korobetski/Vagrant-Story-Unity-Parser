using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using VS.Utils;

namespace VS.FileFormats.TIM
{
    public class TIM:ScriptableObject
    {
        public enum TIMType { WEP, SHP, ZND };

        public string Filename;

        public uint index;
        public uint length;
        public uint h = 0;
        public uint bpp = 0;
        public uint imgLen = 0;
        public uint fx = 0;
        public uint fy = 0;
        public uint width = 0;
        public uint height = 0;
        public uint dataLen = 0;
        public uint dataPtr = 0;

        public byte numColors;
        public byte numPalettes;
        public Palette[] palettes;
        public byte[] clut;

        public TIMType type;


        public void ParseWEPFromBuffer(BinaryReader buffer)
        {

            type = TIMType.WEP;
            numPalettes = 8;
            // the first palette is the common colors (16)
            imgLen = buffer.ReadUInt32();
            bpp = buffer.ReadByte();
            width = (uint) buffer.ReadByte() * 2;
            height = (uint) buffer.ReadByte() * 2;
            numColors = buffer.ReadByte();

            if (numColors > 0)
            {
                uint numCommonColors = (uint)(numColors / 3);
                uint numPaletteColors = numCommonColors * 2;
                palettes = new Palette[numPalettes];

                for (uint i = 0; i < numPalettes; i++)
                {
                    if (i == 0)
                    {
                        palettes[0] = new Palette(numCommonColors);
                        for (uint j = 0; j < numCommonColors; j++)
                        {
                            palettes[0].colors[j] = (ToolBox.BitColorConverter(buffer.ReadUInt16()));
                        }
                    } else
                    {
                        palettes[i] = new Palette(numPaletteColors);
                        for (uint j = 0; j < numPaletteColors; j++)
                        {
                            palettes[i].colors[j] = (ToolBox.BitColorConverter(buffer.ReadUInt16()));
                        }
                    }
                }
            }

            clut = new byte[width * height];
            for (uint i = 0; i < height * width; i++)
            {
                clut[i] = buffer.ReadByte();
            }
            clut = ReverseCLUT();
        }

        public void ParseSHPFromBuffer(BinaryReader buffer, bool hasColoredVertices)
        {
            type = TIMType.SHP;
            numPalettes = 2;
            imgLen = buffer.ReadUInt32();
            bpp = buffer.ReadByte(); // maybe i should use this instead of hasColoredVertices
            width = (uint)buffer.ReadByte() * 2;
            height = (uint)buffer.ReadByte() * 2;
            numColors = buffer.ReadByte();

            if (numColors > 0)
            {
                palettes = new Palette[numPalettes];
                for (uint i = 0; i < numPalettes; i++)
                {
                    palettes[i] = new Palette(numColors);
                    for (uint j = 0; j < numColors; j++)
                    {
                        palettes[i].colors[j] = (ToolBox.BitColorConverter(buffer.ReadUInt16()));
                    }
                }
            }

            if (!hasColoredVertices)
            {
                clut = new byte[width * height];
                for (uint i = 0; i < height * width; i++)
                {
                    clut[i] = buffer.ReadByte();
                }
                clut = ReverseCLUT();
            }
            else
            {
                clut = new byte[width * height * 2];
                for (uint i = 0; i < height * width; i+=2)
                {
                    byte id = buffer.ReadByte();
                    byte l = (byte)Mathf.RoundToInt(id / 16);
                    byte r = (byte)(id % 16);
                    clut[i] = l;
                    clut[i+1] = r;
                }
                clut = ReverseCLUT();

                width *= 2;
            }
        }


        public void ParseZNDFromBuffer(uint i, uint timLength, BinaryReader buffer)
        {
            type = TIMType.ZND;
            index = i;
            length = timLength;

            if (timLength > 0)
            {
                h = buffer.ReadUInt32();
                bpp = buffer.ReadUInt32();
                imgLen = buffer.ReadUInt32();
                fx = buffer.ReadUInt16();
                fy = buffer.ReadUInt16();
                width = buffer.ReadUInt16();
                height = buffer.ReadUInt16();
                dataLen = imgLen - 12;
                dataPtr = (uint)buffer.BaseStream.Position;
                //clut = buffer.ReadBytes((int)dataLen);
                if (dataLen > 0)
                {
                    if (fy > 0)
                    {
                        // it's a color palette, two bytes = one color
                        palettes = new Palette[1];
                        palettes[0] = new Palette(dataLen / 2);
                        
                        for (uint j = 0; j < dataLen/2; j++)
                        {
                            palettes[0].colors[j] = ToolBox.BitColorConverter(buffer.ReadUInt16());
                        }
                        
                    }
                    else
                    {
                        // it must be a texture, one byte = 2 pixels
                        clut = new byte[dataLen*2];
                        for (uint j = 0; j < dataLen*2; j += 2)
                        {
                            byte id = buffer.ReadByte();
                            byte l = (byte)Mathf.RoundToInt(id / 16);
                            byte r = (byte)(id % 16);
                            clut[j] = r;
                            clut[j + 1] = l;
                        }
                        //clut = ReverseCLUT();
                        // we don't modify width because of ZND GetTIM() and GetPalette() method
                        //width *= 4;
                    }
                }

                buffer.BaseStream.Position = dataPtr + dataLen;
            }
        }

        private byte[] ReverseCLUT()
        {
            uint i = 0;
            List<byte> cluts = new List<byte>();
            // width can be wrong
            uint width = (uint) clut.Length / height;
            for (uint x = 0; x < height; x++)
            {
                List<byte> cl2 = new List<byte>();
                for (uint y = 0; y < width; y++)
                {
                    cl2.Add(clut[i]);
                    i++;
                }
                cl2.Reverse();
                cluts.AddRange(cl2);
            }
            cluts.Reverse();
            return cluts.ToArray();
        }
        public void DrawPNG()
        {

            byte[] bytes = BuildTexture().EncodeToPNG();
            ToolBox.DirExNorCreate("Assets/Resources/Textures/Weapon/");
            File.WriteAllBytes("Assets/Resources/Textures/Weapon/WEP_" + Filename + "_tex.png", bytes);
        }

        public Texture2D BuildTexture()
        {
            List<Texture2D> textures;
            Texture2D pack = new Texture2D(1,1);
            switch (type)
            {
                case TIMType.WEP:
                    textures = new List<Texture2D>();
                    uint numCommonColors = (uint)(numColors / 3);
                    uint numPaletteColors = numCommonColors * 2;
                    for (int h = 0; h < numPalettes; h++)
                    {
                        List<Color> colors = new List<Color>();
                        if (h == 0)
                        {
                            for (int y = 0; y < height; y++)
                            {
                                for (int x = 0; x < width; x++)
                                {
                                    colors.Add(Grayscale(clut[(int)((y * width) + x)]) );
                                }
                            }
                        }
                        else
                        {
                            for (int y = 0; y < height; y++)
                            {
                                for (int x = 0; x < width; x++)
                                {
                                    if (clut[(int)((y * width) + x)] < numCommonColors)
                                    {
                                        colors.Add(palettes[0].colors[clut[(int)((y * width) + x)]]);
                                    }
                                    else
                                    {
                                        colors.Add(palettes[h].colors[clut[(int)((y * width) + x)] - numCommonColors]);
                                    }
                                }
                            }
                        }

                        Texture2D tex = new Texture2D((int)width, (int)height, TextureFormat.ARGB32, false);
                        tex.SetPixels(colors.ToArray());
                        tex.Apply();
                        textures.Add(tex);
                    }

                    pack = new Texture2D((int)width * 2, (int)height * 4, TextureFormat.ARGB32, false);
                    pack.PackTextures(textures.ToArray(), 0);
                    pack.filterMode = FilterMode.Point;
                    pack.wrapMode = TextureWrapMode.Repeat;

                    break;
                case TIMType.SHP:
                    textures = new List<Texture2D>();

                    for (int h = 0; h < numPalettes; h++)
                    {
                        List<Color> colors = new List<Color>();
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                colors.Add(palettes[h].colors[clut[(int)((y * width) + x)]]);
                            }
                        }

                        Texture2D tex = new Texture2D((int)width, (int)height, TextureFormat.ARGB32, false);
                        tex.SetPixels(colors.ToArray());
                        tex.Apply();
                        textures.Add(tex);
                    }


                    pack = new Texture2D((int)width*2, (int)height, TextureFormat.ARGB32, false);
                    pack.PackTextures(textures.ToArray(), 0);
                    pack.filterMode = FilterMode.Point;
                    pack.wrapMode = TextureWrapMode.Repeat;
                    break;
            }

            return pack;

        }


        public Texture2D GetTexture(byte paletteId = 0)
        {
            Texture2D tex = new Texture2D(1,1);
            switch (type)
            {
                case TIMType.WEP:

                    uint numCommonColors = (uint)(numColors / 3);
                    uint numPaletteColors = numCommonColors * 2;
                    List<Color> colors = new List<Color>();

                    if (paletteId == 0) {
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                colors.Add(Grayscale(clut[(int)((y * width) + x)]));
                            }
                        }
                    }
                    else
                    {
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                if (clut[(int)((y * width) + x)] < numCommonColors)
                                {
                                    colors.Add(palettes[0].colors[clut[(int)((y * width) + x)]]);
                                }
                                else
                                {
                                    colors.Add(palettes[paletteId].colors[clut[(int)((y * width) + x)] - numCommonColors]);
                                }
                            }
                        }
                    }

                    tex = new Texture2D((int)width, (int)height, TextureFormat.ARGB32, false);
                    tex.SetPixels(colors.ToArray());
                    tex.filterMode = FilterMode.Point;
                    tex.wrapMode = TextureWrapMode.Repeat;
                    tex.Apply();
                    break;
                case TIMType.SHP:
                    colors = new List<Color>();
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            colors.Add(palettes[paletteId].colors[clut[(int)((y * width) + x)]]);
                        }
                    }

                    tex = new Texture2D((int)width, (int)height, TextureFormat.ARGB32, false);
                    tex.SetPixels(colors.ToArray());
                    tex.filterMode = FilterMode.Point;
                    tex.wrapMode = TextureWrapMode.Repeat;
                    tex.Apply();
                    break;
                case TIMType.ZND:
                    colors = new List<Color>();
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            colors.Add(palettes[paletteId].colors[clut[(int)((y * width) + x)]]);
                        }
                    }
                    tex = new Texture2D((int)width, (int)height, TextureFormat.ARGB32, false);
                    tex.SetPixels(colors.ToArray());
                    tex.filterMode = FilterMode.Point;
                    tex.wrapMode = TextureWrapMode.Repeat;
                    tex.Apply();
                    break;
            }

            return tex;

        }


        internal Texture2D GetTextureWithPalette(Palette palette)
        {
            Texture2D tex = new Texture2D(1, 1);
            List<Color> colors = new List<Color>();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width * 4; x++)
                {
                    colors.Add(palette.colors[clut[(int)((y * width * 4) + x)]]);
                }
            }
            tex = new Texture2D((int)width * 4, (int)height, TextureFormat.ARGB32, false);
            tex.SetPixels(colors.ToArray());
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.Apply();

            return tex;
        }
        private Color32 Grayscale(byte c)
        {
            float ratio = 255 / numColors;
            byte v = (byte)Math.Floor(c * ratio);
            return new Color32(v, v, v, 255);
        }

        private byte AlphaFromGrayscale(Color32 cr)
        {
            return (byte)Mathf.Round(cr.r + cr.g + cr.b / 3);
        }
    }




    [CustomEditor(typeof(TIM))]
    public class TIMEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var tim = target as TIM;
            DrawDefaultInspector();
            if (GUILayout.Button("Draw PNG"))
            {
                tim.DrawPNG();
            }
        }
    }
}