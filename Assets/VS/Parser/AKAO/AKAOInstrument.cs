//Minoru Akao
namespace VS.Parser.Akao
{
    public class AKAOInstrument
    {
        public enum InstrumentType { INSTR_MELODIC, INSTR_DRUM }

        public string name = "";
        public AKAORegion[] regions;

        private ushort _MIDIProgram = 0;
        private InstrumentType _type = InstrumentType.INSTR_MELODIC;

        public AKAOInstrument(InstrumentType type = InstrumentType.INSTR_MELODIC)
        {
            _type = type;
        }

        public bool IsDrum()
        {
            return (_type == InstrumentType.INSTR_DRUM) ? true : false;
        }
    }

}
