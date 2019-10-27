using System.Collections.Generic;
using UnityEngine;


namespace VS.Data
{
    [System.Serializable]
    public class Blade : MonoBehaviour
    {
        public static List<Blade> list = new List<Blade>();
        public static string JSONlist()
        {
            return JsonUtility.ToJson(list);
        }

        public static Blade GetBladeByWEP(byte WEPID)
        {
            foreach (Blade b in list)
            {
                if (b.WEP == WEPID)
                {
                    return b;
                }
            }

            return null;
        }

        [SerializeField]
        private string _name = "";
        [SerializeField]
        private byte _ID;
        [SerializeField]
        private byte _WEP;
        [SerializeField]
        private byte _BladeType;
        [SerializeField]
        private byte _DamageType;
        [SerializeField]
        private byte _RISK;
        [SerializeField]
        private sbyte _STR;
        [SerializeField]
        private sbyte _INT;
        [SerializeField]
        private sbyte _AGI;
        [SerializeField]
        private byte _Range;
        [SerializeField]
        private byte _Damage;

        public string Name { get => _name; set => _name = value; }
        public byte ID { get => _ID; set => _ID = value; }
        public byte WEP
        {
            get => _WEP; set
            {
                _WEP = value;
                Blade db = GetBladeByWEP(_WEP);
                _name = db.Name;
                _ID = db.ID;
                _BladeType = db.TypeOfBlade;
                _DamageType = db.DamageType;
                _RISK = db.RISK;
                _STR = db.STR;
                _INT = db.INT;
                _AGI = db.AGI;
                _Range = db.Range;
                _Damage = db.Damage;
            }
        }
        public string sWEP { get => "" + _WEP; set => _WEP = byte.Parse(value); }
        public byte TypeOfBlade { get => _BladeType; set => _BladeType = value; }
        public byte DamageType { get => _DamageType; set => _DamageType = value; }
        public byte RISK { get => _RISK; set => _RISK = value; }
        public sbyte STR { get => _STR; set => _STR = value; }
        public sbyte INT { get => _INT; set => _INT = value; }
        public sbyte AGI { get => _AGI; set => _AGI = value; }
        public byte Range { get => _Range; set => _Range = value; }
        public byte Damage { get => _Damage; set => _Damage = value; }

        public Blade()
        {
            _ID = 0;
            _WEP = 0;
            _BladeType = 0;
            _DamageType = 0;
            _RISK = 0;
            _STR = 0;
            _INT = 0;
            _AGI = 0;
            _Range = 0;
            _Damage = 0;
        }
        public Blade(byte[] rawDatas)
        {
            // From "MENU/BLADE.SYD"
            //ID|ID.WEP|weapon type|damage type|02|Risk|0000|STR|INT|AGI|00|Range|?|range|always 01
            _ID = rawDatas[0];
            _WEP = rawDatas[1];
            _BladeType = rawDatas[2];
            _DamageType = rawDatas[3];
            //rawDatas[4] always 02 
            _RISK = rawDatas[5];
            _STR = (sbyte)rawDatas[8];
            _INT = (sbyte)rawDatas[9];
            _AGI = (sbyte)rawDatas[10];
            _Range = rawDatas[12];
            _Damage = rawDatas[13]; // Not sure about that
        }

        public override string ToString()
        {
            return "Blade #" + _ID + " : " + _name + "  .WEP ID : " + _WEP + " Type : " + _BladeType + "  [STR:" + _STR + "|INT:" + _INT + "|AGI:" + _AGI + "]";
        }
        public string ToJSON()
        {
            return JsonUtility.ToJson(this);
        }


    }
}
