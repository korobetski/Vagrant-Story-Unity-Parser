using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using VS.Entity;
using VS.Format;
using VS.Utils;

//http://datacrystal.romhacking.net/wiki/Vagrant_Story:WEP_files

namespace VS.Parser
{
    public class WEP : FileParser
    {
        public bool GOBuilded = false;
        public bool PrefabBuilded = false;
        public bool DrawPNG = false;
        public GameObject WEPGO;

        private uint numBones;
        private uint numGroups;
        private uint numTri;
        private uint numQuad;
        private uint numFace;
        private uint numPoly;
        private List<VSBone> bones;
        private List<VSGroup> groups;
        private List<VSVertex> vertices;
        private List<VSFace> faces;
        private TIM tim;
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
            char[] head = buffer.ReadChars(4); // signiture "H01"
            numBones = buffer.ReadByte();
            numGroups = buffer.ReadByte();
            numTri = buffer.ReadUInt16();
            numQuad = buffer.ReadUInt16();
            numFace = buffer.ReadUInt16();
            numPoly = numTri + numQuad + numFace;

            long dec = buffer.BaseStream.Position + 4;
            long texturePtr1 = buffer.ReadUInt32() + dec;
            buffer.ReadBytes(0x30); // padding

            long texturePtr = buffer.ReadUInt32() + dec;
            long groupPtr = buffer.ReadUInt32() + dec;
            long vertexPtr = buffer.ReadUInt32() + dec;
            long polygonPtr = buffer.ReadUInt32() + dec;

            // Bones section
            bones = new List<VSBone>();
            for (uint i = 0; i < numBones; i++)
            {
                VSBone bone = new VSBone();
                bone.index = i;
                bone.name = "bone_" + i;
                bone.length = -buffer.ReadInt16();
                buffer.ReadUInt16(); // always 0xFFFF
                bone.parentIndex = buffer.ReadByte();
                byte[] offset = buffer.ReadBytes(3);
                bone.offset = new Vector3(offset[0], offset[1], offset[2]);
                bone.mode = buffer.ReadByte();
                buffer.ReadBytes(7); // always 0000000
                bones.Add(bone);
            }

            // Group section
            if (buffer.BaseStream.Position != groupPtr)
            {
                buffer.BaseStream.Position = groupPtr;
            }

            groups = new List<VSGroup>();
            for (uint i = 0; i < numGroups; i++)
            {
                VSGroup group = new VSGroup();
                group.boneIndex = buffer.ReadInt16();
                group.numVertices = buffer.ReadUInt16();
                if (group.boneIndex != -1)
                {
                    group.bone = bones[group.boneIndex];
                }

                groups.Add(group);
            }

            // Vertices section
            if (buffer.BaseStream.Position != vertexPtr)
            {
                buffer.BaseStream.Position = vertexPtr;
            }

            vertices = new List<VSVertex>();
            uint numVertices = groups[groups.Count - 1].numVertices;
            int g = 0;
            for (uint i = 0; i < numVertices; i++)
            {
                if (i >= groups[g].numVertices)
                {
                    g++;
                }

                VSVertex vertex = new VSVertex();
                vertex.group = groups[g];
                vertex.bone = vertex.group.bone;
                BoneWeight bw = new BoneWeight();
                bw.boneIndex0 = (int)vertex.group.bone.index;
                bw.weight0 = 1;
                vertex.boneWeight = bw;
                int x = buffer.ReadInt16();
                int y = buffer.ReadInt16();
                int z = buffer.ReadInt16();
                buffer.ReadInt16();
                vertex.position = new Vector3(x, y, z) / 100;
                vertices.Add(vertex);
            }

            // Polygone section
            if (buffer.BaseStream.Position != polygonPtr)
            {
                buffer.BaseStream.Position = polygonPtr;
            }

            faces = new List<VSFace>();
            for (uint i = 0; i < numPoly; i++)
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

