using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using VS.Entity;
using VS.Utils;

// http://datacrystal.romhacking.net/wiki/Vagrant_Story:ZND_files

namespace VS.Parser
{
    public class ZND : FileParser
    {
        public Vector3[] MPDInfos;
        public Vector3[] ZUDInfos;
        public ZNDDatas datas;

        private FrameBuffer timfb;
        private VSTIM[] tims;
        private List<Texture2D> textures;

        public void Parse(string filePath)
        {
            PreParse(filePath);
            Parse(buffer);

            buffer.Close();
            fileStream.Close();
        }
        public void Parse(BinaryReader buffer)
        {
            uint mpdPtr = buffer.ReadUInt32();
            uint mpdLen = buffer.ReadUInt32();
            uint enemyPtr = buffer.ReadUInt32();
            uint enemyLen = buffer.ReadUInt32();
            uint timPtr = buffer.ReadUInt32();
            uint timLen = buffer.ReadUInt32();
            uint wave = buffer.ReadByte();
            uint mpdNum = mpdLen / 8;
            buffer.ReadBytes(7); // padding

            if (UseDebug)
            {
                Debug.Log(FileName);
                Debug.Log("mpdPtr : " + mpdPtr);
                Debug.Log("mpdLen : " + mpdLen);
                Debug.Log("mpdNum : " + mpdNum);
                Debug.Log("enemyPtr : " + enemyPtr);
                Debug.Log("enemyLen : " + enemyLen);
                Debug.Log("timPtr : " + timPtr);
                Debug.Log("timLen : " + timLen);
                Debug.Log("wave : " + wave);
                Debug.Log("-------------------------------");
            }

            // MPD Section
            if (buffer.BaseStream.Position != mpdPtr)
            {
                buffer.BaseStream.Position = mpdPtr;
            }

            MPDInfos = new Vector3[mpdNum];
            for (int i = 0; i < mpdNum; i++)
            {
                uint id = buffer.ReadUInt32();
                uint value = buffer.ReadUInt32();
                if (UseDebug)
                {
                    Debug.Log("LBA MPD = " + id + "  - file size = " + value);
                }

                MPDInfos[i] = new Vector3(id, value);
            }
            // ZUD Section
            if (buffer.BaseStream.Position != enemyPtr)
            {
                buffer.BaseStream.Position = enemyPtr;
            }

            uint numEnemies = buffer.ReadUInt32();
            if (UseDebug)
            {
                Debug.Log("numEnemies : " + numEnemies);
            }

            ZUDInfos = new Vector3[numEnemies];
            for (int i = 0; i < numEnemies; i++)
            {
                uint id = buffer.ReadUInt32();
                uint value = buffer.ReadUInt32();

                if (UseDebug)
                {
                    Debug.Log("LBA ZUD = " + id + "  - file size = " + value);
                }

                ZUDInfos[i] = new Vector3(id, value);
            }

            datas = new ZNDDatas();
            datas.monsters = new ZNDMonster[numEnemies];


            // TODO enemy class & parse
            long loopbase = buffer.BaseStream.Position;
            Debug.Log("start loop Position " + buffer.BaseStream.Position);
            Debug.Log("timPtr " + timPtr);

            for (int i = 0; i < numEnemies; i++)
            {
                Debug.Log("ZND ennemy # " + i);
                /*
                if (buffer.BaseStream.Position != (loopbase + i * 464))
                {
                    buffer.BaseStream.Position = (loopbase + i * 464);
                }*/
                datas.monsters[i] = new ZNDMonster(buffer);
            }
            Debug.Log("end loop Position " + buffer.BaseStream.Position);


            // Textures section
            if (buffer.BaseStream.Position != timPtr)
            {
                buffer.BaseStream.Position = timPtr;
            }

            // TODO : Move this section in TIM.cs

            uint timSecLen = buffer.ReadUInt32();
            buffer.ReadBytes(12);
            uint numTim = buffer.ReadUInt32();
            timfb = new FrameBuffer();
            tims = new VSTIM[numTim];
            for (int i = 0; i < numTim; i++)
            {
                if (buffer.BaseStream.Position + 32 < buffer.BaseStream.Length)
                {
                    uint tl = buffer.ReadUInt32();
                    tims[i] = new VSTIM((uint)i, tl, buffer);
                    for (int x = 0; x < tims[i].width; x++)
                    {
                        for (int y = 0; y < tims[i].height; y++)
                        {
                            timfb.setPixel((int)tims[i].fx + x, (int)tims[i].fy + y, ToolBox.BitColorConverter(buffer.ReadUInt16()));
                        }
                    }
                }
            }

            buffer.Close();

            datas.filePath = FilePath;
            datas.tims = tims;
            datas.MPDInfos = MPDInfos;
            datas.ZUDInfos = ZUDInfos;


        }

        public void BuildPrefab(bool erase = false)
        {
            GameObject zoneGO = new GameObject(FileName);
            ZNDDatas zndd = zoneGO.AddComponent<ZNDDatas>();
            zndd = datas;
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
