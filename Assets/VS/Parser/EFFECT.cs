using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VS.Core;
using VS.Parser.Effect;
using VS.Utils;

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
                }
                else
                {
                    // no FBC = no FBT
                }
            }
        }
    }
}

