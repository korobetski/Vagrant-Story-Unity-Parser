using System;
using System.IO;
using UnityEngine;
using VS.Utils;

namespace VS.Entity
{
    [Serializable]
    public class VSTIM
    {
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
        private BinaryReader buffer;

        public VSTIM(uint i, uint tl, BinaryReader buffer)
        {

            index = i;
            length = tl;

            this.buffer = buffer;

            if (tl > 0)
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
            }
        }

        public Color32[] buildCLUT(uint x, uint y, BinaryReader buffer)
        {
            uint ox = x - fx;
            uint oy = y - fy;
            buffer.BaseStream.Position = dataPtr + (oy * width + ox) * 2;
            Color32[] colors = new Color32[16];
            for (int i = 0; i < 16; i++)
            {
                colors[i] = Color.black;
            }

            for (int i = 0; i < 16; i++)
            {
                colors[i] = ToolBox.BitColorConverter(buffer.ReadUInt16());
            }

            return colors;
        }

        public Texture2D buildTexture(Color32[] colors, BinaryReader buffer)
        {
            buffer.BaseStream.Position = dataPtr;
            uint size = width * height * 2;
            Color32[] pixels = new Color32[size * 2];
            for (int i = 0; i < size; i++)
            {
                uint c = buffer.ReadByte();
                uint l = ((c & 0xF0) >> 4);
                uint r = (c & 0x0F);
                Color32 cr = colors[r];
                //cr.a = AlphaFromGrayscale(cr);
                Color32 cl = colors[l];
                //cl.a = AlphaFromGrayscale(cl);
                pixels[i * 2] = cr;
                pixels[i * 2 + 1] = cl;
            }

            Texture2D tex = new Texture2D((int)width * 4, (int)height);
            tex.SetPixels32(pixels);
            tex.Apply();
            return tex;
        }

        private byte AlphaFromGrayscale(Color32 cr)
        {
            return (byte)Mathf.Round(cr.r+cr.g+cr.b/3);
        }
    }
}