/*

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using VS.Serializable;
using VS.Utils;

// http://datacrystal.romhacking.net/wiki/Vagrant_Story:ARM_files
// http://datacrystal.romhacking.net/wiki/Vagrant_Story:areas_list
// MINI MAP File format in SMALL/ folder
// This File Format is near 100% explored

namespace VS.Parser
{
    public class ARM : FileParser
    {
        public bool GOBuilded = false;
        public bool PrefabBuilded = false;
        public GameObject ARMGO;

        public Serializable.ARM so;

        public void CoreParse()
        {
            so = ScriptableObject.CreateInstance<VS.Serializable.ARM>();
            so.NumRooms = buffer.ReadUInt32();
            so.Rooms = new ARMRoom[so.NumRooms];
            for (int i = 0; i < so.NumRooms; i++)
            {
                ARMRoom room = new ARMRoom();
                room.name = "Room_" + i;
                room.Unk = buffer.ReadUInt32(); // ? (RAM only)
                room.Length = buffer.ReadUInt32(); // lenght of map graphics section (RAM: pointer to section)
                room.ZoneId = buffer.ReadUInt16();
                room.MapId = buffer.ReadUInt16();
                so.Rooms[i] = room;
            }

            for (int i = 0; i < so.NumRooms; i++)
            {
                so.Rooms[i].NumVertices = buffer.ReadUInt32();
                so.Rooms[i].Vertices = new VSVertex[so.Rooms[i].NumVertices];
                for (int j = 0; j < so.Rooms[i].NumVertices; j++)
                {
                    VSVertex vertex = new VSVertex();
                    vertex.position = new Vector4(buffer.ReadInt16(), buffer.ReadInt16(), buffer.ReadInt16(), buffer.ReadInt16());
                    so.Rooms[i].Vertices[j] = vertex;
                }

                so.Rooms[i].NumTriangles = buffer.ReadUInt32();
                so.Rooms[i].Triangles = new VSFace[so.Rooms[i].NumTriangles];
                for (int j = 0; j < so.Rooms[i].NumTriangles; j++)
                {
                    VSFace face = new VSFace();
                    face.verticesCount = 3;
                    face.type = 0x24;
                    face.side = 8;
                    face.vertices = new List<ushort>();
                    face.vertices.Add(buffer.ReadByte());
                    face.vertices.Add(buffer.ReadByte());
                    face.vertices.Add(buffer.ReadByte());
                    face.vertices.Add(buffer.ReadByte());
                    so.Rooms[i].Triangles[j] = face;
                }
                so.Rooms[i].NumQuads = buffer.ReadUInt32();
                so.Rooms[i].Quads = new VSFace[so.Rooms[i].NumQuads];
                for (int j = 0; j < so.Rooms[i].NumQuads; j++)
                {
                    VSFace face = new VSFace();
                    face.verticesCount = 4;
                    face.type = 0x2C;
                    face.side = 8;
                    face.vertices = new List<ushort>();
                    face.vertices.Add(buffer.ReadByte());
                    face.vertices.Add(buffer.ReadByte());
                    face.vertices.Add(buffer.ReadByte());
                    face.vertices.Add(buffer.ReadByte());
                    so.Rooms[i].Quads[j] = face;
                }

                so.Rooms[i].NumFloorLines = buffer.ReadUInt32();
                so.Rooms[i].FloorLines = new VSLine[so.Rooms[i].NumFloorLines];
                for (int j = 0; j < so.Rooms[i].NumFloorLines; j++)
                {
                    VSLine line = new VSLine();
                    line.verticesId[0] = buffer.ReadByte();
                    line.verticesId[1] = buffer.ReadByte();
                    line.pad = buffer.ReadUInt16();
                    so.Rooms[i].FloorLines[j] = line;
                }

                so.Rooms[i].NumCeilLines = buffer.ReadUInt32();
                so.Rooms[i].CeilLines = new VSLine[so.Rooms[i].NumCeilLines];
                for (int j = 0; j < so.Rooms[i].NumCeilLines; j++)
                {
                    VSLine line = new VSLine();
                    line.verticesId[0] = buffer.ReadByte();
                    line.verticesId[1] = buffer.ReadByte();
                    line.pad = buffer.ReadUInt16();
                    so.Rooms[i].CeilLines[j] = line;
                }

                so.Rooms[i].NumMarkers = buffer.ReadUInt32();
                so.Rooms[i].Markers = new ARMMarker[so.Rooms[i].NumMarkers];

                for (int j = 0; j < so.Rooms[i].NumMarkers; j++)
                {
                    so.Rooms[i].Markers[j] = new ARMMarker(buffer.ReadBytes(4));
                }
            }
        } 



        public void Parse()
        {
            if (UseDebug)
            {
                Debug.Log("ARM Parse : " + FilePath);
            }

            CoreParse();


            if (UseDebug)
            {
                Debug.Log(FileName + " n° of Rooms : " + so.NumRooms + " File Length : " + buffer.BaseStream.Length);
            }

            if (buffer.BaseStream.Position != buffer.BaseStream.Length) // snowfly forest rooms have no names
            {
                if (UseDebug)
                {
                    Debug.Log("Room info len : " + (buffer.BaseStream.Length - buffer.BaseStream.Position) / so.NumRooms);
                }

                for (int i = 0; i < so.NumRooms; i++)
                {
                    if (UseDebug)
                    {
                        Debug.Log(i + "  - Ptr : " + buffer.BaseStream.Position);
                    }

                    // The following lines of code work with the French version of Vagrant Story SLES 02755
                    if (buffer.BaseStream.Position + 36 <= buffer.BaseStream.Length)
                    {
                        int num1 = buffer.ReadInt16();
                        int num2 = buffer.ReadInt16();
                        int num3 = buffer.ReadInt16();
                        byte[] bn = buffer.ReadBytes(24);
                        string sname = "";
                        foreach (byte b in bn)
                        {
                            sname += VS.Utils.L10n.Charset(b);
                        }
                        string[] subs = sname.Split('|');

                        so.Rooms[i].name = subs[0];
                        int num4 = buffer.ReadInt16();
                        int num5 = buffer.ReadInt16();
                        int num6 = buffer.ReadInt16();
                        //Debug.Log("num1 : " + num1 + "  num2 : " + num2 + "  num3 : " + num3 + "  num4 : " + num4 + "  num5 : " + num5 + "  num5 : " + num6);
                        if (UseDebug)
                        {
                            Debug.Log(sname);
                        }
                    }
                }
            }

            buffer.Close();
            fileStream.Close();
            Parsed = true;

            ToolBox.DirExNorCreate("Assets/");
            ToolBox.DirExNorCreate("Assets/Resources/");
            ToolBox.DirExNorCreate("Assets/Resources/Serialized/");
            ToolBox.DirExNorCreate("Assets/Resources/Serialized/MiniMaps/");
            AssetDatabase.DeleteAsset("Assets/Resources/Serialized/MiniMaps/" + FileName + ".ARM.yaml.asset");
            AssetDatabase.CreateAsset(so, "Assets/Resources/Serialized/MiniMaps/" + FileName + ".ARM.yaml.asset");
            AssetDatabase.SaveAssets();
        }
        public GameObject BuildGameObject()
        {
            Build(false);
            return ARMGO;
        }
        public void BuildPrefab(bool erase = false)
        {
            string miniMapFolder = "Assets/Resources/Prefabs/MiniMaps/";
            ToolBox.DirExNorCreate(miniMapFolder);
            string miniMapFilename = miniMapFolder + FileName + ".prefab";
            if (erase)
            {
                AssetDatabase.DeleteAsset(miniMapFilename);
            }

            Build(true);
        }
        private void Build(bool buildPrefab = false)
        {
            string miniMapFolder = "Assets/Resources/Prefabs/MiniMaps/";
            string miniMapFilename = miniMapFolder + FileName + ".prefab";
            GameObject prefab;
            if (Parsed)
            {
                Material roomsMaterial = (Material)Resources.Load("Prefabs/ARMMaterial", typeof(Material));
                Material linesMaterial = (Material)Resources.Load("Prefabs/ARMLineMaterial", typeof(Material));
#if UNITY_EDITOR
                // Building ...
                if (!roomsMaterial)
                {
                    Texture2D alphaBlue = new Texture2D(1, 1);
                    alphaBlue.SetPixel(0, 0, new Color32(0x00, 0xA0, 0xFF, 128));
                    alphaBlue.Apply();
                    roomsMaterial = new Material(Shader.Find("Standard"));
                    roomsMaterial.SetTexture("_MainTex", alphaBlue);
                    AssetDatabase.CreateAsset(roomsMaterial, "Assets/Resources/Prefabs/ARMMaterial.mat");
                }
                if (!linesMaterial)
                {
                    linesMaterial = new Material(Shader.Find("Standard"));
                    linesMaterial.color = new Color32(0x00, 0xA0, 0xFF, 192);
                    AssetDatabase.CreateAsset(linesMaterial, "Assets/Resources/Prefabs/ARMLineMaterial.mat");
                }
#endif
                GameObject areaGo = new GameObject(FileName);
                areaGo.tag = "MiniMap";
                if (buildPrefab)
                {
                    prefab = PrefabUtility.SaveAsPrefabAsset(areaGo, miniMapFilename);
                }

                for (int i = 0; i < so.NumRooms; i++)
                {
                    List<VSFace> polygones = new List<VSFace>();
                    polygones.AddRange(so.Rooms[i].Triangles);
                    polygones.AddRange(so.Rooms[i].Quads);

                    Mesh roomMesh = BuildMesh(so.Rooms[i].name, so.Rooms[i].Vertices, polygones.ToArray());
                    GameObject meshGo = new GameObject(so.Rooms[i].name);

                    GameObject linesContainer = new GameObject("Lines");
                    linesContainer.transform.parent = meshGo.transform;
                    for (int j = 0; j < so.Rooms[i].NumFloorLines; j++)
                    {
                        GameObject lineGo = new GameObject("Floor Line " + j);
                        lineGo.transform.parent = linesContainer.transform;
                        LineRenderer lines = lineGo.AddComponent<LineRenderer>();
                        lines.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        lines.receiveShadows = false;
                        lines.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                        lines.material = linesMaterial;
                        lines.startWidth = 0.4f;
                        lines.endWidth = 0.4f;
                        lines.positionCount = 2;
                        lines.useWorldSpace = false;
                        lines.SetPositions(so.Rooms[i].GetLinePositions(j));
                    }

                    for (int j = 0; j < so.Rooms[i].NumCeilLines; j++)
                    {
                        GameObject lineGo = new GameObject("Ceil Line " + j);
                        lineGo.transform.parent = linesContainer.transform;
                        LineRenderer lines = lineGo.AddComponent<LineRenderer>();
                        lines.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        lines.receiveShadows = false;
                        lines.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                        lines.material = linesMaterial;
                        lines.startWidth = 0.2f;
                        lines.endWidth = 0.2f;
                        lines.positionCount = 2;
                        lines.useWorldSpace = false;
                        lines.SetPositions(so.Rooms[i].GetLinePositions(j, false));
                    }

                    for (int j = 0; j < so.Rooms[i].NumMarkers; j++)
                    {
                        GameObject markerGO = new GameObject();
                        Material mat = null; //(Material)Resources.Load("Prefabs/ARMMaterial", typeof(Material));

                        ARMMarker mark = so.Rooms[i].Markers[j];

                        if (mark.type == ARMMarker.MarkerType.DOOR)
                        {
                            markerGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            if (mark.lockId > 0)
                            {
                                markerGO.name = "Locked Door";
                                mat = (Material)Resources.Load("Prefabs/ARMRed", typeof(Material));
                            }
                            else
                            {
                                markerGO.name = "Door";
                                mat = (Material)Resources.Load("Prefabs/ARMWhite", typeof(Material));
                            }
                        }
                        if (mark.type == ARMMarker.MarkerType.CENTER)
                        {
                            markerGO.name = "Room Center";
                            // center of the room, usefull to place Ashley's position for an ingame map
                        }

                        if (mark.type == ARMMarker.MarkerType.SAVE_CONTAINER)
                        {
                            markerGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            markerGO.name = "SAVE_CONTAINER";
                            mat = (Material)Resources.Load("Prefabs/ARMRed", typeof(Material));
                        }



                        if (mark.type == ARMMarker.MarkerType.EXIT)
                        {
                            markerGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            markerGO.name = "Zone Exit";
                            mat = (Material)Resources.Load("Prefabs/ARMRed", typeof(Material));
                        }
                        if (mark.type == ARMMarker.MarkerType.SAVE || mark.type == ARMMarker.MarkerType.WORKSHOP || mark.type == ARMMarker.MarkerType.WORKSHOP2)
                        {
                            // these markers are also used as room center
                            markerGO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                            if (mark.type == ARMMarker.MarkerType.SAVE)
                            {
                                markerGO.name = "Save";
                                mat = (Material)Resources.Load("Prefabs/ARMLineMaterial", typeof(Material));
                            }
                            else if (mark.type == ARMMarker.MarkerType.WORKSHOP || mark.type == ARMMarker.MarkerType.WORKSHOP2)
                            {
                                markerGO.name = "Workshop";
                                mat = (Material)Resources.Load("Prefabs/ARMWhite", typeof(Material));
                            }
                        }

                        if (mark.type == ARMMarker.MarkerType.CONTAINER)
                        {
                            markerGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            markerGO.name = "Container";
                            mat = (Material)Resources.Load("Prefabs/ARMWhite", typeof(Material));
                        }

                        if (markerGO.GetComponent<MeshRenderer>() != null)
                        {
                            markerGO.GetComponent<MeshRenderer>().material = mat;
                        }
                        markerGO.transform.position = so.Rooms[i].Vertices[(int)mark.vertexId].position;
                        markerGO.transform.localScale = Vector3.one;
                        markerGO.transform.parent = meshGo.transform;
                    }

                    MeshFilter mf = meshGo.AddComponent<MeshFilter>();
                    mf.mesh = roomMesh;
                    MeshCollider mc = meshGo.AddComponent<MeshCollider>();
                    MeshRenderer mr = meshGo.AddComponent<MeshRenderer>();
                    mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    mr.receiveShadows = false;
                    mr.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                    mr.material = roomsMaterial;
                    //mr.sharedMaterial = roomsMaterial;
                    //roomMesh.RecalculateNormals();
                    meshGo.transform.parent = areaGo.transform;
#if UNITY_EDITOR
                    if (buildPrefab)
                    {
                        AssetDatabase.AddObjectToAsset(roomMesh, miniMapFilename);
                    }
#endif
                }
                if (buildPrefab)
                {
#if UNITY_EDITOR
                    PrefabUtility.SaveAsPrefabAsset(areaGo, miniMapFilename);
                    GameObject.DestroyImmediate(areaGo);
                    //PrefabUtility.ReplacePrefab(areaGo, prefab);
                    AssetDatabase.SaveAssets();
#endif
                }
                else
                {
                    ARMGO = areaGo;
                    GOBuilded = true;
                }
            }
        }


        private Mesh BuildMesh(string meshName, VSVertex[] vertices, VSFace[] faces)
        {
            Mesh mesh = new Mesh();
            List<Vector3> meshVertices = new List<Vector3>();
            List<int> meshTriangles = new List<int>();
            List<Vector2> meshTrianglesUV = new List<Vector2>();

            for (int i = 0; i < vertices.Length; i++)
            {
                meshVertices.Add(vertices[i].position);
                meshTrianglesUV.Add(new Vector2(0.5f, 0.5f));
            }

            for (int i = 0; i < faces.Length; i++)
            {
                if (faces[i].type == 0x2C)
                {
                    if (faces[i].side == 8)
                    {
                        meshTriangles.Add(faces[i].vertices[2]);
                        meshTriangles.Add(faces[i].vertices[1]);
                        meshTriangles.Add(faces[i].vertices[0]);

                        meshTriangles.Add(faces[i].vertices[0]);
                        meshTriangles.Add(faces[i].vertices[3]);
                        meshTriangles.Add(faces[i].vertices[2]);

                        meshTriangles.Add(faces[i].vertices[0]);
                        meshTriangles.Add(faces[i].vertices[1]);
                        meshTriangles.Add(faces[i].vertices[2]);

                        meshTriangles.Add(faces[i].vertices[2]);
                        meshTriangles.Add(faces[i].vertices[3]);
                        meshTriangles.Add(faces[i].vertices[0]);

                    }
                    else
                    {
                        meshTriangles.Add(faces[i].vertices[2]);
                        meshTriangles.Add(faces[i].vertices[1]);
                        meshTriangles.Add(faces[i].vertices[0]);

                        meshTriangles.Add(faces[i].vertices[0]);
                        meshTriangles.Add(faces[i].vertices[3]);
                        meshTriangles.Add(faces[i].vertices[2]);
                    }
                }
                else if (faces[i].type == 0x24)
                {
                    if (faces[i].side == 8)
                    {
                        meshTriangles.Add(faces[i].vertices[2]);
                        meshTriangles.Add(faces[i].vertices[1]);
                        meshTriangles.Add(faces[i].vertices[0]);

                        meshTriangles.Add(faces[i].vertices[0]);
                        meshTriangles.Add(faces[i].vertices[1]);
                        meshTriangles.Add(faces[i].vertices[2]);
                    }
                    else
                    {
                        meshTriangles.Add(faces[i].vertices[2]);
                        meshTriangles.Add(faces[i].vertices[1]);
                        meshTriangles.Add(faces[i].vertices[0]);
                    }
                }
            }
            mesh.name = meshName;
            mesh.vertices = meshVertices.ToArray();
            mesh.triangles = meshTriangles.ToArray();
            mesh.uv = meshTrianglesUV.ToArray();

            return mesh;
        }


    }

}

*/