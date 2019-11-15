using UnityEngine;
using VS.Utils;

namespace VS.Parser.Effect
{
    public class FBC : FileParser
    {
        private Color32[,] _pallets;


        public FBC(string path)
        {
            Parse(path);
        }

        public void Parse(string filePath)
        {
            if (!filePath.EndsWith(".FBC"))
            {
                return;
            }

            PreParse(filePath);

            uint numPallet = (uint)Mathf.RoundToInt(FileSize / 512);

            _pallets = new Color32[numPallet, 256];

            for (uint i = 0; i < numPallet; i++)
            {
                for (uint j = 0; j < 256; j++)
                {
                    _pallets[i, j] = (ToolBox.BitColorConverter(buffer.ReadUInt16()));
                }
            }
        }

        internal Color32[,] GetPallets()
        {
            return _pallets;
        }
    }
}