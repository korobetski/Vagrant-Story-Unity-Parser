using System.Collections.Generic;
using System.Text;

namespace FileFormats
{

    public class XString
    {
        private string _value;

        public XString(string value)
        {
            _value = value;
        }

        public string Value
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
                VLQ vlqLen = new VLQ((uint)_value.Length);
                bytes.AddRange(vlqLen.Bytes);
                bytes.AddRange(Encoding.ASCII.GetBytes(_value));
                return bytes;
            }
        }
    }
}
