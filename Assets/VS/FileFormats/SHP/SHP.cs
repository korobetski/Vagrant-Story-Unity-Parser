using System;
using System.IO;
using UnityEngine;
using VS.FileFormats.GEOM;
using VS.Parser;
using VS.Utils;

namespace VS.FileFormats.SHP
{
    public class SHP:ScriptableObject
    {
        public string Filename;

        public uint Signature;
        public byte numBones;
        public byte numGroups;
        public ushort numTriangles;
        public ushort numQuads;
        public ushort numPolygons;
        public uint numFaces { get => (uint)numTriangles + numQuads + numPolygons; }

        public byte[][] overlay;
        public byte[] Padding0x24;

        public byte[] collider;
        public short menuY;
        public byte[] unk1; // 12 bytes
        public byte[] shadow;
        public byte[] unk2; // 4 bytes
        public short menuScale;
        public byte[] unk3; // 12 bytes

        public uint[] SEQ_LBAs;
        public ushort[] chainIds;
        public uint[] SP_LBAs;
        public byte[] moreLBA;

        public uint ptrSpellEffect;
        public ushort[] spellsId;
        public uint ptrAKAOSection;
        public uint ptrGroupSection;
        public uint ptrVertexSection;
        public uint ptrFaceSection;

        public Bone[] bones;
        public Group[] groups;
        public Vertex[] vertices;
        public Face[] faces;

        public bool hasColoredVertices;

        public uint numAKAO;
        public uint[] ptrAKAO;
        public AKAO.AKAO[] AKAOs;

        public uint numEffects;
        public uint lenEffects;
        public byte[] unkFx;
        public ushort[] PtrEffects;
        public uint[] effects;

        public TIM.TIM TIM;


        public void ParseFromFile(string filepath)
        {
            FileParser fp = new FileParser();
            fp.Read(filepath);

            // in OBJ/**.SHP
            if (fp.Ext == "SHP")
            {
                Filename = fp.FileName;
                ParseFromBuffer(fp.buffer, fp.FileSize);
            }

            fp.Close();
        }

