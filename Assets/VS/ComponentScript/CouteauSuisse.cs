using MyBox;
using System;
using UnityEngine;
using VagrantStory.Core;
using VagrantStory.Database;
using VS.Utils;

namespace VS.ComponentScript
{



    public class CouteauSuisse : MonoBehaviour
    {
        public enum eMaterials { None, Wood, Leather, Bronze, Iron, Hagane, Silver, Damascus };
        public enum eWeaponCategories { None, Dagger, Sword, Great_Sword, Axe, Mace, Great_Axe, Staff, Heavy_Mace, Polearm, Crossbow, Shield };
        public enum eGripCategories { None, Guard, Grip, Pole, Bolt };
        public enum eGems
        {
            None,
            Talos_Feldspear,
            Titan_Malachite,
            Sylphid_Topaz,
            Djinn_Amber,
            Salamander_Ruby,
            Ifrit_Carnelian,
            Gnome_Emerald,
            Dao_Moonstone,
            Undine_Jasper,
            Marid_Aquamarine,
            Angel_Pearl,
            Seraphim_Diamond,
            Morlock_Jet,
            Berial_Black_Pearl,
            Haeralis,
            Orlandu,
            Orion,
            Ogmius,
            Iocus,
            Balvus,
            Trinity,
            Beowulf,
            Dragonite,
            Sigguld,
            Demonia,
            Altema,
            Polaris,
            Basivalin,
            Galerian,
            Vedivier,
            Berion,
            Gervin,
            Tertia,
            Lancer,
            Arturos,
            Braveheart,
            Hellraiser,
            Nightkiller,
            Manabreaker,
            Powerfist,
            Brainshield,
            Speedster,
            Silent_Queen,
            Dark_Queen,
            Death_Queen,
            White_Queen,
        };

        public enum eDaggerBlades
        {
            None = 0,
            Battle_Knife = 1,
            Scramasax,
            Dirk,
            Throwing_Knife,
            Kudi,
            Cinquedea,
            Kris,
            Hatchet,
            Khukuri,
            Baselard,
            Stiletto,
            Jamadhar
        };
        public enum eSwordBlades
        {
            None = 0,
            Spatha = 13,
            Scimitar,
            Rapier,
            Short_Sword,
            Firangi,
            Shamshir,
            Falchion,
            Shotel,
            Khora,
            Khophish,
            Wakizashi,
            Rhomphaia
        };
        public enum eGreatSwordBlades
        {
            None = 0,
            Broad_Sword = 25,
            Norse_Sword,
            Katana,
            Executioner,
            Claymore,
            Schiavona,
            Bastard_Sword,
            Nodachi,
            Rune_Blade,
            Holy_Wind
        };
        public enum eAxeBlades
        {
            None = 0,
            Hand_Axe = 35,
            Battle_Axe,
            Francisca,
            Tabarzin,
            Chamkaq,
            Tabar,
            Bullova,
            Crescent
        };
        public enum eMaceBlades
        {
            None = 0,
            Goblin_Club = 43,
            Spiked_Club,
            Ball_Mace,
            Footmans_Mace,
            Morning_Star,
            War_Hammer,
            Bec_de_Corbin,
            War_Maul

        };
        public enum eGreatAxeBlades
        {
            None = 0,
            Guisarme = 51,
            Large_Crescent,
            Sabre_Halberd,
            Balbriggin,
            Double_Blade,
            Halberd
        };
        public enum eStaffBlades
        {
            None = 0,
            Wizard_Staff = 57,
            Clergy_Rod,
            Summoner_Baton,
            Shamanic_Staff,
            Bishops_Crosier,
            Sages_Cane
        };
        public enum eHeavyMaceBlades
        {
            None = 0,
            Langdebeve = 63,
            Sabre_Mace,
            Footmans_Mace,
            Gloomwing,
            Mjolnir,
            Griever,
            Destroyer,
            Hand_Of_Light
        };
        public enum ePolearmBlades
        {
            None = 0,
            Spear = 71,
            Glaive,
            Scorpion,
            Corcesca,
            Trident,
            Awl_Pike,
            Boar_Spear,
            Fauchard,
            Voulge,
            Pole_Axe,
            Bardysh,
            Brandestoc
        };
        public enum eCrossbowBlades
        {
            None = 0,
            Gastraph_Bow = 83,
            Light_Crossbow,
            Target_Bow,
            Windlass,
            Cranquein,
            Lug_Crossbow,
            Siege_Bow,
            Arbalest
        };
        public enum eShields
        {
            None = 0,
            Buckler_Shield = 1,
            Hoplite_Shield,
            Round_Shield,
            Targe_Shield,
            Quad_Shield,
            Tower_Shield,
            Oval_Shield,
            Pelta_Shield,
            Circle_Shield,
            Heater_Shield,
            Spiked_Shield,
            Kite_Shield,
            Casserole_Shield,
            Jazeraint_Shield,
            Dread_Shield,
            Knight_Shield
        };

