using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using VS.FileFormats.ITEM;
using VS.Utils;

namespace VS.FileFormats.MPD
{
    public class MPD:ScriptableObject
    {
        public string Filename;

        public uint ptrRoomSection;
        public uint lenRoomSection;
        public uint ptrClearedSection;
        public uint lenClearedSection;
        public uint ptrScriptSection;
        public uint lenScriptSection;
        public uint ptrDoorSection;
        public uint lenDoorSection;
        public uint ptrEnemySection;
        public uint lenEnemySection;
        public uint ptrTreasureSection;
        public uint lenTreasureSection;

        public uint lenGeometrySection;
        public uint lenCollisionSection;
        public uint lenTilePropertiesSection;
        public uint lenRoomDoorSection;
        public uint lenLightingSection;
        public uint lenSubSection06;
        public uint lenSubSection07;
        public uint lenSubSection08;
        public uint lenTrapSection;
        public uint lenSubSection0A;
        public uint lenSubSection0B;
        public uint lenTextureEffectsSection;
        public uint lenSubSection0D;
        public uint lenSubSection0E;
        public uint lenMiniMapSection;
        public uint lenSubSection10;
        public uint lenSubSection11;
        public uint lenFloatingStoneSection;
        public uint lenChestInteractionSection;
        public uint lenAKAOSection;
        public uint lenSubSection15;
        public uint lenSubSection16;
        public uint lenSubSection17;
        public uint lenCameraAreaSection;

        public uint numGroups;
        public MPDGroup[] groups;

        public ushort tileWidth;
        public ushort tileHeigth;
        public ushort unk1;
        public ushort numTileModes;
        public MPDTile[] tiles;
        public MPDTileMode[] tileModes;

        public uint numRoomDoors; // round(lenDoorSection / 0x0C)
        public MPDRoomDoor[] roomDoors;

        public uint numLights;
        public MPDLight ambientLight;
        public MPDLight[] lights;

        public byte[] SubSection06;
        public byte[] SubSection07;
        public byte[] SubSection08;

        public uint numTraps;
        public MPDTrap[] traps;

        public byte[] SubSection0A;
        public byte[] SubSection0B;

        public MPDTextureAnimation[] textureAnimations;

        public byte[] SubSection0D;
        public byte[] SubSection0E;

        public ARM.ARM miniMap;

        public byte[] SubSection10;
        public byte[] SubSection11;

        public byte[] FloatingStoneSection;
        public byte[] ChestInteractionSection;
        public byte[] AKAOSection;

        public byte[] SubSection15;
        public byte[] SubSection16;
        public byte[] SubSection17;

        public ushort[] CameraAreaSection;

        public byte[] clearedSection;
        public EVT.EVT scriptSection;
        public byte[] doorSection;
        public byte[][] enemySection;
        public Treasure treasureSection;

        public string[] materialRefs;
        private List<string> _materialRefs;

        public void ParseFromFile(string filepath)
        {
            FileParser fp = new FileParser();
            fp.Read(filepath);

            // in MAP/MAP***.MPD
            if (fp.Ext == "MPD")
            {
                Filename = fp.FileName;
                ParseFromBuffer(fp.buffer, fp.FileSize);
            }

            fp.Close();
        }

        public void ParseFromBuffer(BinaryReader buffer, long limit)
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
            if (lenRoomSection >= 96)
            {
                lenGeometrySection = buffer.ReadUInt32();
                lenCollisionSection = buffer.ReadUInt32();
                lenTilePropertiesSection = buffer.ReadUInt32();
                lenRoomDoorSection = buffer.ReadUInt32();
                lenLightingSection = buffer.ReadUInt32();
                lenSubSection06 = buffer.ReadUInt32();
                lenSubSection07 = buffer.ReadUInt32();
                lenSubSection08 = buffer.ReadUInt32();
                lenTrapSection = buffer.ReadUInt32();
                lenSubSection0A = buffer.ReadUInt32();
                lenSubSection0B = buffer.ReadUInt32();
                lenTextureEffectsSection = buffer.ReadUInt32();
                lenSubSection0D = buffer.ReadUInt32();
                lenSubSection0E = buffer.ReadUInt32();
                lenMiniMapSection = buffer.ReadUInt32();
                lenSubSection10 = buffer.ReadUInt32();
                lenSubSection11 = buffer.ReadUInt32();
                lenFloatingStoneSection = buffer.ReadUInt32();
                lenChestInteractionSection = buffer.ReadUInt32();
                lenAKAOSection = buffer.ReadUInt32();
                lenSubSection15 = buffer.ReadUInt32();
                lenSubSection16 = buffer.ReadUInt32();
                lenSubSection17 = buffer.ReadUInt32();
                lenCameraAreaSection = buffer.ReadUInt32();
            }
            else
            {
                buffer.ReadBytes((int)lenRoomSection);
            }

