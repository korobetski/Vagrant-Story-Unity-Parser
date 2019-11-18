using UnityEngine;
using VS.Utils;

namespace VS.Parser
{
    public class EVT : FileParser
    {


        public EVT(string path)
        {
            UseDebug = true;
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
                    string text = L10n.Translate(buffer.ReadBytes((int)(ptrEndDial - buffer.BaseStream.Position)));
                    if (UseDebug)
                    {
                        Debug.Log(text);
                    }
                }
            }

        }
    }
}