                face.vertices = new List<int>();
                for (uint j = 0; j < face.verticesCount; j++)
                {
                    int vId = buffer.ReadUInt16() / 4;
                    face.vertices.Add(vId);
                }
                face.uv = new List<Vector2>();
                for (uint j = 0; j < face.verticesCount; j++)
                {
                    int u = buffer.ReadByte();
                    int v = buffer.ReadByte();
                    face.uv.Add(new Vector2(u, v));
                }
                faces.Add(face);
            }


            // Textures section
            if (buffer.BaseStream.Position != texturePtr)
            {
                buffer.BaseStream.Position = texturePtr;
            }

            tim = new TIM();
            tim.FileName = FileName;
            tim.ParseWEP(buffer);
            texture = tim.DrawPack(true);

            Parsed = true;
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
            for (int i = 0; i < numPoly; i++)
            {
                for (int j = 0; j < faces[i].verticesCount; j++)
                {
                    float u = faces[i].uv[j].x / tim.width;
                    float v = faces[i].uv[j].y / tim.height;
                    faces[i].uv[j] = new Vector2(u, v);
                }
            }
            Mesh weaponMesh = new Mesh();
            List<Vector3> meshVertices = new List<Vector3>();
            List<int> meshTriangles = new List<int>();
            List<Vector2> meshTrianglesUV = new List<Vector2>();
            List<BoneWeight> meshWeights = new List<BoneWeight>();

