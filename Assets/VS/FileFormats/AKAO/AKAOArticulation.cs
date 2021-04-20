using System;
using UnityEngine;

namespace VS.FileFormats.AKAO
{
    [Serializable]
    public class AKAOArticulation
    {
        public uint id;
        public uint samplePointer;
        public uint loopPt;
        public short fineTune;
        public ushort unityKey;
        public ushort ADSR1;
        public ushort ADSR2;



        /*
         * 
        The ADSR envelope filter works as follows:
        Ar = Attack rate, which specifies the speed at which the volume increases
             from zero to it's maximum value, as soon as the note on is given. The
             slope can be set to lineair or exponential.
        Dr = Decay rate specifies the speed at which the volume decreases to the
             sustain level. Decay is always decreasing exponentially.
        Sl = Sustain level, base level from which sustain starts.
        Sr = Sustain rate is the rate at which the volume of the sustained note
             increases or decreases. This can be either lineair or exponential.
        Rr = Release rate is the rate at which the volume of the note decreases
             as soon as the note off is given.

             lvl |
               ^ |     /\Dr     __
             Sl _| _  / _ \__---  \
                 |   /       ---__ \ Rr
                 |  /Ar       Sr  \ \
                 | /                \\
                 |/___________________\________
                                          ->time

        The overal volume can also be set to sweep up or down lineairly or
        exponentially from it's current value. This can be done seperately
        for left and right.

        Relevant SPU registers:
        -------------------------------------------------------------
        $1f801xx8         Attack/Decay/Sustain level
        bit  |0f|0e 0d 0c 0b 0a 09 08|07 06 05 04|03 02 01 00|
        desc.|Am|         Ar         |Dr         |Sl         |

        Am       0        Attack mode Linear
                 1                    Exponential

        Ar       0-7f     attack rate
        Dr       0-f      decay rate
        Sl       0-f      sustain level
        -------------------------------------------------------------
        $1f801xxa         Sustain rate, Release Rate.
        bit  |0f|0e|0d|0c 0b 0a 09 08 07 06|05|04 03 02 01 00|
        desc.|Sm|Sd| 0|   Sr               |Rm|Rr            |

        Sm       0        sustain rate mode linear
                 1                          exponential
        Sd       0        sustain rate mode increase
                 1                          decrease
        Sr       0-7f     Sustain Rate
        Rm       0        Linear decrease
                 1        Exponential decrease
        Rr       0-1f     Release Rate

        Note: decay mode is always Expontial decrease, and thus cannot
        be set.
        */
        internal double A = 0;             //in seconds
        internal ushort AT = 0;         // 0 = No Transform, 1 = concave Transform
        internal double D = 0;             //in seconds
        internal double S = 0;             //in seconds
        internal double Slv = 0;             //as a percentage
        internal double R = 0;             //in seconds
        internal ushort RT = 0;         // 0 = No Transform, 1 = concave Transform

        public ushort sampleId;
        private AKAOSample _sample;


        public AKAOSample sample { get => _sample; set => _sample = value; }

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


            if (((Am & ~0x01) != 0) || ((Ar & ~0x7F) != 0) || ((Dr & ~0x0F) != 0) || ((Sl & ~0x0F) != 0) ||
                ((Rm & ~0x01) != 0) || ((Rr & ~0x1F) != 0) || ((Sm & ~0x01) != 0) || ((Sd & ~0x01) != 0) ||
                ((Sr & ~0x7F) != 0))
            {
                Debug.LogError("ADSR parameter(s) out of range (Am : " + Am + ", Ar : " + Ar + ", Dr : " + Dr + ", Sl : " + Sl + ", Rm : " + Rm + ", Rr : " + Rr + ", Sm : " + Sm + ", Sd : " + Sd + ", Sr : " + Sr + ")");
                return false;
            }

