using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VS.Utils;


// http://datacrystal.romhacking.net/wiki/Vagrant_Story:SEQ_files
// 

namespace VS.Parser
{
    public class SEQ : FileParser
    {
        public VSAnim[] animations;
        public AnimationClip[] clips;

        private uint numBones;
        private long numAnimations;

        public SEQ()
        {

        }
        public void Parse(string filePath)
        {
            PreParse(filePath);
            Parse(buffer);

            buffer.Close();
            fileStream.Close();
        }
        public void Parse(BinaryReader buffer)
        {
            long basePtr = buffer.BaseStream.Position;
            uint numSlots = buffer.ReadUInt16();
            numBones = buffer.ReadUInt16();
            uint size = buffer.ReadUInt32();
            uint h3 = buffer.ReadUInt32();
            long slotPtr = buffer.ReadUInt32() + 8;
            long dataPtr = slotPtr + numSlots;

            numAnimations = (dataPtr - numSlots - 16) / (numBones * 4 + 10);
            animations = new VSAnim[numAnimations];
            for (int i = 0; i < numAnimations; i++)
            {
                animations[i] = new VSAnim(i, numBones, buffer);
            }

            int[] slots = new int[numSlots];
            for (int i = 0; i < numSlots; i++)
            {
                slots[i] = buffer.ReadSByte();
            }

            for (int i = 0; i < numAnimations; i++)
            {
                animations[i].getData(buffer, basePtr, dataPtr, animations);
            }
        }

        public void FirstPoseModel(GameObject model)
        {

            int t = 0;

            for (int j = 0; j < numBones; j++)
            {
                if (j < animations[0].keyframes.Length)
                {
                    List<NVector4> keyframes = animations[0].keyframes[j];
                    Vector3 pose = animations[0].pose[j];
                    int rx = (int)pose.x * 2;
                    int ry = (int)pose.y * 2;
                    int rz = (int)pose.z * 2;
                    t = 0;
                    int k = 0;
                    int f = keyframes[k].w;
                    t += f;
                    if (keyframes[k].x == 256)
                    {
                        keyframes[k].x = keyframes[k - 1].x;
                    }

                    if (keyframes[k].y == 256)
                    {
                        keyframes[k].y = keyframes[k - 1].y;
                    }

                    if (keyframes[k].z == 256)
                    {
                        keyframes[k].z = keyframes[k - 1].z;
                    }

                    rx += (keyframes[k].x * f);
                    ry += (keyframes[k].y * f);
                    rz += (keyframes[k].z * f);

                    Quaternion qu = ToolBox.quatFromAxisAnle(Vector3.right, ToolBox.rot13toRad(rx));
                    Quaternion qv = ToolBox.quatFromAxisAnle(Vector3.up, ToolBox.rot13toRad(ry));
                    Quaternion qw = ToolBox.quatFromAxisAnle(Vector3.forward, ToolBox.rot13toRad(rz));
                    Quaternion quat = qw * qv * qu;
                    Quaternion aquat = new Quaternion(quat.x, quat.y, quat.z, quat.w);

                    GameObject bone = ToolBox.findBoneIn("bone_" + j, model);
                    bone.transform.localRotation = aquat;
                    /*
                    if (j == 0)
                    {
                        // Upside down root bone
                        Vector3 rot = bone.transform.eulerAngles;
                        bone.transform.eulerAngles = new Vector3(rot.x + 180, rot.y, rot.z);
                    }
                    */
                }
            }
        }

