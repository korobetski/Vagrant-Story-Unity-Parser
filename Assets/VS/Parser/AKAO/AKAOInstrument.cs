//Minoru Akao
namespace VS.Parser.Akao
{
    public class AKAOInstrument
    {
        public enum InstrumentType { INSTR_MELODIC, INSTR_DRUM }

        public string name = "";
        public uint program = 0;
        public AKAORegion[] regions;
        private InstrumentType _type = InstrumentType.INSTR_MELODIC;
        internal bool a1 = false;

        public AKAOInstrument(uint id, InstrumentType type = InstrumentType.INSTR_MELODIC)
        {
            program = id;
            _type = type;
        }

        public bool IsDrum()
        {
            return (_type == InstrumentType.INSTR_DRUM) ? true : false;
        }
    }

}
