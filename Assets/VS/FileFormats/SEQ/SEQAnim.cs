using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VS.FileFormats.GEOM;
using VS.Utils;

namespace VS.FileFormats.SEQ
{
    [Serializable]
    public class SEQAnim
    {
        public int index;
        public uint length;
        public SByte baseAnimationId = -1;
        public Byte scaleFlags;
        public uint ptrActions;
        public uint ptrTrans;

        public uint[] ptrBoneRots;
        public uint[] ptrBoneScales;
        public uint numBones;
        public Vector3 trans;
        public Vector4[] transKeys;
        public SEQAnim baseAnim;
        public List<SEQAction> actions;
        public Vector3[] rotationPerBone;
        public Vector4Col[] rotationKeysPerBone;
        public Vector3[] scalePerBone;
        public Vector4Col[] scaleKeysPerBone;


        public void ParseFromBuffer(BinaryReader buffer)
        {
            length = buffer.ReadUInt16();
            baseAnimationId = buffer.ReadSByte();
            scaleFlags = buffer.ReadByte();
            ptrActions = buffer.ReadUInt16();
            ptrTrans = buffer.ReadUInt16();
            buffer.ReadUInt16(); // padding
            ptrBoneRots = new uint[numBones];
            ptrBoneScales = new uint[numBones];

            for (int i = 0; i < numBones; i++)
            {
                ptrBoneRots[i] = buffer.ReadUInt16();
            }
            for (int i = 0; i < numBones; i++)
            {
                ptrBoneScales[i] = buffer.ReadUInt16();
            }
        }

        public void GetData(BinaryReader buffer, SEQ seq)
        {
            long localPtr = seq.PtrData(ptrTrans);
            buffer.BaseStream.Position = localPtr;

            short x = BitConverter.ToInt16(ToolBox.EndianSwitcher(buffer.ReadBytes(2)), 0);
            short y = BitConverter.ToInt16(ToolBox.EndianSwitcher(buffer.ReadBytes(2)), 0);
            short z = BitConverter.ToInt16(ToolBox.EndianSwitcher(buffer.ReadBytes(2)), 0);

            trans = new Vector3(x, y, z);
            transKeys = ReadKeys(buffer);


            if (ptrActions > 0)
            {
                buffer.BaseStream.Position = seq.PtrData(ptrActions);
                ReadActions(buffer);
            }


            rotationPerBone = new Vector3[(int)numBones];
            rotationKeysPerBone = new Vector4Col[(int)numBones];
            scalePerBone = new Vector3[(int)numBones];
            scaleKeysPerBone = new Vector4Col[(int)numBones];

            for (int i = 0; i < numBones; i++)
            {
                long localPtr2 = seq.PtrData(ptrBoneRots[i]);
                buffer.BaseStream.Position = localPtr2;

                if (baseAnimationId == -1)
                {
                    ushort rx = BitConverter.ToUInt16(ToolBox.EndianSwitcher(buffer.ReadBytes(2)), 0);
                    ushort ry = BitConverter.ToUInt16(ToolBox.EndianSwitcher(buffer.ReadBytes(2)), 0);
                    ushort rz = BitConverter.ToUInt16(ToolBox.EndianSwitcher(buffer.ReadBytes(2)), 0);
                    rotationPerBone[i] = new Vector3(rx, ry, rz);
                }
                rotationKeysPerBone[i] = new Vector4Col();
                rotationKeysPerBone[i].col = ReadKeys(buffer);
                //Debug.Log("rotationKeysPerBone[i].Count : " + rotationKeysPerBone[i].Count);

                long localPtr3 = seq.PtrData(ptrBoneScales[i]);
                buffer.BaseStream.Position = localPtr3;

                if ((byte)(scaleFlags & 0x1) > 0)
                {
                    scalePerBone[i] = new Vector3(buffer.ReadByte(), buffer.ReadByte(), buffer.ReadByte());
                }
                if ((byte)(scaleFlags & 0x2) > 0)
                {
                    scaleKeysPerBone[i] = new Vector4Col();
                    scaleKeysPerBone[i].col = ReadKeys(buffer);
                }
            }
        }