        public AnimationClip[] BuildAnimationClips(GameObject model)
        {
            if (UseDebug)
            {
                Debug.Log("BuildAnimationClips : " + FileName + "(" + numAnimations + ")  in model " + model.name);
            }


            clips = new AnimationClip[numAnimations];
            int t = 0;
            for (int i = 0; i < numAnimations; i++)
            {
                AnimationClip clip = new AnimationClip();
                clip.name = FileName + "_c_" + i;

                for (int j = 0; j < numBones; j++)
                {
                    List<Keyframe> keysRX = new List<Keyframe>();
                    List<Keyframe> keysRY = new List<Keyframe>();
                    List<Keyframe> keysRZ = new List<Keyframe>();
                    List<Keyframe> keysRW = new List<Keyframe>();

                    if (j < animations[i].keyframes.Length)
                    {
                        List<NVector4> keyframes = animations[i].keyframes[j];
                        Vector3 pose = animations[i].pose[j];
                        int rx = (int)pose.x * 2;
                        int ry = (int)pose.y * 2;
                        int rz = (int)pose.z * 2;
                        t = 0;
                        int kfl = animations[i].keyframes[j].Count;

                        for (int k = 0; k < kfl; k++)
                        {
                            int f = keyframes[k].w;
                            t += f;
                            if (keyframes[k].x == 256)
                            {
                                keyframes[k].x = keyframes[k - 1].x;
                            }

                            if (keyframes[k].y == 256)
                            {
                                keyframes[k].y = keyframes[k - 1].y;
                            }

                            if (keyframes[k].z == 256)
                            {
                                keyframes[k].z = keyframes[k - 1].z;
                            }

                            rx += (keyframes[k].x * f);
                            ry += (keyframes[k].y * f);
                            rz += (keyframes[k].z * f);

                            Quaternion qu = ToolBox.quatFromAxisAnle(Vector3.right, ToolBox.rot13toRad(rx));
                            Quaternion qv = ToolBox.quatFromAxisAnle(Vector3.up, ToolBox.rot13toRad(ry));
                            Quaternion qw = ToolBox.quatFromAxisAnle(Vector3.forward, ToolBox.rot13toRad(rz));
                            Quaternion quat = qw * qv * qu;
                            /*
                            if (j == 0)
                            {
                                keysRX.Add(new Keyframe((float)((t) * 0.06), (quat.x + 180f))); // flip the root bone
                            }
                            else
                            {
                                keysRX.Add(new Keyframe((float)((t) * 0.06), quat.x));
                            }
                            */
                            keysRX.Add(new Keyframe((float)((t) * 0.06), quat.x));
                            keysRY.Add(new Keyframe((float)((t) * 0.06), quat.y));
                            keysRZ.Add(new Keyframe((float)((t) * 0.06), quat.z));
                            keysRW.Add(new Keyframe((float)((t) * 0.06), quat.w));

                        }
                    }

                    string bonePath = ToolBox.GetGameObjectPath(ToolBox.findBoneIn("bone_" + j, model), model.name);
                    clip.SetCurve(bonePath, typeof(Transform), "localRotation.x", new AnimationCurve(keysRX.ToArray()));
                    clip.SetCurve(bonePath, typeof(Transform), "localRotation.y", new AnimationCurve(keysRY.ToArray()));
                    clip.SetCurve(bonePath, typeof(Transform), "localRotation.z", new AnimationCurve(keysRZ.ToArray()));
                    clip.SetCurve(bonePath, typeof(Transform), "localRotation.w", new AnimationCurve(keysRW.ToArray()));
                }
                clips[i] = clip;
            }
            return clips;
        }
    }


    public class VSAnim
    {
        public int index;
        public int length;
        public int idOtherAnim = -1;
        public int mode;
        public int ptr1;
        public int ptrTrans;
        public int ptrMove;
        public int[] ptrBones;
        public uint numBones;
        public Vector3 trans;
        public VSAnim baseAnim;
        public List<NVector4>[] keyframes;
        public Vector3[] pose;

        public VSAnim(int index, uint numBones, BinaryReader buffer)
        {
            this.index = index;
            this.numBones = numBones;
            length = buffer.ReadUInt16();
            idOtherAnim = buffer.ReadSByte();
            mode = buffer.ReadByte();
            ptr1 = buffer.ReadUInt16();
            ptrTrans = buffer.ReadUInt16();
            ptrMove = buffer.ReadUInt16();
            ptrBones = new int[numBones];
            for (int i = 0; i < numBones; i++)
            {
                ptrBones[i] = buffer.ReadUInt16();
            }

            for (int i = 0; i < numBones; i++)
            {
                buffer.ReadUInt16();
            }
        }

