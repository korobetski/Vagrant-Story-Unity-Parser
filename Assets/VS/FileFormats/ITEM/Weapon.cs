using System;

namespace VS.FileFormats.ITEM
{
    [Serializable]
    public class Weapon
    {
        public byte[] rawName;
        public string name;
        public Blade blade;
        public Grip grip;
        public Gem[] gems = new Gem[3];
    }
}
