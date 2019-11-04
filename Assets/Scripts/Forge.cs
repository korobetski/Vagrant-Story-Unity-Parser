using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VS.Core;
using VS.Data;
using VS.Parser;
using VS.Utils;

public class Forge : MonoBehaviour
{
    public Dropdown MaterialDD;
    public Dropdown WeaponTypeDD;
    public Dropdown BladeDD;
    public Dropdown GripDD;
    public Dropdown Gem1DD;
    public Dropdown Gem2DD;
    public Dropdown Gem3DD;
    public GameObject container;
    public Text weaponName;
    public Text weaponDesc;
    public GameObject CAT;
    ClassAffinityType catCompo;
    private VSPConfig conf;

    private string WEPFilePath;
    private int WEPID = 0;

    public static readonly List<string> DaggerBlades = new List<string> { "Battle Knife", "Scramasax", "Dirk", "Throwing Knife", "Kudi", "Cinquedea", "Kris", "Hatchet", "Khukuri", "Baselard", "Stiletto", "Jamadhar" };
    public static readonly List<string> SwordBlades = new List<string> {
         "Spatha",
         "Scimitar",
         "Rapier",
         "Short Sword",
         "Firangi",
         "Shamshir",
         "Falchion",
         "Shotel",
         "Khora",
         "Khophish",
         "Wakizashi",
         "Rhomphaia"
     };
    public static readonly List<string> GreatSwordBlades = new List<string> {
         "Broad Sword",
         "Norse Sword",
         "Katana",
         "Executioner",
         "Claymore",
         "Schiavona",
         "Bastard Sword",
         "Nodachi",
         "Rune Blade",
         "Holy Wind"
     };
    public static readonly List<string> AxeBlades = new List<string> {
         "Hand Axe",
         "Battle Axe",
         "Francisca",
         "Tabarzin",
         "Chamkaq",
         "Tabar",
         "Bullova",
         "Crescent"
     };
    public static readonly List<string> MaceBlades = new List<string> {
         "Goblin Club",
         "Spiked Club",
         "Ball Mace",
         "Footmans Mace",
         "Morning Star",
         "War Hammer",
         "Bec de Corbin",
         "War Maul"
     };
    public static readonly List<string> GreatAxeBlades = new List<string> {
         "Guisarme",
         "Large Crescent",
         "Sabre Halberd",
         "Balbriggin",
         "Double Blade",
         "Halberd"
     };
    public static readonly List<string> StaffBlades = new List<string> {
         "Wizard Staff",
         "Clergy Rod",
         "Summoner Baton",
         "Shamanic Staff",
         "Bishops Crosier",
         "Sages Cane"
     };
    public static readonly List<string> HeavyMaceBlades = new List<string> {
         "Langdebeve",
         "Sabre Mace",
         "Footmans Mace",
         "Gloomwing",
         "Mjolnir",
         "Griever",
         "Destroyer",
         "Hand Of Light"
     };
    public static readonly List<string> PolearmBlades = new List<string> {
         "Spear",
         "Glaive",
         "Scorpion",
         "Corcesca",
         "Trident",
         "Awl Pike",
         "Boar Spear",
         "Fauchard",
         "Voulge",
         "Pole Axe",
         "Bardysh",
         "Brandestoc"
     };
    public static readonly List<string> CrossbowBlades = new List<string> {
         "Gastraph Bow",
         "Light Crossbow",
         "Target Bow",
         "Windlass",
         "Cranquein",
         "Lug Crossbow",
         "Siege Bow",
         "Arbalest"
     };
    public static readonly List<string> Shields = new List<string>
    {
        "Buckler_Shield",
        "Hoplite_Shield",
        "Round_Shield",
        "Targe_Shield",
        "Quad_Shield",
        "Tower_Shield",
        "Oval_Shield",
        "Pelta_Shield",
        "Circle_Shield",
        "Heater_Shield",
        "Spiked_Shield",
        "Kite_Shield",
        "Casserole_Shield",
        "Jazeraint_Shield",
        "Dread_Shield",
        "Knight_Shield"
     };

