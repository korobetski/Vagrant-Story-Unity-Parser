using VagrantStory.Items;

namespace VagrantStory.Database
{
    public class ArmorsDB
    {
        public enum eShields
        {
            None = 0,
            Buckler_Shield = 1,
            Hoplite_Shield,
            Round_Shield,
            Targe_Shield,
            Quad_Shield,
            Tower_Shield,
            Oval_Shield,
            Pelta_Shield,
            Circle_Shield,
            Heater_Shield,
            Spiked_Shield,
            Kite_Shield,
            Casserole_Shield,
            Jazeraint_Shield,
            Dread_Shield,
            Knight_Shield
        };


        #region SHIELDS
        public static Armor Buckler_Shield = new Armor("Buckler", "", 1, 96, 1, 5, 3, -1, 0);
        public static Armor Pelta_Shield = new Armor("Pelta", "", 2, 102, 1, 5, 4, -1, 1);
        public static Armor Targe_Shield = new Armor("Targe", "", 3, 99, 1, 6, 4, -1, 1);
        public static Armor Quad_Shield = new Armor("Quad", "", 4, 100, 1, 7, 7, -1, 1);
        public static Armor Circle_Shield = new Armor("Circle", "", 5, 105, 1, 8, 7, -1, 1);
        public static Armor Tower_Shield = new Armor("Tower", "", 6, 101, 1, 12, 10, -2, 1);
        public static Armor Spiked_Shield = new Armor("Spike", "", 7, 106, 1, 12, 11, -2, 1);
        public static Armor Round_Shield = new Armor("Round", "", 8, 98, 1, 14, 13, -2, 2);
        public static Armor Kite_Shield = new Armor("Kite", "", 9, 107, 1, 15, 13, -2, 2);
        public static Armor Casserole_Shield = new Armor("Citadelle", "", 10, 108, 1, 15, 14, -2, 2);
        public static Armor Heater_Shield = new Armor("Brûleur", "", 11, 104, 1, 18, 16, -2, 2);
        public static Armor Oval_Shield = new Armor("Oval", "", 12, 103, 1, 18, 17, -2, 2);
        public static Armor Knight_Shield = new Armor("Muraille", "", 13, 111, 1, 18, 18, -2, 2);
        public static Armor Hoplite_Shield = new Armor("Bouclier Gallois", "", 14, 97, 1, 21, 24, -3, 3);
        public static Armor Jazeraint_Shield = new Armor("Bouclier Romain", "", 15, 109, 1, 23, 23, -3, 3);
        public static Armor Dread_Shield = new Armor("Bouclier Spartiate", "", 16, 110, 1, 25, 20, -3, 3);
        #endregion
        #region HELMS
        /*
        8F - Bandana
        90 - Bear Mask
        91 - Wizard Hat
        92 - Bone Helm
        93 - Chain Coif
        94 - Spangenhelm
        95 - Cabasset
        96 - Sallet
        97 - Barbut
        98 - Basinet
        99 - Armet
        9A - Close Helm
        9B - Burgonet
        9C - Hoplite Helm
        9D - Jazeraint Helm
        9E - Dread Helm
        */
        public static Armor Bandana = new Armor("Serre-tête", "", 17, 0, 2, 1, 3, 0);
        public static Armor Bear_Mask = new Armor("Knuppel", "", 18, 0, 2, 2, 4, 0);
        public static Armor Wizard_Hat = new Armor("Capuchon", "", 19, 0, 2, 1, 10, 0);
        public static Armor Bone_Helm = new Armor("Heaume", "", 20, 0, 2, 2, 3, -1);
        public static Armor Chain_Coif = new Armor("Kriegerhelm", "", 21, 0, 2, 3, 5, -1);
        public static Armor Spangenhelm = new Armor("Spangenhelm", "", 22, 0, 2, 3, 5, -1);
        public static Armor Cabasset = new Armor("Partisane", "", 23, 0, 2, 4, 5, -1);
        public static Armor Sallet = new Armor("Scille", "", 24, 0, 2, 5, 6, -1);
        public static Armor Barbut = new Armor("Porcelain", "", 25, 0, 2, 6, 7, -1);
        public static Armor Basinet = new Armor("Baliste", "", 26, 0, 2, 7, 8, -1);
        public static Armor Armet = new Armor("Sargasse", "", 27, 0, 2, 8, 9, -2);
        public static Armor Close_Helm = new Armor("Haubert", "", 28, 0, 2, 9, 10, -2);
        public static Armor Burgonet = new Armor("Kaiser", "", 29, 0, 2, 10, 11, -2);
        public static Armor Hoplite_Helm = new Armor("Casque Gallois", "", 30, 0, 2, 11, 15, -2);
        public static Armor Jazeraint_Helm = new Armor("Casque Romain", "", 31, 0, 2, 12, 13, -2);
        public static Armor Dread_Helm = new Armor("Casque Spartiate", "", 32, 0, 2, 13, 12, -2);
        #endregion
        #region ARMORS
        /*
        9F - Jerkin
        A0 - Hauberk
        A1 - Wizard_Robe
        A2 - Cuirass
        A3 - Banded_Mail
        A4 - Ring_Mail
        A5 - Chain_Mail
        A6 - Breastplate
        A7 - Segementata
        A8 - Scale_Armor
        A9 - Brigandine
        AA - Plate_Mail
        AB - Fluted_Armor
        AC - Hoplite_Armor
        AD - Jazeraint_Armor
        AE - Dread_Armor
        */
        public static Armor Jerkin = new Armor("Cotte Simple", "", 33, 0, 3, 5, 5, 0);
        public static Armor Hauberk = new Armor("Armure Simple", "", 34, 0, 3, 5, 10, 0);
        public static Armor Wizard_Robe = new Armor("Toge", "", 35, 0, 3, 3, 20, 0);
        public static Armor Cuirass = new Armor("Cuirasse", "", 36, 0, 3, 7, 8, 0);
        public static Armor Banded_Mail = new Armor("Cotte rivetée", "", 37, 0, 3, 8, 8, -1);
        public static Armor Ring_Mail = new Armor("Cotte dentelée", "", 38, 0, 3, 7, 12, -1);
        public static Armor Chain_Mail = new Armor("Cotte ciselée", "", 39, 0, 3, 9, 12, -1);
        public static Armor Breastplate = new Armor("Breastplate", "", 40, 0, 3, 11, 12, -2);
        public static Armor Segementata = new Armor("Segmenta", "", 41, 0, 3, 13, 13, -1);
        public static Armor Scale_Armor = new Armor("Armure d'Ecailles", "", 42, 0, 3, 15, 15, -1);
        public static Armor Brigandine = new Armor("Brigandine", "", 43, 0, 3, 17, 18, -2);
        public static Armor Plate_Mail = new Armor("Armure Plaquée", "", 44, 0, 3, 18, 18, -2);
        public static Armor Fluted_Armor = new Armor("Armure Métal", "", 45, 0, 3, 18, 19, -2);
        public static Armor Hoplite_Armor = new Armor("Armure Galloise", "", 46, 0, 3, 18, 22, -3);
        public static Armor Jazeraint_Armor = new Armor("Armure Romaine", "", 47, 0, 3, 19, 20, -3);
        public static Armor Dread_Armor = new Armor("Armure Spartiate", "", 48, 0, 3, 20, 19, -3);
        #endregion
        #region BOOTS
        /*
        AF - Sandals
        B0 - Boots
        B1 - Long_Boots
        B2 - Cuisse
        B3 - Light_Grieve
        B4 - Ring_Leggings
        B5 - Chain_Leggings
        B6 - Fusskampf
        B7 - Poleyn
        B8 - Jambeau
        B9 - Missgalia
        BA - Plate_Leggings
        BB - Fluted_Leggings
        BC - Hoplite_Leggings
        BD - Jazeraint_Leggings
        BE - Dread_Leggings
        BF - Bandage
    */
        public static Armor Bandage = new Armor("Guêtres", "", 49, 0, 4, 1, 7, 0);
        public static Armor Sandals = new Armor("Brodequins", "", 50, 0, 4, 2, 3, 0);
        public static Armor Boots = new Armor("Bottes", "", 51, 0, 4, 2, 5, 0);
        public static Armor Long_Boots = new Armor("Houseaux", "", 52, 0, 4, 3, 5, 0);
        public static Armor Light_Grieve = new Armor("Speer", "", 53, 0, 4, 4, 5, 0);
        public static Armor Ring_Leggings = new Armor("Cuissardes Nacht", "", 54, 0, 4, 5, 6, -1);
        public static Armor Chain_Leggings = new Armor("Cuissardes Tag", "", 55, 0, 4, 6, 7, -1);
        public static Armor Fusskampf = new Armor("Fusskampf", "", 56, 0, 4, 7, 8, -1);
        public static Armor Poleyn = new Armor("Drachenmark", "", 57, 0, 4, 8, 9, -1);
        public static Armor Jambeau = new Armor("Beim", "", 58, 0, 4, 9, 10, -2);
        public static Armor Missaglia = new Armor("Missaglia", "", 59, 0, 4, 10, 11, -3);
        public static Armor Plate_Leggings = new Armor("Cuissardes Plaquées", "", 60, 0, 4, 11, 11, -2);
        public static Armor Fluted_Leggings = new Armor("Cuissardes Métal", "", 61, 0, 4, 12, 12, -2);
        public static Armor Hoplite_Leggings = new Armor("Cuissardes Galloises", "", 62, 0, 4, 13, 18, -3);
        public static Armor Jazeraint_Leggings = new Armor("Cuissardes Romaines", "", 63, 0, 4, 14, 17, -3);
        public static Armor Dread_Leggings = new Armor("Cuissardes Spartiates", "", 64, 0, 4, 15, 15, -3);
        #endregion
        #region GAUNTLETS 
        /*
        C0 - Leather_Glove
    C1 - Reinforced_Glove
    C2 - Knuckles
    C3 - Ring_Sleeve
    C4 - Chain_Sleeve
    C5 - Gauntlet
    C6 - Vambrace
    C7 - Plate_Glove
    C8 - Rondanche
    C9 - Tilt_Glove
    CA - Freiturnier
    CB - Fluted_Glove
    CC - Hoplite_Glove
    CD - Jazeraint_Glove
    CE - Dread_Glove
        */
        public static Armor Buffle = new Armor("Buffle", "", 65, 0, 5, 1, 8, 0);
        public static Armor Leather_Glove = new Armor("Paumelle", "", 66, 0, 5, 2, 4, 0);
        public static Armor Reinforced_Glove = new Armor("Paumelle rivetée", "", 67, 0, 5, 2, 4, 0);
        public static Armor Knuckles = new Armor("Manicle", "", 68, 0, 5, 3, 5, 0);
        public static Armor Ring_Sleeve = new Armor("Gant dentelé", "", 69, 0, 5, 4, 5, -1);
        public static Armor Chain_Sleeve = new Armor("Gant ciselé", "", 70, 0, 5, 4, 6, -1);
        public static Armor Gauntlet = new Armor("Gauntlet", "", 71, 0, 5, 5, 6, -1);
        public static Armor Vambrace = new Armor("Casar", "", 72, 0, 5, 6, 7, -1);
        public static Armor Plate_Glove = new Armor("Gant plaqué", "", 73, 0, 5, 7, 8, -1);
        public static Armor Rondanche = new Armor("Bogen", "", 74, 0, 5, 8, 9, -1);
        public static Armor Tilt_Glove = new Armor("Paumelle de fer", "", 75, 0, 5, 8, 9, -3);
        public static Armor Freiturnier = new Armor("Ness", "", 76, 0, 5, 9, 10, -1);
        public static Armor Fluted_Glove = new Armor("Poucier", "", 77, 0, 5, 10, 11, -2);
        public static Armor Hoplite_Glove = new Armor("Gantelet Gallois", "", 78, 0, 5, 11, 15, -2);
        public static Armor Jazeraint_Glove = new Armor("Gantelet Romain", "", 79, 0, 5, 12, 14, -2);
        public static Armor Dread_Glove = new Armor("Gantelet Spartiate", "", 80, 0, 5, 13, 13, -2);
        #endregion
        #region ACCESSORIES
        /*
        CF - Rood_Necklace
        E0 - Rune_Earrings
        E1 - Lionhead
        E2 - Rusted_Nails
        E3 - Sylphid_Ring
        E4 - Marduk
        E5 - Salamander_Ring
        E6 - Tamulis_Tongue
        E7 - Gnome_Bracelet
        E8 - Palolos_Ring
        E9 - Undine_Bracelet
        EA - Talian_Ring
        EB - Agriass_Balm
        EC - Kadesh_Ring
        ED - Agrippas_Choker
        EE - Diadras_Earring
        EF - Titans_Ring
        F0 - Lau_Feis_Armlet
        F1 - Swan_Song
        F2 - Pushpaka
        F3 - Edgars_Ring
        F4 - Cross_Choker
        F5 - Ghost_Hound
        F6 - Beaded_Amulet
        F7 - Dragonhead
        F8 - Faufnirs_Tear
        F9 - Agaless_Chain
        FA - Balams_Ring
        FB - Ninja_Coif
        FC - Morgans_Nails
        FD - Marlenes_Ring
        */
        // TODO need to ad stats
        public static Armor Rood_Necklace = new Armor("Necklace", "", 81, 0, 6);
        public static Armor Rune_Earrings = new Armor("Colifichet", "", 82, 0, 6);
        public static Armor Lionhead = new Armor("Lionhead", "", 83, 0, 6);
        public static Armor Rusted_Nails = new Armor("Iron Claw", "", 84, 0, 6);
        public static Armor Sylphid_Ring = new Armor("Sylphide", "", 85, 0, 6);
        public static Armor Marduk = new Armor("Marduk", "", 86, 0, 6);
        public static Armor Salamander_Ring = new Armor("Gecko", "", 87, 0, 6);
        public static Armor Tamulis_Tongue = new Armor("Nail", "", 88, 0, 6);
        public static Armor Gnome_Bracelet = new Armor("Bracelet", "", 89, 0, 6);
        public static Armor Palolos_Ring = new Armor("Palolo", "", 90, 0, 6);
        public static Armor Undine_Bracelet = new Armor("Ondine", "", 91, 0, 6);
        public static Armor Talian_Ring = new Armor("Talia", "", 92, 0, 6);
        public static Armor Agriass_Balm = new Armor("Baume d'Agrias", "", 93, 0, 6);
        public static Armor Kadesh_Ring = new Armor("Bague", "", 94, 0, 6);
        public static Armor Agrippas_Choker = new Armor("Agrippa", "", 95, 0, 6);
        public static Armor Diadras_Earring = new Armor("Babiole", "", 96, 0, 6);
        public static Armor Titans_Ring = new Armor("Anneau", "", 97, 0, 6);
        public static Armor Lau_Feis_Armlet = new Armor("Gourmette", "", 98, 0, 6);
        public static Armor Swan_Song = new Armor("Broche", "", 99, 0, 6);
        public static Armor Pushpaka = new Armor("Pégase", "", 100, 0, 6);
        public static Armor Edgars_Ring = new Armor("Nécro Boucle", "", 101, 0, 6);
        public static Armor Cross_Choker = new Armor("Echarpe", "", 102, 0, 6);
        public static Armor Ghost_Hound = new Armor("Chaînette", "", 103, 0, 6);
        public static Armor Beaded_Amulet = new Armor("Ornement", "", 104, 0, 6);
        public static Armor Dragonhead = new Armor("Dragonhead", "", 105, 0, 6);
        public static Armor Faufnirs_Tear = new Armor("Larme", "", 106, 0, 6);
        public static Armor Agaless_Chain = new Armor("Parure", "", 107, 0, 6);
        public static Armor Balams_Ring = new Armor("Os", "", 108, 0, 6);
        public static Armor Ninja_Coif = new Armor("Coiffe", "", 109, 0, 6);
        public static Armor Morgans_Nails = new Armor("Sarcophage", "", 110, 0, 6);
        public static Armor Marlenes_Ring = new Armor("Alliance", "", 110, 0, 6);
        #endregion

