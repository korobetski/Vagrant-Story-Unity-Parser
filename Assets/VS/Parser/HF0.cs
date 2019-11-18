using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VS.Utils;

namespace VS.Parser
{


    // HF -> Ingame Help 
    public class HF0 : FileParser
    {
        public void Parse(string filePath)
        {
            if (!filePath.EndsWith(".HF0"))
            {
                return;
            }

            PreParse(filePath);

            List<byte> bname = new List<byte>();

            while (buffer.BaseStream.Position < buffer.BaseStream.Length)
            {
                byte b = buffer.ReadByte();
                if (b == 0xE7)
                {
                    string inam = L10n.Translate(bname.ToArray());
                    Debug.Log(string.Concat(inam));

                    bname = new List<byte>();
                }
                else
                {
                    bname.Add(b);
                }
            }

        }
    }
}
