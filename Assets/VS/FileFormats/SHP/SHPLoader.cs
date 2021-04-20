using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VS.FileFormats.GEOM;
using VS.Utils;

namespace VS.FileFormats.SHP
{
    [RequireComponent(typeof(MeshFilter), typeof(SkinnedMeshRenderer))]
    public class SHPLoader : MonoBehaviour
    {
        public SHP SerializedSHP;
        public SEQ.SEQ SerializedSEQ;

        private MeshFilter _mf;
        private SkinnedMeshRenderer _mr;

        private void Start()
        {
            _mf = GetComponent<MeshFilter>();
            _mr = GetComponent<SkinnedMeshRenderer>();
            Build();
        }

        private void OnValidate()
        {
            Build();
        }


        private void Build()
        {
            ToolBox.DestroyChildren(gameObject, true);
            _mf = GetComponent<MeshFilter>();
            _mr = GetComponent<SkinnedMeshRenderer>();

            if (SerializedSHP != null)
            {
                // Building Model
                for (int i = 0; i < SerializedSHP.numFaces; i++)
                {
                    for (int j = 0; j < SerializedSHP.faces[i].verticesCount; j++)
                    {
                        if (SerializedSHP.TIM != null)
                        {
                            float u = SerializedSHP.faces[i].uv[j].x / (SerializedSHP.TIM.width - 1);
                            float v = SerializedSHP.faces[i].uv[j].y / (SerializedSHP.TIM.height - 1);
                            SerializedSHP.faces[i].uv[j] = (new Vector2(u, v));
                        }
                        else
                        {
                            SerializedSHP.faces[i].uv[j] = Vector2.zero;
                        }
                    }
                }

                foreach (Bone bone in SerializedSHP.bones)
                {
                    if (bone.parentBoneId != -1 && bone.parentBoneId != 47) bone.SetParentBone(SerializedSHP.bones[bone.parentBoneId]);
                }

                foreach (Group group in SerializedSHP.groups)
                {
                    group.bone = SerializedSHP.bones[group.boneIndex];
                }

                foreach (Vertex vertex in SerializedSHP.vertices)
                {
                    vertex.bone = SerializedSHP.bones[SerializedSHP.groups[vertex.group].boneIndex];
                    BoneWeight bw = new BoneWeight
                    {
                        boneIndex0 = (int)vertex.bone.index,
                        weight0 = 1
                    };
                    vertex.boneWeight = bw;
                }

                Mesh shapeMesh = new Mesh();
                List<Vector3> meshVertices = new List<Vector3>();
                List<int> meshTriangles = new List<int>();
                List<Vector2> meshTrianglesUV = new List<Vector2>();
                List<BoneWeight> meshWeights = new List<BoneWeight>();
                List<Color32> meshColors = new List<Color32>();

                for (int i = 0; i < SerializedSHP.faces.Length; i++)
                {
                    if (SerializedSHP.faces[i].type == 0x2C || SerializedSHP.faces[i].type == 0x3C)
                    {
                        if (SerializedSHP.faces[i].side != 4)
                        {
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[0]].position);
                            meshWeights.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[0]].boneWeight);
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[1]].position);
                            meshWeights.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[1]].boneWeight);
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[2]].position);
                            meshWeights.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[2]].boneWeight);
                            meshTrianglesUV.Add(SerializedSHP.faces[i].uv[0]);
                            meshTrianglesUV.Add(SerializedSHP.faces[i].uv[1]);
                            meshTrianglesUV.Add(SerializedSHP.faces[i].uv[2]);

                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[3]].position);
                            meshWeights.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[3]].boneWeight);
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[2]].position);
                            meshWeights.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[2]].boneWeight);
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[1]].position);
                            meshWeights.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[1]].boneWeight);
                            meshTrianglesUV.Add(SerializedSHP.faces[i].uv[3]);
                            meshTrianglesUV.Add(SerializedSHP.faces[i].uv[2]);
                            meshTrianglesUV.Add(SerializedSHP.faces[i].uv[1]);

                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[2]].position);
                            meshWeights.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[2]].boneWeight);
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[1]].position);
                            meshWeights.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[1]].boneWeight);
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[0]].position);
                            meshWeights.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[0]].boneWeight);
                            meshTrianglesUV.Add(SerializedSHP.faces[i].uv[2]);
                            meshTrianglesUV.Add(SerializedSHP.faces[i].uv[1]);
                            meshTrianglesUV.Add(SerializedSHP.faces[i].uv[0]);

                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[1]].position);
                            meshWeights.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[1]].boneWeight);
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[2]].position);
                            meshWeights.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[2]].boneWeight);
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[3]].position);
                            meshWeights.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[3]].boneWeight);
                            meshTrianglesUV.Add(SerializedSHP.faces[i].uv[1]);
                            meshTrianglesUV.Add(SerializedSHP.faces[i].uv[2]);
                            meshTrianglesUV.Add(SerializedSHP.faces[i].uv[3]);

                            if (SerializedSHP.hasColoredVertices)
                            {
                                // we have colored vertices
                                meshColors.Add(SerializedSHP.faces[i].colors[0]);
                                meshColors.Add(SerializedSHP.faces[i].colors[1]);
                                meshColors.Add(SerializedSHP.faces[i].colors[2]);

                                meshColors.Add(SerializedSHP.faces[i].colors[3]);
                                meshColors.Add(SerializedSHP.faces[i].colors[2]);
                                meshColors.Add(SerializedSHP.faces[i].colors[1]);

                                meshColors.Add(SerializedSHP.faces[i].colors[2]);
                                meshColors.Add(SerializedSHP.faces[i].colors[1]);
                                meshColors.Add(SerializedSHP.faces[i].colors[0]);

                                meshColors.Add(SerializedSHP.faces[i].colors[1]);
                                meshColors.Add(SerializedSHP.faces[i].colors[2]);
                                meshColors.Add(SerializedSHP.faces[i].colors[3]);
                            }
                        }
                        else
                        {
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[0]].position);
                            meshWeights.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[0]].boneWeight);
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[1]].position);
                            meshWeights.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[1]].boneWeight);
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[2]].position);
                            meshWeights.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[2]].boneWeight);
                            meshTrianglesUV.Add(SerializedSHP.faces[i].uv[0]);
                            meshTrianglesUV.Add(SerializedSHP.faces[i].uv[1]);
                            meshTrianglesUV.Add(SerializedSHP.faces[i].uv[2]);

                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[3]].position);
                            meshWeights.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[3]].boneWeight);
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[2]].position);
                            meshWeights.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[2]].boneWeight);
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[1]].position);
                            meshWeights.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[1]].boneWeight);
                            meshTrianglesUV.Add(SerializedSHP.faces[i].uv[3]);
                            meshTrianglesUV.Add(SerializedSHP.faces[i].uv[2]);
                            meshTrianglesUV.Add(SerializedSHP.faces[i].uv[1]);
                            if (SerializedSHP.hasColoredVertices)
                            {
                                // we have colored vertices
                                meshColors.Add(SerializedSHP.faces[i].colors[0]);
                                meshColors.Add(SerializedSHP.faces[i].colors[1]);
                                meshColors.Add(SerializedSHP.faces[i].colors[2]);

                                meshColors.Add(SerializedSHP.faces[i].colors[3]);
                                meshColors.Add(SerializedSHP.faces[i].colors[2]);
                                meshColors.Add(SerializedSHP.faces[i].colors[1]);
                            }
                        }
                    }
                    else if (SerializedSHP.faces[i].type == 0x24 || SerializedSHP.faces[i].type == 0x34)
                    {
                        if (SerializedSHP.faces[i].side != 4)
                        {
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[0]].position);
                            meshWeights.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[0]].boneWeight);
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[1]].position);
                            meshWeights.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[1]].boneWeight);
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[2]].position);
                            meshWeights.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[2]].boneWeight);
                            meshTrianglesUV.Add(SerializedSHP.faces[i].uv[1]);
                            meshTrianglesUV.Add(SerializedSHP.faces[i].uv[2]);
                            meshTrianglesUV.Add(SerializedSHP.faces[i].uv[0]);

                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[2]].position);
                            meshWeights.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[2]].boneWeight);
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[1]].position);
                            meshWeights.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[1]].boneWeight);
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[0]].position);
                            meshWeights.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[0]].boneWeight);
                            meshTrianglesUV.Add(SerializedSHP.faces[i].uv[0]);
                            meshTrianglesUV.Add(SerializedSHP.faces[i].uv[2]);
                            meshTrianglesUV.Add(SerializedSHP.faces[i].uv[1]);
                            if (SerializedSHP.hasColoredVertices)
                            {
                                // we have colored vertices
                                meshColors.Add(SerializedSHP.faces[i].colors[0]);
                                meshColors.Add(SerializedSHP.faces[i].colors[1]);
                                meshColors.Add(SerializedSHP.faces[i].colors[2]);

                                meshColors.Add(SerializedSHP.faces[i].colors[2]);
                                meshColors.Add(SerializedSHP.faces[i].colors[1]);
                                meshColors.Add(SerializedSHP.faces[i].colors[0]);
                            }
                        }
                        else
                        {
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[0]].position);
                            meshWeights.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[0]].boneWeight);
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[1]].position);
                            meshWeights.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[1]].boneWeight);
                            meshTriangles.Add(meshVertices.Count);
                            meshVertices.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[2]].position);
                            meshWeights.Add(SerializedSHP.vertices[SerializedSHP.faces[i].vertices[2]].boneWeight);
                            meshTrianglesUV.Add(SerializedSHP.faces[i].uv[1]);
                            meshTrianglesUV.Add(SerializedSHP.faces[i].uv[2]);
                            meshTrianglesUV.Add(SerializedSHP.faces[i].uv[0]);
                            if (SerializedSHP.hasColoredVertices)
                            {
                                // we have colored vertices
                                meshColors.Add(SerializedSHP.faces[i].colors[0]);
                                meshColors.Add(SerializedSHP.faces[i].colors[1]);
                                meshColors.Add(SerializedSHP.faces[i].colors[2]);
                            }
                        }
                    }
                }
                shapeMesh.name = SerializedSHP.Filename + "_mesh";
                shapeMesh.vertices = meshVertices.ToArray();
                shapeMesh.triangles = meshTriangles.ToArray();
                shapeMesh.uv = meshTrianglesUV.ToArray();
                shapeMesh.boneWeights = meshWeights.ToArray();
                if (SerializedSHP.hasColoredVertices)
                {
                    shapeMesh.colors32 = meshColors.ToArray();
                }

                Texture2D texture;
                if (SerializedSHP.TIM != null)
                {
                    texture = SerializedSHP.TIM.GetTexture(0);
                }
                else
                {
                    texture = new Texture2D(128, 128);
                }

                Material mat = null;
                if (SerializedSHP.hasColoredVertices)
                {
                    Shader shader = Shader.Find("Particles/Standard Unlit");
                    mat = new Material(shader);
                    mat.name = string.Concat("Material_SHP_", SerializedSHP.Filename);
                    mat.SetTexture("_MainTex", texture);
                    mat.SetFloat("_Mode", 0);
                    mat.SetFloat("_ColorMode", 0);
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    mat.SetInt("_ZWrite", 1);
                    mat.EnableKeyword("_ALPHATEST_ON");
                    mat.DisableKeyword("_ALPHABLEND_ON");
                    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    mat.SetTextureScale("_MainTex", new Vector2(1, -1));

                }
                else
                {
                    Shader shader = Shader.Find("Standard");
                    mat = new Material(shader);
                    mat.name = string.Concat("Material_SHP_", SerializedSHP.Filename);
                    mat.SetTexture("_MainTex", texture);
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
                    mat.SetTextureScale("_MainTex", new Vector2(1, -1));

                }


                Transform[] meshBones = new Transform[SerializedSHP.numBones];
                Matrix4x4[] bindPoses = new Matrix4x4[SerializedSHP.numBones];
                for (int i = 0; i < SerializedSHP.numBones; i++)
                {
                    meshBones[i] = new GameObject("bone_" + i).transform;
                    meshBones[i].localRotation = Quaternion.identity;
                    bindPoses[i] = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
                    if (SerializedSHP.bones[i].parentBoneId == -1)
                    {
                        meshBones[i].parent = gameObject.transform;
                        meshBones[i].localPosition = Vector3.zero;
                        if (SerializedSEQ != null)
                        {
                            meshBones[i].localRotation = SerializedSEQ.animations[0].GetFirstBoneRotation(i);
                        }
                    }
                    else
                    {
                        meshBones[i].parent = meshBones[SerializedSHP.bones[i].parentBoneId];
                        meshBones[i].localPosition = new Vector3((float)(SerializedSHP.bones[SerializedSHP.bones[i].parentBoneId].length) / 128, 0, 0);
                        if (SerializedSEQ != null)
                        {
                            meshBones[i].localRotation = SerializedSEQ.animations[0].GetFirstBoneRotation(i);
                        }
                    }
                }

                shapeMesh.bindposes = bindPoses;

                _mf.mesh = shapeMesh;
                _mr.material = mat;
                _mr.bones = meshBones;
                _mr.quality = SkinQuality.Bone1;
                _mr.rootBone = meshBones[0];
                _mr.sharedMesh = shapeMesh;
            }
        }
    }
}
