using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VS.Utils;

namespace FileFormats
{
    public class OBJ
    {
        private string _name;
        private List<Vector3> _v;
        private List<Vector2> _vt;
        private List<Vector3> _vn;
        private List<Face> _f;
        private MTL _mtl;



        public OBJ(Mesh mesh, string name = "3D obj", MTL mtl = null)
        {
            _name = name;
            _v = new List<Vector3>(mesh.vertices);
            _vt = new List<Vector2>(mesh.uv);
            _vn = new List<Vector3>(mesh.normals);
            _f = new List<Face>();
            _mtl = mtl;

            for (int i = 0; i < mesh.vertices.Length / 3; i++)
            {
                Vector3Int v1 = new Vector3Int(1 + i * 3, 1 + i * 3, 1 + i * 3);
                Vector3Int v2 = new Vector3Int(1 + i * 3 + 1, 1 + i * 3 + 1, 1 + i * 3 + 1);
                Vector3Int v3 = new Vector3Int(1 + i * 3 + 2, 1 + i * 3 + 2, 1 + i * 3 + 2);

                Face f = new Face(v1, v2, v3);
                _f.Add(f);
            }

        }

        public void Write()
        {
            List<string> content = new List<string>();
            content.Add(string.Concat("o ", _name));
            content.Add(string.Concat("mtllib ", _mtl.path));
            for (int i = 0; i < _v.Count; i++)
            {
                content.Add(string.Concat("v ", _v[i].x, " ", _v[i].y, " ", _v[i].z));
            }
            for (int i = 0; i < _vt.Count; i++)
            {
                content.Add(string.Concat("vt ", _vt[i].x, " ", _vt[i].y));
            }
            for (int i = 0; i < _vn.Count; i++)
            {
                content.Add(string.Concat("vn ", _vn[i].x, " ", _vn[i].y, " ", _vn[i].z));
            }
            content.Add(string.Concat("usemtl ", _mtl.name));
            for (int i = 0; i < _f.Count; i++)
            {
                content.Add(string.Concat("f ", _f[i].v1, "/", _f[i].vt1, "/", _f[i].vn1, " ", _f[i].v2, "/", _f[i].vt2, "/", _f[i].vn2, " ", _f[i].v3, "/", _f[i].vt3, "/", _f[i].vn3));
            }

            for (int i = 0; i < content.Count; i++)
            {
                content[i] = content[i].Replace(",", ".");
            }

            ToolBox.DirExNorCreate("Assets/Resources/Obj/");
            File.WriteAllLines("Assets/Resources/Obj/" + _name + ".obj", content);

        }



        private class Face
        {
            private Vector3Int[] _vi;

            public Face(Vector3Int v1, Vector3Int v2, Vector3Int v3)
            {
                _vi = new Vector3Int[3];
                _vi[0] = v1;
                _vi[1] = v2;
                _vi[2] = v3;
            }

            public int v1
            {
                get => _vi[0].x;
            }
            public int vt1
            {
                get => _vi[0].y;
            }
            public int vn1
            {
                get => _vi[0].z;
            }

            public int v2
            {
                get => _vi[1].x;
            }
            public int vt2
            {
                get => _vi[1].y;
            }
            public int vn2
            {
                get => _vi[1].z;
            }

            public int v3
            {
                get => _vi[2].x;
            }
            public int vt3
            {
                get => _vi[2].y;
            }
            public int vn3
            {
                get => _vi[2].z;
            }
        }
    }


}
