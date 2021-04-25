using System;

namespace VS.FileFormats.AKAO
{
    //Minoru Akao
    [Serializable]
    public class AKAOInstrument
    {
        public string name = "";
        public byte program = 0;
        public bool isDrum = false;
        public bool a1 = false;
        public AKAORegion[] regions;

        public AKAOInstrument(byte id, bool _isDrum = false)
        {
            program = id;
            isDrum = _isDrum;
        }
    }
}