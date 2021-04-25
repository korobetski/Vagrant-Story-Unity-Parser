using System;

namespace VS.FileFormats.AKAO
{
    [Serializable]
    public class AKAORegion
    {
        public byte articulationId;
        public byte lowKey = 0;
        public byte hiKey = 179;
        public byte relativeKey = 179;
        public byte ar;                     // ADSR: attack rate (0-127)
        public byte sr;                     // ADSR: sustain rate (0-127)
        public Enums.AKAO.ADSRMode sm;      // ADSR: sustain mode (1: linear increase, 3: linear decrease, 5: exponential increase, 7: exponential decrease)
        public byte rr;                     // ADSR: release rate (0-31)
        //public byte volume = 127;           
        public byte attenuation = 0x00;     // adjust the note volume to n/128 of the original volume (0 will keep the original volume)


        public byte pan = 0x40;  // default to center pan
        public bool reverb = false;

        public void FeedMelodic(byte[] b)
        {
            articulationId = b[0];
            lowKey = b[1];
            hiKey = b[2];
            ar = b[3];
            sr = b[4];
            sm = (Enums.AKAO.ADSRMode)b[5];
            rr = b[6];
            attenuation = b[7];
        }

        public void FeedDrum(byte[] b, int key)
        {
            articulationId = b[0];
            lowKey = (byte)key;
            hiKey = lowKey;
            relativeKey = b[1];
            ar = b[2];
            sr = b[3];
            sm = (Enums.AKAO.ADSRMode)b[4];
            rr = b[5];
            attenuation = b[6];

            byte raw_pan_reverb = b[7];
            pan = (byte)(raw_pan_reverb & 0x7f);
            reverb = (bool)((raw_pan_reverb & 0x80) != 0);
        }
    }
}
