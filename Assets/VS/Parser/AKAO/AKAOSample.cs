using System;
using System.Collections.Generic;
using UnityEngine;
using VS.Format;

//Minoru Akao
namespace VS.Parser.Akao
{
    public class AKAOSample
    {
        public string name = "";
        public byte[] data;
        public int size;
        public ulong offset;
        public uint NumBlocks;

        private bool _IsDecompressed = false;
        public float compressionRatio = 3.5f;
        public List<byte> WAVDatas;


        public int loopStatus = -1;
        public uint loopType;
        public ushort loopStartMeasure;
        public ushort loopLengthMeasure;
        public uint loopStart;
        public ulong loopLength;

        public float prev1 = 0;
        public float prev2 = 0;


        public AKAOSample(string n, byte[] dt, ulong off)
        {
            name = n;
            data = dt;
            size = dt.Length;
            NumBlocks = (uint)(size / 0x10);
            offset = off;
            Debug.Log(string.Concat(name, "   Size : ", size, "   numBlocks : ", NumBlocks, "   offset : ", offset));
        }


        public WAV ConvertToWAV()
        {
            if (!_IsDecompressed)
            {
                WAVDatas = DecompressDatas();
            }
            WAV wav = new WAV(WAVDatas, 1, 1, 44100, 16);
            return wav;
        }

        private List<byte> DecompressDatas()
        {
            List<short> decomp = new List<short>();
            loopStatus = 0;
            prev1 = 0;
            prev2 = 0;

            for (uint k = 0; k < NumBlocks; k++)
            {
                VAGBlk theBlock = new VAGBlk(data[k * 16], data[k * 16 + 1]);

                if (theBlock.flagLoop)
                {
                    loopStart = (k * 16);
                    loopLength = (ulong)(size - k * 16);
                }
                if (theBlock.flagEnd > 0 && theBlock.flagLooping)
                {
                    loopStatus = 1;
                }

                for (uint l = 2; l < 16; l++)
                {
                    theBlock.brr[l - 2] = data[k * 16 + l];
                }

                decomp.AddRange(new short[28]);
                DecompressBlock(decomp, (int)(k * 28), theBlock);
            }
            _IsDecompressed = true;
            return ConvertTo16bitSigned(decomp);
        }

        private List<byte> ConvertTo16bitSigned(List<short> w8b)
        {
            List<byte> decomp = new List<byte>();
            for (int i = 0; i < w8b.Count; i++)
            {

                decomp.AddRange(BitConverter.GetBytes(w8b[i]));
                //decomp.AddRange(new byte[]{ (byte)(w8b[i] << 8), (byte)(w8b[i]) });
            }

            return decomp;
        }

        private void DecompressBlock(List<short> pSmp, int a, VAGBlk pVBlk)
        {
            //Debug.Log(string.Concat("DecompressBlock : ", prev1, "  :  ", prev2));
            float[,] coeff = {
                { 0.0f, 0.0f },
                { 60.0f / 64.0f, 0.0f },
                { 115.0f / 64.0f, 52.0f / 64.0f },
                { 98.0f / 64.0f, 55.0f / 64.0f },
                { 122.0f / 64.0f, 60.0f / 64.0f }
            };

            int i;
            short t;
            float f1, f2;
            float p1, p2;
            int shift = pVBlk.range + 16;

            for (i = 0; i < 14; i++)
            {
                pSmp[a + i * 2] = (short)(((int)pVBlk.brr[i] << 28) >> shift);
                pSmp[a + i * 2 + 1] = (short)(((int)(pVBlk.brr[i] & 0xF0) << 24) >> shift);
            }

            // Apply ADPCM decompression ----------------
            i = pVBlk.filter;
            //Debug.Log(string.Concat("pVBlk.filter : "+ pVBlk.filter));

            if (i > 0)
            {
                f1 = coeff[i, 0];
                f2 = coeff[i, 1];
                p1 = prev1;
                p2 = prev2;

                for (i = 0; i < 28; i++)
                {
                    t = (short)Math.Round(pSmp[a + i] + (p1 * f1) - (p2 * f2));
                    pSmp[a + i] = t;
                    p2 = p1;
                    p1 = t;
                }

                prev1 = p1;
                prev2 = p2;
            }
            else
            {
                prev2 = pSmp[a + 26];
                prev1 = pSmp[a + 27];
            }
        }

        private void SetLoopStatus(int statut)
        {
            loopStatus = statut;
        }

        private void SetLoopLength(uint length)
        {
            loopLength = length;
        }

        private void SetLoopOffset(uint start)
        {
            loopStart = start;
        }

        public class VAGBlk
        {
            public int range = 4;
            public int filter = 4;
            public int flagEnd = 1;
            public bool flagLooping = false;
            public bool flagLoop = false;
            public byte[] brr;

            public VAGBlk(byte a, byte b)
            {
                range = a & 0xF;
                filter = (a & 0xF0) >> 4;
                flagEnd = b & 0x1;
                flagLooping = (b & 0x2) > 0;
                flagLoop = (b & 0x4) > 0;
                brr = new byte[14];
            }

        }
    }

}
