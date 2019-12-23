using UnityEngine;

namespace VagrantStory.Core
{
    public class BreakArt : MonoBehaviour
    {
        public enum WeaponCategory { DAGGER, SWORD, GREAT_SWORD, AXE, MACE, GREAT_AXE, STAFF, HEAVY_MACE, POLEARM, CROSSBOW, UNARMED };

        //public string name = "";
        public string description = "";
        public WeaponCategory category;
        public uint cost; // HP cost of the break art
        public BreakArtEffect effect;
        public Affinity affinity;
        public DamageType damageType;


    }
}
