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

        internal double attack_time;
        internal double decay_time;
        internal double sustain_time;
        internal int sustain_level;
        internal double release_time;
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

            Debug.Log(string.Concat("articulationId : ", articulationId, "   lowRange : ", lowRange, "   hiRange : ", hiRange, "   volume : ", volume));
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
        }

        public void ComputeADSR()
        {
            //Need to fix this
            //ToolBox.PSXConvADSR(articulation.adr1, articulation.adr2);

        }

        public void PSXConvADSR(ushort ADSR1, ushort ADSR2)
        {
            ushort Am = (ushort)((ADSR1 & 0x8000) >> 15);  // if 1, then Exponential, else linear
            ushort Ar = (ushort)((ADSR1 & 0x7F00) >> 8);
            ushort Dr = (ushort)((ADSR1 & 0x00F0) >> 4);
            ushort Sl = (ushort)(ADSR1 & 0x000F);
            ushort Rm = (ushort)((ADSR2 & 0x0020) >> 5);
            ushort Rr = (ushort)(ADSR2 & 0x001F);

            // The following are unimplemented in conversion (because DLS and SF2 do not support Sustain
            // Rate)
            ushort Sm = (ushort)((ADSR2 & 0x8000) >> 15);
            ushort Sd = (ushort)((ADSR2 & 0x4000) >> 14);
            ushort Sr = (ushort)((ADSR2 >> 6) & 0x7F);

            // Make sure all the ADSR values are within the valid ranges
            if (((Am & ~0x01) != 0) || ((Ar & ~0x7F) != 0) || ((Dr & ~0x0F) != 0) || ((Sl & ~0x0F) != 0) ||
                ((Rm & ~0x01) != 0) || ((Rr & ~0x1F) != 0) || ((Sm & ~0x01) != 0) || ((Sd & ~0x01) != 0) ||
                ((Sr & ~0x7F) != 0))
            {
                Debug.LogError("ADSR parameter(s) out of range (Am : " + Am + ", Ar : " + Ar + ", Dr : " + Dr + ", Sl : " + Sl + ", Rm : " + Rm + ", Rr : " + Rr + ", Sm : " + Sm + ", Sd : " + Sd + ", Sr : " + Sr + ")");

                return;
            }

            // PS1 games use 44k, PS2 uses 48k
            double sampleRate = 44100;

            int[] rateIncTable = { 0, 4, 6, 8, 9, 10, 11, 12 };
            ulong envelope_level;
            double samples = 0;
            ulong rate;
            ulong remainder;
            double timeInSecs;
            int l;

            ulong r, rs, rd;
            int i;
            // build the rate table according to Neill's rules
            ulong[] RateTable = new ulong[160];

            r = 3;
            rs = 1;
            rd = 0;

            // we start at pos 32 with the real values... everything before is 0
            for (i = 32; i < 160; i++)
            {
                if (r < 0x3FFFFFFF)
                {
                    r += rs;
                    rd++;
                    if (rd == 5)
                    {
                        rd = 1;
                        rs *= 2;
                    }
                }
                if (r > 0x3FFFFFFF)
                {
                    r = 0x3FFFFFFF;
                }

                RateTable[i] = r;
            }

            // to get the dls 32 bit time cents, take log base 2 of number of seconds * 1200 * 65536
            // (dls1v11a.pdf p25).

            //	if (RateTable[(Ar^0x7F)-0x10 + 32] == 0)
            //		realADSR->attack_time = 0;
            //	else
            //	{
            if ((Ar ^ 0x7F) < 0x10)
            {
                Ar = 0;
            }
            // if linear Ar Mode

            if (Am == 0)
            {
                rate = RateTable[Mathf.FloorToInt((Ar ^ 0x7F) - 0x10 + 32)];
                samples = Mathf.CeilToInt(0x7FFFFFFF / rate);
            }
            else if (Am == 1)
            {
                rate = RateTable[Mathf.FloorToInt((Ar ^ 0x7F) - 0x10) + 32];
                samples = 0x60000000 / rate;
                remainder = 0x60000000 % rate;
                rate = RateTable[Mathf.FloorToInt((Ar ^ 0x7F) - 0x18) + 32];
                samples += Mathf.CeilToInt(Mathf.Max(0, 0x1FFFFFFF - remainder) / rate);
            }

            timeInSecs = samples / sampleRate;


            attack_time = timeInSecs;
            //	}

            // Decay Time

            envelope_level = 0x7FFFFFFF;

            bool bSustainLevFound = false;
            uint realSustainLevel = 0;
            // DLS decay rate value is to -96db (silence) not the sustain level
            for (l = 0; envelope_level > 0; l++)
            {
                if (4 * (Dr ^ 0x1F) < 0x18)
                {
                    Dr = 0;
                }

                switch ((envelope_level >> 28) & 0x7)
                {
                    case 0:
                        envelope_level -= RateTable[Mathf.FloorToInt((4 * (Dr ^ 0x1F)) - 0x18 + 0) + 32];
                        break;
                    case 1:
                        envelope_level -= RateTable[Mathf.FloorToInt((4 * (Dr ^ 0x1F)) - 0x18 + 4) + 32];
                        break;
                    case 2:
                        envelope_level -= RateTable[Mathf.FloorToInt((4 * (Dr ^ 0x1F)) - 0x18 + 6) + 32];
                        break;
                    case 3:
                        envelope_level -= RateTable[Mathf.FloorToInt((4 * (Dr ^ 0x1F)) - 0x18 + 8) + 32];
                        break;
                    case 4:
                        envelope_level -= RateTable[Mathf.FloorToInt((4 * (Dr ^ 0x1F)) - 0x18 + 9) + 32];
                        break;
                    case 5:
                        envelope_level -= RateTable[Mathf.FloorToInt((4 * (Dr ^ 0x1F)) - 0x18 + 10) + 32];
                        break;
                    case 6:
                        envelope_level -= RateTable[Mathf.FloorToInt((4 * (Dr ^ 0x1F)) - 0x18 + 11) + 32];
                        break;
                    case 7:
                        envelope_level -= RateTable[Mathf.FloorToInt((4 * (Dr ^ 0x1F)) - 0x18 + 12) + 32];
                        break;
                }
                if (!bSustainLevFound && ((envelope_level >> 27) & 0xF) <= Sl)
                {
                    realSustainLevel = (uint)envelope_level;
                    bSustainLevFound = true;
                }
            }
            samples = l;
            timeInSecs = samples / sampleRate;
            decay_time = timeInSecs;

            // Sustain Rate

            envelope_level = 0x7FFFFFFF;
            // increasing... we won't even bother
            if (Sd == 0)
            {
                sustain_time = -1;
            }
            else
            {
                if (Sr == 0x7F)
                {
                    sustain_time = -1;  // this is actually infinite
                }
                else
                {
                    // linear
                    if (Sm == 0)
                    {
                        rate = RateTable[Mathf.FloorToInt((Sr ^ 0x7F) - 0x0F) + 32];
                        samples = Mathf.CeilToInt(0x7FFFFFFF / rate);
                    }
                    else
                    {
                        l = 0;
                        // DLS decay rate value is to -96db (silence) not the sustain level
                        while (envelope_level > 0)
                        {
                            ulong envelope_level_diff = 0;
                            ulong envelope_level_target = 0;

                            switch ((envelope_level >> 28) & 0x7)
                            {
                                case 0:
                                    envelope_level_target = 0x00000000;
                                    envelope_level_diff =
                                        RateTable[Mathf.FloorToInt((Sr ^ 0x7F) - 0x1B + 0) + 32];
                                    break;
                                case 1:
                                    envelope_level_target = 0x0fffffff;
                                    envelope_level_diff =
                                        RateTable[Mathf.FloorToInt((Sr ^ 0x7F) - 0x1B + 4) + 32];
                                    break;
                                case 2:
                                    envelope_level_target = 0x1fffffff;
                                    envelope_level_diff =
                                        RateTable[Mathf.FloorToInt((Sr ^ 0x7F) - 0x1B + 6) + 32];
                                    break;
                                case 3:
                                    envelope_level_target = 0x2fffffff;
                                    envelope_level_diff =
                                        RateTable[Mathf.FloorToInt((Sr ^ 0x7F) - 0x1B + 8) + 32];
                                    break;
                                case 4:
                                    envelope_level_target = 0x3fffffff;
                                    envelope_level_diff =
                                        RateTable[Mathf.FloorToInt((Sr ^ 0x7F) - 0x1B + 9) + 32];
                                    break;
                                case 5:
                                    envelope_level_target = 0x4fffffff;
                                    envelope_level_diff =
                                        RateTable[Mathf.FloorToInt((Sr ^ 0x7F) - 0x1B + 10) + 32];
                                    break;
                                case 6:
                                    envelope_level_target = 0x5fffffff;
                                    envelope_level_diff =
                                        RateTable[Mathf.FloorToInt((Sr ^ 0x7F) - 0x1B + 11) + 32];
                                    break;
                                case 7:
                                    envelope_level_target = 0x6fffffff;
                                    envelope_level_diff =
                                        RateTable[Mathf.FloorToInt((Sr ^ 0x7F) - 0x1B + 12) + 32];
                                    break;
                            }

                            ulong steps =
                                (envelope_level - envelope_level_target + (envelope_level_diff - 1)) /
                                envelope_level_diff;
                            envelope_level -= (envelope_level_diff * steps);
                            l += (int)steps;
                        }
                        samples = l;
                    }
                    timeInSecs = samples / sampleRate;
                    sustain_time = LinAmpDecayTimeToLinDBDecayTime(timeInSecs, 0x800);
                }
            }

            // Sustain Level
            // realADSR->sustain_level =
            // (double)envelope_level/(double)0x7FFFFFFF;//(long)ceil((double)envelope_level *
            // 0.030517578139210854);	//in DLS, sustain level is measured as a percentage
            if (Sl == 0)
            {
                realSustainLevel = 0x07FFFFFF;
            }

            sustain_level = (int)realSustainLevel / 0x7FFFFFFF;

            // If decay is going unused, and there's a sustain rate with sustain level close to max...
            //  we'll put the sustain_rate in place of the decay rate.
            if ((decay_time < 2 || (Dr == 0x0F && Sl >= 0x0C)) && Sr < 0x7E && Sd == 1)
            {
                sustain_level = 0;
                decay_time = sustain_time;
                // realADSR->decay_time = 0.5;
            }

            // Release Time

            // sustain_envelope_level = envelope_level;

            // We do this because we measure release time from max volume to 0, not from sustain level to 0
            envelope_level = 0x7FFFFFFF;

            // if linear Rr Mode
            if (Rm == 0)
            {
                rate = RateTable[Mathf.FloorToInt((4 * (Rr ^ 0x1F)) - 0x0C) + 32];

                if (rate != 0)
                {
                    samples = Mathf.Ceil(envelope_level / rate);
                }
                else
                {
                    samples = 0;
                }
            }
            else if (Rm == 1)
            {
                if ((Rr ^ 0x1F) * 4 < 0x18)
                {
                    Rr = 0;
                }

                for (l = 0; envelope_level > 0; l++)
                {
                    switch ((envelope_level >> 28) & 0x7)
                    {
                        case 0:
                            envelope_level -= RateTable[Mathf.FloorToInt((4 * (Rr ^ 0x1F)) - 0x18 + 0) + 32];
                            break;
                        case 1:
                            envelope_level -= RateTable[Mathf.FloorToInt((4 * (Rr ^ 0x1F)) - 0x18 + 4) + 32];
                            break;
                        case 2:
                            envelope_level -= RateTable[Mathf.FloorToInt((4 * (Rr ^ 0x1F)) - 0x18 + 6) + 32];
                            break;
                        case 3:
                            envelope_level -= RateTable[Mathf.FloorToInt((4 * (Rr ^ 0x1F)) - 0x18 + 8) + 32];
                            break;
                        case 4:
                            envelope_level -= RateTable[Mathf.FloorToInt((4 * (Rr ^ 0x1F)) - 0x18 + 9) + 32];
                            break;
                        case 5:
                            envelope_level -= RateTable[Mathf.FloorToInt((4 * (Rr ^ 0x1F)) - 0x18 + 10) + 32];
                            break;
                        case 6:
                            envelope_level -= RateTable[Mathf.FloorToInt((4 * (Rr ^ 0x1F)) - 0x18 + 11) + 32];
                            break;
                        case 7:
                            envelope_level -= RateTable[Mathf.FloorToInt((4 * (Rr ^ 0x1F)) - 0x18 + 12) + 32];
                            break;
                    }
                }
                samples = l;
            }
            timeInSecs = samples / sampleRate;

            // theRate = timeInSecs / sustain_envelope_level;
            // timeInSecs = 0x7FFFFFFF * theRate;	//the release time value is more like a rate.  It is
            // the time from max value to 0, not from sustain level. if (Rm == 0) // if it's linear
            // timeInSecs *=
            // LINEAR_RELEASE_COMPENSATION;

            release_time = LinAmpDecayTimeToLinDBDecayTime(timeInSecs, 0x800);

            // We need to compensate the decay and release times to represent them as the time from full vol
            // to -100db where the drop in db is a fixed amount per time unit (SoundFont2 spec for vol
            // envelopes, pg44.)
            //  We assume the psx envelope is using a linear scale wherein envelope_level / 2 == half
            //  loudness. For a linear release mode (Rm == 0), the time to reach half volume is simply half
            //  the time to reach 0.
            // Half perceived loudness is -10db. Therefore, time_to_half_vol * 10 == full_time * 5 == the
            // correct SF2 time
            // realADSR->decay_time = LinAmpDecayTimeToLinDBDecayTime(realADSR->decay_time, 0x800);
            // realADSR->sustain_time = LinAmpDecayTimeToLinDBDecayTime(realADSR->sustain_time, 0x800);
            // realADSR->release_time = LinAmpDecayTimeToLinDBDecayTime(realADSR->release_time, 0x800);

            // Calculations are done, so now add the articulation data
            // artic->AddADSR(attack_time, Am, decay_time, sustain_lev, release_time, 0);
            return;
        }
        public double LinAmpDecayTimeToLinDBDecayTime(double secondsToFullAtten, int linearVolumeRange)
        {
            double expMinDecibel = -100.0;
            double linearMinDecibel = Mathf.Log10(1.0f / linearVolumeRange) * 20.0;
            double linearToExpScale = Mathf.Log((float)(linearMinDecibel - expMinDecibel)) / Mathf.Log(2.0f);
            return secondsToFullAtten * linearToExpScale;
        }
    }

}
