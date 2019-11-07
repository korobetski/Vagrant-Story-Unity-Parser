using System;
using System.Collections.Generic;
using System.IO;
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
        public long offset;
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


        public AKAOSample(string n, int SIZ, BinaryReader buffer, long OFF)
        {
            name = n;
            size = SIZ;
            offset = OFF;
            data = buffer.ReadBytes(size);
            NumBlocks = (uint)(size / 0x10);
            //Debug.Log(string.Concat("AKAOSample : size ", size, "  AT : ", offset, "  NumBlocks : ", NumBlocks));
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
            List<byte> decomp = new List<byte>();

            float prev1 = 0;
            float prev2 = 0;
            loopStatus = 0;

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

                decomp.AddRange(new byte[28]);
                DecompressBlock(decomp, (int)(k * 28), theBlock, prev1, prev2);
            }
            return decomp;
        }

        private void DecompressBlock(List<byte> pSmp, int a, VAGBlk pVBlk, float prev1, float prev2)
        {
            float[,] coeff = {
                { 0.0f, 0.0f },
                { 60.0f / 64.0f, 0.0f },
                { 115.0f / 64.0f, 52.0f / 64.0f },
                { 98.0f / 64.0f, 55.0f / 64.0f },
                { 122.0f / 64.0f, 60.0f / 64.0f }
            };

            int i;
            byte t;
            float f1, f2;
            float p1, p2;
            int shift = pVBlk.range + 16;

            for (i = 0; i < 14; i++)
            {
                pSmp[a + i * 2] = (byte)(((int)pVBlk.brr[i] << 28) >> shift);
                pSmp[a + i * 2 + 1] = (byte)(((int)(pVBlk.brr[i] & 0xF0) << 24) >> shift);
            }

            // Apply ADPCM decompression ----------------
            i = pVBlk.filter;

            if (i > 0)
            {
                f1 = coeff[i, 0];
                f2 = coeff[i, 1];
                p1 = prev1;
                p2 = prev2;

                for (i = 0; i < 28; i++)
                {
                    t = (byte)Mathf.RoundToInt(pSmp[a + i] + (p1 * f1) - (p2 * f2));
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
