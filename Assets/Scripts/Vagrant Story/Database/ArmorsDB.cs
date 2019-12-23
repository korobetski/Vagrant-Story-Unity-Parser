using VagrantStory.Items;

namespace VagrantStory.Database
{
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

    9F - Jerkin
    A0 - Hauberk
    A1 - Wizard Robe
    A2 - Cuirass
    A3 - Banded Mail
    A4 - Ring Mail
    A5 - Chain Mail
    A6 - Breastplate
    A7 - Segementata
    A8 - Scale Armor
    A9 - Brigandine
    AA - Plate Mail
    AB - Fluted Armor
    AC - Hoplite Armor
    AD - Jazeraint Armor
    AE - Dread Armor

    AF - Sandals
    B0 - Boots
    B1 - Long Boots
    B2 - Cuisse
    B3 - Light Grieve
    B4 - Ring Leggings
    B5 - Chain Leggings
    B6 - Fusskampf
    B7 - Poleyn
    B8 - Jambeau
    B9 - Missgalia
    BA - Plate Leggings
    BB - Fluted Leggings
    BC - Hoplite Leggings
    BD - Jazeraint Leggings
    BE - Dread Leggings
    BF - Bandage

    C0 - Leather Glove
    C1 - Reinforced Glove
    C2 - Knuckles
    C3 - Ring Sleeve
    C4 - Chain Sleeve
    C5 - Gauntlet
    C6 - Vambrace
    C7 - Plate Glove
    C8 - Rondanche
    C9 - Tilt Glove
    CA - Freiturnier
    CB - Fluted Glove
    CC - Hoplite Glove
    CD - Jazeraint Glove
    CE - Dread Glove

    CF - Rood Necklace
    E0 - Rune Earrings
    E1 - Lionhead
    E2 - Rusted Nails
    E3 - Sylphid Ring
    E4 - Marduk
    E5 - Salamander Ring
    E6 - Tamulis Tongue
    E7 - Gnome Bracelet
    E8 - Palolos Ring
    E9 - Undine Bracelet
    EA - Talian Ring
    EB - Agriass Balm
    EC - Kadesh Ring
    ED - Agrippas Choker
    EE - Diadras Earring
    EF - Titans Ring
    F0 - Lau Feis Armlet
    F1 - Swan Song
    F2 - Pushpaka
    F3 - Edgars Ring
    F4 - Cross Choker
    F5 - Ghost Hound
    F6 - Beaded Amulet
    F7 - Dragonhead
    F8 - Faufnir's Tear
    F9 - Agaless Chain
    FA - Balams Ring
    FB - Nimje Coif
    FC - Morgans Nails
    FD - Marlenes Ring

     * */
    public class ArmorsDB
    {
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

        public static Armor Serre_tête = new Armor("Serre-tête", "", 17, 0, 2, 1, 3, 0);
        public static Armor Knuppel = new Armor("Knuppel", "", 18, 0, 2, 2, 4, 0);
        public static Armor Capuchon = new Armor("Capuchon", "", 19, 0, 2, 1, 10, 0);
        public static Armor Heaume = new Armor("Heaume", "", 20, 0, 2, 2, 3, -1);
        public static Armor Kriegerhelm = new Armor("Kriegerhelm", "", 21, 0, 2, 3, 5, -1);
        public static Armor Spangenhelm = new Armor("Spangenhelm", "", 22, 0, 2, 3, 5, -1);
        public static Armor Partisane = new Armor("Partisane", "", 23, 0, 2, 4, 5, -1);
        public static Armor Scille = new Armor("Scille", "", 24, 0, 2, 5, 6, -1);
        public static Armor Porcelain = new Armor("Porcelain", "", 25, 0, 2, 6, 7, -1);
        public static Armor Baliste = new Armor("Baliste", "", 26, 0, 2, 7, 8, -1);
        public static Armor Sargasse = new Armor("Sargasse", "", 27, 0, 2, 8, 9, -2);
        public static Armor Haubert = new Armor("Haubert", "", 28, 0, 2, 9, 10, -2);
        public static Armor Kaiser = new Armor("Kaiser", "", 29, 0, 2, 10, 11, -2);
        public static Armor Csq_Gallois = new Armor("Casque Gallois", "", 30, 0, 2, 11, 15, -2);
        public static Armor Csq_Romain = new Armor("Casque Romain", "", 31, 0, 2, 12, 13, -2);
        public static Armor Csq_Spartiate = new Armor("Casque Spartiate", "", 32, 0, 2, 13, 12, -2);

