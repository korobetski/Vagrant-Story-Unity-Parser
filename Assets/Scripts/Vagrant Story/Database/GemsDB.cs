using UnityEngine;
using VagrantStory.Items;

namespace VagrantStory.Database
{
    public class GemsDB : MonoBehaviour
    {
        public enum eGems
        {
            None,
            Talos_Feldspear, Titan_Malachite, Sylphid_Topaz, Djinn_Amber, Salamander_Ruby, Ifrit_Carnelian, Gnome_Emerald, Dao_Moonstone,
            Undine_Jasper, Marid_Aquamarine, Angel_Pearl, Seraphim_Diamond, Morlock_Jet, Berial_Black_Pearl, Haeralis, Orlandu,
            Orion, Ogmius, Iocus, Balvus, Trinity, Beowulf, Dragonite, Sigguld,
            Demonia, Altema, Polaris, Basivalin, Galerian, Vedivier, Berion, Gervin,
            Tertia, Lancer, Arturos, Braveheart, Hellraiser, Nightkiller, Manabreaker, Powerfist,
            Brainshield, Speedster, Silent_Queen, Dark_Queen, Death_Queen, White_Queen,
        };

        public static Gem Talos_Feldspear = new Gem("Talos", "Pierre envoûtée par les pouvoirs de Talos. Augmente légèrement la puissance des attaques.", "MAJOR", "PHYSICAL");
        public static Gem Titan_Malachite = new Gem("Malachite", "Une âme de titan a été scellée dans cette Malachite. Augmente la puissance des attaques.", "MINOR", "PHYSICAL");
        public static Gem Sylphid_Topaz = new Gem("Topaze", "Topaze imprégnée du pouvoir des Sylphides. Augmente la liaison Air.", "MINOR", "AIR");
        public static Gem Djinn_Amber = new Gem("Ambre", "Ambre possédant la puissance du Jinni. Augmente la liaison Air.", "MAJOR", "AIR");
        public static Gem Salamander_Ruby = new Gem("Salamander", "Rubis ceint du pouvoir des Salamandres. Augmente la liaison Feu.", "MINOR", "FIRE");
        public static Gem Ifrit_Carnelian = new Gem("Anthracite", "Anthracite nimbé de la force des Ifrits. Augmente la liaison Feu.", "MAJOR", "FIRE");
        public static Gem Gnome_Emerald = new Gem("Emeraude", "Emeraude imprégnée du pouvoir des Gnomes. Augmente la liaison Terre.", "MINOR", "EARTH");
        public static Gem Dao_Moonstone = new Gem("Moonstone", "Pierre de Lune auréolée de la force de Daos. Augmente la liaison Terre.", "MAJOR", "EARTH");
        public static Gem Undine_Jasper = new Gem("Jaspe", "Jaspe imbibé du pouvoir des Ondines. Augmente la liaison Eau.", "MINOR", "WATER");
        public static Gem Marid_Aquamarine = new Gem("Aigue-Marine", "Aigue-Marine contenant la force des Marians. Augmente la liaison Eau.", "MAJOR", "WATER");
        public static Gem Angel_Pearl = new Gem("Perle", "MINOR", "Perle sertie d'une aura angélique. Augmente la liaison Lumière.", "LIGHT");
        public static Gem Seraphim_Diamond = new Gem("Diamant", "Diamant renfermant l'âme d'un Archange. Augmente la liaison Lumière.", "MAJOR", "LIGHT");
        public static Gem Morlock_Jet = new Gem("Morlock", "Pierre contenant le pouvoir de Morlock. Augmente la liaison Ténèbres.", "MINOR", "DARK");
        public static Gem Berial_Black_Pearl = new Gem("Perle Noire", "Perle Noire emprisonnant l'âme d'un damné. Augmente la liaison Ténèbres.", "MAJOR", "DARK");
        public static Gem Haeralis = new Gem("Saphir", "Saphir imprégné du pouvoir d'Haéralis le brave. Augmente la résistance aux mutants.", "MINOR", "HUMAN");
        public static Gem Orlandu = new Gem("Skeleton", "Aktinolite contenant un fragment d'os sacré. Augmente la résistance aux mutants.", "MAJOR", "HUMAN");
        public static Gem Orion = new Gem("Orion", "Corail sombre renfermant un cheveu d'Orion. Augmente l'endurance face aux bêtes.", "MINOR", "BEAST");
        public static Gem Ogmius = new Gem("Améthyste", "Améthyste contenant l'âme d'Ogmius le Cerbère. Augmente la résistance aux bêtes.", "MAJOR", "BEAST");
        public static Gem Iocus = new Gem("Lapis-lazuli", "Lapis-lazuli sacré de Iokus. Augmente la résistance aux revenants.", "MINOR", "UNDEAD");
        public static Gem Balvus = new Gem("Schiste", "Schiste contenant les cendres de Balyus. Augmente la résistance aux revenants.", "MAJOR", "UNDEAD");
        public static Gem Trinity = new Gem("Trinity", "Jade renfermant l'âme d'anciens Vikings. Augmente la résistance aux esprits.", "MINOR", "PHANTOM");
        public static Gem Beowulf = new Gem("Beowulf", "Armandine contenant un cheveu de Beowulf. Augmente la résistance aux esprits.", "MAJOR", "PHANTOM");
        public static Gem Dragonite = new Gem("Dragonite", "Serpentine renfermant la force d'un griffon. Augmente la résistance aux dragons.", "MINOR", "DRAGON");
        public static Gem Sigguld = new Gem("Grenat", "Agathe de feu imprégnée de l'âme de Siggurd. Augmente la résistance aux dragons.", "MAJOR", "DRAGON");
        public static Gem Demonia = new Gem("Demonia", "Opale bleue imprégnée de sang diabolique. Augmente la résistance aux spectres.", "MINOR", "EVIL");
        public static Gem Altema = new Gem("Blood Ruby", "Rubis sanguin contenant l'âme d'Altèma. Augmente la résistance aux spectres.", "MAJOR", "EVIL");
        public static Gem Polaris = new Gem("Polaris", "Malachite artificielle fabriquée par les sorciers Kildéans.", "EXCEP", "POLARIS");
        public static Gem Basivalin = new Gem("Serpentine", "Serpentine artificielle fabriquée par les mages Kildéans.", "EXCEP", "BASIVALEN");
        public static Gem Galerian = new Gem("Jade", "Pierre artificielle fabriquée par les sages Kildéans.", "EXCEP", "GALERIAN");
        public static Gem Vedivier = new Gem("Quartz", "Améthyste artificielle fabriquée par les sorciers Kildéans.", "EXCEP", "VEDIVIER");
        public static Gem Berion = new Gem("Cristal", "Pierre de lune artificielle fabriquée par les alchimistes Kildéans.", "EXCEP", "BERION");
        public static Gem Gervin = new Gem("Tourmaline", "Topaze artificielle fabriquée par les magiciens Kildéans.", "EXCEP", "GERVIN");
        public static Gem Tertia = new Gem("Obsidienne", "Emeraude artificielle fabriquée par les enchanteurs Kildéans.", "EXCEP", "TERTIA");
        public static Gem Lancer = new Gem("Almandin", "Rubis artificiel fabriqué par les alchimistes Kildéans.", "EXCEP", "LANCER");
        public static Gem Arturos = new Gem("Cornaline", "Diamant artificiel fabriqué par les sorciers Kildéans.", "EXCEP", "ARTUROS");
        public static Gem Braveheart = new Gem("Braveheart", "Accroît le taux de réussite des attaques non magiques de 20%. A placer sur une arme.", "ATTACK", "HIT");
        public static Gem Hellraiser = new Gem("Hellraiser", "Accroît le taux de réussite des attaques magiques de 20%. A placer sur une arme.", "ATTACK", "SPELL");
        public static Gem Nightkiller = new Gem("Nightkiller", "Accroît les chances d'esquiver les attaques physiques de 20%. A placer sur un bouclier.", "PROTECTION", "EVADE");
        public static Gem Manabreaker = new Gem("Manabreaker", "Accroît les chances d'éviter les attaques magiques de 20%. A placer sur un bouclier.", "PROTECTION", "SPELLEVADE");
        public static Gem Powerfist = new Gem("Powerfist", "Accroît les chances d'éviter 'FRC moins'de 20%. A placer sur un bouclier.", "PROTECTION", "EVADESTR");
        public static Gem Brainshield = new Gem("Brainshield", "Accroît les chances d'éviter 'INT moins'de 20%. A placer sur un bouclier.", "PROTECTION", "EVADEINT");
        public static Gem Speedster = new Gem("Speedster", "Accroît les chances d'éviter 'AGL moins'de 20%. A placer sur un bouclier.", "PROTECTION", "EVADEAGI");
        public static Gem Silent_Queen = new Gem("Silent Queen", "Accroît de 20% les chances d'éviter 'Silence'. A placer sur un bouclier.", "PROTECTION", "EVADESILENT");
        public static Gem Dark_Queen = new Gem("Frost Queen", "Accroît de 20% les chances d'éviter 'Paralysie'. A placer sur un bouclier.", "PROTECTION", "EVADEPARA");
        public static Gem Death_Queen = new Gem("Snake King", "Accroît de 20% les chances d'éviter 'Poison'. A placer sur un bouclier.", "PROTECTION", "EVADEPOISON");
        public static Gem White_Queen = new Gem("Shock King", "Accroît de 20% les chances d'éviter 'Torpeur'. A placer sur un bouclier.", "PROTECTION", "EVADENUMB");


        public static Gem[] List = new Gem[47] {
            null,
            Talos_Feldspear, Titan_Malachite, Sylphid_Topaz, Djinn_Amber, Salamander_Ruby, Ifrit_Carnelian, Gnome_Emerald, Dao_Moonstone, Undine_Jasper, Marid_Aquamarine,
            Angel_Pearl, Seraphim_Diamond, Morlock_Jet, Berial_Black_Pearl, Haeralis, Orlandu, Orion, Ogmius, Iocus, Balvus, Trinity, Beowulf, Dragonite, Sigguld,
            Demonia, Altema, Polaris, Basivalin, Galerian, Vedivier, Berion, Gervin, Tertia, Lancer, Arturos, Braveheart, Hellraiser, Nightkiller, Manabreaker, Powerfist,
            Brainshield, Speedster, Silent_Queen, Dark_Queen, Death_Queen, White_Queen
        };
    }

}