            // Setting default values
            A = int.MaxValue / 50;
            AT = (Am == 1) ? (ushort)1 : (ushort)0;
            D = int.MaxValue / 6;
            S = ushort.MaxValue * 500; // ~50%
            R = int.MaxValue / 6;
            RT = 0;
            // try to get right values
            ComputeADSR(Am, Ar, Dr, Sl, Rm, Rr, Sm, Sd, Sr);


            return true;
        }

        private void ComputeADSR(ushort Am, ushort Ar, ushort Dr, ushort Sl, ushort Rm, ushort Rr, ushort Sm, ushort Sd, ushort Sr)
        {
            ushort sampleRate = 44100;
            ulong r, rs, rd, rate, remain;
            double samples = 0, timeInSecs;
            int i;
            ulong[] rateTable = new ulong[160];
            r = 3; rs = 1; rd = 0;
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

                rateTable[i] = r;
            }

            // Attack
            rate = rateTable[RoundToZero((Ar ^ 0x7F) - 0x10) + 32];
            if (Am == 0) // Line
            {
                samples = Math.Ceiling(0x7FFFFFFF / (double)rate);
            }
            else if (Am == 1)// Expo
            {
                samples = 0x60000000 / rate;
                remain = 0x60000000 % rate;
                rate = rateTable[RoundToZero((Ar ^ 0x7F) - 0x18) + 32];
                samples += Math.Ceiling(Math.Max(0, 0x1FFFFFFF - remain) / (double)rate);
            }

            timeInSecs = samples / sampleRate;
            A = timeInSecs;

            // Decay
            int envelope_level = 0x7FFFFFFF;
            bool bSustainLevFound = false;
            int realSustainLevel = 0;
            int l;
            for (l = 0; envelope_level > 0; l++)
            {
                if (4 * (Dr ^ 0x1F) < 0x18)
                {
                    Dr = 0;
                }

                switch ((envelope_level >> 28) & 0x7)
                {
                    case 0: envelope_level -= (int)rateTable[RoundToZero((4 * (Dr ^ 0x1F)) - 0x18 + 0) + 32]; break;
                    case 1: envelope_level -= (int)rateTable[RoundToZero((4 * (Dr ^ 0x1F)) - 0x18 + 4) + 32]; break;
                    case 2: envelope_level -= (int)rateTable[RoundToZero((4 * (Dr ^ 0x1F)) - 0x18 + 6) + 32]; break;
                    case 3: envelope_level -= (int)rateTable[RoundToZero((4 * (Dr ^ 0x1F)) - 0x18 + 8) + 32]; break;
                    case 4: envelope_level -= (int)rateTable[RoundToZero((4 * (Dr ^ 0x1F)) - 0x18 + 9) + 32]; break;
                    case 5: envelope_level -= (int)rateTable[RoundToZero((4 * (Dr ^ 0x1F)) - 0x18 + 10) + 32]; break;
                    case 6: envelope_level -= (int)rateTable[RoundToZero((4 * (Dr ^ 0x1F)) - 0x18 + 11) + 32]; break;
                    case 7: envelope_level -= (int)rateTable[RoundToZero((4 * (Dr ^ 0x1F)) - 0x18 + 12) + 32]; break;
                }
                if (!bSustainLevFound && ((envelope_level >> 27) & 0xF) <= Sl)
                {
                    realSustainLevel = envelope_level;
                    bSustainLevFound = true;
                }
            }
            samples = l;
            timeInSecs = samples / sampleRate;
            D = timeInSecs;

