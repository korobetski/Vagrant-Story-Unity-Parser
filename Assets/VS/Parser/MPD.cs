using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using VS.Entity;
using VS.Utils;

//http://datacrystal.romhacking.net/wiki/Vagrant_Story:MPD_files

namespace VS.Parser
{
    public class MPD : FileParser
    {
        public string filePath;
        public string mapName;
        public string zoneName;

        private uint ptrRoomSection;
        private uint lenRoomSection;
        private uint ptrClearedSection;
        private uint lenClearedSection;
        private uint ptrScriptSection;
        private uint lenScriptSection;
        private uint ptrDoorSection;
        private uint lenDoorSection;
        private uint ptrEnemySection;
        private uint lenEnemySection;
        private uint ptrTreasureSection;
        private uint lenTreasureSection;
        // One unknown section must be used for traps, one for crates
        private uint lenGeometrySection;
        private uint lenCollisionSection;
        private uint lenSubSection03;
        private uint lenRoomDoorSection;
        private uint lenLightingSection;
        private uint lenSubSection06;
        private uint lenSubSection07;
        private uint lenSubSection08;
        private uint lenSubSection09;
        private uint lenSubSection0A;
        private uint lenSubSection0B;
        private uint lenTextureEffectsSection;

        private uint lenSubSection0D;
        private uint lenSubSection0E;
        private uint lenSubSection0F;
        private uint lenSubSection10;
        private uint lenSubSection11;
        private uint lenSubSection12;
        private uint lenSubSection13;
        private uint lenAKAOSubSection;
        private uint lenSubSection15;
        private uint lenSubSection16;
        private uint lenSubSection17;
        private uint lenSubSection18;

        uint numGroups;
        MPDGroup[] groups;
        List<GameObject> lights;
        MPDDoor[] doors;

        GameObject MapGO;

        private bool _geom = true;

