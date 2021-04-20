using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VS.FileFormats.EVT
{
    public class EVT:ScriptableObject
    {
        public string Filename;

        public ushort length;
        public ushort ptrDialogue;
        public ushort ptr2;
        public ushort ptr3;

        public OPCode[] opCodes;

        public ushort numBubbles;
        public ushort[] bubblesPtrs;
        public Text[] bubbles;
        public byte[] datas2;
        public byte[] datas3;

        public void ParseFromFile(string filepath)
        {
            FileParser fp = new FileParser();
            fp.Read(filepath);

            // in EVENT/0***.EVT
            if (fp.Ext == "EVT")
            {
                Filename = fp.FileName;
                ParseFromBuffer(fp.buffer, fp.FileSize);
            }

            fp.Close();
        }

        public void ParseFromBuffer(BinaryReader buffer, long limit)
        {
            long basePtr = buffer.BaseStream.Position;

            length = buffer.ReadUInt16();
            ptrDialogue = buffer.ReadUInt16();
            ptr2 = buffer.ReadUInt16();
            ptr3 = buffer.ReadUInt16();
            buffer.ReadUInt32(); // padding
            buffer.ReadUInt32(); // padding

            if (limit != (basePtr + length))
            {
                limit = basePtr + length;
            }

            if (length > 0)
            {

                // OPcode section
                List<OPCode> codes = new List<OPCode>();
                while (buffer.BaseStream.Position < basePtr + ptrDialogue - 1)
                {
                    OPCode code = new OPCode();
                    code.OP = buffer.ReadByte();
                    code.name = ((Enums.OP.Type)code.OP).ToString();
                    // there are params when oplen > 1
                    if ((OPCode.OPLen[code.OP] - 1) > 1)
                    {
                        code.parameters = buffer.ReadBytes(OPCode.OPLen[code.OP] - 1);
                    }
                    if (OPCode.OPLen[code.OP] == 0)
                    {
                        //Debug.LogWarning("Invalid OP "+code.OP+" in "+Filename+".EVT");
                    }
                    codes.Add(code);
                }
                opCodes = codes.ToArray();

                if (ptr2 > ptrDialogue)
                {
                    // there is dialogues
                    buffer.BaseStream.Position = basePtr + ptrDialogue;

                    numBubbles = buffer.ReadUInt16();
                    bubblesPtrs = new ushort[numBubbles];
                    bubblesPtrs[0] = (ushort)(ptrDialogue + numBubbles * 2);

                    if (numBubbles > 1)
                    {
                        // i = 1 because the first bubble doesn't need a ptr, its start just after the header
                        for (int j = 1; j < numBubbles; j++)
                        {
                            bubblesPtrs[j] = (ushort)(ptrDialogue + numBubbles * 2 + buffer.ReadUInt16() + 4);
                        }
                    }
                    // ptr arn't good in the SLES-02755
                    // we do another method

                    bubbles = new Text[numBubbles];

                    if (numBubbles == 1)
                    {
                        bubbles[0] = new Text();
                        bubbles[0].raw = buffer.ReadBytes((int)((basePtr + ptr2) - buffer.BaseStream.Position));
                        bubbles[0].text = Utils.L10n.Translate(bubbles[0].raw);
                    }
                    else
                    {
                        List<byte> bname = new List<byte>();
                        List<Text> texts = new List<Text>();
                        int i = 0;
                        while (buffer.BaseStream.Position < basePtr + ptr2)
                        {
                            byte b = buffer.ReadByte();
                            if (b == 0xE7)
                            {
                                string inam = Utils.L10n.Translate(bname.ToArray());
                                bname.Add(b);
                                Text t = new Text();
                                t.raw = bname.ToArray();
                                t.text = inam;
                                texts.Add(t);
                                bname = new List<byte>();
                                i++;
                            }
                            else
                            {
                                bname.Add(b);
                            }
                        }

                        bubbles = texts.ToArray();
                    }
                }

                if (ptr3 > ptr2)
                {
                    buffer.BaseStream.Position = basePtr + ptr2;
                    datas2 = buffer.ReadBytes(ptr3 - ptr2);
                    // probable a 16 bytes header
                }

                if (limit > ptr3)
                {
                    buffer.BaseStream.Position = basePtr + ptr3;
                    datas3 = buffer.ReadBytes((int)(length - ptr3));
                    // probable a 16 bytes header
                }
            }
        }
    }
}