        public enum eGuards { None = 0, Short_Hilt = 1, Swept_Hilt, Cross_Guard, Knuckle_Guard, Counter_Guard, Side_Ring, Power_Palm, Murderers_Hilt, Spiral_Hilt }
        public enum eGrips { None = 0, Wooden_Grip = 10, Sand_Face, Czekan_Type, Sarissa_Grip, Gendarme, Heavy_Grip, Runkastyle, Bhuj_Type, Grimoire_Grip, Elephant }
        public enum ePoles { None = 0, Wooden_Pole = 20, Spiculum_Pole, Winged_Pole, Framea_Pole, Ahlspies, Spiral_Pole }
        public enum eBolts { None = 0, Simple_Bolt = 26, Steel_Bolt, Javelin_Bolt, Falarica_Bolt, Stone_Bullet, Sonic_Bullet }

        public eMaterials material = eMaterials.Bronze;
        [Separator("Blade")]
        public eWeaponCategories category = eWeaponCategories.Sword;
        [ConditionalField(nameof(category), false, eWeaponCategories.Dagger)] public eDaggerBlades dagger = eDaggerBlades.None;
        [ConditionalField(nameof(category), false, eWeaponCategories.Sword)] public eSwordBlades sword = eSwordBlades.None;
        [ConditionalField(nameof(category), false, eWeaponCategories.Great_Sword)] public eGreatSwordBlades greatSword = eGreatSwordBlades.None;
        [ConditionalField(nameof(category), false, eWeaponCategories.Axe)] public eAxeBlades axe = eAxeBlades.None;
        [ConditionalField(nameof(category), false, eWeaponCategories.Mace)] public eMaceBlades mace = eMaceBlades.None;
        [ConditionalField(nameof(category), false, eWeaponCategories.Great_Axe)] public eGreatAxeBlades greatAxe = eGreatAxeBlades.None;
        [ConditionalField(nameof(category), false, eWeaponCategories.Staff)] public eStaffBlades staff = eStaffBlades.None;
        [ConditionalField(nameof(category), false, eWeaponCategories.Heavy_Mace)] public eHeavyMaceBlades heavyMace = eHeavyMaceBlades.None;
        [ConditionalField(nameof(category), false, eWeaponCategories.Polearm)] public ePolearmBlades polearm = ePolearmBlades.None;
        [ConditionalField(nameof(category), false, eWeaponCategories.Crossbow)] public eCrossbowBlades crossbow = eCrossbowBlades.None;
        [ConditionalField(nameof(category), false, eWeaponCategories.Shield)] public eShields shield = eShields.None;

        [Separator("Grip")]
        [ReadOnly] public eGripCategories gripType = eGripCategories.Guard;
        [ConditionalField(nameof(gripType), false, eGripCategories.Guard)] public eGuards guard = eGuards.None;
        [ConditionalField(nameof(gripType), false, eGripCategories.Grip)] public eGrips grip = eGrips.None;
        [ConditionalField(nameof(gripType), false, eGripCategories.Pole)] public ePoles pole = ePoles.None;
        [ConditionalField(nameof(gripType), false, eGripCategories.Bolt)] public eBolts bolt = eBolts.None;

        [Separator("Gems")]
        [ReadOnly] public uint gemSlot = 0;
        [HideInInspector] public bool gemSlot1 = false;
        [HideInInspector] public bool gemSlot2 = false;
        [HideInInspector] public bool gemSlot3 = false;
        [ConditionalField(nameof(gemSlot1))] public eGems gem1 = eGems.None;
        [ConditionalField(nameof(gemSlot2))] public eGems gem2 = eGems.None;
        [ConditionalField(nameof(gemSlot3))] public eGems gem3 = eGems.None;