            // Geometry
            for (int i = 0; i < faces.Count; i++)
            {
                if (faces[i].type == 0x2C)
                {
                    if (faces[i].side != 4)
                    {
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(vertices[faces[i].vertices[0]].position);
                        meshWeights.Add(vertices[faces[i].vertices[0]].boneWeight);
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(vertices[faces[i].vertices[1]].position);
                        meshWeights.Add(vertices[faces[i].vertices[1]].boneWeight);
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(vertices[faces[i].vertices[2]].position);
                        meshWeights.Add(vertices[faces[i].vertices[2]].boneWeight);
                        meshTrianglesUV.Add(faces[i].uv[0]);
                        meshTrianglesUV.Add(faces[i].uv[1]);
                        meshTrianglesUV.Add(faces[i].uv[2]);

                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(vertices[faces[i].vertices[3]].position);
                        meshWeights.Add(vertices[faces[i].vertices[3]].boneWeight);
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(vertices[faces[i].vertices[2]].position);
                        meshWeights.Add(vertices[faces[i].vertices[2]].boneWeight);
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(vertices[faces[i].vertices[1]].position);
                        meshWeights.Add(vertices[faces[i].vertices[1]].boneWeight);
                        meshTrianglesUV.Add(faces[i].uv[3]);
                        meshTrianglesUV.Add(faces[i].uv[2]);
                        meshTrianglesUV.Add(faces[i].uv[1]);

                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(vertices[faces[i].vertices[2]].position);
                        meshWeights.Add(vertices[faces[i].vertices[2]].boneWeight);
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(vertices[faces[i].vertices[3]].position);
                        meshWeights.Add(vertices[faces[i].vertices[3]].boneWeight);
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(vertices[faces[i].vertices[0]].position);
                        meshWeights.Add(vertices[faces[i].vertices[0]].boneWeight);
                        meshTrianglesUV.Add(faces[i].uv[2]);
                        meshTrianglesUV.Add(faces[i].uv[3]);
                        meshTrianglesUV.Add(faces[i].uv[0]);

                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(vertices[faces[i].vertices[3]].position);
                        meshWeights.Add(vertices[faces[i].vertices[3]].boneWeight);
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(vertices[faces[i].vertices[1]].position);
                        meshWeights.Add(vertices[faces[i].vertices[1]].boneWeight);
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(vertices[faces[i].vertices[0]].position);
                        meshWeights.Add(vertices[faces[i].vertices[0]].boneWeight);
                        meshTrianglesUV.Add(faces[i].uv[3]);
                        meshTrianglesUV.Add(faces[i].uv[1]);
                        meshTrianglesUV.Add(faces[i].uv[0]);
                    }
                    else
                    {
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(vertices[faces[i].vertices[2]].position);
                        meshWeights.Add(vertices[faces[i].vertices[2]].boneWeight);
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(vertices[faces[i].vertices[1]].position);
                        meshWeights.Add(vertices[faces[i].vertices[1]].boneWeight);
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(vertices[faces[i].vertices[0]].position);
                        meshWeights.Add(vertices[faces[i].vertices[0]].boneWeight);
                        meshTrianglesUV.Add(faces[i].uv[2]);
                        meshTrianglesUV.Add(faces[i].uv[1]);
                        meshTrianglesUV.Add(faces[i].uv[0]);

                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(vertices[faces[i].vertices[1]].position);
                        meshWeights.Add(vertices[faces[i].vertices[1]].boneWeight);
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(vertices[faces[i].vertices[2]].position);
                        meshWeights.Add(vertices[faces[i].vertices[2]].boneWeight);
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(vertices[faces[i].vertices[3]].position);
                        meshWeights.Add(vertices[faces[i].vertices[3]].boneWeight);
                        meshTrianglesUV.Add(faces[i].uv[1]);
                        meshTrianglesUV.Add(faces[i].uv[2]);
                        meshTrianglesUV.Add(faces[i].uv[3]);
                    }
                }
                else if (faces[i].type == 0x24)
                {
                    if (faces[i].side != 4)
                    {
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(vertices[faces[i].vertices[2]].position);
                        meshWeights.Add(vertices[faces[i].vertices[2]].boneWeight);
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(vertices[faces[i].vertices[1]].position);
                        meshWeights.Add(vertices[faces[i].vertices[1]].boneWeight);
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(vertices[faces[i].vertices[0]].position);
                        meshWeights.Add(vertices[faces[i].vertices[0]].boneWeight);
                        meshTrianglesUV.Add(faces[i].uv[0]);
                        meshTrianglesUV.Add(faces[i].uv[2]);
                        meshTrianglesUV.Add(faces[i].uv[1]);

                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(vertices[faces[i].vertices[0]].position);
                        meshWeights.Add(vertices[faces[i].vertices[0]].boneWeight);
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(vertices[faces[i].vertices[1]].position);
                        meshWeights.Add(vertices[faces[i].vertices[1]].boneWeight);
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(vertices[faces[i].vertices[2]].position);
                        meshWeights.Add(vertices[faces[i].vertices[2]].boneWeight);
                        meshTrianglesUV.Add(faces[i].uv[2]);
                        meshTrianglesUV.Add(faces[i].uv[1]);
                        meshTrianglesUV.Add(faces[i].uv[0]);
                    }
                    else
                    {
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(vertices[faces[i].vertices[2]].position);
                        meshWeights.Add(vertices[faces[i].vertices[2]].boneWeight);
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(vertices[faces[i].vertices[1]].position);
                        meshWeights.Add(vertices[faces[i].vertices[1]].boneWeight);
                        meshTriangles.Add(meshVertices.Count);
                        meshVertices.Add(vertices[faces[i].vertices[0]].position);
                        meshWeights.Add(vertices[faces[i].vertices[0]].boneWeight);
                        meshTrianglesUV.Add(faces[i].uv[0]);
                        meshTrianglesUV.Add(faces[i].uv[2]);
                        meshTrianglesUV.Add(faces[i].uv[1]);
                    }
                }
            }

            weaponMesh.name = FileName + "_mesh";
            weaponMesh.vertices = meshVertices.ToArray();
            weaponMesh.triangles = meshTriangles.ToArray();
            weaponMesh.uv = meshTrianglesUV.ToArray();
            weaponMesh.boneWeights = meshWeights.ToArray();
            weaponMesh.RecalculateNormals();
            weaponMesh.RecalculateTangents();
            weaponMesh.RecalculateBounds();


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
            // Creating bones
            for (var i = 0; i < numBones; i++)
            {
                bones[i].parentIndex = (bones[i].parentIndex < numBones) ? (int)(bones[i].parentIndex + numBones) : -1;
            }

