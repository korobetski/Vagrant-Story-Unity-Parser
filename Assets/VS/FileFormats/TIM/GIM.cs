using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VS.Utils;

namespace VS.FileFormats.TIM
{

    public class GIM : ScriptableObject
    {
        public string Filename;

        public bool has8bitsIds = false;
        public GIMFrame[] frames;

        public Palette[] palettes16;
        public Palette[] palettes256;

        public SplitBlock[] blocks;

        private readonly int blockWidth = 128;
        private readonly int blockHeight = 15;

        public void ParseFromFile(string filepath)
        {
            FileParser fp = new FileParser();
            fp.Read(filepath);

            if (fp.Ext == "GIM")
            {
                Filename = fp.FileName;
                if (Filename == "BLACK")
                {
                    return;
                }
                ParseFromBuffer(fp.buffer, fp.FileSize);
            }

            fp.Close();
        }

        public void ParseFromBuffer(BinaryReader buffer, long limit)
        {
            // frames section
            GIMFrame mainFrame = new GIMFrame();
            mainFrame.fwidth = buffer.ReadByte(); // multiply by 64 to get the real width
            mainFrame.fheigth = buffer.ReadByte(); // multiply by 15 to get the real height
            mainFrame.numBlocks = buffer.ReadByte(); // blocks are 128 width.... so we need to cut them in half
            mainFrame.unk1 = buffer.ReadByte();
            mainFrame.numPalette256 = buffer.ReadInt16(); // num of 256 col pallets (0 means no pallet)
            mainFrame.additionalFrames = buffer.ReadInt16(); // num of additional layers
            // half block table
            int blockTableSize = mainFrame.fwidth * mainFrame.fheigth;
            mainFrame.hbt = new ushort[blockTableSize];
            for (uint i = 0; i < blockTableSize; i++)
            {
                mainFrame.hbt[i] = buffer.ReadUInt16();
                // trick to detect 8bits TIM
                if (!has8bitsIds) if (mainFrame.hbt[i] == 514) has8bitsIds = true;
            }
            frames = new GIMFrame[1 + mainFrame.additionalFrames];
            frames[0] = mainFrame;

            if (mainFrame.additionalFrames > 0)
            {
                for (uint i = 1; i < 1 + mainFrame.additionalFrames; i++)
                {
                    GIMFrame frame = new GIMFrame();
                    frame.fwidth = buffer.ReadByte();
                    frame.fheigth = buffer.ReadByte();
                    // next values are often empty in additionnal frames
                    frame.numBlocks = buffer.ReadByte();
                    frame.unk1 = buffer.ReadByte();
                    frame.numPalette256 = buffer.ReadInt16();
                    frame.additionalFrames = buffer.ReadInt16();
                    int frameBlockTableSize = frame.fwidth * frame.fheigth;
                    frame.hbt = new ushort[frameBlockTableSize];
                    for (uint j = 0; j < frameBlockTableSize; j++)
                    {
                        frame.hbt[j] = buffer.ReadUInt16();
                        // trick to detect 8bits TIM
                        if (!has8bitsIds) if (frame.hbt[j] == 514) has8bitsIds = true;
                    }
                    frames[i] = frame;
                }
            }
            // ultimate test, we must be sure of that
            if (!has8bitsIds) if (mainFrame.numPalette256 == 0) has8bitsIds = true;
            // 8 bytes block align padding
            int rem = (int)buffer.BaseStream.Position % 8;
            if (rem != 0) buffer.ReadBytes(8 - rem);


            // palettes section
            // 16 colors palettes first, always 4 slots
            palettes16 = new Palette[4];
            for (int i = 0; i < 4; i++)
            {
                Color32[] col = new Color32[16];
                for (int j = 0; j < 16; j++)
                {
                    col[j] = ToolBox.BitColorConverter(buffer.ReadUInt16());
                }
                palettes16[i] = new Palette(16);
                palettes16[i].colors = col;
            }

            // then 256 colors palettes
            palettes256 = new Palette[mainFrame.numPalette256];
            for (int i = 0; i < mainFrame.numPalette256; i++)
            {
                Color32[] col = new Color32[256];
                for (int j = 0; j < 256; j++)
                {
                    col[j] = ToolBox.BitColorConverter(buffer.ReadUInt16());
                }
                palettes256[i] = new Palette(256);
                palettes256[i].colors = col;
            }

            // old fashion ways to determine numBlocks :s
            // int numBlocks = (int)(buffer.BaseStream.Length - buffer.BaseStream.Position) / (blockWidth * blockHeight);

            uint numSplit = 2;
            if (has8bitsIds) numSplit = 4;
            List<SplitBlock> _blocks = new List<SplitBlock>();

            for (int i = 0; i < mainFrame.numBlocks; i++)
            {
                // black magic number
                int ome = 512 + 188 * Mathf.FloorToInt(i / 17);
                SplitBlock[] splitBlocks = new SplitBlock[numSplit];
                for (uint j = 0; j < numSplit; j++) splitBlocks[j] = new SplitBlock((uint)(ome + i * 4 + j));
                // we fill blocks
                for (uint x = 0; x < blockHeight; x++)
                {
                    if (!has8bitsIds)
                    {
                        for (uint j = 0; j < numSplit; j++) splitBlocks[j].clut.AddRange(buffer.ReadBytes(64));
                    }
                    else
                    {
                        // we need 8 bits per pixels so we must split byte in two
                        for (uint j = 0; j < numSplit; j++)
                        {
                            List<byte> cl2 = new List<byte>();
                            
                            for (uint y = 0; y < 32; y++)
                            {
                                // EOF security
                                byte id = (buffer.BaseStream.Position < buffer.BaseStream.Length) ? buffer.ReadByte() : (byte)0;
                                byte l = (byte)Mathf.RoundToInt(id / 16);
                                byte r = (byte)(id % 16);
                                cl2.Add(r);
                                cl2.Add(l);
                            }
                            splitBlocks[j].clut.AddRange(cl2);
                        }
                    }
                }
                for (uint j = 0; j < numSplit; j++) _blocks.Add(splitBlocks[j]);
            }

            blocks = _blocks.ToArray();

            // we have gather all informations, now we can reconstruct pictures
            Reconstruct();

        }