            // Sustain
            envelope_level = 0x7FFFFFFF;
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
                        rate = rateTable[RoundToZero((Sr ^ 0x7F) - 0x0F) + 32];
                        samples = Math.Ceiling(0x7FFFFFFF / (double)rate);
                    }
                    else
                    {
                        l = 0;
                        //DLS decay rate value is to -96db (silence) not the sustain level
                        while (envelope_level > 0)
                        {
                            long envelope_level_diff = 0;
                            long envelope_level_target = 0;

                            switch ((envelope_level >> 28) & 0x7)
                            {
                                case 0: envelope_level_target = 0x00000000; envelope_level_diff = (long)rateTable[RoundToZero((Sr ^ 0x7F) - 0x1B + 0) + 32]; break;
                                case 1: envelope_level_target = 0x0fffffff; envelope_level_diff = (long)rateTable[RoundToZero((Sr ^ 0x7F) - 0x1B + 4) + 32]; break;
                                case 2: envelope_level_target = 0x1fffffff; envelope_level_diff = (long)rateTable[RoundToZero((Sr ^ 0x7F) - 0x1B + 6) + 32]; break;
                                case 3: envelope_level_target = 0x2fffffff; envelope_level_diff = (long)rateTable[RoundToZero((Sr ^ 0x7F) - 0x1B + 8) + 32]; break;
                                case 4: envelope_level_target = 0x3fffffff; envelope_level_diff = (long)rateTable[RoundToZero((Sr ^ 0x7F) - 0x1B + 9) + 32]; break;
                                case 5: envelope_level_target = 0x4fffffff; envelope_level_diff = (long)rateTable[RoundToZero((Sr ^ 0x7F) - 0x1B + 10) + 32]; break;
                                case 6: envelope_level_target = 0x5fffffff; envelope_level_diff = (long)rateTable[RoundToZero((Sr ^ 0x7F) - 0x1B + 11) + 32]; break;
                                case 7: envelope_level_target = 0x6fffffff; envelope_level_diff = (long)rateTable[RoundToZero((Sr ^ 0x7F) - 0x1B + 12) + 32]; break;
                            }

                            long steps = (envelope_level - envelope_level_target + (envelope_level_diff - 1)) / envelope_level_diff;
                            envelope_level -= (int)(envelope_level_diff * steps);
                            l += (int)steps;
                        }
                        samples = l;

                    }
                    timeInSecs = samples / sampleRate;
                    S = /*Sm ? timeInSecs : */LinAmpDecayTimeToLinDBDecayTime(timeInSecs, 0x800);
                }
            }

            if ((D < 2 || (Dr == 0x0F && Sl >= 0x0C)) && Sr < 0x7E && Sd == 1)
            {
                D = S;
            }

            // Release
            envelope_level = 0x7FFFFFFF;

            //if linear Rr Mode
            if (Rm == 0)
            {
                rate = rateTable[RoundToZero((4 * (Rr ^ 0x1F)) - 0x0C) + 32];

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
                        case 0: envelope_level -= (int)rateTable[RoundToZero((4 * (Rr ^ 0x1F)) - 0x18 + 0) + 32]; break;
                        case 1: envelope_level -= (int)rateTable[RoundToZero((4 * (Rr ^ 0x1F)) - 0x18 + 4) + 32]; break;
                        case 2: envelope_level -= (int)rateTable[RoundToZero((4 * (Rr ^ 0x1F)) - 0x18 + 6) + 32]; break;
                        case 3: envelope_level -= (int)rateTable[RoundToZero((4 * (Rr ^ 0x1F)) - 0x18 + 8) + 32]; break;
                        case 4: envelope_level -= (int)rateTable[RoundToZero((4 * (Rr ^ 0x1F)) - 0x18 + 9) + 32]; break;
                        case 5: envelope_level -= (int)rateTable[RoundToZero((4 * (Rr ^ 0x1F)) - 0x18 + 10) + 32]; break;
                        case 6: envelope_level -= (int)rateTable[RoundToZero((4 * (Rr ^ 0x1F)) - 0x18 + 11) + 32]; break;
                        case 7: envelope_level -= (int)rateTable[RoundToZero((4 * (Rr ^ 0x1F)) - 0x18 + 12) + 32]; break;
                    }
                }
                samples = l;
            }
            timeInSecs = samples / sampleRate;
            R = timeInSecs;
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