        public void ParseFromBuffer(BinaryReader buffer, long limit)
        {
            Signature = buffer.ReadUInt32();
            numBones = buffer.ReadByte();
            numGroups = buffer.ReadByte();
            numTriangles = buffer.ReadUInt16();
            numQuads = buffer.ReadUInt16();
            numPolygons = buffer.ReadUInt16();

            byte[][] overlays = new byte[8][];
            for (int i = 0; i < 8; i++)
            {
                overlays[i] = buffer.ReadBytes(4);
            }

            Padding0x24 = buffer.ReadBytes(0x24); // Unknown
            collider = buffer.ReadBytes(6); // collision size and height (shape is a cylinder)
            menuY = buffer.ReadInt16(); // menu position Y
            unk1 = buffer.ReadBytes(12); // Unknown
            shadow = buffer.ReadBytes(6);
            unk2 = buffer.ReadBytes(4); // Unknown
            menuScale = buffer.ReadInt16(); // Menu scale
            unk3 = buffer.ReadBytes(12); // Unknown

            SEQ_LBAs = new uint[12];
            for (int i = 0; i < 12; i++)
            {
                SEQ_LBAs[i] = buffer.ReadUInt32();
            }
            chainIds = new ushort[12];
            for (int i = 0; i < 12; i++)
            {
                chainIds[i] = buffer.ReadUInt16();
            }
            SP_LBAs = new uint[4];
            for (int i = 0; i < 4; i++)
            {
                SP_LBAs[i] = buffer.ReadUInt32();
            }

            moreLBA = buffer.ReadBytes(0x20);

            uint dec = (uint)buffer.BaseStream.Position + 4;
            ptrSpellEffect = buffer.ReadUInt32() + dec;
            spellsId = new ushort[24];
            for (int i = 0; i < 24; i++)
            {
                spellsId[i] = buffer.ReadUInt16();
            }

            ptrAKAOSection = buffer.ReadUInt32() + dec;
            ptrGroupSection = buffer.ReadUInt32() + dec;
            ptrVertexSection = buffer.ReadUInt32() + dec;
            ptrFaceSection = buffer.ReadUInt32() + dec;


            // Bones section
            bones = new Bone[numBones];
            for (uint i = 0; i < numBones; i++)
            {
                Bone bone = new Bone
                {
                    index = i,
                    //name = "bone_" + i;
                    length = buffer.ReadInt32(),
                    parentBoneId = buffer.ReadSByte(),
                    groupId = buffer.ReadSByte(),
                    mountId = buffer.ReadSByte(),
                    bodyPartId = buffer.ReadSByte(),
                    mode = buffer.ReadSByte(),
                    unk = buffer.ReadBytes(7), // always 0000000
            };
                if (bone.parentBoneId > numBones) bone.parentBoneId = -1;
                bones[i] = bone;
            }


            // Group section
            if (buffer.BaseStream.Position != ptrGroupSection)
            {
                buffer.BaseStream.Position = ptrGroupSection;
            }
            groups = new Group[numGroups];
            for (uint i = 0; i < numGroups; i++)
            {
                Group group = new Group
                {
                    boneIndex = buffer.ReadInt16(),
                    numVertices = buffer.ReadUInt16()
                };
                if (group.boneIndex != -1) group.bone = bones[group.boneIndex];
                groups[i] = group;
            }


            // Vertices section
            if (buffer.BaseStream.Position != ptrVertexSection)
            {
                buffer.BaseStream.Position = ptrVertexSection;
            }
            uint numVertices = groups[groups.Length - 1].numVertices;
            vertices = new Vertex[numVertices];

            int g = 0;
            for (uint i = 0; i < numVertices; i++)
            {
                if (i >= groups[g].numVertices)
                {
                    g++;
                }

                Vertex vertex = new Vertex
                {
                    group = (byte)g,
                    bone = groups[g].bone
                };

                BoneWeight bw = new BoneWeight
                {
                    boneIndex0 = (int)groups[g].boneIndex,
                    weight0 = 1
                };

                vertex.position = -new Vector4(buffer.ReadInt16(), buffer.ReadInt16(), buffer.ReadInt16(), buffer.ReadInt16()) / 128;
                vertex.boneWeight = bw;
                vertices[i] = (vertex);
            }


            // Polygone section
            if (buffer.BaseStream.Position != ptrFaceSection)
            {
                buffer.BaseStream.Position = ptrFaceSection;
            }

            faces = new Face[numFaces];
            hasColoredVertices = false;
            for (int i = 0; i < numFaces; i++)
            {
                if (hasColoredVertices)
                {
                    long polyDec = buffer.BaseStream.Position;
                    byte[] bytes = buffer.ReadBytes(12);
                    if (bytes[11] == 0x34)
                    {
                        // its a triangle
                        buffer.BaseStream.Position = polyDec;

                        Face face = new Face
                        {
                            verticesCount = 3
                        };
                        for (uint j = 0; j < face.verticesCount; j++)
                        {
                            ushort vId = (ushort)(buffer.ReadUInt16() / 4);
                            face.vertices.Add(vId);
                        }
                        face.uv.Add(new Vector2(buffer.ReadByte(), buffer.ReadByte()));
                        face.colors.Add(new Color32(buffer.ReadByte(), buffer.ReadByte(), buffer.ReadByte(), 255));
                        face.type = buffer.ReadByte();
                        face.colors.Add(new Color32(buffer.ReadByte(), buffer.ReadByte(), buffer.ReadByte(), 255));
                        face.size = buffer.ReadByte();
                        face.colors.Add(new Color32(buffer.ReadByte(), buffer.ReadByte(), buffer.ReadByte(), 255));
                        face.side = buffer.ReadByte();
                        face.uv.Add(new Vector2(buffer.ReadByte(), buffer.ReadByte()));
                        face.uv.Add(new Vector2(buffer.ReadByte(), buffer.ReadByte()));
                    }
                    if (bytes[11] == 0x3C)
                    {
                        // its a quad
                        buffer.BaseStream.Position = polyDec;

                        Face face = new Face
                        {
                            verticesCount = 4
                        };
                        for (uint j = 0; j < face.verticesCount; j++)
                        {
                            ushort vId = (ushort)(buffer.ReadUInt16() / 4);
                            face.vertices.Add(vId);
                        }
                        face.colors.Add(new Color32(buffer.ReadByte(), buffer.ReadByte(), buffer.ReadByte(), 255));
                        face.type = buffer.ReadByte();
                        face.colors.Add(new Color32(buffer.ReadByte(), buffer.ReadByte(), buffer.ReadByte(), 255));
                        face.size = buffer.ReadByte();
                        face.colors.Add(new Color32(buffer.ReadByte(), buffer.ReadByte(), buffer.ReadByte(), 255));
                        face.side = buffer.ReadByte();
                        face.colors.Add(new Color32(buffer.ReadByte(), buffer.ReadByte(), buffer.ReadByte(), 255));
                        face.uv.Add(new Vector2(buffer.ReadByte(), buffer.ReadByte()));
                        face.uv.Add(new Vector2(buffer.ReadByte(), buffer.ReadByte()));
                        face.uv.Add(new Vector2(buffer.ReadByte(), buffer.ReadByte()));
                        face.uv.Add(new Vector2(buffer.ReadByte(), buffer.ReadByte()));
                    }
                }
                else
                {
                    Face face = new Face
                    {
                        type = buffer.ReadByte(),
                        size = buffer.ReadByte(),
                        side = buffer.ReadByte(),
                        alpha = buffer.ReadByte()
                    };
                    if (face.type == 36)
                    {
                        face.verticesCount = 3;
                    }
                    else if (face.type == 44)
                    {
                        face.verticesCount = 4;
                    }
                    else
                    {
                        // we need to restart all the loop
                        hasColoredVertices = true;
                        buffer.BaseStream.Position = ptrFaceSection;
                        i = -1;
                        continue;
                    }
                    // 6 or 8 bytes
                    for (uint j = 0; j < face.verticesCount; j++)
                    {
                        ushort vId = (ushort)(buffer.ReadUInt16() / 4);
                        face.vertices.Add(vId);
                    }
                    // 6 or 8 bytes
                    for (uint j = 0; j < face.verticesCount; j++)
                    {
                        face.uv.Add(new Vector2(buffer.ReadByte(), buffer.ReadByte()));
                    }
                    faces[i] = face;
                }
            }




            // AKAO section
            if (buffer.BaseStream.Position != ptrAKAOSection)
            {
                buffer.BaseStream.Position = ptrAKAOSection;
            }


            numAKAO = buffer.ReadUInt32();

            ptrAKAO = new uint[numAKAO + 1];
            // one pointer for AKAO header, a second for AKAO datas
            for (uint j = 0; j < numAKAO; j++)
            {
                ptrAKAO[j] = buffer.ReadUInt32();
            }
            ptrAKAO[numAKAO] = ptrSpellEffect;
            AKAOs = new AKAO.AKAO[numAKAO];
            for (uint j = 0; j < numAKAO; j++)
            {
                // TODO
            }


            // Spell effect section
            if (buffer.BaseStream.Position != ptrSpellEffect)
            {
                buffer.BaseStream.Position = ptrSpellEffect;
            }
            numEffects = buffer.ReadUInt32();
            lenEffects = buffer.ReadUInt32();
            unkFx = buffer.ReadBytes(12);
            PtrEffects = new ushort[1];
            effects = new uint[1];
            // we skip datas
            if (buffer.BaseStream.Position + (lenEffects - 12) < buffer.BaseStream.Length)
            {
                buffer.BaseStream.Position = buffer.BaseStream.Position + (lenEffects - 12);
            }

            // Textures section
            if (buffer.BaseStream.Position + 8 < buffer.BaseStream.Length)
            {
                // Textures section
                TIM = ScriptableObject.CreateInstance<TIM.TIM>();
                TIM.name = Filename + ".SHP.TIM";
                TIM.Filename = this.Filename;
                TIM.ParseSHPFromBuffer(buffer, hasColoredVertices);

                //ToolBox.SaveScriptableObject("Assets/Resources/Serialized/TIM/SHP/", Filename + ".yaml.asset", TIM);

            }

        }
    }
}
