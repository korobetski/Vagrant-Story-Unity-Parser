using System.Collections.Generic;
using UnityEngine;

namespace VS.Data
{
    public enum ShieldType
    {
        UNARMED = 0,
        Buckler_Shield = 96,
        Pelta_Shield = 102,
        Targe_Shield = 99,
        Quad_Shield = 100,
        Circle_Shield = 105,
        Tower_Shield = 101,
        Spiked_Shield = 106,
        Round_Shield = 98,
        Kite_Shield = 107,
        Casserole_Shield = 108,
        Heater_Shield = 104,
        Oval_Shield = 103,
        Knight_Shield = 111,
        Hoplite_Shield = 97,
        Jazeraint_Shield = 109,
        Dread_Shield = 110
    }
    [System.Serializable]
    public class Shield : MonoBehaviour
    {
        public static readonly List<string> ShieldIds = new List<string> { "60", "61", "62", "63", "64", "65", "66", "67", "68", "69", "6A", "6B", "6C", "6A", "6E", "6F" };
        public static List<Shield> list = new List<Shield>();
        public static string JSONlist()
        {
            return JsonUtility.ToJson(list);
        }
        public static Shield GetShieldByWEP(byte WEPID)
        {
            foreach (Shield s in list)
            {
                if (s.WEP == WEPID)
                {
                    return s;
                }
            }

            return null;
        }
        internal static ShieldType Library(int weaponId)
        {
            ShieldType[] lib = new ShieldType[128];
            lib[0] = ShieldType.UNARMED;
            lib[96] = ShieldType.Buckler_Shield;
            lib[97] = ShieldType.Hoplite_Shield;
            lib[98] = ShieldType.Round_Shield;
            lib[99] = ShieldType.Targe_Shield;
            lib[100] = ShieldType.Quad_Shield;
            lib[101] = ShieldType.Tower_Shield;
            lib[102] = ShieldType.Oval_Shield;
            lib[103] = ShieldType.Pelta_Shield;
            lib[104] = ShieldType.Circle_Shield;
            lib[105] = ShieldType.Heater_Shield;
            lib[106] = ShieldType.Spiked_Shield;
            lib[107] = ShieldType.Kite_Shield;
            lib[108] = ShieldType.Casserole_Shield;
            lib[109] = ShieldType.Jazeraint_Shield;
            lib[110] = ShieldType.Dread_Shield;
            lib[111] = ShieldType.Knight_Shield;

            return lib[weaponId];
        }



        [SerializeField]
        private string _name = "";
        [SerializeField]
        private byte _ID;
        [SerializeField]
        private byte _WEP;
        [SerializeField]
        private byte _GemSlots;
        [SerializeField]
        private sbyte _STR;
        [SerializeField]
        private sbyte _INT;
        [SerializeField]
        private sbyte _AGI;


        private ShieldType _type = ShieldType.UNARMED;

        public Shield(byte[] rawDatas)
        {
            _ID = rawDatas[0];
            _WEP = rawDatas[1];
            _GemSlots = rawDatas[3];
            _STR = (sbyte)rawDatas[4];
            _INT = (sbyte)rawDatas[5];
            _AGI = (sbyte)rawDatas[6];

            //Debug.Log(this.ToString());
        }

        public string Name { get => _name; set => _name = value; }
        public byte ID { get => _ID; set => _ID = value; }
        public byte WEP
        {
            get => _WEP; set
            {
                _WEP = value;
                Shield db = GetShieldByWEP(_WEP);
                _name = db.Name;
                _ID = db.ID;
                _GemSlots = db.GemSlots;
                _STR = db.STR;
                _INT = db.INT;
                _AGI = db.AGI;
            }
        }
        public byte GemSlots { get => _GemSlots; set => _GemSlots = value; }
        public sbyte STR { get => _STR; set => _STR = value; }
        public sbyte INT { get => _INT; set => _INT = value; }
        public sbyte AGI { get => _AGI; set => _AGI = value; }
        public ShieldType type
        {
            get => _type;
            set => _type = value;
        }



        public override string ToString()
        {
            return "Shield #" + _ID + " : " + _name + "  .WEP ID : " + _WEP + " " + _GemSlots + " Gem Slots [STR:" + _STR + "|INT:" + _INT + "|AGI:" + _AGI + "]";
        }
        public string ToJSON()
        {
            return JsonUtility.ToJson(this);
        }
    }
}