using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VS.Core;
using VS.Data;
using VS.Parser;
namespace VS.Utils
{
    public class ToolBox
    {
        public static uint GetNumPositiveBits(int bnumt)
        {
            uint inc = 0;
            BitArray b = new BitArray(new int[] { bnumt });
            if (bnumt != 0)
            {
                for (var j = 0; j < b.Length; j++)
                {
                    if (b.Get(j))
                    {
                        inc++;
                    }
                }
            }
            return inc;
        }
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
        public static void FeedDatabases(string[] DataPaths)
        {

            VSPConfig conf = Memory.LoadConfig();
            if (DataPaths == null)
            {
                DataPaths = new string[]{
                    conf.VSPath+"MENU/SHIELD.SYD",
                    conf.VSPath+"MENU/ARMOR.SYD",
                    conf.VSPath+"MENU/BLADE.SYD",
                    conf.VSPath+"MENU/ITEMNAME.BIN",
                    conf.VSPath+"MENU/ITEMHELP.BIN",
                    conf.VSPath+"MENU/MCMAN.BIN",
                    conf.VSPath+"MENU/MAINMENU.PRG",
                    conf.VSPath+"MENU/MENU0.PRG", // MENU0.PRG  Spells
                    conf.VSPath+"MENU/MENU1.PRG", // MENU1.PRG  Break arts
                    conf.VSPath+"MENU/MENU2.PRG", // MENU2.PRG  Combat technics 
                    conf.VSPath+"MENU/MENU3.PRG", // MENU3.PRG  Status info & Equip menu
                    conf.VSPath+"MENU/MENU4.PRG", // MENU4.PRG  Body part info & buff, debuff informations
                    conf.VSPath+"MENU/MENU5.PRG", // MENU5.PRG  Mini map & nom des zones
                    conf.VSPath+"MENU/MENU7.PRG", // MENU7.PRG  Reserve
                    conf.VSPath+"MENU/MENU8.PRG", // MENU8.PRG  Menu Option
                    conf.VSPath+"MENU/MENU9.PRG", // MENU9.PRG  Menu Scores & achievements
                    //conf.VSPath+"MENU/MENUA.PRG", // empty
                    conf.VSPath+"MENU/MENUB.PRG", // MENUB.PRG  Loot menu
                    conf.VSPath+"MENU/MENUC.PRG", // MENU9.PRG  ?
                    conf.VSPath+"MENU/MENUD.PRG", // MENU9.PRG  Inventory / Reserve
                    conf.VSPath+"MENU/MENUE.PRG", // MENU9.PRG  Game Help
                    conf.VSPath+"MENU/MENUF.PRG", // MENU9.PRG  After boss roulette
                    conf.VSPath+"MENU/MENU12.BIN", // Forge
                    conf.VSPath+"MENU/MENUBG.BIN",
                    conf.VSPath+"SMALL/MON.BIN"      // Monstres names & descriptions
                };
            }
            foreach (string path in DataPaths)
            {
                FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                BinaryReader buffer = new BinaryReader(fileStream);

                string[] h = path.Split("/"[0]);

                switch (h[h.Length - 1])
                {
                    case "SHIELD.SYD":
                        //Debug.Log("VS/MENU/SHIELD.SYD");
                        Shield.list = new List<Shield>();
                        buffer.BaseStream.Position = 0x0178;
                        for (var i = 0; i < 16; i++)
                        {
                            //ID|ID.WEP|armor type 01(shield)|gem slots|STR|INT|AGI|always 00
                            Shield.list.Add(new Shield(buffer.ReadBytes(8)));
                        }
                        break;
                    case "ARMOR.SYD":
                        //Debug.Log("VS/MENU/ARMOR.SYD");
                        Armor.list = new List<Armor>();
                        buffer.BaseStream.Position = 0x1498;
                        for (var i = 0; i < 80; i++)
                        {
                            //ID|ID.WEP|armor type[01-05]|00|STR|INT|AGI|always 00
                            Armor.list.Add(new Armor(buffer.ReadBytes(8)));
                        }
                        break;
                    case "BLADE.SYD":
                        //Debug.Log("VS/MENU/BLADE.SYD");
                        Blade.list = new List<Blade>();
                        Blade.list.Add(new Blade());
                        buffer.BaseStream.Position = 0x2DE0;
                        for (var i = 0; i < 90; i++)
                        {
                            // Damage types : 1 = Blunt - 2 = Edged  -  3 = Piercing
                            //22   22         03          02    02  02  0000 23  00  FA  00  06  05   06       01       Holy Win
                            //ID|ID.WEP|weapon type|damage type|02|Risk|0000|STR|INT|AGI|00|Range|?|range|always 01
                            Blade.list.Add(new Blade(buffer.ReadBytes(16)));
                        }
                        break;
                    case "ITEMNAME.BIN":
                        buffer.BaseStream.Position = 0x18;
                        string sname = "";
                        while (buffer.BaseStream.Position < buffer.BaseStream.Length)
                        {
                            byte b = buffer.ReadByte();
                            if (b == 0xE7)
                            {
                                L10n.itemNames.Add(sname);
                                sname = "";
                            }
                            else sname = sname + L10n.Charset(b);
                        }
                        break;
                    case "ITEMHELP.BIN":
                        // list of 2 bytes, the second seems to be incremental in a certain way from 0x02 to 0x31
                        // lots of things i don't know at the begining so let's jump.
                        buffer.BaseStream.Position = 0x054E;
                        sname = "";
                        while (buffer.BaseStream.Position < buffer.BaseStream.Length)
                        {
                            byte b = buffer.ReadByte();
                            if (b == 0xE7)
                            {
                                L10n.itemDescs.Add(sname);
                                sname = "";
                            }
                            else sname = sname + L10n.Charset(b);
                        }
                        break;
                    case "MCMAN.BIN":
                        sname = "";
                        while (buffer.BaseStream.Position < buffer.BaseStream.Length)
                        {
                            byte b = buffer.ReadByte();
                            if (b == 0xE7)
                            {
                                L10n.menu.Add(sname);
                                sname = "";
                            }
                            else sname = sname + L10n.Charset(b);
                        }
                        break;
                    case "MENU0.PRG":
                        //Debug.Log("MENU0");
                        sname = "";
                        while (buffer.BaseStream.Position < buffer.BaseStream.Length)
                        {
                            byte b = buffer.ReadByte();
                            if (b == 0xE7)
                            {
                                L10n.menu.Add(sname);
                                //Debug.Log(sname);
                                sname = "";
                            }
                            else sname = sname + L10n.Charset(b);
                        }
                        break;
                    case "MENU1.PRG":
                        //Debug.Log("MENU1");
                        sname = "";
                        while (buffer.BaseStream.Position < buffer.BaseStream.Length)
                        {
                            byte b = buffer.ReadByte();
                            if (b == 0xE7)
                            {
                                L10n.menu.Add(sname);
                                //Debug.Log(sname);
                                sname = "";
                            }
                            else sname = sname + L10n.Charset(b);
                        }
                        break;
                    case "MENU2.PRG":
                        //Debug.Log("MENU2");
                        sname = "";
                        while (buffer.BaseStream.Position < buffer.BaseStream.Length)
                        {
                            byte b = buffer.ReadByte();
                            if (b == 0xE7)
                            {
                                L10n.menu.Add(sname);
                                //Debug.Log(sname);
                                sname = "";
                            }
                            else sname = sname + L10n.Charset(b);
                        }
                        break;
                    case "MENU3.PRG":
                        //Debug.Log("MENU3");
                        sname = "";
                        while (buffer.BaseStream.Position < buffer.BaseStream.Length)
                        {
                            byte b = buffer.ReadByte();
                            if (b == 0xE7)
                            {
                                L10n.menu.Add(sname);
                                //Debug.Log(sname);
                                sname = "";
                            }
                            else sname = sname + L10n.Charset(b);
                        }
                        break;
                    case "MENU4.PRG":
                        //Debug.Log("MENU4");
                        sname = "";
                        while (buffer.BaseStream.Position < buffer.BaseStream.Length)
                        {
                            byte b = buffer.ReadByte();
                            if (b == 0xE7)
                            {
                                L10n.menu.Add(sname);
                                //Debug.Log(sname);
                                sname = "";
                            }
                            else sname = sname + L10n.Charset(b);
                        }
                        break;
                    case "MENU5.PRG":
                        //Debug.Log("MENU5");
                        sname = "";
                        while (buffer.BaseStream.Position < buffer.BaseStream.Length)
                        {
                            byte b = buffer.ReadByte();
                            if (b == 0xE7)
                            {
                                L10n.menu.Add(sname);
                                //Debug.Log(sname);
                                sname = "";
                            }
                            else sname = sname + L10n.Charset(b);
                        }
                        break;
                    case "MENU7.PRG":
                        //Debug.Log("MENU7");
                        sname = "";
                        while (buffer.BaseStream.Position < buffer.BaseStream.Length)
                        {
                            byte b = buffer.ReadByte();
                            if (b == 0xE7)
                            {
                                L10n.menu.Add(sname);
                                //Debug.Log(sname);
                                sname = "";
                            }
                            else sname = sname + L10n.Charset(b);
                        }
                        break;
                    case "VMENU8.PRG":
                        //Debug.Log("MENU8");
                        sname = "";
                        while (buffer.BaseStream.Position < buffer.BaseStream.Length)
                        {
                            byte b = buffer.ReadByte();
                            if (b == 0xE7)
                            {
                                L10n.menu.Add(sname);
                                //Debug.Log(sname);
                                sname = "";
                            }
                            else sname = sname + L10n.Charset(b);
                        }
                        break;
                    case "MENU9.PRG":
                        //Debug.Log("MENU9");
                        sname = "";
                        while (buffer.BaseStream.Position < buffer.BaseStream.Length)
                        {
                            byte b = buffer.ReadByte();
                            if (b == 0xE7)
                            {
                                L10n.menu.Add(sname);
                                //Debug.Log(sname);
                                sname = "";
                            }
                            else sname = sname + L10n.Charset(b);
                        }
                        break;
                    case "MENUB.PRG":
                        //Debug.Log("MENUB");
                        sname = "";
                        while (buffer.BaseStream.Position < buffer.BaseStream.Length)
                        {
                            byte b = buffer.ReadByte();
                            if (b == 0xE7)
                            {
                                L10n.menu.Add(sname);
                                //Debug.Log(sname);
                                sname = "";
                            }
                            else sname = sname + L10n.Charset(b);
                        }
                        break;
                    case "MENUC.PRG":
                        //Debug.Log("MENUC");
                        sname = "";
                        while (buffer.BaseStream.Position < buffer.BaseStream.Length)
                        {
                            byte b = buffer.ReadByte();
                            if (b == 0xE7)
                            {
                                L10n.menu.Add(sname);
                                //Debug.Log(sname);
                                sname = "";
                            }
                            else sname = sname + L10n.Charset(b);
                        }
                        break;
                    case "MENUD.PRG":
                        //Debug.Log("MENUD");
                        sname = "";
                        while (buffer.BaseStream.Position < buffer.BaseStream.Length)
                        {
                            byte b = buffer.ReadByte();
                            if (b == 0xE7)
                            {
                                L10n.menu.Add(sname);
                                //Debug.Log(sname);
                                sname = "";
                            }
                            else sname = sname + L10n.Charset(b);
                        }
                        break;
                    case "MENUE.PRG":
                        //Debug.Log("MENUE");
                        sname = "";
                        buffer.BaseStream.Position = 0x1000;
                        while (buffer.BaseStream.Position < buffer.BaseStream.Length)
                        {
                            byte b = buffer.ReadByte();
                            if (b == 0xE7)
                            {
                                L10n.menu.Add(sname);
                                //Debug.Log(sname);
                                sname = "";
                            }
                            else sname = sname + L10n.Charset(b);
                        }
                        break;
                    case "MENUF.PRG":
                        //Debug.Log("MENUF");
                        sname = "";
                        buffer.BaseStream.Position = 0x1000;
                        while (buffer.BaseStream.Position < buffer.BaseStream.Length)
                        {
                            byte b = buffer.ReadByte();
                            if (b == 0xE7)
                            {
                                L10n.menu.Add(sname);
                                //Debug.Log(sname);
                                sname = "";
                            }
                            else sname = sname + L10n.Charset(b);
                        }
                        break;
                    case "MENU12.BIN":
                        //Debug.Log("MENU12");
                        // FORGE MENU
                        sname = "";
                        while (buffer.BaseStream.Position < buffer.BaseStream.Length)
                        {
                            byte b = buffer.ReadByte();
                            if (b == 0xE7)
                            {
                                L10n.menu.Add(sname);
                                //Debug.Log(sname);
                                sname = "";
                            }
                            else sname = sname + L10n.Charset(b);
                        }
                        break;
                    case "MENUBG.BIN":
                        // NOT  FONCTIONNAL
                        /*
                        buffer.ReadByte();

                        List<Color32> hcolors = new List<Color32>();
                            for (uint i = 0; i < 308; i++)
                            {
                                hcolors.Add(VSHelper.BitColorConverter(buffer.ReadUInt16()));
                            }

                        int height = 128;
                        int width = 128;
                        List<int> cluts = new List<int>();

                        buffer.BaseStream.Position = 0x200;
                        for (uint x = 0; x < height; x++)
                        {
                            for (uint y = 0; y < width; y++)
                            {
                                cluts.Add(buffer.ReadByte());
                            }
                        }

                        Texture2D texture = new Texture2D(width, height);
                        List<Color> colors = new List<Color>();
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                if (cluts[(int)((y * width) + x)] < 308)
                                {
                                    colors.Add(hcolors[cluts[(int)((y * width) + x)]]);
                                }
                                else
                                {
                                    colors.Add(Color.clear);
                                }
                            }
                        }

                        Texture2D tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
                        tex.SetPixels(colors.ToArray());
                        tex.Apply();
                        texture = tex;
                        byte[] bytes = texture.EncodeToPNG();
                        File.WriteAllBytes(Application.dataPath + "/../Assets/Resources/Textures/MENU/MENUBG.BIN.png", bytes);
                        */
                        break;
                    case "MON.BIN":
                        sname = "";
                        while (buffer.BaseStream.Position < buffer.BaseStream.Length)
                        {
                            byte b = buffer.ReadByte();
                            if (b == 0xE7)
                            {
                                L10n.menu.Add(sname);
                                //Debug.Log(sname);
                                sname = "";
                            }
                            else sname = sname + L10n.Charset(b);
                        }
                        break;
                }

                buffer.Close();
                fileStream.Close();
            }


        }

