using System;
using UnityEngine;

namespace VS.FileFormats.EFFECT
{
    [Serializable]
    public class PSprite
    {

        public ushort slots;
        public ushort id;
        public ushort texid;
        public ushort paletteId;

        public byte pal;
        public byte uk2;
        public byte tex; // Tex id helper
        public byte uk4; // padding

        public Rect texRect;
        public Rect destRect;
        public Sprite sprite;

        public PSprite(ushort _slots, ushort _id)
        {
            slots = _slots;
            id = _id;
        }

        public void SetTexRect(int x, int y, int w, int h)
        {
            texRect = new Rect(x, y, w, h);
        }

        public void SetDestRect(short xmin, short xmax, short ymin, short ymax)
        {
            destRect = new Rect();
            destRect.xMin = xmin;
            destRect.yMin = ymin;
            destRect.xMax = xmax;
            destRect.yMax = ymax;
        }

    }
}