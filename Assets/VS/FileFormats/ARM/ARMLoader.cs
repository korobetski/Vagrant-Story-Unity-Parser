using System.Collections.Generic;
using UnityEngine;
using VS.FileFormats.GEOM;
using VS.Utils;

namespace VS.FileFormats.ARM
{
    public class ARMLoader:MonoBehaviour
    {
        public ARM SerializedARM;

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
            Material roomsMaterial = (Material)Resources.Load("Prefabs/ARMMaterial", typeof(Material));
            Material linesMaterial = (Material)Resources.Load("Prefabs/ARMLineMaterial", typeof(Material));

            if (SerializedARM != null)
            {
                for (int i = 0; i < SerializedARM.NumRooms; i++)
                {
                    List<Face> polygones = new List<Face>();
                    polygones.AddRange(SerializedARM.Rooms[i].Triangles);
                    polygones.AddRange(SerializedARM.Rooms[i].Quads);

                    Mesh roomMesh = BuildMesh(SerializedARM.Rooms[i].name, SerializedARM.Rooms[i].NegateVertices(), polygones.ToArray());
                    GameObject meshGo = new GameObject(SerializedARM.Rooms[i].name);

                    GameObject linesContainer = new GameObject("Lines");
                    linesContainer.transform.parent = meshGo.transform;
                    for (int j = 0; j < SerializedARM.Rooms[i].NumFloorLines; j++)
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
                        lines.SetPositions(SerializedARM.Rooms[i].GetLinePositions(j));
                    }

                    for (int j = 0; j < SerializedARM.Rooms[i].NumCeilLines; j++)
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
                        lines.SetPositions(SerializedARM.Rooms[i].GetLinePositions(j, false));
                    }

                    for (int j = 0; j < SerializedARM.Rooms[i].NumMarkers; j++)
                    {
                        GameObject markerGO = null;
                        Material mat = null; //(Material)Resources.Load("Prefabs/ARMMaterial", typeof(Material));

                        ARMMarker mark = SerializedARM.Rooms[i].Markers[j];

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
                            markerGO = new GameObject("Room Center");
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
                        if (markerGO != null)
                        {
                            if (markerGO.GetComponent<MeshRenderer>() != null)
                            {
                                markerGO.GetComponent<MeshRenderer>().material = mat;
                            }
                            markerGO.transform.position = SerializedARM.Rooms[i].Vertices[(int)mark.vertexId].position;
                            markerGO.transform.localScale = Vector3.one;
                            markerGO.transform.parent = meshGo.transform;
                        }
                    }

                    MeshFilter mf = meshGo.AddComponent<MeshFilter>();
                    mf.mesh = roomMesh;
                    MeshRenderer mr = meshGo.AddComponent<MeshRenderer>();
                    mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    mr.receiveShadows = false;
                    mr.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                    mr.material = roomsMaterial;
                    //mr.sharedMaterial = roomsMaterial;
                    //roomMesh.RecalculateNormals();

                    meshGo.transform.parent = gameObject.transform;
                }

            }
        }

        private Mesh BuildMesh(string meshName, Vertex[] vertices, Face[] faces)
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
