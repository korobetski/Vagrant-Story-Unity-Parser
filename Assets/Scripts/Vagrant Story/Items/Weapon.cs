using System;
using System.Collections.Generic;
using UnityEngine;
using VagrantStory.Core;

namespace VagrantStory.Items
{

    public class Weapon : Item
    {
        public Blade blade;
        public Grip grip;
        public List<Gem> gems;
        public Statistics statistics;
        public ushort DP = 0;
        public ushort MaxDP = 0;
        public ushort PP = 0;
        public ushort MaxPP = 0;

        public Weapon(string _name, Blade _blade, Grip _grip, ushort _dp, ushort _pp, List<Gem> _gems = null)
        {
            gems = new List<Gem>();
            statistics = new Statistics();
            name = _name;
            blade = _blade;
            grip = _grip;

            MaxDP = _dp;
            DP = _dp;
            MaxPP = _pp;
            PP = 0;

            statistics += blade.statistics;
            statistics += grip.statistics;
            foreach (Gem gem in gems)
            {
                statistics += gem.statistics;
            }
        }

        public GameObject GameObject
        {
            get
            {
                return GameObject.Instantiate<GameObject>(Resources.Load<GameObject>(string.Concat("Prefabs/Weapons/", BitConverter.ToString(new byte[] { blade.wepID }))));
            }
        }
    }

}