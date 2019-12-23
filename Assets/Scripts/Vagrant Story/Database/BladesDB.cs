using VagrantStory.Items;


namespace VagrantStory.Database
{
    public class BladesDB
    {
        public enum eBladeCategory { None, Dagger, Sword, Great_Sword, Axe, Mace, Great_Axe, Staff, Heavy_Mace, Polearm, Crossbow, Shield };

        public enum eDaggerBlades
        {
            None = 0,
            Battle_Knife = 1,
            Scramasax,
            Dirk,
            Throwing_Knife,
            Kudi,
            Cinquedea,
            Kris,
            Hatchet,
            Khukuri,
            Baselard,
            Stiletto,
            Jamadhar
        };
        public enum eSwordBlades
        {
            None = 0,
            Spatha = 13,
            Scimitar,
            Rapier,
            Short_Sword,
            Firangi,
            Shamshir,
            Falchion,
            Shotel,
            Khora,
            Khophish,
            Wakizashi,
            Rhomphaia
        };
        public enum eGreatSwordBlades
        {
            None = 0,
            Broad_Sword = 25,
            Norse_Sword,
            Katana,
            Executioner,
            Claymore,
            Schiavona,
            Bastard_Sword,
            Nodachi,
            Rune_Blade,
            Holy_Wind
        };
        public enum eAxeBlades
        {
            None = 0,
            Hand_Axe = 35,
            Battle_Axe,
            Francisca,
            Tabarzin,
            Chamkaq,
            Tabar,
            Bullova,
            Crescent
        };
        public enum eMaceBlades
        {
            None = 0,
            Goblin_Club = 43,
            Spiked_Club,
            Ball_Mace,
            Footmans_Mace,
            Morning_Star,
            War_Hammer,
            Bec_de_Corbin,
            War_Maul

        };
        public enum eGreatAxeBlades
        {
            None = 0,
            Guisarme = 51,
            Large_Crescent,
            Sabre_Halberd,
            Balbriggin,
            Double_Blade,
            Halberd
        };
        public enum eStaffBlades
        {
            None = 0,
            Wizard_Staff = 57,
            Clergy_Rod,
            Summoner_Baton,
            Shamanic_Staff,
            Bishops_Crosier,
            Sages_Cane
        };
        public enum eHeavyMaceBlades
        {
            None = 0,
            Langdebeve = 63,
            Sabre_Mace,
            Footmans_Mace,
            Gloomwing,
            Mjolnir,
            Griever,
            Destroyer,
            Hand_Of_Light
        };
        public enum ePolearmBlades
        {
            None = 0,
            Spear = 71,
            Glaive,
            Scorpion,
            Corcesca,
            Trident,
            Awl_Pike,
            Boar_Spear,
            Fauchard,
            Voulge,
            Pole_Axe,
            Bardysh,
            Brandestoc
        };
        public enum eCrossbowBlades
        {
            None = 0,
            Gastraph_Bow = 83,
            Light_Crossbow,
            Target_Bow,
            Windlass,
            Cranquein,
            Lug_Crossbow,
            Siege_Bow,
            Arbalest
        };

        public static Blade Battle_Knife = new Blade("Dagger", "Dague simple, pour novice.", 1, 1, 1, 2, 1, 4, 0, 0, 1, 0);
        public static Blade Scramasax = new Blade("Stabber", "Poignard ordinaire.", 2, 3, 1, 3, 1, 6, 0, 0, 1, 0);
        public static Blade Dirk = new Blade("Combat King", "Bon poignard de combat.", 3, 4, 1, 2, 1, 8, 0, 0, 1, 0);
        public static Blade Throwing_Knife = new Blade("Throwing Knife", "Poignard de lancer, effilé comme un rasoir.", 4, 9, 1, 3, 1, 10, 0, 0, 1, 0);
        public static Blade Kudi = new Blade("Kudi", "Dague à lame incurvée.", 5, 5, 1, 2, 1, 12, 0, 0, 1, 0);
        public static Blade Cinquedea = new Blade("Dague cannelée", "Dague ornée d'une lame gravée.", 6, 7, 1, 3, 1, 14, 0, -1, 1, 0);
        public static Blade Kris = new Blade("Suméria", "Dague Sumérienne, à lame ondulée.", 7, 8, 1, 3, 1, 16, 0, -1, 1, 0);
        public static Blade Hatchet = new Blade("Hatchet", "Petite dague-marteau.", 8, 10, 1, 2, 1, 18, 0, -1, 2, 1);
        public static Blade Khukuri = new Blade("Kukuri", "Poignard à lame lourde et incurvée.", 9, 11, 1, 2, 1, 20, 0, -1, 2, 1);
        public static Blade Baselard = new Blade("Vicelard", "Poignard de combat à fine lame plate.", 10, 6, 1, 2, 1, 22, 0, -1, 2, 1);
        public static Blade Stiletto = new Blade("Stiletto", "Dague d'assassin à fine lame plate.", 11, 12, 1, 2, 2, 24, 0, -1, 2, 1);
        public static Blade Jamadhar = new Blade("Kubléa", "Dague Kubléenne à 3 lames.", 12, 2, 1, 3, 2, 26, 0, -1, 2, 1);

