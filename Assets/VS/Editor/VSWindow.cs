using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using VS.Core;
using VS.FileFormats.ARM;
using VS.FileFormats.BIN;
using VS.FileFormats.MPD;
using VS.FileFormats.ZUD;
using VS.FileFormats.WEP;
using VS.FileFormats.SHP;
using VS.FileFormats.EVT;
using VS.FileFormats.ZND;
using VS.FileFormats.SYD;
using VS.FileFormats.TIM;
using VS.FileFormats.AKAO;
using VS.FileFormats.EFFECT;
using VS.FileFormats.PRG;
using VS.FileFormats.ITEM;
using VS.Utils;
using VS.FileFormats.HELP;
using VS.FileFormats.SEQ;

//https://unity3d.college/2017/05/22/unity-attributes/

public class VSWindow : EditorWindow
{
    private string VSPath = "";
    private string FilePath = "";
    private VSPConfig conf;

    bool midTrigger = true;
    bool sf2Trigger = true;
    bool dlsTrigger = false;
    bool wavTrigger = false;

    [MenuItem("Window/Vagrant Story")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        EditorWindow.GetWindow(typeof(VSWindow));
    }

    void OnGUI()
    {
        if (conf == null)
        {
            conf = Memory.LoadConfig();
            if (conf == null)
            {
                Memory.SaveConfig(new VSPConfig());
                conf = Memory.LoadConfig();
            }
            if (conf.VSPath != null)
            {
                VSPath = conf.VSPath;
                if (conf.VS_Version == "")
                {
                    conf.VS_Version = ToolBox.checkVSROM(VSPath);
                    Memory.SaveConfig(conf);
                }
            }
        }

        GUILayout.Label("Vagrant Story Path", EditorStyles.boldLabel);
        GUILayoutOption[] options = { GUILayout.Width(300), GUILayout.MaxWidth(400) };
        VSPath = EditorGUILayout.TextField("Vagrant Story CD path :", VSPath, options);

        GUILayoutOption[] options2 = { GUILayout.MaxWidth(30) };
        bool VSPathTrigger = GUILayout.Button(new GUIContent("..."), options2);
        if (VSPathTrigger)
        {
            string path = EditorUtility.OpenFolderPanel("Path to Vagrant Story CD", "", "");
            VSPath = path + "/";
        }

        GUILayoutOption[] options3 = { GUILayout.Width(200), GUILayout.MaxWidth(400) };
        bool VSSaveTrigger = GUILayout.Button(new GUIContent("Save Path"), options3);
        if (VSSaveTrigger)
        {
            conf.VSPath = VSPath;
            conf.VS_Version = ToolBox.checkVSROM(VSPath);
            Memory.SaveConfig(conf);
        }

        GUILayout.Label("Vagrant Story Version : " + conf.VS_Version);
        GUILayout.Space(10f);

        GUILayout.Label("| One File import", EditorStyles.boldLabel);
        FilePath = EditorGUILayout.TextField("File path (VS Path relativ) :", FilePath, options);
        bool filePathTrigger = GUILayout.Button(new GUIContent("..."), options2);
        if (filePathTrigger)
        {
            string path = EditorUtility.OpenFilePanel("Path to File", VSPath, "");
            FilePath = path.Replace(VSPath, "");
        }
        bool fileLoadTrigger = GUILayout.Button(new GUIContent("Load"), options3);
        if (fileLoadTrigger && VSPath != "" && FilePath != "")
        {
            string[] hash = FilePath.Split("/"[0]);
            string[] h2 = hash[hash.Length - 1].Split("."[0]);
            string folder = hash[0];
            string fileName = h2[0];
            string ext = h2[1];


            switch (folder)
            {
                case "BATTLE":
                    // BATTLE.PRG
                    // BOG.DAT
                    // INITBTL.PRG
                    // SYSTEM.DAT
                    switch (ext)
                    {
                        case "PRG":
                            ParsePRG(VSPath + FilePath);
                            break;
                    }
                    break;
                case "BG":
                    // 001OP01A.FAR & TIM
                    // 002OP01A.FAR & TIM
                    // 007OP01A.FAR & TIM
                    // 008OP01A.FAR & TIM
                    break;
                case "EFFECT":
                    // EFFPURGE.BIN maybe the PLG for E000.P
                    // E*.P
                    // E*.FBC
                    // E*.FBT
                    // PLG*.BIN lot of empty
                    switch (ext)
                    {
                        case "P":
                            ParseEFFECT(VSPath + FilePath);
                            break;
                    }
                    break;
                case "ENDING":
                    // ENDING.PRG
                    // ENDING.XA
                    // ILLUST06.BIN -> ILLUST16.BIN
                    // NULL.DAT
                    switch (ext)
                    {
                        case "BIN":
                            TIM parser = new TIM();
                            //parser.ParseIllust(VSPath + FilePath);
                            break;
                    }
                    break;
                case "EVENT":
                    // ****.EVT
                    ParseEVT(VSPath + FilePath, true);
                    break;
                case "GIM":
                    // ****.GIM
                    ParseGIM(VSPath + FilePath, true);
                    break;
                case "MAP":
                    // MAP***.MPD
                    // Z***U**.ZUD
                    // ZONE***.ZND
                    switch (ext)
                    {
                        case "MPD":
                            ParseMPD(VSPath + FilePath, true);
                            break;
                        case "ZUD":
                            ParseZUD(VSPath + FilePath, fileName, true);
                            break;
                        case "ZND":
                            ParseZND(VSPath + FilePath, true);
                            break;
                    }
                    break;
                case "MENU":
                    // *.SYD
                    // *.BIN
                    // *.PRG
                    switch (ext)
                    {
                        case "BIN":
                            //BIN bparser = new BIN();
                            //bparser.ParseFromFile(VSPath + FilePath);
                            break;
                        case "PRG":
                            ParsePRG(VSPath + FilePath);
                            break;
                    }
                    break;
                case "MOV":
                    // TITLE.STR intro video
                    break;
                case "MUSIC":
                    //ParseAKAO(VSPath + FilePath, AKAO.Type.SEQUENCE, true);
                    break;
                case "OBJ":
                    // **.SHP
                    // **.ESQ one by SHP
                    // **.ETM one by SHP
                    // **.SEQ
                    // **.WEP
                    switch (ext)
                    {
                        case "SHP":
                            ParseSHP(VSPath + FilePath, fileName, true, true);
                            break;
                        case "WEP":
                            ParseWEP(VSPath + FilePath, true);
                            break;
                    }
                    break;
                case "SE":
                    // EFFECT00.DAT
                    // EFFECT01.DAT
                    // SEP000**.DAT
                    break;
                case "SMALL":
                    // **.ARM
                    // MON.BIN
                    // **.DIS
                    // HELP**.HF0
                    // HELP**.HF1

                    switch (ext)
                    {
                        case "ARM":
                            ParseARM(VSPath + FilePath, true);
                            break;
                        case "BIN":
                            ParseBIN(VSPath + FilePath, fileName);
                            break;
                    }
                    break;
                case "SOUND":
                    // WAVE0***.DAT
                    ParseAKAO(VSPath + FilePath, VS.Enums.AKAO.Type.SAMPLE_COLLECTION, true);
                    break;
                case "TITLE":
                    // TITLE.PRG
                    break;
            }
        }



        GUILayout.Label("| Batch imports", EditorStyles.boldLabel);
        GUILayout.BeginVertical(options);
        GUILayout.Label("3D Model Formats : ");
        bool LoadARMTrigger = GUILayout.Button(new GUIContent("Load MiniMaps.ARM"));
        if (LoadARMTrigger && VSPath != "")
        {

            string[] files = Directory.GetFiles(VSPath + "SMALL/", "*.ARM");
            float fileToParse = files.Length;
            float fileParsed = 0;
            foreach (string file in files)
            {
                string[] h = file.Split("/"[0]);
                string filename = h[h.Length - 1];
                EditorUtility.DisplayProgressBar("VS Parsing", "Parsing : " + filename + ", " + fileParsed + " files parsed.", (fileParsed / fileToParse));
                ParseARM(file, false);
                fileParsed++;
            }
            EditorUtility.ClearProgressBar();
        }

        bool LoadWEPTrigger = GUILayout.Button(new GUIContent("Load Weapons.WEP"));
        if (LoadWEPTrigger && VSPath != "")
        {
            string[] files = Directory.GetFiles(VSPath + "OBJ/", "*.WEP");
            float fileToParse = files.Length;
            float fileParsed = 0f;

            foreach (string file in files)
            {
                string[] h = file.Split("/"[0]);
                string filename = h[h.Length - 1];
                EditorUtility.DisplayProgressBar("VS Parsing", "Parsing : " + filename + ", " + fileParsed + " files parsed.", (fileParsed / fileToParse));
                ParseWEP(file, true);
                fileParsed++;
            }
            EditorUtility.ClearProgressBar();
        }

        bool LoadSHPTrigger = GUILayout.Button(new GUIContent("Load 3D Models.SHP"));
        if (LoadSHPTrigger && VSPath != "")
        {
            string[] files = Directory.GetFiles(VSPath + "OBJ/", "*.SHP");
            float fileToParse = files.Length;
            float fileParsed = 0f;

            foreach (string file in files)
            {
                string[] h = file.Split("/"[0]);
                string filename = h[h.Length - 1];
                EditorUtility.DisplayProgressBar("VS Parsing", "Parsing : " + filename + ", " + fileParsed + " files parsed.", (fileParsed / fileToParse));
                ParseSHP(file, filename, false);
                fileParsed++;
            }
            EditorUtility.ClearProgressBar();
        }

        bool LoadSEQTrigger = GUILayout.Button(new GUIContent("Load 3D Models Animations.SEQ"));
        if (LoadSEQTrigger && VSPath != "")
        {
            string[] files = Directory.GetFiles(VSPath + "OBJ/", "*.SEQ");
            float fileToParse = files.Length;
            float fileParsed = 0f;

            foreach (string file in files)
            {
                string[] h = file.Split("/"[0]);
                string filename = h[h.Length - 1];
                EditorUtility.DisplayProgressBar("VS Parsing", "Parsing : " + filename + ", " + fileParsed + " files parsed.", (fileParsed / fileToParse));
                ParseSEQ(file);
                fileParsed++;
            }
            EditorUtility.ClearProgressBar();
        }

        bool LoadZUDTrigger = GUILayout.Button(new GUIContent("Load Zones Units Datas.ZUD"));
        if (LoadZUDTrigger && VSPath != "")
        {
            string[] files = Directory.GetFiles(VSPath + "MAP/", "*.ZUD");
            float fileToParse = files.Length;
            float fileParsed = 0f;
            foreach (string file in files)
            {
                string[] h = file.Split("/"[0]);
                string filename = h[h.Length - 1];
                EditorUtility.DisplayProgressBar("VS Parsing", "Parsing : " + filename + ", " + fileParsed + " files parsed.", (fileParsed / fileToParse));
                ParseZUD(file, filename, false);
                fileParsed++;
            }
            EditorUtility.ClearProgressBar();
        }

        bool LoadZNDTrigger = GUILayout.Button(new GUIContent("Load Zones Datas.ZND"));
        if (LoadZNDTrigger && VSPath != "")
        {
            string[] files = Directory.GetFiles(VSPath + "MAP/", "*.ZND");
            float fileToParse = files.Length;
            float fileParsed = 0f;
            foreach (string file in files)
            {
                string[] h = file.Split("/"[0]);
                string filename = h[h.Length - 1];
                EditorUtility.DisplayProgressBar("VS Parsing", "Parsing : " + filename + ", " + fileParsed + " files parsed.", (fileParsed / fileToParse));
                ParseZND(file, false);
                fileParsed++;
            }
            EditorUtility.ClearProgressBar();
        }

        bool LoadMPDTrigger = GUILayout.Button(new GUIContent("Load Map Datas.MPD"));
        if (LoadMPDTrigger && VSPath != "")
        {

            string[] files = Directory.GetFiles(VSPath + "MAP/", "*.MPD");
            float fileToParse = files.Length;
            float fileParsed = 0f;

            foreach (string file in files)
            {
                string[] h = file.Split("/"[0]);
                string filename = h[h.Length - 1];
                EditorUtility.DisplayProgressBar("VS Parsing", "Parsing : " + filename + ", " + fileParsed + " files parsed.", (fileParsed / fileToParse));
                ParseMPD(file, false);
                fileParsed++;
            }
            EditorUtility.ClearProgressBar();
        }

        bool LoadEFFECTTrigger = GUILayout.Button(new GUIContent("Load Spell Effects .P"));
        if (LoadEFFECTTrigger && VSPath != "")
        {

            string[] files = Directory.GetFiles(VSPath + "EFFECT/", "*.P");
            float fileToParse = files.Length;

            float fileParsed = 0;
            foreach (string file in files)
            {
                string[] h = file.Split("/"[0]);
                string filename = h[h.Length - 1];
                EditorUtility.DisplayProgressBar("VS Parsing", "Parsing : " + filename + ", " + fileParsed + " files parsed.", (fileParsed / fileToParse));
                ParseEFFECT(file);
                fileParsed++;
            }


            //EFFECT effect = new EFFECT(VSPath + "EFFECT/E008.P");

            EditorUtility.ClearProgressBar();
        }
        GUILayout.EndVertical();

        GUILayout.BeginVertical(options);
        GUILayout.Label("Texture Formats : ");
        /*
        bool LoadGIMTrigger = GUILayout.Button(new GUIContent("Load GIM/*.GIM"));
        if (LoadGIMTrigger && VSPath != "")
        {

            string[] files = Directory.GetFiles(VSPath + "GIM/", "*.GIM");
            float fileToParse = files.Length;

            float fileParsed = 0;
            foreach (string file in files)
            {
                string[] h = file.Split("/"[0]);
                string filename = h[h.Length - 1];
                EditorUtility.DisplayProgressBar("VS Parsing", "Parsing : " + filename + ", " + fileParsed + " files parsed.", (fileParsed / fileToParse));
                //GIM gim = new GIM(file);
                fileParsed++;
            }


            EditorUtility.ClearProgressBar();
        }
        */
        /*
        bool LoadDISTrigger = GUILayout.Button(new GUIContent("Load SMALL/*.DIS"));
        if (LoadDISTrigger && VSPath != "")
        {
            string[] files = Directory.GetFiles(VSPath + "SMALL/", "*.DIS");
            float fileToParse = files.Length;

            float fileParsed = 0;
            foreach (string file in files)
            {
                string[] h = file.Split("/"[0]);
                string filename = h[h.Length - 1];
                EditorUtility.DisplayProgressBar("VS Parsing", "Parsing : " + filename + ", " + fileParsed + " files parsed.", (fileParsed / fileToParse));
                //DIS dis = new DIS();
                //dis.ParseFromFile(file);
                fileParsed++;
            }


            EditorUtility.ClearProgressBar();
        }
        */
        /*
        bool LoadTIMTrigger = GUILayout.Button(new GUIContent("BG/*.TIM"));
        if (LoadTIMTrigger && VSPath != "")
        {
            string[] files = Directory.GetFiles(VSPath + "BG/", "*.TIM");
            float fileToParse = files.Length;

            float fileParsed = 0;
            foreach (string file in files)
            {
                string[] h = file.Split("/"[0]);
                string filename = h[h.Length - 1];
                EditorUtility.DisplayProgressBar("VS Parsing", "Parsing : " + filename + ", " + fileParsed + " files parsed.", (fileParsed / fileToParse));
                TIM parser = new TIM();
                //parser.ParseFromFile(file);
                fileParsed++;
            }


            EditorUtility.ClearProgressBar();
        }
        */
        /*
        bool LoadILLUSTTrigger = GUILayout.Button(new GUIContent("ENDING/ILLUST*.BIN (Not Working Yet)"));
        if (LoadILLUSTTrigger && VSPath != "")
        {
            // not working yet
            string[] files = Directory.GetFiles(VSPath + "ENDING/", "*.BIN");
            float fileToParse = files.Length;

            float fileParsed = 0;
            foreach (string file in files)
            {
                string[] h = file.Split("/"[0]);
                string filename = h[h.Length - 1];
                EditorUtility.DisplayProgressBar("VS Parsing", "Parsing : " + filename + ", " + fileParsed + " files parsed.", (fileParsed / fileToParse));
                TIM parser = new TIM();
                parser.ParseIllust(file);
                fileParsed++;
            }

            EditorUtility.ClearProgressBar();
        }
        */

        GUILayout.EndVertical();
        GUILayout.BeginVertical(options);
        GUILayout.Label("Audio Formats : ");

        /*
        bool LoadAKAOTrigger = GUILayout.Button(new GUIContent("Load Akao SOUND/WAVE*.DAT"));
        if (LoadAKAOTrigger && VSPath != "")
        {
            string[] files = Directory.GetFiles(VSPath + "SOUND/", "*.DAT");
            float fileToParse = files.Length;
            float fileParsed = 0;
            foreach (string file in files)
            {
                string[] h = file.Split("/"[0]);
                string filename = h[h.Length - 1];
                EditorUtility.DisplayProgressBar("VS Parsing", "Parsing : " + filename + ", " + fileParsed + " files parsed.", (fileParsed / fileToParse));
                AKAO parser = new AKAO();
                parser.UseDebug = true;
                parser.Parse(file, AKAO.SOUND);
                fileParsed++;
            }

            EditorUtility.ClearProgressBar();
        }
        */

        midTrigger = GUILayout.Toggle(midTrigger, new GUIContent("output a MIDI file ?"));
        sf2Trigger = GUILayout.Toggle(sf2Trigger, new GUIContent("output a SF2 (soundfont) file ?"));
        dlsTrigger = GUILayout.Toggle(dlsTrigger, new GUIContent("output a DLS (soundfont) file ? (Not working well yet)"));
        wavTrigger = GUILayout.Toggle(wavTrigger, new GUIContent("output a WAV file ? ( /_!_\\ heavy files)"));

        bool LoadAKAO1Trigger = GUILayout.Button(new GUIContent("Load Akao SOUND/WAVE****.DAT"));
        if (LoadAKAO1Trigger && VSPath != "")
        {

            string[] files = Directory.GetFiles(VSPath + "SOUND/", "*.DAT");
            float fileToParse = files.Length;

            float fileParsed = 0;
            foreach (string file in files)
            {
                string[] h = file.Split("/"[0]);
                string filename = h[h.Length - 1];
                EditorUtility.DisplayProgressBar("VS Parsing", "Parsing : " + filename + ", " + fileParsed + " files parsed.", (fileParsed / fileToParse));
                ParseAKAO(file, VS.Enums.AKAO.Type.SAMPLE_COLLECTION, false);
                fileParsed++;
            }
            EditorUtility.ClearProgressBar();
        }
        bool LoadAKAO2Trigger = GUILayout.Button(new GUIContent("Load Akao MUSIC/MUSIC*.DAT"));
        if (LoadAKAO2Trigger && VSPath != "")
        {

            string[] files = Directory.GetFiles(VSPath + "MUSIC/", "*.DAT");
            float fileToParse = files.Length;

            float fileParsed = 0;
            foreach (string file in files)
            {
                string[] h = file.Split("/"[0]);
                string filename = h[h.Length - 1];
                EditorUtility.DisplayProgressBar("VS Parsing", "Parsing : " + filename + ", " + fileParsed + " files parsed.", (fileParsed / fileToParse));
                ParseAKAO(file, VS.Enums.AKAO.Type.SEQUENCE, false);
                fileParsed++;
            }
            EditorUtility.ClearProgressBar();
        }
        GUILayout.EndVertical();

        GUILayout.BeginVertical(options);
        GUILayout.Label("Data Formats : ");
        bool LoadSYDTrigger = GUILayout.Button(new GUIContent("Load Workshop *.SYD"));
        if (LoadSYDTrigger && VSPath != "")
        {
            ItemList itemStr = AssetDatabase.LoadAssetAtPath<ItemList>("Assets/Resources/Serialized/MENU/ITEMS.yaml.asset");
            if (itemStr == null)
            {
                itemStr = BuildItemStrings();
            }
            
            string[] files = Directory.GetFiles(VSPath + "MENU/", "*.SYD");
            foreach (string file in files)
            {
                SYD syd = ScriptableObject.CreateInstance<SYD>();
                syd.ParseFromFile(file);
                syd.SetNames(itemStr);

                ToolBox.SaveScriptableObject("Assets/Resources/Serialized/MENU/", syd.Filename + ".yaml.asset", syd);
            }
        }

        bool LoadMONTrigger = GUILayout.Button(new GUIContent("Load Bestiary MON.BIN"));
        if (LoadMONTrigger && VSPath != "")
        {
            MONList mon = ScriptableObject.CreateInstance<MONList>();
            mon.ParseFromFile(VSPath + "SMALL/MON.BIN");

            ToolBox.SaveScriptableObject("Assets/Resources/Serialized/SMALL/", "MON.yaml.asset", mon);
        }

        bool LoadEVENTTrigger = GUILayout.Button(new GUIContent("Load EVENT/*.EVT"));
        if (LoadEVENTTrigger && VSPath != "")
        {

            string[] files = Directory.GetFiles(VSPath + "EVENT/", "*.EVT");
            float fileToParse = files.Length;

            float fileParsed = 0;
            foreach (string file in files)
            {
                string[] h = file.Split("/"[0]);
                string filename = h[h.Length - 1];
                EditorUtility.DisplayProgressBar("VS Parsing", "Parsing : " + filename + ", " + fileParsed + " files parsed.", (fileParsed / fileToParse));

                ParseEVT(file, false);
                fileParsed++;
            }


            EditorUtility.ClearProgressBar();
        }

        bool LoadHFTrigger = GUILayout.Button(new GUIContent("Load InGame Help SMALL/*.HF"));
        if (LoadHFTrigger && VSPath != "")
        {
            string[] files = Directory.GetFiles(VSPath + "SMALL/", "*.HF0");
            float fileToParse = files.Length;

            float fileParsed = 0;
            foreach (string file in files)
            {
                string[] h = file.Split("/"[0]);
                string filename = h[h.Length - 1];
                EditorUtility.DisplayProgressBar("VS Parsing", "Parsing : " + filename + ", " + fileParsed + " files parsed.", (fileParsed / fileToParse));
                HF0 parser = new HF0();
                parser.ParseFromFile(file);
                fileParsed++;
            }


            EditorUtility.ClearProgressBar();
        }
        GUILayout.EndVertical();
    }