        private Vector4[] ReadKeys(BinaryReader buffer)
        {
            List<Vector4> keys = new List<Vector4>();
            keys.Add(Vector4.zero); // Unity need this for root bone i think
            int f = 0;
            while (true)
            {
                Vector4? key = ReadKey(buffer);
                if (key == null) break;
                keys.Add((Vector4)key);
                f += (int)keys[keys.Count-1].w;
                if (f >= length - 1) break;
            }
            return keys.ToArray();
        }

        private Vector4? ReadKey(BinaryReader buffer)
        {
            byte code = buffer.ReadByte();
            if (code == 0x00) return null;

            Vector4 key = new Vector4();

            if ((code & 0xe0) > 0)
            {
                // number of frames, byte case

                key.w = code & 0x1f;

                if (key.w == 0x1f)
                {
                    key.w = 0x20 + buffer.ReadByte();
                }
                else
                {
                    key.w = 1 + key.w;
                }
            }
            else
            {
                // number of frames, half word case

                key.w = code & 0x3;

                if (key.w == 0x3)
                {
                    key.w = 4 + buffer.ReadByte();
                }
                else
                {
                    key.w = 1 + key.w;
                }

                // half word values

                code = (byte)(code << 3);

                short h = BitConverter.ToInt16(ToolBox.EndianSwitcher(buffer.ReadBytes(2)), 0);

                if ((h & 0x4) > 0)
                {
                    key.x = h >> 3;
                    code = (byte)(code & 0x60);

                    if ((h & 0x2) > 0)
                    {
                        key.y = BitConverter.ToInt16(ToolBox.EndianSwitcher(buffer.ReadBytes(2)), 0);
                        code = (byte)(code & 0xa0);
                    }

                    if ((h & 0x1) > 0)
                    {
                        key.z = BitConverter.ToInt16(ToolBox.EndianSwitcher(buffer.ReadBytes(2)), 0);
                        code = (byte)(code & 0xc0);
                    }
                }
                else if ((h & 0x2) > 0)
                {
                    key.y = h >> 3;
                    code = (byte)(code & 0xa0);

                    if ((h & 0x1) > 0)
                    {
                        key.z = BitConverter.ToInt16(ToolBox.EndianSwitcher(buffer.ReadBytes(2)), 0);
                        code = (byte)(code & 0xc0);
                    }
                }
                else if ((h & 0x1) > 0)
                {
                    key.z = h >> 3;
                    code = (byte)(code & 0xc0);
                }
            }

            // byte values (fallthrough)

            if ((code & 0x80) > 0)
            {
                key.x = buffer.ReadSByte();
            }

            if ((code & 0x40) > 0)
            {
                key.y = buffer.ReadSByte();
            }

            if ((code & 0x20) > 0)
            {
                key.z = buffer.ReadSByte();
            }
            //Debug.Log(key.ToString());
            return key;
        }


        private void ReadActions(BinaryReader buffer)
        {
            actions = new List<SEQAction>();

            while (true)
            {
                byte f = buffer.ReadByte(); // frame number or 0xff

                // TODO probably wrong to break here
                if (f == 0xff) break;

                if (f > length)
                {
                    Debug.Log("Unexpected frame number f:" + f + " > length:" + length + " in SEQ action section");
                }

                byte a = buffer.ReadByte(); // action

                if (a == 0x00) return;

                SEQAction action = SEQAction.GetAction(a);

                if (action == null)
                {
                    Debug.Log("Unknown SEQ action " + a + " at frame " + f);
                }
                else
                {
                    //Debug.Log(action.name);
                }

                byte[] parameters;
                if (action.count > 0)
                {
                    parameters = new byte[action.count];

                    for (int i = 0; i < action.count; ++i)
                    {
                        parameters[i] = buffer.ReadByte();
                    }
                    action.paremeters = parameters;
                }

                action.f = f;
                actions.Add(action);
            }
        }

        public Quaternion GetFirstBoneRotation(int boneId)
        {
            Vector4[] keyframes = rotationKeysPerBone[boneId].col;
            Vector3 pose = rotationPerBone[boneId];
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

            return aquat;
        }
    }
}
