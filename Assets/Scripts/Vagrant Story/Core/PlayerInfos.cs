using System.Collections.Generic;
using UnityEngine;
using VagrantStory.Core;
using VagrantStory.Database;
using VagrantStory.Items;

public class PlayerInfos : MonoBehaviour
{


    public ushort HP = 250;
    public ushort MaxHP = 250;
    public ushort MP = 100;
    public ushort MaxMP = 100;
    public ushort Risk = 0;
    public ushort MaxRisk = 100;

    public string Map = "Map009";

    // Equipement HELM, ARMOR, GLOVE, BOOTS, ACCESSORY
    public Armor Helm;
    public Armor BodyArmor;
    public Armor LeftGlove;
    public Armor RightGlove;
    public Armor Boots;
    public Armor Accessory;
    public Weapon MainHand;
    public Armor OffHand; // only shields can be off handed in Vagrant Story

    public bool BattleMode = false;
    public Transform WeaponRootTransfrom;

    // Body status
    public enum BodyPartStatus { PERFECT, WELL, MEDIUM, DANGER }; // blue, green, yellow, red : 100% - 75% - 50% - 25% -0%
    public BodyPartStatus HeadStatus = BodyPartStatus.PERFECT;
    public BodyPartStatus BodyStatus = BodyPartStatus.PERFECT;
    public BodyPartStatus RightArmStatus = BodyPartStatus.PERFECT;
    public BodyPartStatus LeftArmStatus = BodyPartStatus.PERFECT;
    public BodyPartStatus LegsStatus = BodyPartStatus.PERFECT;

    public List<Item> Inventory;
    public List<Spell> Spells;
    public List<BreakArt> BreakArts;
    public List<CombatTech> CombatTechs;



    private GameObject weaponGO;
    private GameObject shieldGO;


    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();

        Inventory = new List<Item>();
        Spells = new List<Spell>();
        BreakArts = new List<BreakArt>();
        CombatTechs = new List<CombatTech>();

        MainHand = WeaponDB.Fandango;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Weapon Switch"))
        {
            BattleMode = !BattleMode;
            if (BattleMode)
            {
                weaponGO = MainHand.GameObject;
                weaponGO.transform.parent = WeaponRootTransfrom;
                weaponGO.transform.localPosition = Vector3.zero;
                weaponGO.transform.localRotation = new Quaternion();
                weaponGO.transform.localScale = Vector3.one;
                animator.SetInteger("Weapon Type", (int)MainHand.blade.bladeType);
                animator.SetLayerWeight(0, 0f);
                animator.SetLayerWeight((int)MainHand.blade.bladeType, 1f);
            }
            else
            {
                Destroy(weaponGO);
                animator.SetInteger("Weapon Type", 0);
                animator.SetLayerWeight(0, 1f);
                animator.SetLayerWeight((int)MainHand.blade.bladeType, 0f);
            }
        }
    }
}
