using System;
using System.IO;
using UnityEngine;


//Minoru Akao
namespace VS.Parser.Akao
{
    public class AKAOArticulation
    {
        public uint sampleOff;
        public uint loopPt;
        public short fineTune;
        public ushort unityKey;
        public ushort ADSR1;
        public ushort ADSR2;

        internal uint sampleNum;
        internal AKAOSample sample;

        internal int A = 0;             //in seconds
        internal ushort AT = 0;         // 0 = No Transform, 1 = concave Transform
        internal int D = 0;             //in seconds
        internal int S = 0;             //in seconds
        internal int Slv = 0;             //as a percentage
        internal int R = 0;             //in seconds
        internal ushort RT = 0;         // 0 = No Transform, 1 = concave Transform

        public AKAOArticulation(BinaryReader buffer)
        {
            sampleOff = buffer.ReadUInt32();
            loopPt = buffer.ReadUInt32();
            fineTune = buffer.ReadInt16();
            unityKey = buffer.ReadUInt16();
            ADSR1 = buffer.ReadUInt16();
            ADSR2 = buffer.ReadUInt16();


            //Debug.Log(string.Concat("AKAOArticulation =>  offset :", sampleOff, "  loopPt : ", loopPt, "  fineTune : ", fineTune, "  unityKey : ", unityKey, "  adr1 : ", ADSR1, "  adr2 : ", ADSR2));
        }

        internal bool BuildADSR()
        {
            ushort Am = (ushort)((ADSR1 & 0x8000) >> 15);  // if 1, then Exponential, else linear
            ushort Ar = (ushort)((ADSR1 & 0x7F00) >> 8);
            ushort Dr = (ushort)((ADSR1 & 0x00F0) >> 4);
            ushort Sl = (ushort)(ADSR1 & 0x000F);
            ushort Rm = (ushort)((ADSR2 & 0x0020) >> 5);
            ushort Rr = (ushort)(ADSR2 & 0x001F);
            ushort Sm = (ushort)((ADSR2 & 0x8000) >> 15);
            ushort Sd = (ushort)((ADSR2 & 0x4000) >> 14);
            ushort Sr = (ushort)((ADSR2 >> 6) & 0x7F);

            // Make sure all the ADSR values are within the valid ranges
            if (((Am & ~0x01) != 0) || ((Ar & ~0x7F) != 0) || ((Dr & ~0x0F) != 0) || ((Sl & ~0x0F) != 0) ||
                ((Rm & ~0x01) != 0) || ((Rr & ~0x1F) != 0) || ((Sm & ~0x01) != 0) || ((Sd & ~0x01) != 0) ||
                ((Sr & ~0x7F) != 0))
            {
                Debug.LogError("ADSR parameter(s) out of range (Am : " + Am + ", Ar : " + Ar + ", Dr : " + Dr + ", Sl : " + Sl + ", Rm : " + Rm + ", Rr : " + Rr + ", Sm : " + Sm + ", Sd : " + Sd + ", Sr : " + Sr + ")");

                return false;
            }
            //Debug.Log("ADSR parameters (Am : " + Am + ", Ar : " + Ar + ", Dr : " + Dr + ", Sl : " + Sl + ", Rm : " + Rm + ", Rr : " + Rr + ", Sm : " + Sm + ", Sd : " + Sd + ", Sr : " + Sr + ")");

            //ComputeADSR(Am, Ar, Dr, Sl, Rm, Rr, Sm, Sd, Sr);  Need to fix this
            // Setting arbitrary values

            A = int.MaxValue / 50;
            AT = (Am == 1) ? (ushort)1 : (ushort)0;
            D = int.MaxValue / 6;
            S = ushort.MaxValue * 500; // ~50%
            R = int.MaxValue / 6;
            RT = 0;


            return true;
        }