        public static Armor[] ShieldList = new Armor[17] {
            null,
            Buckler_Shield, Hoplite_Shield, Round_Shield, Targe_Shield, Quad_Shield, Tower_Shield, Oval_Shield, Pelta_Shield,
            Circle_Shield, Heater_Shield, Spiked_Shield, Kite_Shield, Casserole_Shield, Jazeraint_Shield, Dread_Shield, Knight_Shield
        };
        public static Armor[] HelmList = new Armor[17] {
            null,
            Bandana, Bear_Mask, Wizard_Hat, Bone_Helm, Chain_Coif, Spangenhelm, Cabasset, Sallet,
            Barbut, Basinet, Armet, Close_Helm, Burgonet, Hoplite_Helm, Jazeraint_Helm, Dread_Helm
        };
        public static Armor[] HauberkList = new Armor[17] {
            null,
            Jerkin, Hauberk, Wizard_Robe, Cuirass, Banded_Mail, Ring_Mail, Chain_Mail, Breastplate,
            Segementata, Scale_Armor, Brigandine, Plate_Mail, Fluted_Armor, Hoplite_Armor, Jazeraint_Armor, Dread_Armor
        };
        public static Armor[] BootsList = new Armor[17] {
            null,
            Bandage, Sandals, Boots, Long_Boots, Light_Grieve, Ring_Leggings, Chain_Leggings, Fusskampf,
            Poleyn, Jambeau, Missaglia, Plate_Leggings, Fluted_Leggings, Hoplite_Leggings, Jazeraint_Leggings, Dread_Leggings
        };
        public static Armor[] GloveList = new Armor[17] {
            null,
            Buffle, Leather_Glove, Reinforced_Glove, Knuckles, Ring_Sleeve, Chain_Sleeve, Gauntlet, Vambrace,
            Plate_Glove, Rondanche, Tilt_Glove, Freiturnier, Fluted_Glove, Hoplite_Glove, Jazeraint_Glove, Dread_Glove
        };

        public static Armor[] AccessoryList = new Armor[32] {
            null,
            Rood_Necklace, Rune_Earrings, Lionhead, Rusted_Nails, Sylphid_Ring, Marduk, Salamander_Ring, Tamulis_Tongue,
            Gnome_Bracelet, Palolos_Ring, Undine_Bracelet, Talian_Ring, Agriass_Balm, Kadesh_Ring, Agrippas_Choker, Diadras_Earring,
            Titans_Ring, Lau_Feis_Armlet, Swan_Song, Pushpaka, Edgars_Ring, Cross_Choker, Ghost_Hound, Beaded_Amulet, Dragonhead,
            Faufnirs_Tear, Agaless_Chain, Balams_Ring, Ninja_Coif, Morgans_Nails, Marlenes_Ring
        };

    }

}
