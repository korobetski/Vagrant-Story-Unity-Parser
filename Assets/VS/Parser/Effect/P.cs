using UnityEngine;

namespace VS.Parser.Effect
{
    public class P:FileParser
    {

        public P(string path)
        {
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
            int wid = buffer.ReadInt16(); // 0100
            int hei = buffer.ReadInt16(); // 0100
            int n3 = buffer.ReadByte(); // ptr
            int n4 = buffer.ReadByte(); // ptr
            int n5 = buffer.ReadByte(); // 08
            int n6 = buffer.ReadByte(); // 00
            int p = buffer.ReadInt16(); // 0000

            // frames ? 
            int ptr1 = n4 * 256 + n3 + 4;
            int loop = (int)(ptr1 - buffer.BaseStream.Position) / 4;
            for (int i = 0; i < loop; i++)
            {
                if (buffer.BaseStream.Position + 4 <= buffer.BaseStream.Length)
                {
                    buffer.ReadInt16(); // 0100
                    buffer.ReadByte(); // frame id
                    buffer.ReadByte(); // 00
                }
            }
        }
    }
}