        public static Armor Cotte_Simple = new Armor("Cotte Simple", "", 33, 0, 3, 5, 5, 0);
        public static Armor Armure_Simple = new Armor("Armure Simple", "", 34, 0, 3, 5, 10, 0);
        public static Armor Toge = new Armor("Toge", "", 35, 0, 3, 3, 20, 0);
        public static Armor Cuirasse = new Armor("Cuirasse", "", 36, 0, 3, 7, 8, 0);
        public static Armor Cotte_rivetée = new Armor("Cotte rivetée", "", 37, 0, 3, 8, 8, -1);
        public static Armor Cotte_dentelée = new Armor("Cotte dentelée", "", 38, 0, 3, 7, 12, -1);
        public static Armor Cotte_ciselée = new Armor("Cotte ciselée", "", 39, 0, 3, 9, 12, -1);
        public static Armor Breastplate = new Armor("Breastplate", "", 40, 0, 3, 11, 12, -2);
        public static Armor Segmenta = new Armor("Segmenta", "", 41, 0, 3, 13, 13, -1);
        public static Armor Armure_dEcailles = new Armor("Armure d'Ecailles", "", 42, 0, 3, 15, 15, -1);
        public static Armor Brigandine = new Armor("Brigandine", "", 43, 0, 3, 17, 18, -2);
        public static Armor Armure_Plaquée = new Armor("Armure Plaquée", "", 44, 0, 3, 18, 18, -2);
        public static Armor Armure_Métal = new Armor("Armure Métal", "", 45, 0, 3, 18, 19, -2);
        public static Armor Armure_Galloise = new Armor("Armure Galloise", "", 46, 0, 3, 18, 22, -3);
        public static Armor Armure_Romaine = new Armor("Armure Romaine", "", 47, 0, 3, 19, 20, -3);
        public static Armor Armure_Spartiate = new Armor("Armure Spartiate", "", 48, 0, 3, 20, 19, -3);

        public static Armor Guêtres = new Armor("Guêtres", "", 49, 0, 4, 1, 7, 0);
        public static Armor Brodequins = new Armor("Brodequins", "", 50, 0, 4, 2, 3, 0);
        public static Armor Bottes = new Armor("Bottes", "", 51, 0, 4, 2, 5, 0);
        public static Armor Houseaux = new Armor("Houseaux", "", 52, 0, 4, 3, 5, 0);
        public static Armor Speer = new Armor("Speer", "", 53, 0, 4, 4, 5, 0);
        public static Armor Cuissardes_Nacht = new Armor("Cuissardes Nacht", "", 54, 0, 4, 5, 6, -1);
        public static Armor Cuissardes_Tag = new Armor("Cuissardes Tag", "", 55, 0, 4, 6, 7, -1);
        public static Armor Fusskampf = new Armor("Fusskampf", "", 56, 0, 4, 7, 8, -1);
        public static Armor Drachenmark = new Armor("Drachenmark", "", 57, 0, 4, 8, 9, -1);
        public static Armor Beim = new Armor("Beim", "", 58, 0, 4, 9, 10, -2);
        public static Armor Missaglia = new Armor("Missaglia", "", 59, 0, 4, 10, 11, -3);
        public static Armor Cuissardes_Plaquées = new Armor("Cuissardes Plaquées", "", 60, 0, 4, 11, 11, -2);
        public static Armor Cuissardes_Métal = new Armor("Cuissardes Métal", "", 61, 0, 4, 12, 12, -2);
        public static Armor Cuissardes_Galloises = new Armor("Cuissardes Galloises", "", 62, 0, 4, 13, 18, -3);
        public static Armor Cuissardes_Romaines = new Armor("Cuissardes Romaines", "", 63, 0, 4, 14, 17, -3);
        public static Armor Csrdes_Spartiates = new Armor("Cuissardes Spartiates", "", 64, 0, 4, 15, 15, -3);

