using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Home.UI
{
    public static class InputFieldHelper
    {
        public enum InputFieldType
        {
            PlayerName,
            Pin
        }

        public static readonly Dictionary<InputFieldType, GameObject> TypeToGameObject =
            new Dictionary<InputFieldType, GameObject>
            {
                {
                    InputFieldType.PlayerName,
                    Resources.Load<GameObject>("TT_Keyboard")
                },
                {
                    InputFieldType.Pin,
                    Resources.Load<GameObject>("TT_Numpad")

                }
            };

        public static void SetInputFieldSettings(TMP_InputField inputField, InputFieldType type)
        {
            switch (type)
            {
                case InputFieldType.PlayerName:
                    ApplyPlayerNameSettings(inputField);
                    break;
                case InputFieldType.Pin:
                    ApplyPinSettings(inputField);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private static void ApplyPinSettings(TMP_InputField inputField)
        {
            inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
            inputField.lineType = TMP_InputField.LineType.SingleLine;
            inputField.characterLimit = 4;
            var ingameKeyboard = inputField.GetComponent<IngameKeyboard>();
            if(ingameKeyboard != null)
                ingameKeyboard.InputFieldType = InputFieldType.Pin;
        }

        private static void ApplyPlayerNameSettings(TMP_InputField inputField)
        {
            inputField.contentType = TMP_InputField.ContentType.Alphanumeric;
            inputField.lineType = TMP_InputField.LineType.SingleLine;
            inputField.characterLimit = BitCompressionConstants.PlayerNameMaxLength;
            var ingameKeyboard = inputField.GetComponent<IngameKeyboard>();
            if(ingameKeyboard != null)
                ingameKeyboard.InputFieldType = InputFieldType.PlayerName;
        }
    }
}