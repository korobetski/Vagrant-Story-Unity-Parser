using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VS.Utils;

namespace VS.Format
{
    // https://fr.wikipedia.org/wiki/Material_Template_Library
    public class MTL
    {
        internal Vector3 offset = new Vector3(0, 0, 0);
        internal Vector3 scale = new Vector3(1, 1, 1);
        private string _path;
        private string _name;
        private string _texturePath;


        public MTL(string texturePath, string path, string name)
        {
            _path = path;
            _name = name;
            _texturePath = texturePath;
        }

        public string name { get => _name; set => _name = value; }
        public string path { get => _path; set => _path = value; }

        public void Write()
        {

            List<string> content = new List<string>();
            content.Add(string.Concat("newmtl ", _name));
            content.Add(string.Concat("Ka 1.0 1.0 1.0"));
            content.Add(string.Concat("Kd 1.0 1.0 1.0"));
            content.Add(string.Concat("Ks 0.2 0.2 0.2"));
            content.Add(string.Concat("Tr 1.0"));
            content.Add(string.Concat("illum 2"));
            content.Add(string.Concat("Ns 0.0"));
            content.Add(string.Concat("map_Kd",
                " -s ", scale.x, " ", scale.y, " ", scale.z,
                " -o ", offset.x, " ", offset.y, " ", offset.z,
                " ", _texturePath
                ));
            for (int i = 0; i < content.Count; i++)
            {
                content[i] = content[i].Replace(",", ".");
            }
            ToolBox.DirExNorCreate("Assets/Resources/Obj/");
            File.WriteAllLines("Assets/Resources/Obj/" + _path, content);
        }
    }

}