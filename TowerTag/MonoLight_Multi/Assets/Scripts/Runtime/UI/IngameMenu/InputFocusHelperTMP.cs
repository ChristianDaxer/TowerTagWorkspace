using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.EventSystems;

namespace VRUiKits.Utils
{
    public class InputFocusHelperTMP : MonoBehaviour, ISelectHandler
    {
        private TMP_InputField input;

        void Awake()
        {
            input = GetComponent<TMP_InputField>();
        }

        public void OnSelect(BaseEventData eventData)
        {
            /*
            Set keyboard target explicitly for some 3rd party packages which lost focus when
            user click on keyboard.
            */
            KeyboardManagerTMP.Target = input;
            StartCoroutine(ActivateInputFieldWithCaret());
        }

        IEnumerator ActivateInputFieldWithCaret()
        {
            input.ActivateInputField();

            yield return new WaitForEndOfFrame();

            if (EventSystem.current.currentSelectedGameObject == input.gameObject)
            {
                input.MoveTextEnd(false);
            }
        }
    }
}