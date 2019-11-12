using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using VS.Core;
using VS.Parser;

public class VSWindow : EditorWindow
{
    private string VSPath = "";
    private VSPConfig conf;


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
            VSPath = path;
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
        GUILayout.Label("Batch imports", EditorStyles.boldLabel);
        bool LoadSYDTrigger = GUILayout.Button(new GUIContent("Load MENU DataBase.SYD"));
        if (LoadSYDTrigger && VSPath != "")
        {
            BuildDatabase();
        }

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
                parser.BuildPrefab();
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
                WEP parser = new WEP();
                //parser.UseDebug = true;
                parser.Parse(file);
                parser.BuildPrefab();
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

            List<string> excp = new List<string>();
            excp.Add("26.SHP"); // 26.SHP is a Mimic : http://chrysaliswiki.com/bestiary:mimic#VS
            excp.Add("3A.SHP"); // a dragon maybe the first Wyvern like Z006U00.ZUD
            excp.Add("3B.SHP"); // a dragon maybe the first Wyvern like Z006U00.ZUD
            excp.Add("6A.SHP");
            excp.Add("6B.SHP");
            excp.Add("AC.SHP");
            excp.Add("AE.SHP");
            excp.Add("B1.SHP"); // Platform During Final Battle
            excp.Add("B2.SHP"); // Stone
            excp.Add("B3.SHP"); // Stone
            excp.Add("B4.SHP"); // Stone
            excp.Add("B5.SHP"); // Lever / Switch (Opens Door in Wine Cellar)
            excp.Add("B6.SHP"); // Lever / Switch (Opens Door in Wine Cellar)
            excp.Add("B7.SHP"); // Stone
            excp.Add("B8.SHP"); // Stone
            excp.Add("B9.SHP"); // Stone
            excp.Add("BA.SHP"); // Stone
            excp.Add("BB.SHP"); // Stone
            excp.Add("BC.SHP"); // Stone
            excp.Add("BD.SHP"); // Stone
            excp.Add("BE.SHP"); // Stone
            excp.Add("BF.SHP"); // Stone
            excp.Add("C0.SHP"); // Stone
            excp.Add("C1.SHP"); // Stone
            excp.Add("C2.SHP"); // Stone
            excp.Add("C3.SHP"); // Stone

            List<string> fatal = new List<string>();
            fatal.Add("50.SHP");
            fatal.Add("6D.SHP");
            fatal.Add("7D.SHP");
            fatal.Add("91.SHP");
            fatal.Add("A4.SHP");
            fatal.Add("A5.SHP");
            fatal.Add("AB.SHP");
            fatal.Add("AF.SHP");
            fatal.Add("B0.SHP");
            fatal.Add("C6.SHP");

