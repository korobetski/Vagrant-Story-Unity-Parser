using VagrantStory.Core;

namespace VagrantStory.Items
{
    public class Blade : Item
    {

        public enum BladeType { NONE = 0, DAGGER = 1, SWORD = 2, GREAT_SWORD = 3, AXE = 4, MACE = 5, GREAT_AXE = 6, STAFF = 7, HEAVY_MACE = 8, POLEARM = 9, CROSSBOW = 10 };
        public enum DamageType { NONE, BLUNT, EDGED, PIERCING };


        public BladeType bladeType;
        public SmithMaterial material;
        public DamageType damageType; // Main damage type
        public Statistics statistics;
        public byte id;
        public byte wepID;
        public byte range;
        public byte risk;


        private BladeType[] bladeTypes = { BladeType.NONE, BladeType.DAGGER, BladeType.SWORD, BladeType.GREAT_SWORD, BladeType.AXE,
            BladeType.MACE, BladeType.GREAT_AXE, BladeType.STAFF, BladeType.HEAVY_MACE, BladeType.POLEARM, BladeType.CROSSBOW };
        private DamageType[] damageTypes = { DamageType.NONE, DamageType.BLUNT, DamageType.EDGED, DamageType.PIERCING };

        public Blade(BladeType t)
        {
            bladeType = t;
            statistics = new Statistics();
        }

        public Blade(string na, string desc, byte id, byte wep, byte type, byte damtyp, byte risk, short str, short inte, short agi, byte range, byte damage)
        {
            bladeType = bladeTypes[type];
            statistics = new Statistics();

            name = na;
            description = desc;
            this.id = id;
            wepID = wep;
            damageType = damageTypes[damtyp];
            this.risk = risk;

            statistics.STR.value = str;
            statistics.INT.value = inte;
            statistics.AGI.value = agi;

            this.range = range;

        }
    }
}