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
        public ushort adr1;
        public ushort adr2;

        internal uint sampleNum;
        internal AKAOSample sample;

        public AKAOArticulation(BinaryReader buffer)
        {
            sampleOff = buffer.ReadUInt32();
            loopPt = buffer.ReadUInt32();
            fineTune = buffer.ReadInt16();
            unityKey = buffer.ReadUInt16();
            adr1 = buffer.ReadUInt16();
            adr2 = buffer.ReadUInt16();


            //Debug.Log(string.Concat("AKAOArticulation =>  offset :", sampleOff, "  loopPt : ", loopPt, "  fineTune : ", fineTune, "  unityKey : ", unityKey, "  adr1 : ", adr1, "  adr2 : ", adr2));
        }
    }

}
