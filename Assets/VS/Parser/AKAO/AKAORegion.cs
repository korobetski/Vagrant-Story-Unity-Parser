//Minoru Akao
using UnityEngine;

namespace VS.Parser.Akao
{
    public class AKAORegion
    {
        public byte articulationId;
        public byte lowRange;
        public byte hiRange;
        public byte unk1;
        public byte unk2;
        public byte unk3;
        public byte unk4;
        public byte volume;
        public byte attenuation = 0xF7;  // default to no attenuation
        public ushort relativeKey;
        public byte pan = 0x40;  // default to center pan

        public AKAOArticulation articulation;
        public AKAOSample sample;
        public uint sampleNum;
        /*
        internal double attack_time;
        internal double decay_time;
        internal double sustain_time;
        internal int sustain_level;
        internal double release_time;
        */

        internal uint unityKey;
        internal short fineTune;

        public AKAORegion()
        {

        }

        public void FeedMelodic(byte[] b)
        {
            articulationId = b[0];
            lowRange = b[1];
            hiRange = b[2];
            unk1 = b[3];
            unk2 = b[4];
            unk3 = b[5];
            unk4 = b[6];
            volume = b[7];

            Debug.Log(string.Concat("AKAORegion : articulationId : ", articulationId, "   lowRange : ", lowRange, "   hiRange : ", hiRange, "   volume : ", volume, "  |  ", unk1, ", ", unk2, ", ", unk3, ", ", unk4));
        }

        public void FeedDrum(byte[] b, int key)
        {
            articulationId = b[0];
            relativeKey = b[1];
            unk1 = b[2];
            unk2 = b[3];
            unk3 = b[4];
            unk4 = b[5];
            attenuation = b[6];
            pan = b[7];
            lowRange = (byte)key;
            hiRange = lowRange;
            volume = (byte)(attenuation / 127);

            Debug.Log(string.Concat("AKAORegion : articulationId : ", articulationId, " relativeKey : ", relativeKey, "   note : ", lowRange, "   volume : ", volume, "  |  ", unk1, ", ", unk2, ", ", unk3, ", ", unk4));
        }
    }
}
