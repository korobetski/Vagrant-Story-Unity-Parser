using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VS.Entity;
using VS.Utils;

//http://datacrystal.romhacking.net/wiki/Vagrant_Story:ARM_files
// MINI MAP File format in SMALL/ folder
// This File Format is near 100% explored

namespace VS.Parser
{
    public class ARM : FileParser
    {
        public bool GOBuilded = false;
        public bool PrefabBuilded = false;
        public GameObject ARMGO;

        private uint numRooms;
        private VSRoom[] rooms;

        public void Parse(string filePath)
        {
            PreParse(filePath);
            if (UseDebug)
            {
                Debug.Log("ARM Parse : " + FilePath);
            }

            numRooms = buffer.ReadUInt32();
            rooms = new VSRoom[numRooms];
            for (int i = 0; i < numRooms; i++)
            {
                VSRoom room = new VSRoom();
                room.name = "Room_" + i;
                room.u1 = buffer.ReadUInt32(); // ? (RAM only)
                room.mapLength = buffer.ReadUInt32(); // lenght of map graphics section (RAM: pointer to section)
                room.zoneNumber = buffer.ReadUInt16();
                room.mapNumber = buffer.ReadUInt16();
                rooms[i] = room;
            }

            for (int i = 0; i < numRooms; i++)
            {
                rooms[i].numVertices = buffer.ReadUInt32();
                rooms[i].vertices = new List<VSVertex>();
                for (int j = 0; j < rooms[i].numVertices; j++)
                {
                    VSVertex vertex = new VSVertex();
                    int x = -buffer.ReadInt16();
                    int y = -buffer.ReadInt16();
                    int z = -buffer.ReadInt16();
                    buffer.ReadInt16();
                    vertex.position = new Vector3(x, y, z);
                    rooms[i].vertices.Add(vertex);
                }

                rooms[i].numTriangles = buffer.ReadUInt32();
                rooms[i].triangles = new List<VSFace>();
                for (int j = 0; j < rooms[i].numTriangles; j++)
                {
                    VSFace face = new VSFace();
                    face.verticesCount = 3;
                    face.type = 0x24;
                    face.side = 8;
                    face.vertices = new List<int>();
                    face.vertices.Add(buffer.ReadByte());
                    face.vertices.Add(buffer.ReadByte());
                    face.vertices.Add(buffer.ReadByte());
                    face.vertices.Add(buffer.ReadByte());
                    rooms[i].triangles.Add(face);
                }
                rooms[i].numQuads = buffer.ReadUInt32();
                rooms[i].quads = new List<VSFace>();
                for (int j = 0; j < rooms[i].numQuads; j++)
                {
                    VSFace face = new VSFace();
                    face.verticesCount = 4;
                    face.type = 0x2C;
                    face.side = 8;
                    face.vertices = new List<int>();
                    face.vertices.Add(buffer.ReadByte());
                    face.vertices.Add(buffer.ReadByte());
                    face.vertices.Add(buffer.ReadByte());
                    face.vertices.Add(buffer.ReadByte());
                    rooms[i].quads.Add(face);
                }
                rooms[i].numFloorLines = buffer.ReadUInt32();
                rooms[i].floorLines = new List<VSLine>();
                for (int j = 0; j < rooms[i].numFloorLines; j++)
                {
                    VSLine line = new VSLine();
                    line.points = new List<VSVertex>();
                    line.points.Add(rooms[i].vertices[buffer.ReadByte()]);
                    line.points.Add(rooms[i].vertices[buffer.ReadByte()]);
                    buffer.ReadInt16();
                    rooms[i].floorLines.Add(line);
                }
                rooms[i].numWallLines = buffer.ReadUInt32();
                rooms[i].wallLines = new List<VSLine>();
                for (int j = 0; j < rooms[i].numWallLines; j++)
                {
                    VSLine line = new VSLine();
                    line.points = new List<VSVertex>();
                    line.points.Add(rooms[i].vertices[buffer.ReadByte()]);
                    line.points.Add(rooms[i].vertices[buffer.ReadByte()]);
                    buffer.ReadInt16();
                    rooms[i].wallLines.Add(line);
                }
                rooms[i].numMark = buffer.ReadUInt32();
                rooms[i].markers = new List<byte[]>();

                for (int j = 0; j < rooms[i].numMark; j++)
                {
                    rooms[i].markers.Add(buffer.ReadBytes(4));
                }
            }

            if (UseDebug)
            {
                Debug.Log(FileName + " n° of Rooms : " + numRooms + " File Length : " + buffer.BaseStream.Length);
            }

            if (buffer.BaseStream.Position != buffer.BaseStream.Length) // snowfly forest rooms have no names
            {
                if (UseDebug)
                {
                    Debug.Log("Room info len : " + (buffer.BaseStream.Length - buffer.BaseStream.Position) / numRooms);
                }

                for (int i = 0; i < numRooms; i++)
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
                            sname = sname + VS.Utils.L10n.Charset(b);
                        }
                        rooms[i].name = sname;
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

                for (int i = 0; i < numRooms; i++)
                {
                    List<VSFace> polygones = new List<VSFace>();
                    polygones.AddRange(rooms[i].triangles);
                    polygones.AddRange(rooms[i].quads);
                    Mesh roomMesh = BuildMesh(rooms[i].name, rooms[i].vertices.ToArray(), polygones.ToArray());
                    GameObject meshGo = new GameObject(rooms[i].name);
                    ARMRoom dataScript = meshGo.AddComponent<ARMRoom>();
                    dataScript.mapNumber = rooms[i].mapNumber;
                    dataScript.zoneNumber = rooms[i].zoneNumber;

                    /*
                    LineRenderer lines = meshGo.AddComponent<LineRenderer>();
                    lines.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    lines.receiveShadows = false;
                    lines.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                    lines.material = linesMaterial;
                    lines.startWidth = 0.25f;
                    lines.endWidth = 0.25f;
                    lines.positionCount = (int)(rooms[i].numFloorLines * 2 + rooms[i].numWallLines * 2);
                    //lines.SetVertexCount((int)(rooms[i].numFloorLines * 2 + rooms[i].numWallLines * 2));
                    lines.useWorldSpace = false;
                    Vector3[] linePos = new Vector3[(int)(rooms[i].numFloorLines * 2 + rooms[i].numWallLines * 2)];
                    for (int j = 0; j < rooms[i].numFloorLines; j++)
                    {
                        linePos[j * 2] = rooms[i].floorLines[j].points[0].position;
                        linePos[j * 2 + 1] = rooms[i].floorLines[j].points[1].position;
                    }

                    for (int j = 0; j < rooms[i].numWallLines; j++)
                    {
                        linePos[rooms[i].numFloorLines * 2 + j * 2] = rooms[i].wallLines[j].points[0].position;
                        linePos[rooms[i].numFloorLines * 2 + j * 2 + 1] = rooms[i].wallLines[j].points[1].position;
                    }
                    lines.SetPositions(linePos);
                    */


                    // Ugly script but nicer render, no choice since line component only work with continued lines.
                    GameObject linesContainer = new GameObject("Lines");
                    linesContainer.transform.parent = meshGo.transform;
                    for (int j = 0; j < rooms[i].numFloorLines; j++)
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
                        lines.SetPositions(new Vector3[] { rooms[i].floorLines[j].points[0].position, rooms[i].floorLines[j].points[1].position });
                    }
                    for (int j = 0; j < rooms[i].numWallLines; j++)
                    {
                        GameObject lineGo = new GameObject("Wall Line " + j);
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
                        lines.SetPositions(new Vector3[] { rooms[i].wallLines[j].points[0].position, rooms[i].wallLines[j].points[1].position });
                    }

                    /*
                    GameObject vertContainer = new GameObject("Vertices");
                    vertContainer.transform.parent = meshGo.transform;
                    for (int j = 0; j < rooms[i].vertices.Count; j++)
                    {
                        GameObject vertice = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        vertice.transform.position = rooms[i].vertices[j].position;
                        vertice.transform.localScale = Vector3.one/10;
                        vertice.transform.parent = vertContainer.transform;
                    }
                    */


                    for (int j = 0; j < rooms[i].numMark; j++)
                    {
                        GameObject markerGO = new GameObject();
                        ARMMarker mark = markerGO.AddComponent<ARMMarker>();
                        mark.SetDatas(rooms[i].markers[j]);

                        Material mat = null; //(Material)Resources.Load("Prefabs/ARMMaterial", typeof(Material));

                        if (mark.info == ARMMarker.MarkerType.door)
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
                        if (mark.info == ARMMarker.MarkerType.center)
                        {
                            markerGO.name = "Room Center";
                            // center of the room, usefull to place Ashley's position for an ingame map
                        }

                        if (mark.info == ARMMarker.MarkerType.unk12)
                        {
                            markerGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            markerGO.name = "unk12";
                            mat = (Material)Resources.Load("Prefabs/ARMRed", typeof(Material));
                        }



                        if (mark.info == ARMMarker.MarkerType.exit)
                        {
                            markerGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            markerGO.name = "Zone Exit";
                            mat = (Material)Resources.Load("Prefabs/ARMRed", typeof(Material));
                        }
                        if (mark.info == ARMMarker.MarkerType.save || mark.info == ARMMarker.MarkerType.workshop || mark.info == ARMMarker.MarkerType.reserve)
                        {
                            // these markers are also used as room center
                            markerGO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                            if (mark.info == ARMMarker.MarkerType.save)
                            {
                                markerGO.name = "Save";
                                mat = (Material)Resources.Load("Prefabs/ARMLineMaterial", typeof(Material));
                            }
                            else if (mark.info == ARMMarker.MarkerType.reserve)
                            {
                                markerGO.name = "Reserve & Save";
                                mat = (Material)Resources.Load("Prefabs/ARMLineMaterialSelected", typeof(Material));
                            }
                            else
                            {
                                markerGO.name = "Workshop";
                                mat = (Material)Resources.Load("Prefabs/ARMWhite", typeof(Material));
                            }
                        }

                        if (mark.info == ARMMarker.MarkerType.container)
                        {
                            markerGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            markerGO.name = "Container";
                            mat = (Material)Resources.Load("Prefabs/ARMWhite", typeof(Material));
                        }


                        // One more time ... because we erased this by changing the GO
                        if (markerGO.GetComponent<ARMMarker>() == null)
                        {
                            mark = markerGO.AddComponent<ARMMarker>();
                            mark.SetDatas(rooms[i].markers[j]);
                        }

                        if (markerGO.GetComponent<MeshRenderer>() != null)
                        {
                            markerGO.GetComponent<MeshRenderer>().material = mat;
                        }
                        markerGO.transform.position = rooms[i].vertices[(int)mark.vertexId].position;
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

    public class VSRoom
    {
        public uint u1 = 0;
        public uint mapLength = 0;
        public uint zoneNumber = 0;
        public uint mapNumber = 0;
        public uint numVertices = 0;
        public uint numTriangles = 0;
        public uint numQuads = 0;
        public uint numFloorLines = 0;
        public uint numWallLines = 0;
        public uint numMark = 0;
        public List<VSVertex> vertices = new List<VSVertex>();
        public List<VSFace> triangles = new List<VSFace>();
        public List<VSFace> quads = new List<VSFace>();
        public List<VSLine> floorLines = new List<VSLine>();
        public List<VSLine> wallLines = new List<VSLine>();
        public List<byte[]> markers = new List<byte[]>();
        public string name = "";

        public VSRoom() { }
    }
}
