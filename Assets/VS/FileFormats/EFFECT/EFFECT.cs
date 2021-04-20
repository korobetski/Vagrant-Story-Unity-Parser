using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Sprites;
using UnityEngine;
using VS.Core;
using VS.Utils;

namespace VS.FileFormats.EFFECT
{
    public class EFFECT:ScriptableObject
    {
        public string Filename;

        public P p;
        public FBC fbc;
        public FBT[] fbts;
        public PLG plg;
        public Texture2D atlas;

        public EFFECT()
        {

        }

        /// <param name="filepath">Target the EFFECT/E0*.P</param>
        public void ParseFromFile(string filepath)
        {

            FileParser fp = new FileParser();
            fp.Read(filepath);

            // in EFFECT/E***.P
            if (fp.Ext == "P")
            {
                VSPConfig cf = Memory.LoadConfig();

                Filename = fp.FileName;
                p = ScriptableObject.CreateInstance<P>();
                p.name = Filename + ".P";
                p.ParseFromFile(filepath);
                if (Filename == "E000")
                {
                    // Special case, all other fx starts at 1
                    // this FBT seems to be a 8bits TIM
                    fbc = ScriptableObject.CreateInstance<FBC>();
                    fbc.name = "E000_0.FBC";
                    fbc.ParseFromFile(string.Concat(cf.VSPath, "EFFECT/", "E000_0.FBC"));

                    fbts = new FBT[1];
                    fbts[0] = ScriptableObject.CreateInstance<FBT>();
                    fbts[0].name = "E000_0.FBT";
                    fbts[0].tim8 = true;
                    fbts[0].ParseFromFile(string.Concat(cf.VSPath, "EFFECT/", "E000_0.FBT"));
                    //EFFPURGE.BIN
                    plg = ScriptableObject.CreateInstance<PLG>();
                    plg.name = "EFFPURGE.BIN";
                    plg.ParseFromFile(string.Concat(cf.VSPath, "EFFECT/", "EFFPURGE.BIN"));
                }
                else
                {
                    if (File.Exists(string.Concat(cf.VSPath, "EFFECT/", Filename, "_1.FBC")))
                    {
                        fbc = ScriptableObject.CreateInstance<FBC>();
                        fbc.name = Filename+"_1.FBC";
                        fbc.ParseFromFile(string.Concat(cf.VSPath, "EFFECT/", Filename, "_1.FBC"));

                        string[] fbtsPath = Directory.GetFiles(string.Concat(cf.VSPath, "EFFECT/"), string.Concat(Filename, "_", "*.FBT"));
                        fbts = new FBT[fbtsPath.Length];
                        for (uint i = 0; i < fbtsPath.Length; i++)
                        {
                            string fbtpath = fbtsPath[i];
                            fbts[i] = ScriptableObject.CreateInstance<FBT>();
                            fbts[i].name = string.Concat(Filename, "_", i,".FBT");
                            if (fbc.only16Colors) fbts[i].tim8 = true;
                            fbts[i].ParseFromFile(fbtpath);
                        }
                        // PLG280.BIN
                        string plgpath = string.Concat(cf.VSPath, "EFFECT/", "PLG", Filename.Replace("E", ""), ".BIN");
                        plg = ScriptableObject.CreateInstance<PLG>();
                        plg.name = Filename + ".PLG";
                        plg.ParseFromFile(plgpath);
                    }
                    else
                    {
                        // no FBC = no FBT
                    }
                }
            }

            fp.Close();

            BuildSprites();
        }

        private void BuildSprites()
        {
            if (fbc != null && fbts != null && fbts.Length > 0)
            {
                // we build texture parts
                foreach (FBT fbt in fbts) fbt.BuildTextures(fbc);
                int width = fbts[0].width;
                int height = fbts[0].height;

                List<Color> colors = new List<Color>();
                // we assemble textures from left to right and from bottom to top
                for (uint i = 0; i< fbc.palettes.Length; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        foreach (FBT fbt in fbts)
                        {
                            colors.AddRange(fbt.textures[i].GetPixels(0, j, fbt.width, 1));
                        }
                    }
                }

                atlas = new Texture2D(width * fbts.Length, height * fbc.palettes.Length, TextureFormat.RGBA32, false);
                atlas.filterMode = FilterMode.Point;
                atlas.SetPixels(colors.ToArray());
                if (p.sprites.Length > 0)
                {
                    foreach(PSprite psprite in p.sprites)
                    {
                        if (fbc.only16Colors) if (psprite.paletteId == 3) psprite.paletteId = 0;

                        int tx = (int)psprite.texRect.x;
                        int ty = height - (int)psprite.texRect.height - (int)psprite.texRect.y;
                        int tw = (int)psprite.texRect.width;
                        int th = (int)psprite.texRect.height;

                        tx += psprite.texid * 128;
                        if (fbc.palettes.Length > 1) ty += (psprite.paletteId * height);
                        
                        if (tx < 0) tx = 0;
                        if (ty < 0) ty = 0;
                        if (tx > (atlas.width - psprite.texRect.width)) tx = atlas.width - (int)psprite.texRect.width;
                        if (ty > (atlas.height - psprite.texRect.height)) ty = atlas.height - (int)psprite.texRect.height;

                        Rect packedTexRect = new Rect(tx, ty, tw, th);
                        psprite.sprite = Sprite.Create(atlas, packedTexRect, psprite.destRect.center, 1, 1, SpriteMeshType.FullRect);
                        psprite.sprite.name = "sp_"+psprite.id;

                    }
                }
                atlas.name = Filename + ".ATLAS";
                atlas.alphaIsTransparency = true;
                //atlas.Compress(true);
                atlas.Apply();
            }
        }

        internal void DrawPNG()
        {
            byte[] bytes = atlas.EncodeToPNG();
            ToolBox.DirExNorCreate("Assets/Resources/Textures/Effects/");
            File.WriteAllBytes("Assets/Resources/Textures/Effects/" + Filename + ".png", bytes);
        }
    }


    [CustomEditor(typeof(EFFECT))]
    public class EFFECTEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var effect = target as EFFECT;
            DrawDefaultInspector();
            if (GUILayout.Button("Draw PNG"))
            {
                effect.DrawPNG();
            }
        }
    }
}

