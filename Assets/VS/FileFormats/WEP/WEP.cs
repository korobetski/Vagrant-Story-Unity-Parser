using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VS.FileFormats.GEOM;
using VS.Utils;

namespace VS.FileFormats.WEP
{
    //[CreateAssetMenu(fileName = "Weapons/New WEP", menuName = "ScriptableObjects/WEP", order = 1)]
    public class WEP : ScriptableObject
    {
        public string Filename;

        public byte WEPId;
        public byte Id;
        public string Name;
        public string Description;

        public uint Signature;
        public byte NumBones;
        public byte NumGroups;
        public ushort NumTriangles;
        public ushort NumQuads;
        public ushort NumPolygons;
        public uint NumFaces { get => (uint)NumTriangles + NumQuads + NumPolygons; }
        public Enums.Material.Type material { get; internal set; }

        // Pointers values need +16 in WEP and more in ZUD
        public uint PtrTexture;
        public byte[] Padding0x30;

        public uint PtrTextureSection;
        public uint PtrGroupSection;
        public uint PtrVertexSection;
        public uint PtrFaceSection;

        public Bone[] Bones;
        public Group[] Groups;
        public Vertex[] Vertices;
        public Face[] Faces;
        public TIM.TIM TIM;

        public byte[] Footer;


        public void ParseFromFile(string filepath)
        {
            FileParser fp = new FileParser();
            fp.Read(filepath);

            // in OBJ/**.WEP
            if (fp.Ext == "WEP")
            {
                Filename = fp.FileName;
                WEPId = byte.Parse(Filename, System.Globalization.NumberStyles.HexNumber);
                ParseFromBuffer(fp.buffer, fp.FileSize);
            }

            fp.Close();
        }

        public void ParseFromBuffer(BinaryReader buffer, long limit)
        {

            Signature = buffer.ReadUInt32(); // signiture "H01"

            NumBones = buffer.ReadByte();
            NumGroups = buffer.ReadByte();
            NumTriangles = buffer.ReadUInt16();
            NumQuads = buffer.ReadUInt16();
            NumPolygons = buffer.ReadUInt16();

            long dec = buffer.BaseStream.Position + 4;
            PtrTexture = (uint)(buffer.ReadUInt32() + dec);
            Padding0x30 = buffer.ReadBytes(0x30); // padding

            PtrTextureSection = (uint)(buffer.ReadUInt32() + dec); // same as texturePtr1 in WEP but maybe not in ZUD
            PtrGroupSection = (uint)(buffer.ReadUInt32() + dec);
            PtrVertexSection = (uint)(buffer.ReadUInt32() + dec);
            PtrFaceSection = (uint)(buffer.ReadUInt32() + dec);

            // Bones section
            Bones = new Bone[NumBones];
            for (uint i = 0; i < NumBones; i++)
            {
                Bone bone = new Bone();
                bone.index = i;
                //bone.name = "bone_" + i;
                // https://github.com/morris/vstools/blob/master/src/WEPBone.js
                bone.length = buffer.ReadInt32();
                bone.parentBoneId = buffer.ReadSByte();
                bone.groupId = buffer.ReadSByte();
                bone.mountId = buffer.ReadSByte();
                bone.bodyPartId = buffer.ReadSByte();
                bone.mode = buffer.ReadSByte();
                bone.unk = buffer.ReadBytes(7); // always 0000000
                if (bone.parentBoneId != -1 && bone.parentBoneId != 47)
                {
                    bone.SetParentBone(Bones[bone.parentBoneId]);
                }
                Bones[i] = bone;
            }

            // Group section
            if (buffer.BaseStream.Position != PtrGroupSection) buffer.BaseStream.Position = PtrGroupSection;
            Groups = new Group[NumGroups];
            for (uint i = 0; i < NumGroups; i++)
            {
                Group group = new Group();
                group.boneIndex = buffer.ReadInt16();
                group.numVertices = buffer.ReadUInt16();
                // if (group.boneIndex != -1) group.bone = bones[group.boneIndex];

                Groups[i] = group;
            }

            // Vertices section
            if (buffer.BaseStream.Position != PtrVertexSection) buffer.BaseStream.Position = PtrVertexSection;

            uint numVertices = Groups[Groups.Length - 1].numVertices;
            Vertices = new Vertex[numVertices];
            int g = 0;
            for (uint i = 0; i < numVertices; i++)
            {
                if (i >= Groups[g].numVertices)
                {
                    g++;
                }

                Vertex vertex = new Vertex();
                vertex.index = i;
                vertex.group = (byte)g;
                vertex.bone = Bones[Groups[g].boneIndex];

                BoneWeight bw = new BoneWeight();
                bw.boneIndex0 = (int)Groups[g].boneIndex;
                bw.weight0 = 1;

                vertex.position = new Vector4(buffer.ReadInt16(), buffer.ReadInt16(), buffer.ReadInt16(), buffer.ReadInt16());
                vertex.boneWeight = bw;

                Vertices[i] = vertex;
            }

            // Polygone section
            if (buffer.BaseStream.Position != PtrFaceSection)
            {
                buffer.BaseStream.Position = PtrFaceSection;
            }

            Faces = new Face[NumFaces];
            for (uint i = 0; i < NumFaces; i++)
            {
                Face face = new Face();
                face.type = buffer.ReadByte();
                face.size = buffer.ReadByte();
                face.side = buffer.ReadByte();
                face.alpha = buffer.ReadByte();
                face.verticesCount = 3;
                if (face.type == 36)
                {
                    face.verticesCount = 3;
                }
                else if (face.type == 44)
                {
                    face.verticesCount = 4;
                }

                face.vertices = new List<ushort>();
                for (uint j = 0; j < face.verticesCount; j++)
                {
                    ushort vId = (ushort)(buffer.ReadUInt16() / 4);
                    face.vertices.Add(vId);
                }
                face.uv = new List<Vector2>();
                for (uint j = 0; j < face.verticesCount; j++)
                {
                    int u = buffer.ReadByte();
                    int v = buffer.ReadByte();
                    face.uv.Add(new Vector2(u, v));
                }

                Faces[i] = face;
            }

            // Textures section
            if (buffer.BaseStream.Position != PtrTextureSection)
            {
                buffer.BaseStream.Position = PtrTextureSection;
            }

            TIM = ScriptableObject.CreateInstance<TIM.TIM>();
            TIM.name = Filename + ".WEP.TIM";
            TIM.Filename = this.Filename;
            TIM.ParseWEPFromBuffer(buffer);

            //ToolBox.SaveScriptableObject("Assets/Resources/Serialized/TIM/WEP/", Filename + ".yaml.asset", TIM);

            // rotations
            // its look like SEQAnim rotationPerBone
            Footer = buffer.ReadBytes(24);
        }
    }
}