            // ROOM section
            // Geometry
            if (lenRoomSection > 0x40)
            {
                if (lenGeometrySection > 0)
                {
                    numGroups = buffer.ReadUInt32();
                    groups = new MPDGroup[numGroups];
                    for (uint i = 0; i < numGroups; i++)
                    {
                        groups[i] = new MPDGroup();
                        //groups[i].header = buffer.ReadBytes(64);
                        groups[i].visibility = buffer.ReadByte();
                        groups[i].scaleFlag = buffer.ReadByte();
                        groups[i].overlapping = buffer.ReadUInt16();
                        groups[i].decX = buffer.ReadInt16();
                        groups[i].unk1 = buffer.ReadUInt16();
                        groups[i].decY = buffer.ReadInt16();
                        groups[i].unk2 = buffer.ReadUInt16();
                        groups[i].decZ = buffer.ReadInt16();
                        groups[i].unk3 = buffer.ReadUInt16();
                        groups[i].unkBytes = buffer.ReadBytes(48);


                        if ((groups[i].scaleFlag & 0x08) > 0)
                        {
                            groups[i].scale = 1;
                        }
                    }
                    _materialRefs = new List<string>();
                    for (uint i = 0; i < numGroups; i++)
                    {
                        groups[i].numTriangles = buffer.ReadUInt32();
                        groups[i].numQuads = buffer.ReadUInt32();
                        groups[i].faces = new MPDFace[groups[i].numTriangles + groups[i].numQuads];

                        for (uint j = 0; j < groups[i].numTriangles + groups[i].numQuads; j++)
                        {
                            MPDFace face = new MPDFace(buffer);
                            groups[i].faces[j] = face;

                            string matRef = string.Concat(face.textureId, "@", face.palettePtr);
                            if (!_materialRefs.Contains(matRef)) _materialRefs.Add(matRef);
                        }
                    }

                    materialRefs = _materialRefs.ToArray();
                }

                // collision
                if (lenCollisionSection > 0)
                {
                    long collisionPtr = buffer.BaseStream.Position;

                    tileWidth = buffer.ReadUInt16();
                    tileHeigth = buffer.ReadUInt16();
                    unk1 = buffer.ReadUInt16();
                    numTileModes = buffer.ReadUInt16();
                    tiles = new MPDTile[tileWidth * tileHeigth];
                    for (uint i = 0; i < tileWidth * tileHeigth; i++)
                    {
                        MPDTile tile = new MPDTile();
                        tile.floorHeight = buffer.ReadByte();
                        tile.floorMode = buffer.ReadByte();
                        tiles[i] = tile;
                    }

                    for (uint i = 0; i < tileWidth * tileHeigth; i++)
                    {
                        tiles[i].ceilHeight = buffer.ReadByte();
                        tiles[i].ceilMode = buffer.ReadByte();
                    }

                    tileModes = new MPDTileMode[numTileModes];
                    for (uint i = 0; i < numTileModes; i++)
                    {
                        tileModes[i] = new MPDTileMode();
                        tileModes[i].datas = buffer.ReadBytes(16);
                    }

                    if (buffer.BaseStream.Position != collisionPtr + lenCollisionSection)
                    {
                        Debug.LogWarning("Wrong position after collision");
                        buffer.BaseStream.Position = collisionPtr + lenCollisionSection;
                    }
                }

                // tile properties
                //
                if (lenTilePropertiesSection > 0)
                {
                    long tilePropsPtr = buffer.BaseStream.Position;

                    int loop = tileWidth * tileHeigth;
                    for (uint i = 0; i < loop; i++)
                    {
                        tiles[i].properties = buffer.ReadBytes(4);
                    }

                    if (buffer.BaseStream.Position != tilePropsPtr + lenTilePropertiesSection)
                    {
                        Debug.LogWarning("Wrong position after tileProps");
                        buffer.BaseStream.Position = tilePropsPtr + lenTilePropertiesSection;
                    }
                }

                // door section
                if (lenRoomDoorSection > 0)
                {
                    long doorPtr = buffer.BaseStream.Position;
                    numRoomDoors = lenRoomDoorSection / 0x0C;
                    roomDoors = new MPDRoomDoor[numRoomDoors];
                    for (uint i = 0; i < numRoomDoors; i++)
                    {
                        MPDRoomDoor door = new MPDRoomDoor();
                        door.zoneId = buffer.ReadByte();
                        door.roomId = buffer.ReadByte();
                        door.tileIndex = buffer.ReadUInt16();
                        door.destination = new ushort[] { buffer.ReadUInt16(), buffer.ReadUInt16() };
                        door.doorId = buffer.ReadUInt32();
                        roomDoors[i] = door;
                    }

                    if (buffer.BaseStream.Position != doorPtr + lenRoomDoorSection)
                    {
                        Debug.LogWarning("Wrong position after roomDoor");
                        buffer.BaseStream.Position = doorPtr + lenRoomDoorSection;
                    }
                }

                // lights section
                // http://www.psxdev.net/forum/viewtopic.php?f=51&t=3383
                if (lenLightingSection > 0)
                {
                    long lightPtr = buffer.BaseStream.Position;
                    numLights = (lenLightingSection - 12) / 32;
                    lights = new MPDLight[numLights];
                    ambientLight = new MPDLight();
                    byte[] cols = buffer.ReadBytes(12);
                    ambientLight.colors[0] = new Color32(cols[0], cols[1], cols[2], cols[3]);
                    ambientLight.colors[1] = new Color32(cols[4], cols[5], cols[6], cols[7]);
                    ambientLight.colors[2] = new Color32(cols[8], cols[9], cols[10], cols[11]);

                    for (uint i = 0; i < numLights; i++)
                    {
                        for (uint j = 0; j < 10; j++)
                        {
                            lights[i] = new MPDLight();
                            lights[i].datas[j] = buffer.ReadInt16();
                        }

                        cols = buffer.ReadBytes(12);
                        lights[i].colors[0] = new Color32(cols[0], cols[1], cols[2], cols[3]);
                        lights[i].colors[1] = new Color32(cols[4], cols[5], cols[6], cols[7]);
                        lights[i].colors[2] = new Color32(cols[8], cols[9], cols[10], cols[11]);

                        /*
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
                        l.shadows = LightShadows.t;
                        lights.Add(lgo);
                        */
                    }

                    if (buffer.BaseStream.Position != lightPtr + lenLightingSection)
                    {
                        Debug.LogWarning("Wrong position after light");
                        buffer.BaseStream.Position = lightPtr + lenLightingSection;
                    }
                }

                if (lenSubSection06 > 0)
                {
                    SubSection06 = buffer.ReadBytes((int)lenSubSection06);
                }

                if (lenSubSection07 > 0)
                {
                    SubSection07 = buffer.ReadBytes((int)lenSubSection07);
                }

                if (lenSubSection08 > 0)
                {
                    SubSection08 = buffer.ReadBytes((int)lenSubSection08);
                }

                if (lenTrapSection > 0)
                {
                    long trapPtr = buffer.BaseStream.Position;

                    uint numTraps = (uint)(lenTrapSection / 12);
                    traps = new MPDTrap[numTraps];
                    for (uint i = 0; i < numTraps; i++)
                    {
                        traps[i] = new MPDTrap();
                        traps[i].position = new Vector2(buffer.ReadUInt16(), buffer.ReadUInt16()); // grid based
                        ushort p = buffer.ReadUInt16(); // padding
                        traps[i].skillId = buffer.ReadUInt16(); // http://datacrystal.romhacking.net/wiki/Vagrant_Story:skills_list
                        traps[i].unk1 = buffer.ReadByte();
                        traps[i].save = buffer.ReadByte();
                        traps[i].saveIndex = buffer.ReadByte();
                        traps[i].zero = buffer.ReadByte();
                    }

                    if (buffer.BaseStream.Position != trapPtr + lenTrapSection)
                    {
                        Debug.LogWarning("Wrong position after traps");
                        buffer.BaseStream.Position = trapPtr + lenTrapSection;
                    }
                }

                if (lenSubSection0A > 0)
                {
                    SubSection0A = buffer.ReadBytes((int)lenSubSection0A);
                }

                if (lenSubSection0B > 0)
                {
                    SubSection0B = buffer.ReadBytes((int)lenSubSection0B);
                }

                if (lenTextureEffectsSection > 0)
                {
                    long ptrTextureEffectsSection = buffer.BaseStream.Position;

                    List<MPDTextureAnimation> texAnims = new List<MPDTextureAnimation>();
                    while (buffer.BaseStream.Position + 20 <= (ptrTextureEffectsSection + lenTextureEffectsSection))
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
                                Debug.Log("Unknown Texture animation type : " + textureAnimation.type + "  in file : " + Filename);
                                break;
                        }
                        texAnims.Add(textureAnimation);
                    }

