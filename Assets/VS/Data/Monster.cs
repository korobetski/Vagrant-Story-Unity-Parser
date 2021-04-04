using System;
using System.Collections.Generic;
using UnityEngine;

namespace VS.Data
{

    [System.Serializable]
    public class Monster
    {
        public static List<Monster> list = new List<Monster>();
        public static string JSONlist()
        {
            string[] slst = new string[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                slst[i] = list[i].ToJSON();
            }
            return string.Concat("[", string.Join(", ", slst), "]");
        }



        public string name = "";
        public string desc = "";
        public uint id = 0;
        public byte shp1 = 0;
        public byte shp2 = 0;
        public byte uk1 = 0;
        public byte uk2 = 0;

        public Monster(byte[] rawDatas, uint id, string name = "", string desc = "")
        {
            shp1 = rawDatas[0];
            uk1 = rawDatas[2];
            shp2 = rawDatas[4];
            uk2 = rawDatas[6];
            this.id = id;
            this.name = name;
            this.desc = desc;
        }

        public new string ToString()
        {
            return string.Concat("Monster #", id, " - ", name, " - ", desc, " - ", shp1, " - ", shp2, " - ", uk1, " - ", uk2);
        }

        public string ToJSON()
        {
            return JsonUtility.ToJson(this);
        }
    }
}
