using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif
using UnityEngine;
using VS.Entity;
using VS.Utils;

//http://datacrystal.romhacking.net/wiki/Vagrant_Story:SHP_files

namespace VS.Parser
{
    public class SHP : FileParser
    {
        public uint numBones;
        public uint numGroups;
        public uint numTriangles;
        public uint numQuads;
        public uint numPolys;
        public uint numFaces;

        public List<VSBone> bones;
        public List<VSGroup> groups;
        public List<VSVertex> vertices;
        public List<VSFace> faces;

        public bool UseDebug = false;

        public Mesh mesh;
        public Texture2D texture;
        public Material material;
        private TIM tim;
        public GameObject shapeGo;

        public SHP()
        {
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
            byte[] header = buffer.ReadBytes(4);
            numBones = buffer.ReadByte();
            numGroups = buffer.ReadByte();
            numTriangles = buffer.ReadUInt16();
            numQuads = buffer.ReadUInt16();
            numPolys = buffer.ReadUInt16();
            numFaces = numTriangles + numQuads + numPolys;
            byte[][] overlays = new byte[8][];
            for (int i = 0; i < 8; i++)
            {
                overlays[i] = buffer.ReadBytes(4);
            }

            buffer.ReadBytes(0x24); // Unknown
            buffer.ReadBytes(6); // collision size and height (shape is a cylinder)
            buffer.ReadBytes(2); // menu position Y
            buffer.ReadBytes(12); // Unknown
            buffer.ReadBytes(2); // Shadow radius
            buffer.ReadBytes(2); // Shadow size increase rate
            buffer.ReadBytes(2); // Shadow size decrease rate
            buffer.ReadBytes(4); // Unknown
            buffer.ReadBytes(2); // Menu scale
            buffer.ReadBytes(2); // Unknown
            buffer.ReadBytes(2); // Target sphere position Y
            buffer.ReadBytes(8); // Unknown

            if (UseDebug)
            {
                Debug.Log(FileName);
                Debug.Log("numBones : " + numBones);
                Debug.Log("numGroups : " + numGroups);
                Debug.Log("numTriangles : " + numTriangles);
                Debug.Log("numQuads : " + numQuads);
                Debug.Log("numPolys : " + numPolys);
                Debug.Log("numFaces : " + numFaces);
            }

            // LBA XX_BTX.SEQ  (battle animations first one is actually XX_COM.SEQ)     certainly one weapon type for each file 
            /*
             * LBA SEQ : 98646  OBJ\01_COM.SEQ
             * LBA SEQ : 0      
             * LBA SEQ : 98599  OBJ\01_BT2.SEQ
             * LBA SEQ : 0
             * LBA SEQ : 98611  OBJ\01_BT4.SEQ
             * LBA SEQ : 0
             * LBA SEQ : 98623  OBJ\01_BT6.SEQ
             * LBA SEQ : 0
             * LBA SEQ : 0
             * LBA SEQ : 0
             * LBA SEQ : 98635  OBJ\01_BTA.SEQ
             * LBA SEQ : 0
            */
            for (int i = 0; i < 12; i++)
            {
                buffer.ReadUInt32();
            }

            for (int i = 0; i < 12; i++)
            {
                buffer.ReadBytes(2); // chain attack animation ID
            }

            for (int i = 0; i < 4; i++)
            {
                buffer.ReadBytes(4); // LBA XXSP0X.SEQ (special attack animations)	
            }

            buffer.ReadBytes(0x20); // unknown (probably more LBA tables, there are also special attack ids stored here.)  file like 00BT1B00.SEQ (Battle Techniques)
            long dec = buffer.BaseStream.Position + 4;
            long magicPtr = buffer.ReadUInt32() + dec;
            for (int i = 0; i < 0x18; i++)
            {
                buffer.ReadBytes(2); // unknown (noticeable effects when casting spell
            }

            long AKAOPtr = buffer.ReadUInt32() + dec;
            long groupPtr = buffer.ReadUInt32() + dec;
            long vertexPtr = buffer.ReadUInt32() + dec;
            long facePtr = buffer.ReadUInt32() + dec;

            if (UseDebug)
            {
                Debug.Log("magicPtr : " + magicPtr);
                Debug.Log("AKAOPtr : " + AKAOPtr);
                Debug.Log("groupPtr : " + groupPtr);
                Debug.Log("vertexPtr : " + vertexPtr);
                Debug.Log("facePtr : " + facePtr);
            }

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
                if (UseDebug)
                {
                    Debug.Log(buffer.BaseStream.Position + " != " + groupPtr);
                    Debug.Log("le pointeur groupPtr n'est pas à la bonne place");
                }
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
                if (UseDebug)
                {
                    Debug.Log(buffer.BaseStream.Position + " != " + vertexPtr);
                    Debug.Log("le pointeur vertexPtr n'est pas à la bonne place");
                }
                buffer.BaseStream.Position = vertexPtr;
            }
            vertices = new List<VSVertex>();
            uint numVertices = groups[groups.Count - 1].numVertices;
            if (UseDebug)
            {
                Debug.Log("numVertices : " + numVertices);
            }

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
            if (buffer.BaseStream.Position != facePtr)
            {
                if (UseDebug)
                {
                    Debug.Log(buffer.BaseStream.Position + " != " + facePtr);
                    Debug.Log("le pointeur facePtr n'est pas à la bonne place");
                }
                buffer.BaseStream.Position = facePtr;
            }

