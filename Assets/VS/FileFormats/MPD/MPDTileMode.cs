using System;
using System.Collections.Generic;

namespace VS.FileFormats.MPD
{
    // it tells how the collision tile is set, it can be flat or ramp or cut in diagonal
    [Serializable]
    public class MPDTileMode
    {
        public enum Mode
        {
            FLAT, // classical walkable tile
            TOP, // the floor to the top, not walkable collision pillar
            VOID, // no ceil tile or very high
            DECIMAL, // non integer tile heigth, need to compute the value
            // ramps
            RAMPXP,
            RAMPXN,
            RAMPYP,
            RAMPYN,
            // diagonal cutted tile (we need to change vertices order)
            DIAG0,
            DIAG1,
            DIAG2,
            DIAG3,
        }


        public Mode mode;
        public int from;
        public int to;
        // 16 bytes
        public byte[] datas;

        public void SetDatas(byte[] value) {
            datas = value;
            List<byte> _d = new List<byte>(datas);
            if (_d.TrueForAll(x => x == 0))
            {
                mode = Mode.FLAT;
            }
            else if (_d.TrueForAll(x => x == 2))
            {
                mode = Mode.DECIMAL;
            }
            else if (_d.TrueForAll(x => x == 3))
            {
                mode = Mode.TOP;
            }
            else if (_d.TrueForAll(x => x == 131))
            {
                mode = Mode.VOID;
            } else
            {
                if (datas[0] < datas[1] && datas[1] < datas[2] && datas[2] < datas[3])
                {
                    // XP Ramps
                    mode = Mode.RAMPXP;
                    from = datas[0];
                    to = datas[15];
                } else if (datas[0] > datas[1] && datas[1] > datas[2] && datas[2] > datas[3])
                {
                    // XN Ramps
                    mode = Mode.RAMPXN;
                    from = datas[0];
                    to = datas[15];
                }
                else if (datas[0] == datas[1] && datas[4] == datas[5] && datas[0] > datas[4])
                {
                    // YP Ramps
                    mode = Mode.RAMPYP;
                    from = datas[0];
                    to = datas[15];
                }
                else if (datas[0] == datas[1] && datas[4] == datas[5] && datas[0] < datas[4])
                {
                    // YN Ramps
                    mode = Mode.RAMPXN;
                    from = datas[0];
                    to = datas[15];
                }
            }
        }
    }
}
