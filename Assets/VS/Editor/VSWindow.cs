using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using VS.Core;
using VS.Parser;

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
                    conf.VS_Version = LBA.checkVSROM(VSPath);
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
            conf.VS_Version = LBA.checkVSROM(VSPath);
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
                            PRG parser = new PRG();
                            parser.Parse(VSPath + FilePath);
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
                            EFFECT effect = new EFFECT(VSPath + FilePath);
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
                            parser.ParseIllust(VSPath + FilePath);
                            break;
                    }
                    break;
                case "EVENT":
                    // ****.EVT
                    ParseEVT(VSPath + FilePath, true);
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
                        case "PRG":
                            PRG parser = new PRG();
                            parser.Parse(VSPath + FilePath);
                            break;
                    }
                    break;
                case "MOV":
                    break;
                case "MUSIC":
                    ParseAKAO(VSPath + FilePath, AKAO.MUSIC, true);
                    break;
                case "OBJ":
                    // **.SHP
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
                case "SOUND":
                    ParseAKAO(VSPath + FilePath, AKAO.SOUND, true);
                    break;
            }
        }



        GUILayout.Label("| Batch imports", EditorStyles.boldLabel);
        GUILayout.BeginVertical();
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
                ARM parser = new ARM();
                //parser.UseDebug = true;
                parser.Parse(file);
                parser.BuildPrefab(true);
                fileParsed++;
            }
            EditorUtility.ClearProgressBar();
        }

        bool LoadWEPTrigger = GUILayout.Button(new GUIContent("Load Weapons.WEP"));
        if (LoadWEPTrigger && VSPath != "")
        {

            BuildDatabase();
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

        bool LoadEFFECTTrigger = GUILayout.Button(new GUIContent("Load EFFECT/E0*.P, E0*.FBC, E0*.FBT (Only Texture right now)"));
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
                EFFECT effect = new EFFECT(file);
                fileParsed++;
            }


            //EFFECT effect = new EFFECT(VSPath + "EFFECT/E008.P");

            EditorUtility.ClearProgressBar();
        }
        GUILayout.EndVertical();

        GUILayout.BeginVertical();
        GUILayout.Label("Texture Formats : ");

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
                GIM gim = new GIM(file);
                fileParsed++;
            }


            EditorUtility.ClearProgressBar();
        }

        bool LoadMENUBGTrigger = GUILayout.Button(new GUIContent("Load MENU/*BG.BIN"));
        if (LoadMENUBGTrigger && VSPath != "")
        {

            string[] files = new string[] { VSPath + "MENU/MAPBG.BIN", VSPath + "MENU/MENUBG.BIN" };
            float fileToParse = files.Length;

            float fileParsed = 0;
            foreach (string file in files)
            {
                string[] h = file.Split("/"[0]);
                string filename = h[h.Length - 1];
                EditorUtility.DisplayProgressBar("VS Parsing", "Parsing : " + filename + ", " + fileParsed + " files parsed.", (fileParsed / fileToParse));
                TIM bg = new TIM();
                bg.ParseBG(file);
                fileParsed++;
            }


            EditorUtility.ClearProgressBar();
        }

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
                DIS dis = new DIS();
                dis.Parse(file);
                fileParsed++;
            }


            EditorUtility.ClearProgressBar();
        }

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
                parser.Parse(file);
                fileParsed++;
            }


            EditorUtility.ClearProgressBar();
        }

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
        GUILayout.EndVertical();


        GUILayout.BeginVertical();
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
                ParseAKAO(file, AKAO.MUSIC, false);
                fileParsed++;
            }
            EditorUtility.ClearProgressBar();
        }
        GUILayout.EndVertical();

        GUILayout.BeginVertical();
        GUILayout.Label("Data Formats : ");
        bool LoadSYDTrigger = GUILayout.Button(new GUIContent("Load MENU DataBase.SYD"));
        if (LoadSYDTrigger && VSPath != "")
        {
            BuildDatabase();
        }

        bool LoadITEMTrigger = GUILayout.Button(new GUIContent("Load MENU ITEM*.BIN"));
        if (LoadITEMTrigger && VSPath != "")
        {
            BIN itemDB = new BIN();
            itemDB.BuildItems(VSPath + "MENU/ITEMNAME.BIN", VSPath + "MENU/ITEMHELP.BIN");
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
                parser.Parse(file);
                fileParsed++;
            }


            EditorUtility.ClearProgressBar();
        }
        GUILayout.EndVertical();


        bool LoadEXPLOTrigger = GUILayout.Button(new GUIContent("Explore..."));
        if (LoadEXPLOTrigger && VSPath != "")
        {
            //BIN parser = new BIN();
            //parser.Explore(VSPath + "SLES_027.55"); // spell and skills
            // "BATTLE/INITBTL.PRG" // Fandango
            //parser.Explore(VSPath + "BATTLE/BOG.DAT");
            /*
            string[] files = Directory.GetFiles(VSPath + "MENU/", "*.PRG");
            ToolBox.FeedDatabases(files);
            */
            BIN parser = new BIN();
            parser.Explore(VSPath + "SLES_027.55"); // spell and skills
            //PRG parser = new PRG();
            //parser.Parse(VSPath + "TITLE/TITLE.PRG"); // spell and skills
            //parser.Parse(VSPath + "ENDING/ENDING.PRG");
            //parser.Parse(VSPath + "BATTLE/BATTLE.PRG");
            //parser.Parse(VSPath + "BATTLE/INITBTL.PRG");
        }

    }

    private void ParseEVT(string path, bool UseDebug)
    {
        EVT evt = new EVT(path, UseDebug);
    }

    private void BuildDatabase()
    {
        BIN DB = new BIN();
        //List<string>[] texts = DB.BuildItems(VSPath + "MENU/ITEMNAME.BIN", VSPath + "MENU/ITEMHELP.BIN");
        DB.Parse(VSPath + "SMALL/MON.BIN");
        /*
        string[] files = Directory.GetFiles(VSPath + "MENU/", "*.SYD");
        foreach (string file in files)
        {
            SYD parser = new SYD();
            //parser.UseDebug = true;
            parser.Parse(file, texts);
        }
        */
    }


    private void ParseWEP(string path, bool UseDebug)
    {
        WEP parser = new WEP();
        parser.UseDebug = UseDebug;
        parser.Parse(path);
        parser.BuildPrefab(true);
    }
    private void ParseSHP(string path, string filename, bool UseDebug, bool erase = false)
    {
        // excp = list of models with colored vertex polygons section
        List<string> excp = new List<string>();
        excp.Add("26"); // 26.SHP is a Mimic : http://chrysaliswiki.com/bestiary:mimic#VS
        excp.Add("3A"); // D'Tok first Wyvern same as Z006U00.ZUD
        excp.Add("3B"); // a Wyvern same as Z050U00.ZUD
        excp.Add("65"); // 65.SHP is Damascus Golem  : http://chrysaliswiki.com/bestiary:damascus-golem#VS
        excp.Add("6A");
        excp.Add("6B");
        excp.Add("AC");
        excp.Add("B1"); // Platform During Final Battle
        excp.Add("B2"); // Stone
        excp.Add("B3"); // Stone
        excp.Add("B4"); // Stone
        excp.Add("B5"); // Lever / Switch (Opens Door in Wine Cellar)
        excp.Add("B6"); // Lever / Switch (Opens Door in Wine Cellar)
        excp.Add("B7"); // Stone
        excp.Add("B8"); // Stone
        excp.Add("B9"); // Stone
        excp.Add("BA"); // Stone
        excp.Add("BB"); // Stone
        excp.Add("BC"); // Stone
        excp.Add("BD"); // Stone
        excp.Add("BE"); // Stone
        excp.Add("BF"); // Stone
        excp.Add("C0"); // Stone
        excp.Add("C1"); // Stone
        excp.Add("C2"); // Stone
        excp.Add("C3"); // Stone

        SHP parser = new SHP();
        if (!excp.Contains(filename))
        {
            parser.UseDebug = UseDebug;
            parser.Parse(path);
            parser.BuildPrefab(erase);
        }
        else
        {
            parser.UseDebug = UseDebug;
            parser.excpFaces = true;
            parser.Parse(path);
            parser.BuildPrefab(true);
        }
    }

    private void ParseZND(string path, bool UseDebug)
    {
        ZND parser = new ZND();
        parser.UseDebug = UseDebug;
        parser.Parse(path);
        parser.BuildPrefab();
    }

    private void ParseZUD(string path, string filename, bool UseDebug)
    {
        List<string> excp = new List<string>();
        excp.Add("Z006U00"); // same as 3A.SHP
        excp.Add("Z050U00"); // same as 3B.SHP
        excp.Add("Z050U11"); // same as 26.SHP
        excp.Add("Z051U21"); // same as 26.SHP
        excp.Add("Z054U00"); // same as 65.SHP
        excp.Add("Z054U01"); // same as 65.SHP
        excp.Add("Z055U04"); // same as 3B.SHP (red color)
        excp.Add("Z055U05"); // same as 3B.SHP (yellow color)
        excp.Add("Z234U16"); // same as 3B.SHP (without SEQ attached)

        if (!excp.Contains(filename))
        {
            ZUD parser = new ZUD();
            parser.UseDebug = UseDebug;
            parser.Parse(path);
            parser.BuildPrefab();
        }
        else
        {
            ZUD parser = new ZUD();
            parser.excpFaces = true;
            parser.UseDebug = UseDebug;
            parser.Parse(path);
            parser.BuildPrefab();
        }
    }

    private void ParseMPD(string path, bool UseDebug)
    {
        MPD parser = new MPD();
        parser.UseDebug = false;
        parser.Parse(path);
        parser.BuildPrefab(true);
    }

    private void ParseAKAO(string path, AKAO.AKAOType type, bool UseDebug)
    {
        AKAO parser = new AKAO();
        parser.UseDebug = UseDebug;
        parser.bMID = midTrigger;
        parser.bSF2 = sf2Trigger;
        parser.bDLS = dlsTrigger;
        parser.bWAV = wavTrigger;
        parser.Parse(path, type);
    }
}