                    textureAnimations = texAnims.ToArray();

                    if (buffer.BaseStream.Position != ptrTextureEffectsSection + lenTextureEffectsSection)
                    {
                        buffer.BaseStream.Position = ptrTextureEffectsSection + lenTextureEffectsSection;
                    }
                }

                if (lenSubSection0D > 0)
                {
                    SubSection0D = buffer.ReadBytes((int)lenSubSection0D);
                }

                if (lenSubSection0E > 0)
                {
                    SubSection0E = buffer.ReadBytes((int)lenSubSection0E);
                }

                if (lenMiniMapSection > 0)
                {
                    long ptrMiniMapSection = buffer.BaseStream.Position;

                    // ARM Format
                    ARM.ARM arm = ScriptableObject.CreateInstance<ARM.ARM>();
                    arm.name = Filename + ".ARM";
                    arm.ParseFromBuffer(buffer, buffer.BaseStream.Position + lenMiniMapSection);

                    miniMap = arm;

                    if (buffer.BaseStream.Position != ptrMiniMapSection + lenMiniMapSection)
                    {
                        buffer.BaseStream.Position = ptrMiniMapSection + lenMiniMapSection;
                    }
                }

                if (lenSubSection10 > 0)
                {
                    SubSection10 = buffer.ReadBytes((int)lenSubSection10);
                }

