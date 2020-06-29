using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VS.Core;
using VS.Parser.Effect;
using VS.Utils;
using UnityEditor;

namespace VS.Parser
{
    public class EFFECT
    {
        public string name = "";

        P baseFx;
        FBC colFx;
        List<FBT> lTexFx;

        /// <param name="path">Target the EFFECT/E0*.P</param>
        public EFFECT(string path)
        {
            // it seems to be linked with EFFECT/PLG*.BIN where assembly script is (controls what the effect does)
            VSPConfig cf = Memory.LoadConfig();

            baseFx = new P(path);
            lTexFx = new List<FBT>();

            string[] hash = path.Split("/"[0]);
            name = hash[hash.Length - 1].Split("."[0])[0];

            if (name == "E000")
            {
                // Special case, all other fx starts at 1
                colFx = new FBC(string.Concat(cf.VSPath, "EFFECT/", "E000_0.FBC"));
                FBT texFx = new FBT(string.Concat(cf.VSPath, "EFFECT/", "E000_0.FBT"), colFx.GetPallets());
                lTexFx.Add(texFx);
            }
            else
            {
                if (File.Exists(string.Concat(cf.VSPath, "EFFECT/", name, "_1.FBC")))
                {
                    // one effect can have up to 7 FBT and somtimes there is no FBC and FBT, maybe empty fx...
                    colFx = new FBC(string.Concat(cf.VSPath, "EFFECT/", name, "_1.FBC"));
                    string[] files = Directory.GetFiles(string.Concat(cf.VSPath, "EFFECT/"), string.Concat(name, "_", "*.FBT"));

                    Texture2D[] textures = new Texture2D[files.Length];
                    int i = 0;
                    for (i = 0; i < files.Length; i++)
                    {
                        string file = files[i];
                        FBT texFx = new FBT(file, colFx.GetPallets());
                        lTexFx.Add(texFx);
                        textures[i] = texFx.texture;
                    }

                    Texture2D pack = new Texture2D(textures[0].width * i, textures[0].height, TextureFormat.ARGB32, false);
                    pack.PackTextures(textures, 0);
                    pack.filterMode = FilterMode.Trilinear;
                    pack.anisoLevel = 4;
                    pack.wrapMode = TextureWrapMode.Repeat;

                    byte[] bytes = pack.EncodeToPNG();
                    ToolBox.DirExNorCreate(Application.dataPath + "/Resources/Textures/Effects/");
                    File.WriteAllBytes(Application.dataPath + "/Resources/Textures/Effects/" + name + ".png", bytes);

                    GameObject fx = new GameObject("Effect_" + name);
                    SpriteRenderer sr = fx.AddComponent<SpriteRenderer>();
                    SpriteAnimator animator = fx.AddComponent<SpriteAnimator>();
                    sr.drawMode = SpriteDrawMode.Sliced;
                    sr.spriteSortPoint = SpriteSortPoint.Pivot;

                    List<Sprite> sprites = new List<Sprite>();
                    List<Rect> spritesRect = new List<Rect>();

                    for (i = 0; i < baseFx.frames.Count; i++)
                    {
                        EffectFrame frame = baseFx.frames[i];
                        int xdec = (frame.texid - 1) * 128;
                        Rect texRect = new Rect((int)frame.texRect.x + xdec, 256 - (int)frame.texRect.y - (int)frame.texRect.height, (int)frame.texRect.width, (int)frame.texRect.height);
                        Sprite frameSprite;
                        if (spritesRect.Contains(texRect))
                        {
                            // we already get this sprite
                            frameSprite = sprites[spritesRect.IndexOf(texRect)];
                        } else {
                            frameSprite = Sprite.Create(pack, texRect, new Vector2(-frame.destRect.xMin, -frame.destRect.yMin) / 100);
                            frameSprite.name =  string.Concat(name, "Sp", sprites.Count);
                            if (i == 0) sr.sprite = frameSprite;
                            sprites.Add(frameSprite);
                            spritesRect.Add(texRect);
                        }
                        baseFx.frames[i].sprite = frameSprite;
                    }

                    animator.frames = baseFx.frames;
                    fx.transform.localScale = Vector3.one/100;

                    ToolBox.DirExNorCreate("Assets/Resources/Prefabs/Effects/");
                    GameObject prefab = PrefabUtility.SaveAsPrefabAsset(fx, "Assets/Resources/Prefabs/Effects/Effect_" + name+".prefab");
                    AssetDatabase.AddObjectToAsset(pack, "Assets/Resources/Prefabs/Effects/Effect_" + name + ".prefab");
                    for (i = 0; i < sprites.Count; i++)
                    {
                        AssetDatabase.AddObjectToAsset(sprites[i], "Assets/Resources/Prefabs/Effects/Effect_" + name + ".prefab");
                    }
                    foreach (Transform child in fx.transform)
                    {
                        GameObject.DestroyImmediate(child.gameObject);
                    }
                    prefab = PrefabUtility.SaveAsPrefabAsset(fx, "Assets/Resources/Prefabs/Effects/Effect_" + name + ".prefab");

                    GameObject.DestroyImmediate(fx);
                    AssetDatabase.SaveAssets();
                }
                else
                {
                    // no FBC = no FBT
                }
            }
        }
    }
}

