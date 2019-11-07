using System.IO;


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

        // DLS DOC page 48/77

        public AKAOArticulation(BinaryReader buffer)
        {

            sampleOff = buffer.ReadUInt32();
            loopPt = buffer.ReadUInt32();
            fineTune = buffer.ReadInt16();
            unityKey = buffer.ReadUInt16();
            adr1 = buffer.ReadUInt16();
            adr2 = buffer.ReadUInt16();

            //blocks = new ConnectionBlock[0];
        }

    }

}
