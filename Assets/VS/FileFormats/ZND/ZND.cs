using System;
using System.IO;
using UnityEngine;
using VS.FileFormats.TIM;

namespace VS.FileFormats.ZND
{
    public class ZND:ScriptableObject
    {
        public string Filename;
        public uint ptrMPD;
        public uint lenMPD;
        public uint numMPD; // lenMPD / 8
        public uint ptrEnemies;
        public uint lenEnemies;
        public uint numEnemies;
        public uint ptrTIM;
        public uint lenTIM;
        public byte waveId;
        // 7 bytes padding;

        public Vector2Int[] MPD_LBA;
        public Vector2Int[] ZUD_LBA;
        public ZNDMonster[] monsters;

        public uint lenTIM2;
        public uint numTIM;
        public TIM.TIM[] TIMs;



        public void ParseFromFile(string filepath)
        {
            FileParser fp = new FileParser();
            fp.Read(filepath);

            // in MAP/ZONE***.ZND
            if (fp.Ext == "ZND")
            {
                Filename = fp.FileName;
                ParseFromBuffer(fp.buffer, fp.FileSize);
            }

            fp.Close();
        }

        public void ParseFromBuffer(BinaryReader buffer, long limit)
        {
            ptrMPD = buffer.ReadUInt32();
            lenMPD = buffer.ReadUInt32();
            ptrEnemies = buffer.ReadUInt32();
            lenEnemies = buffer.ReadUInt32();
            ptrTIM = buffer.ReadUInt32();
            lenTIM = buffer.ReadUInt32();
            waveId = buffer.ReadByte();
            numMPD = lenMPD / 8;
            buffer.ReadBytes(7); // padding


            // MPD Section
            if (buffer.BaseStream.Position != ptrMPD)
            {
                buffer.BaseStream.Position = ptrMPD;
            }
            MPD_LBA = new Vector2Int[numMPD];
            for (int i = 0; i < numMPD; i++)
            {
                MPD_LBA[i] = new Vector2Int((int)buffer.ReadUInt32(), (int)buffer.ReadUInt32());
            }

            // ZUD Section
            if (buffer.BaseStream.Position != ptrEnemies)
            {
                buffer.BaseStream.Position = ptrEnemies;
            }

            numEnemies = buffer.ReadUInt32();
            monsters = new ZNDMonster[numEnemies];

            ZUD_LBA = new Vector2Int[numEnemies];
            for (int i = 0; i < numEnemies; i++)
            {
                ZUD_LBA[i] = new Vector2Int((int)buffer.ReadUInt32(), (int)buffer.ReadUInt32());
            }

            ptrEnemies = (uint) buffer.BaseStream.Position;
            for (int i = 0; i < numEnemies; i++)
            {
                buffer.BaseStream.Position = ptrEnemies + i * 0x464;
                monsters[i] = new ZNDMonster(buffer);
            }


            // Textures section
            if (buffer.BaseStream.Position != ptrTIM)
            {
                buffer.BaseStream.Position = ptrTIM;
            }

            lenTIM2 = buffer.ReadUInt32();
            buffer.ReadBytes(12); // padding always 00
            numTIM = buffer.ReadUInt32();
            TIMs = new TIM.TIM[numTIM];
            for (int i = 0; i < numTIM; i++)
            {
                if (buffer.BaseStream.Position + 32 < buffer.BaseStream.Length)
                {
                    uint timLength = buffer.ReadUInt32();
                    TIMs[i] = ScriptableObject.CreateInstance<TIM.TIM>();
                    TIMs[i].name = string.Concat("TIM #", i);
                    TIMs[i].ParseZNDFromBuffer((uint)i, timLength, buffer);
                }
            }
        }

        public Texture2D GetTexture(string materialRef)
        {
            string[] subs = materialRef.Split('@');
            uint textureId = uint.Parse(subs[0]);
            uint palettePtr = uint.Parse(subs[1]);
            TIM.TIM textureTIM = GetTIM(textureId);
            return textureTIM.GetTextureWithPalette(GetPalette(palettePtr));
        }


        private TIM.TIM GetTIM(uint idx)
        {
            uint x = (idx * 64) % 1024;
            //uint y = (uint)Math.Floor((decimal)(idx * 64 / 1024));
            foreach (TIM.TIM tim in TIMs)
            {
                if (tim.fx == x) return tim;
            }
            return TIMs[0];
        }

        private Palette GetPalette(uint palettePtr)
        {
            uint x = (palettePtr * 16) % 1024;
            uint y = ((palettePtr * 16) / 1024);
            for (uint i = 0; i < TIMs.Length; i++)
            {
                TIM.TIM tim = TIMs[i];
                if (tim.fx <= x && tim.fx + tim.width > x && tim.fy <= y && tim.fy + tim.height > y)
                {
                    uint ox = x - tim.fx;
                    uint oy = y - tim.fy;
                    uint dec = oy * tim.width + ox;
                    Palette palette = new Palette(16);
                    uint k = 0;
                    for (uint j = dec; j < dec+16; j++)
                    {
                        palette.colors[k] = tim.palettes[0].colors[j];
                        k++;
                    }
                    /*
                    if (tim.palettes == null)
                    {
                        // its bad
                        tim.palettes = new Palette[1];
                        tim.palettes[0] = new Palette(16);
                    }
                    */
                    return palette;
                }
            }

            return null;
        }
    }
}
