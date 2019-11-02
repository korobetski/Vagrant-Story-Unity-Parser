using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using VS.Data;

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

        public static void SaveDB()
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(Application.persistentDataPath + "/VSDB.db");
            VSDatas save = new VSDatas();
            save.ArmorList = Armor.JSONlist();
            save.BladeList = Blade.JSONlist();
            save.ShieldList = Shield.JSONlist();
            bf.Serialize(file, save);
            file.Close();
            if (UseDebug)
            {
                Debug.Log("/VSDB.db Saved");
            }
        }
        public static bool LoadDB()
        {
            if (File.Exists(Application.persistentDataPath + "/VSDB.db"))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(Application.persistentDataPath + "/VSDB.db", FileMode.Open);
                VSDatas save = (VSDatas)bf.Deserialize(file);
                Blade.list = JsonUtility.FromJson(save.BladeList, typeof(List<Blade>)) as List<Blade>;
                Armor.list = JsonUtility.FromJson(save.ArmorList, typeof(List<Armor>)) as List<Armor>;
                Shield.list = JsonUtility.FromJson(save.ShieldList, typeof(List<Shield>)) as List<Shield>;
                file.Close();
                if (UseDebug)
                {
                    Debug.Log("/VSDB.db Loaded");
                }

                return true;
            }
            return false;
        }


    }

    [System.Serializable]
    public class VSPConfig
    {
        public string VSPath = "";
        public string VS_Version = "";
    }

    [System.Serializable]
    public class VSDatas
    {
        public string ShieldList = "";
        public string ArmorList = "";
        public string BladeList = "";
    }
}
