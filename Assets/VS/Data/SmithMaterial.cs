using System.Collections.Generic;

/*
 * 
01 - Wood
02 - Leather
03 - Bronze
04 - Iron
05 - Hagane
06 - Silver
07 - Damascus
*/

namespace VS.Data
{

    public enum SmithMaterialType
    {
        Wood,
        Leather,
        Bronze,
        Iron,
        Hagane,
        Silver,
        Damascus,
    }
    public class SmithMaterial : ClassAffinityType
    {
        public static List<SmithMaterial> list = new List<SmithMaterial>(){
                null,
                SmithMaterial.Wood,
                SmithMaterial.Leather,
                SmithMaterial.Bronze,
                SmithMaterial.Iron,
                SmithMaterial.Hagane,
                SmithMaterial.Silver,
                SmithMaterial.Damascus
            };
        public static SmithMaterial Wood = new SmithMaterial("Wood", 0, 0, 0, 0, 0, 0, 4, -6, 8, 8, -6, -4, -4, 5, 8, 0);
        public static SmithMaterial Leather = new SmithMaterial("Leather", 0, 0, 0, 0, 0, 0, 2, 5, 5, -1, -1, -5, -5, 1, 6, 0);
        public static SmithMaterial Bronze = new SmithMaterial("Bronze", -1, -1, 2, -1, -1, -5, 8, -5, -5, 3, 3, -2, -2, 3, 2, -2);
        public static SmithMaterial Iron = new SmithMaterial("Iron", 1, 1, -2, 1, 1, 0, 10, 0, -4, -4, 0, -1, -1, 5, 2, -2);
        public static SmithMaterial Hagane = new SmithMaterial("Hagane", 5, 5, 1, 0, 5, 5, 14, 3, 3, -5, -5, -5, -5, 6, 3, -1);
        public static SmithMaterial Silver = new SmithMaterial("Silver", 0, 0, 20, 15, 0, 5, 5, -5, -5, -5, -5, 20, -20, 4, 2, -1);
        public static SmithMaterial Damascus = new SmithMaterial("Damascus", 10, 10, -2, 0, 10, 10, 20, -5, -5, -5, -5, 20, -20, 9, 4, -1);


        private string _name;

        public string Name { get => _name; set => _name = value; }

        public SmithMaterial()
        {

        }
        public SmithMaterial(string _name, int _h, int _b, int _u, int _p, int _d, int _e, int _phy, int _ai, int _fi, int _ea, int _wa, int _li, int _da, int _str, int _int, int _agi)
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
            STR = _str;
            INT = _int;
            AGI = _agi;
        }
    }
}