using System.Collections.Generic;

/*
05 - Talos Feldspear
06 - Titan Malachite
07 - Sylphid Topaz
08 - Djinn Amber
09 - Salamander Ruby
0A - Ifrit Carnelian
0B - Gnome Emerald
0C - Dao Moonstone
0D - Undine Jasper
0E - Marid Aquamarine
0F - Angel Pearl
10 - Seraphim Diamond
11 - Morlock Jet
12 - Berial Black Pearl
13 - Haeralis
14 - Orlandu
15 - Orion
16 - Ogmius
17 - Iocus
18 - Balvus
19 - Trinity
1A - Beowulf
1B - Dragonite
1C - Sigguld
1D - Demonia
1E - Altema
1F - Polaris
20 - Basivalin
21 - Galerian
22 - Vedivier
23 - Berion
24 - Gervin
25 - Tertia
26 - Lancer
27 - Arturos
28 - Braveheart
29 - Hellraiser
2A - Nightkiller
2B - Manabreaker
2C - Powerfist
2D - Brainshield
2E - Speedster
30 - Silent Queen
31 - Dark Queen
32 - Death Queen
33 - White Queen
*/


namespace VS.Data
{

    public enum GemType
    {
        NONE,
        Talos_Feldspear,
        Titan_Malachite,
        Sylphid_Topaz,
        Djinn_Amber,
        Salamander_Ruby,
        Ifrit_Carnelian,
        Gnome_Emerald,
        Dao_Moonstone,
        Undine_Jasper,
        Marid_Aquamarine,
        Angel_Pearl,
        Seraphim_Diamond,
        Morlock_Jet,
        Berial_Black_Pearl,
        Haeralis,
        Orlandu,
        Orion,
        Ogmius,
        Iocus,
        Balvus,
        Trinity,
        Beowulf,
        Dragonite,
        Sigguld,
        Demonia,
        Altema,
        Polaris,
        Basivalin,
        Galerian,
        Vedivier,
        Berion,
        Gervin,
        Tertia,
        Lancer,
        Arturos,
        Braveheart,
        Hellraiser,
        Nightkiller,
        Manabreaker,
        Powerfist,
        Brainshield,
        Speedster,
        Silent_Queen,
        Dark_Queen,
        Death_Queen,
        White_Queen,
    }
    public class Gem : ClassAffinityType
    {

