//Minoru Akao

namespace VS.Parser.Akao
{
    public class AKAORegion
    {
        public byte articulationId;
        public byte lowRange = 0;
        public byte hiRange = 179; // 179
        public byte lowVel = 0;
        public byte hiVel = 127;
        public byte unk3;
        public byte unk4;
        public byte volume = 127;
        public byte attenuation = 0xF7;  // default to no attenuation
        public ushort relativeKey;
        public byte pan = 0x40;  // default to center pan

        public AKAOArticulation articulation;
        public AKAOSample sample;
        public uint sampleNum;

        internal uint unityKey;
        internal short fineTune;
        internal bool isDrum = false;

        public AKAORegion()
        {
        }

        public void FeedMelodic(byte[] b)
        {
            isDrum = false;
            articulationId = b[0];
            lowRange = b[1];
            hiRange = b[2];
            lowVel = b[3];
            hiVel = b[4];
            unk3 = b[5];
            unk4 = b[6];
            volume = b[7];
            attenuation = (byte)((1440 - (volume + lowVel) * 5) / 10);
        }

        public void FeedDrum(byte[] b, int key)
        {
            isDrum = true;
            articulationId = b[0];
            relativeKey = b[1];
            lowVel = b[2];
            hiVel = b[3];
            unk3 = b[4];
            unk4 = b[5];
            attenuation = b[6];
            pan = b[7];
            lowRange = (byte)key;
            hiRange = lowRange;
            volume = (byte)(attenuation / 127);
        }

        public override string ToString()
        {
            string str = "";
            if (isDrum)
            {
                str = string.Concat("AKAORegion Drum : articulationId : ", articulationId, " relativeKey : ", relativeKey, "   note : ", lowRange,
                    "   attenuation : ", attenuation, "  |  ", lowVel, ", ", hiVel, ", ", unk3, ", ", unk4);
            }
            else
            {
                str = string.Concat("AKAORegion : articulationId : ", articulationId, "   lowRange : ", lowRange, "   hiRange : ", hiRange,
                    "   volume : ", volume, "  |  ", lowVel, ", ", hiVel, ", ", unk3, ", ", unk4);
            }
            return str;
        }
    }
}
