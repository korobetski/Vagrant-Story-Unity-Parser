/*
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using VS.Serializable;
using VS.Utils;

// http://datacrystal.romhacking.net/wiki/Vagrant_Story:ZND_files

namespace VS.Parser
{
    public class ZND : FileParser
    {
        public Serializable.ZND so;

        public void Parse(string filePath)
        {
            PreParse(filePath);
            Parse(buffer);

            buffer.Close();
            fileStream.Close();
        }
        public void Parse(BinaryReader buffer)
        {
            so = ScriptableObject.CreateInstance<Serializable.ZND>();

            so.ptrMPD = buffer.ReadUInt32();
            so.lenMPD = buffer.ReadUInt32();
            so.ptrEnemies = buffer.ReadUInt32();
            so.lenEnemies = buffer.ReadUInt32();
            so.ptrTIM = buffer.ReadUInt32();
            so.lenTIM = buffer.ReadUInt32();
            so.waveId = buffer.ReadByte();
            so.numMPD = so.lenMPD / 8;
            buffer.ReadBytes(7); // padding


            // MPD Section
            if (buffer.BaseStream.Position != so.ptrMPD)
            {
                buffer.BaseStream.Position = so.ptrMPD;
            }
            so.MPD_LBA = new Vector2Int[so.numMPD];
            for (int i = 0; i < so.numMPD; i++)
            {
                so.MPD_LBA[i] = new Vector2Int((int)buffer.ReadUInt32(), (int)buffer.ReadUInt32());
            }

            // ZUD Section
            if (buffer.BaseStream.Position != so.ptrEnemies)
            {
                buffer.BaseStream.Position = so.ptrEnemies;
            }

            so.numEnemies = buffer.ReadUInt32();
            so.monsters = new ZNDMonster[so.numEnemies];

            so.ZUD_LBA = new Vector2Int[so.numEnemies];
            for (int i = 0; i < so.numEnemies; i++)
            {
                so.ZUD_LBA[i] = new Vector2Int((int)buffer.ReadUInt32(), (int)buffer.ReadUInt32());
            }

            long ptrEnemies = buffer.BaseStream.Position;
            for (int i = 0; i < so.numEnemies; i++)
            {
                buffer.BaseStream.Position = ptrEnemies + i * 0x464;
                so.monsters[i] = new ZNDMonster(buffer);
            }


            // Textures section
            if (buffer.BaseStream.Position != so.ptrTIM)
            {
                buffer.BaseStream.Position = so.ptrTIM;
            }

            // TODO : Move this section in TIM.cs

            so.lenTIM2 = buffer.ReadUInt32();
            buffer.ReadBytes(12);
            so.numTIM = buffer.ReadUInt32();
            so.TIMs = new VSTIM[so.numTIM];
            for (int i = 0; i < so.numTIM; i++)
            {
                if (buffer.BaseStream.Position + 32 < buffer.BaseStream.Length)
                {
                    uint tl = buffer.ReadUInt32();
                    so.TIMs[i] = new VSTIM((uint)i, tl, buffer);
                }
            }

            buffer.Close();


            ToolBox.DirExNorCreate("Assets/");
            ToolBox.DirExNorCreate("Assets/Resources/");
            ToolBox.DirExNorCreate("Assets/Resources/Serialized/");
            ToolBox.DirExNorCreate("Assets/Resources/Serialized/Zones/");
            AssetDatabase.DeleteAsset("Assets/Resources/Serialized/Zones/" + FileName + ".ZND.yaml.asset");
            AssetDatabase.CreateAsset(so, "Assets/Resources/Serialized/Zones/" + FileName + ".ZND.yaml.asset");
            AssetDatabase.SaveAssets();
        }

        public void BuildPrefab(bool erase = false)
        {
            GameObject zoneGO = new GameObject(FileName);
            string zndFolder = "Assets/Resources/Prefabs/Zones/Datas/";
            ToolBox.DirExNorCreate(zndFolder);
            PrefabUtility.SaveAsPrefabAsset(zoneGO, "Assets/Resources/Prefabs/Zones/Datas/" + FileName + ".prefab");
            AssetDatabase.SaveAssets();
            GameObject.DestroyImmediate(zoneGO);

        }



        private class FrameBuffer
        {
            public uint width;
            public uint height;
            byte[] buf;

            public FrameBuffer()
            {
                width = 1024;
                height = 512;
                buf = new byte[width * height * 4];
            }

            public void setPixel(int x, int y, Color32 color)
            {
                int i = (int)(y * width + x) * 4;
                buf[i + 0] = (byte)color.r;
                buf[i + 1] = (byte)color.g;
                buf[i + 2] = (byte)color.b;
                buf[i + 3] = (byte)color.a;
            }
        }
    }




}
*/