        public static Armor Buffle = new Armor("Buffle", "", 65, 0, 5, 1, 8, 0);
        public static Armor Paumelle = new Armor("Paumelle", "", 66, 0, 5, 2, 4, 0);
        public static Armor Paumelle_rivetée = new Armor("Paumelle rivetée", "", 67, 0, 5, 2, 4, 0);
        public static Armor Manicle = new Armor("Manicle", "", 68, 0, 5, 3, 5, 0);
        public static Armor Gant_dentelé = new Armor("Gant dentelé", "", 69, 0, 5, 4, 5, -1);
        public static Armor Gant_ciselé = new Armor("Gant ciselé", "", 70, 0, 5, 4, 6, -1);
        public static Armor Gauntlet = new Armor("Gauntlet", "", 71, 0, 5, 5, 6, -1);
        public static Armor Casar = new Armor("Casar", "", 72, 0, 5, 6, 7, -1);
        public static Armor Gant_plaqué = new Armor("Gant plaqué", "", 73, 0, 5, 7, 8, -1);
        public static Armor Bogen = new Armor("Bogen", "", 74, 0, 5, 8, 9, -1);
        public static Armor Paumelle_de_fer = new Armor("Paumelle de fer", "", 75, 0, 5, 8, 9, -3);
        public static Armor Ness = new Armor("Ness", "", 76, 0, 5, 9, 10, -1);
        public static Armor Poucier = new Armor("Poucier", "", 77, 0, 5, 10, 11, -2);
        public static Armor Gantelet_Gallois = new Armor("Gantelet Gallois", "", 78, 0, 5, 11, 15, -2);
        public static Armor Gantelet_Romain = new Armor("Gantelet Romain", "", 79, 0, 5, 12, 14, -2);
        public static Armor Gantelet_Spartiate = new Armor("Gantelet Spartiate", "", 80, 0, 5, 13, 13, -2);


        // TODO need to ad stats
        public static Armor Necklace = new Armor("Necklace", "", 81, 0, 6);
        public static Armor Colifichet = new Armor("Colifichet", "", 82, 0, 6);
        public static Armor Lionhead = new Armor("Lionhead", "", 83, 0, 6);
        public static Armor Iron_Claw = new Armor("Iron Claw", "", 84, 0, 6);
        public static Armor Sylphide = new Armor("Sylphide", "", 85, 0, 6);
        public static Armor Marduk = new Armor("Marduk", "", 86, 0, 6);
        public static Armor Gecko = new Armor("Gecko", "", 87, 0, 6);
        public static Armor Nail = new Armor("Nail", "", 88, 0, 6);
        public static Armor Bracelet = new Armor("Bracelet", "", 89, 0, 6);
        public static Armor Palolo = new Armor("Palolo", "", 90, 0, 6);
        public static Armor Ondine = new Armor("Ondine", "", 91, 0, 6);
        public static Armor Talia = new Armor("Talia", "", 92, 0, 6);
        public static Armor Baume_dAgrias = new Armor("Baume d'Agrias", "", 93, 0, 6);
        public static Armor Bague = new Armor("Bague", "", 94, 0, 6);
        public static Armor Agrippa = new Armor("Agrippa", "", 95, 0, 6);
        public static Armor Babiole = new Armor("Babiole", "", 96, 0, 6);
        public static Armor Anneau = new Armor("Anneau", "", 97, 0, 6);
        public static Armor Gourmette = new Armor("Gourmette", "", 98, 0, 6);
        public static Armor Broche = new Armor("Broche", "", 99, 0, 6);
        public static Armor Pégase = new Armor("Pégase", "", 100, 0, 6);
        public static Armor Nécro_Boucle = new Armor("Nécro Boucle", "", 101, 0, 6);
        public static Armor Echarpe = new Armor("Echarpe", "", 102, 0, 6);
        public static Armor Chaînette = new Armor("Chaînette", "", 103, 0, 6);
        public static Armor Ornement = new Armor("Ornement", "", 104, 0, 6);
        public static Armor Dragonhead = new Armor("Dragonhead", "", 105, 0, 6);
        public static Armor Larme = new Armor("Larme", "", 106, 0, 6);
        public static Armor Parure = new Armor("Parure", "", 107, 0, 6);
        public static Armor Os = new Armor("Os", "", 108, 0, 6);
        public static Armor Coiffe = new Armor("Coiffe", "", 109, 0, 6);
        public static Armor Sarcophage = new Armor("Sarcophage", "", 110, 0, 6);
        public static Armor Alliance = new Armor("Alliance", "", 110, 0, 6);


        public static Armor[] ShieldList = new Armor[17] {
            null,
            Buckler_Shield, Hoplite_Shield, Round_Shield, Targe_Shield, Quad_Shield, Tower_Shield, Oval_Shield, Pelta_Shield,
            Circle_Shield, Heater_Shield, Spiked_Shield, Kite_Shield, Casserole_Shield, Jazeraint_Shield, Dread_Shield, Knight_Shield
        };
    }

}