using System;
using System.Collections.Generic;
using UnityEngine;
using VS.FileFormats.SHP;
using VS.FileFormats.WEP;
using VS.Utils;

namespace VS.FileFormats.ZUD
{
    class ZUDLoader:MonoBehaviour
    {
        public ZUD SerializedZUD;

        private SHPLoader SHPContainer;
        private WEPLoader WEPContainer;
        private WEPLoader WEP2Container;


        private void Start()
        {
            Build();
        }

        private void OnValidate()
        {
            Build();
        }

        private void Build()
        {
            ToolBox.DestroyChildren(gameObject, true);

            SHPContainer = null;
            WEPContainer = null;
            WEP2Container = null;

            if (SerializedZUD != null)
            {
                GameObject SHPGO = new GameObject("SHP_"+SerializedZUD.idCharacter);
                SHPGO.transform.parent = gameObject.transform;
                SHPContainer = SHPGO.AddComponent<SHPLoader>();
                SHPContainer.SerializedSHP = SerializedZUD.zudShape;

                if (SerializedZUD.zudWeapon != null)
                {
                    GameObject WEPGO = new GameObject("WEP_" + SerializedZUD.idWeapon);
                    WEPGO.transform.parent = gameObject.transform;
                    WEPContainer = WEPGO.AddComponent<WEPLoader>();
                    WEPContainer.SerializedWEP = SerializedZUD.zudWeapon;
                    WEPContainer.Build();
                }

                if (SerializedZUD.zudShield != null)
                {
                    GameObject WEP2GO = new GameObject("Shield_" + SerializedZUD.idShield);
                    WEP2GO.transform.parent = gameObject.transform;
                    WEP2Container = WEP2GO.AddComponent<WEPLoader>();
                    WEP2Container.SerializedWEP = SerializedZUD.zudShield;
                    WEP2Container.Build();
                }

                if (SerializedZUD.zudComSeq != null)
                {
                    SHPContainer.SerializedSEQ = SerializedZUD.zudComSeq;
                }

                else if (SerializedZUD.zudBatSeq != null)
                {
                    SHPContainer.SerializedSEQ = SerializedZUD.zudBatSeq;
                }


                SHPContainer.Build();
                WEPContainer.gameObject.transform.parent = SHPContainer.GetWeaponBone().transform;
                WEPContainer.gameObject.transform.localPosition = Vector3.zero;
                WEPContainer.gameObject.transform.localRotation = Quaternion.identity;
                WEP2Container.gameObject.transform.parent = SHPContainer.GetShieldBone().transform;
                WEP2Container.gameObject.transform.localPosition = Vector3.zero;
                WEP2Container.gameObject.transform.localRotation = Quaternion.identity;
            }
        }
    }
}
