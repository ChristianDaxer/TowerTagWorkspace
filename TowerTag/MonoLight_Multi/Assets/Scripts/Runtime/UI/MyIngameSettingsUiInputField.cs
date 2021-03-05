using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 
/// </summary>
[RequireComponent(typeof(InputField))]
public class MyIngameSettingsUiInputField : InputField {
    public delegate void InputFieldAction(object sender);

    public event InputFieldAction InputFieldSelect;

    public override void OnSelect(BaseEventData eventData) {
        base.OnSelect(eventData);
        InputFieldSelect?.Invoke(this);
    }
}