            faces = new List<VSFace>();
            for (uint i = 0; i < numFaces; i++)
            {
                VSFace face = new VSFace();
                face.type = buffer.ReadByte();
                face.size = buffer.ReadByte();
                face.side = buffer.ReadByte();
                face.alpha = buffer.ReadByte();
                long polyDec = buffer.BaseStream.Position - 4;
                if (UseDebug)
                {
                    Debug.Log("face# " + i + "   face.type : " + face.type + "   face.size : " + face.size + "   face.side : " + face.side + "   face.alpha : " + face.alpha);
                }

                uint[] table = new uint[256];
                table[36] = 3;
                table[44] = 4;

                face.verticesCount = 0; // fallback
                if (table[face.type] != 0)
                {
                    face.verticesCount = table[face.type];
                }
                else
                {
                    if (UseDebug)
                    {
                        Debug.Log("####  Unknown face type !");
                    }

                    if (face.size < 5)
                    {
                        face.verticesCount = (uint)face.size;
                    }
                }

                face.vertices = new List<int>();
                for (uint j = 0; j < face.verticesCount; j++)
                {
                    int vId = buffer.ReadUInt16() / 4;
                    face.vertices.Add(vId);
                    if (UseDebug)
                    {
                        Debug.Log("vId : " + j + " - " + vId);
                    }
                }
                face.uv = new List<Vector2>();
                for (uint j = 0; j < face.verticesCount; j++)
                {
                    int u = buffer.ReadByte();
                    int v = buffer.ReadByte();
                    face.uv.Add(new Vector2(u, v));
                    if (UseDebug)
                    {
                        Debug.Log("u : " + u + "    v : " + v);
                    }
                }
                faces.Add(face);
            }

            // AKAO section
            if (buffer.BaseStream.Position != AKAOPtr)
            {
                if (UseDebug)
                {
                    Debug.LogWarning(buffer.BaseStream.Position + " != " + AKAOPtr + " le pointeur AKAOPtr n'est pas à la bonne place " + FileName);
                }

                buffer.BaseStream.Position = AKAOPtr;


                uint akaoNum = buffer.ReadUInt32();
                uint[] akaoFramesPtr = new uint[akaoNum];
                for (uint j = 0; j < akaoNum; j++)
                {
                    akaoFramesPtr[j] = buffer.ReadUInt32();
                }

                for (uint j = 0; j < akaoNum; j++)
                {
                    /*
                    AKAO akao = new AKAO();
                    akao.soundName = shapeName + "_AKAO_" + j;
                    akao.debugger = debugger;
                    akao.aType = AKAO.akaotypes.shp;
                    akao.parseFromBuffer(buffer);
                    */
                }
            }
            // skip

