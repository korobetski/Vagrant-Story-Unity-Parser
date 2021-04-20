using System;

namespace VS.FileFormats.ARM
{

    [Serializable]
    public class ARMMarker
    {
        public enum MarkerType { DOOR = 0, CENTER = 1, SAVE = 2, EXIT = 4, WORKSHOP = 8, CONTAINER = 10, SAVE_CONTAINER = 12, WORKSHOP2 = 18 };

        public string name;
        public uint vertexId;
        public uint exitZone;
        public MarkerType type;
        public uint lockId; // http://datacrystal.romhacking.net/wiki/Vagrant_Story:misc_items_list
        /* Locked
         * Latch
         * One-Way
         * Rood Inverse
         * Named Key
         * Named Sigil
         */

        public void SetDatas(byte[] b)
        {
            vertexId = b[0];
            exitZone = b[1];
            type = (MarkerType)b[2];
            name = (string)type.ToString();
            lockId = b[3];
        }
    }
}