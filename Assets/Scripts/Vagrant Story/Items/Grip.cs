using VagrantStory.Core;

namespace VagrantStory.Items
{
    public class Grip : Item
    {

        public enum GripType { NONE, GUARD, GRIP, POLE, BOLT }; // Maybe we can add a new Shield Grip Type to extend the Craft System
        public GripType gripType;
        public Statistics statistics;
        public byte gemSlots = 0;


        private GripType[] gripTypes = { GripType.NONE, GripType.GUARD, GripType.GRIP, GripType.POLE, GripType.BOLT };

        public Grip(string na, string desc, byte _gs = 0, short _str = 0, short _int = 0, short _agi = 0, short blunt = 0, short edged = 0, short piercing = 0, byte type = 0)
        {
            name = na;
            statistics = new Statistics();
            gemSlots = _gs;
            statistics.STR.value = _str;
            statistics.INT.value = _int;
            statistics.AGI.value = _agi;
            statistics.BLUNT.value = blunt;
            statistics.EDGED.value = edged;
            statistics.PIERCING.value = piercing;
            gripType = gripTypes[type];
        }
    }
}