        public void Reconstruct()
        {
            for(uint i = 0; i < frames.Length; i++)
            {
                GIMFrame f = frames[i];
                Palette palette;
                if (frames[0].numPalette256 > 0) palette = (f.numPalette256 > 0) ? palettes256[f.numPalette256 - 1] : palettes256[0];
                else palette = palettes16[i];
                Texture2D texture = new Texture2D(f.fwidth * 64, f.fheigth * 15, TextureFormat.ARGB32, false);
                uint z = 0;
                for (uint y = 0; y < f.fheigth; y++)
                {
                    for (uint x = 0; x < f.fwidth; x++)
                    {
                        if (f.hbt[z] != 0)
                        {
                            if (f.hbt[z] >= 512)
                            {
                                texture.SetPixels32((int)x * 64, (int)y * 15, 64, 15, GetHalfBlockColors(f.hbt[z], blocks, palette));
                            }
                        }
                        z++;
                    }
                }
                // we need a vertical mirror
                List<Color[]> flipped = new List<Color[]>();
                for (int y = 0; y < f.fheigth * 15; y++)
                {
                    Color[] yLine = texture.GetPixels(0, y, f.fwidth * 64, 1);
                    flipped.Add(yLine);
                }
                flipped.Reverse();
                for (int y = 0; y < f.fheigth * 15; y++)
                {
                    texture.SetPixels(0, y, f.fwidth * 64, 1, flipped[y]);
                }
                texture.Apply();

                byte[] bytes = texture.EncodeToPNG();
                ToolBox.DirExNorCreate("Assets/Resources/Textures/GIM/");
                File.WriteAllBytes(string.Concat("Assets/Resources/Textures/GIM/",Filename,"_", i, ".png"), bytes);
            }
        }

        private Color32[] GetHalfBlockColors(ushort v, SplitBlock[] blocks, Palette palette)
        {
            List<Color32> _block = new List<Color32>();
            foreach (SplitBlock b in blocks)
            {
                if (b.id == v)
                {
                    for (int i = 0; i < b.clut.Count; i++)
                    {
                        _block.Add(palette.colors[b.clut[i]]);
                    }

                    return _block.ToArray();
                }
            }
            for (int j = 0; j < 64 * 15; j++) _block.Add(Color.black);
            return _block.ToArray();
        }

        [Serializable]
        public class SplitBlock
        {
            public uint id;
            public List<byte> clut;

            public SplitBlock(uint _id)
            {
                id = _id;
                clut = new List<byte>();
            }

            public SplitBlock(uint _id, List<byte> _clut)
            {
                id = _id;
                clut = _clut;
            }
        }
    }
}