        public static List<string> slist = new List<string>() { "Talos_Feldspear",
        "Titan_Malachite",
        "Sylphid_Topaz",
        "Djinn_Amber",
        "Salamander_Ruby",
        "Ifrit_Carnelian",
        "Gnome_Emerald",
        "Dao_Moonstone",
        "Undine_Jasper",
        "Marid_Aquamarine",
        "Angel_Pearl",
        "Seraphim_Diamond",
        "Morlock_Jet",
        "Berial_Black_Pearl",
        "Haeralis",
        "Orlandu",
        "Orion",
        "Ogmius",
        "Iocus",
        "Balvus",
        "Trinity",
        "Beowulf",
        "Dragonite",
        "Sigguld",
        "Demonia",
        "Altema",
        "Polaris",
        "Basivalin",
        "Galerian",
        "Vedivier",
        "Berion",
        "Gervin",
        "Tertia",
        "Lancer",
        "Arturos",
        "Braveheart",
        "Hellraiser",
        "Nightkiller",
        "Manabreaker",
        "Powerfist",
        "Brainshield",
        "Speedster",
        "Silent_Queen",
        "Dark_Queen",
        "Death_Queen",
        "White_Queen"};
        public static List<Gem> list = new List<Gem>();
        public static List<Gem> GemList()
        {
            IEnumerable<Gem> l = new List<Gem>()
            {
                new Gem("Talos Feldspear", new ClassAffinityType("MAJOR", "PHYSICAL")),
                new Gem("Titan Malachite", new ClassAffinityType("MINOR", "PHYSICAL")),
                new Gem("Sylphid Topaz", new ClassAffinityType("MINOR", "AIR")),
                new Gem("Djinn Amber", new ClassAffinityType("MAJOR", "AIR")),
                new Gem("Salamander Ruby", new ClassAffinityType("MINOR", "FIRE")),
                new Gem("Ifrit Carnelian", new ClassAffinityType("MAJOR", "FIRE")),
                new Gem("Gnome Emerald", new ClassAffinityType("MINOR", "EARTH")),
                new Gem("Dao Moonstone", new ClassAffinityType("MAJOR", "EARTH")),
                new Gem("Undine Jasper", new ClassAffinityType("MINOR", "WATER")),
                new Gem("Marid Aquamarine", new ClassAffinityType("MAJOR", "WATER")),
                new Gem("Angel Pearl", new ClassAffinityType("MINOR", "LIGHT")),
                new Gem("Seraphim Diamond", new ClassAffinityType("MAJOR", "LIGHT")),
                new Gem("Morlock Jet", new ClassAffinityType("MINOR", "DARK")),
                new Gem("Berial_Black_Pearl", new ClassAffinityType("MAJOR", "DARK")),
                new Gem("Haeralis", new ClassAffinityType("MINOR", "HUMAN")),
                new Gem("Orlandu", new ClassAffinityType("MAJOR", "HUMAN")),
                new Gem("Orion", new ClassAffinityType("MINOR", "BEAST")),
                new Gem("Ogmius", new ClassAffinityType("MAJOR", "BEAST")),
                new Gem("Iocus", new ClassAffinityType("MINOR", "UNDEAD")),
                new Gem("Balvus", new ClassAffinityType("MAJOR", "UNDEAD")),
                new Gem("Trinity", new ClassAffinityType("MINOR", "PHANTOM")),
                new Gem("Beowulf", new ClassAffinityType("MAJOR", "PHANTOM")),
                new Gem("Dragonite", new ClassAffinityType("MINOR", "DRAGON")),
                new Gem("Sigguld", new ClassAffinityType("MAJOR", "DRAGON")),
                new Gem("Demonia", new ClassAffinityType("MINOR", "EVIL")),
                new Gem("Altema", new ClassAffinityType("MAJOR", "EVIL")),
                new Gem("Polaris", new ClassAffinityType("EXCEP", "POLARIS")),
                new Gem("Basivalin", new ClassAffinityType("EXCEP", "BASIVALEN")),
                new Gem("Galerian", new ClassAffinityType("EXCEP", "GALERIAN")),
                new Gem("Vedivier", new ClassAffinityType("EXCEP", "VEDIVIER")),
                new Gem("Berion", new ClassAffinityType("EXCEP", "BERION")),
                new Gem("Gervin", new ClassAffinityType("EXCEP", "GERVIN")),
                new Gem("Tertia", new ClassAffinityType("EXCEP", "TERTIA")),
                new Gem("Lancer", new ClassAffinityType("EXCEP", "LANCER")),
                new Gem("Arturos", new ClassAffinityType("EXCEP", "ARTUROS")),
                new Gem("Braveheart", new ClassAffinityType("ATTACK", "HIT")),
                new Gem("Hellraiser", new ClassAffinityType("ATTACK", "SPELL")),
                new Gem("Nightkiller", new ClassAffinityType("PROTECTION", "EVADE")),
                new Gem("Manabreaker", new ClassAffinityType("PROTECTION", "SPELLEVADE")),
                new Gem("Powerfist", new ClassAffinityType("PROTECTION", "EVADESTR")),
                new Gem("Brainshield", new ClassAffinityType("PROTECTION", "EVADEINT")),
                new Gem("Speedster", new ClassAffinityType("PROTECTION", "EVADEAGI")),
                new Gem("Silent_Queen", new ClassAffinityType("PROTECTION", "EVADESILENT")),
                new Gem("Dark_Queen", new ClassAffinityType("PROTECTION", "EVADEPARA")),
                new Gem("Death_Queen", new ClassAffinityType("PROTECTION", "EVADEPOISON")),
                new Gem("White_Queen", new ClassAffinityType("PROTECTION", "EVADENUMB"))
            };
            list.AddRange(l);

            return list;
        }
        public static Gem FindByName(string name)
        {
            if (Gem.list.Count == 0)
            {
                GemList();
            }

            foreach (Gem gem in Gem.list)
            {
                if (name == gem.name)
                {
                    return gem;
                }
            }
            return null;
        }

        public uint type = 0; // 0 = standard gem, 1 = attack gem, 2 = protection gem
        public string description = "";

        public Gem()
        {

        }
        public Gem(string _name)
        {
            name = _name;
        }
        public Gem(string _name = "Gem", ClassAffinityType cat = null)
        {
            name = _name;
            Human = cat.Human;
            Beast = cat.Beast;
            Undead = cat.Undead;
            Phantom = cat.Phantom;
            Dragon = cat.Dragon;
            Evil = cat.Evil;
            Physical = cat.Physical;
            Air = cat.Air;
            Fire = cat.Fire;
            Earth = cat.Earth;
            Water = cat.Water;
            Light = cat.Light;
            Dark = cat.Dark;
            Blunt = cat.Blunt;
            Edged = cat.Edged;
            Piercing = cat.Piercing;
            Range = cat.Range;
            Risk = cat.Risk;
            STR = cat.STR;
            INT = cat.INT;
            AGI = cat.AGI;
        }
    }
}