        public void getData(BinaryReader buffer, long basePtr, long dataPtr, VSAnim[] animations)
        {
            long localPtr = ptrTrans + basePtr + dataPtr;
            buffer.BaseStream.Position = localPtr;
            short x = BitConverter.ToInt16(ToolBox.EndianSwitcher(buffer.ReadBytes(2)), 0);
            short y = BitConverter.ToInt16(ToolBox.EndianSwitcher(buffer.ReadBytes(2)), 0);
            short z = BitConverter.ToInt16(ToolBox.EndianSwitcher(buffer.ReadBytes(2)), 0);
            trans = new Vector3(x, y, z);

            baseAnim = this;
            if (idOtherAnim != -1)
            {
                baseAnim = animations[idOtherAnim];
            }

            keyframes = new List<NVector4>[numBones];
            pose = new Vector3[numBones];
            for (int i = 0; i < numBones; i++)
            {
                keyframes[i] = new List<NVector4>();
                keyframes[i].Add(NVector4.zero);

                long localPtr2 = baseAnim.ptrBones[i] + basePtr + dataPtr;
                buffer.BaseStream.Position = localPtr2;

                ushort rx = BitConverter.ToUInt16(ToolBox.EndianSwitcher(buffer.ReadBytes(2)), 0);
                ushort ry = BitConverter.ToUInt16(ToolBox.EndianSwitcher(buffer.ReadBytes(2)), 0);
                ushort rz = BitConverter.ToUInt16(ToolBox.EndianSwitcher(buffer.ReadBytes(2)), 0);
                pose[i] = new Vector3(rx, ry, rz);

                uint f = 0;
                while (true)
                {
                    NVector4 op = readOpCode(buffer);
                    if (op == null)
                    {
                        break;
                    }
                    else
                    {
                        if (op.w != 256)
                        {
                            f += (uint)(op.w);
                        }

                        keyframes[i].Add(op);
                        if (f >= length - 1)
                        {
                            break;
                        }
                    }

                }
            }

        }

        public NVector4 readOpCode(BinaryReader buffer)
        {
            int op = buffer.ReadByte();
            int op0 = op;
            if (op == 0)
            {
                return null;
            }

            int x = 256, y = 256, z = 256, f = 256;

            if ((op & 0xe0) > 0)
            {
                // number of frames, byte case
                f = op & 0x1f;

                if (f == 0x1f)
                {
                    f = 0x20 + buffer.ReadByte();
                }
                else
                {
                    f = 1 + f;
                }
            }
            else
            {
                // number of frames, half word case
                f = op & 0x3;
                if (f == 0x3)
                {
                    f = 4 + buffer.ReadByte();
                }
                else
                {
                    f = 1 + f;
                }
                // half word values
                //buffer.CurrentEndian = Endian.Big;
                op = (byte)(op << 3);
                var h = BitConverter.ToInt16(ToolBox.EndianSwitcher(buffer.ReadBytes(2)), 0);

                if ((h & 0x4) > 0)
                {
                    x = h >> 3;
                    op = (byte)(op & 0x60);

                    if ((h & 0x2) > 0)
                    {
                        y = BitConverter.ToInt16(ToolBox.EndianSwitcher(buffer.ReadBytes(2)), 0);
                        op = (byte)(op & 0xa0);
                    }

                    if ((h & 0x1) > 0)
                    {
                        z = BitConverter.ToInt16(ToolBox.EndianSwitcher(buffer.ReadBytes(2)), 0);
                        op = (byte)(op & 0xc0);
                    }

                }
                else if ((h & 0x2) > 0)
                {
                    y = h >> 3;
                    op = (byte)(op & 0xa0);

                    if ((h & 0x1) > 0)
                    {
                        z = BitConverter.ToInt16(ToolBox.EndianSwitcher(buffer.ReadBytes(2)), 0);
                        op = (byte)(op & 0xc0);
                    }
                }
                else if ((h & 0x1) > 0)
                {
                    z = h >> 3;
                    op = (byte)(op & 0xc0);
                }
            }

            // byte values (fallthrough)
            if ((op & 0x80) > 0)
            {
                x = buffer.ReadSByte();
            }

            if ((op & 0x40) > 0)
            {
                y = buffer.ReadSByte();
            }

            if ((op & 0x20) > 0)
            {
                z = buffer.ReadSByte();
            }

            return new NVector4(x, y, z, f);
        }
    }

    public class NVector4
    {
        public int x;
        public int y;
        public int z;
        public int w;
        public static NVector4 zero = new NVector4(0, 0, 0, 0);
        public static NVector4 one = new NVector4(1, 1, 1, 1);

        public NVector4()
        {
            x = 256;
            y = 256;
            z = 256;
            w = 256;
        }

        public NVector4(int x, int y, int z, int w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }
    }


}