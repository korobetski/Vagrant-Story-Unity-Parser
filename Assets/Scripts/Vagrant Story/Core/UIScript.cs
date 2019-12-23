using UnityEngine;
using UnityEngine.UI;

namespace VagrantStory.Core
{

    public class UIScript : MonoBehaviour
    {

        public PlayerInfos playerInfos;


        public Sprite FullHead;
        public Sprite FullBody;
        public Sprite FullBatModeRightArm;
        public Sprite FullBatModeLeftArm;
        public Sprite FullComModeRightArm;
        public Sprite FullComModeLeftArm;
        public Sprite FullLegs;

        public Sprite WellHead;
        public Sprite WellBody;
        public Sprite WellBatModeRightArm;
        public Sprite WellBatModeLeftArm;
        public Sprite WellComModeRightArm;
        public Sprite WellComModeLeftArm;
        public Sprite WellLegs;

        public Sprite MediumHead;
        public Sprite MediumBody;
        public Sprite MediumBatModeRightArm;
        public Sprite MediumBatModeLeftArm;
        public Sprite MediumComModeRightArm;
        public Sprite MediumComModeLeftArm;
        public Sprite MediumLegs;

        public Sprite DangerHead;
        public Sprite DangerBody;
        public Sprite DangerBatModeRightArm;
        public Sprite DangerBatModeLeftArm;
        public Sprite DangerComModeRightArm;
        public Sprite DangerComModeLeftArm;
        public Sprite DangerLegs;


        private Slider uiHP;
        private Slider uiMP;
        //public Slider uiRisk;

        private GameObject _head;
        private GameObject _body;
        private GameObject _rightArm;
        private GameObject _leftArm;
        private GameObject _legs;

        // Start is called before the first frame update
        void Start()
        {
            uiHP = GameObject.Find("SliderHP").GetComponent<Slider>();
            uiMP = GameObject.Find("SliderMP").GetComponent<Slider>();

            uiHP.maxValue = playerInfos.MaxHP;
            uiMP.maxValue = playerInfos.MaxMP;

            _head = GameObject.Find("body_status_head");
            _body = GameObject.Find("body_status_body");
            _rightArm = GameObject.Find("body_status_right_arm");
            _leftArm = GameObject.Find("body_status_left_arm");
            _legs = GameObject.Find("body_status_legs");
        }

        // Update is called once per frame
        void Update()
        {
            uiHP.value = playerInfos.HP;
            uiMP.value = playerInfos.MP;

            Sprite BatModeRightArm = null;
            Sprite BatModeLeftArm = null;
            Sprite ComModeRightArm = null;
            Sprite ComModeLeftArm = null;

            switch (playerInfos.HeadStatus)
            {
                case PlayerInfos.BodyPartStatus.PERFECT:
                    _head.GetComponent<SpriteRenderer>().sprite = FullHead;
                    break;
                case PlayerInfos.BodyPartStatus.WELL:
                    _head.GetComponent<SpriteRenderer>().sprite = WellHead;
                    break;
                case PlayerInfos.BodyPartStatus.MEDIUM:
                    _head.GetComponent<SpriteRenderer>().sprite = MediumHead;
                    break;
                case PlayerInfos.BodyPartStatus.DANGER:
                    _head.GetComponent<SpriteRenderer>().sprite = DangerHead;
                    break;
            }

            switch (playerInfos.BodyStatus)
            {
                case PlayerInfos.BodyPartStatus.PERFECT:
                    _body.GetComponent<SpriteRenderer>().sprite = FullBody;
                    break;
                case PlayerInfos.BodyPartStatus.WELL:
                    _body.GetComponent<SpriteRenderer>().sprite = WellBody;
                    break;
                case PlayerInfos.BodyPartStatus.MEDIUM:
                    _body.GetComponent<SpriteRenderer>().sprite = MediumBody;
                    break;
                case PlayerInfos.BodyPartStatus.DANGER:
                    _body.GetComponent<SpriteRenderer>().sprite = DangerBody;
                    break;
            }

            switch (playerInfos.RightArmStatus)
            {
                case PlayerInfos.BodyPartStatus.PERFECT:
                    BatModeRightArm = FullBatModeRightArm;
                    ComModeRightArm = FullComModeRightArm;
                    break;
                case PlayerInfos.BodyPartStatus.WELL:
                    BatModeRightArm = WellBatModeRightArm;
                    ComModeRightArm = WellComModeRightArm;
                    break;
                case PlayerInfos.BodyPartStatus.MEDIUM:
                    BatModeRightArm = MediumBatModeRightArm;
                    ComModeRightArm = MediumComModeRightArm;
                    break;
                case PlayerInfos.BodyPartStatus.DANGER:
                    BatModeRightArm = DangerBatModeRightArm;
                    ComModeRightArm = DangerComModeRightArm;
                    break;
            }

            switch (playerInfos.LeftArmStatus)
            {
                case PlayerInfos.BodyPartStatus.PERFECT:
                    BatModeLeftArm = FullBatModeLeftArm;
                    ComModeLeftArm = FullComModeLeftArm;
                    break;
                case PlayerInfos.BodyPartStatus.WELL:
                    BatModeLeftArm = WellBatModeLeftArm;
                    ComModeLeftArm = WellComModeLeftArm;
                    break;
                case PlayerInfos.BodyPartStatus.MEDIUM:
                    BatModeLeftArm = MediumBatModeLeftArm;
                    ComModeLeftArm = MediumComModeLeftArm;
                    break;
                case PlayerInfos.BodyPartStatus.DANGER:
                    BatModeLeftArm = DangerBatModeLeftArm;
                    ComModeLeftArm = DangerComModeLeftArm;
                    break;
            }

            switch (playerInfos.LegsStatus)
            {
                case PlayerInfos.BodyPartStatus.PERFECT:
                    _legs.GetComponent<SpriteRenderer>().sprite = FullLegs;
                    break;
                case PlayerInfos.BodyPartStatus.WELL:
                    _legs.GetComponent<SpriteRenderer>().sprite = WellLegs;
                    break;
                case PlayerInfos.BodyPartStatus.MEDIUM:
                    _legs.GetComponent<SpriteRenderer>().sprite = MediumLegs;
                    break;
                case PlayerInfos.BodyPartStatus.DANGER:
                    _legs.GetComponent<SpriteRenderer>().sprite = DangerLegs;
                    break;
            }


            if (playerInfos.BattleMode)
            {
                _rightArm.GetComponent<SpriteRenderer>().sprite = BatModeRightArm;
                _leftArm.GetComponent<SpriteRenderer>().sprite = BatModeLeftArm;
            }
            else
            {
                _rightArm.GetComponent<SpriteRenderer>().sprite = ComModeRightArm;
                _leftArm.GetComponent<SpriteRenderer>().sprite = ComModeLeftArm;
            }
        }
    }

}