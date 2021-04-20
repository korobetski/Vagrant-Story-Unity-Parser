/*
using System.Collections.Generic;
using System.IO;
using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif
using UnityEngine;
using VS.Serializable;
using VS.Utils;


// http://datacrystal.romhacking.net/wiki/Vagrant_Story:ZUD_files
// Zone Unit Datas in MAP/ folder       Z024U08.ZUD   ->   Zone 024 | Unit 08

namespace VS.Parser
{
    public class ZUD : FileParser
    {
        public string ZoneID = "";
        public string UnitID = "";

        public bool GOBuilded = false;
        public bool PrefabBuilded = false;
        public bool DrawPNG = false;
        public GameObject ZUDGO;

        public Serializable.ZUD so;

        public ZUD()
        {
        }

        public void Parse(string filePath)
        {
            PreParse(filePath);
            ZoneID = FileName.Substring(1, 3);
            UnitID = FileName.Substring(5, 2);

            Parse(buffer);

            buffer.Close();
            fileStream.Close();
        }
        public void Parse(BinaryReader buffer)
        {
            so = ScriptableObject.CreateInstance<Serializable.ZUD>();
            so.idCharacter = buffer.ReadByte();
            so.idWeapon = buffer.ReadByte();
            so.weaponCategory = (WeaponType) buffer.ReadByte();
            so.weaponMaterial = (SmithMaterial.MaterialEnum)buffer.ReadByte();
            so.idShield = buffer.ReadByte();
            so.shieldMaterial = (SmithMaterial.MaterialEnum)buffer.ReadByte();
            so.unk1 = buffer.ReadByte();
            so.pad = buffer.ReadByte();

            // pointers
            so.ptrCharacterSHP = buffer.ReadUInt32();
            so.lenCharacterSHP = buffer.ReadUInt32();
            so.ptrWeaponWEP = buffer.ReadUInt32();
            so.lenWeaponWEP = buffer.ReadUInt32();
            so.ptrShieldWEP = buffer.ReadUInt32();
            so.lenShieldWEP = buffer.ReadUInt32();
            so.ptrCommonSEQ = buffer.ReadUInt32();
            so.lenCommonSEQ = buffer.ReadUInt32();
            so.ptrBattleSEQ = buffer.ReadUInt32();
            so.lenBattleSEQ = buffer.ReadUInt32();

            // shape section
            if (so.lenCharacterSHP > 0)
            {
                if (buffer.BaseStream.Position != so.ptrCharacterSHP)
                {
                    Debug.LogWarning("le pointeur ptrCharacterSHP n'est pas à la bonne place : " + buffer.BaseStream.Position + " != " + ptrCharacterSHP);
                    buffer.BaseStream.Position = so.ptrCharacterSHP;
                }
                zudShape = new SHP();
                zudShape.excpFaces = excpFaces;
                zudShape.FileName = FileName + "_ZSHP";
                zudShape.UseDebug = UseDebug;
                zudShape.Parse(buffer);
            }

            // weapon section
            if (so.lenWeaponWEP > 0)
            {
                if (buffer.BaseStream.Position != so.ptrWeaponWEP)
                {
                    Debug.LogWarning("le pointeur ptrWeaponWEP n'est pas à la bonne place : " + buffer.BaseStream.Position + " != " + ptrWeaponWEP);
                    buffer.BaseStream.Position = so.ptrWeaponWEP;
                }
                zudWeapon = new WEP();
                zudWeapon.FileName = ((byte)idWeapon).ToString();
                zudWeapon.SmithMaterial = idWeaponMaterial;
                zudWeapon.Parse(buffer);
            }

            // shield section
            if (so.lenShieldWEP > 0)
            {
                if (buffer.BaseStream.Position != so.ptrShieldWEP)
                {
                    Debug.LogWarning("le pointeur ptrShieldWEP n'est pas à la bonne place : " + buffer.BaseStream.Position + " != " + ptrShieldWEP);
                    buffer.BaseStream.Position = so.ptrShieldWEP;
                }
                zudShield = new WEP();
                zudShield.FileName = ((byte)idShield).ToString();
                zudShield.SmithMaterial = idShieldMaterial;
                zudShield.Parse(buffer);
            }

            // common anim section
            if (so.lenCommonSEQ > 0)
            {
                if (buffer.BaseStream.Position != so.ptrCommonSEQ)
                {
                    Debug.LogWarning("le pointeur ptrCommonSEQ n'est pas à la bonne place : " + buffer.BaseStream.Position + " != " + ptrCommonSEQ);
                    buffer.BaseStream.Position = so.ptrCommonSEQ;
                }
                zudComSeq = new SEQ();
                zudComSeq.FileName = FileName + "_COM_SEQ";
                zudComSeq.Parse(buffer);
            }

            // battle anim section
            if (so.lenBattleSEQ > 0)
            {
                if (buffer.BaseStream.Position != so.ptrBattleSEQ)
                {
                    Debug.LogWarning("le pointeur ptrBattleSEQ n'est pas à la bonne place : " + buffer.BaseStream.Position + " != " + ptrBattleSEQ);
                    buffer.BaseStream.Position = so.ptrBattleSEQ;
                }
                zudBatSeq = new SEQ();
                zudBatSeq.FileName = FileName + "_BAT_SEQ";
                zudBatSeq.Parse(buffer);
            }

        }

        public void BuildPrefab(bool erase = false)
        {
            ToolBox.DirExNorCreate("Assets/Resources/Prefabs/Zones/");
            string zoneFolder = "Assets/Resources/Prefabs/Zones/Zone" + ZoneID + "/";
            ToolBox.DirExNorCreate(zoneFolder);
            string path = zoneFolder + "Unit" + UnitID + ".prefab";

            GameObject pref = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
            if (pref != null && erase == false)
            {
                return;
            }
            else
            {
                if (erase)
                {
                    AssetDatabase.DeleteAsset(path);
                }
            }


#if UNITY_EDITOR


            GameObject shapeGO = null;
            GameObject weaponGO;
            GameObject shieldGO;

            if (zudShape != null)
            {
                shapeGO = zudShape.BuildGameObject();
                AssetDatabase.AddObjectToAsset(zudShape.mesh, path);
                AssetDatabase.AddObjectToAsset(zudShape.texture, path);
                AssetDatabase.AddObjectToAsset(zudShape.material, path);
            }
            if (zudWeapon != null)
            {
                weaponGO = zudWeapon.BuildGameObject();
                if (shapeGO)
                {
                    weaponGO.transform.parent = shapeGO.transform;
                }

                AssetDatabase.AddObjectToAsset(zudWeapon.mesh, path);
                AssetDatabase.AddObjectToAsset(zudWeapon.texture, path);
                AssetDatabase.AddObjectToAsset(zudWeapon.material, path);
            }
            if (zudShield != null)
            {
                shieldGO = zudShield.BuildGameObject();
                if (shapeGO)
                {
                    shieldGO.transform.parent = shapeGO.transform;
                }

                AssetDatabase.AddObjectToAsset(zudShield.mesh, path);
                AssetDatabase.AddObjectToAsset(zudShield.texture, path);
                AssetDatabase.AddObjectToAsset(zudShield.material, path);
            }
            AnimatorController ac;
            if ((zudComSeq != null || zudBatSeq != null) && shapeGO != null)
            {
                Avatar ava = AvatarBuilder.BuildGenericAvatar(shapeGO, "");
                //Avatar ava = AvatarBuilder.BuildHumanAvatar(shapeGO, new HumanDescription());
                ava.name = FileName + "_Ava";
                //ava.humanDescription.

                AssetDatabase.AddObjectToAsset(ava, path);
                Animator animator = shapeGO.GetComponent<Animator>();
                if (!animator)
                {
                    animator = shapeGO.AddComponent<Animator>();
                }

                ac = AnimatorController.Instantiate(AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Resources/Prefabs/DefaultAC.controller") as AnimatorController);
                ac.name = FileName + "_AC";
                animator.runtimeAnimatorController = ac;
                animator.avatar = ava;

                AssetDatabase.AddObjectToAsset(ac, path);
                int i = 0;
                if (zudComSeq != null && shapeGO != null)
                {
                    zudComSeq.FirstPoseModel(shapeGO);
                    AnimationClip[] clips = zudComSeq.BuildAnimationClips(shapeGO);
                    ac.AddLayer(FileName + "_COM");
                    i++;
                    foreach (AnimationClip clip in clips)
                    {
                        if (clip != null)
                        {
                            AssetDatabase.AddObjectToAsset(clip, path);
                            AnimatorState state = ac.layers[i].stateMachine.AddState(clip.name);
                            state.motion = clip;
                        }
                    }
                }
                if (zudBatSeq != null && shapeGO != null)
                {
                    if (zudComSeq == null)
                    {
                        zudBatSeq.FirstPoseModel(shapeGO);
                    }
                    
                    AnimationClip[] clips = zudBatSeq.BuildAnimationClips(shapeGO);
                    ac.AddLayer(FileName + "_BAT");
                    i++;
                    foreach (AnimationClip clip in clips)
                    {
                        if (clip != null)
                        {
                            AssetDatabase.AddObjectToAsset(clip, path);
                            AnimatorState state = ac.layers[i].stateMachine.AddState(clip.name);
                            state.motion = clip;
                        }
                    }
                    
                }
            }

            //GameObject prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(shapeGO, path, InteractionMode.AutomatedAction);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(shapeGO, path);
            AssetDatabase.SaveAssets();
            GameObject.DestroyImmediate(shapeGO);
#endif
        }
    }
}
*/