            // Magic section
            if (buffer.BaseStream.Position != magicPtr)
            {
                if (UseDebug)
                {
                    Debug.LogWarning(buffer.BaseStream.Position + " != " + magicPtr + " le pointeur magicPtr n'est pas à la bonne place " + FileName);
                }

                buffer.BaseStream.Position = magicPtr;
            }
            long num = buffer.ReadUInt32();
            long magicSize = buffer.ReadUInt32(); //size of magic effect section (doesnt include this 8 byte header)

            long num1 = buffer.ReadUInt32();
            long num2 = buffer.ReadUInt32();
            long num3 = buffer.ReadUInt32();

            if (UseDebug)
            {
                Debug.Log("num : " + num);
                Debug.Log("magicSize : " + magicSize);
                Debug.Log("num1 : " + num1);
                Debug.Log("num2 : " + num2);
                Debug.Log("num3 : " + num3);
            }
            if (buffer.BaseStream.Position + (magicSize - 12) < buffer.BaseStream.Length)
            {
                buffer.BaseStream.Position = buffer.BaseStream.Position + (magicSize - 12);
            }



            // Textures section
            tim = new TIM();
            tim.FileName = FileName;
            tim.ParseSHP(buffer);
            texture = tim.DrawSHP(true);

            //buffer.Close();
        }



        public GameObject BuildGameObject()
        {
            Build(false);
            return shapeGo;
        }

        public void BuildPrefab(bool erase = false)
        {
            string modFolder = "Assets/Resources/Prefabs/Models/";
            ToolBox.DirExNorCreate(modFolder);
            string modFilename = modFolder + FileName + ".prefab";
            GameObject pref = AssetDatabase.LoadAssetAtPath(modFilename, typeof(GameObject)) as GameObject;
            if (pref != null && erase == false)
            {
                return;
            }
            else
            {
                if (erase)
                {
                    AssetDatabase.DeleteAsset(modFilename);
                }

                Build(true);
            }
        }