        [Separator("Statistics")]
        [ReadOnly] public Statistics statistics;



        private byte _modelId = 0;

        private void Start()
        {
        }


        private void OnValidate()
        {
            bool reloadModel = false;

            statistics = new Statistics();
            if (material != eMaterials.None)
            {
                statistics += MaterialsDB.List[(int)material].statistics;
            }

            if (category == eWeaponCategories.Dagger && dagger != eDaggerBlades.None)
            {
                statistics += BladesDB.List[(int)dagger].statistics;
                if (_modelId != BladesDB.List[(int)dagger].wepID)
                {
                    _modelId = BladesDB.List[(int)dagger].wepID;
                    reloadModel = true;
                }
            }
            else if (category == eWeaponCategories.Sword && sword != eSwordBlades.None)
            {
                statistics += BladesDB.List[(int)sword].statistics;
                if (_modelId != BladesDB.List[(int)sword].wepID)
                {
                    _modelId = BladesDB.List[(int)sword].wepID;
                    reloadModel = true;
                }
            }
            else if (category == eWeaponCategories.Great_Sword && greatSword != eGreatSwordBlades.None)
            {
                statistics += BladesDB.List[(int)greatSword].statistics;
                if (_modelId != BladesDB.List[(int)greatSword].wepID)
                {
                    _modelId = BladesDB.List[(int)greatSword].wepID;
                    reloadModel = true;
                }
            }
            else if (category == eWeaponCategories.Axe && axe != eAxeBlades.None)
            {
                statistics += BladesDB.List[(int)axe].statistics;
                if (_modelId != BladesDB.List[(int)axe].wepID)
                {
                    _modelId = BladesDB.List[(int)axe].wepID;
                    reloadModel = true;
                }
            }
            else if (category == eWeaponCategories.Mace && mace != eMaceBlades.None)
            {
                statistics += BladesDB.List[(int)mace].statistics;
                if (_modelId != BladesDB.List[(int)mace].wepID)
                {
                    _modelId = BladesDB.List[(int)mace].wepID;
                    reloadModel = true;
                }
            }
            else if (category == eWeaponCategories.Great_Axe && greatAxe != eGreatAxeBlades.None)
            {
                statistics += BladesDB.List[(int)greatAxe].statistics;
                if (_modelId != BladesDB.List[(int)greatAxe].wepID)
                {
                    _modelId = BladesDB.List[(int)greatAxe].wepID;
                    reloadModel = true;
                }
            }
            else if (category == eWeaponCategories.Staff && staff != eStaffBlades.None)
            {
                statistics += BladesDB.List[(int)staff].statistics;
                if (_modelId != BladesDB.List[(int)staff].wepID)
                {
                    _modelId = BladesDB.List[(int)staff].wepID;
                    reloadModel = true;
                }
            }
            else if (category == eWeaponCategories.Heavy_Mace && heavyMace != eHeavyMaceBlades.None)
            {
                statistics += BladesDB.List[(int)heavyMace].statistics;
                if (_modelId != BladesDB.List[(int)heavyMace].wepID)
                {
                    _modelId = BladesDB.List[(int)heavyMace].wepID;
                    reloadModel = true;
                }
            }
            else if (category == eWeaponCategories.Polearm && polearm != ePolearmBlades.None)
            {
                statistics += BladesDB.List[(int)polearm].statistics;
                if (_modelId != BladesDB.List[(int)polearm].wepID)
                {
                    _modelId = BladesDB.List[(int)polearm].wepID;
                    reloadModel = true;
                }
            }
            else if (category == eWeaponCategories.Crossbow && crossbow != eCrossbowBlades.None)
            {
                statistics += BladesDB.List[(int)crossbow].statistics;
                if (_modelId != BladesDB.List[(int)crossbow].wepID)
                {
                    _modelId = BladesDB.List[(int)crossbow].wepID;
                    reloadModel = true;
                }
            }
            else if (category == eWeaponCategories.Shield && shield != eShields.None)
            {
                statistics += ArmorsDB.ShieldList[(int)shield].statistics;
                if (_modelId != ArmorsDB.ShieldList[(int)shield].wepID)
                {
                    _modelId = ArmorsDB.ShieldList[(int)shield].wepID;
                    reloadModel = true;
                }
            }

            if (category == eWeaponCategories.Dagger || category == eWeaponCategories.Sword || category == eWeaponCategories.Great_Sword)
            {
                gripType = eGripCategories.Guard;
                //guard = eGuards.None;
                grip = eGrips.None;
                pole = ePoles.None;
                bolt = eBolts.None;

                if (guard != eGuards.None)
                {
                    gemSlot = GripsDB.List[(int)guard].gemSlots;
                    statistics += GripsDB.List[(int)guard].statistics;
                }
                else
                {
                    gemSlot = 0;
                }

                RefreshGemSlots();
            }
            else if (category == eWeaponCategories.Axe || category == eWeaponCategories.Mace || category == eWeaponCategories.Great_Axe || category == eWeaponCategories.Staff || category == eWeaponCategories.Heavy_Mace)
            {
                gripType = eGripCategories.Grip;
                guard = eGuards.None;
                //grip = eGrips.None;
                pole = ePoles.None;
                bolt = eBolts.None;
                if (grip != eGrips.None)
                {
                    gemSlot = GripsDB.List[(int)grip].gemSlots; statistics += GripsDB.List[(int)grip].statistics;
                }
                else
                {
                    gemSlot = 0;
                }

                RefreshGemSlots();
            }
            else if (category == eWeaponCategories.Polearm)
            {
                gripType = eGripCategories.Pole;
                guard = eGuards.None;
                grip = eGrips.None;
                //pole = ePoles.None;
                bolt = eBolts.None;
                if (pole != ePoles.None) { gemSlot = GripsDB.List[(int)pole].gemSlots; statistics += GripsDB.List[(int)pole].statistics; }
                else
                {
                    gemSlot = 0;
                }

                RefreshGemSlots();
            }
            else if (category == eWeaponCategories.Crossbow)
            {
                gripType = eGripCategories.Bolt;
                guard = eGuards.None;
                grip = eGrips.None;
                pole = ePoles.None;
                //bolt = eBolts.None;
                if (bolt != eBolts.None) { gemSlot = GripsDB.List[(int)bolt].gemSlots; statistics += GripsDB.List[(int)bolt].statistics; }
                else
                {
                    gemSlot = 0;
                }

                RefreshGemSlots();
            }
            else if (category == eWeaponCategories.Shield)
            {
                gripType = eGripCategories.None;
                guard = eGuards.None;
                grip = eGrips.None;
                pole = ePoles.None;
                bolt = eBolts.None;

                if (shield != eShields.None) { gemSlot = ArmorsDB.ShieldList[(int)shield].gemSlots; }
                else
                {
                    gemSlot = 0;
                }

                RefreshGemSlots();
            }
            else
            {
                gripType = eGripCategories.None;
                guard = eGuards.None;
                grip = eGrips.None;
                pole = ePoles.None;
                bolt = eBolts.None;
                gemSlot = 0;
                RefreshGemSlots();
            }

            if (reloadModel)
            {
                LoadWEPModel(_modelId);
            }
        }

        private void LoadWEPModel(byte wepId)
        {
            ToolBox.DestroyChildren(gameObject, true);

            string wepPath = string.Concat("Prefabs/Weapons/", BitConverter.ToString(new byte[] { wepId }));


            GameObject wepPrefab = Instantiate(Resources.Load(wepPath)) as GameObject;
            wepPrefab.transform.parent = gameObject.transform;
        }

        private void RefreshGemSlots()
        {
            gemSlot1 = (gemSlot > 0);
            gemSlot2 = (gemSlot > 1);
            gemSlot3 = (gemSlot > 2);

            if (!gemSlot1)
            {
                gem1 = eGems.None;
            }
            else
            {
                if(gem1 != eGems.None) statistics += GemsDB.List[(int)gem1].statistics;
            }

            if (!gemSlot2)
            {
                gem2 = eGems.None;
            }
            else
            {
                if (gem2 != eGems.None) statistics += GemsDB.List[(int)gem2].statistics;
            }

            if (!gemSlot3)
            {
                gem3 = eGems.None;
            }
            else
            {
                if (gem3 != eGems.None) statistics += GemsDB.List[(int)gem3].statistics;
            }
        }
    }

}