    public static readonly List<string> GripTypeGuard = new List<string> { "Short Hilt", "Swept Hilt", "Cross Guard", "Knuckle Guard", "Counter Guard", "Side Ring", "Power Palm", "Murderer's Hilt", "Spiral Hilt" };
    public static readonly List<string> GripTypeGrip = new List<string> {
        "Wooden Grip",
        "Sand Face",
        "Czekan Type",
        "Sarissa Grip",
        "Gendarme",
        "Heavy Grip",
        "Runkastyle",
        "Bhuj Type",
        "Grimoire Grip",
        "Elephant"
    };
    public static readonly List<string> GripTypePole = new List<string>
    {
        "Wooden Pole",
        "Spiculum Pole",
        "Winged Pole",
        "Framea Pole",
        "Ahlspies",
        "Spiral Pole",
    };
    public static readonly List<string> GripTypeBolt = new List<string>
    {
        "Simple Bolt",
        "Steel Bolt",
        "Javelin Bolt",
        "Falarica Bolt",
        "Stone Bullet",
        "Sonic Bullet"
    };

    private GameObject model;

    // Start is called before the first frame update
    void Start()
    {
        conf = Memory.LoadConfig();
        catCompo = CAT.GetComponent<ClassAffinityType>();
        ToolBox.FeedDatabases(new string[] { conf.VSPath + "MENU/SHIELD.SYD", conf.VSPath + "MENU/ARMOR.SYD", conf.VSPath + "MENU/BLADE.SYD", conf.VSPath + "MENU/ITEMNAME.BIN", conf.VSPath + "MENU/ITEMHELP.BIN" });
        Grip.GripList();
        Gem.GemList();

        BladeDD.enabled = false;
        GripDD.enabled = false;
        Gem1DD.enabled = false;
        Gem2DD.enabled = false;
        Gem3DD.enabled = false;
        BladeDD.ClearOptions();
        GripDD.ClearOptions();
        Gem1DD.ClearOptions();
        Gem2DD.ClearOptions();
        Gem3DD.ClearOptions();

        catCompo.RAZ();
        MaterialDD.ClearOptions();
        MaterialDD.AddOptions(new List<string>() { "Wood", "Leather", "Bronze", "Iron", "Hagane", "Silver", "Damascus" });
        WeaponTypeDD.ClearOptions();
        WeaponTypeDD.AddOptions(new List<string>() { "Weapon Type", "Dagger", "Sword", "Greatwsord", "Axe", "Mace", "Great Axe", "Staff", "Heavy Mace", "Polearm", "Crossbow", "Shield" });
    }

    // Update is called once per frame
    void Update()
    {
        if (container.transform.childCount > 0)
        {
            Quaternion qw = container.transform.GetChild(0).localRotation;
            qw.x += 0.1f * Time.deltaTime;
            container.transform.GetChild(0).localRotation = qw;

        }

        if (Input.GetButtonDown("Cancel"))
        {
            SceneSwitcher.ReturnToMainMenu();
        }
    }

    public void onMaterialSelect()
    {
        actualiseCAT();
    }