                if (lenSubSection11 > 0)
                {
                    SubSection11 = buffer.ReadBytes((int)lenSubSection11);
                }

                if (lenFloatingStoneSection > 0)
                {
                    FloatingStoneSection = buffer.ReadBytes((int)lenFloatingStoneSection);
                }

                if (lenChestInteractionSection > 0)
                {
                    ChestInteractionSection = buffer.ReadBytes((int)lenChestInteractionSection);
                }

                if (lenAKAOSection > 0)
                {
                    AKAOSection = buffer.ReadBytes((int)lenAKAOSection);
                }

                if (lenSubSection15 > 0)
                {
                    SubSection15 = buffer.ReadBytes((int)lenSubSection15);
                }

                if (lenSubSection16 > 0)
                {
                    SubSection16 = buffer.ReadBytes((int)lenSubSection16);
                }

                if (lenSubSection17 > 0)
                {
                    SubSection17 = buffer.ReadBytes((int)lenSubSection17);
                }

                if (lenCameraAreaSection > 0)
                {
                    CameraAreaSection = new ushort[] { buffer.ReadUInt16(), buffer.ReadUInt16(), buffer.ReadUInt16(), buffer.ReadUInt16() };
                }
            }

            // Cleared section
            if (buffer.BaseStream.Position != ptrClearedSection)
            {
                buffer.BaseStream.Position = ptrClearedSection;
            }
            clearedSection = buffer.ReadBytes((int)lenClearedSection);

            // Script section
            if (buffer.BaseStream.Position != ptrScriptSection)
            {
                buffer.BaseStream.Position = ptrScriptSection;
            }

            if (lenScriptSection > 0)
            {
                // EVT format
                //scriptSection = buffer.ReadBytes((int)lenScriptSection);
                EVT.EVT evt = ScriptableObject.CreateInstance<EVT.EVT>();
                evt.name = Filename + ".EVT";
                evt.ParseFromBuffer(buffer, buffer.BaseStream.Position + lenScriptSection);

                scriptSection = evt;

            }


            // Door section
            if (buffer.BaseStream.Position != ptrDoorSection)
            {
                buffer.BaseStream.Position = ptrDoorSection;
            }
            doorSection = buffer.ReadBytes((int)lenDoorSection);


            // Ennemy section
            if (buffer.BaseStream.Position != ptrEnemySection)
            {
                buffer.BaseStream.Position = ptrEnemySection;
            }
            if (lenEnemySection >= 40)
            {
                uint numEnemy = lenEnemySection / 40;
                List<byte[]> enemyDatas = new List<byte[]>();
                for (uint i = 0; i < numEnemy; i++)
                {
                    enemyDatas.Add(buffer.ReadBytes(40));
                }
                enemySection = enemyDatas.ToArray();
            }



            // Treasure section
            if (buffer.BaseStream.Position != ptrTreasureSection)
            {
                buffer.BaseStream.Position = ptrTreasureSection;
            }
            //treasureSection = buffer.ReadBytes((int)lenTreasureSection);

            if (lenTreasureSection > 0)
            {
                // we need this to get item names
                ItemList items = Resources.Load("Serialized/MENU/ITEMS.yaml") as ItemList;

                long basePtr = buffer.BaseStream.Position;

                Treasure treasure = ScriptableObject.CreateInstance<Treasure>();
                treasure.name = Filename + ".CHEST";

                treasureSection = treasure;
                Weapon weapon = new Weapon();
                weapon.blade = new Blade();
                weapon.blade.SetDatasFromMPD(buffer);
                weapon.blade.name = items.GetName(weapon.blade.nameId);
                weapon.blade.description = items.GetDescription(weapon.blade.nameId);

                weapon.grip = new Grip();
                weapon.grip.SetDatasFromMPD(buffer);
                weapon.grip.name = items.GetName(weapon.grip.nameId);
                weapon.grip.description = items.GetDescription(weapon.grip.nameId);
                for (uint i = 0; i < 3; i++)
                {
                    weapon.gems[i] = new Gem();
                    weapon.gems[i].SetDatasFromMPD(buffer);
                    weapon.gems[i].name = items.GetName(weapon.gems[i].nameId);
                    weapon.gems[i].description = items.GetDescription(weapon.gems[i].nameId);
                }
                weapon.name = Utils.L10n.CleanTranslate(buffer.ReadBytes(20));
                treasureSection.weapon = weapon;

                treasureSection.blade = new Blade();
                treasureSection.blade.SetDatasFromMPD(buffer);
                treasureSection.blade.name = items.GetName(treasureSection.blade.nameId);
                treasureSection.blade.description = items.GetDescription(treasureSection.blade.nameId);

                treasureSection.grip = new Grip();
                treasureSection.grip.check = buffer.ReadUInt32();
                treasureSection.grip.SetDatasFromMPD(buffer);
                treasureSection.grip.name = items.GetName(treasureSection.grip.nameId);
                treasureSection.grip.description = items.GetDescription(treasureSection.grip.nameId);

                treasureSection.shield = new Shield();
                treasureSection.shield.SetDatasFromMPD(buffer);
                treasureSection.shield.name = items.GetName(treasureSection.shield.nameId);
                for (uint i = 0; i < 3; i++)
                {
                    treasureSection.shield.gems[i] = new Gem();
                    treasureSection.shield.gems[i].SetDatasFromMPD(buffer);
                    treasureSection.shield.gems[i].name = items.GetName(treasureSection.shield.gems[i].nameId);
                    treasureSection.shield.gems[i].description = items.GetDescription(treasureSection.shield.gems[i].nameId);
                }
                treasureSection.armor1 = new Armor();
                treasureSection.armor1.SetDatasFromMPD(buffer);
                treasureSection.armor1.name = items.GetName(treasureSection.armor1.nameId);

                treasureSection.armor2 = new Armor();
                treasureSection.armor2.SetDatasFromMPD(buffer);
                treasureSection.armor2.name = items.GetName(treasureSection.armor2.nameId);

                treasureSection.accessory = new Armor();
                treasureSection.accessory.SetDatasFromMPD(buffer);
                treasureSection.accessory.name = items.GetName(treasureSection.accessory.nameId);
                treasureSection.accessory.description = items.GetDescription(treasureSection.accessory.nameId);

                treasureSection.gem = new Gem();
                treasureSection.gem.check = buffer.ReadUInt32();
                treasureSection.gem.SetDatasFromMPD(buffer);
                treasureSection.gem.name = items.GetName(treasureSection.gem.nameId);
                treasureSection.gem.description = items.GetDescription(treasureSection.gem.nameId);

                for (uint i = 0; i < 3; i++)
                {
                    treasureSection.miscItems[i] = new MiscItem();
                    treasureSection.miscItems[i].SetDatasFromMPD(buffer);
                    treasureSection.miscItems[i].name = items.GetName(treasureSection.miscItems[i].nameId);
                    treasureSection.miscItems[i].description = items.GetDescription(treasureSection.miscItems[i].nameId);
                }
            }

        }
    }
}
