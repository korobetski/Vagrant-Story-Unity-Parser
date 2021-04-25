using System;
namespace VS.FileFormats.AKAO
{
    [Serializable]
    public class AKAOP
    {
        public string name;
        public byte op;
        public byte meta;
        public byte length;
        public uint adr;
        public byte[] parameters;

        public AKAOP(string _name, byte _op, byte _length, byte _meta = 0, byte[] _parameters = null)
        {
            name = _name;
            op = _op;
            meta = _meta;
            length = _length;
            parameters = _parameters;
        }

        public static AKAOP GetOp(byte _op)
        {
            if (_op < 0x9A)
            {
                if (_op < 0x83) // Note On
                {
                    return new AKAOP("Note On", _op, 1);
                }
                else if (_op < 0x8F) // Tie
                {
                    return new AKAOP("Tie", _op, 1);
                }
                else // Rest
                {
                    return new AKAOP("Rest", _op, 1);
                }
            }
            else
            {
                for (uint i = 0; i < EVENTS.Length; i++)
                {
                    if (EVENTS[i].op == _op)
                    {
                        return EVENTS[i];
                    }
                }
            }

            return new AKAOP("ERROR !", _op, 1);
        }

        public AKAOP Clone()
        {
            return new AKAOP(name, op, length, meta);
        }

        public static AKAOP GetMeta(byte _meta)
        {
            for (uint i = 0; i < METAS_EVENTS.Length; i++)
            {
                if (METAS_EVENTS[i].meta == _meta)
                {
                    return METAS_EVENTS[i];
                }
            }
            return new AKAOP("ERROR META !", 0xFE, 2, _meta);
        }



