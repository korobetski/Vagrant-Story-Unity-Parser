using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VS.Utils;

namespace VS.FileFormats.SEQ
{
    public class SEQ:ScriptableObject
    {
        public string Filename;
        public ushort numSlots;
        public ushort numBones;
        public uint size;
        public uint baseOffset;
        public uint dataOffset; // + 8;
        public uint slotOffset; // + 8;

        public uint headerOffset;
        public uint numAnimations;
        public SEQAnim[] animations;
        public int[] slots;



        public void ParseFromFile(string filepath)
        {
            FileParser fp = new FileParser();
            fp.Read(filepath);

            // in OBJ/***.SEQ
            if (fp.Ext == "SEQ")
            {
                Filename = fp.FileName;
                ParseFromBuffer(fp.buffer, fp.FileSize);
            }

            fp.Close();
        }

        public void ParseFromBuffer(BinaryReader buffer, long limit)
        {

            baseOffset = (uint)buffer.BaseStream.Position;
            numSlots = buffer.ReadUInt16();
            numBones = buffer.ReadUInt16();
            size = buffer.ReadUInt32();
            dataOffset = buffer.ReadUInt32() + 8;
            slotOffset = buffer.ReadUInt32() + 8;
            headerOffset = slotOffset + numSlots;
            numAnimations = (uint)((headerOffset - numSlots - 16) / (numBones * 4 + 10));
            //Debug.Log("numAnimations : "+ numAnimations);
            animations = new SEQAnim[numAnimations];
            for (int i = 0; i < numAnimations; i++)
            {
                animations[i] = new SEQAnim();
                animations[i].index = i;
                animations[i].numBones = numBones;
                animations[i].ParseFromBuffer(buffer);
            }

            slots = new int[numSlots];
            for (int i = 0; i < numSlots; i++)
            {
                byte slot = buffer.ReadByte();
                slots[i] = slot;
            }

            for (int i = 0; i < numAnimations; i++)
            {
                animations[i].GetData(buffer, this);
            }
        }

        public long PtrData(uint i)
        {
            return i + headerOffset + baseOffset;
        }

        public void FirstPoseModel(GameObject model)
        {
            Debug.Log("model : " + model);
            for (int j = 0; j < numBones; j++)
            {
                if (animations.Length > 1)
                {
                    if (j < animations[0].rotationKeysPerBone.Length)
                    {
                        Vector4[] keyframes = animations[0].rotationKeysPerBone[j].col;
                        Vector3 pose = animations[0].rotationPerBone[j];
                        int rx = (int)pose.x * 2;
                        int ry = (int)pose.y * 2;
                        int rz = (int)pose.z * 2;

                        int f = (int)keyframes[0].w;
                        rx += ((int)keyframes[0].x * f);
                        ry += ((int)keyframes[0].y * f);
                        rz += ((int)keyframes[0].z * f);

                        Quaternion qu = ToolBox.quatFromAxisAngle(Vector3.right, ToolBox.rot13toRad(rx));
                        Quaternion qv = ToolBox.quatFromAxisAngle(Vector3.up, ToolBox.rot13toRad(ry));
                        Quaternion qw = ToolBox.quatFromAxisAngle(Vector3.forward, ToolBox.rot13toRad(rz));
                        Quaternion quat = qw * qv * qu;
                        Quaternion aquat = new Quaternion(quat.x, quat.y, quat.z, quat.w);

                        GameObject bone = ToolBox.findBoneIn("bone_" + j, model);
                        bone.transform.rotation = aquat;
                    }
                }
            }
        }

    }
}
