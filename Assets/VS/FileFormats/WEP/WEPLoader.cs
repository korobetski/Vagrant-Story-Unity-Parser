using System.Collections.Generic;
using UnityEngine;
using VS.FileFormats.GEOM;
using VS.Utils;

namespace VS.FileFormats.WEP
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class WEPLoader:MonoBehaviour
    {
        public WEP SerializedWEP;

        private MeshFilter _mf;
        private MeshRenderer _mr;

        private void Start()
        {
            _mf = GetComponent<MeshFilter>();
            _mr = GetComponent<MeshRenderer>();
            Build();
        }

        private void OnValidate()
        {
            Build();
        }

        private void Build()
        {
            _mf = GetComponent<MeshFilter>();
            _mr = GetComponent<MeshRenderer>();

            if (SerializedWEP != null)
            {
                Mesh weaponMesh = new Mesh();
                List<Vector3> meshVertices = new List<Vector3>();
                List<int> meshTriangles = new List<int>();
                List<Vector2> meshTrianglesUV = new List<Vector2>();
                List<BoneWeight> meshWeights = new List<BoneWeight>();

                for (int i = 0; i < SerializedWEP.NumFaces; i++)
                {
                    for (int j = 0; j < SerializedWEP.Faces[i].verticesCount; j++)
                    {
                        float u = SerializedWEP.Faces[i].uv[j].x / (SerializedWEP.TIM.width - 1);
                        float v = SerializedWEP.Faces[i].uv[j].y / (SerializedWEP.TIM.height - 1);
                        SerializedWEP.Faces[i].uv[j] = new Vector2(u, v);
                    }
                }

                foreach (Bone bone in SerializedWEP.Bones)
                {
                    if (bone.parentBoneId != -1 && bone.parentBoneId != 47) bone.SetParentBone(SerializedWEP.Bones[bone.parentBoneId]);
                }

                foreach (Group group in SerializedWEP.Groups)
                {
                    group.bone = SerializedWEP.Bones[group.boneIndex];
                }

                foreach (Vertex vertex in SerializedWEP.Vertices)
                {
                    vertex.bone = SerializedWEP.Bones[SerializedWEP.Groups[vertex.group].boneIndex];
                }

                // hard fixes for staves 39.WEP to 3F.WEP
                List<string> staves = new List<string> { "39", "3A", "3B", "3C", "3D", "3E", "3F" };
                if (staves.Contains(SerializedWEP.Filename))
                {
                    // its a staff, SerializedWEP.we need to correct vertices of the first group
                    for (int i = 0; i < SerializedWEP.Groups[0].numVertices; i++)
                    {
                        SerializedWEP.Vertices[i].position.x = (-SerializedWEP.Groups[0].bone.length * 2 - SerializedWEP.Vertices[i].position.x);
                        SerializedWEP.Vertices[i].position.y = -SerializedWEP.Vertices[i].position.y;
                    }
                }

                // Geometry
                for (int i = 0; i < SerializedWEP.Faces.Length; i++)
                {
                    if (SerializedWEP.Faces[i].type == 0x2C)
                    {
                        if (SerializedWEP.Faces[i].side != 4)
                        {
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[2]].GetAbsPosition());
                            meshWeights.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[2]].boneWeight);
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[1]].GetAbsPosition());
                            meshWeights.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[1]].boneWeight);
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[0]].GetAbsPosition());
                            meshWeights.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[0]].boneWeight);
                            meshTrianglesUV.Add(SerializedWEP.Faces[i].uv[2]);
                            meshTrianglesUV.Add(SerializedWEP.Faces[i].uv[1]);
                            meshTrianglesUV.Add(SerializedWEP.Faces[i].uv[0]);

                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[1]].GetAbsPosition());
                            meshWeights.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[1]].boneWeight);
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[2]].GetAbsPosition());
                            meshWeights.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[2]].boneWeight);
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[3]].GetAbsPosition());
                            meshWeights.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[3]].boneWeight);
                            meshTrianglesUV.Add(SerializedWEP.Faces[i].uv[1]);
                            meshTrianglesUV.Add(SerializedWEP.Faces[i].uv[2]);
                            meshTrianglesUV.Add(SerializedWEP.Faces[i].uv[3]);

                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[0]].GetAbsPosition());
                            meshWeights.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[0]].boneWeight);
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[3]].GetAbsPosition());
                            meshWeights.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[3]].boneWeight);
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[2]].GetAbsPosition());
                            meshWeights.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[2]].boneWeight);
                            meshTrianglesUV.Add(SerializedWEP.Faces[i].uv[0]);
                            meshTrianglesUV.Add(SerializedWEP.Faces[i].uv[3]);
                            meshTrianglesUV.Add(SerializedWEP.Faces[i].uv[2]);

                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[0]].GetAbsPosition());
                            meshWeights.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[0]].boneWeight);
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[1]].GetAbsPosition());
                            meshWeights.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[1]].boneWeight);
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[3]].GetAbsPosition());
                            meshWeights.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[3]].boneWeight);
                            meshTrianglesUV.Add(SerializedWEP.Faces[i].uv[0]);
                            meshTrianglesUV.Add(SerializedWEP.Faces[i].uv[1]);
                            meshTrianglesUV.Add(SerializedWEP.Faces[i].uv[3]);
                        }
                        else
                        {
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[0]].GetAbsPosition());
                            meshWeights.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[0]].boneWeight);
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[1]].GetAbsPosition());
                            meshWeights.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[1]].boneWeight);
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[2]].GetAbsPosition());
                            meshWeights.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[2]].boneWeight);
                            meshTrianglesUV.Add(SerializedWEP.Faces[i].uv[0]);
                            meshTrianglesUV.Add(SerializedWEP.Faces[i].uv[1]);
                            meshTrianglesUV.Add(SerializedWEP.Faces[i].uv[2]);

                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[3]].GetAbsPosition());
                            meshWeights.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[3]].boneWeight);
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[2]].GetAbsPosition());
                            meshWeights.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[2]].boneWeight);
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[1]].GetAbsPosition());
                            meshWeights.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[1]].boneWeight);
                            meshTrianglesUV.Add(SerializedWEP.Faces[i].uv[3]);
                            meshTrianglesUV.Add(SerializedWEP.Faces[i].uv[2]);
                            meshTrianglesUV.Add(SerializedWEP.Faces[i].uv[1]);
                        }
                    }
                    else if (SerializedWEP.Faces[i].type == 0x24)
                    {
                        if (SerializedWEP.Faces[i].side != 4)
                        {
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[0]].GetAbsPosition());
                            meshWeights.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[0]].boneWeight);
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[1]].GetAbsPosition());
                            meshWeights.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[1]].boneWeight);
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[2]].GetAbsPosition());
                            meshWeights.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[2]].boneWeight);
                            meshTrianglesUV.Add(SerializedWEP.Faces[i].uv[1]);
                            meshTrianglesUV.Add(SerializedWEP.Faces[i].uv[2]);
                            meshTrianglesUV.Add(SerializedWEP.Faces[i].uv[0]);

                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[2]].GetAbsPosition());
                            meshWeights.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[2]].boneWeight);
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[1]].GetAbsPosition());
                            meshWeights.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[1]].boneWeight);
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[0]].GetAbsPosition());
                            meshWeights.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[0]].boneWeight);
                            meshTrianglesUV.Add(SerializedWEP.Faces[i].uv[0]);
                            meshTrianglesUV.Add(SerializedWEP.Faces[i].uv[1]);
                            meshTrianglesUV.Add(SerializedWEP.Faces[i].uv[2]);
                        }
                        else
                        {
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[0]].GetAbsPosition());
                            meshWeights.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[0]].boneWeight);
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[1]].GetAbsPosition());
                            meshWeights.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[1]].boneWeight);
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[2]].GetAbsPosition());
                            meshWeights.Add(SerializedWEP.Vertices[SerializedWEP.Faces[i].vertices[2]].boneWeight);
                            meshTrianglesUV.Add(SerializedWEP.Faces[i].uv[1]);
                            meshTrianglesUV.Add(SerializedWEP.Faces[i].uv[2]);
                            meshTrianglesUV.Add(SerializedWEP.Faces[i].uv[0]);
                        }
                    }
                }

                for (int i = 0; i < meshVertices.Count; i++)
                {
                    meshVertices[i] = -meshVertices[i] / 128;
                }

                weaponMesh.name = SerializedWEP.Filename + "_mesh";
                weaponMesh.vertices = meshVertices.ToArray();
                weaponMesh.triangles = meshTriangles.ToArray();
                weaponMesh.uv = meshTrianglesUV.ToArray();
                weaponMesh.boneWeights = meshWeights.ToArray();
                weaponMesh.Optimize();


                Shader shader = Shader.Find("Standard");
                Material mat = new Material(shader);
                mat.name = SerializedWEP.Filename + "_mat";
                mat.SetTexture("_MainTex", SerializedWEP.TIM.GetTexture(3));
                mat.SetTextureScale("_MainTex", new Vector2(1, -1));
                mat.SetTextureOffset("_MainTex", new Vector2(0, 0));
                mat.SetFloat("_Mode", 1);
                mat.SetFloat("_Cutoff", 0.5f);
                mat.SetFloat("_Glossiness", 0.0f);
                mat.SetFloat("_SpecularHighlights", 0.0f);
                mat.SetFloat("_GlossyReflections", 0.0f);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                mat.SetInt("_ZWrite", 1);
                mat.EnableKeyword("_ALPHATEST_ON");
                mat.DisableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 2450;

                _mf.mesh = weaponMesh;
                _mr.material = mat;
            }
        }
    }
}
