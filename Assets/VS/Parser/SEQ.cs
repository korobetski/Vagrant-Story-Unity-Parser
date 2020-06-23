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
            numBones = buffer.ReadByte();
            buffer.ReadByte(); // padding
            uint size = buffer.ReadUInt32();
            uint dataOffset = buffer.ReadUInt32() + 8;
            long slotOffset = buffer.ReadUInt32() + 8;
            long headerOffset = slotOffset + numSlots;



            numAnimations = (headerOffset - numSlots - 16) / (numBones * 4 + 10);
            //Debug.Log("numAnimations : "+ numAnimations);
            animations = new VSAnim[numAnimations];
            for (int i = 0; i < numAnimations; i++)
            {
                animations[i] = new VSAnim(i, numBones, buffer);
            }

            int[] slots = new int[numSlots];
            for (int i = 0; i < numSlots; i++)
            {
                byte slot = buffer.ReadByte();
                if (slot >= numAnimations && slot != 255)
                {
                    Debug.Log('?');
                }
                slots[i] = slot;
            }

            for (int i = 0; i < numAnimations; i++)
            {
                animations[i].getData(buffer, basePtr, headerOffset, animations);
            }
        }

        public void FirstPoseModel(GameObject model)
        {
            for (int j = 0; j < numBones; j++)
            {
                if (j < animations[1].rotationKeysPerBone.Length)
                {
                    //Debug.Log("keyframes.Count : " + animations[0].rotationKeysPerBone.Length);
                    List<NVector4> keyframes = animations[0].rotationKeysPerBone[j];
                    Vector3 pose = animations[0].rotationPerBone[j];
                    int rx = (int)pose.x * 2;
                    int ry = (int)pose.y * 2;
                    int rz = (int)pose.z * 2;

                    //Debug.Log("keyframes.Count : "+keyframes.Count);
                    int f = (int)keyframes[0].f;

                    rx += ((int)keyframes[0].x * f);
                    ry += ((int)keyframes[0].y * f);
                    rz += ((int)keyframes[0].z * f);

                    Quaternion qu = ToolBox.quatFromAxisAnle(Vector3.right, ToolBox.rot13toRad(rx));
                    Quaternion qv = ToolBox.quatFromAxisAnle(Vector3.up, ToolBox.rot13toRad(ry));
                    Quaternion qw = ToolBox.quatFromAxisAnle(Vector3.forward, ToolBox.rot13toRad(rz));
                    Quaternion quat = qw * qv * qu;
                    Quaternion aquat = new Quaternion(quat.x, quat.y, quat.z, quat.w);

                    GameObject bone = ToolBox.findBoneIn("bone_" + j, model);
                    bone.transform.localRotation = aquat;
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

                // Translation
                int tkl = animations[i].transKeys.Count;
                List<NVector4> keyframes = animations[i].transKeys;
                /*
                Debug.Log("animations["+i+"].trans : "+ animations[i].trans.ToString());
                int tx = (int)animations[i].trans.x/64;
                int ty = (int)animations[i].trans.y/64;
                int tz = (int)animations[i].trans.z/64;
                */
                float tx = 0;
                float ty = 0;
                float tz = 0;

                List<Keyframe> keysTX = new List<Keyframe>();
                List<Keyframe> keysTY = new List<Keyframe>();
                List<Keyframe> keysTZ = new List<Keyframe>();

                t = 0;
                for (int k = 0; k < tkl; k++)
                {
                    int f = (int)keyframes[k].f;
                    t += f;
                    if (keyframes[k].x == null)
                    {
                        keyframes[k].x = keyframes[k - 1].x;
                    }

                    if (keyframes[k].y == null)
                    {
                        keyframes[k].y = keyframes[k - 1].y;
                    }

                    if (keyframes[k].z == null)
                    {
                        keyframes[k].z = keyframes[k - 1].z;
                    }


                    tx += (float)keyframes[k].x/64 * f;
                    ty += (float)keyframes[k].y/64 * f;
                    tz += (float)keyframes[k].z/64 * f;

                    keysTX.Add(new Keyframe((float)((t) * 0.04), tx));
                    keysTY.Add(new Keyframe((float)((t) * 0.04), ty));
                    keysTZ.Add(new Keyframe((float)((t) * 0.04), tz));

                }
                clip.SetCurve("", typeof(Transform), "localPosition.x", new AnimationCurve(keysTX.ToArray()));
                clip.SetCurve("", typeof(Transform), "localPosition.y", new AnimationCurve(keysTY.ToArray()));
                clip.SetCurve("", typeof(Transform), "localPosition.z", new AnimationCurve(keysTZ.ToArray()));

                // Bones rotation and scale
                for (int j = 0; j < numBones; j++)
                {
                    List<Keyframe> keysRX = new List<Keyframe>();
                    List<Keyframe> keysRY = new List<Keyframe>();
                    List<Keyframe> keysRZ = new List<Keyframe>();
                    List<Keyframe> keysRW = new List<Keyframe>();
                    List<Keyframe> keysSX = new List<Keyframe>();
                    List<Keyframe> keysSY = new List<Keyframe>();
                    List<Keyframe> keysSZ = new List<Keyframe>();

                    VSAnim baseAnim = (animations[i].baseAnimationId == -1) ? animations[i] : animations[animations[i].baseAnimationId];

                    if (j < animations[i].rotationKeysPerBone.Length)
                    {
                        keyframes = animations[i].rotationKeysPerBone[j];
                        Vector3 pose = baseAnim.rotationPerBone[j];
                        int rx = (int)pose.x * 2;
                        int ry = (int)pose.y * 2;
                        int rz = (int)pose.z * 2;
                        t = 0;
                        int kfl = animations[i].rotationKeysPerBone[j].Count;

                        for (int k = 0; k < kfl; k++)
                        {
                            int f = (int)keyframes[k].f;
                            t += f;
                            if (keyframes[k].x == null)
                            {
                                keyframes[k].x = keyframes[k - 1].x;
                            }

                            if (keyframes[k].y == null)
                            {
                                keyframes[k].y = keyframes[k - 1].y;
                            }

                            if (keyframes[k].z == null)
                            {
                                keyframes[k].z = keyframes[k - 1].z;
                            }

                            rx += ((int)keyframes[k].x * f);
                            ry += ((int)keyframes[k].y * f);
                            rz += ((int)keyframes[k].z * f);

                            Quaternion qu = ToolBox.quatFromAxisAnle(Vector3.right, ToolBox.rot13toRad(rx));
                            Quaternion qv = ToolBox.quatFromAxisAnle(Vector3.up, ToolBox.rot13toRad(ry));
                            Quaternion qw = ToolBox.quatFromAxisAnle(Vector3.forward, ToolBox.rot13toRad(rz));
                            Quaternion quat = qw * qv * qu;

                            keysRX.Add(new Keyframe((float)((t) * 0.04), quat.x));
                            keysRY.Add(new Keyframe((float)((t) * 0.04), quat.y));
                            keysRZ.Add(new Keyframe((float)((t) * 0.04), quat.z));
                            keysRW.Add(new Keyframe((float)((t) * 0.04), quat.w));

                        }
                    }
                    if (j < animations[i].scaleKeysPerBone.Length)
                    {
                        Vector3 baseScale = ((animations[i].scaleFlags & 0x1) > 0) ? animations[i].scalePerBone[j] : new Vector3(64, 64, 64);
                        List<NVector4> scaleKeys = ((animations[i].scaleFlags & 0x2) > 0) ? animations[i].scaleKeysPerBone[j] : new List<NVector4>();
                        int skl = scaleKeys.Count;
                        if (skl == 0) scaleKeys.Add(NVector4.zero);
                        skl = scaleKeys.Count;
                        Vector3 scale = baseScale / 64;
                        t = 0;
                        for (int k = 0; k < skl; k++)
                        {
                            NVector4 key = scaleKeys[k];
                            int f = (int)key.f;
                            if (key.x == null) key.x = scaleKeys[i - 1].x;
                            if (key.y == null) key.y = scaleKeys[i - 1].y;
                            if (key.z == null) key.z = scaleKeys[i - 1].z;
                            t += f;
                            scale.x += (float)key.x / 64 * f;
                            scale.y += (float)key.y / 64 * f;
                            scale.z += (float)key.z / 64 * f;

                            keysSX.Add(new Keyframe((float)((t) * 0.04), scale.x));
                            keysSY.Add(new Keyframe((float)((t) * 0.04), scale.y));
                            keysSZ.Add(new Keyframe((float)((t) * 0.04), scale.z));
                        }
                    }
                    string bonePath = ToolBox.GetGameObjectPath(ToolBox.findBoneIn("bone_" + j, model), model.name);
                    clip.SetCurve(bonePath, typeof(Transform), "localRotation.x", new AnimationCurve(keysRX.ToArray()));
                    clip.SetCurve(bonePath, typeof(Transform), "localRotation.y", new AnimationCurve(keysRY.ToArray()));
                    clip.SetCurve(bonePath, typeof(Transform), "localRotation.z", new AnimationCurve(keysRZ.ToArray()));
                    clip.SetCurve(bonePath, typeof(Transform), "localRotation.w", new AnimationCurve(keysRW.ToArray()));
                    clip.SetCurve(bonePath, typeof(Transform), "localScale.x", new AnimationCurve(keysSX.ToArray()));
                    clip.SetCurve(bonePath, typeof(Transform), "localScale.y", new AnimationCurve(keysSY.ToArray()));
                    clip.SetCurve(bonePath, typeof(Transform), "localScale.z", new AnimationCurve(keysSZ.ToArray()));
                }
                clips[i] = clip;
            }
            return clips;
        }
    }

    public class VSAnim
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
        public List<NVector4> transKeys;
        public VSAnim baseAnim;
        public List<VSAction> actions;
        public Vector3[] rotationPerBone;
        public List<NVector4>[] rotationKeysPerBone;
        public Vector3[] scalePerBone;
        public List<NVector4>[] scaleKeysPerBone;

        public VSAnim(int index, uint numBones, BinaryReader buffer)
        {
            this.index = index;
            this.numBones = numBones;
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
            /*
            Debug.Log("VSAnim -> index : "+index+" , numBones : "+numBones+ ", length : "+ length + ", baseAnimationId : "+ baseAnimationId +
                ", scaleFlags : "+ scaleFlags+ ", ptrActions : "+ ptrActions+ ", ptrTrans : "+ ptrTrans);
            */
        }

        public void getData(BinaryReader buffer, long basePtr, long dataPtr, VSAnim[] animations)
        {
            long localPtr = ptrTrans + basePtr + dataPtr;
            buffer.BaseStream.Position = localPtr;

            short x = BitConverter.ToInt16(ToolBox.EndianSwitcher(buffer.ReadBytes(2)), 0);
            short y = BitConverter.ToInt16(ToolBox.EndianSwitcher(buffer.ReadBytes(2)), 0);
            short z = BitConverter.ToInt16(ToolBox.EndianSwitcher(buffer.ReadBytes(2)), 0);

            trans = new Vector3(x, y, z);
            transKeys = ReadKeys(buffer);


            if (ptrActions > 0)
            {
                buffer.BaseStream.Position = ptrActions + basePtr + dataPtr;
                ReadActions(buffer);
            }


            rotationPerBone = new Vector3[(int)numBones];
            rotationKeysPerBone = new List<NVector4>[(int)numBones];
            scalePerBone = new Vector3[(int)numBones];
            scaleKeysPerBone = new List<NVector4>[(int)numBones];

            for (int i = 0; i < numBones; i++)
            {
                long localPtr2 = ptrBoneRots[i] + basePtr + dataPtr;
                buffer.BaseStream.Position = localPtr2;

                if (baseAnimationId == -1)
                {
                    ushort rx = BitConverter.ToUInt16(ToolBox.EndianSwitcher(buffer.ReadBytes(2)), 0);
                    ushort ry = BitConverter.ToUInt16(ToolBox.EndianSwitcher(buffer.ReadBytes(2)), 0);
                    ushort rz = BitConverter.ToUInt16(ToolBox.EndianSwitcher(buffer.ReadBytes(2)), 0);
                    rotationPerBone[i] = new Vector3(rx, ry, rz);
                }
                rotationKeysPerBone[i] = ReadKeys(buffer);
                //Debug.Log("rotationKeysPerBone[i].Count : " + rotationKeysPerBone[i].Count);

                long localPtr3 = ptrBoneScales[i] + basePtr + dataPtr;
                buffer.BaseStream.Position = localPtr3;

                if ((byte)(scaleFlags & 0x1) > 0)
                {
                    scalePerBone[i] = new Vector3(buffer.ReadByte(), buffer.ReadByte(), buffer.ReadByte());
                }
                if ((byte)(scaleFlags & 0x2) > 0)
                {
                    scaleKeysPerBone[i] = ReadKeys(buffer);
                }
            }
        }


        public List<NVector4> ReadKeys(BinaryReader buffer)
        {
            List<NVector4> keys = new List<NVector4>();
            keys.Add(NVector4.zero); // Unity need this for root bone i think
            int f = 0;
            while(true)
            {
                NVector4 key = ReadKey(buffer);
                if (key == null) break;
                keys.Add(key);
                if (key.f.HasValue)
                {
                    f += (int)key.f;
                }
                
                if (f >= length - 1) break;
            }
            return keys;
        }


        private NVector4 ReadKey(BinaryReader buffer)
        {
            byte code = buffer.ReadByte();
            if (code == 0x00) return null;

            NVector4 key = new NVector4();

            if ((code & 0xe0) > 0)
            {
                // number of frames, byte case

                key.f = code & 0x1f;

                if (key.f == 0x1f)
                {
                    key.f = 0x20 + buffer.ReadByte();
                }
                else
                {
                    key.f = 1 + key.f;
                }
            }
            else
            {
                // number of frames, half word case

                key.f = code & 0x3;

                if (key.f == 0x3)
                {
                    key.f = 4 + buffer.ReadByte();
                }
                else
                {
                    key.f = 1 + key.f;
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
                if (key.x != null)
                {
                    Debug.Log("Expected undefined x in SEQ animation data");
                }

                key.x = buffer.ReadSByte();
            }

            if ((code & 0x40) > 0)
            {
                if (key.y != null)
                {
                    Debug.Log("Expected undefined y in SEQ animation data");
                }

                key.y = buffer.ReadSByte();
            }

            if ((code & 0x20) > 0)
            {
                if (key.z != null)
                {
                    Debug.Log("Expected undefined z in SEQ animation data");
                }

                key.z = buffer.ReadSByte();
            }
            //Debug.Log(key.ToString());
            return key;

        }


        private VSAction GetAction(byte a)
        {
            // TODO : make a switch statement
            VSAction[] ACTIONS = new VSAction[0x50];

            ACTIONS[0x01] = new VSAction("loop", 0); // verified
            ACTIONS[0x02] = new VSAction("0x02", 0); // often at end, used for attack animations
            ACTIONS[0x04] = new VSAction("0x04", 1); //
            ACTIONS[0x0a] = new VSAction("0x0a", 1); // verified in 00_COM (no other options, 0x00 x00 follows)
            ACTIONS[0x0b] = new VSAction("0x0b", 0); // pretty sure, used with walk/run, followed by 0x17/left, 0x18/right
            ACTIONS[0x0c] = new VSAction("0x0c", 1);
            ACTIONS[0x0d] = new VSAction("0x0d", 0);
            ACTIONS[0x0f] = new VSAction("0x0f", 1); // first
            ACTIONS[0x13] = new VSAction("unlockBone", 1); // verified in emulation
            ACTIONS[0x14] = new VSAction("0x14", 1); // often at end of non-looping
            ACTIONS[0x15] = new VSAction("0x15", 1); // verified 00_COM (no other options, 0x00 0x00 follows)
            ACTIONS[0x16] = new VSAction("0x16", 2); // first, verified 00_BT3
            ACTIONS[0x17] = new VSAction("0x17", 0); // + often at end
            ACTIONS[0x18] = new VSAction("0x18", 0); // + often at end
            ACTIONS[0x19] = new VSAction("0x19", 0);// first, verified 00_COM (no other options, 0x00 0x00 follows)
            ACTIONS[0x1a] = new VSAction("0x1a", 1); // first, verified 00_BT1 (0x00 0x00 follows)
            ACTIONS[0x1b] = new VSAction("0x1b", 1); // first, verified 00_BT1 (0x00 0x00 follows)
            ACTIONS[0x1c] = new VSAction("0x1c", 1);
            ACTIONS[0x1d] = new VSAction("paralyze?", 0); // first, verified 1C_BT1
            ACTIONS[0x24] = new VSAction("0x24", 2); // first
            ACTIONS[0x27] = new VSAction("0x27", 4); // first, verified see 00_COM
            ACTIONS[0x34] = new VSAction("0x34", 3); // first
            ACTIONS[0x35] = new VSAction("0x35", 5); // first
            ACTIONS[0x36] = new VSAction("0x36", 3);
            ACTIONS[0x37] = new VSAction("0x37", 1); // pretty sure
            ACTIONS[0x38] = new VSAction("0x38", 1);
            ACTIONS[0x39] = new VSAction("0x39", 1);
            ACTIONS[0x3a] = new VSAction("disappear", 0); // used in death animations
            ACTIONS[0x3b] = new VSAction("land", 0);
            ACTIONS[0x3c] = new VSAction("adjustShadow", 1); // verified
            ACTIONS[0x3f] = new VSAction("0x3f", 0); // first, pretty sure, often followed by 0x16
            ACTIONS[0x40] = new VSAction("0x40", 0); // often preceded by 0x1a, 0x1b, often at end

            return ACTIONS[a];
        }

        private void ReadActions(BinaryReader buffer)
        {
            actions = new List<VSAction>();

            while (true)
            {
                byte f = buffer.ReadByte(); // frame number or 0xff

                // TODO probably wrong to break here
                if (f == 0xff) break;

                if (f > length)
                {
                    Debug.Log("Unexpected frame number f:"+f+" > length:"+ length + " in SEQ action section");
                }

                byte a = buffer.ReadByte(); // action

                if (a == 0x00) return;

                VSAction action = GetAction(a);

                if (action == null)
                {
                    Debug.Log("Unknown SEQ action "+a+" at frame "+f);
                } else
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

    }

    public class VSAction
    {
        public byte f;
        public string name;
        public int count = 0;
        public byte[] paremeters;

        public VSAction(string n, int c)
        {
            name = n;
            count = c;
        }
    }

    public class NVector4
    {
        public int? x;
        public int? y;
        public int? z;
        public int? f;
        public static NVector4 zero = new NVector4(0, 0, 0, 0);
        public static NVector4 one = new NVector4(1, 1, 1, 1);

        public NVector4()
        {
        }

        public NVector4(int x, int y, int z, int f)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.f = f;
        }

        public new string ToString()
        {
            return "NVector f:"+f+", x:"+x+", y:"+y+", z:"+z;
        }
    }
}