using System;
using UnityEngine;

namespace VS.FileFormats.MPD
{
    [Serializable]
    public class MPDLight
    {
        public Color32[] colors;
        public short[] datas;

        public MPDLight()
        {
            colors = new Color32[3];
            datas = new short[10];
        }
    }
}
