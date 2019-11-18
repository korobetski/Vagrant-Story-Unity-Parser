using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VS.Data
{

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



        [SerializeField]
        private string _name = "";
        [SerializeField]
        private string _desc = "";
        [SerializeField]
        private uint _id = 0;
        [SerializeField]
        private byte _shp1 = 0;
        [SerializeField]
        private byte _shp2 = 0;

        public Monster(byte[] rawDatas, uint id, string name = "", string desc = "")
        {
            _shp1 = rawDatas[0];
            _shp2 = rawDatas[4];
            _id = id;
            _name = name;
            _desc = desc;
        }


        public string Name { get => _name; set => _name = value; }
        public string Desc { get => _desc; set => _desc = value; }
        public uint Id { get => _id; set => _id = value; }
        public byte Shp1 { get => _shp1; set => _shp1 = value; }
        public byte Shp2 { get => _shp2; set => _shp2 = value; }
        public string ToJSON()
        {
            return JsonUtility.ToJson(this);
        }
    }
}
