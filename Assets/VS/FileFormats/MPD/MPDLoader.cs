using System.Collections.Generic;
using UnityEngine;
using VS.Core;
using VS.Utils;

namespace VS.FileFormats.MPD
{
    public class MPDLoader : MonoBehaviour
    {
        [SerializeField] private Shader _vColoredUnlitShader;
        public MPD SerializedMPD;
        public ZND.ZND SerializedZND;
        public bool generate_collision_mesh = true;

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
            if (SerializedMPD != null)
            {
                List<Material> materials = new List<Material>();
                // we need the associated ZND to get textures
                string zndFileName = ToolBox.MPDToZND(string.Concat(SerializedMPD.Filename, ".MPD"), false);
                if (zndFileName != null)
                {
                    SerializedZND = Resources.Load<ZND.ZND>(string.Concat("Serialized/ZND/", zndFileName, ".yaml.asset"));
                    if (SerializedZND == null)
                    {
                        // Corresponding serialized ZND not found, so we try to serialize
                        VSPConfig conf = Memory.LoadConfig();
                        string zndFilePath = string.Concat(conf.VSPath, "MAP/", zndFileName);
                        SerializedZND = ScriptableObject.CreateInstance<ZND.ZND>();
                        SerializedZND.ParseFromFile(zndFilePath);

                        ToolBox.SaveScriptableObject("Assets/Resources/Serialized/ZND/", SerializedZND.Filename + ".ZND.yaml.asset", SerializedZND, SerializedZND.TIMs);
                    }
                }

                // we create all needed materials
                if (SerializedZND != null && SerializedZND.TIMs.Length > 0)
                {
                    //Shader shader = Shader.Find("Shader Graphs/V Colored Unlit");
                    for (uint i = 0; i < SerializedMPD.materialRefs.Length; i++)
                    {
                        Material mat = new Material(_vColoredUnlitShader);
                        mat.name = SerializedMPD.materialRefs[i];
                        mat.SetTexture("_MainTex", SerializedZND.GetTexture(SerializedMPD.materialRefs[i]));
                        /*
                        mat.SetFloat("_Mode", 1);
                        mat.SetFloat("_ColorMode", 0);
                        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                        mat.SetInt("_ZWrite", 1);
                        mat.EnableKeyword("_ALPHATEST_ON");
                        mat.DisableKeyword("_ALPHABLEND_ON");
                        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        */
                        materials.Add(mat);
                    }
                }

                GameObject roomGO = new GameObject(string.Concat("Room_", SerializedMPD.Filename));
                roomGO.transform.parent = gameObject.transform;

                for (uint i = 0; i < SerializedMPD.numGroups; i++)
                {
                    List<Vector3> meshVertices = new List<Vector3>();
                    List<List<int>> subMeshTriangles = new List<List<int>>();
                    List<Vector2> meshTrianglesUV = new List<Vector2>();
                    List<Vector3> meshNormals = new List<Vector3>();
                    List<Color32> meshColors = new List<Color32>();
                    List<Material> meshMaterials = new List<Material>();


                    int iv = 0;
                    int lmf = SerializedMPD.groups[i].faces.Length;
                    for (int k = 0; k < lmf; k++)
                    {
                        MPDFace f = SerializedMPD.groups[i].faces[k];
                        if (!meshMaterials.Exists(x => x.name.Equals(f.materialRef)))
                        {
                            meshMaterials.Add(materials.Find(x => x.name.Equals(f.materialRef)));
                            subMeshTriangles.Add(new List<int>());
                        }

                        int subMeshIndex = meshMaterials.IndexOf(meshMaterials.Find(x => x.name.Equals(f.materialRef)));
                        if (f.translucent)
                        {
                            meshMaterials[subMeshIndex].SetFloat("_Mode", 3);
                        }
                        if (f.doubleSided)
                        {
                            meshMaterials[subMeshIndex].SetFloat("_Cull", 0);
                        }

                        if (f.isQuad)
                        {
                            meshVertices.Add(f.GetOpVertex(SerializedMPD.groups[i], 0) / 128);
                            meshVertices.Add(f.GetOpVertex(SerializedMPD.groups[i], 1) / 128);
                            meshVertices.Add(f.GetOpVertex(SerializedMPD.groups[i], 2) / 128);
                            meshVertices.Add(f.GetOpVertex(SerializedMPD.groups[i], 3) / 128);
                            meshColors.Add(f.colors[0]);
                            meshColors.Add(f.colors[1]);
                            meshColors.Add(f.colors[2]);
                            meshColors.Add(f.colors[3]);
                            meshTrianglesUV.Add(f.uvs[1] / 256);
                            meshTrianglesUV.Add(f.uvs[2] / 256);
                            meshTrianglesUV.Add(f.uvs[0] / 256);
                            meshTrianglesUV.Add(f.uvs[3] / 256);

                            //meshNormals.Add(f.n);

                            subMeshTriangles[subMeshIndex].Add(iv + 0);
                            subMeshTriangles[subMeshIndex].Add(iv + 1);
                            subMeshTriangles[subMeshIndex].Add(iv + 2);

                            subMeshTriangles[subMeshIndex].Add(iv + 1);
                            subMeshTriangles[subMeshIndex].Add(iv + 3);
                            subMeshTriangles[subMeshIndex].Add(iv + 2);

                            iv += 4;
                        }
                        else
                        {
                            meshVertices.Add(f.GetOpVertex(SerializedMPD.groups[i], 2) / 128);
                            meshVertices.Add(f.GetOpVertex(SerializedMPD.groups[i], 1) / 128);
                            meshVertices.Add(f.GetOpVertex(SerializedMPD.groups[i], 0) / 128);
                            meshColors.Add(f.colors[2]);
                            meshColors.Add(f.colors[1]);
                            meshColors.Add(f.colors[0]);
                            meshTrianglesUV.Add(f.uvs[0] / 256);
                            meshTrianglesUV.Add(f.uvs[2] / 256);
                            meshTrianglesUV.Add(f.uvs[1] / 256);

                            //meshNormals.Add(f.n);
                            //meshNormals.Add(f.n);
                            //meshNormals.Add(f.n);

                            subMeshTriangles[subMeshIndex].Add(iv + 2);
                            subMeshTriangles[subMeshIndex].Add(iv + 1);
                            subMeshTriangles[subMeshIndex].Add(iv + 0);
                            iv += 3;
                        }
                    }
                    GameObject groupGO = new GameObject("Group_" + i);
                    groupGO.transform.parent = roomGO.transform;
                    // we apply group position for no-static groups like doors, floating stones, chest top, billboard effects etc...
                    // by doing so we can apply rotations
                    groupGO.transform.position = SerializedMPD.groups[i].positionYInv/128;

                    Mesh mesh = new Mesh();
                    mesh.name = "mesh_" + i;
                    mesh.vertices = meshVertices.ToArray();
                    //mesh.triangles = meshTriangles.ToArray();


                    mesh.subMeshCount = subMeshTriangles.Count;
                    for (int j = 0; j < subMeshTriangles.Count; j++)
                    {
                        mesh.SetTriangles(subMeshTriangles[j].ToArray(), j);
                    }

                    mesh.uv = meshTrianglesUV.ToArray();
                    mesh.RecalculateNormals();
                    mesh.colors32 = meshColors.ToArray();

                    MeshFilter mf = groupGO.AddComponent<MeshFilter>();
                    mf.mesh = mesh;

                    MeshRenderer mr = groupGO.AddComponent<MeshRenderer>();
                    mr.materials = meshMaterials.ToArray();
                }



                // Collision mesh
                if (generate_collision_mesh)
                {
                    GameObject collisionGO = new GameObject("Floor Collision");
                    collisionGO.transform.parent = gameObject.transform;
                    int v0, v1, v2, v3, v4, v5;
                    List<Vector3> colliVertices = new List<Vector3>();
                    List<int> colliTriangles = new List<int>();

                    for (uint y = 0; y < SerializedMPD.tileHeigth; y++)
                    {
                        for (uint x = 0; x < SerializedMPD.tileWidth; x++)
                        {
                            v0= v1= v2= v3= v4= v5 = 0;
                            uint k = y * SerializedMPD.tileWidth + x;
                            MPDTile tile = SerializedMPD.tiles[k];
                            tile.id = k;
                            tile.y = y;
                            tile.x = x;
                            MPDTileMode floorMode = SerializedMPD.tileModes[tile.floorMode];

                            float z = (float)tile.floorHeight / 16;

                            Vector3[] vertices;

                            if (floorMode.mode == MPDTileMode.Mode.DIAG0 || floorMode.mode == MPDTileMode.Mode.DIAG1 || floorMode.mode == MPDTileMode.Mode.DIAG2 || floorMode.mode == MPDTileMode.Mode.DIAG3)
                            {
                                // two triangles at different height, so we need 6 vertices
                                vertices = new Vector3[6];
                                float dec = floorMode.to / 16;
                                switch (floorMode.mode)
                                {
                                    case MPDTileMode.Mode.DIAG0:
                                        v0 = GetVertexIndex(new Vector3(x, z, y), colliVertices);
                                        v1 = GetVertexIndex(new Vector3(x + 1, z, y), colliVertices);
                                        v2 = GetVertexIndex(new Vector3(x, z + dec, y + 1), colliVertices);
                                        v3 = GetVertexIndex(new Vector3(x, z + dec, y), colliVertices);
                                        v4 = GetVertexIndex(new Vector3(x + 1, z, y + 1), colliVertices);
                                        v5 = GetVertexIndex(new Vector3(x + 1, z + dec, y + 1), colliVertices);

                                        colliTriangles.AddRange(new int[] { v4, v1, v0 });
                                        colliTriangles.AddRange(new int[] { v5, v3, v2 });
                                        colliTriangles.AddRange(new int[] { v0, v3, v5 });
                                        colliTriangles.AddRange(new int[] { v0, v5, v4 });
                                        break;
                                    case MPDTileMode.Mode.DIAG1:
                                        v0 = GetVertexIndex(new Vector3(x, z, y), colliVertices);
                                        v1 = GetVertexIndex(new Vector3(x + 1, z, y), colliVertices);
                                        v2 = GetVertexIndex(new Vector3(x, z, y + 1), colliVertices);
                                        v3 = GetVertexIndex(new Vector3(x + 1, z + dec, y), colliVertices);
                                        v4 = GetVertexIndex(new Vector3(x, z + dec, y + 1), colliVertices);
                                        v5 = GetVertexIndex(new Vector3(x + 1, z + dec, y + 1), colliVertices);

                                        colliTriangles.AddRange(new int[] { v2, v1, v0 });
                                        colliTriangles.AddRange(new int[] { v5, v3, v4 });
                                        colliTriangles.AddRange(new int[] { v1, v2, v3 });
                                        colliTriangles.AddRange(new int[] { v3, v2, v4 });
                                        break;
                                    case MPDTileMode.Mode.DIAG2:
                                        v0 = GetVertexIndex(new Vector3(x, z + dec, y), colliVertices);
                                        v1 = GetVertexIndex(new Vector3(x + 1, z + dec, y), colliVertices);
                                        v2 = GetVertexIndex(new Vector3(x, z, y + 1), colliVertices);
                                        v3 = GetVertexIndex(new Vector3(x, z, y), colliVertices);
                                        v4 = GetVertexIndex(new Vector3(x + 1, z + dec, y + 1), colliVertices);
                                        v5 = GetVertexIndex(new Vector3(x + 1, z, y + 1), colliVertices);

                                        colliTriangles.AddRange(new int[] { v4, v1, v0 });
                                        colliTriangles.AddRange(new int[] { v5, v3, v2 });
                                        colliTriangles.AddRange(new int[] { v0, v3, v5 });
                                        colliTriangles.AddRange(new int[] { v0, v5, v4 });
                                        break;
                                    case MPDTileMode.Mode.DIAG3:
                                        v0 = GetVertexIndex(new Vector3(x, z + dec, y), colliVertices);
                                        v1 = GetVertexIndex(new Vector3(x + 1, z + dec, y), colliVertices);
                                        v2 = GetVertexIndex(new Vector3(x, z + dec, y + 1), colliVertices);
                                        v3 = GetVertexIndex(new Vector3(x + 1, z, y), colliVertices);
                                        v4 = GetVertexIndex(new Vector3(x, z, y + 1), colliVertices);
                                        v5 = GetVertexIndex(new Vector3(x + 1, z, y + 1), colliVertices);

                                        colliTriangles.AddRange(new int[] { v2, v1, v0 });
                                        colliTriangles.AddRange(new int[] { v5, v3, v4 });
                                        colliTriangles.AddRange(new int[] { v1, v2, v3 });
                                        colliTriangles.AddRange(new int[] { v3, v2, v4 });
                                        break;
                                }
                            }
                            else
                            {
                                float dec;
                                switch (floorMode.mode)
                                {
                                    case MPDTileMode.Mode.FLAT:
                                        dec = (float)floorMode.from/16;
                                        v0 = GetVertexIndex(new Vector3(x, z + dec, y), colliVertices);
                                        v1 = GetVertexIndex(new Vector3(x + 1, z + dec, y), colliVertices);
                                        v2 = GetVertexIndex(new Vector3(x, z + dec, y + 1), colliVertices);
                                        v3 = GetVertexIndex(new Vector3(x + 1, z + dec, y + 1), colliVertices);
                                        break;
                                    case MPDTileMode.Mode.DECIMAL:
                                        v0 = GetVertexIndex(new Vector3(x, z + 0.5f, y), colliVertices);
                                        v1 = GetVertexIndex(new Vector3(x + 1, z + 0.5f, y), colliVertices);
                                        v2 = GetVertexIndex(new Vector3(x, z + 0.5f, y + 1), colliVertices);
                                        v3 = GetVertexIndex(new Vector3(x + 1, z + 0.5f, y + 1), colliVertices);
                                        break;
                                    case MPDTileMode.Mode.RAMPXP:
                                        dec = (float)(floorMode.to - floorMode.from) / 6;
                                        v0 = GetVertexIndex(new Vector3(x, z, y), colliVertices);
                                        v1 = GetVertexIndex(new Vector3(x + 1, z + dec, y), colliVertices);
                                        v2 = GetVertexIndex(new Vector3(x, z, y + 1), colliVertices);
                                        v3 = GetVertexIndex(new Vector3(x + 1, z + dec, y + 1), colliVertices);
                                        break;
                                    case MPDTileMode.Mode.RAMPXN:
                                        dec = (float)(floorMode.from - floorMode.to) / 6;
                                        v0 = GetVertexIndex(new Vector3(x, z + dec, y), colliVertices);
                                        v1 = GetVertexIndex(new Vector3(x + 1, z, y), colliVertices);
                                        v2 = GetVertexIndex(new Vector3(x, z + dec, y + 1), colliVertices);
                                        v3 = GetVertexIndex(new Vector3(x + 1, z, y + 1), colliVertices);
                                        break;
                                    case MPDTileMode.Mode.RAMPYP:
                                        dec = (float)(floorMode.from - floorMode.to) / 6;
                                        v0 = GetVertexIndex(new Vector3(x, z + dec, y), colliVertices);
                                        v1 = GetVertexIndex(new Vector3(x + 1, z + dec, y), colliVertices);
                                        v2 = GetVertexIndex(new Vector3(x, z, y + 1), colliVertices);
                                        v3 = GetVertexIndex(new Vector3(x + 1, z, y + 1), colliVertices);
                                        break;
                                    case MPDTileMode.Mode.RAMPYN:
                                        dec = (float)(floorMode.to - floorMode.from) / 6;
                                        v0 = GetVertexIndex(new Vector3(x, z, y), colliVertices);
                                        v1 = GetVertexIndex(new Vector3(x + 1, z, y), colliVertices);
                                        v2 = GetVertexIndex(new Vector3(x, z + dec, y + 1), colliVertices);
                                        v3 = GetVertexIndex(new Vector3(x + 1, z + dec, y + 1), colliVertices);
                                        break;
                                    default:
                                        v0 = GetVertexIndex(new Vector3(x, z, y), colliVertices);
                                        v1 = GetVertexIndex(new Vector3(x + 1, z, y), colliVertices);
                                        v2 = GetVertexIndex(new Vector3(x, z, y + 1), colliVertices);
                                        v3 = GetVertexIndex(new Vector3(x + 1, z, y + 1), colliVertices);
                                        break;

                                }
                                colliTriangles.AddRange(new int[] { v2, v1, v0 });
                                colliTriangles.AddRange(new int[] { v2, v3, v1 });
                                if (v4 != 0)
                                {
                                    tile.heigths = new List<float>()
                                    {
                                        colliVertices[v0].y,
                                        colliVertices[v1].y,
                                        colliVertices[v2].y,
                                        colliVertices[v3].y,
                                        colliVertices[v4].y,
                                        colliVertices[v5].y,
                                    };
                                } else
                                {
                                    tile.heigths = new List<float>()
                                    {
                                        colliVertices[v0].y,
                                        colliVertices[v1].y,
                                        colliVertices[v2].y,
                                        colliVertices[v3].y,
                                    };
                                }
                            }

                            // pillars (connections faces between tiles of different heigth)
                            // TODO : we need to manage DIAG pillars
                            if (y > 0 && y < SerializedMPD.tileHeigth)
                            {
                                uint south = (y-1) * SerializedMPD.tileWidth + x;
                                MPDTile southTile = SerializedMPD.tiles[south];
                                if (tile.heigths[0] != southTile.heigths[2])
                                {
                                    colliTriangles.AddRange(new int[] {
                                        v0,
                                        v1,
                                        GetVertexIndex(new Vector3(x, southTile.heigths[2], y), colliVertices)
                                    });
                                }

                                if (tile.heigths[1] != southTile.heigths[3])
                                {
                                    colliTriangles.AddRange(new int[] {
                                        v1,
                                        GetVertexIndex(new Vector3(x+1, southTile.heigths[3], y), colliVertices),
                                        GetVertexIndex(new Vector3(x, southTile.heigths[2], y), colliVertices)
                                    });
                                }
                            }
                            if (x > 0 && x < SerializedMPD.tileWidth)
                            {
                                uint east = y * SerializedMPD.tileWidth + x - 1;
                                MPDTile eastTile = SerializedMPD.tiles[east];
                                if (tile.heigths[0] != eastTile.heigths[1])
                                {
                                    colliTriangles.AddRange(new int[] {
                                        GetVertexIndex(new Vector3(x, eastTile.heigths[1], y), colliVertices),
                                        v2,
                                        v0
                                    });
                                }

                                if (tile.heigths[2] != eastTile.heigths[3])
                                {
                                    colliTriangles.AddRange(new int[] {
                                        GetVertexIndex(new Vector3(x, eastTile.heigths[1], y), colliVertices),
                                        GetVertexIndex(new Vector3(x, eastTile.heigths[3], y+1),
                                        colliVertices),
                                        v2
                                    });
                                }
                            }
                        }
                    }




                    Mesh colliMesh = new Mesh();
                    colliMesh.name = "floor_collision";
                    colliMesh.vertices = colliVertices.ToArray();
                    colliMesh.triangles = colliTriangles.ToArray();

                    MeshFilter mf = collisionGO.AddComponent<MeshFilter>();
                    mf.mesh = colliMesh;

                    MeshCollider mc = collisionGO.AddComponent<MeshCollider>();




                    GameObject collisionGO2 = new GameObject("Ceil Collision");
                    collisionGO2.transform.parent = gameObject.transform;

                    colliVertices = new List<Vector3>();
                    colliTriangles = new List<int>();

                    for (uint y = 0; y < SerializedMPD.tileHeigth; y++)
                    {
                        for (uint x = 0; x < SerializedMPD.tileWidth; x++)
                        {
                            uint k = y * SerializedMPD.tileWidth + x;
                            MPDTile tile = SerializedMPD.tiles[k];
                            MPDTileMode ceilMode = SerializedMPD.tileModes[tile.ceilMode];

                            float z = (float)tile.ceilHeight / 16;
                            float dec;
                            switch (ceilMode.mode)
                            {
                                case MPDTileMode.Mode.DECIMAL:
                                    v0 = GetVertexIndex(new Vector3(x, z + 0.5f, y), colliVertices);
                                    v1 = GetVertexIndex(new Vector3(x + 1, z + 0.5f, y), colliVertices);
                                    v2 = GetVertexIndex(new Vector3(x, z + 0.5f, y + 1), colliVertices);
                                    v3 = GetVertexIndex(new Vector3(x + 1, z + 0.5f, y + 1), colliVertices);
                                    break;
                                case MPDTileMode.Mode.RAMPXP:
                                    dec = (float)(ceilMode.to - ceilMode.from) / 6;
                                    v0 = GetVertexIndex(new Vector3(x, z, y), colliVertices);
                                    v1 = GetVertexIndex(new Vector3(x + 1, z + dec, y), colliVertices);
                                    v2 = GetVertexIndex(new Vector3(x, z, y + 1), colliVertices);
                                    v3 = GetVertexIndex(new Vector3(x + 1, z + dec, y + 1), colliVertices);
                                    break;
                                case MPDTileMode.Mode.RAMPXN:
                                    dec = (float)(ceilMode.from - ceilMode.to) / 6;
                                    v0 = GetVertexIndex(new Vector3(x, z + dec, y), colliVertices);
                                    v1 = GetVertexIndex(new Vector3(x + 1, z, y), colliVertices);
                                    v2 = GetVertexIndex(new Vector3(x, z + dec, y + 1), colliVertices);
                                    v3 = GetVertexIndex(new Vector3(x + 1, z, y + 1), colliVertices);
                                    break;
                                case MPDTileMode.Mode.RAMPYP:
                                    dec = (float)(ceilMode.from - ceilMode.to) / 6;
                                    v0 = GetVertexIndex(new Vector3(x, z + dec, y), colliVertices);
                                    v1 = GetVertexIndex(new Vector3(x + 1, z + dec, y), colliVertices);
                                    v2 = GetVertexIndex(new Vector3(x, z, y + 1), colliVertices);
                                    v3 = GetVertexIndex(new Vector3(x + 1, z, y + 1), colliVertices);
                                    break;
                                case MPDTileMode.Mode.RAMPYN:
                                    dec = (float)(ceilMode.to - ceilMode.from) / 6;
                                    v0 = GetVertexIndex(new Vector3(x, z, y), colliVertices);
                                    v1 = GetVertexIndex(new Vector3(x + 1, z, y), colliVertices);
                                    v2 = GetVertexIndex(new Vector3(x, z + dec, y + 1), colliVertices);
                                    v3 = GetVertexIndex(new Vector3(x + 1, z + dec, y + 1), colliVertices);
                                    break;
                                default:
                                    v0 = GetVertexIndex(new Vector3(x, z, y), colliVertices);
                                    v1 = GetVertexIndex(new Vector3(x + 1, z, y), colliVertices);
                                    v2 = GetVertexIndex(new Vector3(x, z, y + 1), colliVertices);
                                    v3 = GetVertexIndex(new Vector3(x + 1, z, y + 1), colliVertices);
                                    break;

                            }
                            colliTriangles.AddRange(new int[] { v0, v1, v2 });
                            colliTriangles.AddRange(new int[] { v1, v3, v2 });

                                tile.ceilHeigths = new List<float>()
                                    {
                                        colliVertices[v0].y,
                                        colliVertices[v1].y,
                                        colliVertices[v2].y,
                                        colliVertices[v3].y,
                                    };

                            // pillars
                            if (y > 0 && y < SerializedMPD.tileHeigth)
                            {
                                uint south = (y-1) * SerializedMPD.tileWidth + x;
                                MPDTile southTile = SerializedMPD.tiles[south];
                                if (tile.ceilHeigths[0] != southTile.ceilHeigths[2])
                                {
                                    colliTriangles.AddRange(new int[] {
                                        GetVertexIndex(new Vector3(x, southTile.ceilHeigths[2], y), colliVertices),
                                        v1,
                                        v0
                                    });
                                }

                                if (tile.ceilHeigths[1] != southTile.ceilHeigths[3])
                                {
                                    colliTriangles.AddRange(new int[] {
                                        GetVertexIndex(new Vector3(x, southTile.ceilHeigths[2], y), colliVertices),
                                        GetVertexIndex(new Vector3(x+1, southTile.ceilHeigths[3], y), colliVertices),
                                        v1
                                    });
                                }
                            }
                            if (x > 0 && x < SerializedMPD.tileWidth)
                            {
                                uint east = k- 1;
                                MPDTile eastTile = SerializedMPD.tiles[east];

                                if (tile.ceilHeigths[0] != eastTile.ceilHeigths[1])
                                {
                                    colliTriangles.AddRange(new int[] {
                                        v0,
                                        v2,
                                        GetVertexIndex(new Vector3(x, eastTile.ceilHeigths[1], y), colliVertices)
                                    });
                                }

                                if (tile.ceilHeigths[2] != eastTile.ceilHeigths[3])
                                {
                                    colliTriangles.AddRange(new int[] {
                                        v2,
                                        GetVertexIndex(new Vector3(x, eastTile.ceilHeigths[3], y+1), colliVertices),
                                        GetVertexIndex(new Vector3(x, eastTile.ceilHeigths[1], y), colliVertices)
                                    });
                                }
                            }
                        }
                    }

                    Mesh colliMesh2 = new Mesh();
                    colliMesh2.name = "ceil_collision";
                    colliMesh2.vertices = colliVertices.ToArray();
                    colliMesh2.triangles = colliTriangles.ToArray();

                    MeshFilter mf2 = collisionGO2.AddComponent<MeshFilter>();
                    mf2.mesh = colliMesh2;

                    MeshCollider mc2 = collisionGO2.AddComponent<MeshCollider>();
                } 
            }
        }
    
        private int GetVertexIndex(Vector3 vertex, List<Vector3> verticesList)
        {
            if (verticesList.Contains(vertex))
            {
                return verticesList.IndexOf(vertex);
            } else
            {
                verticesList.Add(vertex);
                return verticesList.Count - 1;
            }
        }
    }
}