        public static Blade Spatha = new Blade("Brutal", "Sabre à lame large.", 13, 14, 2, 2, 1, 5, 0, -1, 3, 2);
        public static Blade Scimitar = new Blade("Cimeterre", "Sabre à grande lame incurvée.", 14, 16, 2, 2, 1, 7, 0, -1, 3, 2);
        public static Blade Rapier = new Blade("Rapière", "Sabre à fine lame (arme des Crimson Blades).", 15, 15, 2, 3, 1, 9, 0, -1, 3, 2);
        public static Blade Short_Sword = new Blade("Duel", "Sabre à lame large, pour combats rapprochés.", 16, 13, 2, 2, 1, 11, 0, -2, 3, 2);
        public static Blade Firangi = new Blade("Firangi", "Sabre oriental à lame longue.", 17, 22, 2, 2, 1, 13, 0, -2, 3, 2);
        public static Blade Shamshir = new Blade("Shamshir", "Sabre à simple tranchant et lame incurvée.", 18, 21, 2, 2, 1, 15, 0, -2, 3, 2);
        public static Blade Falchion = new Blade("Strike", "Sabre court pour attaque éclair.", 19, 17, 2, 2, 2, 17, 0, -3, 3, 2);
        public static Blade Shotel = new Blade("Lame rivetée", "Sabre oriental à lame en forme de 'S'.", 20, 20, 2, 2, 2, 19, 0, -3, 3, 2);
        public static Blade Khora = new Blade("Kora", "Sabre lourd élargi à la pointe.", 21, 18, 2, 2, 2, 21, 0, -3, 3, 2);
        public static Blade Khophish = new Blade("Khopesh", "Sabre à simple tranchant et lame incurvée.", 22, 23, 2, 2, 2, 23, 0, -4, 3, 2);
        public static Blade Wakizashi = new Blade("Wakizashi", "Sabre forgé par des artisans orientaux.", 23, 19, 2, 2, 2, 25, 0, -4, 3, 2);
        public static Blade Rhomphaia = new Blade("Lame ciselée", "Sabre lourd à longue lame.", 24, 24, 2, 2, 3, 27, 0, -5, 5, 4);

        public static Blade Broad_Sword = new Blade("Lame dentelée", "Sabre à large lame de la Garde Royale.", 25, 28, 3, 2, 1, 10, 0, -2, 4, 3);
        public static Blade Norse_Sword = new Blade("Norsk", "Sabre lourd utilisé par les Vikings.", 26, 30, 3, 2, 1, 12, 0, -2, 4, 3);
        public static Blade Katana = new Blade("Katana", "Sabre de combat légendaire venu d'Orient.", 27, 27, 3, 2, 1, 16, 0, -3, 4, 3);
        public static Blade Executioner = new Blade("Executioner", "Sabre à pointe émoussée pour les exécutions.", 28, 29, 3, 2, 1, 19, 0, -3, 4, 3);
        public static Blade Claymore = new Blade("Claymore", "Sabre à 2 mains.", 29, 26, 3, 2, 2, 22, 0, -4, 4, 3);
        public static Blade Schiavona = new Blade("Bestial", "Sabre à simple tranchant et lame longue.", 30, 31, 3, 2, 2, 25, 0, -4, 4, 3);
        public static Blade Bastard_Sword = new Blade("Bastard Sword", "Epée à double lame.", 31, 25, 3, 2, 2, 27, 0, -4, 4, 3);
        public static Blade Nodachi = new Blade("Nodachi", "Epée forgée en Orient.", 32, 33, 3, 2, 3, 30, 0, -5, 4, 3);
        public static Blade Rune_Blade = new Blade("Rune Blade", "Epée lourde incrustée d'incantations.", 33, 32, 3, 2, 3, 33, 0, -5, 5, 4);
        public static Blade Holy_Wind = new Blade("Vent Mortel", "Sabre forgé d'après le crucifix de Iokus.", 34, 34, 3, 2, 2, 35, 0, -6, 6, 5);

