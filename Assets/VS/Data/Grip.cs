using System.Collections.Generic;
using UnityEngine;

/*
 * 
60 - Short Hilt
61 - Swept Hilt
62 - Cross Guard
63 - Knuckle Guard
64 - Counter Guard
65 - Side Ring
66 - Power Palm
67 - Murderers Hilt
68 - Spirial Hilt
69 - Wooden Grip
6A - Sand Face
6B - Czekan Type
6C - Sarissa Grip
6D - Gendarme
6E - Heavy Grip
6F - Runkasyle
70 - Bhuj Type
71 - Grimoire Grip
72 - Elephant
73 - Wooden Pole
74 - Spiculum Pole
75 - Winged Pole
76 - Framea Pole
77 - Ahlspies
78 - Spiral Pole
79 - Simple Bolt
7A - Steel Bolt
7B - Javelin Bolt
7C - Falarica Bolt
7D - Stone Bullet
7E - Sonic Bullet
*/

namespace VS.Data
{
    public enum GripType
    {
        //01 - Grip For Daggers/Swords/Great Swords
        Short_Hilt,
        Swept_Hilt,
        Cross_Guard,
        Knuckle_Guard,
        Counter_Guard,
        Side_Ring,
        Power_Palm,
        Murderers_Hilt,
        Spirial_Hilt,
        //02 - Grip For Axes/Maces/Staves
        Wooden_Grip,
        Sand_Face,
        Czekan_Type,
        Sarissa_Grip,
        Gendarme,
        Heavy_Grip,
        Runkasyle,
        Bhuj_Type,
        Grimoire_Grip,
        Elephant,
        //03 - Grip For Polearms
        Wooden_Pole,
        Spiculum_Pole,
        Winged_Pole,
        Framea_Pole,
        Ahlspies,
        Spiral_Pole,
        //04 - Grip For Crossbows
        Simple_Bolt,
        Steel_Bolt,
        Javelin_Bolt,
        Falarica_Bolt,
        Stone_Bullet,
        Sonic_Bullet,
    }
    public enum GripTypeGuard
    {
        //01 - Grip For Daggers/Swords/Great Swords
        Short_Hilt,
        Swept_Hilt,
        Cross_Guard,
        Knuckle_Guard,
        Counter_Guard,
        Side_Ring,
        Power_Palm,
        Murderers_Hilt,
        Spirial_Hilt
    }
    public enum GripTypeGrip
    {
        //02 - Grip For Axes/Maces/Staves
        Wooden_Grip,
        Sand_Face,
        Czekan_Type,
        Sarissa_Grip,
        Gendarme,
        Heavy_Grip,
        Runkasyle,
        Bhuj_Type,
        Grimoire_Grip,
        Elephant
    }
    public enum GripTypePole
    {
        //03 - Grip For Polearms
        Wooden_Pole,
        Spiculum_Pole,
        Winged_Pole,
        Framea_Pole,
        Ahlspies,
        Spiral_Pole,
    }
    public enum GripTypeBolt
    {
        //04 - Grip For Crossbows
        Simple_Bolt,
        Steel_Bolt,
        Javelin_Bolt,
        Falarica_Bolt,
        Stone_Bullet,
        Sonic_Bullet
    }


