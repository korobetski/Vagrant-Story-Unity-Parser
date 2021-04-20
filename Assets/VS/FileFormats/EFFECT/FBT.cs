using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VS.FileFormats.TIM;
using VS.Utils;

namespace VS.FileFormats.EFFECT
{
    public class FBT : ScriptableObject
    {
        public int width = 128;
        public int height = 128;

        public byte[] clut;
        public Texture2D[] textures;

        private long _filesize;
        internal bool tim8 = false;

        public void ParseFromFile(string filepath)
        {

            FileParser fp = new FileParser();
            fp.Read(filepath);

            // in EFFECT/E***_1.FBT
            if (fp.Ext == "FBT")
            {
                _filesize = fp.FileSize;
                ParseFromBuffer(fp.buffer, fp.FileSize);
            }

            fp.Close();


        }

        private void ParseFromBuffer(BinaryReader buffer, long fileSize)
        {
            // E000 is maybe not the only tim8bits effect E204 ?
            if (tim8)
            {
                width = 128;
                height = 128;
                int pad = Mathf.FloorToInt(_filesize / (width * height));
                height *= pad;

                List<byte> _clut = new List<byte>();

                for (uint x = 0; x < height; x++)
                {
                    List<byte> cl2 = new List<byte>();
                    for (uint y = 0; y < width; y++)
                    {
                        byte id = buffer.ReadByte();
                        byte l = (byte)Mathf.RoundToInt(id / 16);
                        byte r = (byte)(id % 16);
                        cl2.Add(r);
                        cl2.Add(l);
                    }
                    cl2.Reverse();
                    _clut.AddRange(cl2);
                }
                _clut.Reverse();
                clut = _clut.ToArray();

                width *= 2;
            }
            else
            {
                width = 128;
                height = 128;
                int pad = Mathf.FloorToInt(_filesize / (width * height));
                height *= pad;

                List<byte> _clut = new List<byte>();

                for (uint x = 0; x < height; x++)
                {
                    List<byte> cl2 = new List<byte>();
                    for (uint y = 0; y < width; y++)
                    {
                        cl2.Add(buffer.ReadByte());
                    }
                    cl2.Reverse();
                    _clut.AddRange(cl2);
                }
                _clut.Reverse();
                clut = _clut.ToArray();
            }
        }

        public Texture2D[] BuildTextures(FBC fbc)
        {
            textures = new Texture2D[fbc.palettes.Length];
            for (uint i = 0; i < fbc.palettes.Length; i++)
            {
                Palette pal = fbc.palettes[i];
                textures[i] = new Texture2D(width, height, TextureFormat.RGBA32, false);
                List<Color> colors = new List<Color>();
                for (uint j = 0; j < width * height; j++)
                {
                    colors.Add(pal.colors[clut[j]]);
                }
                textures[i].SetPixels(colors.ToArray());
                textures[i].alphaIsTransparency = true;
                textures[i].filterMode = FilterMode.Point;
                textures[i].Apply();
            }


            return textures;
        }
    }
}