        public void Build(bool buildPrefab = false)
        {

            // Building Model
            for (int i = 0; i < numFaces; i++)
            {
                for (int j = 0; j < faces[i].verticesCount; j++)
                {
                    float u = faces[i].uv[j].x / tim.width;
                    float v = faces[i].uv[j].y / tim.height;
                    faces[i].uv[j] = (new Vector2(u, v));
                }
            }
            Mesh shapeMesh = new Mesh();
            List<Vector3> meshVertices = new List<Vector3>();
            List<int> meshTriangles = new List<int>();
            List<Vector2> meshTrianglesUV = new List<Vector2>();
            List<BoneWeight> meshWeights = new List<BoneWeight>();

            for (int i = 0; i < faces.Count; i++)
            {
                //Debug.Log("faces["+i+"]     .type : " + faces[i].type + " ||    .size : " + faces[i].size + " ||    .side : " + faces[i].side + " ||    .alpha : " + faces[i].alpha);
                if (faces[i].type == 0x2C)
                {
                    if (faces[i].side == 8)
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
                    if (faces[i].side == 8)
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
            shapeMesh.name = FileName + "_mesh";
            shapeMesh.vertices = meshVertices.ToArray();
            shapeMesh.triangles = meshTriangles.ToArray();
            shapeMesh.uv = meshTrianglesUV.ToArray();
            shapeMesh.boneWeights = meshWeights.ToArray();

            mesh = shapeMesh;

            texture = tim.textures[0];

            texture.filterMode = FilterMode.Trilinear;
            texture.anisoLevel = 4;
#if UNITY_EDITOR
            texture.alphaIsTransparency = true;
#endif
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.Compress(true);


            Shader shader = Shader.Find("Standard");
            Material mat = new Material(shader);
            mat.name = FileName + "_mat";
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

            material = mat;

            GameObject shapeGo = new GameObject(FileName);
            GameObject meshGo = new GameObject(FileName + "_mesh");
            meshGo.transform.parent = shapeGo.transform;
            /*
            MeshFilter mf = meshGo.AddComponent<MeshFilter>();
            mf.mesh = shapeMesh;
            */
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
                    meshBones[i].parent = shapeGo.transform;
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
            SkinnedMeshRenderer mr = meshGo.AddComponent<SkinnedMeshRenderer>();
            mr.material = mat;
            shapeMesh.bindposes = bindPoses;
            mr.bones = meshBones;
            mr.rootBone = meshBones[0];
            mr.sharedMesh = shapeMesh;

            this.shapeGo = shapeGo;

            string modFolder = "Assets/Resources/Prefabs/Models/";
            string modFilename = modFolder + FileName + ".prefab";



#if UNITY_EDITOR
            if (buildPrefab == true)
            {

                GameObject shpBase = shapeGo;
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(shpBase, modFilename);
                if (shpBase != null)
                {
                    AssetDatabase.AddObjectToAsset(mesh, modFilename);
                    AssetDatabase.AddObjectToAsset(material, modFilename);
                    AssetDatabase.AddObjectToAsset(texture, modFilename);
                }
                string shpId = FileName;
                if (shpId == "06" || shpId == "99" || shpId == "C9" || shpId == "CA")
                {
                    shpId = "00"; // Ashley Shapes
                }


                if (shpId == "7C")
                {
                    shpId = "02"; // Rozencrantz
                }

                if (shpId == "9B")
                {
                    shpId = "03"; // Merlose
                }

                if (shpId == "98")
                {
                    shpId = "83"; // Sydney
                }

                // Animations seeker
                Avatar ava = AvatarBuilder.BuildGenericAvatar(shpBase, "");
                ava.name = FileName + "_Ava";
                AssetDatabase.AddObjectToAsset(ava, modFilename);
                Animator animator = shpBase.GetComponent<Animator>();
                if (!animator)
                {
                    animator = shpBase.AddComponent<Animator>();
                }

                AnimatorController ac = AnimatorController.Instantiate(AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Resources/Prefabs/DefaultAC.controller") as AnimatorController);
                ac.name = FileName + "_AC";
                animator.runtimeAnimatorController = ac;
                animator.avatar = ava;
                AssetDatabase.AddObjectToAsset(ac, modFilename);


                string[] hash = FilePath.Split("/"[0]);
                hash[hash.Length - 1] = "";
                string[] files = Directory.GetFiles(String.Join("/", hash), "*.SEQ");
                bool posed = false;
                uint i = 0;
                foreach (string file in files)
                {
                    List<string> topa = new List<string>();
                    topa.Add("_COM.SEQ");
                    topa.Add("_BT1.SEQ");
                    topa.Add("_BT2.SEQ");
                    topa.Add("_BT3.SEQ");
                    topa.Add("_BT4.SEQ");
                    topa.Add("_BT5.SEQ");
                    topa.Add("_BT6.SEQ");
                    topa.Add("_BT7.SEQ");
                    topa.Add("_BT8.SEQ");
                    topa.Add("_BT9.SEQ");
                    topa.Add("_BTA.SEQ");
                    hash = file.Split("/"[0]);
                    if (hash[hash.Length - 1].StartsWith(shpId) && file.EndsWith(".SEQ"))
                    {
                        if (topa.Contains(hash[hash.Length - 1].Substring(2, 8)))
                        {
                            SEQ _seq = new SEQ();
                            _seq.Parse(file);
                            if (!posed || file == shpId + "_COM.SEQ")
                            {
                                posed = true;
                                _seq.FirstPoseModel(shpBase);
                            }
                            AnimationClip[] clips = _seq.BuildAnimationClips(shpBase);
                            AnimatorControllerLayer layer = new AnimatorControllerLayer();
                            layer.name = hash[hash.Length - 1].Substring(0, 6);
                            layer.stateMachine = new AnimatorStateMachine();
                            layer.stateMachine.states = ac.layers[0].stateMachine.states;
                            ac.AddLayer(hash[hash.Length - 1].Substring(0, 6));
                            i++;
                            foreach (AnimationClip clip in clips)
                            {
                                if (clip != null)
                                {
                                    AssetDatabase.AddObjectToAsset(clip, modFilename);
                                    AnimatorState state = ac.layers[i].stateMachine.AddState(clip.name);
                                    state.motion = clip;
                                }
                            }
                        }
                    }
                }

                //PrefabUtility.ReplacePrefab(shpBase, prefab);
                prefab = PrefabUtility.SaveAsPrefabAsset(shpBase, modFilename);
                AssetDatabase.SaveAssets();
                GameObject.DestroyImmediate(shpBase);
            }

#endif
        }

    }
}
