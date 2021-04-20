using System;
using UnityEngine;

namespace VS.FileFormats.EVT
{
    [Serializable]
    public class Text
    {
        public byte[] raw;
        [TextArea (4, 8)]
        public string text;
    }
}