            foreach (string file in files)
            {
                string[] h = file.Split("/"[0]);
                string filename = h[h.Length - 1];
                EditorUtility.DisplayProgressBar("VS Parsing", "Parsing : " + filename + ", " + fileParsed + " files parsed.", (fileParsed / fileToParse));

                if (filename == "65.SHP")
                {
                    // 65.SHP is Damascus Golem  : http://chrysaliswiki.com/bestiary:damascus-golem#VS
                    // But somthings wrong when parsing, so we try to use the mesh of 37.SHP Simple Golem
                    SHP parser = new SHP();
                    parser.Parse(VSPath + "OBJ/65.SHP");
                    AssetDatabase.CopyAsset("Assets/Resources/Prefabs/Models/37.prefab", "Assets/Resources/Prefabs/Models/65.prefab");
                    GameObject golemPrefab = AssetDatabase.LoadAssetAtPath("Assets/Resources/Prefabs/Models/65.prefab", typeof(GameObject)) as GameObject;

                    AssetDatabase.AddObjectToAsset(parser.texture, "Assets/Resources/Prefabs/Models/65.prefab");
                    golemPrefab.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial.mainTexture = parser.texture;
                    AssetDatabase.SaveAssets();
                }
                else if (!excp.Contains(filename) && !fatal.Contains(filename))
                {
                    SHP parser = new SHP();
                    //parser.UseDebug = true;
                    parser.Parse(file);
                    parser.BuildPrefab();
                }
                else
                {
                    if (!fatal.Contains(filename))
                    {
                        SHP parser = new SHP();
                        parser.UseDebug = true;
                        parser.Parse(file);
                        //parser.buildPrefab();
                    }
                    else
                    {
                        // cannot parse fatal files
                    }
                }
                fileParsed++;
            }
            EditorUtility.ClearProgressBar();
        }

        bool LoadZUDTrigger = GUILayout.Button(new GUIContent("Load Zones Units Datas.ZUD"));
        if (LoadZUDTrigger && VSPath != "")
        {

            BuildDatabase();
            string[] files = Directory.GetFiles(VSPath + "MAP/", "*.ZUD");
            float fileToParse = files.Length;
            float fileParsed = 0f;

            List<string> excp = new List<string>();
            excp.Add("Z006U00.ZUD");
            excp.Add("Z050U00.ZUD");
            excp.Add("Z050U11.ZUD");
            excp.Add("Z051U21.ZUD");
            excp.Add("Z054U00.ZUD");
            excp.Add("Z054U01.ZUD");
            excp.Add("Z055U04.ZUD");
            excp.Add("Z055U05.ZUD");
            excp.Add("Z234U16.ZUD");


            List<string> fatal = new List<string>();
            fatal.Add("Z027U04.ZUD");

            foreach (string file in files)
            {
                string[] h = file.Split("/"[0]);
                string filename = h[h.Length - 1];
                EditorUtility.DisplayProgressBar("VS Parsing", "Parsing : " + filename + ", " + fileParsed + " files parsed.", (fileParsed / fileToParse));

                if (!excp.Contains(filename) && !fatal.Contains(filename))
                {
                    ZUD parser = new ZUD();
                    //parser.UseDebug = true;
                    parser.Parse(file);
                    parser.BuildPrefab();
                }
                else
                {
                    if (!fatal.Contains(filename))
                    {
                        ZUD parser = new ZUD();
                        parser.UseDebug = true;
                        parser.Parse(file);
                    }
                    else
                    {
                        // cannot parse fatal files
                    }
                }

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
                MPD parser = new MPD();
                //parser.UseDebug = true;
                parser.Parse(file);
                parser.BuildPrefab();
                fileParsed++;
            }
            EditorUtility.ClearProgressBar();
        }
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
        bool LoadAKAO2Trigger = GUILayout.Button(new GUIContent("Load Akao MUSIC/MUSIC*.DAT"));
        if (LoadAKAO2Trigger && VSPath != "")
        {

            string[] files = Directory.GetFiles(VSPath + "MUSIC/", "*.DAT");
            float fileToParse = files.Length;
            /*
            float fileParsed = 0;
            foreach (string file in files)
            {
                string[] h = file.Split("/"[0]);
                string filename = h[h.Length - 1];
                EditorUtility.DisplayProgressBar("VS Parsing", "Parsing : " + filename + ", " + fileParsed + " files parsed.", (fileParsed / fileToParse));
                AKAO parser = new AKAO();
                //parser.UseDebug = true;
                parser.Parse(file, AKAO.MUSIC);
                if (parser.FileSize > 4)
                {
                    parser.composer.OutputMidiFile();
                }
                fileParsed++;
            }
            */

            AKAO parser = new AKAO();
            parser.UseDebug = true;
            parser.Parse(VSPath + "MUSIC/MUSIC020.DAT", AKAO.MUSIC);
            if (parser.FileSize > 4)
            {
                parser.composer.OutputMidiFile();
            }

            EditorUtility.ClearProgressBar();
        }
    }


    private void BuildDatabase()
    {
        string[] files = Directory.GetFiles(VSPath + "MENU/", "*.SYD");
        foreach (string file in files)
        {
            Debug.Log(file);
            SYD parser = new SYD();
            //parser.UseDebug = true;
            parser.Parse(file);
        }
        Memory.SaveDB();
    }
}
