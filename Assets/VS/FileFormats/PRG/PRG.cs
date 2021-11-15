using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VS.Utils;

namespace VS.FileFormats.PRG
{
    // Program file
    // MAINMENU.PRG = Button Triangle Menu
    // MENU0.PRG = Magic & Teleport
    // MENU1.PRG = Break Arts
    // MENU2.PRG = Chain & Defense
    // MENU3.PRG = Items Menu
    // MENU4.PRG = Status
    // MENU5.PRG = Map Menu
    // MENU6.PRG = doesn't exist
    // MENU7.PRG = Datas Menu
    // MENU8.PRG = Options Menu
    // MENU9.PRG = Score Menu
    // MENUA.PRG = Empty, but maybe an hook for MENUE.PRG
    // MENUB.PRG = Items loot
    // MENUC.PRG = Workshop menu
    // MENUD.PRG = Container Sub Menu
    // MENUE.PRG = Manual (i think it use SMALL/HELP**.HF0, etc...)
    // MENUF.PRG = ?

    public class PRG : ScriptableObject
    {
        private string Filename;

        public void ParseFromFile(string filepath)
        {
            FileParser fp = new FileParser();
            fp.Read(filepath);

            // ***.PRG
            if (fp.Ext == "PRG")
            {
                Filename = fp.FileName;
                ParseFromBuffer(fp.buffer, fp.FileSize);
            }

            fp.Close();
        }