    private ItemList BuildItemStrings()
    {
        ItemList  itemsStr = ScriptableObject.CreateInstance<ItemList>();
        itemsStr.ParseFromFile(VSPath + "MENU/ITEMNAME.BIN");
        itemsStr.ParseFromFile(VSPath + "MENU/ITEMHELP.BIN");

        ToolBox.SaveScriptableObject("Assets/Resources/Serialized/MENU/", "ITEMS.yaml.asset", itemsStr);

        return itemsStr;
    }

    private void BuildBestiary()
    {
        MONList mon = ScriptableObject.CreateInstance<MONList>();
        mon.ParseFromFile(VSPath + "SMALL/MON.BIN");

        ToolBox.SaveScriptableObject("Assets/Resources/Serialized/SMALL/", "MON.yaml.asset", mon);
    }

    private void ParseARM(string path, bool UseDebug)
    {
        ARM arm = ScriptableObject.CreateInstance<ARM>();
        arm.ParseFromFile(path);

        ToolBox.SaveScriptableObject("Assets/Resources/Serialized/ARM/", arm.filename+".yaml.asset", arm);
    }

    private void ParseWEP(string path, bool UseDebug)
    {
        WEP wep = ScriptableObject.CreateInstance<WEP>();
        wep.ParseFromFile(path);

        ToolBox.SaveScriptableObject("Assets/Resources/Serialized/WEP/", wep.Filename + ".WEP.yaml.asset", wep, new UnityEngine.Object[]{ wep.TIM });
    }