        public void Parse(string filePath)
        {
            PreParse(filePath);
            mapName = FileName + ".MPD";
            Parse(buffer);

            buffer.Close();
            fileStream.Close();
        }
        public void Parse(BinaryReader buffer)
        {
            ptrRoomSection = buffer.ReadUInt32();
            lenRoomSection = buffer.ReadUInt32();
            ptrClearedSection = buffer.ReadUInt32();
            lenClearedSection = buffer.ReadUInt32();
            ptrScriptSection = buffer.ReadUInt32();
            lenScriptSection = buffer.ReadUInt32();
            ptrDoorSection = buffer.ReadUInt32();
            lenDoorSection = buffer.ReadUInt32();
            ptrEnemySection = buffer.ReadUInt32();
            lenEnemySection = buffer.ReadUInt32();
            ptrTreasureSection = buffer.ReadUInt32();
            lenTreasureSection = buffer.ReadUInt32();
            // Room sub sections
            lenGeometrySection = buffer.ReadUInt32();
            lenCollisionSection = buffer.ReadUInt32();
            lenSubSection03 = buffer.ReadUInt32();
            lenRoomDoorSection = buffer.ReadUInt32();
            lenLightingSection = buffer.ReadUInt32();
            lenSubSection06 = buffer.ReadUInt32();
            lenSubSection07 = buffer.ReadUInt32();
            lenSubSection08 = buffer.ReadUInt32();
            lenSubSection09 = buffer.ReadUInt32();
            lenSubSection0A = buffer.ReadUInt32();
            lenSubSection0B = buffer.ReadUInt32();
            lenTextureEffectsSection = buffer.ReadUInt32();
            lenSubSection0D = buffer.ReadUInt32();
            lenSubSection0E = buffer.ReadUInt32();
            lenSubSection0F = buffer.ReadUInt32();
            lenSubSection10 = buffer.ReadUInt32();
            lenSubSection11 = buffer.ReadUInt32();
            lenSubSection12 = buffer.ReadUInt32();
            lenSubSection13 = buffer.ReadUInt32();
            lenAKAOSubSection = buffer.ReadUInt32();
            lenSubSection15 = buffer.ReadUInt32();
            lenSubSection16 = buffer.ReadUInt32();
            lenSubSection17 = buffer.ReadUInt32();
            lenSubSection18 = buffer.ReadUInt32();


            if (UseDebug)
            {
                Debug.Log("MPD parse : " + filePath);
                Debug.Log("ptrRoomSection :" + ptrRoomSection + "  lenRoomSection : " + lenRoomSection);
                Debug.Log("ptrClearedSection :" + ptrClearedSection + "  lenClearedSection : " + lenClearedSection);
                Debug.Log("ptrScriptSection :" + ptrScriptSection + "  lenScriptSection : " + lenScriptSection);
                Debug.Log("ptrDoorSection :" + ptrDoorSection + "  lenDoorSection : " + lenDoorSection);
                Debug.Log("ptrEnemySection :" + ptrEnemySection + "  lenEnemySection : " + lenEnemySection);
                Debug.Log("ptrTreasureSection :" + ptrTreasureSection + "  lenTreasureSection : " + lenTreasureSection);
                Debug.Log("lenGeometrySection :" + lenGeometrySection);
                Debug.Log("lenCollisionSection :" + lenCollisionSection);
                Debug.Log("lenSubSection03 :" + lenSubSection03);
                Debug.Log("lenRoomDoorSection :" + lenRoomDoorSection);
                Debug.Log("lenLightingSection :" + lenLightingSection);
                Debug.Log("lenSubSection06 :" + lenSubSection06);
                Debug.Log("lenSubSection07 :" + lenSubSection07);
                Debug.Log("lenSubSection08 :" + lenSubSection08);
                Debug.Log("lenSubSection09 :" + lenSubSection09);
                Debug.Log("lenSubSection0A :" + lenSubSection0A);
                Debug.Log("lenSubSection0B :" + lenSubSection0B);
                Debug.Log("lenTextureEffectsSection :" + lenTextureEffectsSection);
                Debug.Log("lenSubSection0D :" + lenSubSection0D);
                Debug.Log("lenSubSection0E :" + lenSubSection0E);
                Debug.Log("lenSubSection0F :" + lenSubSection0F);
                Debug.Log("lenSubSection10 :" + lenSubSection10);
                Debug.Log("lenSubSection11 :" + lenSubSection11);
                Debug.Log("lenSubSection12 :" + lenSubSection12);
                Debug.Log("lenSubSection13 :" + lenSubSection13);
                Debug.Log("lenAKAOSubSection :" + lenAKAOSubSection);
                Debug.Log("lenSubSection15 :" + lenSubSection15);
                Debug.Log("lenSubSection16 :" + lenSubSection16);
                Debug.Log("lenSubSection17 :" + lenSubSection17);
                Debug.Log("lenSubSection18 :" + lenSubSection18);
            }

            // ROOM section
            if (UseDebug)
            {
                Debug.Log("ROOM section : " + buffer.BaseStream.Position);
            }
            // Geometry
            if (lenRoomSection > 4)
            {
                if (lenGeometrySection > 0)
                {
                    numGroups = buffer.ReadUInt32();
                    if (UseDebug)
                    {
                        Debug.Log("numGroups : " + numGroups);
                    }
                    groups = new MPDGroup[numGroups];
                    for (uint i = 0; i < numGroups; i++)
                    {
                        groups[i] = new MPDGroup();
                        groups[i].header = buffer.ReadBytes(64);
                        if ((groups[i].header[1] & 0x08) > 0)
                        {
                            groups[i].scale = 1;
                        }
                    }
                    for (uint i = 0; i < numGroups; i++)
                    {
                        uint numTriangles = buffer.ReadUInt32();
                        uint numQuads = buffer.ReadUInt32();
                        for (uint j = 0; j < numTriangles; j++)
                        {
                            MPDFace face = new MPDFace(groups[i], false);
                            face.feed(buffer);
                            MPDMesh m = groups[i].getMesh(face.textureId, face.clutId);
                            m.addFace(face);
                        }
                        for (uint j = 0; j < numQuads; j++)
                        {
                            MPDFace face = new MPDFace(groups[i], true);
                            face.feed(buffer);
                            MPDMesh m = groups[i].getMesh(face.textureId, face.clutId);
                            m.addFace(face);
                        }
                    }
                }

                // collision
                if (lenCollisionSection > 0)
                {
                    long collisionPtr = buffer.BaseStream.Position;
                    uint TyleWidth = buffer.ReadUInt16();
                    uint TyleHeight = buffer.ReadUInt16();
                    if (UseDebug)
                    {
                        Debug.Log("TyleWidth : " + TyleWidth + "  TyleHeight : " + TyleHeight);
                    }

                    uint unk1 = buffer.ReadUInt16();
                    uint unk2 = buffer.ReadUInt16();
                    uint[] FloorHeight = new uint[TyleWidth * TyleHeight];
                    uint[] CeilingHeight = new uint[TyleWidth * TyleHeight];
                    uint[] Incline = new uint[TyleWidth * TyleHeight];
                    //Debug.Log("Collision ptr : " + buffer.BaseStream.Position);
                    for (uint i = 0; i < TyleWidth * TyleHeight; i++)
                    {
                        FloorHeight[i] = buffer.ReadUInt16();
                        if (UseDebug)
                        {
                            //Debug.Log("FloorHeight[i] : " + FloorHeight[i]);
                        }
                    }

                    for (uint i = 0; i < TyleWidth * TyleHeight; i++)
                    {
                        CeilingHeight[i] = buffer.ReadUInt16();
                        if (UseDebug)
                        {
                            //Debug.Log("CeilingHeight[i] : " + CeilingHeight[i]);
                        }
                    }

                    for (uint i = 0; i < TyleWidth * TyleHeight / 2; i++)
                    {
                        byte b = buffer.ReadByte();
                        Incline[i * 2] = (uint)b << 4;
                        Incline[i * 2 + 1] = (uint)b >> 4;
                    }
                    buffer.BaseStream.Position = collisionPtr + lenCollisionSection;
                }

                // section 3 ??
                //
                if (lenSubSection03 > 0)
                {
                    Debug.Log("lenSubSection03 ptr : " + buffer.BaseStream.Position + "   lenSubSection03 : " + lenSubSection03);
                    buffer.BaseStream.Position = buffer.BaseStream.Position + lenSubSection03;
                }

                // door section
                if (lenRoomDoorSection > 0)
                {
                    long doorPtr = buffer.BaseStream.Position;
                    uint numDoors = lenRoomDoorSection / 0x0C;
                    doors = new MPDDoor[numDoors];
                    for (uint i = 0; i < numDoors; i++)
                    {
                        MPDDoor d = new MPDDoor();
                        d.destZone = buffer.ReadByte();
                        d.destRoom = buffer.ReadByte();
                        d.unkn = buffer.ReadBytes(6);
                        d.idCurrentDoor = buffer.ReadUInt32();
                        doors[i] = d;
                        if (UseDebug)
                        {
                            Debug.Log("MPDDoor # " + i + "  idCurrentDoor : " + d.idCurrentDoor + "  destZone : " + d.destZone + "  destRoom : " + d.destRoom);
                            Debug.Log(d.unkn[0] + ", " + d.unkn[1] + ", " + d.unkn[2] + ", " + d.unkn[3] + ", " + d.unkn[4] + ", " + d.unkn[5]);
                        }
                    }

                    buffer.BaseStream.Position = doorPtr + lenRoomDoorSection;
                }

                // lights section
                // http://www.psxdev.net/forum/viewtopic.php?f=51&t=3383
                if (lenLightingSection > 0)
                {
                    long lightPtr = buffer.BaseStream.Position;
                    if (UseDebug)
                    {
                        Debug.Log("lights ptr : " + lightPtr + "   lenLightingSection : " + lenLightingSection + "(" + (lightPtr + lenLightingSection) + ")");
                    }

                    Color32 mc = new Color32(buffer.ReadByte(), buffer.ReadByte(), buffer.ReadByte(), buffer.ReadByte());
                    uint numLights = buffer.ReadUInt32();
                    if (UseDebug)
                    {
                        Debug.Log("numLights : " + numLights + "  mainColor  :  " + mc.ToString());
                    }

                    buffer.ReadUInt32(); // padding

                    lights = new List<GameObject>();

                    string lightsDebug = "";
                    for (uint i = 0; i < numLights; i++)
                    {
                        string lgtMat = "";
                        short[] matrix = new short[10];

                        byte[] hexa = buffer.ReadBytes(20);
                        buffer.BaseStream.Position -= 20;

                        for (uint j = 0; j < 10; j++)
                        {
                            matrix[j] = buffer.ReadInt16();
                            lgtMat += (matrix[j]) + " | ";
                        }


                        byte[] cols = buffer.ReadBytes(12);
                        Color32 colorX = new Color32(cols[0], cols[1], cols[2], cols[3]);
                        Color32 colorY = new Color32(cols[4], cols[5], cols[6], cols[7]);
                        Color32 colorZ = new Color32(cols[8], cols[9], cols[10], cols[11]);

                        Color32 main = Color.black;
                        if (colorX.r != mc.r && colorX.g != mc.g && colorX.b != mc.b)
                        {
                            main = colorX;
                        }
                        if (colorY.r != mc.r && colorY.g != mc.g && colorY.b != mc.b)
                        {
                            main = colorY;
                        }
                        if (colorZ.r != mc.r && colorZ.g != mc.g && colorZ.b != mc.b)
                        {
                            main = colorZ;
                        }
                        main.a = 255;

                        lightsDebug += string.Concat("Light # ", i, "  :  ", BitConverter.ToString(hexa), "  ->  ", lgtMat, "  |  ", colorX, ", ", colorY, ", ", colorZ, "\r\n");
                        GameObject lgo = new GameObject("Point Light");
                        Rect lightRect = new Rect();
                        lightRect.xMin = -matrix[0] / 100;
                        lightRect.yMin = -matrix[1] / 100;
                        lightRect.xMax = -matrix[2] / 100;
                        lightRect.yMax = -matrix[3] / 100;
                        // i need to find a way to get Y axe
                        lgo.transform.position = new Vector3(lightRect.center.x, 5f, lightRect.center.y);
                        lgo.transform.localScale = Vector3.one;
                        Light l = lgo.AddComponent<Light>();
                        l.name = "l" + i;
                        l.type = LightType.Point;
                        l.range = 10f;
                        l.intensity = 1.5f;
                        l.color = main;
                        l.shadows = LightShadows.Soft;
                        lights.Add(lgo);

                    }

                    if (UseDebug)
                    {
                        Debug.Log(lightsDebug);
                    }

                    if (UseDebug)
                    {
                        Debug.Log("end lights  :  " + buffer.BaseStream.Position);
                    }

                    buffer.BaseStream.Position = lightPtr + lenLightingSection;
                }

                if (lenSubSection06 > 0)
                {
                    Debug.Log("SubSection06 ptr : " + buffer.BaseStream.Position + "   lenSubSection06 : " + lenSubSection06);
                    buffer.BaseStream.Position = buffer.BaseStream.Position + lenSubSection06;
                }

                if (lenSubSection07 > 0)
                {
                    Debug.Log("SubSection07 ptr : " + buffer.BaseStream.Position + "   lenSubSection07 : " + lenSubSection07);
                    buffer.BaseStream.Position = buffer.BaseStream.Position + lenSubSection07;
                }

                if (lenSubSection08 > 0)
                {
                    Debug.Log("SubSection08 ptr : " + buffer.BaseStream.Position + "   lenSubSection08 : " + lenSubSection08);
                    buffer.BaseStream.Position = buffer.BaseStream.Position + lenSubSection08;
                }

                if (lenSubSection09 > 0)
                {
                    Debug.Log("SubSection09 ptr : " + buffer.BaseStream.Position + "   lenSubSection09 : " + lenSubSection09);
                    buffer.BaseStream.Position = buffer.BaseStream.Position + lenSubSection09;
                }

                if (lenSubSection0A > 0)
                {
                    Debug.Log("SubSection0A ptr : " + buffer.BaseStream.Position + "   lenSubSection0A : " + lenSubSection0A);
                    buffer.BaseStream.Position = buffer.BaseStream.Position + lenSubSection0A;
                }

                if (lenSubSection0B > 0)
                {
                    Debug.Log("SubSection0B ptr : " + buffer.BaseStream.Position + "   lenSubSection0B : " + lenSubSection0B);
                    buffer.BaseStream.Position = buffer.BaseStream.Position + lenSubSection0B;
                }

                if (lenTextureEffectsSection > 0)
                {
                    Debug.Log("TextureEffectsSection ptr : " + buffer.BaseStream.Position + "   lenTextureEffectsSection : " + lenTextureEffectsSection);
                    buffer.BaseStream.Position = buffer.BaseStream.Position + lenTextureEffectsSection;
                }

                if (lenSubSection0D > 0)
                {
                    Debug.Log("SubSection0D ptr : " + buffer.BaseStream.Position + "   lenSubSection0D : " + lenSubSection0D);
                    buffer.BaseStream.Position = buffer.BaseStream.Position + lenSubSection0D;
                }

                if (lenSubSection0E > 0)
                {
                    Debug.Log("SubSection0E ptr : " + buffer.BaseStream.Position + "   lenSubSection0E : " + lenSubSection0E);
                    buffer.BaseStream.Position = buffer.BaseStream.Position + lenSubSection0E;
                }

                if (lenSubSection0F > 0)
                {
                    Debug.Log("SubSection0F ptr : " + buffer.BaseStream.Position + "   lenSubSection0F : " + lenSubSection0F);
                    buffer.BaseStream.Position = buffer.BaseStream.Position + lenSubSection0F;
                }

                if (lenSubSection10 > 0)
                {
                    Debug.Log("SubSection10 ptr : " + buffer.BaseStream.Position + "   lenSubSection10 : " + lenSubSection10);
                    buffer.BaseStream.Position = buffer.BaseStream.Position + lenSubSection10;
                }

                if (lenSubSection11 > 0)
                {
                    Debug.Log("SubSection11 ptr : " + buffer.BaseStream.Position + "   lenSubSection11 : " + lenSubSection11);
                    buffer.BaseStream.Position = buffer.BaseStream.Position + lenSubSection11;
                }

                if (lenSubSection12 > 0)
                {
                    Debug.Log("SubSection12 ptr : " + buffer.BaseStream.Position + "   lenSubSection12 : " + lenSubSection12);
                    buffer.BaseStream.Position = buffer.BaseStream.Position + lenSubSection12;
                }

                if (lenSubSection13 > 0)
                {
                    Debug.Log("SubSection13 ptr : " + buffer.BaseStream.Position + "   lenSubSection13 : " + lenSubSection13);
                    buffer.BaseStream.Position = buffer.BaseStream.Position + lenSubSection13;
                }

                if (lenAKAOSubSection > 0)
                {
                    long akaoPtr = buffer.BaseStream.Position;
                    if (UseDebug)
                    {
                        Debug.Log("akaoPtr : " + akaoPtr + "   lenAKAOSubSection : " + lenAKAOSubSection + "(" + (akaoPtr + lenAKAOSubSection) + ")");
                    }

                    buffer.ReadUInt32(); // 0200 0000
                    buffer.ReadUInt32(); // 0000 0000
                    buffer.ReadUInt32(); // 0C00 0000


                    AKAO audio = new AKAO();
                    audio.FileName = FileName;
                    audio.UseDebug = true;
                    audio.Parse(buffer, AKAO.UNKNOWN, akaoPtr + lenAKAOSubSection);
                    buffer.BaseStream.Position = akaoPtr + lenAKAOSubSection;
                }

                if (lenSubSection15 > 0)
                {
                    Debug.Log("SubSection15 ptr : " + buffer.BaseStream.Position + "   lenSubSection15 : " + lenSubSection15);
                    buffer.BaseStream.Position = buffer.BaseStream.Position + lenSubSection15;
                }

                if (lenSubSection16 > 0)
                {
                    Debug.Log("SubSection16 ptr : " + buffer.BaseStream.Position + "   lenSubSection16 : " + lenSubSection16);
                    buffer.BaseStream.Position = buffer.BaseStream.Position + lenSubSection16;
                }

                if (lenSubSection17 > 0)
                {
                    Debug.Log("SubSection17 ptr : " + buffer.BaseStream.Position + "   lenSubSection17 : " + lenSubSection17);
                    buffer.BaseStream.Position = buffer.BaseStream.Position + lenSubSection17;
                }

                if (lenSubSection18 > 0)
                {
                    Debug.Log("SubSection18 ptr : " + buffer.BaseStream.Position + "   lenSubSection18 : " + lenSubSection18);
                    buffer.BaseStream.Position = buffer.BaseStream.Position + lenSubSection18;
                }
            }
            else
            {
                // No geometry :s
                _geom = false;
            }

            // Cleared section
            if (buffer.BaseStream.Position != ptrClearedSection)
            {
                buffer.BaseStream.Position = ptrClearedSection;
            }
            if (UseDebug)
            {
                Debug.Log("Cleared section : " + buffer.BaseStream.Position);
            }


            // Script section
            if (buffer.BaseStream.Position != ptrScriptSection)
            {
                buffer.BaseStream.Position = ptrScriptSection;
            }
            if (UseDebug)
            {
                Debug.Log("Script section : " + buffer.BaseStream.Position);
            }

            // See Opcode.cs


            // Door section

            if (buffer.BaseStream.Position != ptrDoorSection)
            {
                buffer.BaseStream.Position = ptrDoorSection;
            }
            if (UseDebug)
            {
                Debug.Log("Door section : " + buffer.BaseStream.Position);
            }
            if (lenDoorSection > 0)
            {
            }

            // Ennemy section
            if (buffer.BaseStream.Position != ptrEnemySection)
            {
                buffer.BaseStream.Position = ptrEnemySection;
            }
            if (UseDebug)
            {
                Debug.Log("Ennemy section : " + buffer.BaseStream.Position);
            }


            // Treasure section
            if (buffer.BaseStream.Position != ptrTreasureSection)
            {
                buffer.BaseStream.Position = ptrTreasureSection;
            }
            if (UseDebug)
            {
                Debug.Log("Treasure section : " + buffer.BaseStream.Position);
            }


            buffer.Close();

        }
        public GameObject BuildGameObject()
        {
            if (_geom)
            {
                Build(false);
                return MapGO;
            }
            else
            {
                return null;
            }
        }

