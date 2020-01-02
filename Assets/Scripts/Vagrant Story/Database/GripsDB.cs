using VagrantStory.Items;

namespace VagrantStory.Database
{

    public class GripsDB
    {
        public enum eGripCategories { None, Guard, Grip, Pole, Bolt };
        public enum eGuards { None = 0, Short_Hilt = 1, Swept_Hilt, Cross_Guard, Knuckle_Guard, Counter_Guard, Side_Ring, Power_Palm, Murderers_Hilt, Spiral_Hilt }
        public enum eGrips { None = 0, Wooden_Grip = 10, Sand_Face, Czekan_Type, Sarissa_Grip, Gendarme, Heavy_Grip, Runkastyle, Bhuj_Type, Grimoire_Grip, Elephant }
        public enum ePoles { None = 0, Wooden_Pole = 20, Spiculum_Pole, Winged_Pole, Framea_Pole, Ahlspies, Spiral_Pole }
        public enum eBolts { None = 0, Simple_Bolt = 26, Steel_Bolt, Javelin_Bolt, Falarica_Bolt, Stone_Bullet, Sonic_Bullet }

        // type 1 -> dagger, sword, great sword
        public static Grip Short_Hilt = new Grip("Chêne", "Poignée en bois recouverte de cuir.", 0, 1, 0, -1, 0, 4, 1, 0);
        public static Grip Swept_Hilt = new Grip("Métal", "Poignée à garde de métal.", 0, 1, 1, -1, 0, 2, 4, 0);
        public static Grip Cross_Guard = new Grip("Cross", "Poignée à garde croisée.", 1, 2, 1, -1, 8, 6, 2, 0);
        public static Grip Knuckle_Guard = new Grip("Handy", "Poignée capuchon protégeant les mains.", 2, 2, 2, -2, 0, 5, 9, 0);
        public static Grip Counter_Guard = new Grip("Short Guard", "Poignée-défense à lame courte.", 1, 3, 2, -2, 0, 8, 7, 0);
        public static Grip Side_Ring = new Grip("British", "Poignée anglaise de protection.", 2, 3, 3, -2, 10, 12, 12, 0);
        public static Grip Power_Palm = new Grip("Griffure", "Poignée à gant.", 3, 4, 3, -3, 0, 15, 12, 0);
        public static Grip Murderers_Hilt = new Grip("Murder", "Poignée légère des assassins.", 2, 4, 4, -3, 0, 13, 17, 0);
        public static Grip Spiral_Hilt = new Grip("Kildéan", "Poignée gravée d'anciennes runes Kildéannes.", 3, 5, 4, -3, 20, 20, 20, 0);
        // type 2 -> mace, axe, staff, great axe, heavy mace
        public static Grip Wooden_Grip = new Grip("Châtaignier", "Poignée légère en bois.", 0, 1, 0, -2, 5, 1, 0, 1);
        public static Grip Sand_Face = new Grip("Noyer", "Poignée en bois recouverte de résine.", 1, 1, 2, -2, 3, 6, 0, 1);
        public static Grip Czekan_Type = new Grip("Hêtre d'Acier", "Poignée en bois recouverte d'acier.", 0, 2, 1, -2, 8, 4, 0, 1);
        public static Grip Sarissa_Grip = new Grip("Bouleau", "Poignée légère en bois renforcé.", 1, 2, 2, -3, 6, 9, 0, 1);
        public static Grip Heavy_Grip = new Grip("Alliage", "Poignée de cérémonie finement ciselée.", 1, 3, 1, -3, 6, 15, 0, 1);
        public static Grip Gendarme = new Grip("Morailles", "Poignée de force en acier.", 2, 3, 2, -3, 13, 5, 0, 1);
        public static Grip Runkastyle = new Grip("Fonte", "Poignée de cérémonie en métal.", 2, 4, 3, -3, 17, 7, 0, 1);
        public static Grip Grimoire_Grip = new Grip("Tricoise", "Poignée en métal aussi légère que le bois.", 2, 1, 8, -4, 21, 9, 0, 1);
        public static Grip Bhuj_Type = new Grip("Gargamène", "Poignée de sorcier.", 3, 5, 1, -4, 8, 19, 0, 1);
        public static Grip Elephant = new Grip("Acajou Maillé", "Puissante poignée en mailles Damascus.", 3, 6, 3, -4, 11, 22, 0, 1);
        // type 3 -> polearm
        public static Grip Wooden_Pole = new Grip("Chypre", "Très légère poignée de bois.", 0, 1, 0, -3, 11, 0, 1, 2);
        public static Grip Winged_Pole = new Grip("Tenaille", "Poignée d'acier, très résistante.", 0, 3, 2, -4, 2, 6, 16, 2);
        public static Grip Spiculum_Pole = new Grip("Steel", "Poignée à lames de bourreau.", 1, 2, 1, -3, 2, 12, 4, 2);
        public static Grip Ahlspies = new Grip("Amalgame", "Poignée croisée augmentant l'impact.", 1, 0, 0, 0, 10, 14, 12, 2);
        public static Grip Framea_Pole = new Grip("Laiton", "Poignée en métal léger.", 2, 4, 3, -4, 16, 4, 10, 2);
        public static Grip Spiral_Pole = new Grip("Herse", "Poignée en forme de spirale.", 3, 6, 5, -5, 15, 6, 21, 2);
        // type 4 -> crossbow
        public static Grip Simple_Bolt = new Grip("Simple Bolt", "Carreau en bois à pointe d'acier.", 0, 1, 0, -1, 1, 0, 10, 3);
        public static Grip Steel_Bolt = new Grip("Steel Bolt", "Carreau en acier renforcé.", 0, 2, 0, -1, 2, 0, 13, 3);
        public static Grip Javelin_Bolt = new Grip("Heavy Bolt", "Carreau lourd à pointe épaisse.", 1, 3, 1, -1, 17, 0, 2, 3);
        public static Grip Falarica_Bolt = new Grip("Harpoon Bolt", "Carreau à pointe en harpon.", 1, 4, 1, -1, 3, 0, 20, 3);
        public static Grip Stone_Bullet = new Grip("Stone Bullet", "Boulet de pierre à faible impact.", 1, 2, 0, -2, 23, 0, 4, 3);
        public static Grip Sonic_Bullet = new Grip("Steel Bullet", "Bille d'acier à fort impact.", 1, 4, 2, -2, 5, 0, 25, 3);

        public static Grip[] List = new Grip[32] {
            null,
            Short_Hilt, Swept_Hilt, Cross_Guard, Knuckle_Guard, Counter_Guard, Side_Ring, Power_Palm, Murderers_Hilt, Spiral_Hilt,
            Wooden_Grip, Sand_Face, Czekan_Type, Sarissa_Grip, Gendarme, Heavy_Grip, Runkastyle, Bhuj_Type, Grimoire_Grip, Elephant,
            Wooden_Pole, Spiculum_Pole, Winged_Pole, Framea_Pole, Ahlspies, Spiral_Pole,
            Simple_Bolt, Steel_Bolt, Javelin_Bolt, Falarica_Bolt, Stone_Bullet, Sonic_Bullet
        };
    }

}