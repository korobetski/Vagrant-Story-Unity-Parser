using System;
using UnityEngine;

namespace VS.Entity
{

    [Serializable]
    public class ARMMarker:MonoBehaviour
    {
        public enum MarkerType { door = 0, center = 1, save = 2, exit = 4, workshop = 8, container = 10, unk12 = 12, reserve = 18 };

        public uint vertexId;
        public uint exitZone;
        public MarkerType info;
        /* $00 = white point
         * $01 = no point graphics
         * $02 = yellow save
         * $04 = labelled exit
         * $08 = container + save + workshop
         * $10 = container
        */
        public uint lockId;
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
            info = (MarkerType)b[2];
            lockId = b[3];
        }
    }
}