        public void BuildPrefab(bool erase = false)
        {
            string zonesFolder = "Assets/Resources/Prefabs/Zones/";
            ToolBox.DirExNorCreate(zonesFolder);
            string zoneFolder = "Assets/Resources/Prefabs/Zones/" + ToolBox.MPDToZND(mapName, true) + "/";
            ToolBox.DirExNorCreate(zoneFolder);
            string zoneFilename = zoneFolder + FileName + ".prefab";

            if (erase)
            {
                AssetDatabase.DeleteAsset(zoneFilename);
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(zoneFilename);
            if (prefab == null)
            {
                Build(true);
            }
        }

        public void Build(bool buildPrefab = false)
        {
            // First we need to read the zone informations
            zoneName = ToolBox.MPDToZND(mapName);
            if (zoneName == null)
            {
                Debug.LogWarning(mapName + " has no zone attached !");
                return;
            }

            ZND zndParser = new ZND();
            zndParser.FileName = zoneName;
            string[] hash = FilePath.Split("/"[0]);
            hash[hash.Length - 1] = zoneName;
            zndParser.FilePath = String.Join("/", hash);
            zndParser.UseDebug = UseDebug;
            zndParser.Parse(zndParser.FilePath);

            string zn = ToolBox.MPDToZND(mapName, true);
            string zoneFolder = "Assets/Resources/Prefabs/Zones/" + zn + "/";
            string zoneFilename = zoneFolder + FileName + ".prefab";
            GameObject mapGO;

            if (buildPrefab)
            {
#if UNITY_EDITOR
                ToolBox.DirExNorCreate("Assets/Resources/Prefabs/Zones/");
                ToolBox.DirExNorCreate("Assets/Resources/Prefabs/Zones/" + zn + "/");
#endif
            }
            else
            {
            }

            mapGO = new GameObject(zn);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(mapGO, zoneFilename);

            // Then we can build the room
            MPDDatas datas = mapGO.AddComponent<MPDDatas>();
            ZNDDatas z = mapGO.AddComponent<ZNDDatas>();
            z.filePath = String.Join("/", hash);
            z.tims = zndParser.datas.tims;
            z.MPDInfos = zndParser.datas.MPDInfos;
            z.ZUDInfos = zndParser.datas.ZUDInfos;
            datas.doors = doors;
            for (uint i = 0; i < numGroups; i++)
            {
                GameObject groupGO = new GameObject("Group_" + i);
                groupGO.transform.parent = mapGO.transform;
                int lgm = groups[i].meshes.Count;
                for (int j = 0; j < lgm; j++)
                {
                    Mesh mesh = new Mesh();
                    List<Vector3> meshVertices = new List<Vector3>();
                    List<int> meshTriangles = new List<int>();
                    List<Vector2> meshTrianglesUV = new List<Vector2>();
                    List<Vector3> meshNormals = new List<Vector3>();
                    List<Color32> meshColors = new List<Color32>();
                    int iv = 0;
                    int lmf = groups[i].meshes[j].faces.Count;
                    for (int k = 0; k < lmf; k++)
                    {
                        MPDFace f = groups[i].meshes[j].faces[k];
                        if (f.isQuad)
                        {
                            meshVertices.Add(-f.v1.position / 100);
                            meshVertices.Add(-f.v2.position / 100);
                            meshVertices.Add(-f.v3.position / 100);
                            meshVertices.Add(-f.v4.position / 100);
                            meshColors.Add(f.v1.color);
                            meshColors.Add(f.v2.color);
                            meshColors.Add(f.v3.color);
                            meshColors.Add(f.v4.color);
                            meshTrianglesUV.Add(f.v2.uv / 256);
                            meshTrianglesUV.Add(f.v3.uv / 256);
                            meshTrianglesUV.Add(f.v1.uv / 256);
                            meshTrianglesUV.Add(f.v4.uv / 256);
                            meshNormals.Add(f.n);
                            meshNormals.Add(f.n);
                            meshNormals.Add(f.n);
                            meshNormals.Add(f.n);

                            meshTriangles.Add(iv + 0);
                            meshTriangles.Add(iv + 1);
                            meshTriangles.Add(iv + 2);

                            meshTriangles.Add(iv + 3);
                            meshTriangles.Add(iv + 2);
                            meshTriangles.Add(iv + 1);
                            iv += 4;
                        }
                        else
                        {
                            meshVertices.Add(-f.v1.position / 100);
                            meshVertices.Add(-f.v2.position / 100);
                            meshVertices.Add(-f.v3.position / 100);
                            meshColors.Add(f.v1.color);
                            meshColors.Add(f.v2.color);
                            meshColors.Add(f.v3.color);
                            meshTrianglesUV.Add(f.v2.uv / 256);
                            meshTrianglesUV.Add(f.v3.uv / 256);
                            meshTrianglesUV.Add(f.v1.uv / 256);
                            meshNormals.Add(f.n);
                            meshNormals.Add(f.n);
                            meshNormals.Add(f.n);
                            meshTriangles.Add(iv + 0);
                            meshTriangles.Add(iv + 1);
                            meshTriangles.Add(iv + 2);

                            iv += 3;
                        }
                    }
                    mesh.name = "mesh_" + i + "-" + j;
                    mesh.vertices = meshVertices.ToArray();
                    mesh.triangles = meshTriangles.ToArray();
                    mesh.uv = meshTrianglesUV.ToArray();
                    mesh.normals = meshNormals.ToArray();
                    mesh.colors32 = meshColors.ToArray();
                    mesh.RecalculateNormals();

                    GameObject meshGo = new GameObject("mesh_" + i + "-" + j);
                    meshGo.transform.parent = groupGO.transform;

                    MeshFilter mf = meshGo.AddComponent<MeshFilter>();
                    mf.mesh = mesh;

                    MeshRenderer mr = meshGo.AddComponent<MeshRenderer>();
                    if (z != null && z.tims.Length > 0)
                    {
                        mr.material = z.GetMaterial(groups[i].meshes[j].textureId, groups[i].meshes[j].clutId);
                    }
                    if (buildPrefab)
                    {
#if UNITY_EDITOR

                        AssetDatabase.AddObjectToAsset(mesh, zoneFilename);
                        if (!AssetDatabase.Contains(mr.sharedMaterial))
                        {
                            AssetDatabase.AddObjectToAsset(mr.sharedMaterial, zoneFilename);
                        }

                        if (!AssetDatabase.Contains(mr.sharedMaterial.mainTexture))
                        {
                            AssetDatabase.AddObjectToAsset(mr.sharedMaterial.mainTexture, zoneFilename);
                        }

#endif
                    }
                }

            }

            if (lights != null)
            {
                for (int i = 0; i < lights.Count; i++)
                {
                    lights[i].transform.parent = mapGO.transform;
                }

            }



            if (buildPrefab)
            {
#if UNITY_EDITOR

                prefab = PrefabUtility.SaveAsPrefabAsset(mapGO, zoneFilename);
                //PrefabUtility.ReplacePrefab(mapGO, prefab);
                AssetDatabase.SaveAssets();


                if (mapGO)
                {
                    GameObject.DestroyImmediate(mapGO);
                }

                if (mapGO)
                {
                    GameObject.Destroy(mapGO);
                }
#endif
            }
            else
            {
                MapGO = mapGO;
            }
        }
    }

