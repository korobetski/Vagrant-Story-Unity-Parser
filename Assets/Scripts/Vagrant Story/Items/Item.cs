
using System;

namespace VagrantStory.Items
{
    // Generic class for all items in the game
    [Serializable]
    public class Item
    {
        public enum ItemType { MISC, GEM, KEY, WEAPON, ARMOR, BLADE, GRIP, GRIMOIRE, FOOD };

        public string name = "";
        public string description = "";
        public ItemType type = ItemType.MISC;
        public bool stackable = true;
        public uint quantity = 0;
    }

    public class ItemEffect
    {
        public enum Target { SELF, TARGET, VOID };
        public enum Type { MODIFIER, PERMA_MOD, ADD_STATUS, REMOVE_STATUS };
        public enum Mod { NULL, HP, MP, RISK, STR, INT, AGI };
        public enum Status { NULL, Poison, Paralysis, Numbness, Curse, Speed, Argon, ALL_DEBUFF };
        public static short Roll(uint v)
        {
            return (short)UnityEngine.Random.Range(1, v);
        }


        public Target target = Target.VOID;
        public Type type = Type.MODIFIER;
        public Mod mod = Mod.NULL;
        public Status status = Status.NULL;
        public short value;



        public ItemEffect(Target _target, Type _type, Mod _mod)
        {
            target = _target;
            type = _type;
            mod = _mod;
        }

        public ItemEffect(Target _target, Type _type, Mod _mod, short _value)
        {
            target = _target;
            type = _type;
            mod = _mod;
            value = _value;
        }

        public ItemEffect(Target _target, Type _type, Status _status)
        {
            target = _target;
            type = _type;
            status = _status;
        }
        public ItemEffect(Target _target, Type _type, Status _status, short _duration)
        {
            target = _target;
            type = _type;
            status = _status;
            value = _duration;
        }
    }
}
