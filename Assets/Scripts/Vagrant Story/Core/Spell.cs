using UnityEngine;


namespace VagrantStory.Core
{
    public class Spell : MonoBehaviour
    {
        public enum SpellCategory { SHAMAN, ENCHANTER, WARLOCK, SORCERER, OTHER };

        //public string name = "";
        public string description = "";
        public SpellCategory category;
        public uint cost; // MP cost of the spell
        public BreakArtEffect effect;

    }
}
