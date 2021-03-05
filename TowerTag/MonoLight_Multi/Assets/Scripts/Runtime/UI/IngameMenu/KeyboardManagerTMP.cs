using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace VRUiKits.Utils
{
    public class KeyboardManagerTMP : MonoBehaviour
    {
        #region Public Variables

        [Header("User defined")] [Tooltip("If the character is uppercase at the initialization")]
        public bool isUppercase;

        [Header("Essentials")] public Transform keys;

        public static TMP_InputField Target
        {
            get
            {
                var _this = EventSystem.current.currentSelectedGameObject;

                if (null != _this && null != _this.GetComponent<InputField>())
                {
                    return _this.GetComponent<TMP_InputField>();
                }

                if (null != target)
                {
                    return target;
                }

                return null;
            }
            set { target = value; }
        }

        #endregion

        #region Private Variables

        /*
         Record a helper target for some 3rd party packages which lost focus when
         user click on keyboard.
         */
        private static TMP_InputField target;

        private string Input
        {
            get
            {
                if (null == Target)
                {
                    return "";
                }

                return Target.text;
            }
            set
            {
                if (null == Target)
                {
                    return;
                }

                Target.text = value;
                // Force target input field activated if losing selection
                Target.ActivateInputField();
                Target.MoveTextEnd(false);
            }
        }

        private Key[] keyList;
        private bool capslockFlag;
        private bool IsCharacterLimitReached => Input.ToCharArray().Length >= Target.characterLimit;

        #endregion

        #region Monobehaviour Callbacks

        void Awake()
        {
            keyList = keys.GetComponentsInChildren<Key>(true);
        }

        void Start()
        {
            foreach (var key in keyList)
            {
                key.OnKeyClicked += GenerateInput;
            }

            capslockFlag = isUppercase;
            CapsLock();
        }

        #endregion

        #region Public Methods

        [UsedImplicitly]
        public void Backspace()
        {
            if (Input.Length > 0)
            {
                Input = Input.Remove(Input.Length - 1);
            }
        }

        [UsedImplicitly]
        public void Clear()
        {
            Input = "";
        }

        [UsedImplicitly]
        public void CapsLock()
        {
            foreach (var key in keyList)
            {
                if (key is Alphabet)
                {
                    key.CapsLock(capslockFlag);
                }
            }

            capslockFlag = !capslockFlag;
        }

        [UsedImplicitly]
        public void Shift()
        {
            foreach (var key in keyList)
            {
                if (key is Shift)
                {
                    key.ShiftKey();
                }
            }
        }

        [UsedImplicitly]
        public void GenerateInput(string s)
        {
            if(!IsCharacterLimitReached)
                Input += s;
        }

        #endregion
    }
}