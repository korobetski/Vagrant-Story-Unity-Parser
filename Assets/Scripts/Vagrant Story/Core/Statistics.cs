using MyBox;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VagrantStory.Core
{

    // http://www.lerpg.com/divers/vagrantstory/vagrant.txt

    [Serializable]
    public class Statistics
    {

        private Attribute _STR;
        private Attribute _INT;
        private Attribute _AGI;

        private Class _HUMAN;
        private Class _BEAST;
        private Class _UNDEAD;
        private Class _PHANTOM;
        private Class _DRAGON;
        private Class _EVIL;

        private Affinity _PHYSICAL;
        private Affinity _AIR;
        private Affinity _FIRE;
        private Affinity _EARTH;
        private Affinity _WATER;
        private Affinity _LIGHT;
        private Affinity _DARK;

        private DamageType _BLUNT;
        private DamageType _EDGED;
        private DamageType _PIERCING;


        public Attribute STR { get => _STR; set { _STR = value; STRValue = value.value; } }
        public Attribute INT { get => _INT; set { _INT = value; INTValue = value.value; } }
        public Attribute AGI { get => _AGI; set { _AGI = value; AGIValue = value.value; } }

        public Class HUMAN { get => _HUMAN; set { _HUMAN = value; HUMANValue = value.value; } }
        public Class BEAST { get => _BEAST; set { _BEAST = value; BEASTValue = value.value; } }
        public Class UNDEAD { get => _UNDEAD; set { _UNDEAD = value; UNDEADValue = value.value; } }
        public Class PHANTOM { get => _PHANTOM; set { _PHANTOM = value; PHANTOMValue = value.value; } }
        public Class DRAGON { get => _DRAGON; set { _DRAGON = value; DRAGONValue = value.value; } }
        public Class EVIL { get => _EVIL; set { _EVIL = value; EVILValue = value.value; } }

        public Affinity PHYSICAL { get => _PHYSICAL; set { _PHYSICAL = value; PHYSICALValue = value.value; } }
        public Affinity AIR { get => _AIR; set { _AIR = value; AIRValue = value.value; } }
        public Affinity FIRE { get => _FIRE; set { _FIRE = value; FIREValue = value.value; } }
        public Affinity EARTH { get => _EARTH; set { _EARTH = value; EARTHValue = value.value; } }
        public Affinity WATER { get => _WATER; set { _WATER = value; WATERValue = value.value; } }
        public Affinity LIGHT { get => _LIGHT; set { _LIGHT = value; LIGHTValue = value.value; } }
        public Affinity DARK { get => _DARK; set { _DARK = value; DARKValue = value.value; } }

        public DamageType BLUNT { get => _BLUNT; set { _BLUNT = value; BLUNTValue = value.value; } }
        public DamageType EDGED { get => _EDGED; set { _EDGED = value; EDGEDValue = value.value; } }
        public DamageType PIERCING { get => _PIERCING; set { _PIERCING = value; PIERCINGValue = value.value; } }


        [ReadOnly] [Range(-255, 255)] public float STRValue;
        [ReadOnly] [Range(-255, 255)] public float INTValue;
        [ReadOnly] [Range(-255, 255)] public float AGIValue;

        [ReadOnly] [Range(-100, 100)] public float HUMANValue;
        [ReadOnly] [Range(-100, 100)] public float BEASTValue;
        [ReadOnly] [Range(-100, 100)] public float UNDEADValue;
        [ReadOnly] [Range(-100, 100)] public float PHANTOMValue;
        [ReadOnly] [Range(-100, 100)] public float DRAGONValue;
        [ReadOnly] [Range(-100, 100)] public float EVILValue;

        [ReadOnly] [Range(-100, 100)] public float PHYSICALValue;
        [ReadOnly] [Range(-100, 100)] public float AIRValue;
        [ReadOnly] [Range(-100, 100)] public float FIREValue;
        [ReadOnly] [Range(-100, 100)] public float EARTHValue;
        [ReadOnly] [Range(-100, 100)] public float WATERValue;
        [ReadOnly] [Range(-100, 100)] public float LIGHTValue;
        [ReadOnly] [Range(-100, 100)] public float DARKValue;

        [ReadOnly] [Range(-100, 100)] public float BLUNTValue;
        [ReadOnly] [Range(-100, 100)] public float EDGEDValue;
        [ReadOnly] [Range(-100, 100)] public float PIERCINGValue;




        public Statistics()
        {
            STR = new Attribute("STR", 0);
            INT = new Attribute("INT", 0);
            AGI = new Attribute("AGI", 0);

            HUMAN = new Class("HUMAN", 0);
            BEAST = new Class("BEAST", 0);
            UNDEAD = new Class("UNDEAD", 0);
            PHANTOM = new Class("PHANTOM", 0);
            DRAGON = new Class("DRAGON", 0);
            EVIL = new Class("EVIL", 0);

            PHYSICAL = new Affinity("PHYSICAL", 0);
            AIR = new Affinity("AIR", 0, EARTH);
            FIRE = new Affinity("FIRE", 0, WATER);
            EARTH = new Affinity("EARTH", 0, AIR);
            WATER = new Affinity("WATER", 0, EARTH);
            LIGHT = new Affinity("LIGHT", 0, DARK);
            DARK = new Affinity("DARK", 0, LIGHT);

            BLUNT = new DamageType("BLUNT", 0);
            EDGED = new DamageType("EDGED", 0);
            PIERCING = new DamageType("PIERCING", 0);
        }
        public Statistics(string pow, string type)
        {
            STR = new Attribute("STR", 0);
            INT = new Attribute("INT", 0);
            AGI = new Attribute("AGI", 0);

            HUMAN = new Class("HUMAN", 0);
            BEAST = new Class("BEAST", 0);
            UNDEAD = new Class("UNDEAD", 0);
            PHANTOM = new Class("PHANTOM", 0);
            DRAGON = new Class("DRAGON", 0);
            EVIL = new Class("EVIL", 0);

            PHYSICAL = new Affinity("PHYSICAL", 0);
            AIR = new Affinity("AIR", 0, EARTH);
            FIRE = new Affinity("FIRE", 0, WATER);
            EARTH = new Affinity("EARTH", 0, AIR);
            WATER = new Affinity("WATER", 0, EARTH);
            LIGHT = new Affinity("LIGHT", 0, DARK);
            DARK = new Affinity("DARK", 0, LIGHT);

            BLUNT = new DamageType("BLUNT", 0);
            EDGED = new DamageType("EDGED", 0);
            PIERCING = new DamageType("PIERCING", 0);

            List<string> classes = new List<string> { "HUMAN", "BEAST", "UNDEAD", "PHANTOM", "DRAGON", "EVIL" };
            List<string> elementals = new List<string> { "PHYSICAL", "AIR", "FIRE", "EARTH", "WATER", "LIGHT", "DARK" };
            if (pow == "MINOR")
            {
                STR.value = 2;
                INT.value = 4;
                AGI.value = 3;
                switch (type)
                {
                    case "HUMAN":
                        HUMAN.value = 15;
                        BEAST.value = -3;
                        UNDEAD.value = -3;
                        break;
                    case "BEAST":
                        BEAST.value = 15;
                        UNDEAD.value = -3;
                        PHANTOM.value = -3;
                        break;
                    case "UNDEAD":
                        UNDEAD.value = 15;
                        PHANTOM.value = -3;
                        DRAGON.value = -3;
                        break;
                    case "PHANTOM":
                        PHANTOM.value = 15;
                        DRAGON.value = -3;
                        EVIL.value = -3;
                        break;
                    case "DRAGON":
                        DRAGON.value = 15;
                        EVIL.value = -3;
                        HUMAN.value = -3;
                        break;
                    case "EVIL":
                        EVIL.value = 15;
                        HUMAN.value = -3;
                        BEAST.value = -3;
                        break;
                    case "PHYSICAL":
                        PHYSICAL.value = 15;
                        break;
                    case "AIR":
                        AIR.value = 15;
                        EARTH.value = -5;
                        break;
                    case "FIRE":
                        FIRE.value = 15;
                        WATER.value = -5;
                        break;
                    case "EARTH":
                        EARTH.value = 15;
                        AIR.value = -5;
                        break;
                    case "WATER":
                        WATER.value = 15;
                        FIRE.value = -5;
                        break;
                    case "LIGHT":
                        LIGHT.value = 15;
                        DARK.value = -5;
                        break;
                    case "DARK":
                        DARK.value = 15;
                        LIGHT.value = -5;
                        break;
                }
            }
            if (pow == "MAJOR")
            {
                STR.value = 1;
                INT.value = 6;
                AGI.value = 3;
                if (classes.Contains(type))
                {
                    HUMAN.value = -3;
                    BEAST.value = -3;
                    UNDEAD.value = -3;
                    PHANTOM.value = -3;
                    DRAGON.value = -3;
                    EVIL.value = -3;
                }
                if (elementals.Contains(type))
                {
                    PHYSICAL.value = -5;
                    AIR.value = -5;
                    FIRE.value = -5;
                    EARTH.value = -5;
                    WATER.value = -5;
                    LIGHT.value = -5;
                    DARK.value = -5;
                }
                switch (type)
                {
                    case "HUMAN":
                        HUMAN.value = 30;
                        BEAST.value = -6;
                        UNDEAD.value = -6;
                        break;
                    case "BEAST":
                        BEAST.value = 30;
                        UNDEAD.value = -6;
                        PHANTOM.value = -6;
                        break;
                    case "UNDEAD":
                        UNDEAD.value = 30;
                        PHANTOM.value = -6;
                        DRAGON.value = -6;
                        break;
                    case "PHANTOM":
                        PHANTOM.value = 30;
                        DRAGON.value = -6;
                        EVIL.value = -6;
                        break;
                    case "DRAGON":
                        DRAGON.value = 30;
                        EVIL.value = -6;
                        HUMAN.value = -6;
                        break;
                    case "EVIL":
                        EVIL.value = 30;
                        HUMAN.value = -6;
                        BEAST.value = -6;
                        break;
                    case "PHYSICAL":
                        PHYSICAL.value = 30;
                        break;
                    case "AIR":
                        AIR.value = 30;
                        EARTH.value = -15;
                        break;
                    case "FIRE":
                        FIRE.value = 30;
                        WATER.value = -15;
                        break;
                    case "EARTH":
                        EARTH.value = 30;
                        AIR.value = -15;
                        break;
                    case "WATER":
                        WATER.value = 30;
                        FIRE.value = -15;
                        break;
                    case "LIGHT":
                        LIGHT.value = 30;
                        DARK.value = -15;
                        break;
                    case "DARK":
                        DARK.value = 30;
                        LIGHT.value = -15;
                        break;
                }
            }
            if (pow == "EXCEP")
            {
                if (type == "POLARIS" || type == "BASIVALEN" || type == "GALERIAN")
                {
                    STR.value = -3;
                    INT.value = 12;
                    PHYSICAL.value = 20;
                    FIRE.value = -10;
                    WATER.value = -10;
                    AIR.value = -10;
                    EARTH.value = -10;
                    LIGHT.value = -10;
                    DARK.value = -10;
                }
                switch (type)
                {
                    case "POLARIS":
                        AIR.value = 20;
                        EARTH.value = 20;
                        break;
                    case "BASIVALEN":
                        FIRE.value = 20;
                        WATER.value = 20;
                        break;
                    case "GALERIAN":
                        LIGHT.value = 20;
                        DARK.value = 20;
                        break;
                    case "VEDIVIER":
                        STR.value = 1;
                        INT.value = 1;
                        AGI.value = 1;
                        PHYSICAL.value = 5;
                        FIRE.value = 5;
                        WATER.value = 5;
                        AIR.value = 5;
                        EARTH.value = 5;
                        LIGHT.value = 5;
                        DARK.value = 5;
                        HUMAN.value = 5;
                        BEAST.value = 5;
                        UNDEAD.value = 5;
                        PHANTOM.value = 5;
                        DRAGON.value = 5;
                        EVIL.value = 5;
                        break;
                    case "BERION":
                        STR.value = 2;
                        INT.value = 3;
                        AGI.value = 2;
                        PHYSICAL.value = 10;
                        FIRE.value = 10;
                        WATER.value = 10;
                        AIR.value = 10;
                        EARTH.value = 10;
                        LIGHT.value = 10;
                        DARK.value = 10;
                        break;
                    case "GERVIN":
                        STR.value = 3;
                        INT.value = 6;
                        AGI.value = 3;
                        PHYSICAL.value = 15;
                        FIRE.value = 15;
                        WATER.value = 15;
                        AIR.value = 15;
                        EARTH.value = 15;
                        LIGHT.value = 15;
                        DARK.value = 15;
                        HUMAN.value = 15;
                        BEAST.value = 15;
                        UNDEAD.value = 15;
                        PHANTOM.value = 15;
                        DRAGON.value = 15;
                        EVIL.value = 15;
                        break;
                    case "TERTIA":
                        STR.value = 4;
                        INT.value = 9;
                        AGI.value = 4;
                        PHYSICAL.value = 20;
                        FIRE.value = 20;
                        WATER.value = 20;
                        AIR.value = 20;
                        EARTH.value = 20;
                        LIGHT.value = 20;
                        DARK.value = 20;
                        HUMAN.value = 20;
                        BEAST.value = 20;
                        UNDEAD.value = 20;
                        PHANTOM.value = 20;
                        DRAGON.value = 20;
                        EVIL.value = 20;
                        break;
                    case "LANCER":
                        STR.value = 5;
                        INT.value = 12;
                        AGI.value = 5;
                        PHYSICAL.value = 25;
                        FIRE.value = 25;
                        WATER.value = 25;
                        AIR.value = 25;
                        EARTH.value = 25;
                        LIGHT.value = 25;
                        DARK.value = 25;
                        HUMAN.value = 25;
                        BEAST.value = 25;
                        UNDEAD.value = 25;
                        PHANTOM.value = 25;
                        DRAGON.value = 25;
                        EVIL.value = 25;
                        break;
                    case "ARTUROS":
                        STR.value = 8;
                        INT.value = 15;
                        AGI.value = 8;
                        PHYSICAL.value = 30;
                        FIRE.value = 30;
                        WATER.value = 30;
                        AIR.value = 30;
                        EARTH.value = 30;
                        LIGHT.value = 30;
                        DARK.value = 30;
                        HUMAN.value = 30;
                        BEAST.value = 30;
                        UNDEAD.value = 30;
                        PHANTOM.value = 30;
                        DRAGON.value = 30;
                        EVIL.value = 30;
                        break;
                }
            }
            if (pow == "ATTACK" || pow == "PROTECTION")
            {
                PHYSICAL.value = 3;
                FIRE.value = 3;
                WATER.value = 3;
                AIR.value = 3;
                EARTH.value = 3;
                LIGHT.value = 3;
                DARK.value = 3;
                HUMAN.value = 3;
                BEAST.value = 3;
                UNDEAD.value = 3;
                PHANTOM.value = 3;
                DRAGON.value = 3;
                EVIL.value = 3;
                if (pow == "ATTACK")
                {
                    STR.value = 2;
                    INT.value = 0;
                    AGI.value = 5;
                    if (type == "HIT") { }
                    if (type == "SPELL") { }
                }
                if (pow == "PROTECTION")
                {
                    AGI.value = 1;
                }

                if (type == "EVADE")
                {
                    STR.value = 2;
                    INT.value = 0;
                    AGI.value = 5;
                }
                if (type == "SPELLEVADE")
                {
                    STR.value = 0;
                    INT.value = 3;
                    AGI.value = 5;
                }
            }
        }

        public void SetAttributes(short str, short inte, short agi)
        {
            STR.value = str;
            INT.value = inte;
            AGI.value = agi;
        }

        public void SetClasses(short hum, short bea, short und, short pha, short dra, short evi)
        {
            HUMAN.value = hum;
            BEAST.value = bea;
            UNDEAD.value = und;
            PHANTOM.value = pha;
            DRAGON.value = dra;
            EVIL.value = evi;
        }

        public void SetAffinities(short phy, short air, short fir, short ear, short wat, short lig, short dar)
        {
            PHYSICAL.value = phy;
            AIR.value = air;
            FIRE.value = fir;
            EARTH.value = ear;
            WATER.value = wat;
            LIGHT.value = lig;
            DARK.value = dar;
        }

        public void SetDamageTypes(short blu, short edg, short pie)
        {
            BLUNT.value = blu;
            EDGED.value = edg;
            PIERCING.value = pie;
        }

        public static Statistics operator +(Statistics lhs, Statistics rhs)
        {
            lhs.HUMAN += rhs.HUMAN;
            lhs.BEAST += rhs.BEAST;
            lhs.UNDEAD += rhs.UNDEAD;
            lhs.PHANTOM += rhs.PHANTOM;
            lhs.DRAGON += rhs.DRAGON;
            lhs.EVIL += rhs.EVIL;
            lhs.PHYSICAL += rhs.PHYSICAL;
            lhs.AIR += rhs.AIR;
            lhs.FIRE += rhs.FIRE;
            lhs.EARTH += rhs.EARTH;
            lhs.WATER += rhs.WATER;
            lhs.LIGHT += rhs.LIGHT;
            lhs.DARK += rhs.DARK;
            lhs.BLUNT += rhs.BLUNT;
            lhs.EDGED += rhs.EDGED;
            lhs.PIERCING += rhs.PIERCING;
            lhs.STR += rhs.STR;
            lhs.INT += rhs.INT;
            lhs.AGI += rhs.AGI;
            return lhs;
        }
    }
}
