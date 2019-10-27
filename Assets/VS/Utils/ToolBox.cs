using System;
using System.IO;
using UnityEngine;


namespace VS.Utils
{
    public class ToolBox
    {

        public static string GetGameObjectPath(GameObject obj, string baseName)
        {
            string path = obj.name;
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = obj.name + "/" + path;
            }
            path = path.Remove(0, baseName.Length + 1);
            return path;
        }
        public static GameObject findBoneIn(string boneName, GameObject obj)
        {
            foreach (Transform child in obj.transform)
            {
                if (child.name == boneName)
                {
                    return child.gameObject;
                }
                else
                {
                    GameObject littleChild = findBoneIn(boneName, child.gameObject);
                    if (littleChild != null)
                    {
                        return littleChild;
                    }
                }
            }

            return null;
        }
        public static Quaternion quatFromAxisAnle(Vector3 axis, float angle)
        {
            Quaternion q = new Quaternion(axis.x * Mathf.Sin(angle / 2), axis.y * Mathf.Sin(angle / 2), axis.z * Mathf.Sin(angle / 2), Mathf.Cos(angle / 2));
            q.Normalize();
            return q;
        }

        public static float rot13toRad(int angle)
        {
            float f = Mathf.PI / 4096;
            return (float)(f * angle);
        }

        public static byte[] EndianSwitcher(byte[] bytes)
        {
            Array.Reverse(bytes);
            return bytes;
        }
        public static void DirExNorCreate(string dirPath)
        {
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
        }

        public static Color32 BitColorConverter(ushort rawColor)
        {
            if (rawColor == 0)
            {
                return new Color32(0, 0, 0, 0);
            }
            else
            {
                int a = (rawColor & 0x8000) >> 15;
                int b = (rawColor & 0x7C00) >> 10;
                int g = (rawColor & 0x03E0) >> 5;
                int r = (rawColor & 0x001F);
                if (r == 0 && g == 0 && b == 0)
                {
                    if ((rawColor & 0x8000) == 0)
                    {
                        // black, and the alpha bit is NOT set
                        a = (byte)0; // totally transparent
                    }
                    else
                    {
                        // black, and the alpha bit IS set
                        a = (byte)255; // totally opaque
                    }
                }
                else
                {
                    if ((rawColor & 0x8000) == 0)
                    {
                        // some color, and the alpha bit is NOT set
                        a = (byte)255; // totally opaque
                    }
                    else
                    {
                        // some color, and the alpha bit IS set
                        a = (byte)250; // some variance of transparency
                    }
                }

                Color32 color = new Color32((byte)(r * 8), (byte)(g * 8), (byte)(b * 8), (byte)a);
                return color;
            }
        }


