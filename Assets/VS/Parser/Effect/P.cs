using System;
using System.Collections.Generic;
using UnityEngine;

namespace VS.Parser.Effect
{
    public class P : FileParser
    {
        public List<EffectFrame> frames;

        public P(string path)
        {
            UseDebug = true;
            frames = new List<EffectFrame>();
            Parse(path);
        }


        public void Parse(string filePath)
        {
            if (!filePath.EndsWith(".P"))
            {
                return;
            }

            PreParse(filePath);

            int n1 = buffer.ReadByte();
            int n2 = buffer.ReadByte(); // related to frame length, maybe duration in sec
            int wid = buffer.ReadUInt16(); // 0100
            int hei = buffer.ReadUInt16(); // 0100
            int framePtr = buffer.ReadUInt16(); // ptr
            int n5 = buffer.ReadByte(); // 08
            int n6 = buffer.ReadByte(); // 00
            int p = buffer.ReadInt16(); // 0000

            if (UseDebug)
            {
                Debug.Log(string.Concat("n1 : ", n1, "  n2 : ", n2, "  wid : ", wid, "  hei : ", hei, "  framePtr : ", framePtr, "  n5 : ", n5, "  n6 : ", n6, "  p : ", p));
            }

            int ptr1 = framePtr + 4;
            int loop = (int)(ptr1 - buffer.BaseStream.Position) / 4;

            if (UseDebug)
            {
                Debug.Log("## loop " + loop + " times");
            }

            for (int i = 0; i < loop; i++)
            {
                if (buffer.BaseStream.Position + 4 <= buffer.BaseStream.Length)
                {

                    ushort tex = buffer.ReadUInt16(); // texture id (maybe not)
                    ushort id = buffer.ReadUInt16(); // frame id

                    frames.Add(new EffectFrame(id, tex));

                    if (UseDebug)
                    {
                        Debug.Log(string.Concat("tex : ", tex, "   ID : ", id));
                    }
                }
                else
                {
                    return;
                }
            }
            if (UseDebug)
            {
                Debug.Log("## 1 loop ends " + buffer.BaseStream.Position);
            }

            for (int i = 0; i < loop; i++)
            {
                if (buffer.BaseStream.Position + 24 <= buffer.BaseStream.Length)
                {
                    byte uk1 = buffer.ReadByte();
                    byte uk2 = buffer.ReadByte();
                    byte uk3 = buffer.ReadByte(); // Tex id helper
                    byte uk4 = buffer.ReadByte(); // padding

                    if (uk3 == 0xB9 || uk3 == 0xD9)
                    {
                        frames[i].texid = 1;
                    }
                    if (uk3 == 0xBA || uk3 == 0xDA)
                    {
                        frames[i].texid = 2;
                    }
                    if (uk3 == 0xBB || uk3 == 0xDB)
                    {
                        frames[i].texid = 3;
                    }
                    if (uk3 == 0xBC || uk3 == 0xDC)
                    {
                        frames[i].texid = 4;
                    }
                    if (uk3 == 0xBD || uk3 == 0xDD)
                    {
                        frames[i].texid = 5;
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

                    frames[i].SetTexRect(posx, posy, width, height);
                    frames[i].SetDestRect(x1, x2, y1, y3);

                    if (UseDebug)
                    {
                        Debug.Log(string.Concat("# ", i, "  uk1 : ", uk1, "  ,  uk2 : ", uk2, "  ,  uk3 : ", uk3, "(", BitConverter.ToString(new byte[]{uk3}) , ")", "  ,  uk4 : ", uk4, "  ,  posx : ", posx, "  ,  posy : ", posy, "  ,  width : ", width, "  ,  height : ", height, "  |  x1 : ", x1, "  ,  y1 : ", y1, "  ,  x2 : ", x2, "  ,  y2 : ", y2, "  ,  x3 : ", x3, "  ,  y3 : ", y3, "  ,  x4 : ", x4, "  ,  y4 : ", y4));
                    }
                }
                else
                {
                    return;
                }
            }

            if (UseDebug)
            {
                Debug.Log("## 2 loop ends " + buffer.BaseStream.Position);
            }

            // Lots of unknown things here


            // AKAO Section for the moment i'll crawl all the file to find the magic word AKAO (41 4B 41 4F)
            // AKAO instructions seems to be in an older version and then there is a sample
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
                akaoFx.UseDebug = true;
                buffer.BaseStream.Position = akaoStarts[i];
                akaoFx.Parse(buffer, AKAO.UNKNOWN, akaoEnds[i]);
            }


        }
    }
}