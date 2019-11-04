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
            //0D242A3828
            string[] table = new string[256];
            table[0x00] = "";
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
            //
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
            table[0x40] = "�?";
            table[0x41] = "Â";
            table[0x42] = "Ä";
            table[0x43] = "Ç";
            table[0x44] = "È";
            table[0x45] = "É";
            table[0x46] = "Ê";
            table[0x47] = "Ë";
            table[0x48] = "Ì";
            table[0x49] = "�?";
            table[0x4A] = "Î";
            table[0x4B] = "�?";
            table[0x4C] = "Ò";
            table[0x4D] = "Ó";
            table[0x4E] = "Ô";
            table[0x4F] = "Ö";
            table[0x50] = "Ù";
            table[0x51] = "Ú";
            table[0x52] = "Û";
            table[0x53] = "Ü";
            table[0x54] = "ß";
            table[0x55] = "æ";
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
            table[0x8F] = " ";
            table[0x90] = "!";
            table[0x91] = "\"";
            table[0x94] = "%";
            table[0x96] = "'";
            table[0x97] = "(";
            table[0x98] = ")";
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
            table[0xA7] = "-";
            table[0xA8] = "+";
            table[0xB6] = "Lv.";
            table[0xE7] = "\n";
            return table[num];
        }
    }

}
