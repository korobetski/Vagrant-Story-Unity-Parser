using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif
using UnityEngine;
using VS.Entity;
using VS.Format;
using VS.Utils;

//http://datacrystal.romhacking.net/wiki/Vagrant_Story:SHP_files

namespace VS.Parser
{
    public class SHP : FileParser
    {
        public Mesh mesh;
        public Texture2D texture;
        public Material material;
        public GameObject shapeGo;

        private uint numBones;
        private uint numGroups;
        private uint numTriangles;
        private uint numQuads;
        private uint numPolys;
        private uint numFaces;
        private List<VSBone> bones;
        private List<VSGroup> groups;
        private List<VSVertex> vertices;
        private List<VSFace> faces;
        private TIM tim;
        public bool excpFaces = false;

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
                bone.length = buffer.ReadInt16();
                //Debug.LogWarning("bone  "+i+ " .length : "+ bone.length);
                buffer.ReadUInt16(); // always 0xFFFF
                bone.parentIndex = buffer.ReadByte();
                if (bone.parentIndex > numBones)
                {
                    bone.parentIndex = -1;
                }
                //Debug.LogWarning("bone.parentIndex : " + bone.parentIndex);
                // https://github.com/morris/vstools/blob/master/src/WEPBone.js
                byte[] offset = buffer.ReadBytes(3);
                bone.offset = new Vector3(offset[0], offset[1], offset[2]);
                bone.mode = buffer.ReadByte();
                // 0 - 2 normal ?
                // 3 - 6 normal + roll 90 degrees
                // 7 - 255 absolute, different angles

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
                vertex.position = -new Vector3(x, y, z) / 100;
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

            if (excpFaces)
            {
                if (UseDebug)
                {
                    Debug.LogWarning("-------------   TRIANGLES    ----------------------");
                    Debug.LogWarning(buffer.BaseStream.Position);
                }
                for (uint i = 0; i < numTriangles; i++)
                {
                    faces.Add(ParseColoredFace(buffer, 3));
                }
                if (UseDebug)
                {
                    Debug.LogWarning("-------------   QUAD    ----------------------");
                    Debug.LogWarning(buffer.BaseStream.Position);
                }
                for (uint i = 0; i < numQuads; i++)
                {
                    faces.Add(ParseColoredFace(buffer, 4));
                }
                if (UseDebug)
                {
                    Debug.LogWarning("-------------   POLY    ----------------------");
                    Debug.LogWarning(buffer.BaseStream.Position);
                }
                for (uint i = 0; i < numPolys; i++)
                {
                    long polyDec = buffer.BaseStream.Position;
                    byte[] bytes = buffer.ReadBytes(24);
                    if (bytes[11] == 0x34)
                    {
                        // its a triangle
                        buffer.BaseStream.Position = polyDec;
                        faces.Add(ParseColoredFace(buffer, 3));
                    }
                    if (bytes[11] == 0x3C)
                    {
                        // its a quad
                        buffer.BaseStream.Position = polyDec;
                        faces.Add(ParseColoredFace(buffer, 4));
                    }
                }
            }
            else
            {
                for (uint i = 0; i < numFaces; i++)
                {
                    VSFace face = new VSFace();
                    long polyDec = buffer.BaseStream.Position;
                    // 4 bytes
                    face.type = buffer.ReadByte();
                    face.size = buffer.ReadByte();
                    face.side = buffer.ReadByte();
                    face.alpha = buffer.ReadByte();

                    /*
                     * Triangles
                     * 24-10-04-00-C406-CC06-C806-08-78-05-6D-05-7C
                     * 24-10-04-00-D006-D806-D406-13-6D-0A-6D-0C-78
                     * 24-10-04-00-D406-E006-DC06-13-78-13-6D-0F-78
                     * 24-10-04-00-D406-D806-E006-0F-78-13-6D-0C-78
                     * */
                    
                    // if (UseDebug)Debug.Log(string.Concat("#####   At : ", polyDec, " face# ", i, "   face.type : ", face.type, "   face.size : ", face.size, "   face.side : ", face.side, "   face.alpha : ", face.alpha));
                    

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
                            Debug.LogError("####  Unknown face type !");
                        }
                    }

