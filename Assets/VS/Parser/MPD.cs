/*
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using VS.Serializable;
using VS.Utils;

//http://datacrystal.romhacking.net/wiki/Vagrant_Story:MPD_files

namespace VS.Parser
{
    public class MPD : FileParser
    {
        public string filePath;
        public string mapName;
        public string zoneName;
        GameObject MapGO;
        public Serializable.MPD so;
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

            so = ScriptableObject.CreateInstance<Serializable.MPD>();

            so.ptrRoomSection = buffer.ReadUInt32();
            so.lenRoomSection = buffer.ReadUInt32();
            so.ptrClearedSection = buffer.ReadUInt32();
            so.lenClearedSection = buffer.ReadUInt32();
            so.ptrScriptSection = buffer.ReadUInt32();
            so.lenScriptSection = buffer.ReadUInt32();
            so.ptrDoorSection = buffer.ReadUInt32();
            so.lenDoorSection = buffer.ReadUInt32();
            so.ptrEnemySection = buffer.ReadUInt32();
            so.lenEnemySection = buffer.ReadUInt32();
            so.ptrTreasureSection = buffer.ReadUInt32();
            so.lenTreasureSection = buffer.ReadUInt32();

            // Room sub sections
            if (so.lenRoomSection >= 96)
            {
                so.lenGeometrySection = buffer.ReadUInt32();
                so.lenCollisionSection = buffer.ReadUInt32();
                so.lenTilePropertiesSection = buffer.ReadUInt32();
                so.lenRoomDoorSection = buffer.ReadUInt32();
                so.lenLightingSection = buffer.ReadUInt32();
                so.lenSubSection06 = buffer.ReadUInt32();
                so.lenSubSection07 = buffer.ReadUInt32();
                so.lenSubSection08 = buffer.ReadUInt32();
                so.lenTrapSection = buffer.ReadUInt32();
                so.lenSubSection0A = buffer.ReadUInt32();
                so.lenSubSection0B = buffer.ReadUInt32();
                so.lenTextureEffectsSection = buffer.ReadUInt32();
                so.lenSubSection0D = buffer.ReadUInt32();
                so.lenSubSection0E = buffer.ReadUInt32();
                so.lenMiniMapSection = buffer.ReadUInt32();
                so.lenSubSection10 = buffer.ReadUInt32();
                so.lenSubSection11 = buffer.ReadUInt32();
                so.lenFloatingStoneSection = buffer.ReadUInt32();
                so.lenChestInteractionSection = buffer.ReadUInt32();
                so.lenAKAOSection = buffer.ReadUInt32();
                so.lenSubSection15 = buffer.ReadUInt32();
                so.lenSubSection16 = buffer.ReadUInt32();
                so.lenSubSection17 = buffer.ReadUInt32();
                so.lenCameraAreaSection = buffer.ReadUInt32();
            }
            else
            {
                buffer.ReadBytes((int)so.lenRoomSection);
            }

            // ROOM section
            if (UseDebug)
            {
                Debug.Log("ROOM section : " + buffer.BaseStream.Position);
            }
            // Geometry
            if (so.lenRoomSection > 0x40)
            {
                if (so.lenGeometrySection > 0)
                {
                    so.numGroups = buffer.ReadUInt32();
                    so.groups = new MPDGroup[so.numGroups];
                    for (uint i = 0; i < so.numGroups; i++)
                    {
                        so.groups[i] = new MPDGroup();
                        //groups[i].header = buffer.ReadBytes(64);
                        so.groups[i].visibility = buffer.ReadByte();
                        so.groups[i].scaleFlag = buffer.ReadByte();
                        so.groups[i].overlapping = buffer.ReadUInt16();
                        so.groups[i].decX = buffer.ReadInt16();
                        so.groups[i].unk1 = buffer.ReadUInt16();
                        so.groups[i].decY = buffer.ReadInt16();
                        so.groups[i].unk2 = buffer.ReadUInt16();
                        so.groups[i].decZ = buffer.ReadInt16();
                        so.groups[i].unk3 = buffer.ReadUInt16();

                        so.groups[i].unkBytes = buffer.ReadBytes(48);


                        if ((so.groups[i].scaleFlag & 0x08) > 0)
                        {
                            so.groups[i].scale = 1;
                        }
                    }
                    for (uint i = 0; i < so.numGroups; i++)
                    {
                        so.groups[i].numTriangles = buffer.ReadUInt32();
                        so.groups[i].numQuads = buffer.ReadUInt32();
                        so.groups[i].faces = new MPDFace[so.groups[i].numTriangles + so.groups[i].numQuads];
                        
                        for (uint j = 0; j < so.groups[i].numTriangles + so.groups[i].numQuads; j++)
                        {
                            MPDFace face = new MPDFace(buffer);
                            so.groups[i].faces[j] = face;
                        }
                    }
                }

                // collision
                if (so.lenCollisionSection > 0)
                {
                    long collisionPtr = buffer.BaseStream.Position;

                    so.tileWidth = buffer.ReadUInt16();
                    so.tileHeigth = buffer.ReadUInt16();
                    so.unk1 = buffer.ReadUInt16();
                    so.numTileModes = buffer.ReadUInt16();
                    so.tiles = new MPDTile[so.tileWidth * so.tileHeigth];
                    for (uint i = 0; i < so.tileWidth * so.tileHeigth; i++)
                    {
                        MPDTile tile = new MPDTile();
                        tile.floorHeight = buffer.ReadByte();
                        tile.floorMode = buffer.ReadByte();
                        so.tiles[i] = tile;
                    }

                    for (uint i = 0; i < so.tileWidth * so.tileHeigth; i++)
                    {
                        so.tiles[i].ceilHeight = buffer.ReadByte();
                        so.tiles[i].ceilMode = buffer.ReadByte();
                    }

                    so.tileModes = new MPDTileMode[so.numTileModes];
                    for (uint i = 0; i < so.numTileModes; i++)
                    {
                        so.tileModes[i] = new MPDTileMode();
                        so.tileModes[i].datas = buffer.ReadBytes(16);
                    }

                    if (buffer.BaseStream.Position != collisionPtr + so.lenCollisionSection)
                    {
                        Debug.LogWarning("Wrong position after collision");
                        buffer.BaseStream.Position = collisionPtr + so.lenCollisionSection;
                    }
                }

                // tile properties
                //
                if (so.lenTilePropertiesSection > 0)
                {
                    long tilePropsPtr = buffer.BaseStream.Position;

                    int loop = so.tileWidth * so.tileHeigth;
                    for (uint i = 0; i < loop; i++)
                    {
                        so.tiles[i].properties = buffer.ReadBytes(4);
                    }

                    if (buffer.BaseStream.Position != tilePropsPtr + so.lenTilePropertiesSection)
                    {
                        Debug.LogWarning("Wrong position after tileProps");
                        buffer.BaseStream.Position = tilePropsPtr + so.lenTilePropertiesSection;
                    }
                }

                // door section
                if (so.lenRoomDoorSection > 0)
                {
                    long doorPtr = buffer.BaseStream.Position;
                    so.numRoomDoors = so.lenRoomDoorSection / 0x0C;
                    so.roomDoors = new MPDRoomDoor[so.numRoomDoors];
                    for (uint i = 0; i < so.numRoomDoors; i++)
                    {
                        MPDRoomDoor door = new MPDRoomDoor();
                        door.zoneId = buffer.ReadByte();
                        door.roomId = buffer.ReadByte();
                        door.tileIndex = buffer.ReadUInt16();
                        door.destination = new ushort[] { buffer.ReadUInt16(), buffer.ReadUInt16() };
                        door.doorId = buffer.ReadUInt32();
                        so.roomDoors[i] = door;
                    }

                    if (buffer.BaseStream.Position != doorPtr + so.lenRoomDoorSection)
                    {
                        Debug.LogWarning("Wrong position after roomDoor");
                        buffer.BaseStream.Position = doorPtr + so.lenRoomDoorSection;
                    }
                }

                // lights section
                // http://www.psxdev.net/forum/viewtopic.php?f=51&t=3383
                if (so.lenLightingSection > 0)
                {
                    long lightPtr = buffer.BaseStream.Position;
                    so.numLights = (so.lenLightingSection - 12) / 32;
                    so.lights = new MPDLight[so.numLights];
                    so.ambientLight = new MPDLight();
                    byte[] cols = buffer.ReadBytes(12);
                    so.ambientLight.colors[0] = new Color32(cols[0], cols[1], cols[2], cols[3]);
                    so.ambientLight.colors[1] = new Color32(cols[4], cols[5], cols[6], cols[7]);
                    so.ambientLight.colors[2] = new Color32(cols[8], cols[9], cols[10], cols[11]);

                    for (uint i = 0; i < so.numLights; i++)
                    {
                        for (uint j = 0; j < 10; j++)
                        {
                            so.lights[i] = new MPDLight();
                            so.lights[i].datas[j] = buffer.ReadInt16();
                        }

                        cols = buffer.ReadBytes(12);
                        so.lights[i].colors[0] = new Color32(cols[0], cols[1], cols[2], cols[3]);
                        so.lights[i].colors[1] = new Color32(cols[4], cols[5], cols[6], cols[7]);
                        so.lights[i].colors[2] = new Color32(cols[8], cols[9], cols[10], cols[11]);

                        
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
                        l.range = Vector2.Distance(lightRect.center, lightRect.min);
                        l.intensity = 2f;
                        l.color = main;
                        l.shadows = LightShadows.Soft;
                        lights.Add(lgo);
                        
                    }

                    if (buffer.BaseStream.Position != lightPtr + so.lenLightingSection)
                    {
                        Debug.LogWarning("Wrong position after light");
                        buffer.BaseStream.Position = lightPtr + so.lenLightingSection;
                    }
                }

                if (so.lenSubSection06 > 0)
                {
                    so.SubSection06 = buffer.ReadBytes((int)so.lenSubSection06);
                }

                if (so.lenSubSection07 > 0)
                {
                    so.SubSection07 = buffer.ReadBytes((int)so.lenSubSection07);
                }

                if (so.lenSubSection08 > 0)
                {
                    so.SubSection08 = buffer.ReadBytes((int)so.lenSubSection08);
                }

                if (so.lenTrapSection > 0)
                {
                    long trapPtr = buffer.BaseStream.Position;

                    uint numTraps = (uint)(so.lenTrapSection / 12);
                    so.traps = new MPDTrap[numTraps];
                    for (uint i = 0; i < numTraps; i++)
                    {
                        so.traps[i] = new MPDTrap();
                        so.traps[i].position = new Vector2(buffer.ReadUInt16(), buffer.ReadUInt16()); // grid based
                        ushort p = buffer.ReadUInt16(); // padding
                        so.traps[i].skillId = buffer.ReadUInt16(); // http://datacrystal.romhacking.net/wiki/Vagrant_Story:skills_list
                        so.traps[i].unk1 = buffer.ReadByte();
                        so.traps[i].save = buffer.ReadByte();
                        so.traps[i].saveIndex = buffer.ReadByte();
                        so.traps[i].zero = buffer.ReadByte();
                    }

                    if (buffer.BaseStream.Position != trapPtr + so.lenTrapSection)
                    {
                        Debug.LogWarning("Wrong position after traps");
                        buffer.BaseStream.Position = trapPtr + so.lenTrapSection;
                    }
                }

                if (so.lenSubSection0A > 0)
                {
                    so.SubSection0A = buffer.ReadBytes((int)so.lenSubSection0A);
                }

                if (so.lenSubSection0B > 0)
                {
                    so.SubSection0B = buffer.ReadBytes((int)so.lenSubSection0B);
                }

                if (so.lenTextureEffectsSection > 0)
                {
                    long ptrTextureEffectsSection = buffer.BaseStream.Position;

                    List<MPDTextureAnimation> texAnims = new List<MPDTextureAnimation>();
                    while(buffer.BaseStream.Position+20 <= (ptrTextureEffectsSection + so.lenTextureEffectsSection))
                    {
                        byte[] texAnimHead = buffer.ReadBytes(8);
                        buffer.BaseStream.Position -= 8;
                        MPDTextureAnimation textureAnimation = new MPDTextureAnimation();
                        textureAnimation.type = texAnimHead[6];
                        switch (texAnimHead[6])
                        {
                            case 1:
                                textureAnimation.datas = buffer.ReadBytes(20);
                                break;
                            case 2:
                                textureAnimation.datas = buffer.ReadBytes(32);
                                break;
                            case 6:
                                textureAnimation.datas = buffer.ReadBytes(36);
                                break;
                            default:
                                Debug.Log("Unknown Texture animation type : " + textureAnimation.type+"  in file : "+FileName);
                                break;
                        }
                        texAnims.Add(textureAnimation);
                    }

                    so.textureAnimations = texAnims.ToArray();

                    if (buffer.BaseStream.Position != ptrTextureEffectsSection + so.lenTextureEffectsSection)
                    {
                        Debug.LogWarning("Wrong position after TextureEffectsSection " + "  in file : " + FileName);
                        buffer.BaseStream.Position = ptrTextureEffectsSection + so.lenTextureEffectsSection;
                    }
                }

                if (so.lenSubSection0D > 0)
                {
                    so.SubSection0D = buffer.ReadBytes((int)so.lenSubSection0D);
                }

                if (so.lenSubSection0E > 0)
                {
                    so.SubSection0E = buffer.ReadBytes((int)so.lenSubSection0E);
                }

                if (so.lenMiniMapSection > 0)
                {
                    long ptrMiniMapSection = buffer.BaseStream.Position;

                    // ARM Format
                    ARM armParser = new ARM();
                    armParser.buffer = this.buffer;
                    armParser.CoreParse();

                    ToolBox.DirExNorCreate("Assets/");
                    ToolBox.DirExNorCreate("Assets/Resources/");
                    ToolBox.DirExNorCreate("Assets/Resources/Serialized/");
                    ToolBox.DirExNorCreate("Assets/Resources/Serialized/MiniMaps/");
                    AssetDatabase.DeleteAsset("Assets/Resources/Serialized/MiniMaps/" + FileName + ".MPD.yaml.asset");
                    AssetDatabase.CreateAsset(armParser.so, "Assets/Resources/Serialized/MiniMaps/" + FileName + ".MPD.yaml.asset");
                    AssetDatabase.SaveAssets();

                    so.miniMap = armParser.so;

                    if (buffer.BaseStream.Position != ptrMiniMapSection + so.lenMiniMapSection)
                    {
                        Debug.LogWarning("Wrong position after MiniMapSection " + "  in file : " + FileName);
                        buffer.BaseStream.Position = ptrMiniMapSection + so.lenMiniMapSection;
                    }
                }

                if (so.lenSubSection10 > 0)
                {
                    so.SubSection10 = buffer.ReadBytes((int)so.lenSubSection10);
                }

                if (so.lenSubSection11 > 0)
                {
                    so.SubSection11 = buffer.ReadBytes((int)so.lenSubSection11);
                }

                if (so.lenFloatingStoneSection > 0)
                {
                    so.FloatingStoneSection = buffer.ReadBytes((int)so.lenFloatingStoneSection);
                }

                if (so.lenChestInteractionSection > 0)
                {
                    so.ChestInteractionSection = buffer.ReadBytes((int)so.lenChestInteractionSection);
                }

                if (so.lenAKAOSection > 0)
                {
                    so.AKAOSection = buffer.ReadBytes((int)so.lenAKAOSection);
                }

                if (so.lenSubSection15 > 0)
                {
                    so.SubSection15 = buffer.ReadBytes((int)so.lenSubSection15);
                }

                if (so.lenSubSection16 > 0)
                {
                    so.SubSection16 = buffer.ReadBytes((int)so.lenSubSection16);
                }

                if (so.lenSubSection17 > 0)
                {
                    so.SubSection17 = buffer.ReadBytes((int)so.lenSubSection17);
                }

                if (so.lenCameraAreaSection > 0)
                {
                    so.CameraAreaSection = new ushort[] { buffer.ReadUInt16(), buffer.ReadUInt16() , buffer.ReadUInt16() , buffer.ReadUInt16() };
                }
            }
            else
            {
                // No geometry :s
                _geom = false;
            }

            // Cleared section
            if (buffer.BaseStream.Position != so.ptrClearedSection)
            {
                buffer.BaseStream.Position = so.ptrClearedSection;
            }
            so.clearedSection = buffer.ReadBytes((int)so.lenClearedSection);

            // Script section
            if (buffer.BaseStream.Position != so.ptrScriptSection)
            {
                buffer.BaseStream.Position = so.ptrScriptSection;
            }

            if (so.lenScriptSection > 0)
            {
                // EVT format
                //so.scriptSection = buffer.ReadBytes((int)so.lenScriptSection);
                EVT evtParser = new EVT();
                evtParser.buffer = this.buffer;
                evtParser.CoreParse();

                ToolBox.DirExNorCreate("Assets/");
                ToolBox.DirExNorCreate("Assets/Resources/");
                ToolBox.DirExNorCreate("Assets/Resources/Serialized/");
                ToolBox.DirExNorCreate("Assets/Resources/Serialized/Events/");
                AssetDatabase.DeleteAsset("Assets/Resources/Serialized/Events/" + FileName + ".MPD.yaml.asset");
                AssetDatabase.CreateAsset(evtParser.so, "Assets/Resources/Serialized/Events/" + FileName + ".MPD.yaml.asset");
                AssetDatabase.SaveAssets();

                so.scriptSection = evtParser.so;

            }


            // Door section
            if (buffer.BaseStream.Position != so.ptrDoorSection)
            {
                buffer.BaseStream.Position = so.ptrDoorSection;
            }
            so.doorSection = buffer.ReadBytes((int)so.lenDoorSection);


            // Ennemy section
            if (buffer.BaseStream.Position != so.ptrEnemySection)
            {
                buffer.BaseStream.Position = so.ptrEnemySection;
            }
            if (so.lenEnemySection >= 40)
            {
                uint numEnemy = so.lenEnemySection / 40;
                List<byte[]> enemyDatas = new List<byte[]>();
                for (uint i = 0; i < numEnemy; i++)
                {
                    enemyDatas.Add(buffer.ReadBytes(40));
                }
                so.enemySection = enemyDatas.ToArray();
            }



            // Treasure section
            if (buffer.BaseStream.Position != so.ptrTreasureSection)
            {
                buffer.BaseStream.Position = so.ptrTreasureSection;
            }
            //so.treasureSection = buffer.ReadBytes((int)so.lenTreasureSection);

            if (so.lenTreasureSection > 0)
            {
                // we need this to get item names
                ItemList itemSO = Resources.Load("Serialized/Datas/ITEM.BIN.yaml") as ItemList;


                Treasure treasureSo = ScriptableObject.CreateInstance<Treasure>();

                so.treasureSection = treasureSo;
                Weapon weapon = new Weapon();
                weapon.blade = new Blade().SetDatasFromMPD(buffer);
                weapon.blade.name = itemSO.GetName(weapon.blade.nameId);
                weapon.blade.description = itemSO.GetDescription(weapon.blade.nameId);
                weapon.grip = new Grip().SetDatasFromMPD(buffer);
                weapon.grip.name = itemSO.GetName(weapon.grip.nameId);
                weapon.grip.description = itemSO.GetDescription(weapon.grip.nameId);
                for (uint i = 0; i < 3; i++)
                {
                    weapon.gems[i] = new Gem().SetDatasFromMPD(buffer);
                    weapon.gems[i].name = itemSO.GetName(weapon.gems[i].nameId);
                    weapon.gems[i].description = itemSO.GetDescription(weapon.gems[i].nameId);
                }
                weapon.name = Utils.L10n.CleanTranslate(buffer.ReadBytes(18));
                so.treasureSection.weapon = weapon;
                so.treasureSection.blade = new Blade().SetDatasFromMPD(buffer);
                so.treasureSection.blade.name = itemSO.GetName(so.treasureSection.blade.nameId);
                so.treasureSection.blade.description = itemSO.GetDescription(so.treasureSection.blade.nameId);
                so.treasureSection.grip = new Grip().SetDatasFromMPD(buffer);
                so.treasureSection.grip.name = itemSO.GetName(so.treasureSection.grip.nameId);
                so.treasureSection.grip.description = itemSO.GetDescription(so.treasureSection.grip.nameId);
                so.treasureSection.shield = new Shield().SetDatasFromMPD(buffer);
                so.treasureSection.shield.name = itemSO.GetName(so.treasureSection.shield.nameId);
                for (uint i = 0; i < 3; i++)
                {
                    so.treasureSection.shield.gems[i] = new Gem().SetDatasFromMPD(buffer);
                    so.treasureSection.shield.gems[i].name = itemSO.GetName(so.treasureSection.shield.gems[i].nameId);
                    so.treasureSection.shield.gems[i].description = itemSO.GetDescription(so.treasureSection.shield.gems[i].nameId);
                }
                so.treasureSection.armor1 = new Armor().SetDatasFromMPD(buffer);
                so.treasureSection.armor1.name = itemSO.GetName(so.treasureSection.armor1.nameId);
                so.treasureSection.armor2 = new Armor().SetDatasFromMPD(buffer);
                so.treasureSection.armor2.name = itemSO.GetName(so.treasureSection.armor2.nameId);
                so.treasureSection.accessory = new Armor().SetDatasFromMPD(buffer);
                so.treasureSection.accessory.name = itemSO.GetName(so.treasureSection.accessory.nameId);
                so.treasureSection.accessory.description = itemSO.GetDescription(so.treasureSection.accessory.nameId);
                so.treasureSection.gem = new Gem().SetDatasFromMPD(buffer);
                so.treasureSection.gem.name = itemSO.GetName(so.treasureSection.gem.nameId);
                so.treasureSection.gem.description = itemSO.GetDescription(so.treasureSection.gem.nameId);
                for (uint i = 0; i < 4; i++)
                {
                    so.treasureSection.miscItems[i] = new MiscItem().SetDatasFromMPD(buffer);
                    so.treasureSection.miscItems[i].name = itemSO.GetName(so.treasureSection.miscItems[i].nameId);
                    so.treasureSection.miscItems[i].description = itemSO.GetDescription(so.treasureSection.miscItems[i].nameId);
                }

                ToolBox.DirExNorCreate("Assets/");
                ToolBox.DirExNorCreate("Assets/Resources/");
                ToolBox.DirExNorCreate("Assets/Resources/Serialized/");
                ToolBox.DirExNorCreate("Assets/Resources/Serialized/Maps/");
                ToolBox.DirExNorCreate("Assets/Resources/Serialized/Maps/Treasures/");
                AssetDatabase.DeleteAsset("Assets/Resources/Serialized/Maps/Treasures/" + FileName + ".CHEST.yaml.asset");
                AssetDatabase.CreateAsset(treasureSo, "Assets/Resources/Serialized/Maps/Treasures/" + FileName + ".CHEST.yaml.asset");
                AssetDatabase.SaveAssets();
            }


            ToolBox.DirExNorCreate("Assets/");
            ToolBox.DirExNorCreate("Assets/Resources/");
            ToolBox.DirExNorCreate("Assets/Resources/Serialized/");
            ToolBox.DirExNorCreate("Assets/Resources/Serialized/Maps/");
            AssetDatabase.DeleteAsset("Assets/Resources/Serialized/Maps/" + FileName + ".MPD.yaml.asset");
            AssetDatabase.CreateAsset(so, "Assets/Resources/Serialized/Maps/" + FileName + ".MPD.yaml.asset");
            AssetDatabase.SaveAssets();


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

        public void BuildPrefab(bool erase = true)
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

            for (uint i = 0; i < so.numGroups; i++)
            {
                GameObject groupGO = new GameObject("Group_" + i);
                groupGO.transform.parent = mapGO.transform;

                Mesh mesh = new Mesh();
                List<Vector3> meshVertices = new List<Vector3>();
                List<int> meshTriangles = new List<int>();
                List<Vector2> meshTrianglesUV = new List<Vector2>();
                List<Vector3> meshNormals = new List<Vector3>();
                List<Color32> meshColors = new List<Color32>();
                int iv = 0;
                int lmf = so.groups[i].faces.Length;
                for (int k = 0; k < lmf; k++)
                {
                    MPDFace f = so.groups[i].faces[k];

                    if (f.isQuad)
                    {
                        meshVertices.Add(-f.GetOpVertex(so.groups[i], 0) / 128);
                        meshVertices.Add(-f.GetOpVertex(so.groups[i], 1) / 128);
                        meshVertices.Add(-f.GetOpVertex(so.groups[i], 2) / 128);
                        meshVertices.Add(-f.GetOpVertex(so.groups[i], 3) / 128);
                        meshColors.Add(f.colors[0]);
                        meshColors.Add(f.colors[1]);
                        meshColors.Add(f.colors[2]);
                        meshColors.Add(f.colors[3]);
                        meshTrianglesUV.Add(f.uvs[0] / 256);
                        meshTrianglesUV.Add(f.uvs[1] / 256);
                        meshTrianglesUV.Add(f.uvs[2] / 256);
                        meshTrianglesUV.Add(f.uvs[3] / 256);
                        
                        //meshNormals.Add(f.n);
                        
                        meshTriangles.Add(iv + 0);
                        meshTriangles.Add(iv + 1);
                        meshTriangles.Add(iv + 2);

                        meshTriangles.Add(iv + 3);
                        meshTriangles.Add(iv + 2);
                        meshTriangles.Add(iv + 1);
                        iv += 4;
                    } else
                    {
                        meshVertices.Add(-f.GetOpVertex(so.groups[i], 0) / 128);
                        meshVertices.Add(-f.GetOpVertex(so.groups[i], 1) / 128);
                        meshVertices.Add(-f.GetOpVertex(so.groups[i], 2) / 128);
                        meshColors.Add(f.colors[0]);
                        meshColors.Add(f.colors[1]);
                        meshColors.Add(f.colors[2]);
                        meshTrianglesUV.Add(f.uvs[0] / 256);
                        meshTrianglesUV.Add(f.uvs[1] / 256);
                        meshTrianglesUV.Add(f.uvs[2] / 256);
                        
                        //meshNormals.Add(f.n);
                        //meshNormals.Add(f.n);
                        //meshNormals.Add(f.n);
                        
                        meshTriangles.Add(iv + 0);
                        meshTriangles.Add(iv + 1);
                        meshTriangles.Add(iv + 2);
                        iv += 3;
                    }
                }
                mesh.name = "mesh_" + i;
                mesh.vertices = meshVertices.ToArray();
                mesh.triangles = meshTriangles.ToArray();
                mesh.uv = meshTrianglesUV.ToArray();
                //mesh.normals = meshNormals.ToArray();
                mesh.colors32 = meshColors.ToArray();
                mesh.RecalculateNormals();

                GameObject meshGo = new GameObject("mesh_" + i);
                meshGo.transform.parent = groupGO.transform;

                MeshFilter mf = meshGo.AddComponent<MeshFilter>();
                mf.mesh = mesh;

                MeshRenderer mr = meshGo.AddComponent<MeshRenderer>();
                
                if (z != null && z.tims.Length > 0)
                {
                    //mr.material = z.GetMaterial(groups[i].meshes[j].textureId, groups[i].meshes[j].clutId);
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
}
*/