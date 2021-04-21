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
            if (AllEquals(datas))
            {
                if (datas[0] == 0)
                {
                    mode = Mode.FLAT;
                }
                else if (datas[0] == 2)
                {
                    mode = Mode.DECIMAL;
                }
                else if (datas[0] == 3)
                {
                    mode = Mode.TOP;
                }
                else if (datas[0] == 131)
                {
                    mode = Mode.VOID;
                }
                else
                {
                    mode = Mode.FLAT;
                    from = datas[0];
                }
            } 
            else
            {
                if (datas[0] < datas[1] && datas[1] < datas[2] && datas[2] < datas[3] && datas[0] == datas[4])
                {
                    // XP Ramps
                    mode = Mode.RAMPXP;
                    from = datas[0];
                    to = datas[15];
                } else if (datas[0] > datas[1] && datas[1] > datas[2] && datas[2] > datas[3] && datas[0] == datas[4])
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
                    mode = Mode.RAMPYN;
                    from = datas[0];
                    to = datas[15];
                }
                else if (datas[0] == datas[1] && datas[0] == datas [2] && datas[0] == datas[3] && datas[0] == datas[5] && datas[0] == datas[6] && datas[0] == datas[7] && datas[0] == datas[10] && datas[0] == datas[11] && datas[0] == datas[15])
                {
                    mode = Mode.DIAG0;
                    from = datas[0];
                    to = datas[4];
                }
                else if (datas[0] == datas[1] && datas[0] == datas[2] && datas[0] == datas[3] && datas[0] == datas[4] && datas[0] == datas[5] && datas[0] == datas[6] && datas[0] == datas[8] && datas[0] == datas[9] && datas[0] == datas[12])
                {
                    mode = Mode.DIAG1;
                    from = datas[0];
                    to = datas[15];
                }
                else if (datas[0] == datas[4] && datas[0] == datas[5] && datas[0] == datas[8] && datas[0] == datas[9] && datas[0] == datas[10] && datas[0] == datas[12] && datas[0] == datas[13] && datas[0] == datas[14] && datas[0] == datas[15])
                {
                    mode = Mode.DIAG2;
                    from = datas[0];
                    to = datas[1];
                }
                else if (datas[3] == datas[6] && datas[3] == datas[7] && datas[3] == datas[9] && datas[3] == datas[10] && datas[3] == datas[11] && datas[3] == datas[12] && datas[3] == datas[13] && datas[3] == datas[14] && datas[3] == datas[15])
                {
                    mode = Mode.DIAG3;
                    from = datas[3];
                    to = datas[0];
                }
            }
        }

        private bool AllEquals(byte[] b)
        {
            List<byte> _d = new List<byte>(b);
            _d.Sort();
            if(_d[0] == _d[15])
            {
                return true;
            } else
            {
                return false;
            }
        }
    }
}
