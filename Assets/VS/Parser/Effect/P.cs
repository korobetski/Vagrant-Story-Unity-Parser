using System.Collections.Generic;
using UnityEngine;

namespace VS.Parser.Effect
{
    public class P : FileParser
    {


        public P(string path)
        {
            UseDebug = false;
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
            int n3 = buffer.ReadByte(); // ptr
            int n4 = buffer.ReadByte(); // ptr
            int n5 = buffer.ReadByte(); // 08
            int n6 = buffer.ReadByte(); // 00
            int p = buffer.ReadInt16(); // 0000

            if (UseDebug)
            {
                Debug.Log(string.Concat("n1 : ", n1, "  n2 : ", n2, "  wid : ", wid, "  hei : ", hei, "  n3 : ", n3, "  n4 : ", n4, "  n5 : ", n5, "  n6 : ", n6, "  p : ", p));
            }

            // frames ? 
            int ptr1 = n4 * 256 + n3 + 4;
            int loop = (int)(ptr1 - buffer.BaseStream.Position) / 4;

            if (UseDebug)
            {
                Debug.Log("## loop " + loop + " times");
            }

            for (int i = 0; i < loop; i++)
            {
                if (buffer.BaseStream.Position + 4 <= buffer.BaseStream.Length)
                {

                    short layer = buffer.ReadInt16(); // 0100
                    sbyte id = buffer.ReadSByte(); // frame id
                    buffer.ReadByte(); // 00

                    //if (UseDebug) Debug.Log(string.Concat("layer : ", layer, "   ID : ", id));
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

            int i1, i2, i3, i4, i5, i6, i7, i8, i9, iA, iB, iC;

            for (int i = 0; i < loop; i++)
            {
                if (buffer.BaseStream.Position + 24 <= buffer.BaseStream.Length)
                {
                    i1 = buffer.ReadInt16();
                    i2 = buffer.ReadInt16();
                    //int i3 = buffer.ReadInt16();
                    //int i4 = buffer.ReadInt16();
                    byte b1 = buffer.ReadByte();
                    byte b2 = buffer.ReadByte();
                    byte b3 = buffer.ReadByte();
                    byte b4 = buffer.ReadByte();
                    i5 = buffer.ReadInt16();
                    i6 = buffer.ReadInt16();
                    i7 = buffer.ReadInt16();
                    i8 = buffer.ReadInt16();
                    i9 = buffer.ReadInt16();
                    iA = buffer.ReadInt16();
                    iB = buffer.ReadInt16();
                    iC = buffer.ReadInt16();

                    /*
                    if (UseDebug) Debug.Log(string.Concat("i1 : ", i1, "   i2 : ", i2, "   b1 : ", b1,
                        "   b2 : ", b2, "   b3 : ", b3, "   b4 : ", b4, "   i5 : ", i5,
                        "   i6 : ", i6, "   i7 : ", i7, "   i8 : ", i8, "   i9 : ", i9,
                        "   iA : ", iA, "   iB : ", iB, "   iC : ", iC
                        ));
                        */
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

            for (int i = 0; i < loop; i++)
            {
                if (buffer.BaseStream.Position + 4 <= buffer.BaseStream.Length)
                {
                    ushort s1 = buffer.ReadUInt16();
                    byte s2 = buffer.ReadByte();
                    byte s3 = buffer.ReadByte();
                    if (UseDebug)
                    {
                        Debug.Log(string.Concat("s1 : ", s1, "   s2 : ", s2, "   s3 : ", s3));
                    }
                }
                else
                {
                    return;
                }
            }
            if (UseDebug)
            {
                Debug.Log("## 3 loop ends " + buffer.BaseStream.Position); // 70C
            }


            /*
            for (int i = 0; i < 24; i++)
            {
                if (buffer.BaseStream.Position + 8 <= buffer.BaseStream.Length)
                {
                    if (UseDebug)
                    {
                        Debug.Log("## 8B  ->    " + BitConverter.ToString(buffer.ReadBytes(8)));
                    }
                }
                else
                {
                    return;
                }
            }
            if (UseDebug)
            {
                Debug.Log("## 4 loop ends " + buffer.BaseStream.Position);
            }

            for (int i = 0; i < 24; i++)
            {
                if (buffer.BaseStream.Position + 8 <= buffer.BaseStream.Length)
                {
                    if (UseDebug)
                    {
                        Debug.Log("## 8B  ->    " + BitConverter.ToString(buffer.ReadBytes(8)));
                    }
                }
                else
                {
                    return;
                }
            }

            if (UseDebug)
            {
                Debug.Log("## 5 loop ends " + buffer.BaseStream.Position);
                Debug.Log("## 16B  ->    " + BitConverter.ToString(buffer.ReadBytes(16)));
            }

            for (int i = 0; i < loop; i++)
            {
                if (buffer.BaseStream.Position + 19 <= buffer.BaseStream.Length)
                {
                    byte[] b = buffer.ReadBytes(19);
                }
                else
                {
                    return;
                }
            }
            if (UseDebug)
            {
                Debug.Log("## 6 loop ends " + buffer.BaseStream.Position);
                Debug.Log(FileName);
                Debug.Log("## AKAO Section" + buffer.BaseStream.Position);
            }
            */

            // AKAO Section for the moment i'll crawl all the file to find the magic word AKAO (41 4B 41 4F)
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
                buffer.BaseStream.Position = akaoStarts[i];
                akaoFx.Parse(buffer, AKAO.UNKNOWN, akaoEnds[i]);
            }


        }
    }
}