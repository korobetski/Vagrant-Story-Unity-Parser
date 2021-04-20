namespace VS.FileFormats.SEQ
{
    public class SEQAction
    {
        public byte f;
        public string name;
        public int count = 0;
        public byte[] paremeters;

        public SEQAction(string n, int c)
        {
            name = n;
            count = c;
        }



        public static SEQAction GetAction(byte a)
        {
            SEQAction[] ACTIONS = new SEQAction[0x50];

            ACTIONS[0x01] = new SEQAction("loop", 0); // verified
            ACTIONS[0x02] = new SEQAction("0x02", 0); // often at end, used for attack animations
            ACTIONS[0x04] = new SEQAction("0x04", 1); //
            ACTIONS[0x0a] = new SEQAction("0x0a", 1); // verified in 00_COM (no other options, 0x00 x00 follows)
            ACTIONS[0x0b] = new SEQAction("0x0b", 0); // pretty sure, used with walk/run, followed by 0x17/left, 0x18/right
            ACTIONS[0x0c] = new SEQAction("0x0c", 1);
            ACTIONS[0x0d] = new SEQAction("0x0d", 0);
            ACTIONS[0x0f] = new SEQAction("0x0f", 1); // first
            ACTIONS[0x13] = new SEQAction("unlockBone", 1); // verified in emulation
            ACTIONS[0x14] = new SEQAction("0x14", 1); // often at end of non-looping
            ACTIONS[0x15] = new SEQAction("0x15", 1); // verified 00_COM (no other options, 0x00 0x00 follows)
            ACTIONS[0x16] = new SEQAction("0x16", 2); // first, verified 00_BT3
            ACTIONS[0x17] = new SEQAction("0x17", 0); // + often at end
            ACTIONS[0x18] = new SEQAction("0x18", 0); // + often at end
            ACTIONS[0x19] = new SEQAction("0x19", 0);// first, verified 00_COM (no other options, 0x00 0x00 follows)
            ACTIONS[0x1a] = new SEQAction("0x1a", 1); // first, verified 00_BT1 (0x00 0x00 follows)
            ACTIONS[0x1b] = new SEQAction("0x1b", 1); // first, verified 00_BT1 (0x00 0x00 follows)
            ACTIONS[0x1c] = new SEQAction("0x1c", 1);
            ACTIONS[0x1d] = new SEQAction("paralyze?", 0); // first, verified 1C_BT1
            ACTIONS[0x24] = new SEQAction("0x24", 2); // first
            ACTIONS[0x27] = new SEQAction("0x27", 4); // first, verified see 00_COM
            ACTIONS[0x34] = new SEQAction("0x34", 3); // first
            ACTIONS[0x35] = new SEQAction("0x35", 5); // first
            ACTIONS[0x36] = new SEQAction("0x36", 3);
            ACTIONS[0x37] = new SEQAction("0x37", 1); // pretty sure
            ACTIONS[0x38] = new SEQAction("0x38", 1);
            ACTIONS[0x39] = new SEQAction("0x39", 1);
            ACTIONS[0x3a] = new SEQAction("disappear", 0); // used in death animations
            ACTIONS[0x3b] = new SEQAction("land", 0);
            ACTIONS[0x3c] = new SEQAction("adjustShadow", 1); // verified
            ACTIONS[0x3f] = new SEQAction("0x3f", 0); // first, pretty sure, often followed by 0x16
            ACTIONS[0x40] = new SEQAction("0x40", 0); // often preceded by 0x1a, 0x1b, often at end

            return ACTIONS[a];
        }
    }
}
