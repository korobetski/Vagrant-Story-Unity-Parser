using System;

namespace VS.FileFormats.GEOM
{
    [Serializable]
    public class Line
    {
        public byte[] verticesId;
        public ushort pad;

        public Line()
        {
            verticesId = new byte[2];
        }
    }
}