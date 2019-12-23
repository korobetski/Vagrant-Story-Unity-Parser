using VagrantStory.Core;

namespace VagrantStory.Items
{
    public class Armor : Item
    {

        public enum ArmorType { NONE, SHIELD, HELM, ARMOR, GLOVE, BOOTS, ACCESSORY };

        public ArmorType armorType;
        public SmithMaterial material;
        public Statistics statistics;
        public byte id;
        public byte wepID; // only usefull for shields
        public byte gemSlots = 0; // only usefull for shields


        private ArmorType[] armorTypes = { ArmorType.NONE, ArmorType.SHIELD, ArmorType.HELM, ArmorType.ARMOR, ArmorType.GLOVE, ArmorType.BOOTS, ArmorType.ACCESSORY };

        public Armor(string na, string desc, byte id, byte wepid, byte type, byte gs = 0)
        {
            armorType = armorTypes[type];
            statistics = new Statistics();

            name = na;
            description = desc;
            this.id = id;
            this.wepID = wepid;
            gemSlots = gs;
        }

        public Armor(string na, string desc, byte id, byte wepid, byte type, short str, short inte, short agi, byte gs = 0)
        {
            armorType = armorTypes[type];
            statistics = new Statistics();

            name = na;
            description = desc;
            this.id = id;
            this.wepID = wepid;

            statistics.STR.value = str;
            statistics.INT.value = inte;
            statistics.AGI.value = agi;
            gemSlots = gs;

        }
    }
}