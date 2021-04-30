using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VS.FileFormats.EFFECT
{
    public class P : ScriptableObject
    {
        public ushort n1;
        public ushort wid;
        public ushort hei;
        public ushort framePtr;
        public ushort n2;

        public PSprite[] sprites;

        public List<PChunk> chunks;

        public void ParseFromFile(string filepath)
        {

            FileParser fp = new FileParser();
            fp.Read(filepath);

            // in EFFECT/E***.P
            if (fp.Ext == "P")
            {
                ParseFromBuffer(fp.buffer, fp.FileSize);
            }

            fp.Close();
        }

        private void ParseFromBuffer(BinaryReader buffer, long fileSize)
        {
            n1 = buffer.ReadUInt16();
            wid = buffer.ReadUInt16(); // 0100
            hei = buffer.ReadUInt16(); // 0100
            framePtr = buffer.ReadUInt16(); // ptr
            n2 = buffer.ReadUInt16(); // 0800
            buffer.ReadUInt16(); // padding

            int ptr1 = framePtr + 4;
            int loop = (int)(ptr1 - buffer.BaseStream.Position) / 4;
            if (framePtr == 0) loop = 0;

            List<PSprite> _sprites = new List<PSprite>();
            PSprite lastSprite = new PSprite(0,0);
            for (int i = 0; i < loop; i++)
            {
                if (buffer.BaseStream.Position + 4 <= buffer.BaseStream.Length)
                {
                    ushort slots = buffer.ReadUInt16();
                    ushort id = buffer.ReadUInt16();
                    if (id == (lastSprite.id + lastSprite.slots))
                    {
                        lastSprite = new PSprite(slots, id);
                        _sprites.Add(lastSprite);
                    } else
                    {
                        buffer.BaseStream.Position -= 4;
                        break;
                    }
                }
                else
                {
                    return;
                }
            }

            sprites = _sprites.ToArray();

            for (int i = 0; i < sprites.Length; i++)
            {
                if (buffer.BaseStream.Position + 24 <= buffer.BaseStream.Length)
                {
                    sprites[i].pal = buffer.ReadByte(); // palette helper
                    sprites[i].uk2 = buffer.ReadByte(); // most of the time 0xBC
                    sprites[i].tex = buffer.ReadByte(); // Tex id helper
                    sprites[i].uk4 = buffer.ReadByte(); // padding


                    if (sprites[i].pal == 0x70) sprites[i].paletteId = 0;
                    else if (sprites[i].pal == 0xB0) sprites[i].paletteId = 1;
                    else if (sprites[i].pal == 0xF0) sprites[i].paletteId = 2;
                    else if (sprites[i].pal == 0x30) sprites[i].paletteId = 3;

                    if (sprites[i].tex == 0x38 || sprites[i].tex == 0xB9 || sprites[i].tex == 0xD9)
                    {
                        sprites[i].texid = 0;
                    }
                    if (sprites[i].tex == 0xBA || sprites[i].tex == 0xDA)
                    {
                        sprites[i].texid = 1;
                    }
                    if (sprites[i].tex == 0xBB || sprites[i].tex == 0xDB)
                    {
                        sprites[i].texid = 2;
                    }
                    if (sprites[i].tex == 0xBC || sprites[i].tex == 0xDC)
                    {
                        sprites[i].texid = 3;
                    }
                    if (sprites[i].tex == 0xBD || sprites[i].tex == 0xDD)
                    {
                        sprites[i].texid = 4;
                    }


                    byte posx = buffer.ReadByte();
                    byte posy = buffer.ReadByte();
                    byte width = buffer.ReadByte();
                    byte height = buffer.ReadByte();

                    // Rect vertices
                    short x1 = buffer.ReadInt16();
                    short y1 = buffer.ReadInt16();
                    short x2 = buffer.ReadInt16();
                    short y2 = buffer.ReadInt16();
                    short x3 = buffer.ReadInt16();
                    short y3 = buffer.ReadInt16();
                    short x4 = buffer.ReadInt16();
                    short y4 = buffer.ReadInt16();

                    sprites[i].SetTexRect(posx, posy, width, height);
                    sprites[i].SetDestRect(x1, x2, y1, y3);
                }
                else
                {
                    return;
                }
            }

            Debug.Log(string.Concat("buffer.BaseStream.Position : ", buffer.BaseStream.Position));


            // TODO : lot of works to do here

            chunks = new List<PChunk>();

            // Section3
            PChunk sec3 = new PChunk();
            sec3.pointer = (ushort)buffer.BaseStream.Position;
            sec3.length = buffer.ReadUInt16();
            sec3.type = buffer.ReadUInt16();
            // pointer are relative to this point ?
            sec3.num = buffer.ReadUInt16();
            sec3.pad = buffer.ReadUInt16();
            sec3.pointers = new ushort[sec3.num + 1];
            for (int i = 0; i < sec3.num; i++)
            {
                sec3.pointers[i] = (ushort)(sec3.pointer + 4 + buffer.ReadUInt16());
            }
            sec3.pointers[sec3.num] = (ushort)(sec3.pointer + sec3.length);
            // 8 bytes align
            int rem = (int)buffer.BaseStream.Position % 8;
            if (rem != 0) buffer.ReadBytes(8 - rem);

            sec3.items = new PChunkItem[sec3.num];
            for (int i = 0; i < sec3.num; i++)
            {
                sec3.items[i] = new PChunkItem();
                int len = Mathf.RoundToInt((sec3.pointers[i + 1] - sec3.pointers[i]) / 2);

                sec3.items[i].d = new ushort[len];
                for (int j = 0; j < len; j++)
                {
                    sec3.items[i].d[j] = buffer.ReadUInt16();
                }
            }
            chunks.Add(sec3);

            // Section4
            PChunk sec4 = new PChunk();
            sec4.pointer = (ushort)buffer.BaseStream.Position;
            sec4.length = buffer.ReadUInt16();
            sec4.type = buffer.ReadUInt16();
            // pointer are relative to this point ?
            sec4.num = buffer.ReadUInt16();
            sec4.pad = buffer.ReadUInt16();
            sec4.pointers = new ushort[sec4.num + 1];
            for (int i = 0; i < sec4.num; i++)
            {
                sec4.pointers[i] = (ushort)(sec4.pointer + 4 + buffer.ReadUInt16());
            }
            sec4.pointers[sec4.num] = (ushort)(sec4.pointer + sec4.length);
            // 8 bytes align
            rem = (int)buffer.BaseStream.Position % 8;
            if (rem != 0) buffer.ReadBytes(8 - rem);

            sec4.items = new PChunkItem[sec4.num];
            for (int i = 0; i < sec4.num; i++)
            {
                sec4.items[i] = new PChunkItem();
                int len = Mathf.RoundToInt((sec4.pointers[i + 1] - sec4.pointers[i]) / 2);
                sec4.items[i].d = new ushort[len];
                for (int j = 0; j < len; j++)
                {
                    sec4.items[i].d[j] = buffer.ReadUInt16();
                }
            }
            chunks.Add(sec4);


            // AKAO Section for the moment i'll crawl all the file to find the magic word AKAO (41 4B 41 4F)
            // AKAO instructions seems to be in an older version and then there is a sample
            /*
            List<long> akaoStarts = new List<long>();
            List<long> akaoEnds = new List<long>();
            while (buffer.BaseStream.Position < buffer.BaseStream.Length)
            {
                if (buffer.ReadByte() == 0x41)
                {
                    if (buffer.ReadByte() == 0x4B)
                    {
                        if (buffer.ReadByte() == 0x41)
                        {
                            if (buffer.ReadByte() == 0x4F)
                            {
                                if (akaoStarts.Count > 0)
                                {
                                    akaoEnds.Add(buffer.BaseStream.Position - 4);
                                }
                                akaoStarts.Add(buffer.BaseStream.Position - 4);
                            }
                        }
                    }
                }
            }
            akaoEnds.Add(buffer.BaseStream.Length);
            for (int i = 0; i < akaoStarts.Count; i++)
            {
                AKAO akaoFx = new AKAO();
                akaoFx.FileName = string.Concat(FileName, "_akao_", i);
                akaoFx.UseDebug = false;
                buffer.BaseStream.Position = akaoStarts[i];
                akaoFx.Parse(buffer, AKAO.UNKNOWN, akaoEnds[i]);
            }
            */
        }
    }
}