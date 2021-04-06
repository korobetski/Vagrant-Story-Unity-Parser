using UnityEngine;
using VS.Utils;

namespace VS.Parser
{
    public class EVT : FileParser
    {


        public EVT(string path, bool debug)
        {
            UseDebug = debug;
            Parse(path);
        }


        public void Parse(string filePath)
        {
            if (!filePath.EndsWith(".EVT"))
            {
                return;
            }

            PreParse(filePath);

            ushort ptrEnd = buffer.ReadUInt16();
            ushort ptrDial = buffer.ReadUInt16();
            ushort ptrEndDial = buffer.ReadUInt16();
            ushort ptr4 = buffer.ReadUInt16();
            buffer.ReadUInt32(); // padding
            buffer.ReadUInt32(); // padding
            if (UseDebug)
            {
                Debug.Log(string.Concat("EVENT   ", FileName, "  ptrEnd : ", ptrEnd, "  ptrDial : ", ptrDial, "  ptrEndDial : ", ptrEndDial, "  ptr4 : ", ptr4));
            }

            if (ptrEnd > 0)
            {
                if (ptrEndDial > ptrDial)
                {
                    buffer.BaseStream.Position = ptrDial;

                    ushort numBubbles = buffer.ReadUInt16();
                    ushort[] bubblesPtrs = new ushort[numBubbles+1];

                    bubblesPtrs[0] = (ushort)(ptrDial + numBubbles * 2);
                    if (numBubbles > 1) {
                        // i = 1 because the first bubble doesn't need a ptr, its start just after the header
                        for (int i = 1; i < numBubbles; i++)
                        {
                            bubblesPtrs[i] = (ushort)(ptrDial + numBubbles * 2 + buffer.ReadUInt16() + 4);
                        }
                    }
                    bubblesPtrs[numBubbles] = ptrEndDial;
                    // ptr arn't good in the SLES-02755
                    // so we do another method

                    string text = L10n.Translate(buffer.ReadBytes((int)(ptrEndDial +2 + numBubbles * 2 - ptrDial)));
                    string[] subs = text.Split('|');

                    if (UseDebug)
                    {
                        for (int i = 0; i < numBubbles; i++)
                        {
                            Debug.Log($"{subs[i]}");
                        }
                    }
                }
            }

        }
    }
}
