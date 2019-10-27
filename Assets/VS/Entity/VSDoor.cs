using System;

namespace VS.Entity
{

    [Serializable]
    public class VSDoor
    {
        public uint vid;
        public uint exit;
        public DoorInfo info;
        /* $00 = white point
         * $01 = no point graphics
         * $02 = yellow save
         * $04 = labelled exit
         * $08 = container + save + workshop
         * $10 = container
        */
        public uint lid;
        /* Locked
         * Latch
         * One-Way
         * Rood Inverse
         * Named Key
         * Named Sigil
         */

        public VSDoor()
        {

        }
    }
    public enum DoorInfo { white_point = 0, none = 1, yellow_save = 2, exit = 4, workshop = 8, container = 10 };
}