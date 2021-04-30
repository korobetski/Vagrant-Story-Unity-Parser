using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using VS.Core;
using VS.Data;
using VS.FileFormats.WEP;
using VS.Parser;
using VS.Utils;

public class Forge : MonoBehaviour
{
    public Dropdown BladeDD;
    public WEPLoader weaponLoader;

    private VSPConfig conf;


    // Start is called before the first frame update
    void Start()
    {
        conf = Memory.LoadConfig();
        BladeDD.ClearOptions();
        BladeDD.AddOptions(new List<string> (Directory.GetFiles(conf.VSPath + "OBJ/", "*.WEP")));
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetButtonDown("Cancel"))
        {
            SceneSwitcher.ReturnToMainMenu();
        }
    }


    public void onBladeSelect()
    {
        onLoad();
    }



    public void onLoad()
    {
        string wepFileName = BladeDD.options[BladeDD.value].text;
        WEP SerializedWEP = Resources.Load<WEP>(string.Concat("Serialized/WEP/", wepFileName, ".yaml.asset"));
        if (SerializedWEP == null)
        {
            // Corresponding serialized ZND not found, so we try to serialize
            VSPConfig conf = Memory.LoadConfig();
            string wepFilePath = wepFileName;
            SerializedWEP = ScriptableObject.CreateInstance<WEP>();
            SerializedWEP.Filename = wepFileName;
            SerializedWEP.ParseFromFile(wepFilePath);

            weaponLoader.SerializedWEP = SerializedWEP;
            weaponLoader.Build();

            //ToolBox.SaveScriptableObject("Assets/Resources/Serialized/WEP/", SerializedWEP.Filename + ".WEP.yaml.asset", SerializedWEP, new Object[] { SerializedWEP.TIM });
        }
    }
}