        private bool ComputeADSR(ushort Am, ushort Ar, ushort Dr, ushort Sl, ushort Rm, ushort Rr, ushort Sm, ushort Sd, ushort Sr)
        {
            try
            {

                double sampleRate = 44100;
                int[] rateIncTable = new int[8] { 0, 4, 6, 8, 9, 10, 11, 12 };
                uint envelope_level;
                double samples = 0;
                uint rate;
                uint remainder;
                double timeInSecs;
                int l;

                uint r, rs, rd;
                int i;

                // build the rate table according to Neill's rules
                uint[] RateTable = new uint[160];

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

                //to get the dls 32 bit time cents, take log base 2 of number of seconds * 1200 * 65536 (dls1v11a.pdf p25).

                //	if (RateTable[(Ar^0x7F)-0x10 + 32] == 0)
                //		realADSR->attack_time = 0;
                //	else
                //	{
                if ((Ar ^ 0x7F) < 0x10)
                {
                    Ar = 0;
                }
                //if linear Ar Mode
                if (Am == 0)
                {
                    rate = RateTable[RoundToZero((Ar ^ 0x7F) - 0x10) + 32];
                    samples = Math.Ceiling(0x7FFFFFFF / (double)rate);
                }
                else if (Am == 1)
                {
                    rate = RateTable[RoundToZero((Ar ^ 0x7F) - 0x10) + 32];
                    samples = 0x60000000 / rate;
                    remainder = 0x60000000 % rate;
                    rate = RateTable[RoundToZero((Ar ^ 0x7F) - 0x18) + 32];
                    samples += Math.Ceiling(Math.Max(0, 0x1FFFFFFF - remainder) / (double)rate);
                }
                timeInSecs = samples / sampleRate;
                A = (int)timeInSecs;
                //	}


                //Decay Time

                envelope_level = 0x7FFFFFFF;

                bool bSustainLevFound = false;
                uint realSustainLevel;
                //DLS decay rate value is to -96db (silence) not the sustain level
                for (l = 0; envelope_level > 0; l++)
                {
                    if (4 * (Dr ^ 0x1F) < 0x18)
                    {
                        Dr = 0;
                    }

                    switch ((envelope_level >> 28) & 0x7)
                    {
                        case 0: envelope_level -= RateTable[RoundToZero((4 * (Dr ^ 0x1F)) - 0x18 + 0) + 32]; break;
                        case 1: envelope_level -= RateTable[RoundToZero((4 * (Dr ^ 0x1F)) - 0x18 + 4) + 32]; break;
                        case 2: envelope_level -= RateTable[RoundToZero((4 * (Dr ^ 0x1F)) - 0x18 + 6) + 32]; break;
                        case 3: envelope_level -= RateTable[RoundToZero((4 * (Dr ^ 0x1F)) - 0x18 + 8) + 32]; break;
                        case 4: envelope_level -= RateTable[RoundToZero((4 * (Dr ^ 0x1F)) - 0x18 + 9) + 32]; break;
                        case 5: envelope_level -= RateTable[RoundToZero((4 * (Dr ^ 0x1F)) - 0x18 + 10) + 32]; break;
                        case 6: envelope_level -= RateTable[RoundToZero((4 * (Dr ^ 0x1F)) - 0x18 + 11) + 32]; break;
                        case 7: envelope_level -= RateTable[RoundToZero((4 * (Dr ^ 0x1F)) - 0x18 + 12) + 32]; break;
                    }
                    if (!bSustainLevFound && ((envelope_level >> 27) & 0xF) <= Sl)
                    {
                        realSustainLevel = envelope_level;
                        bSustainLevFound = true;
                    }
                }
                samples = l;
                timeInSecs = samples / sampleRate;
                D = (int)timeInSecs;

                // Sustain Rate

                envelope_level = 0x7FFFFFFF;
                // increasing... we won't even bother
                if (Sd == 0)
                {
                    S = -1;
                }
                else
                {
                    if (Sr == 0x7F)
                    {
                        S = -1;        // this is actually infinite
                    }
                    else
                    {
                        // linear
                        if (Sm == 0)
                        {
                            rate = RateTable[RoundToZero((Sr ^ 0x7F) - 0x0F) + 32];
                            samples = Math.Ceiling(0x7FFFFFFF / (double)rate);
                        }
                        else
                        {
                            l = 0;
                            //DLS decay rate value is to -96db (silence) not the sustain level
                            while (envelope_level > 0)
                            {
                                uint envelope_level_diff = 0;
                                int envelope_level_target = 0;

                                switch ((envelope_level >> 28) & 0x7)
                                {
                                    case 0: envelope_level_target = 0x00000000; envelope_level_diff = RateTable[RoundToZero((Sr ^ 0x7F) - 0x1B + 0) + 32]; break;
                                    case 1: envelope_level_target = 0x0fffffff; envelope_level_diff = RateTable[RoundToZero((Sr ^ 0x7F) - 0x1B + 4) + 32]; break;
                                    case 2: envelope_level_target = 0x1fffffff; envelope_level_diff = RateTable[RoundToZero((Sr ^ 0x7F) - 0x1B + 6) + 32]; break;
                                    case 3: envelope_level_target = 0x2fffffff; envelope_level_diff = RateTable[RoundToZero((Sr ^ 0x7F) - 0x1B + 8) + 32]; break;
                                    case 4: envelope_level_target = 0x3fffffff; envelope_level_diff = RateTable[RoundToZero((Sr ^ 0x7F) - 0x1B + 9) + 32]; break;
                                    case 5: envelope_level_target = 0x4fffffff; envelope_level_diff = RateTable[RoundToZero((Sr ^ 0x7F) - 0x1B + 10) + 32]; break;
                                    case 6: envelope_level_target = 0x5fffffff; envelope_level_diff = RateTable[RoundToZero((Sr ^ 0x7F) - 0x1B + 11) + 32]; break;
                                    case 7: envelope_level_target = 0x6fffffff; envelope_level_diff = RateTable[RoundToZero((Sr ^ 0x7F) - 0x1B + 12) + 32]; break;
                                }

                                long steps = (envelope_level - envelope_level_target + (envelope_level_diff - 1)) / envelope_level_diff;
                                envelope_level -= (uint)(envelope_level_diff * steps);
                                l += (int)steps;
                            }
                            samples = l;

                        }
                        timeInSecs = samples / sampleRate;
                        S = (int)LinAmpDecayTimeToLinDBDecayTime(timeInSecs, 0x800);
                    }
                }

                //Sustain Level
                //realADSR->sustain_level = (double)envelope_level/(double)0x7FFFFFFF;//(long)ceil((double)envelope_level * 0.030517578139210854);	//in DLS, sustain level is measured as a percentage
                realSustainLevel = 0;
                if (Sl == 0)
                {
                    realSustainLevel = 0x07FFFFFF;
                }

                Slv = (int)Math.Round(realSustainLevel / (double)0x7FFFFFFF);


                // If decay is going unused, and there's a sustain rate with sustain level close to max...
                //  we'll put the sustain_rate in place of the decay rate.
                if ((D < 2 || (Dr == 0x0F && Sl >= 0x0C)) && Sr < 0x7E && Sd == 1)
                {
                    Slv = 0;
                    D = S;
                    //realADSR->decay_time = 0.5;
                }

                //Release Time

                //sustain_envelope_level = envelope_level;

                //We do this because we measure release time from max volume to 0, not from sustain level to 0
                envelope_level = 0x7FFFFFFF;

                //if linear Rr Mode
                if (Rm == 0)
                {
                    rate = RateTable[RoundToZero((4 * (Rr ^ 0x1F)) - 0x0C) + 32];

                    if (rate != 0)
                    {
                        samples = Math.Ceiling((double)envelope_level / (double)rate);
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
                            case 0: envelope_level -= RateTable[RoundToZero((4 * (Rr ^ 0x1F)) - 0x18 + 0) + 32]; break;
                            case 1: envelope_level -= RateTable[RoundToZero((4 * (Rr ^ 0x1F)) - 0x18 + 4) + 32]; break;
                            case 2: envelope_level -= RateTable[RoundToZero((4 * (Rr ^ 0x1F)) - 0x18 + 6) + 32]; break;
                            case 3: envelope_level -= RateTable[RoundToZero((4 * (Rr ^ 0x1F)) - 0x18 + 8) + 32]; break;
                            case 4: envelope_level -= RateTable[RoundToZero((4 * (Rr ^ 0x1F)) - 0x18 + 9) + 32]; break;
                            case 5: envelope_level -= RateTable[RoundToZero((4 * (Rr ^ 0x1F)) - 0x18 + 10) + 32]; break;
                            case 6: envelope_level -= RateTable[RoundToZero((4 * (Rr ^ 0x1F)) - 0x18 + 11) + 32]; break;
                            case 7: envelope_level -= RateTable[RoundToZero((4 * (Rr ^ 0x1F)) - 0x18 + 12) + 32]; break;
                        }
                    }
                    samples = l;
                }
                timeInSecs = samples / sampleRate;

                //theRate = timeInSecs / sustain_envelope_level;
                //timeInSecs = 0x7FFFFFFF * theRate;	//the release time value is more like a rate.  It is the time from max value to 0, not from sustain level.
                //if (Rm == 0) // if it's linear
                //	timeInSecs *=  LINEAR_RELEASE_COMPENSATION;

                R = /*Rm ? timeInSecs : */(int)LinAmpDecayTimeToLinDBDecayTime(timeInSecs, 0x800);

                // We need to compensate the decay and release times to represent them as the time from full vol to -100db
                // where the drop in db is a fixed amount per time unit (SoundFont2 spec for vol envelopes, pg44.)
                //  We assume the psx envelope is using a linear scale wherein envelope_level / 2 == half loudness.
                //  For a linear release mode (Rm == 0), the time to reach half volume is simply half the time to reach 0.
                // Half perceived loudness is -10db. Therefore, time_to_half_vol * 10 == full_time * 5 == the correct SF2 time
                //realADSR->decay_time = LinAmpDecayTimeToLinDBDecayTime(realADSR->decay_time, 0x800);
                //realADSR->sustain_time = LinAmpDecayTimeToLinDBDecayTime(realADSR->sustain_time, 0x800);
                //realADSR->release_time = LinAmpDecayTimeToLinDBDecayTime(realADSR->release_time, 0x800);



                //Calculations are done, so now add the articulation data
                //artic->AddADSR(attack_time, Am, decay_time, sustain_lev, release_time, 0);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private int RoundToZero(int v)
        {
            if (v < 0)
            {
                v = 0;
            }
            return v;
        }

        private double LinAmpDecayTimeToLinDBDecayTime(double secondsToFullAtten, int linearVolumeRange)
        {
            double expMinDecibel = -100.0;
            double linearMinDecibel = Mathf.Log10(1.0f / linearVolumeRange) * 20.0;
            double linearToExpScale = Mathf.Log((float)(linearMinDecibel - expMinDecibel)) / Mathf.Log(2.0f);
            return secondsToFullAtten * linearToExpScale;
        }
    }


}