    [Serializable]
    public class MPDDoor
    {
        public uint destZone;
        public uint destRoom;
        public byte[] unkn;
        public uint idCurrentDoor;

        public MPDDoor()
        {
        }
    }

    public class MPDGroup
    {
        public uint scale = 8;
        public byte[] header;
        public List<MPDMesh> meshes;

        public MPDGroup()
        {
            meshes = new List<MPDMesh>();
        }

        public MPDMesh getMesh(uint textureId, uint clutId)
        {
            string idx = textureId.ToString() + "_" + clutId.ToString();
            MPDMesh mesh = contains(idx);
            if (mesh == null)
            {
                mesh = new MPDMesh(idx, textureId, clutId, this);
                meshes.Add(mesh);
            }
            return mesh;
        }

        public MPDMesh contains(string idx)
        {
            int ml = meshes.Count;
            for (int i = 0; i < ml; i++)
            {
                if (meshes[i].idx == idx)
                {
                    return meshes[i];
                }
            }

            return null;
        }
    }
    public class MPDMesh
    {
        public string idx;
        public MPDGroup group;
        public uint textureId;
        public uint clutId;
        public List<MPDFace> faces;
        public Texture2D texture;

        public MPDMesh(string idx, uint textureId, uint clutId, MPDGroup group)
        {
            this.idx = idx;
            this.textureId = textureId;
            this.clutId = clutId;
            this.group = group;
            faces = new List<MPDFace>();
        }

