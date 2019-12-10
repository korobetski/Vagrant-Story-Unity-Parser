using UnityEngine;
using UnityEngine.UI;
using VS.Core;
using VS.Entity;
using VS.Parser;
using VS.Utils;

public class MiniMapsViewver : MonoBehaviour
{
    public Dropdown myDropdown;
    public GameObject container;
    public Text roomNameTF;

    private string[] ARMList;
    private string[] MPDList;
    private GameObject selectedRoom;
    private string selectedRoomFilePath;

    //private float orthographicSizeMin = 2f;
    //private float orthographicSizeMax = 8f;
    private int idRot = 0;
    private VSPConfig conf;

    private Vector3[] camRot = new Vector3[8]
    {
        new Vector3(-25, 45, -25),
        new Vector3(0, 90, -35),
        new Vector3(25, 135, -25),
        new Vector3(35, 180, 0),
        new Vector3(25, 225, 25),
        new Vector3(0, 270, 35),
        new Vector3(-25, 315, 25),
        new Vector3(-35, 0, 0)
    };
    void Start()
    {
        conf = Memory.LoadConfig();
        ARMList = new string[30] {
            "SCEN001.ARM",
            "SCEN002.ARM",
            "SCEN003.ARM",
            "SCEN004.ARM",
            "SCEN005.ARM",
            "SCEN006.ARM",
            "SCEN007.ARM",
            "SCEN008.ARM",
            "SCEN009.ARM",
            "SCEN010.ARM",
            "SCEN011.ARM",
            "SCEN012.ARM",
            "SCEN013.ARM",
            "SCEN014.ARM",
            "SCEN015.ARM",
            "SCEN016.ARM",
            "SCEN017.ARM",
            "SCEN019.ARM",
            "SCEN020.ARM",
            "SCEN021.ARM",
            "SCEN022.ARM",
            "SCEN023.ARM",
            "SCEN024.ARM",
            "SCEN025.ARM",
            "SCEN026.ARM",
            "SCEN027.ARM",
            "SCEN028.ARM",
            "SCEN029.ARM",
            "SCEN030.ARM",
            "SCEN031.ARM"
        };


        myDropdown.onValueChanged.AddListener(delegate
        {
            myDropdownValueChangedHandler(myDropdown);
        });

        ARM aRM = new ARM();
        aRM.Parse(conf.VSPath + "SMALL/SCEN001.ARM");
        GameObject miniMapGO = aRM.BuildGameObject();
        miniMapGO.transform.parent = container.transform;
        miniMapGO.transform.localPosition = Vector3.zero;
        miniMapGO.transform.localRotation = new Quaternion();
        miniMapGO.transform.localScale = Vector3.one;
    }
    void Destroy()
    {
        myDropdown.onValueChanged.RemoveAllListeners();
    }
    private void myDropdownValueChangedHandler(Dropdown target)
    {
        foreach (Transform child in container.transform)
        {
            Destroy(child.gameObject);
        }
        ARM aRM = new ARM();
        //aRM.parseFromFile("E:/SMALL/" + ARMList[target.value]);
        aRM.Parse(conf.VSPath + "SMALL/" + ARMList[target.value]);
        GameObject miniMapGO = aRM.BuildGameObject();
        miniMapGO.transform.parent = container.transform;
        miniMapGO.transform.localPosition = Vector3.zero;
        miniMapGO.transform.localRotation = new Quaternion();
        miniMapGO.transform.localScale = Vector3.one;
    }
    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            SceneSwitcher.ReturnToMainMenu();
        }
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (selectedRoom != null)
                {
                    roomNameTF.text = "";
                    selectedRoom.GetComponent<MeshRenderer>().material = (Material)Resources.Load("Prefabs/ARMMaterial", typeof(Material));
                    selectedRoom.GetComponentInChildren<LineRenderer>().material = (Material)Resources.Load("Prefabs/ARMLineMaterial", typeof(Material));
                }
                GameObject room = hit.transform.gameObject;
                selectedRoom = room;
                roomNameTF.text = room.name;
                room.GetComponent<MeshRenderer>().material = (Material)Resources.Load("Prefabs/ARMMaterialSelected", typeof(Material));
                room.GetComponentInChildren<LineRenderer>().material = (Material)Resources.Load("Prefabs/ARMLineMaterialSelected", typeof(Material));


                MPDList = ToolBox.GetZNDRoomList(room.GetComponent<ARMRoom>().zoneNumber);
                selectedRoomFilePath = conf.VSPath + "MAP/" + MPDList[room.GetComponent<ARMRoom>().mapNumber - 1];
            }
        }

        if (Input.GetAxis("Horizontal") > 0)
        {
            Vector3 pos = container.transform.localPosition;
            pos.x -= 10;
            container.transform.localPosition = pos;
        }
        if (Input.GetAxis("Horizontal") < 0)
        {
            Vector3 pos = container.transform.localPosition;
            pos.x += 10;
            container.transform.localPosition = pos;
        }
        if (Input.GetAxis("Vertical") > 0)
        {
            Vector3 pos = container.transform.localPosition;
            pos.y -= 10;
            container.transform.localPosition = pos;
        }
        if (Input.GetAxis("Vertical") < 0)
        {
            Vector3 pos = container.transform.localPosition;
            pos.y += 10;
            container.transform.localPosition = pos;
        }

        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            Vector3 sca = container.transform.localScale;
            sca *= 0.9f;
            container.transform.localScale = sca;
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            Vector3 sca = container.transform.localScale;
            sca *= 1.1f;
            container.transform.localScale = sca;
        }

        if (Input.GetKeyDown("r"))
        {
            idRot += 1;
            if (idRot == 8)
            {
                idRot = 0;
            }

            container.transform.localRotation = Quaternion.Euler(camRot[idRot]);
        }

    }
    public void SetDropdownIndex(int index)
    {
        myDropdown.value = index;
    }
    public void OnClick()
    {
        if (selectedRoomFilePath != null)
        {
            foreach (Transform child in container.transform)
            {
                Destroy(child.gameObject);
            }


            MPD roomParser = new MPD();
            //roomParser.debugger = true;
            roomParser.Parse(selectedRoomFilePath);
            GameObject roomModel = roomParser.BuildGameObject();
            roomModel.transform.parent = container.transform;
            roomModel.transform.localPosition = Vector3.zero;
            roomModel.transform.localRotation = new Quaternion();
            roomModel.transform.localScale = Vector3.one * 10;
        }
    }
}
