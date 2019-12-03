using System;
using System.Collections.Generic;
using UnityEngine;

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

namespace VS.Data
{
    [Serializable]
    public class Armor
    {
        public enum ArmorTypes { NONE = 0, SHIELD = 1, HELM = 2, ARMOR = 3, BOOTS = 4, GLOVE = 5, ACCESSORY = 6 }

        public static ArmorTypes NONE = ArmorTypes.NONE;
        public static ArmorTypes SHIELD = ArmorTypes.SHIELD;
        public static ArmorTypes HELM = ArmorTypes.HELM;
        public static ArmorTypes ARMOR = ArmorTypes.ARMOR;
        public static ArmorTypes BOOTS = ArmorTypes.BOOTS;
        public static ArmorTypes GLOVE = ArmorTypes.GLOVE;
        public static ArmorTypes ACCESSORY = ArmorTypes.ACCESSORY;

        public static ArmorTypes[] Types = new ArmorTypes[] { NONE, SHIELD, HELM, ARMOR, BOOTS, GLOVE, ACCESSORY };

        public static List<Armor> list = new List<Armor>();
        public static string JSONlist()
        {
            string[] slst = new string[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                slst[i] = list[i].ToJSON();
            }
            return string.Concat("[", string.Join(", ", slst), "]");
        }
        public static string CSlist()
        {
            string[] slst = new string[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                Armor b = list[i];
                string sanitizedName = b._name.Replace(" ", "_");

                slst[i] = string.Concat("public static Armor ", sanitizedName, " = new Armor(",
                    "\"", b._name, "\", ", "\"", b._desc, "\", ", b._ID, ", ", b._WEP, ", ", b._ArmorType, ", ", b._STR, ", ", b._INT, ", ", b._AGI, ");");
            }
            return string.Concat(string.Join("\r\n", slst));
        }


        [SerializeField]
        private string _name = "";
        [SerializeField]
        private string _desc = "";
        [SerializeField]
        private byte _ID;
        [SerializeField]
        private byte _WEP;
        [SerializeField]
        private byte _ArmorType;
        [SerializeField]
        private byte _GemSlots;
        [SerializeField]
        private sbyte _STR;
        [SerializeField]
        private sbyte _INT;
        [SerializeField]
        private sbyte _AGI;

        public Armor(byte[] rawDatas, string name = "", string desc = "")
        {
            _ID = rawDatas[0];
            _WEP = rawDatas[1];
            _ArmorType = rawDatas[2];
            GemSlots = rawDatas[3];
            _STR = (sbyte)rawDatas[4];
            _INT = (sbyte)rawDatas[5];
            _AGI = (sbyte)rawDatas[6];

            _name = name;
            _desc = desc;
        }

        public string Name { get => _name; set => _name = value; }
        public byte ID { get => _ID; set => _ID = value; }
        public byte WEP { get => _WEP; set => _WEP = value; }
        public byte ArmorType { get => _ArmorType; set => _ArmorType = value; }
        public sbyte STR { get => _STR; set => _STR = value; }
        public sbyte INT { get => _INT; set => _INT = value; }
        public sbyte AGI { get => _AGI; set => _AGI = value; }
        public byte GemSlots { get => _GemSlots; set => _GemSlots = value; }
        public string Desc { get => _desc; set => _desc = value; }

        public override string ToString()
        {
            return "Armor #" + _ID + " : " + _name + "  .WEP ID : " + _WEP + " Type : " + _ArmorType + "  [STR:" + _STR + "|INT:" + _INT + "|AGI:" + _AGI + "]";
        }
        public string ToJSON()
        {
            return JsonUtility.ToJson(this);
        }
    }
}