    private void ParseSHP(string path, string filename, bool UseDebug, bool erase = false)
    {
        SHP shp = ScriptableObject.CreateInstance<SHP>();
        shp.ParseFromFile(path);

        ToolBox.SaveScriptableObject("Assets/Resources/Serialized/SHP/", shp.Filename + ".SHP.yaml.asset", shp, new UnityEngine.Object[] { shp.TIM });
    }

    private void ParseSEQ(string path)
    {
        SEQ seq = ScriptableObject.CreateInstance<SEQ>();
        seq.ParseFromFile(path);

        ToolBox.SaveScriptableObject("Assets/Resources/Serialized/SEQ/", seq.Filename + ".yaml.asset", seq);
    }

    private void ParseZUD(string path, string filename, bool UseDebug)
    {
        ZUD zud = ScriptableObject.CreateInstance<ZUD>();
        zud.ParseFromFile(path);

        ToolBox.SaveScriptableObject("Assets/Resources/Serialized/ZUD/", zud.Filename + ".ZUD.yaml.asset", zud);
        string zudPath = "Assets/Resources/Serialized/ZUD/" + zud.Filename + ".ZUD.yaml.asset";
        if (zud.zudShape != null)
        {
            zud.zudShape.name = zud.Filename + ".ZUD.SHP";
            AssetDatabase.AddObjectToAsset(zud.zudShape, zudPath);
            zud.zudShape.TIM.name = zud.Filename + ".ZUD.SHP.TIM";
            AssetDatabase.AddObjectToAsset(zud.zudShape.TIM, zudPath);
        }
        if (zud.zudWeapon != null)
        {
            zud.zudWeapon.name = zud.Filename + ".ZUD.WEP";
            AssetDatabase.AddObjectToAsset(zud.zudWeapon, zudPath);
            zud.zudWeapon.TIM.name = zud.Filename + ".ZUD.WEP.TIM";
            AssetDatabase.AddObjectToAsset(zud.zudWeapon.TIM, zudPath);
        }
        if (zud.zudShield != null)
        {
            zud.zudShield.name = zud.Filename + ".ZUD.WEP2";
            AssetDatabase.AddObjectToAsset(zud.zudShield, zudPath);
            zud.zudShield.TIM.name = zud.Filename + ".ZUD.WEP2.TIM";
            AssetDatabase.AddObjectToAsset(zud.zudShield.TIM, zudPath);
        }
        if (zud.zudComSeq != null)
        {
            zud.zudComSeq.name = zud.Filename + ".ZUD.COMSEQ";
            AssetDatabase.AddObjectToAsset(zud.zudComSeq, zudPath);
        }
        if (zud.zudBatSeq != null)
        {
            zud.zudBatSeq.name = zud.Filename + ".ZUD.BATSEQ";
            AssetDatabase.AddObjectToAsset(zud.zudBatSeq, zudPath);
        }
    }