    public void onWeaponTypeSelect()
    {
        BladeDD.enabled = false;
        GripDD.enabled = false;
        Gem1DD.enabled = false;
        Gem2DD.enabled = false;
        Gem3DD.enabled = false;
        BladeDD.ClearOptions();
        GripDD.ClearOptions();
        Gem1DD.ClearOptions();
        Gem2DD.ClearOptions();
        Gem3DD.ClearOptions();
        catCompo.RAZ();
        switch (WeaponTypeDD.value)
        {
            case 0:
                break;
            case 1://dagger
                BladeDD.enabled = true;
                BladeDD.AddOptions(Forge.DaggerBlades);
                BladeDD.value = 0;
                WEPFilePath = Blade.DaggerBlades[0];
                GripDD.enabled = true;
                GripDD.AddOptions(Forge.GripTypeGuard);
                break;
            case 2://Sword
                BladeDD.enabled = true;
                BladeDD.AddOptions(Forge.SwordBlades);
                BladeDD.value = 0;
                WEPFilePath = Blade.SwordBlades[0];
                GripDD.enabled = true;
                GripDD.AddOptions(Forge.GripTypeGuard);
                break;
            case 3://Greatwsord
                BladeDD.enabled = true;
                BladeDD.AddOptions(Forge.GreatSwordBlades);
                BladeDD.value = 0;
                WEPFilePath = Blade.GreatSwordBlades[0];
                GripDD.enabled = true;
                GripDD.AddOptions(Forge.GripTypeGuard);
                break;
            case 4://Axe
                BladeDD.enabled = true;
                BladeDD.AddOptions(Forge.AxeBlades);
                BladeDD.value = 0;
                WEPFilePath = Blade.AxeBlades[0];
                GripDD.enabled = true;
                GripDD.AddOptions(Forge.GripTypeGrip);
                break;
            case 5://Mace
                BladeDD.enabled = true;
                BladeDD.AddOptions(Forge.MaceBlades);
                BladeDD.value = 0;
                WEPFilePath = Blade.MaceBlades[0];
                GripDD.enabled = true;
                GripDD.AddOptions(Forge.GripTypeGrip);
                break;
            case 6://Great axe
                BladeDD.enabled = true;
                BladeDD.AddOptions(Forge.GreatAxeBlades);
                BladeDD.value = 0;
                WEPFilePath = Blade.GreatAxeBlades[0];
                GripDD.enabled = true;
                GripDD.AddOptions(Forge.GripTypeGrip);
                break;
            case 7://Staff
                BladeDD.enabled = true;
                BladeDD.AddOptions(Forge.StaffBlades);
                BladeDD.value = 0;
                WEPFilePath = Blade.StaffBlades[0];
                GripDD.enabled = true;
                GripDD.AddOptions(Forge.GripTypeGrip);
                break;
            case 8://Heavy mace
                BladeDD.enabled = true;
                BladeDD.AddOptions(Forge.HeavyMaceBlades);
                BladeDD.value = 0;
                WEPFilePath = Blade.HeavyMaceBlades[0];
                GripDD.enabled = true;
                GripDD.AddOptions(Forge.GripTypeGrip);
                break;
            case 9://polearm
                BladeDD.enabled = true;
                BladeDD.AddOptions(Forge.PolearmBlades);
                BladeDD.value = 0;
                WEPFilePath = Blade.PolearmBlades[0];
                GripDD.enabled = true;
                GripDD.AddOptions(Forge.GripTypePole);
                break;
            case 10://crossbow
                BladeDD.enabled = true;
                BladeDD.AddOptions(Forge.CrossbowBlades);
                BladeDD.value = 0;
                WEPFilePath = Blade.CrossbowBlades[0];
                GripDD.enabled = true;
                GripDD.AddOptions(Forge.GripTypeBolt);
                break;
            case 11://shield
                BladeDD.enabled = true;
                BladeDD.AddOptions(Forge.Shields);
                BladeDD.value = 0;
                WEPFilePath = Shield.ShieldIds[0];
                break;
            default:
                break;
        }

    }

