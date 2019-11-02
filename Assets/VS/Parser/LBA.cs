using System.Collections.Generic;
using System.IO;

namespace VS.Parser
{
    public class LBA
    {


        public static string checkVSROM(string path)
        {
            if (!Directory.Exists(path + "BATTLE/"))
            {
                return null;
            }

            if (!Directory.Exists(path + "SOUND/"))
            {
                return null;
            }

            if (!Directory.Exists(path + "MUSIC/"))
            {
                return null;
            }

            if (!Directory.Exists(path + "SE/"))
            {
                return null;
            }

            if (!Directory.Exists(path + "MENU/"))
            {
                return null;
            }

            if (!Directory.Exists(path + "SMALL/"))
            {
                return null;
            }

            if (!Directory.Exists(path + "BG/"))
            {
                return null;
            }

            if (!Directory.Exists(path + "OBJ/"))
            {
                return null;
            }

            if (!Directory.Exists(path + "EVENT/"))
            {
                return null;
            }

            if (!Directory.Exists(path + "EFFECT/"))
            {
                return null;
            }

            if (!Directory.Exists(path + "GIM/"))
            {
                return null;
            }

            if (!Directory.Exists(path + "TITLE/"))
            {
                return null;
            }

            if (!Directory.Exists(path + "MOV/"))
            {
                return null;
            }

            if (!Directory.Exists(path + "ENDING/"))
            {
                return null;
            }
            /*
            (US) SLUS-01040
            (JP) SLPS-02377
            (UK) SLES-02754
            (FR) SLES-02755
            (GR) SLES-02756
            (JP) SLPS-91457
            (JP) SLPM-87393
            (CH) SLPS-02804 
            */
            List<string> sls = new List<string> {
                "SLUS_010.40",
                "SLPS_023.77",
                "SLES_027.54",
                "SLES_027.55",
                "SLES_027.56",
                "SLPS_914.57",
                "SLPM_873.93",
                "SLPS_028.04"
            };
            string slt = null;
            foreach (string s in sls)
            {
                if (File.Exists(path + s))
                {
                    slt = s;
                    break;
                }
            }

            return slt;
        }
    }


}