        /// <summary>
        /// http://datacrystal.romhacking.net/wiki/Vagrant_Story:rooms_list
        /// </summary>
        /// <param name="mdpName">filename.MPD</param>
        public static string MPDToZND(string mdpName, bool toLowerCase = false)
        {
            string[][] table = new string[256][];
            table[1] = new string[] { "MAP001.MPD", "MAP215.MPD" };
            table[2] = new string[] { "MAP002.MPD", "MAP216.MPD" };
            table[3] = new string[] { "MAP003.MPD", "MAP217.MPD" };
            table[4] = new string[] { "MAP004.MPD", "MAP218.MPD" };
            table[5] = new string[] { "MAP005.MPD", "MAP219.MPD" };
            table[6] = new string[] { "MAP006.MPD" };
            table[7] = new string[] { "MAP007.MPD" };
            table[8] = new string[] { "MAP008.MPD" };
            table[9] = new string[] { "MAP009.MPD", "MAP010.MPD", "MAP011.MPD", "MAP012.MPD", "MAP013.MPD", "MAP014.MPD", "MAP015.MPD", "MAP016.MPD", "MAP017.MPD", "MAP018.MPD", "MAP019.MPD", "MAP020.MPD", "MAP021.MPD", "MAP022.MPD", "MAP023.MPD", "MAP024.MPD", "MAP027.MPD", "MAP409.MPD", "MAP412.MPD" };
            table[10] = new string[] { "MAP211.MPD" }; // Ashley and Merlose outside the Wine Cellar gate
            table[11] = new string[] { "MAP025.MPD" };
            table[12] = new string[] { "MAP026.MPD", "MAP408.MPD" };
            table[13] = new string[] { "MAP028.MPD", "MAP029.MPD", "MAP030.MPD", "MAP031.MPD", "MAP032.MPD", "MAP033.MPD", "MAP034.MPD", "MAP035.MPD", "MAP036.MPD", "MAP037.MPD", "MAP038.MPD", "MAP039.MPD", "MAP040.MPD", "MAP041.MPD", "MAP042.MPD", "MAP043.MPD", "MAP044.MPD", "MAP045.MPD" };
            table[14] = new string[] { "MAP046.MPD" };
            table[15] = new string[] { "MAP047.MPD", "MAP048.MPD", "MAP049.MPD", "MAP050.MPD", "MAP051.MPD", "MAP052.MPD", "MAP053.MPD", "MAP054.MPD", "MAP055.MPD", "MAP056.MPD", "MAP057.MPD", "MAP058.MPD", "MAP059.MPD" };
            table[16] = new string[] { "MAP060.MPD" };
            table[17] = new string[] { "MAP061.MPD" };
            table[18] = new string[] { "MAP062.MPD" }; // Bardorba and Rosencrantz
            table[19] = new string[] { "MAP212.MPD" }; // Ashley's flashback
            table[20] = new string[] { "MAP213.MPD" }; // VKP briefing
            table[21] = new string[] { "MAP214.MPD" }; // Ashley meets Merlose outside manor
            table[22] = new string[] { "MAP063.MPD", "MAP064.MPD", "MAP065.MPD", "MAP066.MPD", "MAP067.MPD", "MAP068.MPD", "MAP069.MPD", "MAP070.MPD", "MAP071.MPD", "MAP072.MPD" }; // 22
            table[23] = new string[] { "MAP073.MPD", "MAP074.MPD", "MAP075.MPD", "MAP076.MPD", "MAP077.MPD", "MAP078.MPD" }; // 23
            table[24] = new string[] { "MAP079.MPD", "MAP080.MPD", "MAP081.MPD", "MAP082.MPD", "MAP083.MPD", "MAP084.MPD", "MAP085.MPD", "MAP086.MPD", "MAP087.MPD", "MAP088.MPD", "MAP089.MPD", "MAP090.MPD", "MAP091.MPD", "MAP092.MPD", "MAP093.MPD", "MAP094.MPD" }; //
            table[25] = new string[] { "MAP095.MPD", "MAP096.MPD", "MAP097.MPD", "MAP098.MPD", "MAP099.MPD" }; //
            table[26] = new string[] { "MAP100.MPD" }; // Ashley finds Sydney in the Cathedral
            table[27] = new string[] { "MAP101.MPD", "MAP102.MPD" }; //
            table[28] = new string[] { "MAP105.MPD", "MAP106.MPD", "MAP107.MPD", "MAP108.MPD", "MAP109.MPD", "MAP110.MPD", "MAP111.MPD", "MAP112.MPD", "MAP113.MPD", "MAP114.MPD", "MAP115.MPD", "MAP116.MPD", "MAP117.MPD", "MAP118.MPD", "MAP119.MPD", "MAP120.MPD", "MAP121.MPD", "MAP122.MPD", "MAP123.MPD" }; // 28
            table[29] = new string[] { "MAP124.MPD", "MAP125.MPD", "MAP126.MPD", "MAP127.MPD", "MAP128.MPD", "MAP129.MPD", "MAP130.MPD" }; //
            table[30] = new string[] { "MAP139.MPD", "MAP140.MPD", "MAP141.MPD", "MAP142.MPD", "MAP143.MPD", "MAP144.MPD" }; //
            table[31] = new string[] { "MAP145.MPD", "MAP146.MPD" }; //
            table[32] = new string[] { "MAP147.MPD", "MAP148.MPD", "MAP149.MPD", "MAP150.MPD", "MAP151.MPD", "MAP152.MPD", "MAP153.MPD", "MAP154.MPD", "MAP155.MPD", "MAP156.MPD", "MAP157.MPD", "MAP158.MPD", "MAP159.MPD", "MAP160.MPD", "MAP161.MPD", "MAP162.MPD", "MAP163.MPD", "MAP164.MPD", "MAP165.MPD", "MAP166.MPD", "MAP167.MPD", "MAP168.MPD", "MAP169.MPD", "MAP170.MPD" }; //
            table[33] = new string[] { "MAP172.MPD" }; // 33 Merlose finds corpses at Leà Monde's entrance
            table[34] = new string[] { "MAP173.MPD" }; // 34 Dinas Walk
            table[35] = new string[] { "MAP174.MPD" }; //35
            table[36] = new string[] { "MAP175.MPD" }; // 36 Gharmes Walk
            table[37] = new string[] { "MAP176.MPD" }; //37
            table[38] = new string[] { "MAP177.MPD" }; // 38 The House Gilgitte
            table[39] = new string[] { "MAP171.MPD" }; // 39 Plateia Lumitar
            table[40] = new string[] { "MAP179.MPD", "MAP180.MPD", "MAP181.MPD", "MAP182.MPD", "MAP183.MPD", "MAP184.MPD", "MAP185.MPD", "MAP186.MPD", "MAP187.MPD", "MAP188.MPD", "MAP189.MPD", "MAP190.MPD", "MAP191.MPD", "MAP192.MPD", "MAP193.MPD", "MAP194.MPD", "MAP195.MPD", "MAP196.MPD", "MAP197.MPD", "MAP198.MPD", "MAP199.MPD", "MAP200.MPD", "MAP201.MPD", "MAP202.MPD", "MAP203.MPD", "MAP204.MPD" }; // 40 Snowfly Forest
            table[41] = new string[] { "MAP348.MPD", "MAP349.MPD", "MAP350.MPD" }; // 41 Snowfly Forest East
            table[42] = new string[] { "MAP205.MPD" }; // 42 Workshop "Work of Art"
            table[43] = new string[] { "MAP206.MPD" }; // 43 Workshop "Magic Hammer"
            table[44] = new string[] { "MAP207.MPD" }; // 44 Wkshop "Keane's Crafts"
            table[45] = new string[] { "MAP208.MPD" }; // 45 Workshop "Metal Works"
            table[46] = new string[] { "MAP209.MPD" }; // 46 Wkshop "Junction Point"
            table[47] = new string[] { "MAP210.MPD" }; // 47 Workshop "Godhands"
            table[48] = new string[] { "MAP220.MPD", "MAP221.MPD", "MAP222.MPD", "MAP223.MPD", "MAP224.MPD", "MAP225.MPD", "MAP226.MPD", "MAP227.MPD", "MAP228.MPD", "MAP229.MPD", "MAP230.MPD", "MAP231.MPD", "MAP232.MPD", "MAP233.MPD", "MAP234.MPD", "MAP235.MPD", "MAP236.MPD", "MAP237.MPD", "MAP238.MPD", "MAP239.MPD", "MAP240.MPD", "MAP241.MPD", "MAP242.MPD", "MAP243.MPD", "MAP244.MPD", "MAP245.MPD", "MAP246.MPD" }; // 48 Undercity West
            table[49] = new string[] { "MAP247.MPD", "MAP248.MPD", "MAP249.MPD", "MAP250.MPD", "MAP251.MPD", "MAP252.MPD", "MAP253.MPD", "MAP254.MPD", "MAP255.MPD", "MAP256.MPD", "MAP257.MPD", "MAP258.MPD", "MAP259.MPD" }; // 49 Undercity East
            table[50] = new string[] { "MAP260.MPD", "MAP261.MPD", "MAP262.MPD", "MAP263.MPD", "MAP264.MPD", "MAP265.MPD", "MAP266.MPD", "MAP267.MPD", "MAP268.MPD", "MAP269.MPD", "MAP270.MPD", "MAP271.MPD", "MAP272.MPD", "MAP273.MPD", "MAP274.MPD", "MAP275.MPD", "MAP276.MPD", "MAP277.MPD", "MAP278.MPD", "MAP279.MPD", "MAP280.MPD", "MAP281.MPD", "MAP282.MPD", "MAP283.MPD" }; //50
            table[51] = new string[] { "MAP284.MPD", "MAP285.MPD", "MAP286.MPD", "MAP287.MPD", "MAP288.MPD", "MAP289.MPD", "MAP290.MPD", "MAP291.MPD", "MAP292.MPD", "MAP293.MPD", "MAP294.MPD", "MAP295.MPD", "MAP296.MPD", "MAP297.MPD", "MAP298.MPD", "MAP299.MPD", "MAP300.MPD", "MAP301.MPD", "MAP302.MPD", "MAP303.MPD", "MAP304.MPD", "MAP305.MPD", "MAP306.MPD", "MAP307.MPD", "MAP308.MPD", "MAP309.MPD", "MAP310.MPD", "MAP410.MPD", "MAP411.MPD" }; // 51 Abandoned Mines B2
            table[52] = new string[] { "MAP351.MPD", "MAP352.MPD", "MAP353.MPD", "MAP354.MPD", "MAP355.MPD", "MAP356.MPD", "MAP357.MPD", "MAP358.MPD" }; // 52 Escapeway
            table[53] = new string[] { "MAP311.MPD", "MAP312.MPD", "MAP313.MPD", "MAP314.MPD", "MAP315.MPD", "MAP316.MPD", "MAP317.MPD", "MAP318.MPD", "MAP319.MPD", "MAP320.MPD", "MAP321.MPD", "MAP322.MPD", "MAP323.MPD", "MAP324.MPD", "MAP325.MPD", "MAP326.MPD", "MAP327.MPD", "MAP328.MPD", "MAP329.MPD", "MAP330.MPD", "MAP331.MPD", "MAP332.MPD", "MAP333.MPD", "MAP334.MPD", "MAP335.MPD", "MAP336.MPD", "MAP337.MPD", "MAP338.MPD", "MAP339.MPD", "MAP340.MPD", "MAP341.MPD", "MAP342.MPD" }; // 53 Limestone Quarry
            table[54] = new string[] { "MAP343.MPD", "MAP344.MPD", "MAP345.MPD", "MAP346.MPD", "MAP347.MPD" }; // 54
            table[55] = new string[382 - 359];
            for (int i = 0; i < 382 - 359; i++)
            {
                table[55][i] = "MAP" + (359 + i) + ".MPD";
            }

            table[56] = new string[408 - 382];
            for (int i = 0; i < 408 - 382; i++)
            {
                table[56][i] = "MAP" + (382 + i) + ".MPD";
            }

            table[57] = new string[] { "MAP103.MPD" }; //
            table[58] = new string[] { "MAP104.MPD" }; //
            table[59] = new string[] { "MAP413.MPD" }; //
            table[60] = new string[] { "MAP131.MPD" }; // 60
            table[61] = new string[] { "MAP132.MPD" }; //
            table[62] = new string[] { "MAP133.MPD" }; //
            table[63] = new string[] { "MAP134.MPD" }; //
            table[64] = new string[] { "MAP135.MPD" }; //
            table[65] = new string[] { "MAP136.MPD" }; //
            table[66] = new string[] { "MAP137.MPD" }; //
            table[67] = new string[] { "MAP138.MPD" }; //
            table[68] = new string[] { "MAP178.MPD" }; //
            table[69] = new string[] { "MAP414.MPD" }; //
            table[70] = new string[] { "MAP415.MPD" }; // 70

            table[96] = new string[] { "MAP427.MPD" }; // 96
            table[97] = new string[] { "MAP428.MPD" }; // 97
            table[98] = new string[] { "MAP429.MPD" }; // 98
            table[99] = new string[] { "MAP430.MPD" }; // 99
            table[100] = new string[] { "MAP000.MPD" }; // 100

            table[250] = new string[] { "MAP506.MPD" }; // 250

            for (int i = 0; i < 256; i++)
            {
                if (table[i] != null)
                {
                    for (int j = 0; j < table[i].Length; j++)
                    {
                        if (mdpName == table[i][j])
                        {
                            string ZNDId = i.ToString();
                            if (ZNDId.Length < 3)
                            {
                                ZNDId = "0" + ZNDId;
                            }

                            if (ZNDId.Length < 3)
                            {
                                ZNDId = "0" + ZNDId;
                            }

                            if (toLowerCase)
                            {
                                return "Zone" + ZNDId;
                            }
                            else
                            {
                                return "ZONE" + ZNDId + ".ZND";
                            }
                        }
                    }
                }
            }

            return null;
        }

    }
}