    public class Grip
    {
        public static List<Grip> list = new List<Grip>();
        public static List<Grip> GripList()
        {
            IEnumerable<Grip> l = new List<Grip>() { new Grip("Short Hilt", 0, 1, 0, -1, 0, 4, 1, 0),
            new Grip("Swept Hilt", 0, 1, 1, -1, 0, 2, 4, 0),
            new Grip("Cross Guard", 1, 2, 1, -1, 8, 6, 2, 0),
            new Grip("Knuckle Guard", 2, 2, 2, -2, 0, 5, 9, 0),
            new Grip("Counter Guard", 1, 3, 2, -2, 0, 8, 7, 0),
            new Grip("Side Ring", 2, 3, 3, -2, 10, 12, 12, 0),
            new Grip("Power Palm", 3, 4, 3, -3, 0, 15, 12, 0),
            new Grip("Murderer's Hilt", 2, 4, 4, -3, 0, 13, 17, 0),
            new Grip("Spiral Hilt", 3, 5, 4, -3, 20, 20, 20, 0),
            // type 1
            new Grip("Wooden Grip", 0, 1, 0, -2, 5, 1, 0, 1),
            new Grip("Sand Face", 1, 1, 2, -2, 3, 6, 0, 1),
            new Grip("Czekan Type", 0, 2, 1, -2, 8, 4, 0, 1),
            new Grip("Sarissa Grip", 1, 2, 2, -3, 6, 9, 0, 1),
            new Grip("Heavy Grip", 1, 3, 1, -3, 6, 15, 0, 1),
            new Grip("Gendarme", 2, 3, 2, -3, 13, 5, 0, 1),
            new Grip("Runkastyle", 2, 4, 3, -3, 17, 7, 0, 1),
            new Grip("Grimoire Grip", 2, 1, 8, -4, 21, 9, 0, 1),
            new Grip("Bhuj Type", 3, 5, 1, -4, 8, 19, 0, 1),
            new Grip("Elephant", 3, 6, 3, -4, 11, 22, 0, 1),
            // type 2
            new Grip("Wooden Pole", 0, 1, 0, -3, 11, 0, 1, 2),
            new Grip("Winged Pole", 0, 3, 2, -4, 2, 6, 16, 2),
            new Grip("Spiculum Pole", 1, 2, 1, -3, 2, 12, 4, 2),
            new Grip("Ahlspies", 1, 0, 0, 0, 10, 14, 12, 2),
            new Grip("Framea Pole", 2, 4, 3, -4, 16, 4, 10, 2),
            new Grip("Spiral Pole", 3, 6, 5, -5, 15, 6, 21, 2),
            // type 3
            new Grip("Simple Bolt", 0, 1, 0, -1, 1, 0, 10, 3),
            new Grip("Steel Bolt", 0, 2, 0, -1, 2, 0, 13, 3),
            new Grip("Javelin Bolt", 1, 3, 1, -1, 17, 0, 2, 3),
            new Grip("Falarica Bolt", 1, 4, 1, -1, 3, 0, 20, 3),
            new Grip("Stone Bullet", 1, 2, 0, -2, 23, 0, 4, 3),
            new Grip("Sonic Bullet", 1, 4, 2, -2, 5, 0, 25, 3)};
            list.AddRange(l);

            return list;
        }

        public static Grip FindByName(string name)
        {
            if (Grip.list.Count == 0)
            {
                GripList();
            }

            foreach (Grip grp in Grip.list)
            {
                if (name == grp.name)
                {
                    return grp;
                }
            }
            return null;
        }

        [SerializeField]
        private string _name;
        [SerializeField]
        private string _desc = "";
        [SerializeField]
        private uint _gemSlots;
        [SerializeField]
        private int _STR;
        [SerializeField]
        private int _INT;
        [SerializeField]
        private int _AGI;
        [SerializeField]
        private int _blunt;
        [SerializeField]
        private int _edged;
        [SerializeField]
        private int _piercing;
        public uint _type; // { Guard = 0, Grip = 1, Pole = 2, Bolt = 3};

        public string name { get => _name; set => _name = value; }
        public uint GemSlots { get => _gemSlots; set => _gemSlots = value; }
        public int STR { get => _STR; set => _STR = value; }
        public int INT { get => _INT; set => _INT = value; }
        public int AGI { get => _AGI; set => _AGI = value; }
        public int Blunt { get => _blunt; set => _blunt = value; }
        public int Edged { get => _edged; set => _edged = value; }
        public int Piercing { get => _piercing; set => _piercing = value; }

        public override string ToString()
        {
            return "Grip : " + _name + " Description : " + _desc+" Gems : " + _gemSlots + " [STR:" + _STR + "|INT:" + _INT + "|AGI:" + _AGI + "]";
        }

        public Grip(string name = "grip", uint _gs = 0, int _str = 0, int _int = 0, int _agi = 0, int blunt = 0, int edged = 0, int piercing = 0, uint type = 0)
        {
            _name = name;
            _gemSlots = _gs;
            _STR = _str;
            _INT = _int;
            _AGI = _agi;
            _blunt = blunt;
            _edged = edged;
            _piercing = piercing;
            _type = type;
        }
    }
}