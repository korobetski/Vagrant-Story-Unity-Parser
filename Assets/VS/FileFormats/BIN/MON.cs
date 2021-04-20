using System;
using UnityEngine;

namespace VS.FileFormats.BIN
{
    [Serializable]
    public class MON
    {
        public string name; // 28 bytes
        [TextArea]
        public string description; // variable
        public ushort PtrDesc;
        public ushort zudToDisplay;
        public Enums.Class.Type type;
        public ushort zudToKill; // value always near or equal zudId, lock or unlock bestiary entry
        public ushort unk; // most case 2, several 4 (Chronos, Amazone, Chrono Knight, Styx),  only one case 6 Chimère, lock the entry when set to 0
    }

}
