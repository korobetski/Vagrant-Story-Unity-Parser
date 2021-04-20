using System;
using UnityEngine;

namespace VS.FileFormats.TIM
{
    [Serializable]
    public class Palette
    {
        public Color32[] colors;

        public Palette(uint numColors)
        {
            colors = new Color32[numColors];
        }
    }
}