        public static string[] GetZNDRoomList(uint ZNDId)
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

            if (table[ZNDId] != null)
            {
                return table[ZNDId];
            }

            return new string[0];
        }
        public static void PSXConvADSR(AKAOInstrumentRegion region, ushort ADSR1, ushort ADSR2)
        {
            ushort Am = (ushort)((ADSR1 & 0x8000) >> 15);  // if 1, then Exponential, else linear
            ushort Ar = (ushort)((ADSR1 & 0x7F00) >> 8);
            ushort Dr = (ushort)((ADSR1 & 0x00F0) >> 4);
            ushort Sl = (ushort)(ADSR1 & 0x000F);
            ushort Rm = (ushort)((ADSR2 & 0x0020) >> 5);
            ushort Rr = (ushort)(ADSR2 & 0x001F);

            // The following are unimplemented in conversion (because DLS and SF2 do not support Sustain
            // Rate)
            ushort Sm = (ushort)((ADSR2 & 0x8000) >> 15);
            ushort Sd = (ushort)((ADSR2 & 0x4000) >> 14);
            ushort Sr = (ushort)((ADSR2 >> 6) & 0x7F);

            // Make sure all the ADSR values are within the valid ranges
            if (((Am & ~0x01) != 0) || ((Ar & ~0x7F) != 0) || ((Dr & ~0x0F) != 0) || ((Sl & ~0x0F) != 0) ||
                ((Rm & ~0x01) != 0) || ((Rr & ~0x1F) != 0) || ((Sm & ~0x01) != 0) || ((Sd & ~0x01) != 0) ||
                ((Sr & ~0x7F) != 0))
            {
                Debug.LogError("ADSR parameter(s) out of range (Am : " + Am + ", Ar : " + Ar + ", Dr : " + Dr + ", Sl : " + Sl + ", Rm : " + Rm + ", Rr : " + Rr + ", Sm : " + Sm + ", Sd : " + Sd + ", Sr : " + Sr + ")");

                return;
            }

            // PS1 games use 44k, PS2 uses 48k
            double sampleRate = 44100;

            int[] rateIncTable = { 0, 4, 6, 8, 9, 10, 11, 12 };
            ulong envelope_level;
            double samples = 0;
            ulong rate;
            ulong remainder;
            double timeInSecs;
            int l;

            ulong r, rs, rd;
            int i;
            // build the rate table according to Neill's rules
            ulong[] RateTable = new ulong[160];

            r = 3;
            rs = 1;
            rd = 0;

            // we start at pos 32 with the real values... everything before is 0
            for (i = 32; i < 160; i++)
            {
                if (r < 0x3FFFFFFF)
                {
                    r += rs;
                    rd++;
                    if (rd == 5)
                    {
                        rd = 1;
                        rs *= 2;
                    }
                }
                if (r > 0x3FFFFFFF)
                {
                    r = 0x3FFFFFFF;
                }

                RateTable[i] = r;
            }

            // to get the dls 32 bit time cents, take log base 2 of number of seconds * 1200 * 65536
            // (dls1v11a.pdf p25).

            //	if (RateTable[(Ar^0x7F)-0x10 + 32] == 0)
            //		realADSR->attack_time = 0;
            //	else
            //	{
            if ((Ar ^ 0x7F) < 0x10)
            {
                Ar = 0;
            }
            // if linear Ar Mode

            if (Am == 0)
            {
                rate = RateTable[Mathf.FloorToInt((Ar ^ 0x7F) - 0x10 + 32)];
                samples = Mathf.CeilToInt(0x7FFFFFFF / rate);
            }
            else if (Am == 1)
            {
                rate = RateTable[Mathf.FloorToInt((Ar ^ 0x7F) - 0x10) + 32];
                samples = 0x60000000 / rate;
                remainder = 0x60000000 % rate;
                rate = RateTable[Mathf.FloorToInt((Ar ^ 0x7F) - 0x18) + 32];
                samples += Mathf.CeilToInt(Mathf.Max(0, 0x1FFFFFFF - remainder) / rate);
            }

            timeInSecs = samples / sampleRate;


            region.attack_time = timeInSecs;
            //	}

            // Decay Time

            envelope_level = 0x7FFFFFFF;

            bool bSustainLevFound = false;
            uint realSustainLevel = 0;
            // DLS decay rate value is to -96db (silence) not the sustain level
            for (l = 0; envelope_level > 0; l++)
            {
                if (4 * (Dr ^ 0x1F) < 0x18)
                {
                    Dr = 0;
                }

                switch ((envelope_level >> 28) & 0x7)
                {
                    case 0:
                        envelope_level -= RateTable[Mathf.FloorToInt((4 * (Dr ^ 0x1F)) - 0x18 + 0) + 32];
                        break;
                    case 1:
                        envelope_level -= RateTable[Mathf.FloorToInt((4 * (Dr ^ 0x1F)) - 0x18 + 4) + 32];
                        break;
                    case 2:
                        envelope_level -= RateTable[Mathf.FloorToInt((4 * (Dr ^ 0x1F)) - 0x18 + 6) + 32];
                        break;
                    case 3:
                        envelope_level -= RateTable[Mathf.FloorToInt((4 * (Dr ^ 0x1F)) - 0x18 + 8) + 32];
                        break;
                    case 4:
                        envelope_level -= RateTable[Mathf.FloorToInt((4 * (Dr ^ 0x1F)) - 0x18 + 9) + 32];
                        break;
                    case 5:
                        envelope_level -= RateTable[Mathf.FloorToInt((4 * (Dr ^ 0x1F)) - 0x18 + 10) + 32];
                        break;
                    case 6:
                        envelope_level -= RateTable[Mathf.FloorToInt((4 * (Dr ^ 0x1F)) - 0x18 + 11) + 32];
                        break;
                    case 7:
                        envelope_level -= RateTable[Mathf.FloorToInt((4 * (Dr ^ 0x1F)) - 0x18 + 12) + 32];
                        break;
                }
                if (!bSustainLevFound && ((envelope_level >> 27) & 0xF) <= Sl)
                {
                    realSustainLevel = (uint)envelope_level;
                    bSustainLevFound = true;
                }
            }
            samples = l;
            timeInSecs = samples / sampleRate;
            region.decay_time = timeInSecs;

            // Sustain Rate

            envelope_level = 0x7FFFFFFF;
            // increasing... we won't even bother
            if (Sd == 0)
            {
                region.sustain_time = -1;
            }
            else
            {
                if (Sr == 0x7F)
                {
                    region.sustain_time = -1;  // this is actually infinite
                }
                else
                {
                    // linear
                    if (Sm == 0)
                    {
                        rate = RateTable[Mathf.FloorToInt((Sr ^ 0x7F) - 0x0F) + 32];
                        samples = Mathf.CeilToInt(0x7FFFFFFF / rate);
                    }
                    else
                    {
                        l = 0;
                        // DLS decay rate value is to -96db (silence) not the sustain level
                        while (envelope_level > 0)
                        {
                            ulong envelope_level_diff = 0;
                            ulong envelope_level_target = 0;

                            switch ((envelope_level >> 28) & 0x7)
                            {
                                case 0:
                                    envelope_level_target = 0x00000000;
                                    envelope_level_diff =
                                        RateTable[Mathf.FloorToInt((Sr ^ 0x7F) - 0x1B + 0) + 32];
                                    break;
                                case 1:
                                    envelope_level_target = 0x0fffffff;
                                    envelope_level_diff =
                                        RateTable[Mathf.FloorToInt((Sr ^ 0x7F) - 0x1B + 4) + 32];
                                    break;
                                case 2:
                                    envelope_level_target = 0x1fffffff;
                                    envelope_level_diff =
                                        RateTable[Mathf.FloorToInt((Sr ^ 0x7F) - 0x1B + 6) + 32];
                                    break;
                                case 3:
                                    envelope_level_target = 0x2fffffff;
                                    envelope_level_diff =
                                        RateTable[Mathf.FloorToInt((Sr ^ 0x7F) - 0x1B + 8) + 32];
                                    break;
                                case 4:
                                    envelope_level_target = 0x3fffffff;
                                    envelope_level_diff =
                                        RateTable[Mathf.FloorToInt((Sr ^ 0x7F) - 0x1B + 9) + 32];
                                    break;
                                case 5:
                                    envelope_level_target = 0x4fffffff;
                                    envelope_level_diff =
                                        RateTable[Mathf.FloorToInt((Sr ^ 0x7F) - 0x1B + 10) + 32];
                                    break;
                                case 6:
                                    envelope_level_target = 0x5fffffff;
                                    envelope_level_diff =
                                        RateTable[Mathf.FloorToInt((Sr ^ 0x7F) - 0x1B + 11) + 32];
                                    break;
                                case 7:
                                    envelope_level_target = 0x6fffffff;
                                    envelope_level_diff =
                                        RateTable[Mathf.FloorToInt((Sr ^ 0x7F) - 0x1B + 12) + 32];
                                    break;
                            }

                            ulong steps =
                                (envelope_level - envelope_level_target + (envelope_level_diff - 1)) /
                                envelope_level_diff;
                            envelope_level -= (envelope_level_diff * steps);
                            l += (int)steps;
                        }
                        samples = l;
                    }
                    timeInSecs = samples / sampleRate;
                    region.sustain_time = LinAmpDecayTimeToLinDBDecayTime(timeInSecs, 0x800);
                }
            }

            // Sustain Level
            // realADSR->sustain_level =
            // (double)envelope_level/(double)0x7FFFFFFF;//(long)ceil((double)envelope_level *
            // 0.030517578139210854);	//in DLS, sustain level is measured as a percentage
            if (Sl == 0)
            {
                realSustainLevel = 0x07FFFFFF;
            }

            region.sustain_level = (int)realSustainLevel / 0x7FFFFFFF;

            // If decay is going unused, and there's a sustain rate with sustain level close to max...
            //  we'll put the sustain_rate in place of the decay rate.
            if ((region.decay_time < 2 || (Dr == 0x0F && Sl >= 0x0C)) && Sr < 0x7E && Sd == 1)
            {
                region.sustain_level = 0;
                region.decay_time = region.sustain_time;
                // realADSR->decay_time = 0.5;
            }

            // Release Time

            // sustain_envelope_level = envelope_level;

            // We do this because we measure release time from max volume to 0, not from sustain level to 0
            envelope_level = 0x7FFFFFFF;

            // if linear Rr Mode
            if (Rm == 0)
            {
                rate = RateTable[Mathf.FloorToInt((4 * (Rr ^ 0x1F)) - 0x0C) + 32];

                if (rate != 0)
                {
                    samples = Mathf.Ceil(envelope_level / rate);
                }
                else
                {
                    samples = 0;
                }
            }
            else if (Rm == 1)
            {
                if ((Rr ^ 0x1F) * 4 < 0x18)
                {
                    Rr = 0;
                }

                for (l = 0; envelope_level > 0; l++)
                {
                    switch ((envelope_level >> 28) & 0x7)
                    {
                        case 0:
                            envelope_level -= RateTable[Mathf.FloorToInt((4 * (Rr ^ 0x1F)) - 0x18 + 0) + 32];
                            break;
                        case 1:
                            envelope_level -= RateTable[Mathf.FloorToInt((4 * (Rr ^ 0x1F)) - 0x18 + 4) + 32];
                            break;
                        case 2:
                            envelope_level -= RateTable[Mathf.FloorToInt((4 * (Rr ^ 0x1F)) - 0x18 + 6) + 32];
                            break;
                        case 3:
                            envelope_level -= RateTable[Mathf.FloorToInt((4 * (Rr ^ 0x1F)) - 0x18 + 8) + 32];
                            break;
                        case 4:
                            envelope_level -= RateTable[Mathf.FloorToInt((4 * (Rr ^ 0x1F)) - 0x18 + 9) + 32];
                            break;
                        case 5:
                            envelope_level -= RateTable[Mathf.FloorToInt((4 * (Rr ^ 0x1F)) - 0x18 + 10) + 32];
                            break;
                        case 6:
                            envelope_level -= RateTable[Mathf.FloorToInt((4 * (Rr ^ 0x1F)) - 0x18 + 11) + 32];
                            break;
                        case 7:
                            envelope_level -= RateTable[Mathf.FloorToInt((4 * (Rr ^ 0x1F)) - 0x18 + 12) + 32];
                            break;
                    }
                }
                samples = l;
            }
            timeInSecs = samples / sampleRate;

            // theRate = timeInSecs / sustain_envelope_level;
            // timeInSecs = 0x7FFFFFFF * theRate;	//the release time value is more like a rate.  It is
            // the time from max value to 0, not from sustain level. if (Rm == 0) // if it's linear
            // timeInSecs *=
            // LINEAR_RELEASE_COMPENSATION;

            region.release_time = LinAmpDecayTimeToLinDBDecayTime(timeInSecs, 0x800);

            // We need to compensate the decay and release times to represent them as the time from full vol
            // to -100db where the drop in db is a fixed amount per time unit (SoundFont2 spec for vol
            // envelopes, pg44.)
            //  We assume the psx envelope is using a linear scale wherein envelope_level / 2 == half
            //  loudness. For a linear release mode (Rm == 0), the time to reach half volume is simply half
            //  the time to reach 0.
            // Half perceived loudness is -10db. Therefore, time_to_half_vol * 10 == full_time * 5 == the
            // correct SF2 time
            // realADSR->decay_time = LinAmpDecayTimeToLinDBDecayTime(realADSR->decay_time, 0x800);
            // realADSR->sustain_time = LinAmpDecayTimeToLinDBDecayTime(realADSR->sustain_time, 0x800);
            // realADSR->release_time = LinAmpDecayTimeToLinDBDecayTime(realADSR->release_time, 0x800);

            // Calculations are done, so now add the articulation data
            // artic->AddADSR(attack_time, Am, decay_time, sustain_lev, release_time, 0);
            return;
        }
        public static double LinAmpDecayTimeToLinDBDecayTime(double secondsToFullAtten, int linearVolumeRange)
        {
            double expMinDecibel = -100.0;
            double linearMinDecibel = Mathf.Log10(1.0f / linearVolumeRange) * 20.0;
            double linearToExpScale = Mathf.Log((float)(linearMinDecibel - expMinDecibel)) / Mathf.Log(2.0f);
            return secondsToFullAtten * linearToExpScale;
        }
    }
}