    private void ParseZND(string path, bool UseDebug)
    {
        ZND znd = ScriptableObject.CreateInstance<ZND>();
        znd.ParseFromFile(path);

        ToolBox.SaveScriptableObject("Assets/Resources/Serialized/ZND/", znd.Filename + ".ZND.yaml.asset", znd, znd.TIMs);
    }


    private void ParseMPD(string path, bool UseDebug)
    {
        MPD mpd = ScriptableObject.CreateInstance<MPD>();
        mpd.ParseFromFile(path);
        UnityEngine.Object[] subAssets = new UnityEngine.Object[] { mpd.miniMap, mpd.scriptSection, mpd.treasureSection };

        ToolBox.SaveScriptableObject("Assets/Resources/Serialized/MPD/", mpd.Filename + ".MPD.yaml.asset", mpd, subAssets);
    }


    private void ParseEFFECT(string path)
    {
        EFFECT effect = ScriptableObject.CreateInstance<EFFECT>();
        effect.ParseFromFile(path);

        List<UnityEngine.Object> subAssets = new List<UnityEngine.Object>();
        subAssets.Add(effect.p);
        if (effect.fbc != null) subAssets.Add(effect.fbc);
        if (effect.fbts != null && effect.fbts.Length > 0)
        {
            for (uint i = 0; i < effect.fbts.Length; i++)
            {
                subAssets.Add(effect.fbts[i]);
            }
            
            subAssets.Add(effect.atlas);

            for (uint i = 0; i < effect.p.sprites.Length; i++)
            {
                subAssets.Add(effect.p.sprites[i].sprite);
            }
        }
        if (effect.plg != null) subAssets.Add(effect.plg);

        ToolBox.SaveScriptableObject("Assets/Resources/Serialized/EFFECT/", effect.Filename + ".yaml.asset", effect, subAssets.ToArray());
    }

