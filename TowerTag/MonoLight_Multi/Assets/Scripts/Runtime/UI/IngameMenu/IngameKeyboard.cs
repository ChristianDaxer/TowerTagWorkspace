using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Home.UI
{
    [RequireComponent(typeof(TMP_InputField))]
    public class IngameKeyboard : MonoBehaviour
    {
        [SerializeField] private InputFieldHelper.InputFieldType _inputFieldType;
        [SerializeField] UnityEvent _onValueChangedCallback;

        [SerializeField, Tooltip("Those selectables won't be interactable while the keyboard is spawned")]
        private Selectable[] _selectableUIComponents;

        [SerializeField, Tooltip("Keyboard gets spawned in front of this canvas")]
        private Transform _spawnAnchor;

        [SerializeField] private string _title;

        [Header("Input Conditions")] [SerializeField]
        private int _minCharacters;

        private GameObject _keyboard;
        private TMP_InputField _inputField;

        [Header("ShakeSettings")] [SerializeField]
        private float _shakeRadius = 0.05f;

        [SerializeField] private float _duration = 0.25f;
        private Vector3 _inputTextFieldStartPosition;

        public InputFieldHelper.InputFieldType InputFieldType
        {
            set => _inputFieldType = value;
        }

        private bool _exitWithoutSaving;

        private void Awake()
        {
            _inputField = GetComponent<TMP_InputField>();
        }

        /// <summary>
        /// Activate/Deactivate ingame ui keyboard
        /// </summary>
        /// <param name="status">new status of the keyboard</param>
        public void ToggleIngameKeyboard(bool status)
        {
            if (!TowerTagSettings.Home) return;

            if (status && _keyboard == null)
            {
                InstantiateKeyboard();
            }
            else if (!status && _keyboard != null)
            {
                if (!_exitWithoutSaving)
                {
                    var inputFieldTextComponent = _keyboard.GetComponent<KeyboardReferences>().InputField.textComponent;
                    if (!IsInputValid(inputFieldTextComponent))
                    {
                        StartCoroutine(ShakeText(inputFieldTextComponent, _duration));
                        return;
                    }
                }

                DestroyKeyboard();
            }

            ToggleAllSettingsUiObjects(!status);
        }

        private void InstantiateKeyboard()
        {
            _keyboard = InstantiateWrapper.InstantiateWithMessage(InputFieldHelper.TypeToGameObject[_inputFieldType], _inputField.transform);
            _keyboard.transform.position = _spawnAnchor.position;
            KeyboardReferences keyboardReferences = _keyboard.GetComponent<KeyboardReferences>();
            InputFieldHelper.SetInputFieldSettings(keyboardReferences.InputField, _inputFieldType);
            keyboardReferences.InputField.text = _inputField.text;
            keyboardReferences.Title.text = _title;
            StartCoroutine(DelayedFocusOnObject(keyboardReferences.InputField.gameObject));
            if (_onValueChangedCallback != null)
            {
                keyboardReferences.InputField.onValueChanged.AddListener(delegate
                {
                    _onValueChangedCallback.Invoke();
                });
            }

            keyboardReferences.EnterButton.onClick.AddListener(delegate { ToggleIngameKeyboard(false); });
            keyboardReferences.CloseButton.onClick.AddListener(OnCloseButtonPressed);
        }

        private void DestroyKeyboard()
        {
            KeyboardReferences keyboardReferences = _keyboard.GetComponent<KeyboardReferences>();
            keyboardReferences.EnterButton.onClick.RemoveListener(delegate { ToggleIngameKeyboard(false); });
            keyboardReferences.CloseButton.onClick.RemoveListener(OnCloseButtonPressed);
            if (_onValueChangedCallback != null)
            {
                keyboardReferences.InputField.onValueChanged.RemoveListener(delegate
                {
                    _onValueChangedCallback.Invoke();
                });
            }

            if (!_exitWithoutSaving)
                _inputField.text = keyboardReferences.InputField.text;
            Destroy(_keyboard);
            _keyboard = null;
            _exitWithoutSaving = false;
        }

        private void OnCloseButtonPressed()
        {
            _exitWithoutSaving = true;
            ToggleIngameKeyboard(false);
        }

        /// <summary>
        /// To select the InputField of the keyboard, we have to wait until the current selection is completed
        /// </summary>
        /// <param name="objectToFocus"></param>
        /// <returns></returns>
        private IEnumerator DelayedFocusOnObject(GameObject objectToFocus)
        {
            yield return new WaitUntil(() => !EventSystem.current.alreadySelecting);
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(objectToFocus, null);
        }

        private void ToggleAllSettingsUiObjects(bool status)
        {
            _selectableUIComponents.ForEach(selectable => selectable.interactable = status);
        }

        private bool IsInputValid(TMP_Text text)
        {
            return IsMinCharacterSizeReached(text) && IsBlankSpaceRuleObserved(text) && IsSymbolRuleObserved(text);
        }

        private bool IsMinCharacterSizeReached(TMP_Text text)
        {
            return text.text.ToCharArray().Length > _minCharacters;
        }

        private bool IsBlankSpaceRuleObserved(TMP_Text text)
        {
            char[] chars = text.text.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i] == ' '
                    && (i - 1 > 0 && chars[i - 1] == ' '
                        || i + 1 <= chars.Length && chars[i + 1] == ' '))
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsSymbolRuleObserved(TMP_Text text)
        {
            char[] chars = text.text.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (IsSymbol(chars[i])
                    && (i - 1 > 0 && IsSymbol(chars[i - 1])
                        || i + 1 <= chars.Length && IsSymbol(chars[i + 1])))
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsSymbol(char c)
        {
            return Char.IsSymbol(c) || Char.IsPunctuation(c);
        }

        private IEnumerator ShakeText(TMP_Text text, float duration)
        {
            var timer = 0f;
            var startPos = Vector3.zero;
            while (timer < duration)
            {
                timer += Time.deltaTime;

                var randomPos = startPos + (Random.insideUnitSphere * _shakeRadius);

                text.transform.localPosition = randomPos;


                yield return null;
            }

            text.transform.localPosition = startPos;
        }
    }
}