        public static Blade Hand_Axe = new Blade("Teutonique", "Hache légère utilisée pour la coupe du bois.", 35, 35, 4, 2, 1, 6, 0, -1, 3, 2);
        public static Blade Battle_Axe = new Blade("Steel Axe", "Hache à lame forgée pour trancher l'acier.", 36, 36, 4, 2, 1, 8, 0, -1, 3, 2);
        public static Blade Francisca = new Blade("Barbarian", "Hache de lancer d'une horde barbare.", 37, 37, 4, 2, 1, 10, 0, -2, 3, 2);
        public static Blade Tabarzin = new Blade("Tabarzine", "Hache de combat à lame lourde et courbe.", 38, 38, 4, 2, 1, 13, 0, -2, 3, 2);
        public static Blade Chamkaq = new Blade("Harvest", "Francisque de lancer utilisée comme une faux.", 39, 39, 4, 2, 1, 16, 0, -3, 3, 2);
        public static Blade Tabar = new Blade("Bardiche", "Hache orientale à lame en croissant.", 40, 40, 4, 2, 2, 19, 0, -3, 3, 2);
        public static Blade Bullova = new Blade("Abject", "Hache à large lame avec pointe perforante.", 41, 41, 4, 2, 2, 22, 0, -4, 3, 2);
        public static Blade Crescent = new Blade("Crescent", "Hache à lame large flanquée d'une petite lame.", 42, 42, 4, 2, 2, 25, 0, -4, 3, 2);

        public static Blade Goblin_Club = new Blade("Fléau", "Le gourdin préféré des Gobelins.", 43, 48, 4, 1, 1, 6, 0, -1, 3, 2);
        public static Blade Spiked_Club = new Blade("Burin", "Masse incrustée de pointes.", 44, 49, 4, 1, 1, 8, 0, -1, 3, 2);
        public static Blade Ball_Mace = new Blade("Osselet", "Masse ornée d'une boule d'acier.", 45, 50, 4, 1, 1, 10, 0, -2, 3, 2);
        public static Blade Spike_Mace = new Blade("Spike Mace", "Masse légère à pointes.", 46, 43, 4, 1, 1, 13, 0, -2, 3, 2);
        public static Blade Morning_Star = new Blade("Morning Star", "Masse à boule d'acier incrustée de pics.", 47, 45, 4, 1, 1, 16, 0, -3, 3, 2);
        public static Blade War_Hammer = new Blade("War Hammer", "Marteau de combat.", 48, 47, 4, 1, 2, 19, 0, -3, 3, 2);
        public static Blade Bec_de_Corbin = new Blade("Heurtoir", "Marteau de combat à pic.", 49, 44, 4, 1, 2, 22, 0, -4, 3, 2);
        public static Blade War_Maul = new Blade("Maillet", "Marteau de combat lourd.", 50, 46, 4, 1, 2, 25, 0, -4, 3, 2);

