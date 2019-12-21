using System.Collections.Generic;

namespace VS.Utils
{
    public class L10n
    {
        internal static List<string> itemNames = new List<string>();
        internal static List<string> itemDescs = new List<string>();
        internal static List<string> menu = new List<string>();

        public static string Charset(int num)
        {
            string[] table = new string[256];
            table[0x00] = "0";
            table[0x01] = "1";
            table[0x02] = "2";
            table[0x03] = "3";
            table[0x04] = "4";
            table[0x05] = "5";
            table[0x06] = "6";
            table[0x07] = "7";
            table[0x08] = "8";
            table[0x09] = "9";
            table[0x0A] = "A";
            table[0x0B] = "B";
            table[0x0C] = "C";
            table[0x0D] = "D";
            table[0x0E] = "E";
            table[0x0F] = "F";

            table[0x10] = "G";
            table[0x11] = "H";
            table[0x12] = "I";
            table[0x13] = "J";
            table[0x14] = "K";
            table[0x15] = "L";
            table[0x16] = "M";
            table[0x17] = "N";
            table[0x18] = "O";
            table[0x19] = "P";
            table[0x1A] = "Q";
            table[0x1B] = "R";
            table[0x1C] = "S";
            table[0x1D] = "T";
            table[0x1E] = "U";
            table[0x1F] = "V";

            table[0x20] = "W";
            table[0x21] = "X";
            table[0x22] = "Y";
            table[0x23] = "Z";
            table[0x24] = "a";
            table[0x25] = "b";
            table[0x26] = "c";
            table[0x27] = "d";
            table[0x28] = "e";
            table[0x29] = "f";
            table[0x2A] = "g";
            table[0x2B] = "h";
            table[0x2C] = "i";
            table[0x2D] = "j";
            table[0x2E] = "k";
            table[0x2F] = "l";

            table[0x30] = "m";
            table[0x31] = "n";
            table[0x32] = "o";
            table[0x33] = "p";
            table[0x34] = "q";
            table[0x35] = "r";
            table[0x36] = "s";
            table[0x37] = "t";
            table[0x38] = "u";
            table[0x39] = "v";
            table[0x3A] = "w";
            table[0x3B] = "x";
            table[0x3C] = "y";
            table[0x3D] = "z";
            table[0x3E] = "Œ";
            table[0x3F] = "À";

            table[0x40] = "Á";
            table[0x41] = "Â";
            table[0x42] = "Ä";
            table[0x43] = "Ç";
            table[0x44] = "È";
            table[0x45] = "É";
            table[0x46] = "Ê";
            table[0x47] = "Ë";
            table[0x48] = "Ì";
            table[0x49] = "Í";
            table[0x4A] = "Î";
            table[0x4B] = "Ï";
            table[0x4C] = "Ò";
            table[0x4D] = "Ó";
            table[0x4E] = "Ô";
            table[0x4F] = "Ö";

            table[0x50] = "Ù";
            table[0x51] = "Ú";
            table[0x52] = "Û";
            table[0x53] = "Ü";
            table[0x54] = "ß";
            table[0x55] = "œ";
            table[0x56] = "à";
            table[0x57] = "á";
            table[0x58] = "â";
            table[0x59] = "ä";
            table[0x5A] = "ç";
            table[0x5B] = "è";
            table[0x5C] = "é";
            table[0x5D] = "ê";
            table[0x5E] = "ë";
            table[0x5F] = "ì";

            table[0x60] = "í";
            table[0x61] = "î";
            table[0x62] = "ï";
            table[0x63] = "ò";
            table[0x64] = "ó";
            table[0x65] = "ô";
            table[0x66] = "ö";
            table[0x67] = "ù";
            table[0x68] = "ú";
            table[0x69] = "û";
            table[0x6A] = "ü";
            table[0x6B] = "(0x6B)";
            table[0x6C] = "(0x6C)";
            table[0x6D] = "(0x6D)";
            table[0x6E] = "(0x6E)";
            table[0x6F] = "(0x6F)";

            table[0x70] = "(0x70)";
            table[0x71] = "(0x71)";
            table[0x72] = "(0x72)";
            table[0x73] = "(0x73)";
            table[0x74] = "(0x74)";
            table[0x75] = "(0x75)";
            table[0x76] = "(0x76)";
            table[0x77] = "(0x77)";
            table[0x78] = "(0x78)";
            table[0x79] = "(0x79)";
            table[0x7A] = "(0x7A)";
            table[0x7B] = "(0x7B)";
            table[0x7C] = "(0x7C)";
            table[0x7D] = "(0x7D)";
            table[0x7E] = "(0x7E)";
            table[0x7F] = "(0x7F)";

            table[0x80] = "(0x80)";
            table[0x81] = "(0x81)";
            table[0x82] = "(0x82)";
            table[0x83] = "(0x83)";
            table[0x84] = "(0x84)";
            table[0x85] = "(0x85)";
            table[0x86] = "„";
            table[0x87] = "‼";
            table[0x88] = "≠";
            table[0x89] = "≤";
            table[0x8A] = "≥";
            table[0x8B] = "÷";
            table[0x8C] = "·";
            table[0x8D] = "-";
            table[0x8E] = "…";
            table[0x8F] = " ";

            table[0x90] = "!";
            table[0x91] = "\"";
            table[0x92] = "#";
            table[0x93] = "$";
            table[0x94] = "%";
            table[0x95] = "&";
            table[0x96] = "'";
            table[0x97] = "(";
            table[0x98] = ")";
            table[0x99] = "=";
            table[0x9A] = "@";
            table[0x9B] = "[";
            table[0x9C] = "]";
            table[0x9D] = ";";
            table[0x9E] = ":";
            table[0x9F] = ",";

            table[0xA0] = ".";
            table[0xA1] = "/";
            table[0xA2] = "\\";
            table[0xA3] = "<";
            table[0xA4] = ">";
            table[0xA5] = "?";
            table[0xA6] = "_";
            table[0xA7] = "-";
            table[0xA8] = "+";
            table[0xA9] = "*";
            table[0xAA] = "\'";
            table[0xAB] = "{";
            table[0xAC] = "}";
            table[0xAD] = "♪";
            table[0xAE] = "∆";
            table[0xAF] = "□";

            table[0xB0] = "○";
            table[0xB1] = "X";
            table[0xB2] = "←";
            table[0xB3] = "→";
            table[0xB4] = "↑";
            table[0xB5] = "↓";
            table[0xB6] = "Lv.";
            table[0xB7] = "★";
            table[0xB8] = "█";
            table[0xB9] = "~";
            table[0xBA] = "ꜜ";
            table[0xBB] = "ꜜ";
            table[0xBC] = "ꜜ";
            table[0xBD] = "(0xBD)";
            table[0xBE] = "(0xBE)";
            table[0xBF] = "(0xBF)";

            table[0xC0] = "(0xC0)";
            table[0xC1] = "(0xC1)";
            table[0xC2] = "(0xC2)";
            table[0xC3] = "(0xC3)";
            table[0xC4] = "(0xC4)";
            table[0xC5] = "(0xC5)";
            table[0xC6] = "(0xC6)";
            table[0xC7] = "(0xC7)";
            table[0xC8] = "(0xC8)";
            table[0xC9] = "(0xC9)";
            table[0xCA] = "(0xCA)";
            table[0xCB] = "(0xCB)";
            table[0xCC] = "(0xCC)";
            table[0xCD] = "(0xCD)";
            table[0xCE] = "(0xCE)";
            table[0xCF] = "(0xCF)";

            table[0xD0] = "(0xD0)";
            table[0xD1] = "(0xD1)";
            table[0xD2] = "(0xD2)";
            table[0xD3] = "(0xD3)";
            table[0xD4] = "(0xD4)";
            table[0xD5] = "(0xD5)";
            table[0xD6] = "(0xD6)";
            table[0xD7] = "(0xD7)";
            table[0xD8] = "(0xD8)";
            table[0xD9] = "(0xD9)";
            table[0xDA] = "(0xDA)";
            table[0xDB] = "(0xDB)";
            table[0xDC] = "(0xDC)";
            table[0xDD] = "(0xDD)";
            table[0xDE] = "(0xDE)";
            table[0xDF] = "(0xDF)";

            table[0xE0] = "(0xE0)";
            table[0xE1] = "(0xE1)";
            table[0xE2] = "(0xE2)";
            table[0xE3] = "(0xE3)";
            table[0xE4] = "(0xE4)";
            table[0xE5] = "(0xE5)";
            table[0xE6] = "(0xE6)";
            table[0xE7] = "\r\n";
            table[0xE8] = "\r\n";
            table[0xE9] = "(0xE9)";
            table[0xEA] = "(0xEA)";
            table[0xEB] = "\n"; // New line ? 
            table[0xEC] = "(0xEC)";
            table[0xED] = "(0xED)";
            table[0xEE] = "(0xEE)";
            table[0xEF] = "(0xEF)";

            table[0xF0] = "(0xF0)";
            table[0xF1] = "(0xF1)";
            table[0xF2] = "(0xF2)";
            table[0xF3] = "(0xF3)";
            table[0xF4] = "(0xF4)";
            table[0xF5] = "(0xF5)";
            table[0xF6] = "(0xF6)";
            table[0xF7] = "(0xF7)";
            table[0xF8] = "";
            table[0xF9] = "";
            table[0xFA] = "";
            table[0xFB] = "";
            table[0xFC] = "";
            table[0xFD] = "";
            table[0xFE] = "";
            table[0xFF] = "";
            return table[num];
        }

