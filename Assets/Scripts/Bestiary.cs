using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using VS.Data;

public class Bestiary : MonoBehaviour
{
    public GameObject container;
    public Text monterName;
    public Text monterDesc;



    // Start is called before the first frame update
    void Start()
    {
        StreamReader sr = File.OpenText(Application.dataPath + "/Resources/JSON/MONSTER.json");
        string json = sr.ReadToEnd();

        json = json.Replace("[", "");
        json = json.Replace("]", "");
        json = json.Replace("},", "}§");
        string[] jsons = json.Split("§"[0]);
        Monster[] monsters = new Monster[jsons.Length];
        for (int i = 0; i < jsons.Length; i++)
        {
            //Debug.Log(jsons[i]);
            monsters[i] = JsonUtility.FromJson<Monster>(jsons[i]);
            Debug.Log(monsters[i].ToString());
        }

        int monsterId = 24;

        
        monterName.text = monsters[monsterId].name;
        monterDesc.text = monsters[monsterId].desc;
        /*
        monterName.text = "";
        monterDesc.text = "";
        */
        foreach (Transform child in container.transform)
        {
            Destroy(child.gameObject);
        }
        sbyte sb = (sbyte)monsters[monsterId].shp1;
        string path = string.Concat("Prefabs/Models/SHP_", BitConverter.ToString(new byte[] { (byte)sb }));
        GameObject prefab = Instantiate(Resources.Load<GameObject>(path));

        prefab.transform.parent = container.transform;
        prefab.transform.localPosition = Vector3.zero;
        prefab.transform.localRotation = new Quaternion(0, -180, 0, 0);
        prefab.transform.localScale = Vector3.one * 75;


        //Monster[] monsters = JsonHelper.FromJson<Monster>(json);
        //Debug.Log(monsters);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