        //https://translate.google.com/translate?hl=&sl=ja&tl=en&u=https%3A%2F%2Fw.atwiki.jp%2Fsagafrontier%2Fpages%2F43.html
        //https://wiki.ffrtt.ru/index.php?title=FF9/Sound/AKAO_sequence
        private static readonly AKAOP[] EVENTS = new AKAOP[]
        {
            // 0x00 to 0x99
            new AKAOP("Note On",            0x00, 1),
            new AKAOP("Tie",                0x84, 1),
            new AKAOP("Rest",               0x90, 1),
            // 0x9A to 0x9F Undefined
            new AKAOP("End Track",          0xA0, 1),
            new AKAOP("Program Change(A1)", 0xA1, 2),
            new AKAOP("Delta",              0xA2, 2), // Ignores the regular length (delta-time) of the next note and overwrites it with the specified length. 
            new AKAOP("Volume",             0xA3, 2),
            new AKAOP("Portamento",         0xA4, 3),
            new AKAOP("Octave",             0xA5, 2),
            new AKAOP("Increase Octave",    0xA6, 1),
            new AKAOP("Decrease Octave",    0xA7, 1),
            new AKAOP("Expression",         0xA8, 2),
            new AKAOP("Expression Slide",   0xA9, 3),
            new AKAOP("Pan",                0xAA, 2),
            new AKAOP("Pan Slide",          0xAB, 3),
            new AKAOP("Phase noise",        0xAC, 2),
            new AKAOP("ADSR:Attack",        0xAD, 2),
            new AKAOP("ADSR:Decay",         0xAE, 2),
            new AKAOP("ADSR:Sustain Lvl",   0xAF, 2),
            new AKAOP("ADSR:Decay&Sustain", 0xB0, 3),
            new AKAOP("ADSR:Sustain",       0xB1, 2),
            new AKAOP("ADSR:Release",       0xB2, 2),
            new AKAOP("ADSR:Reset",         0xB3, 1),
            new AKAOP("Vibrato",            0xB4, 4),
            new AKAOP("Vibrato Depth",      0xB5, 2),
            new AKAOP("Vibrato Off",        0xB6, 1),
            new AKAOP("ADSR:Attack Mode",   0xB7, 2),
            new AKAOP("Tremolo",            0xB8, 4),
            new AKAOP("Tremolo Depth",      0xB9, 2),
            new AKAOP("Tremolo Off",        0xBA, 1),
            new AKAOP("ADSR:Sustain Mode",  0xBB, 2),
            new AKAOP("LFO Pan",            0xBC, 3),
            new AKAOP("LFO Pan Depth",      0xBD, 2),
            new AKAOP("LFO Pan Off",        0xBE, 1),
            new AKAOP("ADSR:Release Mode",  0xBF, 2),
            new AKAOP("Transpose (abs)",    0xC0, 2), // signed byte
            new AKAOP("Transpose (rel)",    0xC1, 2), // signed byte
            new AKAOP("Reverb On",          0xC2, 1),
            new AKAOP("Reverb Off",         0xC3, 1),
            new AKAOP("Noise On",           0xC4, 1),
            new AKAOP("Noise Off",          0xC5, 1),
            new AKAOP("FM On",              0xC6, 1),
            new AKAOP("FM Off",             0xC7, 1),
            new AKAOP("Loop Start",         0xC8, 1),
            new AKAOP("Loop Return X",      0xC9, 2),
            new AKAOP("Loop Return",        0xCA, 1),
            new AKAOP("Reset Effects",      0xCB, 1),
            new AKAOP("Legato On",          0xCC, 1),
            new AKAOP("Legato Off",         0xCD, 1),
            new AKAOP("Noise On Delay",     0xCE, 2),
            new AKAOP("Noise Off Delay",    0xCF, 2),
            new AKAOP("Full Length On",     0xD0, 1),
            new AKAOP("Full Length Off",    0xD1, 1),
            new AKAOP("FM On Delay",        0xD2, 2),
            new AKAOP("FM Off Delay",       0xD3, 2),
            new AKAOP("Playback On",        0xD4, 1),
            new AKAOP("Playback Off",       0xD5, 1),
            new AKAOP("Playback Pitch On",  0xD6, 1),
            new AKAOP("Playback Pitch Off", 0xD7, 1),
            new AKAOP("Fine Tuning (abs)",  0xD8, 2),
            new AKAOP("Fine Tuning (rel)",  0xD9, 2),
            new AKAOP("Portamento On",      0xDA, 2),
            new AKAOP("Portamento Off",     0xDB, 1),
            new AKAOP("Fix Note Length",    0xDC, 2),
            new AKAOP("Vibrato Slide",      0xDD, 3),
            new AKAOP("Tremolo Slide",      0xDE, 3),
            new AKAOP("LFO Pan Slide",      0xDF, 3),
            new AKAOP("Unknown E0",         0xE0, 1),
            new AKAOP("Unknown E1",         0xE1, 2),
            new AKAOP("Unknown E2",         0xE2, 1), // E1 Off
            new AKAOP("Unknown E3",         0xE3, 1),
            new AKAOP("Unknown E4",         0xE4, 3), // Vibrato Rate Slide 
            new AKAOP("Unknown E5",         0xE5, 3), // Tremolo Rate Slide 
            new AKAOP("Unknown E6",         0xE6, 3), // Channel Pan LFO Rate Slide 
            new AKAOP("Unknown E7",         0xE7, 1),
            new AKAOP("Unknown E8",         0xE8, 1),
            new AKAOP("Unknown E9",         0xE9, 1),
            new AKAOP("Unknown EA",         0xEA, 1),
            new AKAOP("Unknown EB",         0xEB, 1),
            new AKAOP("Unknown EC",         0xEC, 1),
            new AKAOP("Unknown ED",         0xED, 1),
            new AKAOP("Unknown EE",         0xEE, 1),
            new AKAOP("Unknown EF",         0xEF, 1),

            new AKAOP("Note On *",            0xF0, 2),
            new AKAOP("Note On *",            0xF1, 2),
            new AKAOP("Note On *",            0xF2, 2),
            new AKAOP("Note On *",            0xF3, 2),
            new AKAOP("Note On *",            0xF4, 2),
            new AKAOP("Note On *",            0xF5, 2),
            new AKAOP("Note On *",            0xF6, 2),
            new AKAOP("Note On *",            0xF7, 2),
            new AKAOP("Note On *",            0xF8, 2),
            new AKAOP("Note On *",            0xF9, 2),
            new AKAOP("Note On *",            0xFA, 2),
            new AKAOP("Note On *",            0xFB, 2),
            new AKAOP("Tie *",                0xFC, 2),
            new AKAOP("Rest *",               0xFD, 2),
            new AKAOP("-Meta Events-",      0xFE, 2),
            new AKAOP("Padding",            0xFF, 1)
        };
        public static readonly AKAOP[] METAS_EVENTS = new AKAOP[]
        {
            // Meta Events 0xFE + byte + params
            new AKAOP("Tempo",              0xFE, 4, 0x00),
            new AKAOP("Tempo Slide",        0xFE, 5, 0x01),
            new AKAOP("Reverb Depth",       0xFE, 4, 0x02),
            new AKAOP("Reverb Depth Slide", 0xFE, 5, 0x03),
            new AKAOP("Drum Mode On",       0xFE, 2, 0x04),
            new AKAOP("Drum Mode Off",      0xFE, 2, 0x05),
            new AKAOP("Jump To",            0xFE, 4, 0x06),
            new AKAOP("Conditinal Jump",    0xFE, 5, 0x07),
            new AKAOP("Loop Jump",          0xFE, 5, 0x08),
            new AKAOP("Loop Break",         0xFE, 5, 0x09),
            new AKAOP("No Attack Program",  0xFE, 3, 0x0A),
            new AKAOP("Unknown Meta 0x0B",  0xFE, 6, 0x0B),
            new AKAOP("Unknown Meta 0x0C",  0xFE, 2, 0x0C),
            new AKAOP("Unknown Meta 0x0D",  0xFE, 2, 0x0D),
            new AKAOP("Save & Goto",        0xFE, 4, 0x0E),
            new AKAOP("Return to Save",     0xFE, 2, 0x0F),
            new AKAOP("Reserve Voices",     0xFE, 3, 0x10),
            new AKAOP("Free Reserved",      0xFE, 2, 0x11),
            new AKAOP("Volume Slide",       0xFE, 3, 0x12),
            new AKAOP("Unknown Meta 0x13",  0xFE, 2, 0x13),
            new AKAOP("Program Change * ",  0xFE, 3, 0x14),
            new AKAOP("Time Signature",     0xFE, 4, 0x15),
            new AKAOP("Measure Number",     0xFE, 4, 0x16),
            new AKAOP("Unknown Meta 0x17",  0xFE, 2, 0x17),
            new AKAOP("Unknown Meta 0x18",  0xFE, 2, 0x18),
            new AKAOP("Notes Volume Slide", 0xFE, 4, 0x19),
            new AKAOP("Unknown Meta 0x1A",  0xFE, 2, 0x1A),
            new AKAOP("Unknown Meta 0x1B",  0xFE, 2, 0x1B), // turn 1A Off
            new AKAOP("Unknown Meta 0x1C",  0xFE, 3, 0x1C),
            new AKAOP("Use Reserved",       0xFE, 2, 0x1D),
            new AKAOP("Use Reserved Off",   0xFE, 2, 0x1E),
            new AKAOP("Unknown Meta 0x1F",  0xFE, 2, 0x1F),
        };
    }
}
