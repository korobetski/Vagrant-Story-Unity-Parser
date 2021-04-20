using UnityEngine;
using VS.FileFormats.ITEM;

namespace VS.FileFormats.MPD
{
    public class Treasure:ScriptableObject
    {
        public Weapon weapon;
        public Blade blade;
        public Grip grip;
        public Shield shield;
        public Armor armor1;
        public Armor armor2;
        public Armor accessory;
        public Gem gem;
        public MiscItem[] miscItems = new MiscItem[4];
    }
}