        public static Blade Guisarme = new Blade("Narrow Axe", "Hache à lame longue et étroite.", 51, 55, 5, 2, 1, 11, 0, -2, 4, 3);
        public static Blade Large_Crescent = new Blade("Large Crescent", "Hache de combat à lame lourde.", 52, 51, 5, 2, 1, 15, 0, -3, 4, 3);
        public static Blade Sabre_Halberd = new Blade("Corinthien", "Hache à lame longue.", 53, 52, 5, 2, 2, 20, 0, -3, 4, 3);
        public static Blade Balbriggin = new Blade("Terror", "Francisque à lame amovible.", 54, 53, 5, 2, 2, 25, 0, -4, 4, 3);
        public static Blade Double_Blade = new Blade("Double Blade", "Hache à doubles lames.", 55, 54, 5, 2, 3, 30, 0, -4, 4, 3);
        public static Blade Halberd = new Blade("Halberd", "Lance pouvant trancher et percer.", 56, 56, 5, 2, 3, 35, 0, -5, 4, 3);

        public static Blade Wizard_Staff = new Blade("Wizard", "Sceptre des anciens sorciers Kildéans.", 57, 57, 6, 1, 1, 1, 5, -1, 2, 1);
        public static Blade Clergy_Rod = new Blade("Clergy", "Sceptre au crucifix sacré.", 58, 58, 6, 1, 1, 2, 10, -1, 2, 1);
        public static Blade Summoner_Baton = new Blade("Diamond", "Sceptre incrusté de gemmes.", 59, 59, 6, 1, 1, 3, 15, -2, 2, 1);
        public static Blade Shamanic_Staff = new Blade("Shamanic", "Sceptre-talisman.", 60, 60, 6, 1, 1, 4, 20, -2, 3, 2);
        public static Blade Bishops_Crosier = new Blade("Bishop Cross", "Sceptre de prêtre de Iokus.", 61, 61, 6, 1, 1, 5, 25, -3, 3, 2);
        public static Blade Sages_Cane = new Blade("Sagesse", "Sceptre des vénérables sages Kildéans.", 62, 62, 6, 1, 1, 6, 30, -3, 3, 2);

        public static Blade Langdebeve = new Blade("Mandrin", "Puissante masse à tête métallique.", 63, 63, 7, 1, 1, 10, 0, -2, 4, 3);
        public static Blade Sabre_Mace = new Blade("Sabre Mace", "Masse perce-armure.", 64, 64, 7, 1, 1, 14, 0, -3, 4, 3);
        public static Blade Spike_Maul = new Blade("Spike Maul", "Marteau lourd à tête d'acier.", 65, 67, 7, 1, 2, 19, 0, -3, 4, 3);
        public static Blade Gloomwing = new Blade("Gloomwing", "Marteau lourd destiné à briser les armures.", 66, 65, 7, 1, 2, 23, 0, -4, 4, 3);
        public static Blade Mjolnir = new Blade("Mjolnir", "Pic de combat broyeur d'armures.", 67, 66, 7, 1, 2, 27, 0, -4, 4, 3);
        public static Blade Griever = new Blade("Griever", "Masse à tête d'acier triangulaire.", 68, 70, 7, 1, 3, 32, 0, -5, 4, 3);
        public static Blade Destroyer = new Blade("Destroyer", "Gourdin de combat d'ogre.", 69, 69, 7, 1, 3, 36, 0, -5, 4, 3);
        public static Blade Hand_Of_Light = new Blade("Hand of Light", "Masse sacrée repoussant les Ténèbres.", 70, 68, 7, 1, 1, 30, 0, -7, 7, 6);

