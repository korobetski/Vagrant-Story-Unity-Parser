using System;

namespace VS.FileFormats.ZND
{
    [Serializable]
    public class Skill
    {
        public byte id;
        public byte uk1;
        public byte uk2;
        public byte localId;


        public Skill(byte[] bytes)
        {
            id = bytes[0];
            uk1 = bytes[1];
            uk2 = bytes[2];
            localId = bytes[3];
        }
    }
}