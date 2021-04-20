using System.Collections.Generic;
using UnityEngine;
using VS.Core;
using VS.Utils;

namespace VS.FileFormats.MPD
{
    public class MPDLoader : MonoBehaviour
    {
        public MPD SerializedMPD;
        public ZND.ZND SerializedZND;

        private void Start()
        {
            Build();
        }

        private void OnValidate()
        {
            Build();
        }

        private void Build()
        {
            ToolBox.DestroyChildren(gameObject, true);
            if (SerializedMPD != null)
            {
                List<Material> materials = new List<Material>();
                // we need the associated ZND to get textures
                string zndFileName = ToolBox.MPDToZND(string.Concat(SerializedMPD.Filename, ".MPD"), false);
                if (zndFileName != null)
                {
                    SerializedZND = Resources.Load<ZND.ZND>(string.Concat("Serialized/ZND/", zndFileName, ".yaml.asset"));
                    if (SerializedZND == null)
                    {
                        // Corresponding serialized ZND not found, so we try to serialize
                        VSPConfig conf = Memory.LoadConfig();
                        string zndFilePath = string.Concat(conf.VSPath, "MAP/", zndFileName);
                        SerializedZND = ScriptableObject.CreateInstance<ZND.ZND>();
                        SerializedZND.ParseFromFile(zndFilePath);

                        ToolBox.SaveScriptableObject("Assets/Resources/Serialized/ZND/", SerializedZND.Filename + ".ZND.yaml.asset", SerializedZND, SerializedZND.TIMs);
                    }
                }

                // we create all needed materials
                if (SerializedZND != null && SerializedZND.TIMs.Length > 0)
                {
                    Shader shader = Shader.Find("Particles/Standard Unlit");
                    for (uint i = 0; i < SerializedMPD.materialRefs.Length; i++)
                    {
                        Material mat = new Material(shader);
                        mat.name = SerializedMPD.materialRefs[i];
                        mat.SetTexture("_MainTex", SerializedZND.GetTexture(SerializedMPD.materialRefs[i]));
                        mat.SetFloat("_Mode", 0);
                        mat.SetFloat("_ColorMode", 0);
                        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                        mat.SetInt("_ZWrite", 1);
                        mat.EnableKeyword("_ALPHATEST_ON");
                        mat.DisableKeyword("_ALPHABLEND_ON");
                        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        materials.Add(mat);
                    }
                }

                for (uint i = 0; i < SerializedMPD.numGroups; i++)
                {
                    List<Vector3> meshVertices = new List<Vector3>();
                    List<List<int>> subMeshTriangles = new List<List<int>>();
                    List<Vector2> meshTrianglesUV = new List<Vector2>();
                    List<Vector3> meshNormals = new List<Vector3>();
                    List<Color32> meshColors = new List<Color32>();
                    List<Material> meshMaterials = new List<Material>();


                    int iv = 0;
                    int lmf = SerializedMPD.groups[i].faces.Length;
                    for (int k = 0; k < lmf; k++)
                    {
                        MPDFace f = SerializedMPD.groups[i].faces[k];
                        if (!meshMaterials.Exists(x => x.name.Equals(f.materialRef)))
                        {
                            meshMaterials.Add(materials.Find(x => x.name.Equals(f.materialRef)));
                            subMeshTriangles.Add(new List<int>());
                        }

                        int subMeshIndex = meshMaterials.IndexOf(meshMaterials.Find(x => x.name.Equals(f.materialRef)));

                        if (f.isQuad)
                        {
                            meshVertices.Add(-f.GetOpVertex(SerializedMPD.groups[i], 0) / 128);
                            meshVertices.Add(-f.GetOpVertex(SerializedMPD.groups[i], 1) / 128);
                            meshVertices.Add(-f.GetOpVertex(SerializedMPD.groups[i], 2) / 128);
                            meshVertices.Add(-f.GetOpVertex(SerializedMPD.groups[i], 3) / 128);
                            meshColors.Add(f.colors[0]);
                            meshColors.Add(f.colors[1]);
                            meshColors.Add(f.colors[2]);
                            meshColors.Add(f.colors[3]);
                            meshTrianglesUV.Add(f.uvs[1] / 256);
                            meshTrianglesUV.Add(f.uvs[2] / 256);
                            meshTrianglesUV.Add(f.uvs[0] / 256);
                            meshTrianglesUV.Add(f.uvs[3] / 256);

                            //meshNormals.Add(f.n);

                            subMeshTriangles[subMeshIndex].Add(iv + 0);
                            subMeshTriangles[subMeshIndex].Add(iv + 1);
                            subMeshTriangles[subMeshIndex].Add(iv + 2);

                            subMeshTriangles[subMeshIndex].Add(iv + 1);
                            subMeshTriangles[subMeshIndex].Add(iv + 3);
                            subMeshTriangles[subMeshIndex].Add(iv + 2);

                            iv += 4;
                        }
                        else
                        {
                            meshVertices.Add(-f.GetOpVertex(SerializedMPD.groups[i], 2) / 128);
                            meshVertices.Add(-f.GetOpVertex(SerializedMPD.groups[i], 1) / 128);
                            meshVertices.Add(-f.GetOpVertex(SerializedMPD.groups[i], 0) / 128);
                            meshColors.Add(f.colors[2]);
                            meshColors.Add(f.colors[1]);
                            meshColors.Add(f.colors[0]);
                            meshTrianglesUV.Add(f.uvs[0] / 256);
                            meshTrianglesUV.Add(f.uvs[2] / 256);
                            meshTrianglesUV.Add(f.uvs[1] / 256);

                            //meshNormals.Add(f.n);
                            //meshNormals.Add(f.n);
                            //meshNormals.Add(f.n);

                            subMeshTriangles[subMeshIndex].Add(iv + 2);
                            subMeshTriangles[subMeshIndex].Add(iv + 1);
                            subMeshTriangles[subMeshIndex].Add(iv + 0);
                            iv += 3;
                        }
                    }
                    GameObject groupGO = new GameObject("Group_" + i);
                    groupGO.transform.parent = gameObject.transform;

                    Mesh mesh = new Mesh();
                    mesh.name = "mesh_" + i;
                    mesh.vertices = meshVertices.ToArray();
                    //mesh.triangles = meshTriangles.ToArray();


                    mesh.subMeshCount = subMeshTriangles.Count;
                    for (int j = 0; j < subMeshTriangles.Count; j++)
                    {
                        mesh.SetTriangles(subMeshTriangles[j].ToArray(), j);
                    }

                    mesh.uv = meshTrianglesUV.ToArray();
                    //mesh.normals = meshNormals.ToArray();
                    mesh.colors32 = meshColors.ToArray();

                    MeshFilter mf = groupGO.AddComponent<MeshFilter>();
                    mf.mesh = mesh;

                    MeshRenderer mr = groupGO.AddComponent<MeshRenderer>();
                    mr.materials = meshMaterials.ToArray();
                }
            }
        }
    }
}
