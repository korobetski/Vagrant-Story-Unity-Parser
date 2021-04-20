using FileFormats;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VS.FileFormats.AKAO
{
    [Serializable]
    public class AKAOSample
    {
        public string name;
        public byte id;
        public ulong pointer;

        [SerializeField]
        private byte[] _rawDatas;
        private uint _size; // rawDatas.Length;
        private uint _numBlocks; // size / 16;

        public uint loopStart = 0;
        private float _prev1 = 0;
        private float _prev2 = 0;

        public byte[] rawDatas { 
            set
            {
                _rawDatas = value;
                _size = (uint)_rawDatas.Length;
                _numBlocks = (uint)Math.Floor((decimal)_size / 16);
            }
            get => _rawDatas;
        }

        public uint size { get => _size; }
        public uint numBlocks { get => _numBlocks; }

        public WAV ConvertToWAV()
        {
            List<short> WAVDatas = DecompressDatas();
            bool looping = (loopStart > 0) ? true : false;
            uint lend = (uint)(size * 1.75);
            WAV wav = new WAV(From16bTo8b(WAVDatas), 1, 1, 44100, 16, looping, loopStart, lend);
            return wav;
        }

        private List<short> DecompressDatas()
        {
            List<short> decomp = new List<short>();
            _prev1 = 0;
            _prev2 = 0;

            for (uint k = 0; k < _numBlocks; k++)
            {
                ADPCMBlock theBlock = new ADPCMBlock(_rawDatas[k * 16], _rawDatas[k * 16 + 1]);
                for (uint l = 2; l < 16; l++)
                {
                    theBlock.brr[l - 2] = _rawDatas[k * 16 + l];
                }
                decomp.AddRange(new short[28]);
                if (_rawDatas[k * 16] != 0xFF && _rawDatas[k * 16 + 1] != 0xFF)
                {
                    DecompressBlock(decomp, (int)(k * 28), theBlock);
                }
            }
            //_IsDecompressed = true;
            return decomp;
        }

        private void DecompressBlock(List<short> pSmp, int a, ADPCMBlock pVBlk)
        {
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
            if (i > 0)
            {
                f1 = coeff[i, 0];
                f2 = coeff[i, 1];
                p1 = _prev1;
                p2 = _prev2;

                for (i = 0; i < 28; i++)
                {
                    t = (short)Math.Round(pSmp[a + i] + (p1 * f1) - (p2 * f2));
                    pSmp[a + i] = t;
                    p2 = p1;
                    p1 = t;
                }

                _prev1 = p1;
                _prev2 = p2;
            }
            else
            {
                _prev2 = pSmp[a + 26];
                _prev1 = pSmp[a + 27];
            }
        }

        private List<byte> From16bTo8b(List<short> w16b)
        {
            List<byte> decomp = new List<byte>();
            for (int i = 0; i < w16b.Count; i++)
            {

                decomp.AddRange(BitConverter.GetBytes(w16b[i]));
            }

            return decomp;
        }
    }
}