    public void onBladeSelect()
    {

        WEPFilePath = "";
        switch (WeaponTypeDD.value)
        {
            case 1://dagger
                WEPFilePath = Blade.DaggerBlades[BladeDD.value];
                break;
            case 2://Sword
                WEPFilePath = Blade.SwordBlades[BladeDD.value];
                break;
            case 3://Greatwsord
                WEPFilePath = Blade.GreatSwordBlades[BladeDD.value];
                break;
            case 4://Axe
                WEPFilePath = Blade.AxeBlades[BladeDD.value];
                break;
            case 5://Mace
                WEPFilePath = Blade.MaceBlades[BladeDD.value];
                break;
            case 6://Great axe
                WEPFilePath = Blade.GreatAxeBlades[BladeDD.value];
                break;
            case 7://Staff
                WEPFilePath = Blade.StaffBlades[BladeDD.value];
                break;
            case 8://Heavy mace
                WEPFilePath = Blade.HeavyMaceBlades[BladeDD.value];
                break;
            case 9://polearm
                WEPFilePath = Blade.PolearmBlades[BladeDD.value];
                break;
            case 10://crossbow
                WEPFilePath = Blade.CrossbowBlades[BladeDD.value];
                break;
            case 11://shield
                WEPFilePath = Shield.ShieldIds[BladeDD.value];
                break;
            default:
                break;
        }
        WEPID = byte.Parse(WEPFilePath, System.Globalization.NumberStyles.HexNumber);
        weaponName.text = L10n.itemNames[WEPID];
        weaponDesc.text = L10n.itemDescs[WEPID];
        onLoad();
    }
    public void onGripSelect()
    {
        Grip selectedGrip = Grip.FindByName(GripDD.options[GripDD.value].text);
        //Debug.Log(selectedGrip);
        if (selectedGrip.GemSlots > 0)
        {
            Gem1DD.enabled = true;
            Gem1DD.ClearOptions();
            Gem1DD.AddOptions(Gem.slist);
        }
        if (selectedGrip.GemSlots > 1)
        {
            Gem2DD.enabled = true;
            Gem2DD.ClearOptions();
            Gem2DD.AddOptions(Gem.slist);
        }
        if (selectedGrip.GemSlots > 2)
        {
            Gem3DD.enabled = true;
            Gem3DD.ClearOptions();
            Gem3DD.AddOptions(Gem.slist);
        }
        actualiseCAT();
    }
    public void onGem1Select()
    {
        actualiseCAT();
    }
    public void onGem2Select()
    {
        actualiseCAT();
    }
    public void onGem3Select()
    {
        actualiseCAT();
    }

    private void actualiseCAT()
    {
        if (WEPID > 0)
        {
            catCompo.RAZ();
            if (WeaponTypeDD.value == 11)
            {
                /*
                Debug.Log("Shield ID = "+WEPID);
                Debug.Log("Shield.list.Count : "+Shield.list.Count);
                Debug.Log("Shield = " + Shield.GetShield(WEPID).ToString());
                */
                catCompo.STR = Shield.GetShieldByWEP((byte)WEPID).STR;
                catCompo.INT = Shield.GetShieldByWEP((byte)WEPID).INT;
                catCompo.AGI = Shield.GetShieldByWEP((byte)WEPID).AGI;
            }
            else
            {
                //Debug.Log(Blade.list[WEPID]);
                catCompo.Range = Blade.list[WEPID].Range;
                catCompo.Risk = Blade.list[WEPID].RISK;
                catCompo.STR = Blade.list[WEPID].STR;
                catCompo.INT = Blade.list[WEPID].INT;
                catCompo.AGI = Blade.list[WEPID].AGI;

                Grip grp = Grip.FindByName(GripDD.options[GripDD.value].text);
                catCompo.STR = grp.STR;
                catCompo.INT = grp.INT;
                catCompo.AGI = grp.AGI;
                catCompo.Blunt = grp.Blunt;
                catCompo.Edged = grp.Edged;
                catCompo.Piercing = grp.Piercing;
            }
            //catCompo += SmithMaterial.list[MaterialDD.value];

            SmithMaterial[] mats = new SmithMaterial[]{
                SmithMaterial.Wood,
                SmithMaterial.Leather,
                SmithMaterial.Bronze,
                SmithMaterial.Iron,
                SmithMaterial.Hagane,
                SmithMaterial.Silver,
                SmithMaterial.Damascus
            };

            catCompo += mats[MaterialDD.value];

            if (Gem1DD.enabled)
            {
                catCompo += Gem.list[Gem1DD.value];
            }

            if (Gem2DD.enabled)
            {
                catCompo += Gem.list[Gem2DD.value];
            }

            if (Gem3DD.enabled)
            {
                catCompo += Gem.list[Gem3DD.value];
            }
        }
    }

    public void onLoad()
    {
        foreach (Transform child in container.transform)
        {
            Destroy(child.gameObject);
        }

        WEP wepModelParser = new WEP();
        wepModelParser.Parse(conf.VSPath + "OBJ/" + WEPFilePath + ".WEP");
        GameObject wepGO = wepModelParser.BuildGameObject();
        wepGO.transform.parent = container.transform;
        wepGO.transform.localPosition = Vector3.zero;
        wepGO.transform.localRotation = new Quaternion();
        wepGO.transform.localScale = Vector3.one * 25;
        actualiseCAT();
    }
}