        public void addFace(MPDFace face)
        {
            faces.Add(face);
        }
    }
    public class MPDVertex
    {
        public Vector3 position;
        public Color32 color;
        public Vector2 uv;

        public MPDVertex()
        {
            position = new Vector3();
            color = new Color32();
            uv = new Vector2();
        }
    }
    public class MPDFace
    {
        public MPDGroup group;
        public bool isQuad;
        public uint type;
        public uint clutId;
        public uint textureId;
        public MPDVertex v1 = new MPDVertex();
        public MPDVertex v2 = new MPDVertex();
        public MPDVertex v3 = new MPDVertex();
        public MPDVertex v4 = new MPDVertex();
        public Vector3 n;


        public MPDFace(MPDGroup group, bool isQuad)
        {
            this.group = group;
            this.isQuad = isQuad;
        }

        public void feed(BinaryReader buffer)
        {

            v1.position = new Vector3(buffer.ReadInt16(), buffer.ReadInt16(), buffer.ReadInt16());
            v2.position = new Vector3(buffer.ReadSByte(), buffer.ReadSByte(), buffer.ReadSByte());
            v3.position = new Vector3(buffer.ReadSByte(), buffer.ReadSByte(), buffer.ReadSByte());
            v1.color = new Color32(buffer.ReadByte(), buffer.ReadByte(), buffer.ReadByte(), 0);
            type = buffer.ReadByte();
            v2.color = new Color32(buffer.ReadByte(), buffer.ReadByte(), buffer.ReadByte(), 0);
            v1.uv.x = buffer.ReadByte();
            v3.color = new Color32(buffer.ReadByte(), buffer.ReadByte(), buffer.ReadByte(), 0);
            v1.uv.y = buffer.ReadByte();
            v2.uv = new Vector2(buffer.ReadByte(), buffer.ReadByte());

            clutId = buffer.ReadUInt16();
            v3.uv = new Vector2(buffer.ReadByte(), buffer.ReadByte());
            textureId = buffer.ReadUInt16();

            if (isQuad == true)
            {
                v4.position = new Vector3(buffer.ReadSByte(), buffer.ReadSByte(), buffer.ReadSByte());
                v4.uv.x = buffer.ReadByte();
                v4.color = new Color32(buffer.ReadByte(), buffer.ReadByte(), buffer.ReadByte(), 0);
                v4.uv.y = buffer.ReadByte();
                v4.position = v4.position * group.scale + v1.position;
            }
            v2.position = v2.position * group.scale + v1.position;
            v3.position = v3.position * group.scale + v1.position;
            Vector3 u = v2.position - v1.position;
            Vector3 v = v3.position - v1.position;
            n = new Vector3(u.y * v.z - u.z * v.y, u.z * v.x - u.x * v.z, u.x * v.y - u.y * v.x);
        }
    }

}