    private void ParseEVT(string path, bool UseDebug)
    {
        EVT evt = ScriptableObject.CreateInstance<EVT>();
        evt.ParseFromFile(path);

        ToolBox.SaveScriptableObject("Assets/Resources/Serialized/EVT/", evt.Filename + ".yaml.asset", evt);
    }

    private void ParseAKAO(string path, VS.Enums.AKAO.Type type, bool UseDebug)
    {
        switch(type)
        {
            case (VS.Enums.AKAO.Type.SAMPLE_COLLECTION):
                AKAOSampleCollection collection = ScriptableObject.CreateInstance<AKAOSampleCollection>();
                collection.ParseFromFile(path);

                ToolBox.SaveScriptableObject("Assets/Resources/Serialized/AKAO/SampleCollections/", collection.Filename + ".yaml.asset", collection);
                break;
            case (VS.Enums.AKAO.Type.SEQUENCE):
                AKAOSequence sequence = ScriptableObject.CreateInstance<AKAOSequence>();
                sequence.ParseFromFile(path);

                ToolBox.SaveScriptableObject("Assets/Resources/Serialized/AKAO/Sequence/", sequence.Filename + ".yaml.asset", sequence);
                break;
        }
    }

    private void ParseGIM(string path, bool UseDebug)
    {
        //GIM parser = new GIM(path);
    }

    private void ParseBIN(string path, string fileName)
    {
        //BIN evt = new BIN();
        //evt.Parse(path);
    }


    private void ParsePRG(string v)
    {
        PRG parser = new PRG();
        parser.ParseFromFile(v);
    }
}