            // translation bones
            for (var i = numBones; i < numBones * 2; ++i)
            {
                VSBone transBone = new VSBone();
                transBone.index = i;
                transBone.name = "tbone_" + i;
                transBone.parentIndex = (int)(i - numBones);
                transBone.length = bones[transBone.parentIndex].length;
                bones.Add(transBone);
            }

            Transform[] meshBones = new Transform[numBones * 2];
            Matrix4x4[] bindPoses = new Matrix4x4[numBones * 2];
            for (int i = 0; i < numBones; i++)
            {
                meshBones[i] = new GameObject(bones[i].name).transform;
                if (bones[i].parentIndex == -1)
                {
                    meshBones[i].parent = weaponGo.transform;
                }
                else
                {
                    meshBones[i].parent = meshBones[bones[i].parentIndex];
                }

                meshBones[i].localRotation = Quaternion.identity;
                meshBones[i].localPosition = Vector3.zero;
                bindPoses[i] = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);

                meshBones[i + numBones] = new GameObject(bones[(int)(i + numBones)].name).transform;
                meshBones[i + numBones].parent = meshBones[bones[(int)(i + numBones)].parentIndex];
                meshBones[i + numBones].localRotation = Quaternion.identity;
                meshBones[i + numBones].localPosition = new Vector3((float)(bones[i].length) / 100, 0, 0);
                bindPoses[i + numBones] = Matrix4x4.TRS(new Vector3((float)(bones[i].length) / 100, 0, 0), Quaternion.identity, Vector3.one);
            }
            SkinnedMeshRenderer mr = weaponGo.AddComponent<SkinnedMeshRenderer>();
            mr.material = mat;
            weaponMesh.bindposes = bindPoses;
            mr.bones = meshBones;
            mr.rootBone = meshBones[0];
            mr.sharedMesh = weaponMesh;
            Mesh baked = new Mesh();
            baked.name = FileName + "_Mesh";
            mr.BakeMesh(baked);
            baked.RecalculateNormals();

            MTL mtl = new MTL(FileName + "_tex.png", string.Concat(FileName, ".mtl"), string.Concat("material_wep_", FileName));
            mtl.offset = new Vector3(0, 0, 0);
            mtl.scale = new Vector3(2f, 2.5f, 1f);
            mtl.Write();


            OBJ wepObj = new OBJ(baked, FileName, mtl);
            wepObj.Write();

            baked.Optimize();
            mesh = baked;

            // Database 
            /*
            int weaponId = int.Parse(FileName, System.Globalization.NumberStyles.HexNumber);
            if (weaponId < 91)
            {
                Blade weaponScript = weaponGo.AddComponent<Blade>();
                weaponScript.WEP = (byte)weaponId;
            }
            else if (weaponId > 95 && weaponId < 112)
            {
                Shield shieldScript = weaponGo.AddComponent<Shield>();
                shieldScript.type = Shield.Library(weaponId);
                shieldScript.WEP = (byte)weaponId;
            }
            */
            WEPGO = weaponGo;
            GOBuilded = true;


#if UNITY_EDITOR
            if (buildPrefab)
            {
                ToolBox.DirExNorCreate(wepFolder);
                prefab = PrefabUtility.SaveAsPrefabAsset(weaponGo, wepFilename);
                GameObject.DestroyImmediate(weaponGo.GetComponent<SkinnedMeshRenderer>());
                foreach (Transform child in weaponGo.transform)
                {
                    GameObject.DestroyImmediate(child.gameObject);
                }
                prefab = PrefabUtility.SaveAsPrefabAsset(weaponGo, wepFilename);
                WEPGO = null;
                GOBuilded = false;
                GameObject.DestroyImmediate(weaponGo);
                AssetDatabase.AddObjectToAsset(baked, wepFilename);
                AssetDatabase.AddObjectToAsset(texture, wepFilename);
                AssetDatabase.AddObjectToAsset(material, wepFilename);
                prefab.AddComponent<MeshFilter>().mesh = baked;
                prefab.AddComponent<MeshRenderer>().material = material;
                AssetDatabase.SaveAssets();
            }
#endif

        }
    }
}