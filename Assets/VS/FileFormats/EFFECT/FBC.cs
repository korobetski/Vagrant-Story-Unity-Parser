using System;
using System.IO;
using UnityEngine;
using VS.FileFormats.TIM;
using VS.Utils;

namespace VS.FileFormats.EFFECT
{
    [Serializable]
    public class FBC: ScriptableObject
    {
        public Palette[] palettes;

        private bool _only16colors = false;

        public void ParseFromFile(string filepath)
        {

            FileParser fp = new FileParser();
            fp.Read(filepath);

            // in EFFECT/E***_1.FBC
            if (fp.Ext == "FBC")
            {
                ParseFromBuffer(fp.buffer, fp.FileSize);
            }

            fp.Close();
        }

        private void ParseFromBuffer(BinaryReader buffer, long fileSize)
        {
            uint numPallet = (uint)Mathf.RoundToInt(fileSize / 512);

            palettes = new Palette[numPallet];

            for (uint i = 0; i < numPallet; i++)
            {
                palettes[i] = new Palette(256);
                for (uint j = 0; j < 256; j++)
                {
                    palettes[i].colors[j] = (ToolBox.BitColorConverter(buffer.ReadUInt16()));
                    if(j == 16 && palettes[i].colors[j] == Color.clear)
                    {
                        _only16colors = true;
                    }
                }
            }
        }

        public bool only16Colors { get => _only16colors; }
    }
}