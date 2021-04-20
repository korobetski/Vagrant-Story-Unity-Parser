/*
using System;
using UnityEngine;
using VagrantStory.Core;
using VagrantStory.Database;
using VS.Utils;

namespace VS.ComponentScript
{

    public class CouteauSuisse : MonoBehaviour
    {

        public MaterialsDB.eMaterials material = MaterialsDB.eMaterials.Bronze;
        [Separator("Blade")]
        public BladesDB.eBladeCategory category = BladesDB.eBladeCategory.Sword;
        [ConditionalField(nameof(category), false, BladesDB.eBladeCategory.Dagger)] public BladesDB.eDaggerBlades dagger = BladesDB.eDaggerBlades.None;
        [ConditionalField(nameof(category), false, BladesDB.eBladeCategory.Sword)] public BladesDB.eSwordBlades sword = BladesDB.eSwordBlades.None;
        [ConditionalField(nameof(category), false, BladesDB.eBladeCategory.Great_Sword)] public BladesDB.eGreatSwordBlades greatSword = BladesDB.eGreatSwordBlades.None;
        [ConditionalField(nameof(category), false, BladesDB.eBladeCategory.Axe)] public BladesDB.eAxeBlades axe = BladesDB.eAxeBlades.None;
        [ConditionalField(nameof(category), false, BladesDB.eBladeCategory.Mace)] public BladesDB.eMaceBlades mace = BladesDB.eMaceBlades.None;
        [ConditionalField(nameof(category), false, BladesDB.eBladeCategory.Great_Axe)] public BladesDB.eGreatAxeBlades greatAxe = BladesDB.eGreatAxeBlades.None;
        [ConditionalField(nameof(category), false, BladesDB.eBladeCategory.Staff)] public BladesDB.eStaffBlades staff = BladesDB.eStaffBlades.None;
        [ConditionalField(nameof(category), false, BladesDB.eBladeCategory.Heavy_Mace)] public BladesDB.eHeavyMaceBlades heavyMace = BladesDB.eHeavyMaceBlades.None;
        [ConditionalField(nameof(category), false, BladesDB.eBladeCategory.Polearm)] public BladesDB.ePolearmBlades polearm = BladesDB.ePolearmBlades.None;
        [ConditionalField(nameof(category), false, BladesDB.eBladeCategory.Crossbow)] public BladesDB.eCrossbowBlades crossbow = BladesDB.eCrossbowBlades.None;
        [ConditionalField(nameof(category), false, BladesDB.eBladeCategory.Shield)] public ArmorsDB.eShields shield = ArmorsDB.eShields.None;

        [Separator("Grip")]
        [ReadOnly] public GripsDB.eGripCategories gripType = GripsDB.eGripCategories.Guard;
        [ConditionalField(nameof(gripType), false, GripsDB.eGripCategories.Guard)] public GripsDB.eGuards guard = GripsDB.eGuards.None;
        [ConditionalField(nameof(gripType), false, GripsDB.eGripCategories.Grip)] public GripsDB.eGrips grip = GripsDB.eGrips.None;
        [ConditionalField(nameof(gripType), false, GripsDB.eGripCategories.Pole)] public GripsDB.ePoles pole = GripsDB.ePoles.None;
        [ConditionalField(nameof(gripType), false, GripsDB.eGripCategories.Bolt)] public GripsDB.eBolts bolt = GripsDB.eBolts.None;

        [Separator("Gems")]
        [ReadOnly] public uint gemSlot = 0;
        [HideInInspector] public bool gemSlot1 = false;
        [HideInInspector] public bool gemSlot2 = false;
        [HideInInspector] public bool gemSlot3 = false;
        [ConditionalField(nameof(gemSlot1))] public GemsDB.eGems gem1 = GemsDB.eGems.None;
        [ConditionalField(nameof(gemSlot2))] public GemsDB.eGems gem2 = GemsDB.eGems.None;
        [ConditionalField(nameof(gemSlot3))] public GemsDB.eGems gem3 = GemsDB.eGems.None;


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
            if (material != MaterialsDB.eMaterials.None)
            {
                statistics += MaterialsDB.List[(int)material].statistics;
            }

            if (category == BladesDB.eBladeCategory.Dagger && dagger != BladesDB.eDaggerBlades.None)
            {
                statistics += BladesDB.List[(int)dagger].statistics;
                if (_modelId != BladesDB.List[(int)dagger].wepID)
                {
                    _modelId = BladesDB.List[(int)dagger].wepID;
                    reloadModel = true;
                }
            }
            else if (category == BladesDB.eBladeCategory.Sword && sword != BladesDB.eSwordBlades.None)
            {
                statistics += BladesDB.List[(int)sword].statistics;
                if (_modelId != BladesDB.List[(int)sword].wepID)
                {
                    _modelId = BladesDB.List[(int)sword].wepID;
                    reloadModel = true;
                }
            }
            else if (category == BladesDB.eBladeCategory.Great_Sword && greatSword != BladesDB.eGreatSwordBlades.None)
            {
                statistics += BladesDB.List[(int)greatSword].statistics;
                if (_modelId != BladesDB.List[(int)greatSword].wepID)
                {
                    _modelId = BladesDB.List[(int)greatSword].wepID;
                    reloadModel = true;
                }
            }
            else if (category == BladesDB.eBladeCategory.Axe && axe != BladesDB.eAxeBlades.None)
            {
                statistics += BladesDB.List[(int)axe].statistics;
                if (_modelId != BladesDB.List[(int)axe].wepID)
                {
                    _modelId = BladesDB.List[(int)axe].wepID;
                    reloadModel = true;
                }
            }
            else if (category == BladesDB.eBladeCategory.Mace && mace != BladesDB.eMaceBlades.None)
            {
                statistics += BladesDB.List[(int)mace].statistics;
                if (_modelId != BladesDB.List[(int)mace].wepID)
                {
                    _modelId = BladesDB.List[(int)mace].wepID;
                    reloadModel = true;
                }
            }
            else if (category == BladesDB.eBladeCategory.Great_Axe && greatAxe != BladesDB.eGreatAxeBlades.None)
            {
                statistics += BladesDB.List[(int)greatAxe].statistics;
                if (_modelId != BladesDB.List[(int)greatAxe].wepID)
                {
                    _modelId = BladesDB.List[(int)greatAxe].wepID;
                    reloadModel = true;
                }
            }
            else if (category == BladesDB.eBladeCategory.Staff && staff != BladesDB.eStaffBlades.None)
            {
                statistics += BladesDB.List[(int)staff].statistics;
                if (_modelId != BladesDB.List[(int)staff].wepID)
                {
                    _modelId = BladesDB.List[(int)staff].wepID;
                    reloadModel = true;
                }
            }
            else if (category == BladesDB.eBladeCategory.Heavy_Mace && heavyMace != BladesDB.eHeavyMaceBlades.None)
            {
                statistics += BladesDB.List[(int)heavyMace].statistics;
                if (_modelId != BladesDB.List[(int)heavyMace].wepID)
                {
                    _modelId = BladesDB.List[(int)heavyMace].wepID;
                    reloadModel = true;
                }
            }
            else if (category == BladesDB.eBladeCategory.Polearm && polearm != BladesDB.ePolearmBlades.None)
            {
                statistics += BladesDB.List[(int)polearm].statistics;
                if (_modelId != BladesDB.List[(int)polearm].wepID)
                {
                    _modelId = BladesDB.List[(int)polearm].wepID;
                    reloadModel = true;
                }
            }
            else if (category == BladesDB.eBladeCategory.Crossbow && crossbow != BladesDB.eCrossbowBlades.None)
            {
                statistics += BladesDB.List[(int)crossbow].statistics;
                if (_modelId != BladesDB.List[(int)crossbow].wepID)
                {
                    _modelId = BladesDB.List[(int)crossbow].wepID;
                    reloadModel = true;
                }
            }
            else if (category == BladesDB.eBladeCategory.Shield && shield != ArmorsDB.eShields.None)
            {
                statistics += ArmorsDB.ShieldList[(int)shield].statistics;
                if (_modelId != ArmorsDB.ShieldList[(int)shield].wepID)
                {
                    _modelId = ArmorsDB.ShieldList[(int)shield].wepID;
                    reloadModel = true;
                }
            }

            if (category == BladesDB.eBladeCategory.Dagger || category == BladesDB.eBladeCategory.Sword || category == BladesDB.eBladeCategory.Great_Sword)
            {
                gripType = GripsDB.eGripCategories.Guard;
                grip = GripsDB.eGrips.None;
                pole = GripsDB.ePoles.None;
                bolt = GripsDB.eBolts.None;

                if (guard != GripsDB.eGuards.None)
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
            else if (category == BladesDB.eBladeCategory.Axe || category == BladesDB.eBladeCategory.Mace || category == BladesDB.eBladeCategory.Great_Axe
                || category == BladesDB.eBladeCategory.Staff || category == BladesDB.eBladeCategory.Heavy_Mace)
            {
                gripType = GripsDB.eGripCategories.Grip;
                guard = GripsDB.eGuards.None;
                pole = GripsDB.ePoles.None;
                bolt = GripsDB.eBolts.None;
                if (grip != GripsDB.eGrips.None)
                {
                    gemSlot = GripsDB.List[(int)grip].gemSlots; statistics += GripsDB.List[(int)grip].statistics;
                }
                else
                {
                    gemSlot = 0;
                }

                RefreshGemSlots();
            }
            else if (category == BladesDB.eBladeCategory.Polearm)
            {
                gripType = GripsDB.eGripCategories.Pole;
                guard = GripsDB.eGuards.None;
                grip = GripsDB.eGrips.None;
                bolt = GripsDB.eBolts.None;
                if (pole != GripsDB.ePoles.None) { gemSlot = GripsDB.List[(int)pole].gemSlots; statistics += GripsDB.List[(int)pole].statistics; }
                else
                {
                    gemSlot = 0;
                }

                RefreshGemSlots();
            }
            else if (category == BladesDB.eBladeCategory.Crossbow)
            {
                gripType = GripsDB.eGripCategories.Bolt;
                guard = GripsDB.eGuards.None;
                grip = GripsDB.eGrips.None;
                pole = GripsDB.ePoles.None;
                if (bolt != GripsDB.eBolts.None) { gemSlot = GripsDB.List[(int)bolt].gemSlots; statistics += GripsDB.List[(int)bolt].statistics; }
                else
                {
                    gemSlot = 0;
                }

                RefreshGemSlots();
            }
            else if (category == BladesDB.eBladeCategory.Shield)
            {
                // shields have no grip, but have gem slots
                gripType = GripsDB.eGripCategories.None;
                guard = GripsDB.eGuards.None;
                grip = GripsDB.eGrips.None;
                pole = GripsDB.ePoles.None;
                bolt = GripsDB.eBolts.None;

                if (shield != ArmorsDB.eShields.None)
                {
                    gemSlot = ArmorsDB.ShieldList[(int)shield].gemSlots;
                }
                else
                {
                    gemSlot = 0;
                }

                RefreshGemSlots();
            }
            else
            {
                gripType = GripsDB.eGripCategories.None;
                guard = GripsDB.eGuards.None;
                grip = GripsDB.eGrips.None;
                pole = GripsDB.ePoles.None;
                bolt = GripsDB.eBolts.None;
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
                gem1 = GemsDB.eGems.None;
            }
            else
            {
                if (gem1 != GemsDB.eGems.None)
                {
                    statistics += GemsDB.List[(int)gem1].statistics;
                }
            }

            if (!gemSlot2)
            {
                gem2 = GemsDB.eGems.None;
            }
            else
            {
                if (gem2 != GemsDB.eGems.None)
                {
                    statistics += GemsDB.List[(int)gem2].statistics;
                }
            }

            if (!gemSlot3)
            {
                gem3 = GemsDB.eGems.None;
            }
            else
            {
                if (gem3 != GemsDB.eGems.None)
                {
                    statistics += GemsDB.List[(int)gem3].statistics;
                }
            }
        }
    }

}
*/