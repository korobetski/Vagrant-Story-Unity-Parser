using System;
using UnityEngine;

namespace VS.Parser.Effect
{
    [Serializable]
    public class EffectFrame
    {

        public ushort id;
        public ushort texid;
        public Rect texRect;
        public Rect destRect;
        public Sprite sprite;

        public EffectFrame(ushort _id, ushort _texid = 1)
        {
            id = _id;
            texid = _texid;
        }

        public void SetTexRect(byte x, byte y, byte w, byte h)
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