                    // 6 or 8 bytes
                    for (uint j = 0; j < face.verticesCount; j++)
                    {
                        int vId = buffer.ReadUInt16() / 4;
                        face.vertices.Add(vId);

                        // if (UseDebug) Debug.Log("vId : " + j + " - " + vId);
                    }

                    // 6 or 8 bytes
                    for (uint j = 0; j < face.verticesCount; j++)
                    {
                        int u = buffer.ReadByte();
                        int v = buffer.ReadByte();
                        face.uv.Add(new Vector2(u, v));
                        // if (UseDebug) Debug.Log("u : " + u + "    v : " + v);
                    }
                    faces.Add(face);
                }

            }

            // AKAO section
            if (buffer.BaseStream.Position != AKAOPtr)
            {
                if (UseDebug)
                {
                    Debug.LogWarning(buffer.BaseStream.Position + " != " + AKAOPtr + " le pointeur AKAOPtr n'est pas à la bonne place " + FileName);
                }

                buffer.BaseStream.Position = AKAOPtr;
            }


            uint akaoNum = buffer.ReadUInt32();
            if (UseDebug)
            {
                Debug.Log(string.Concat("akaoNum : ", akaoNum));
            }

            uint[] akaoFramesPtr = new uint[akaoNum];
            // one pointer for AKAO header, a second for AKAO datas
            for (uint j = 0; j < akaoNum; j++)
            {
                akaoFramesPtr[j] = buffer.ReadUInt32();
            }
            for (uint j = 0; j < akaoNum; j += 2)
            {
                long limit;
                if (j < akaoFramesPtr.Length - 1)
                {
                    limit = AKAOPtr + akaoFramesPtr[j + 1];
                    // somtimes there are empty ptrs at the begining so we skip
                    if (akaoFramesPtr[j + 1] > 0)
                    {
                        AKAO akao = new AKAO();
                        akao.FileName = string.Concat(FileName, "_Akao_", j);
                        akao.UseDebug = true;
                        //akao.Parse(buffer, AKAO.UNKNOWN, limit);
                    }
                }
                else
                {
                    limit = magicPtr;
                    AKAO akao = new AKAO();
                    akao.FileName = string.Concat(FileName, "_Akao_", j);
                    akao.UseDebug = true;
                    //akao.Parse(buffer, AKAO.UNKNOWN, limit);
                }


            }

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
                Debug.Log(string.Concat("Magic   num : ", num, "   magicSize : ", magicSize, "  num1 : " + num1, "  num2 : " + num2, "  num3 : " + num3));
            }
            if (buffer.BaseStream.Position + (magicSize - 12) < buffer.BaseStream.Length)
            {
                buffer.BaseStream.Position = buffer.BaseStream.Position + (magicSize - 12);
            }

            if (UseDebug)
            {
                Debug.Log(string.Concat("buffer.BaseStream.Position : ", buffer.BaseStream.Position, "   buffer.BaseStream.Length : ", buffer.BaseStream.Length));
            }

            if (buffer.BaseStream.Position + 8 < buffer.BaseStream.Length)
            {
                // Textures section
                tim = new TIM();
                tim.FileName = FileName;
                tim.ParseSHP(buffer, excpFaces);
                texture = tim.DrawSHP(true);
            }

            //buffer.Close();
        }

        private VSFace ParseColoredFace(BinaryReader buffer, byte v)
        {
            VSFace face = new VSFace();
            long polyDec = buffer.BaseStream.Position;
            byte[] bytes;
            Vector2 uv1;
            Vector2 uv2;
            Vector2 uv3;
            Vector2 uv4;
            switch (v)
            {
                case 3:
                    // 24 bytes
                    if (UseDebug)
                    {
                        bytes = buffer.ReadBytes(24);
                        Debug.Log(BitConverter.ToString(bytes));
                        buffer.BaseStream.Position = polyDec;
                    }
                    // vt1  vt2  vt3  u1-v1 col1   t  col2   sz col3   sd u2-v2 u3-v3
                    // D006-D806-D406-13-6D-FFEFB5-34-FFEFB5-18-FFEFB5-04-0A-6D-0C-78
                    // D406-E006-DC06-13-78-FFEFB5-34-FFEFB5-18-2C2C26-04-13-6D-0F-78
                    // D406-D806-E006-0F-78-151515-34-151515-18-151515-04-13-6D-0C-78
                    // E406-EC06-E806-40-78-181817-34-4F4F45-18-4F4F45-04-43-7C-43-6E

                    // vt1 / vt2 / vt3
                    face.verticesCount = 3;
                    for (uint j = 0; j < face.verticesCount; j++)
                    {
                        int vId = buffer.ReadUInt16() / 4;
                        face.vertices.Add(vId);
                    }
                    // uv1
                    uv1 = new Vector2(buffer.ReadByte(), buffer.ReadByte());
                    // color 1
                    face.colors.Add(new Color32(buffer.ReadByte(), buffer.ReadByte(), buffer.ReadByte(), 255));
                    // face type
                    face.type = buffer.ReadByte();
                    // color 2
                    face.colors.Add(new Color32(buffer.ReadByte(), buffer.ReadByte(), buffer.ReadByte(), 255));
                    // face size (bytes)
                    face.size = buffer.ReadByte();
                    // color 3
                    face.colors.Add(new Color32(buffer.ReadByte(), buffer.ReadByte(), buffer.ReadByte(), 255));
                    // face side
                    face.side = buffer.ReadByte();
                    // uv2
                    uv2 = new Vector2(buffer.ReadByte(), buffer.ReadByte());
                    // uv3
                    uv3 = new Vector2(buffer.ReadByte(), buffer.ReadByte());

                    //Debug.Log(string.Concat("uv1 : ", uv1, "  uv2 : ", uv2, "  uv3 : ", uv3));

                    face.uv.Add(uv1);
                    face.uv.Add(uv2);
                    face.uv.Add(uv3);
                    break;
                case 4:
                    // 32 bytes
                    if (UseDebug)
                    {
                        bytes = buffer.ReadBytes(32);
                        Debug.Log(BitConverter.ToString(bytes));
                        buffer.BaseStream.Position = polyDec;
                    }
                    // vt1  vt2  vt3  vt4  col1   t  col2   sz col3   sd col4   pa u1-v1 u2-v2 u3-v3 u4-v4
                    // FC06-1807-1007-0C07-393937-3C-1B1B1B-20-323231-04-383838-00-46-68-4D-68-4A-5E-4D-5E
                    // 5007-4C07-4407-4807-4D4C46-3C-B7B8BF-20-53534F-04-B7B8AF-00-1B-89-1E-8C-21-81-21-89
                    // 6007-5C07-5407-5807-C7C8FF-3C-555555-20-555555-04-555555-00-28-81-28-89-2E-89-2B-8C
                    // 7007-6C07-6407-6807-4D4C46-3C-B7B8BF-20-53534F-04-B7B8BF-00-1B-89-1E-8C-21-81-21-89

                    // vt1 / vt2 / vt3 / vt4
                    face.verticesCount = 4;
                    for (uint j = 0; j < face.verticesCount; j++)
                    {
                        int vId = buffer.ReadUInt16() / 4;
                        face.vertices.Add(vId);
                    }
                    // color 1
                    face.colors.Add(new Color32(buffer.ReadByte(), buffer.ReadByte(), buffer.ReadByte(), 255));
                    // face type
                    face.type = buffer.ReadByte();
                    // color 2
                    face.colors.Add(new Color32(buffer.ReadByte(), buffer.ReadByte(), buffer.ReadByte(), 255));
                    // face size (bytes)
                    face.size = buffer.ReadByte();
                    // color 3
                    face.colors.Add(new Color32(buffer.ReadByte(), buffer.ReadByte(), buffer.ReadByte(), 255));
                    // face side
                    face.side = buffer.ReadByte();
                    // color 4
                    face.colors.Add(new Color32(buffer.ReadByte(), buffer.ReadByte(), buffer.ReadByte(), 255));
                    // padding
                    buffer.ReadByte();
                    // uv1
                    uv1 = new Vector2(buffer.ReadByte(), buffer.ReadByte());
                    // uv2
                    uv2 = new Vector2(buffer.ReadByte(), buffer.ReadByte());
                    // uv3
                    uv3 = new Vector2(buffer.ReadByte(), buffer.ReadByte());
                    // uv4
                    uv4 = new Vector2(buffer.ReadByte(), buffer.ReadByte());

                    //Debug.LogWarning(string.Concat("uv1 : ", uv1, "  uv2 : ", uv2, "  uv3 : ", uv3, "  uv4 : ", uv4));

                    face.uv.Add(uv1);
                    face.uv.Add(uv2);
                    face.uv.Add(uv3);
                    face.uv.Add(uv4);

                    break;
            }

            return face;
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
            string modFilename = string.Concat(modFolder, "SHP_", FileName, ".prefab");
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
                    if (tim != null)
                    {
                        float u = faces[i].uv[j].x / tim.width;
                        float v = faces[i].uv[j].y / tim.height;
                        faces[i].uv[j] = (new Vector2(u, v));
                    }
                    else
                    {
                        faces[i].uv[j] = Vector2.zero;
                    }
                }
            }
            Mesh shapeMesh = new Mesh();
            List<Vector3> meshVertices = new List<Vector3>();
            List<int> meshTriangles = new List<int>();
            List<Vector2> meshTrianglesUV = new List<Vector2>();
            List<BoneWeight> meshWeights = new List<BoneWeight>();
            List<Color32> meshColors = new List<Color32>();

            for (int i = 0; i < faces.Count; i++)
            {
                //Debug.Log("faces["+i+"]     .type : " + faces[i].type + " ||    .size : " + faces[i].size + " ||    .side : " + faces[i].side + " ||    .alpha : " + faces[i].alpha);
                if (faces[i].type == 0x2C || faces[i].type == 0x3C)
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

                        if (excpFaces)
                        {
                            // we have colored vertices
                            meshColors.Add(faces[i].colors[0]);
                            meshColors.Add(faces[i].colors[1]);
                            meshColors.Add(faces[i].colors[2]);

                            meshColors.Add(faces[i].colors[3]);
                            meshColors.Add(faces[i].colors[2]);
                            meshColors.Add(faces[i].colors[1]);

                            meshColors.Add(faces[i].colors[2]);
                            meshColors.Add(faces[i].colors[1]);
                            meshColors.Add(faces[i].colors[0]);

                            meshColors.Add(faces[i].colors[1]);
                            meshColors.Add(faces[i].colors[2]);
                            meshColors.Add(faces[i].colors[3]);
                        }
                    }
                    else
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
                        if (excpFaces)
                        {
                            // we have colored vertices
                            meshColors.Add(faces[i].colors[0]);
                            meshColors.Add(faces[i].colors[1]);
                            meshColors.Add(faces[i].colors[2]);

                            meshColors.Add(faces[i].colors[3]);
                            meshColors.Add(faces[i].colors[2]);
                            meshColors.Add(faces[i].colors[1]);
                        }
                    }
                }
                else if (faces[i].type == 0x24 || faces[i].type == 0x34)
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
                        meshTrianglesUV.Add(faces[i].uv[1]);
                        meshTrianglesUV.Add(faces[i].uv[2]);
                        meshTrianglesUV.Add(faces[i].uv[0]);

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
                        if (excpFaces)
                        {
                            // we have colored vertices
                            meshColors.Add(faces[i].colors[0]);
                            meshColors.Add(faces[i].colors[1]);
                            meshColors.Add(faces[i].colors[2]);

                            meshColors.Add(faces[i].colors[2]);
                            meshColors.Add(faces[i].colors[1]);
                            meshColors.Add(faces[i].colors[0]);
                        }
                    }
                    else
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
                        meshTrianglesUV.Add(faces[i].uv[1]);
                        meshTrianglesUV.Add(faces[i].uv[2]);
                        meshTrianglesUV.Add(faces[i].uv[0]);
                        if (excpFaces)
                        {
                            // we have colored vertices
                            meshColors.Add(faces[i].colors[0]);
                            meshColors.Add(faces[i].colors[1]);
                            meshColors.Add(faces[i].colors[2]);
                        }
                    }
                }
            }
            shapeMesh.name = FileName + "_mesh";
            shapeMesh.vertices = meshVertices.ToArray();
            shapeMesh.triangles = meshTriangles.ToArray();
            shapeMesh.uv = meshTrianglesUV.ToArray();
            shapeMesh.boneWeights = meshWeights.ToArray();
            if (excpFaces)
            {
                shapeMesh.colors32 = meshColors.ToArray();
            }

            if (tim != null)
            {
                texture = tim.textures[0];
                texture.filterMode = FilterMode.Trilinear;
                texture.anisoLevel = 4;
#if UNITY_EDITOR
                texture.alphaIsTransparency = true;
#endif
                texture.wrapMode = TextureWrapMode.Repeat;
                texture.Compress(true);
            }
            else
            {
                texture = new Texture2D(128, 128);
            }

            Material mat = null;
            if (excpFaces)
            {
                Shader shader = Shader.Find("Particles/Standard Unlit");
                mat = new Material(shader);
                mat.name = string.Concat("Material_SHP_", FileName);
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
                mat.name = string.Concat("Material_SHP_", FileName);
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

            material = mat;

            GameObject shapeGo = new GameObject(FileName);
            GameObject meshGo = new GameObject(FileName + "_mesh");
            meshGo.transform.parent = shapeGo.transform;


            Transform[] meshBones = new Transform[numBones];
            Matrix4x4[] bindPoses = new Matrix4x4[numBones];
            for (int i = 0; i < numBones; i++)
            {
                meshBones[i] = new GameObject(bones[i].name).transform;
                meshBones[i].localRotation = Quaternion.identity;
                bindPoses[i] = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
                if (bones[i].parentIndex == -1)
                {
                    meshBones[i].parent = shapeGo.transform;
                    meshBones[i].localPosition = Vector3.zero;
                }
                else
                {
                    meshBones[i].parent = meshBones[bones[i].parentIndex];
                    meshBones[i].localPosition = new Vector3((float)(bones[bones[i].parentIndex].length) / 100, 0, 0);
                }
            }
            SkinnedMeshRenderer mr = meshGo.AddComponent<SkinnedMeshRenderer>();
            mr.material = mat;
            mr.bones = meshBones;
            shapeMesh.bindposes = bindPoses;

            mr.rootBone = meshBones[0];
            mr.sharedMesh = shapeMesh;

            mesh = shapeMesh;

            this.shapeGo = shapeGo;

            string modFolder = "Assets/Resources/Prefabs/Models/";
            string modFilename = string.Concat(modFolder, "SHP_", FileName, ".prefab");



#if UNITY_EDITOR
            if (buildPrefab == true)
            {

                GameObject shpBase = shapeGo;
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(shpBase, modFilename);
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
                Avatar ava = AvatarBuilder.BuildGenericAvatar(shpBase, "bone_0");
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
                            /*
                            if (!posed || file == shpId + "_COM.SEQ")
                            {
                                posed = true;
                                _seq.FirstPoseModel(shpBase);

                                Mesh baked = new Mesh();
                                baked.name = string.Concat("Baked_Mesh_SHP_", FileName);
                                mr.BakeMesh(baked);
                                //baked.bindposes = bindPoses; // need to recalculate this before using the baked mesh
                                //baked.boneWeights = meshWeights.ToArray();
                                //mr.sharedMesh = baked;

                                //shapeMesh.RecalculateNormals();
                                shapeMesh.RecalculateTangents();
                                shapeMesh.Optimize();

                                //AssetDatabase.RemoveObjectFromAsset(mesh);
                                AssetDatabase.AddObjectToAsset(baked, modFilename);

                                MTL mtl = new MTL(FileName + "_tex.png", string.Concat(FileName, ".mtl"), string.Concat("material_shp_", FileName));
                                mtl.offset = new Vector3(0, 0, 0);
                                mtl.scale = new Vector3(1f, 2f, 1f);
                                mtl.Write();


                                //OBJ shpObj = new OBJ(baked, FileName, mtl);
                                //shpObj.Write();
                            }
                            */

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

                if (shpBase != null)
                {
                    mesh = shapeMesh;
                    AssetDatabase.AddObjectToAsset(mesh, modFilename);
                    AssetDatabase.AddObjectToAsset(material, modFilename);
                    AssetDatabase.AddObjectToAsset(texture, modFilename);
                }
                //PrefabUtility.ReplacePrefab(shpBase, prefab);
                prefab = PrefabUtility.SaveAsPrefabAsset(shpBase, modFilename);
                AssetDatabase.SaveAssets();
                GameObject.DestroyImmediate(shpBase);
            }

#endif
            mesh = shapeMesh;
        }

    }
}
