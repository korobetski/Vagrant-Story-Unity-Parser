using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace VS.Data
{
    public class ClassAffinityType : MonoBehaviour
    {
        private int _Human = 0;
        private int _Beast = 0;
        private int _Undead = 0;
        private int _Phantom = 0;
        private int _Dragon = 0;
        private int _Evil = 0;

        private int _Physical = 0;
        private int _Air = 0;
        private int _Fire = 0;
        private int _Earth = 0;
        private int _Water = 0;
        private int _Light = 0;
        private int _Dark = 0;

        private int _Blunt = 0;
        private int _Edged = 0;
        private int _Piercing = 0;

        private uint _Range = 0;
        private uint _Risk = 0;
        private int _STR = 0;
        private int _INT = 0;
        private int _AGI = 0;

        public Slider humanSlider;
        public Slider beastSlider;
        public Slider undeadSlider;
        public Slider phantomSlider;
        public Slider dragonSlider;
        public Slider evilSlider;

        public Slider physicalSlider;
        public Slider airSlider;
        public Slider fireSlider;
        public Slider earthSlider;
        public Slider waterSlider;
        public Slider lightSlider;
        public Slider darkSlider;

        public Slider bluntSlider;
        public Slider edgedSlider;
        public Slider piercingSlider;

        public Text rangeTxt;
        public Text riskTxt;
        public Text strTxt;
        public Text intTxt;
        public Text agiTxt;

        public ClassAffinityType()
        {
        }
        public ClassAffinityType(int _h, int _b, int _u, int _p, int _d, int _e, int _phy, int _ai, int _fi, int _ea, int _wa, int _li, int _da, int _bl, int _ed, int _pi, uint _ra, uint _ri, int _str, int _int, int _agi)
        {
            Human = _h;
            Beast = _b;
            Undead = _u;
            Phantom = _p;
            Dragon = _d;
            Evil = _e;
            Physical = _phy;
            Air = _ai;
            Fire = _fi;
            Earth = _ea;
            Water = _wa;
            Light = _li;
            Dark = _da;
            Blunt = _bl;
            Edged = _ed;
            Piercing = _pi;
            Range = _ra;
            Risk = _ri;
            STR = _str;
            INT = _int;
            AGI = _agi;
        }

        /// <param name="pow">MINOR, MAJOR, EXCEP, ATTACK, PROTECTION</param>
        /// <param name="type">can be an affinity, a class, or else see Gem.cs GemList</param>
        public ClassAffinityType(string pow, string type)
        {
            List<string> classes = new List<string> { "HUMAN", "BEAST", "UNDEAD", "PHANTOM", "DRAGON", "EVIL" };
            List<string> elementals = new List<string> { "PHYSICAL", "AIR", "FIRE", "EARTH", "WATER", "LIGHT", "DARK" };
            if (pow == "MINOR")
            {
                STR = 2;
                INT = 4;
                AGI = 3;
                switch (type)
                {
                    case "HUMAN":
                        Human = 15;
                        Beast = -3;
                        Undead = -3;
                        break;
                    case "BEAST":
                        Beast = 15;
                        Undead = -3;
                        Phantom = -3;
                        break;
                    case "UNDEAD":
                        Undead = 15;
                        Phantom = -3;
                        Dragon = -3;
                        break;
                    case "PHANTOM":
                        Phantom = 15;
                        Dragon = -3;
                        Evil = -3;
                        break;
                    case "DRAGON":
                        Dragon = 15;
                        Evil = -3;
                        Human = -3;
                        break;
                    case "EVIL":
                        Evil = 15;
                        Human = -3;
                        Beast = -3;
                        break;
                    case "PHYSICAL":
                        Physical = 15;
                        break;
                    case "AIR":
                        Air = 15;
                        Earth = -5;
                        break;
                    case "FIRE":
                        Fire = 15;
                        Water = -5;
                        break;
                    case "EARTH":
                        Earth = 15;
                        Air = -5;
                        break;
                    case "WATER":
                        Water = 15;
                        Earth = -5;
                        break;
                    case "LIGHT":
                        Light = 15;
                        Dark = -5;
                        break;
                    case "DARK":
                        Dark = 15;
                        Light = -5;
                        break;
                }
            }
            if (pow == "MAJOR")
            {
                STR = 1;
                INT = 6;
                AGI = 3;
                if (classes.Contains(type))
                {
                    Human = -3;
                    Beast = -3;
                    Undead = -3;
                    Phantom = -3;
                    Dragon = -3;
                    Evil = -3;
                }
                if (elementals.Contains(type))
                {
                    Physical = -5;
                    Air = -5;
                    Fire = -5;
                    Earth = -5;
                    Water = -5;
                    Light = -5;
                    Dark = -5;
                }
                switch (type)
                {
                    case "HUMAN":
                        Human = 30;
                        Beast = -6;
                        Undead = -6;
                        break;
                    case "BEAST":
                        Beast = 30;
                        Undead = -6;
                        Phantom = -6;
                        break;
                    case "UNDEAD":
                        Undead = 30;
                        Phantom = -6;
                        Dragon = -6;
                        break;
                    case "PHANTOM":
                        Phantom = 30;
                        Dragon = -6;
                        Evil = -6;
                        break;
                    case "DRAGON":
                        Dragon = 30;
                        Evil = -6;
                        Human = -6;
                        break;
                    case "EVIL":
                        Evil = 30;
                        Human = -6;
                        Beast = -6;
                        break;
                    case "PHYSICAL":
                        Physical = 30;
                        break;
                    case "AIR":
                        Air = 30;
                        Earth = -15;
                        break;
                    case "FIRE":
                        Fire = 30;
                        Water = -15;
                        break;
                    case "EARTH":
                        Earth = 30;
                        Air = -15;
                        break;
                    case "WATER":
                        Water = 30;
                        Earth = -15;
                        break;
                    case "LIGHT":
                        Light = 30;
                        Dark = -15;
                        break;
                    case "DARK":
                        Dark = 30;
                        Light = -15;
                        break;
                }
            }
            if (pow == "EXCEP")
            {
                if (type == "POLARIS" || type == "BASIVALEN" || type == "GALERIAN")
                {
                    STR = -3;
                    INT = 12;
                    Physical = 20;
                    Fire = -10;
                    Water = -10;
                    Air = -10;
                    Earth = -10;
                    Light = -10;
                    Dark = -10;
                }
                switch (type)
                {
                    case "POLARIS":
                        Air = 20;
                        Earth = 20;
                        break;
                    case "BASIVALEN":
                        Fire = 20;
                        Water = 20;
                        break;
                    case "GALERIAN":
                        Light = 20;
                        Dark = 20;
                        break;
                    case "VEDIVIER":
                        STR = 1;
                        INT = 1;
                        AGI = 1;
                        Physical = 5;
                        Fire = 5;
                        Water = 5;
                        Air = 5;
                        Earth = 5;
                        Light = 5;
                        Dark = 5;
                        Human = 5;
                        Beast = 5;
                        Undead = 5;
                        Phantom = 5;
                        Dragon = 5;
                        Evil = 5;
                        break;
                    case "BERION":
                        STR = 2;
                        INT = 3;
                        AGI = 2;
                        Physical = 10;
                        Fire = 10;
                        Water = 10;
                        Air = 10;
                        Earth = 10;
                        Light = 10;
                        Dark = 10;
                        break;
                    case "GERVIN":
                        STR = 3;
                        INT = 6;
                        AGI = 3;
                        Physical = 15;
                        Fire = 15;
                        Water = 15;
                        Air = 15;
                        Earth = 15;
                        Light = 15;
                        Dark = 15;
                        Human = 15;
                        Beast = 15;
                        Undead = 15;
                        Phantom = 15;
                        Dragon = 15;
                        Evil = 15;
                        break;
                    case "TERTIA":
                        STR = 4;
                        INT = 9;
                        AGI = 4;
                        Physical = 20;
                        Fire = 20;
                        Water = 20;
                        Air = 20;
                        Earth = 20;
                        Light = 20;
                        Dark = 20;
                        Human = 20;
                        Beast = 20;
                        Undead = 20;
                        Phantom = 20;
                        Dragon = 20;
                        Evil = 20;
                        break;
                    case "LANCER":
                        STR = 5;
                        INT = 12;
                        AGI = 5;
                        Physical = 25;
                        Fire = 25;
                        Water = 25;
                        Air = 25;
                        Earth = 25;
                        Light = 25;
                        Dark = 25;
                        Human = 25;
                        Beast = 25;
                        Undead = 25;
                        Phantom = 25;
                        Dragon = 25;
                        Evil = 25;
                        break;
                    case "ARTUROS":
                        STR = 8;
                        INT = 15;
                        AGI = 8;
                        Physical = 30;
                        Fire = 30;
                        Water = 30;
                        Air = 30;
                        Earth = 30;
                        Light = 30;
                        Dark = 30;
                        Human = 30;
                        Beast = 30;
                        Undead = 30;
                        Phantom = 30;
                        Dragon = 30;
                        Evil = 30;
                        break;
                }
            }
            if (pow == "ATTACK" || pow == "PROTECTION")
            {
                Physical = 3;
                Fire = 3;
                Water = 3;
                Air = 3;
                Earth = 3;
                Light = 3;
                Dark = 3;
                Human = 3;
                Beast = 3;
                Undead = 3;
                Phantom = 3;
                Dragon = 3;
                Evil = 3;
                if (pow == "ATTACK")
                {
                    STR = 2;
                    INT = 0;
                    AGI = 5;
                    if (type == "HIT") { }
                    if (type == "SPELL") { }
                }
                if (pow == "PROTECTION")
                {
                    AGI = 1;
                }

                if (type == "EVADE")
                {
                    STR = 2;
                    INT = 0;
                    AGI = 5;
                }
                if (type == "SPELLEVADE")
                {
                    STR = 0;
                    INT = 3;
                    AGI = 5;
                }
            }
        }

        public int Human { get => _Human; set { _Human = value; if (humanSlider) { humanSlider.value = value; } } }
        public int Beast { get => _Beast; set { _Beast = value; if (beastSlider) { beastSlider.value = value; } } }
        public int Undead { get => _Undead; set { _Undead = value; if (undeadSlider) { undeadSlider.value = value; } } }
        public int Phantom { get => _Phantom; set { _Phantom = value; if (phantomSlider) { phantomSlider.value = value; } } }
        public int Dragon { get => _Dragon; set { _Dragon = value; if (dragonSlider) { dragonSlider.value = value; } } }
        public int Evil { get => _Evil; set { _Evil = value; if (evilSlider) { evilSlider.value = value; } } }
        public int Physical { get => _Physical; set { _Physical = value; if (physicalSlider) { physicalSlider.value = value; } } }
        public int Air { get => _Air; set { _Air = value; if (airSlider) { airSlider.value = value; } } }
        public int Fire { get => _Fire; set { _Fire = value; if (fireSlider) { fireSlider.value = value; } } }
        public int Earth { get => _Earth; set { _Earth = value; if (earthSlider) { earthSlider.value = value; } } }
        public int Water { get => _Water; set { _Water = value; if (waterSlider) { waterSlider.value = value; } } }
        public int Light { get => _Light; set { _Light = value; if (lightSlider) { lightSlider.value = value; } } }
        public int Dark { get => _Dark; set { _Dark = value; if (darkSlider) { darkSlider.value = value; } } }
        public int Blunt { get => _Blunt; set { _Blunt = value; if (bluntSlider) { bluntSlider.value = value; } } }
        public int Edged { get => _Edged; set { _Edged = value; if (edgedSlider) { edgedSlider.value = value; } } }
        public int Piercing { get => _Piercing; set { _Piercing = value; if (piercingSlider) { piercingSlider.value = value; } } }
        public uint Range { get => _Range; set { _Range = value; if (rangeTxt) { rangeTxt.text = value.ToString(); } } }
        public uint Risk { get => _Risk; set { _Risk = value; if (riskTxt) { riskTxt.text = value.ToString(); } } }
        public int STR { get => _STR; set { _STR = value; if (strTxt) { strTxt.text = value.ToString(); } } }
        public int INT { get => _INT; set { _INT = value; if (intTxt) { intTxt.text = value.ToString(); } } }
        public int AGI { get => _AGI; set { _AGI = value; if (agiTxt) { agiTxt.text = value.ToString(); } } }


        public static ClassAffinityType operator +(ClassAffinityType lhs, ClassAffinityType rhs)
        {
            lhs.Human += rhs.Human;
            lhs.Beast += rhs.Beast;
            lhs.Undead += rhs.Undead;
            lhs.Phantom += rhs.Phantom;
            lhs.Dragon += rhs.Dragon;
            lhs.Evil += rhs.Evil;
            lhs.Physical += rhs.Physical;
            lhs.Air += rhs.Air;
            lhs.Fire += rhs.Fire;
            lhs.Earth += rhs.Earth;
            lhs.Water += rhs.Water;
            lhs.Light += rhs.Light;
            lhs.Dark += rhs.Dark;
            lhs.Blunt += rhs.Blunt;
            lhs.Edged += rhs.Edged;
            lhs.Piercing += rhs.Piercing;
            lhs.Range += rhs.Range;
            lhs.Risk += rhs.Risk;
            lhs.STR += rhs.STR;
            lhs.INT += rhs.INT;
            lhs.AGI += rhs.AGI;
            return lhs;
        }
        public void RAZ()
        {
            Human = 0;
            Beast = 0;
            Undead = 0;
            Phantom = 0;
            Dragon = 0;
            Evil = 0;
            Physical = 0;
            Air = 0;
            Fire = 0;
            Earth = 0;
            Water = 0;
            Light = 0;
            Dark = 0;
            Blunt = 0;
            Edged = 0;
            Piercing = 0;
            Range = 0;
            Risk = 0;
            STR = 0;
            INT = 0;
            AGI = 0;
        }
    }
}