        public static string Translate(byte[] raw)
        {
            string text = "";
            for (int i = 0; i < raw.Length; i++)
            {
                byte c = raw[i];
                if (c < 0xF0)
                {
                    text += Charset(c);
                }
                else
                {
                    switch (c)
                    {
                        case 0xF8: // ???
                            if (raw.Length > i + 1)
                            {
                                text += string.Concat("\n (§F8:", raw[i + 1], ") ");
                            }
                            else
                            {
                                text += string.Concat("\n (§F8) ");
                            }
                            i++;
                            break;
                        case 0xFA: // space between words + one byte parameter
                            if (raw.Length > i + 1)
                            {
                                int sps = raw[i + 1] / 6;
                                string[] spaces = new string[sps];
                                for (int j = 0; j < sps; j++)
                                {
                                    spaces[j] = " ";
                                }
                                text += string.Join("", spaces);
                            }
                            else
                            {
                                text += " ";
                            }
                            i++;
                            break;
                        case 0xFB: // new dialog bubble + one byte parameter
                            if (raw.Length > i + 1)
                            {
                                text += string.Concat("\n (§FB:", raw[i + 1], ") ");
                            }
                            else
                            {
                                text += string.Concat("\n (§FB) ");
                            }
                            i++;
                            break;
                    }
                }
            }

            return text;
        }

    }
}
