using System.Collections.Generic;

namespace VS.Format
{

    public class XBytes
    {
        private List<byte> _value;

        public XBytes(List<byte> value)
        {
            _value = value;
        }

        public List<byte> Value
        {
            get => _value;

            set
            {
                _value = value;
            }
        }

        public List<byte> Bytes
        {
            get
            {
                List<byte> bytes = new List<byte>();
                VLQ vlqLen = new VLQ((uint)_value.Count);
                bytes.AddRange(vlqLen.Bytes);
                bytes.AddRange(_value);
                return bytes;
            }
        }
    }
}
