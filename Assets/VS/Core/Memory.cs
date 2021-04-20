using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
namespace VS.Core
{
    public class Memory
    {
        public static bool UseDebug = false;
        //public static string VSPath;
        //public static string VS_Version = "None";

        public static void SaveConfig(VSPConfig save)
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(Application.persistentDataPath + "/VSP.conf");
            bf.Serialize(file, save);
            file.Close();
            if (UseDebug)
            {
                Debug.Log("VSP.conf Saved");
            }
        }
        public static VSPConfig LoadConfig()
        {
            if (File.Exists(Application.persistentDataPath + "/VSP.conf"))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(Application.persistentDataPath + "/VSP.conf", FileMode.Open);
                VSPConfig save = (VSPConfig)bf.Deserialize(file);
                file.Close();
                if (UseDebug)
                {
                    Debug.Log("VSP.conf Loaded");
                }

                return save;
            }
            return null;
        }
    }

    [Serializable]
    public class VSPConfig
    {
        public string VSPath = "";
        public string VS_Version = "";
    }
}
