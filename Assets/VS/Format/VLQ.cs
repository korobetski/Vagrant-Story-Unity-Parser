using System;
using System.Collections.Generic;

namespace VS.Format
{


    public struct VLQ
    {
        //https://en.wikipedia.org/wiki/Variable-length_quantity


        private uint _value;
        private List<byte> _bytes;

        public VLQ(uint val)
        {
            _value = val;
            _bytes = new List<byte>();
            _bytes = ToVlqCollection(_value);
        }

        public VLQ(List<byte> bt)
        {
            _bytes = bt;
            _value = 0;
            _value = FromVlqCollection(_bytes);
        }

        public uint Value
        {
            get => _value; set
            {
                _value = value;
                _bytes = ToVlqCollection(_value);
            }
        }

        public List<byte> Bytes
        {
            get => _bytes; set
            {
                _bytes = value;
                _value = FromVlqCollection(_bytes);
            }
        }

        private uint FromVlqCollection(List<byte> bytes)
        {
            throw new NotImplementedException();
        }

        private List<byte> ToVlqCollection(uint integer)
        {
            List<byte> vlq = new List<byte>();
            if (integer >= 0x80)
            {
                string binary = Convert.ToString(integer, 2);
                for (int i = binary.Length; i > 0; i -= 7)
                {
                    if (i >= 7)
                    {
                        if (i == binary.Length)
                        {
                            vlq.Add((byte)Convert.ToInt32(binary.Substring(i - 7, 7).PadLeft(8, '0'), 2));
                        }
                        else
                        {
                            vlq.Add((byte)Convert.ToInt32("1" + binary.Substring(i - 7, 7), 2));
                        }
                    }
                    else if (binary.Length < 7)
                    {
                        vlq.Add((byte)Convert.ToInt32(binary.Substring(0, i).PadLeft(8, '0'), 2));
                    }
                    else
                    {
                        vlq.Add((byte)Convert.ToInt32("1" + binary.Substring(0, i).PadLeft(7, '0'), 2));
                    }
                }
                vlq.Reverse();
            }
            else
            {
                vlq.Add((byte)integer);
            }


            return vlq;
        }
    }

}

