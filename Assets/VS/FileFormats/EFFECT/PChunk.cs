using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VS.FileFormats.EFFECT
{
    [Serializable]
    public class PChunk
    {
        public ushort pointer;
        public ushort length;
        public ushort type;
        public ushort num;
        public ushort pad;

        public ushort[] pointers;
        public PChunkItem[] items;
    }
}
