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
        public bool Parsed = false;
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
                rooms[i].numDoors = buffer.ReadUInt32();
                rooms[i].doors = new List<VSDoor>();
                for (int j = 0; j < rooms[i].numDoors; j++)
                {
                    VSDoor door = new VSDoor();
                    door.vid = buffer.ReadByte();
                    door.exit = buffer.ReadByte();
                    door.info = (DoorInfo)buffer.ReadByte();
                    door.lid = buffer.ReadByte();
                    rooms[i].doors.Add(door);
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
                    dataScript.doors = rooms[i].doors.ToArray();

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

                    MeshFilter mf = meshGo.AddComponent<MeshFilter>();
                    mf.mesh = roomMesh;
                    //MeshCollider mc = meshGo.AddComponent<MeshCollider>();
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
        public uint numDoors = 0;
        public List<VSVertex> vertices = new List<VSVertex>();
        public List<VSFace> triangles = new List<VSFace>();
        public List<VSFace> quads = new List<VSFace>();
        public List<VSLine> floorLines = new List<VSLine>();
        public List<VSLine> wallLines = new List<VSLine>();
        public List<VSDoor> doors = new List<VSDoor>();
        public string name = "";

        public VSRoom() { }
    }
}
