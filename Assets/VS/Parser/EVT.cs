/*
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VS.Serializable;
using VS.Utils;

namespace VS.Parser
{
    public class EVT : FileParser
    {
        public Serializable.EVT so;
        public long limit;

        public EVT()
        {
        }

        public void CoreParse()
        {
            so = ScriptableObject.CreateInstance<Serializable.EVT>();

            long basePtr = buffer.BaseStream.Position;

            so.length = buffer.ReadUInt16();
            so.ptrDialogue = buffer.ReadUInt16();
            so.ptr2 = buffer.ReadUInt16();
            so.ptr3 = buffer.ReadUInt16();
            buffer.ReadUInt32(); // padding
            buffer.ReadUInt32(); // padding

            limit = basePtr + so.length;

            if (so.length > 0)
            {

                // OPcode section
                List<OPCode> codes = new List<OPCode>();
                while (buffer.BaseStream.Position < basePtr+so.ptrDialogue -1)
                {
                    OPCode code = new OPCode();
                    code.OP = buffer.ReadByte();
                    // there are params when oplen > 1
                    if ((OPCode.OPLen[code.OP]-1) > 1)
                    {
                        code.parameters = buffer.ReadBytes(OPCode.OPLen[code.OP] - 1);
                    }
                    
                    codes.Add(code);
                }
                so.opCodes = codes.ToArray();

                if (so.ptr2 > so.ptrDialogue)
                {
                    // there is dialogues
                    buffer.BaseStream.Position = basePtr + so.ptrDialogue;

                    so.numBubbles = buffer.ReadUInt16();
                    so.bubblesPtrs = new ushort[so.numBubbles];
                    so.bubblesPtrs[0] = (ushort)(so.ptrDialogue + so.numBubbles * 2);

                    if (so.numBubbles > 1)
                    {
                        // i = 1 because the first bubble doesn't need a ptr, its start just after the header
                        for (int j = 1; j < so.numBubbles; j++)
                        {
                            so.bubblesPtrs[j] = (ushort)(so.ptrDialogue + so.numBubbles * 2 + buffer.ReadUInt16() + 4);
                        }
                    }
                    // ptr arn't good in the SLES-02755
                    // so we do another method

                    so.bubbles = new VSTextBubble[so.numBubbles];

                    if (so.numBubbles == 1)
                    {
                        so.bubbles[0] = new VSTextBubble();
                        so.bubbles[0].raw = buffer.ReadBytes((int)((basePtr + so.ptr2) - buffer.BaseStream.Position));
                        so.bubbles[0].text = Utils.L10n.Translate(so.bubbles[0].raw);
                    } else
                    {
                        List<byte> bname = new List<byte>();
                        int i = 0;
                        while (buffer.BaseStream.Position < basePtr + so.ptr2)
                        {
                            byte b = buffer.ReadByte();
                            if (b == 0xE7)
                            {
                                string inam = Utils.L10n.Translate(bname.ToArray());
                                bname.Add(b);
                                so.bubbles[i] = new VSTextBubble();
                                so.bubbles[i].raw = bname.ToArray();
                                so.bubbles[i].text = inam;
                                bname = new List<byte>();
                                i++;
                            }
                            else
                            {
                                bname.Add(b);
                            }
                        }
                    }
                }

                if (so.ptr3 > so.ptr2)
                {
                    buffer.BaseStream.Position = basePtr + so.ptr2;
                    so.datas2 = buffer.ReadBytes(so.ptr3 - so.ptr2);
                    // probable a 16 bytes header
                }

                if (limit > so.ptr3)
                {
                    buffer.BaseStream.Position = basePtr + so.ptr3;
                    so.datas3 = buffer.ReadBytes((int)(so.length - so.ptr3));
                    // probable a 16 bytes header
                }
            }
        }


        public void Parse(string filePath)
        {
            if (!filePath.EndsWith(".EVT"))
            {
                return;
            }

            PreParse(filePath);
            CoreParse();

            ToolBox.DirExNorCreate("Assets/");
            ToolBox.DirExNorCreate("Assets/Resources/");
            ToolBox.DirExNorCreate("Assets/Resources/Serialized/");
            ToolBox.DirExNorCreate("Assets/Resources/Serialized/Events/");
            AssetDatabase.DeleteAsset("Assets/Resources/Serialized/Events/" + FileName + ".EVT.yaml.asset");
            AssetDatabase.CreateAsset(so, "Assets/Resources/Serialized/Events/" + FileName + ".EVT.yaml.asset");
            AssetDatabase.SaveAssets();
        }
    }
}
*/