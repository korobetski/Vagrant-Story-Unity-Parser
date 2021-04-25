using System;
using System.Collections.Generic;

namespace VS.FileFormats.AKAO
{
    [Serializable]
    public class AKAOTrack
    {
        public List<byte> programs;
        public AKAOP[] operations;


        public void SetDatas(byte[] _datas, uint trackPtr)
        {
            programs = new List<byte>();
            List<AKAOP> ops = new List<AKAOP>();

            uint i = 0;
            while (i < _datas.Length -1)
            {
                byte op = _datas[i];
                i++;
                AKAOP evt;
                if (op == 0xFE)
                {
                    // this is a meta event
                    byte meta = _datas[i];
                    i++;
                    evt = AKAOP.GetMeta(meta).Clone();
                    evt.adr = trackPtr + i - 2;
                    if (evt.length > 2 && (i + evt.length - 2) <= _datas.Length) 
                    {
                        List<byte> parameters = new List<byte>();
                        for (uint j = 0; j < evt.length - 2; j++)
                        {
                            parameters.Add(_datas[i]);
                            i++;
                        }
                        evt.parameters = parameters.ToArray();
                    }
                }
                else
                {
                    evt = AKAOP.GetOp(op).Clone();
                    evt.adr = trackPtr + i - 1;
                    evt.op = op;
                    if (evt.length > 1 && (i + evt.length - 1) <= _datas.Length)
                    {
                        List<byte> parameters = new List<byte>();
                        for (uint j = 0; j < evt.length - 1; j++)
                        {
                            parameters.Add(_datas[i]);
                            i++;
                        }
                        evt.parameters = parameters.ToArray();
                    }
                }

                ops.Add(evt);
                if (evt.name == "Program Change(A1)" || evt.name == "Program Change * ")
                {
                    if (!programs.Contains(evt.parameters[0]))
                    {
                        programs.Add(evt.parameters[0]);
                    }
                }
            }

            operations = ops.ToArray();
        }
    }
}