        public static Blade Spear = new Blade("Fourche", "Lance simple à lame en feuille.", 71, 71, 8, 3, 1, 8, 0, -2, 5, 4);
        public static Blade Glaive = new Blade("Butcher", "Grand manche à coutelas de boucher.", 72, 72, 8, 2, 1, 10, 0, -2, 5, 4);
        public static Blade Scorpion = new Blade("Skorpio", "Lance perçante à lame amovible.", 73, 73, 8, 1, 1, 12, 0, -2, 5, 4);
        public static Blade Corcesca = new Blade("Corcesca", "Lance à deux pointes.", 74, 74, 8, 3, 1, 14, 0, -3, 6, 5);
        public static Blade Trident = new Blade("Trident", "Lance fourche à trois pointes.", 75, 75, 8, 3, 2, 17, 0, -3, 6, 5);
        public static Blade Awl_Pike = new Blade("Pike", "Lance longue à pointe effilée.", 76, 76, 8, 3, 2, 19, 0, -3, 6, 5);
        public static Blade Boar_Spear = new Blade("Lance de chasse", "Lance à large lame et harpon.", 77, 77, 8, 3, 2, 21, 0, -4, 5, 4);
        public static Blade Fauchard = new Blade("Faucheur", "Lance de cérémonie, efficace au combat.", 78, 78, 8, 3, 2, 23, 0, -4, 5, 4);
        public static Blade Voulge = new Blade("Voulge", "Lance à lame effilée et perçante.", 79, 79, 8, 2, 3, 26, 0, -4, 5, 4);
        public static Blade Pole_Axe = new Blade("Pole Axe", "Pique à lame de hache en croissant.", 80, 80, 8, 1, 3, 28, 0, -5, 6, 5);
        public static Blade Bardysh = new Blade("Bardysh", "Sandre à lame élargie.", 81, 81, 8, 2, 3, 30, 0, -5, 6, 5);
        public static Blade Brandestoc = new Blade("Brandestoc", "Lance à lame longue.", 82, 82, 8, 3, 3, 33, 0, -5, 6, 5);

        public static Blade Gastraph_Bow = new Blade("Power Balist", "Arbalète qui démultiplie votre force.", 83, 83, 9, 3, 2, 9, 0, -2, 9, 8);
        public static Blade Light_Crossbow = new Blade("Light Crossbow", "Arbalète légère et automatique.", 84, 84, 9, 3, 2, 11, 0, -1, 9, 8);
        public static Blade Target_Bow = new Blade("Arbalestrum", "Arbalète projetant rocs et boules d'acier.", 85, 85, 9, 1, 2, 13, 0, -3, 9, 8);
        public static Blade Windlass = new Blade("Firebalest", "Arbalète lourde, d'une puissance légendaire.", 86, 86, 9, 3, 3, 15, 0, -3, 10, 9);
        public static Blade Cranquein = new Blade("Crennequin", "Arbalète surpuissante.", 87, 87, 9, 1, 3, 17, 0, -3, 10, 9);
        public static Blade Lug_Crossbow = new Blade("Lug Crossbow", "Arbalète automatisée à grande vitesse.", 88, 88, 9, 3, 3, 19, 0, -4, 10, 9);
        public static Blade Siege_Bow = new Blade("Siege Bow", "Arbalète projetant de petits boulets.", 89, 89, 9, 1, 4, 21, 0, -4, 11, 10);
        public static Blade Arbalest = new Blade("Arbrier", "Arbalète perce-armure.", 90, 90, 9, 3, 4, 23, 0, -5, 12, 11);


        public static Blade[] List = new Blade[91] {
            null,
            Battle_Knife, Scramasax, Dirk, Throwing_Knife, Kudi, Cinquedea, Kris, Hatchet, Khukuri, Baselard, Stiletto, Jamadhar,
            Spatha, Scimitar, Rapier, Short_Sword, Firangi, Shamshir, Falchion, Shotel, Khora, Khophish, Wakizashi, Rhomphaia,
            Broad_Sword, Norse_Sword, Katana, Executioner, Claymore, Schiavona, Bastard_Sword, Nodachi, Rune_Blade, Holy_Wind,
            Hand_Axe, Battle_Axe, Francisca, Tabarzin, Chamkaq, Tabar, Bullova, Crescent,
            Goblin_Club, Spiked_Club, Ball_Mace, Spike_Mace, Morning_Star, War_Hammer, Bec_de_Corbin, War_Maul,
            Guisarme, Large_Crescent, Sabre_Halberd, Balbriggin, Double_Blade, Halberd,
            Wizard_Staff, Clergy_Rod, Summoner_Baton, Shamanic_Staff, Bishops_Crosier, Sages_Cane,
            Langdebeve, Sabre_Mace, Spike_Maul, Gloomwing, Mjolnir, Griever, Destroyer, Hand_Of_Light,
            Spear, Glaive, Scorpion, Corcesca, Trident, Awl_Pike, Boar_Spear, Fauchard, Voulge, Pole_Axe, Bardysh, Brandestoc,
            Gastraph_Bow, Light_Crossbow, Target_Bow, Windlass, Cranquein, Lug_Crossbow, Siege_Bow, Arbalest
        };
    }

}