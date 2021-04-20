/*
using VS.Serializable;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using VS.Format;
using VS.Utils;

//http://datacrystal.romhacking.net/wiki/Vagrant_Story:WEP_files

namespace VS.Parser
{
    public class WEP : FileParser
    {
        public VS.Serializable.WEP so;
        public bool GOBuilded = false;
        public bool PrefabBuilded = false;
        public bool DrawPNG = false;
        public GameObject WEPGO;
        private int _idSmiMat = 1;
        public Mesh mesh;
        public Texture2D texture;
        public Material material;

        public int SmithMaterial
        {
            get => _idSmiMat; set
            {
                _idSmiMat = value;
                if (GOBuilded)
                {
                    MeshRenderer mr = WEPGO.GetComponent<MeshRenderer>();
                    if (mr)
                    {
                        float x = (_idSmiMat - 1 > 4) ? (float)0.5 : 0;
                        float y = (_idSmiMat - 1 % 2) * (float)0.25;
                        mr.sharedMaterial.SetTextureOffset("_MainTex", new Vector2(x, y));
                    }
                }
            }
        }

        public void Parse(string filePath)
        {
            PreParse(filePath);
            Parse(buffer);

            buffer.Close();
            fileStream.Close();
        }
        public void Parse(BinaryReader buffer)
        {
            this.buffer = buffer;

            so = ScriptableObject.CreateInstance<VS.Serializable.WEP>();
            so.WEPId = byte.Parse(FileName, System.Globalization.NumberStyles.HexNumber);
            so.Signature = buffer.ReadUInt32(); // signiture "H01"

            so.NumBones = buffer.ReadByte();
            so.NumGroups = buffer.ReadByte();
            so.NumTriangles = buffer.ReadUInt16();
            so.NumQuads = buffer.ReadUInt16();
            so.NumPolygons = buffer.ReadUInt16();

            long dec = buffer.BaseStream.Position + 4;
            so.PtrTexture = (uint)(buffer.ReadUInt32() + dec);

            so.Padding0x30 = buffer.ReadBytes(0x30); // padding

            so.PtrTextureSection = (uint)(buffer.ReadUInt32() + dec); // same as texturePtr1 in WEP but maybe not in ZUD
            so.PtrGroupSection = (uint)(buffer.ReadUInt32() + dec);
            so.PtrVertexSection = (uint)(buffer.ReadUInt32() + dec);
            so.PtrFaceSection = (uint)(buffer.ReadUInt32() + dec);

            // Bones section
            so.Bones = new VSBone[so.NumBones];
            for (uint i = 0; i < so.NumBones; i++)
            {
                VSBone bone = new VSBone();
                bone.index = i;
                //bone.name = "bone_" + i;
                // https://github.com/morris/vstools/blob/master/src/WEPBone.js
                bone.length = buffer.ReadInt32();
                bone.parentBoneId = buffer.ReadSByte();
                bone.groupId = buffer.ReadSByte();
                bone.mountId = buffer.ReadSByte();
                bone.bodyPartId = buffer.ReadSByte();
                bone.mode = buffer.ReadSByte();
                bone.unk = buffer.ReadBytes(7); // always 0000000
                if (bone.parentBoneId != -1 && bone.parentBoneId != 47)
                {
                    bone.SetParentBone(so.Bones[bone.parentBoneId]);
                }
                so.Bones[i] = bone;
            }

            // Group section
            if (buffer.BaseStream.Position != so.PtrGroupSection) buffer.BaseStream.Position = so.PtrGroupSection;
            so.Groups = new VSGroup[so.NumGroups];
            for (uint i = 0; i < so.NumGroups; i++)
            {
                VSGroup group = new VSGroup();
                group.boneIndex = buffer.ReadInt16();
                group.numVertices = buffer.ReadUInt16();
                // if (group.boneIndex != -1) group.bone = bones[group.boneIndex];

                so.Groups[i] = group;
            }

            // Vertices section
            if (buffer.BaseStream.Position != so.PtrVertexSection) buffer.BaseStream.Position = so.PtrVertexSection;

            uint numVertices = so.Groups[so.Groups.Length - 1].numVertices;
            so.Vertices = new VSVertex[numVertices];
            int g = 0;
            for (uint i = 0; i < numVertices; i++)
            {
                if (i >= so.Groups[g].numVertices)
                {
                    g++;
                }

                VSVertex vertex = new VSVertex();
                vertex.index = i;
                vertex.group = (byte)g;
                vertex.SetBone(so.Bones[so.Groups[g].boneIndex]);

                BoneWeight bw = new BoneWeight();
                bw.boneIndex0 = (int)so.Groups[g].boneIndex;
                bw.weight0 = 1;

                vertex.position = new Vector4(buffer.ReadInt16(), buffer.ReadInt16(), buffer.ReadInt16(), buffer.ReadInt16());
                vertex.SetBoneWeight(bw);

                so.Vertices[i] = vertex;
            }

            // Polygone section
            if (buffer.BaseStream.Position != so.PtrFaceSection)
            {
                buffer.BaseStream.Position = so.PtrFaceSection;
            }

            so.Faces = new VSFace[so.GetNumFaces()];
            for (uint i = 0; i < so.GetNumFaces(); i++)
            {
                VSFace face = new VSFace();
                face.type = buffer.ReadByte();
                face.size = buffer.ReadByte();
                face.side = buffer.ReadByte();
                face.alpha = buffer.ReadByte();
                face.verticesCount = 3;
                if (face.type == 36)
                {
                    face.verticesCount = 3;
                }
                else if (face.type == 44)
                {
                    face.verticesCount = 4;
                }

                face.vertices = new List<ushort>();
                for (uint j = 0; j < face.verticesCount; j++)
                {
                    ushort vId = (ushort)(buffer.ReadUInt16() / 4);
                    face.vertices.Add(vId);
                }
                face.uv = new List<Vector2>();
                for (uint j = 0; j < face.verticesCount; j++)
                {
                    int u = buffer.ReadByte();
                    int v = buffer.ReadByte();
                    face.uv.Add(new Vector2(u, v));
                }

                so.Faces[i] = face;
            }

            // Textures section
            if (buffer.BaseStream.Position != so.PtrTextureSection)
            {
                buffer.BaseStream.Position = so.PtrTextureSection;
            }

            TIM tim = new TIM();
            tim.FileName = FileName;
            tim.ParseWEP(buffer);
            texture = tim.DrawPack(true);

            so.TIM = tim;

            // rotations
            so.Footer = buffer.ReadBytes(24);

            Parsed = true;

            ItemList itemSO = Resources.Load("Serialized/Datas/ITEM.BIN.yaml") as ItemList;
            BladeWorkshop bladeSO = Resources.Load("Serialized/Datas/BLADE.SYD.yaml") as BladeWorkshop;
            ShieldWorkshop shieldSO = Resources.Load("Serialized/Datas/SHIELD.SYD.yaml") as ShieldWorkshop;
            if (itemSO != null && bladeSO != null)
            {
                if (so.WEPId < 91)
                {
                    foreach (Blade blade in bladeSO.Blades)
                    {
                        if (blade.wepId == so.WEPId)
                        {
                            so.Name = itemSO.Names[blade.id - 1];
                            so.Description = itemSO.Descriptions[blade.id - 1];
                            break;
                        }
                    }
                }
                else
                {
                    foreach (Shield shield in shieldSO.Shields)
                    {
                        if (shield.wepId == so.WEPId)
                        {
                            so.Name = itemSO.Names[shield.id + 120];
                            so.Description = "";
                            break;
                        }
                    }
                }
            }

            ToolBox.DirExNorCreate("Assets/");
            ToolBox.DirExNorCreate("Assets/Resources/");
            ToolBox.DirExNorCreate("Assets/Resources/Serialized/");
            ToolBox.DirExNorCreate("Assets/Resources/Serialized/Weapons/");
            AssetDatabase.DeleteAsset("Assets/Resources/Serialized/Weapons/" + FileName + ".WEP.yaml.asset");
            AssetDatabase.CreateAsset(so, "Assets/Resources/Serialized/Weapons/" + FileName + ".WEP.yaml.asset");
            AssetDatabase.SaveAssets();
        }
        public GameObject BuildGameObject()
        {
            Build(false);
            return WEPGO;
        }
        public void BuildPrefab(bool erase = false)
        {
            string wepFolder = "Assets/Resources/Prefabs/Weapons/";
            ToolBox.DirExNorCreate(wepFolder);
            string wepFilename = wepFolder + FileName + ".prefab";
            if (erase)
            {
                AssetDatabase.DeleteAsset(wepFilename);
            }

            Build(true);
        }

        public void Build(bool buildPrefab = false)
        {
            string wepFolder = "Assets/Resources/Prefabs/Weapons/";
            string wepFilename = wepFolder + FileName + ".prefab";
            GameObject prefab;
            // Building Model
            for (int i = 0; i < so.GetNumFaces(); i++)
            {
                for (int j = 0; j < so.Faces[i].verticesCount; j++)
                {
                    float u = so.Faces[i].uv[j].x / (so.TIM.width -1);
                    float v = so.Faces[i].uv[j].y / (so.TIM.height -1);
                    so.Faces[i].uv[j] = new Vector2(u, v);
                }
            }
            Mesh weaponMesh = new Mesh();
            List<Vector3> meshVertices = new List<Vector3>();
            List<int> meshTriangles = new List<int>();
            List<Vector2> meshTrianglesUV = new List<Vector2>();
            List<BoneWeight> meshWeights = new List<BoneWeight>();

            // Geometry
            for (int i = 0; i < so.Faces.Length; i++)
            {
                if (so.Faces[i].type == 0x2C)
                {
                    if (so.Faces[i].side != 4)
                    {
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(so.Vertices[so.Faces[i].vertices[2]].GetAbsPosition());
                        meshWeights.Add(so.Vertices[so.Faces[i].vertices[2]].GetBoneWeight());
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(so.Vertices[so.Faces[i].vertices[1]].GetAbsPosition());
                        meshWeights.Add(so.Vertices[so.Faces[i].vertices[1]].GetBoneWeight());
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(so.Vertices[so.Faces[i].vertices[0]].GetAbsPosition());
                        meshWeights.Add(so.Vertices[so.Faces[i].vertices[0]].GetBoneWeight());
                        meshTrianglesUV.Add(so.Faces[i].uv[2]);
                        meshTrianglesUV.Add(so.Faces[i].uv[1]);
                        meshTrianglesUV.Add(so.Faces[i].uv[0]);

                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(so.Vertices[so.Faces[i].vertices[1]].GetAbsPosition());
                        meshWeights.Add(so.Vertices[so.Faces[i].vertices[1]].GetBoneWeight());
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(so.Vertices[so.Faces[i].vertices[2]].GetAbsPosition());
                        meshWeights.Add(so.Vertices[so.Faces[i].vertices[2]].GetBoneWeight());
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(so.Vertices[so.Faces[i].vertices[3]].GetAbsPosition());
                        meshWeights.Add(so.Vertices[so.Faces[i].vertices[3]].GetBoneWeight());
                        meshTrianglesUV.Add(so.Faces[i].uv[1]);
                        meshTrianglesUV.Add(so.Faces[i].uv[2]);
                        meshTrianglesUV.Add(so.Faces[i].uv[3]);

                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(so.Vertices[so.Faces[i].vertices[0]].GetAbsPosition());
                        meshWeights.Add(so.Vertices[so.Faces[i].vertices[0]].GetBoneWeight());
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(so.Vertices[so.Faces[i].vertices[3]].GetAbsPosition());
                        meshWeights.Add(so.Vertices[so.Faces[i].vertices[3]].GetBoneWeight());
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(so.Vertices[so.Faces[i].vertices[2]].GetAbsPosition());
                        meshWeights.Add(so.Vertices[so.Faces[i].vertices[2]].GetBoneWeight());
                        meshTrianglesUV.Add(so.Faces[i].uv[0]);
                        meshTrianglesUV.Add(so.Faces[i].uv[3]);
                        meshTrianglesUV.Add(so.Faces[i].uv[2]);

                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(so.Vertices[so.Faces[i].vertices[0]].GetAbsPosition());
                        meshWeights.Add(so.Vertices[so.Faces[i].vertices[0]].GetBoneWeight());
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(so.Vertices[so.Faces[i].vertices[1]].GetAbsPosition());
                        meshWeights.Add(so.Vertices[so.Faces[i].vertices[1]].GetBoneWeight());
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(so.Vertices[so.Faces[i].vertices[3]].GetAbsPosition());
                        meshWeights.Add(so.Vertices[so.Faces[i].vertices[3]].GetBoneWeight());
                        meshTrianglesUV.Add(so.Faces[i].uv[0]);
                        meshTrianglesUV.Add(so.Faces[i].uv[1]);
                        meshTrianglesUV.Add(so.Faces[i].uv[3]);
                    }
                    else
                    {
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(so.Vertices[so.Faces[i].vertices[0]].GetAbsPosition());
                        meshWeights.Add(so.Vertices[so.Faces[i].vertices[0]].GetBoneWeight());
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(so.Vertices[so.Faces[i].vertices[1]].GetAbsPosition());
                        meshWeights.Add(so.Vertices[so.Faces[i].vertices[1]].GetBoneWeight());
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(so.Vertices[so.Faces[i].vertices[2]].GetAbsPosition());
                        meshWeights.Add(so.Vertices[so.Faces[i].vertices[2]].GetBoneWeight());
                        meshTrianglesUV.Add(so.Faces[i].uv[0]);
                        meshTrianglesUV.Add(so.Faces[i].uv[1]);
                        meshTrianglesUV.Add(so.Faces[i].uv[2]);

                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(so.Vertices[so.Faces[i].vertices[3]].GetAbsPosition());
                        meshWeights.Add(so.Vertices[so.Faces[i].vertices[3]].GetBoneWeight());
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(so.Vertices[so.Faces[i].vertices[2]].GetAbsPosition());
                        meshWeights.Add(so.Vertices[so.Faces[i].vertices[2]].GetBoneWeight());
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(so.Vertices[so.Faces[i].vertices[1]].GetAbsPosition());
                        meshWeights.Add(so.Vertices[so.Faces[i].vertices[1]].GetBoneWeight());
                        meshTrianglesUV.Add(so.Faces[i].uv[3]);
                        meshTrianglesUV.Add(so.Faces[i].uv[2]);
                        meshTrianglesUV.Add(so.Faces[i].uv[1]);
                    }
                }
                else if (so.Faces[i].type == 0x24)
                {
                    if (so.Faces[i].side != 4)
                    {
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(so.Vertices[so.Faces[i].vertices[0]].GetAbsPosition());
                        meshWeights.Add(so.Vertices[so.Faces[i].vertices[0]].GetBoneWeight());
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(so.Vertices[so.Faces[i].vertices[1]].GetAbsPosition());
                        meshWeights.Add(so.Vertices[so.Faces[i].vertices[1]].GetBoneWeight());
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(so.Vertices[so.Faces[i].vertices[2]].GetAbsPosition());
                        meshWeights.Add(so.Vertices[so.Faces[i].vertices[2]].GetBoneWeight());
                        meshTrianglesUV.Add(so.Faces[i].uv[1]);
                        meshTrianglesUV.Add(so.Faces[i].uv[2]);
                        meshTrianglesUV.Add(so.Faces[i].uv[0]);

                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(so.Vertices[so.Faces[i].vertices[2]].GetAbsPosition());
                        meshWeights.Add(so.Vertices[so.Faces[i].vertices[2]].GetBoneWeight());
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(so.Vertices[so.Faces[i].vertices[1]].GetAbsPosition());
                        meshWeights.Add(so.Vertices[so.Faces[i].vertices[1]].GetBoneWeight());
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(so.Vertices[so.Faces[i].vertices[0]].GetAbsPosition());
                        meshWeights.Add(so.Vertices[so.Faces[i].vertices[0]].GetBoneWeight());
                        meshTrianglesUV.Add(so.Faces[i].uv[0]);
                        meshTrianglesUV.Add(so.Faces[i].uv[1]);
                        meshTrianglesUV.Add(so.Faces[i].uv[2]);
                    }
                    else
                    {
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(so.Vertices[so.Faces[i].vertices[0]].GetAbsPosition());
                        meshWeights.Add(so.Vertices[so.Faces[i].vertices[0]].GetBoneWeight());
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(so.Vertices[so.Faces[i].vertices[1]].GetAbsPosition());
                        meshWeights.Add(so.Vertices[so.Faces[i].vertices[1]].GetBoneWeight());
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(so.Vertices[so.Faces[i].vertices[2]].GetAbsPosition());
                        meshWeights.Add(so.Vertices[so.Faces[i].vertices[2]].GetBoneWeight());
                        meshTrianglesUV.Add(so.Faces[i].uv[1]);
                        meshTrianglesUV.Add(so.Faces[i].uv[2]);
                        meshTrianglesUV.Add(so.Faces[i].uv[0]);
                    }
                }
            }

            for (int i = 0; i < meshVertices.Count; i++)
            {
                meshVertices[i] = -meshVertices[i] / 128;
            }

            // hard fixes for staves 39.WEP to 3F.WEP
            
            //List<string> staves = new List<string> { "39", "3A", "3B", "3C", "3D", "3E", "3F" };
            //if (staves.Contains(FileName))
            //{
            //    // its a staff, so we need to correct vertices of the first group
            //    Debug.Log("groups[0].bone.length : " + bones[groups[0].boneIndex].length / 128);
            //    for (int i = 0; i < groups[0].numVertices; i++)
            //    {
            //        vertices[i].position.x = (bones[groups[0].boneIndex].length * 2 - vertices[i].position.x * 128) / 128;
            //        vertices[i].position.y = -vertices[i].position.y;
            //    }
            //}

            weaponMesh.name = FileName + "_mesh";
            weaponMesh.vertices = meshVertices.ToArray();
            weaponMesh.triangles = meshTriangles.ToArray();
            weaponMesh.uv = meshTrianglesUV.ToArray();
            weaponMesh.boneWeights = meshWeights.ToArray();
            weaponMesh.Optimize();


            Shader shader = Shader.Find("Standard");
            Material mat = new Material(shader);
            mat.name = FileName + "_mat";
            mat.SetTexture("_MainTex", texture);
            mat.SetTextureScale("_MainTex", new Vector2(0.5f, -0.25f));
            float x = (_idSmiMat - 1 > 4) ? 0.5f : 0f;
            float y = (_idSmiMat - 1 % 2) * -0.25f;
            mat.SetTextureOffset("_MainTex", new Vector2(x, y));
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
            material = mat;

            GameObject weaponGo = new GameObject(FileName);
            weaponGo.tag = "Weapon";


            WEPGO = weaponGo;
            GOBuilded = true;


#if UNITY_EDITOR
            if (buildPrefab)
            {
                ToolBox.DirExNorCreate(wepFolder);
                prefab = PrefabUtility.SaveAsPrefabAsset(weaponGo, wepFilename);
                GameObject.DestroyImmediate(weaponGo.GetComponent<MeshRenderer>());
                foreach (Transform child in weaponGo.transform)
                {
                    GameObject.DestroyImmediate(child.gameObject);
                }
                prefab = PrefabUtility.SaveAsPrefabAsset(weaponGo, wepFilename);
                WEPGO = null;
                GOBuilded = false;
                GameObject.DestroyImmediate(weaponGo);
                AssetDatabase.AddObjectToAsset(weaponMesh, wepFilename);
                AssetDatabase.AddObjectToAsset(texture, wepFilename);
                AssetDatabase.AddObjectToAsset(material, wepFilename);
                prefab.AddComponent<MeshFilter>().mesh = weaponMesh;
                prefab.AddComponent<MeshRenderer>().material = material;
                AssetDatabase.SaveAssets();
            }
#endif

        }
    }
}
*/