        public void ParseFromBuffer(BinaryReader buffer, long limit)
        {
            switch (Filename)
            {
                case "TITLE":
                    ToolBox.ColorScaleHexa(buffer, Filename, 64);
                    /*
                    ToolBox.ColorScaleHexa(buffer, Filename, 64);
                    ToolBox.ColorScaleHexa(buffer, Filename, 128);
                    ToolBox.ColorScaleHexa(buffer, Filename, 192);
                    ToolBox.ColorScaleHexa(buffer, Filename, 256);
                    ToolBox.ColorScaleHexa(buffer, Filename, 320);
                    */

                    //ToolBox.GreyScaleHexa(buffer, Filename, 64);
                    ToolBox.GreyScaleHexa(buffer, Filename, 128);
                    //ToolBox.GreyScaleHexa(buffer, Filename, 192);
                    //ToolBox.GreyScaleHexa(buffer, Filename, 256);
                    //ToolBox.GreyScaleHexa(buffer, Filename, 320);


                    buffer.BaseStream.Position = 0xA70C;
                    TIM.TIM tex = ScriptableObject.CreateInstance<TIM.TIM>();
                    tex.type = TIM.TIM.TIMType.TIM;
                    tex.width = 128;
                    tex.height = 54;
                    tex.numColors = 16;
                    tex.numPalettes = 1;
                    tex.SetPalettesColors(buffer);
                    tex.SetCluts(buffer, true);
                    byte[] bytes = tex.GetTexture(0, true).EncodeToPNG();
                    ToolBox.DirExNorCreate(Application.dataPath + "/../Assets/Resources/Textures/Ex/");
                    File.WriteAllBytes(Application.dataPath + "/../Assets/Resources/Textures/Ex/Produced.png", bytes);

                    tex.type = TIM.TIM.TIMType.TIM;
                    tex.width = 64;
                    tex.height = 13;
                    tex.numColors = 16;
                    tex.numPalettes = 1;
                    tex.SetCluts(buffer, true);
                    bytes = tex.GetTexture(0, true).EncodeToPNG();
                    ToolBox.DirExNorCreate(Application.dataPath + "/../Assets/Resources/Textures/Ex/");
                    File.WriteAllBytes(Application.dataPath + "/../Assets/Resources/Textures/Ex/Squaresoft.png", bytes);

                    // palettes at 0x18A74, 16x16 colors, maybe for Square logo

                    buffer.BaseStream.Position = 0x46E48 + 0x20;
                    tex.type = TIM.TIM.TIMType.TIM;
                    tex.width = 128;
                    tex.height = 220;
                    tex.numColors = 24;
                    tex.numPalettes = 1;
                    tex.SetCluts(buffer, true);
                    bytes = tex.GetTexture(0, true).EncodeToPNG();
                    ToolBox.DirExNorCreate(Application.dataPath + "/../Assets/Resources/Textures/Ex/");
                    File.WriteAllBytes(Application.dataPath + "/../Assets/Resources/Textures/Ex/Font.png", bytes);


                    // 16x16 Palettes at 0x4DC68
                    // 16x16 Palettes at 0x54C68

                    buffer.BaseStream.Position = 0x59B68;
                    tex = ScriptableObject.CreateInstance<TIM.TIM>();
                    tex.type = TIM.TIM.TIMType.TIM;
                    tex.width = 128;
                    tex.height = 480;
                    tex.numColors = 16;
                    tex.numPalettes = 16;
                    tex.SetPalettesColors(buffer);
                    tex.SetCluts(buffer, true);
                    bytes = tex.BuildTexture(true).EncodeToPNG();
                    ToolBox.DirExNorCreate(Application.dataPath + "/../Assets/Resources/Textures/Ex/");
                    File.WriteAllBytes(Application.dataPath + "/../Assets/Resources/Textures/Ex/Icons.png", bytes);

                    tex = new TIM.TIM();
                    tex = ScriptableObject.CreateInstance<TIM.TIM>();
                    tex.type = TIM.TIM.TIMType.TIM;
                    tex.width = 192;
                    tex.height = 240;
                    tex.numColors = 256;
                    tex.numPalettes = 1;
                    tex.SetPalettesColors(buffer);
                    tex.SetCluts(buffer);
                    bytes = tex.GetTexture().EncodeToPNG();
                    ToolBox.DirExNorCreate(Application.dataPath + "/../Assets/Resources/Textures/Ex/");
                    File.WriteAllBytes(Application.dataPath + "/../Assets/Resources/Textures/Ex/Background.png", bytes);

                    break;
                case "ENDING":
                    buffer.BaseStream.Position = 0x76F0;
                    tex = ScriptableObject.CreateInstance<TIM.TIM>();
                    tex.type = TIM.TIM.TIMType.TIM;
                    tex.width = 128;
                    tex.height = 224;
                    tex.numColors = 22;
                    tex.numPalettes = 1;
                    tex.SetPalettesColors(buffer);
                    tex.SetCluts(buffer, true);
                    bytes = tex.GetTexture(0, true).EncodeToPNG();
                    ToolBox.DirExNorCreate(Application.dataPath + "/../Assets/Resources/Textures/Ex/");
                    File.WriteAllBytes(Application.dataPath + "/../Assets/Resources/Textures/Ex/Ending_Font.png", bytes);
                    for (int i = 0; i < 5; i++)
                    {
                        tex = ScriptableObject.CreateInstance<TIM.TIM>();
                        tex.type = TIM.TIM.TIMType.TIM;
                        tex.width = 320;
                        tex.height = 224;
                        tex.numColors = 256;
                        tex.numPalettes = 1;
                        buffer.ReadBytes(20);
                        tex.SetPalettesColors(buffer);
                        buffer.ReadBytes(12);
                        tex.SetCluts(buffer);
                        bytes = tex.GetTexture(0).EncodeToPNG();
                        ToolBox.DirExNorCreate(Application.dataPath + "/../Assets/Resources/Textures/Ex/");
                        File.WriteAllBytes(Application.dataPath + "/../Assets/Resources/Textures/Ex/Ending_pic"+i+".png", bytes);
                        // must be substracted with the following background to get the expected result
                    }
                    tex = ScriptableObject.CreateInstance<TIM.TIM>();
                    tex.type = TIM.TIM.TIMType.TIM;
                    tex.width = 160;
                    tex.height = 224;
                    tex.numColors = 16;
                    tex.numPalettes = 1;
                    buffer.ReadBytes(20);
                    tex.SetPalettesColors(buffer);
                    buffer.ReadBytes(12);
                    tex.SetCluts(buffer, true);
                    bytes = tex.GetTexture(0, true).EncodeToPNG();
                    ToolBox.DirExNorCreate(Application.dataPath + "/../Assets/Resources/Textures/Ex/");
                    File.WriteAllBytes(Application.dataPath + "/../Assets/Resources/Textures/Ex/Ending_pic_bg.png", bytes);

                    buffer.BaseStream.Position += 6096 + 36;
                    tex = ScriptableObject.CreateInstance<TIM.TIM>();
                    tex.type = TIM.TIM.TIMType.TIM;
                    tex.width = 80;
                    tex.height = 80;
                    tex.numColors = 16;
                    tex.numPalettes = 1;

                    tex.SetPalettesColors(buffer);
                    buffer.ReadBytes(12);
                    tex.SetCluts(buffer, true);
                    bytes = tex.GetTexture(0, true).EncodeToPNG();
                    ToolBox.DirExNorCreate(Application.dataPath + "/../Assets/Resources/Textures/Ex/");
                    File.WriteAllBytes(Application.dataPath + "/../Assets/Resources/Textures/Ex/kanjis.png", bytes);
                    break;
                default:
                    ToolBox.ColorScaleHexa(buffer, Filename, 64);
                    ToolBox.ColorScaleHexa(buffer, Filename, 128);
                    ToolBox.ColorScaleHexa(buffer, Filename, 192);
                    ToolBox.ColorScaleHexa(buffer, Filename, 256);
                    ToolBox.ColorScaleHexa(buffer, Filename, 320);
                    ToolBox.GreyScaleHexa(buffer, Filename, 64);
                    ToolBox.GreyScaleHexa(buffer, Filename, 128);
                    ToolBox.GreyScaleHexa(buffer, Filename, 192);
                    ToolBox.GreyScaleHexa(buffer, Filename, 256);
                    ToolBox.GreyScaleHexa(buffer, Filename, 320);
                    break;
            }
        }
    }
}