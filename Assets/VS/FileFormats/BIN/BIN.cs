using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VS.Utils;
using VS.Core;

namespace VS.FileFormats.BIN
{
    // Program file
    public class BIN : ScriptableObject
    {
        private string Filename;

        public void ParseFromFile(string filepath)
        {
            FileParser fp = new FileParser();
            fp.Read(filepath);

            // ***.BIN
            if (fp.Ext == "BIN")
            {
                Filename = fp.FileName;
                ParseFromBuffer(fp.buffer, fp.FileSize);
            }

            fp.Close();
        }

        public void ParseFromBuffer(BinaryReader buffer, long limit)
        {
            // MENU/MENU12.BIN
            // contains strings for workshop crafting

            switch (Filename) {
                case "SPMCIMG":
                    buffer.BaseStream.Position = 0;
                    TIM.TIM tex = ScriptableObject.CreateInstance<TIM.TIM>();
                    tex.type = TIM.TIM.TIMType.TIM;
                    tex.width = 448;
                    tex.height = 256;
                    tex.numColors = 256;
                    tex.numPalettes = 2;
                    tex.SetCluts(buffer);

                    FileParser fp2 = new FileParser();
                    VSPConfig conf = Memory.LoadConfig();
                    string prgPath = string.Concat(conf.VSPath, "MENU/", "MCDATA.BIN");
                    fp2.Read(prgPath);
                    fp2.buffer.BaseStream.Position = 0;
                    tex.SetPalettesColors(fp2.buffer);
                    fp2.Close();

                    byte[] bytes = tex.GetTexture().EncodeToPNG();
                    ToolBox.DirExNorCreate(Application.dataPath + "/../Assets/Resources/Textures/Ex/");
                    File.WriteAllBytes(Application.dataPath + "/../Assets/Resources/Textures/Ex/SPMCIMG.png", bytes);
                    bytes = tex.GetTexture(1).EncodeToPNG();
                    ToolBox.DirExNorCreate(Application.dataPath + "/../Assets/Resources/Textures/Ex/");
                    File.WriteAllBytes(Application.dataPath + "/../Assets/Resources/Textures/Ex/SPMCIMG_2.png", bytes);
                    break;
                case "MAPBG":
                    buffer.BaseStream.Position = 0;
                    tex = ScriptableObject.CreateInstance<TIM.TIM>();
                    tex.type = TIM.TIM.TIMType.TIM;
                    tex.width = 320;
                    tex.height = 240;
                    tex.numColors = 256;
                    tex.numPalettes = 1;
                    tex.SetPalettesColors(buffer);
                    tex.SetCluts(buffer);
                    bytes = tex.GetTexture(0).EncodeToPNG();
                    ToolBox.DirExNorCreate(Application.dataPath + "/../Assets/Resources/Textures/Ex/");
                    File.WriteAllBytes(Application.dataPath + "/../Assets/Resources/Textures/Ex/MAPBG.png", bytes);
                    break;
                default:
                    ToolBox.ColorScaleHexa(buffer, Filename, 48);
                    ToolBox.ColorScaleHexa(buffer, Filename, 64);
                    ToolBox.GreyScaleHexa(buffer, Filename, 48);
                    ToolBox.GreyScaleHexa(buffer, Filename, 64);
                    ToolBox.GreyScaleHexa(buffer, Filename, 96);
                    ToolBox.GreyScaleHexa(buffer, Filename, 128);
                    ToolBox.GreyScaleHexa(buffer, Filename, 192);
                    ToolBox.GreyScaleHexa(buffer, Filename, 224);
                    ToolBox.GreyScaleHexa(buffer, Filename, 256);
                    ToolBox.GreyScaleHexa(buffer, Filename, 320);
                    break;
            }
        }
    }
}