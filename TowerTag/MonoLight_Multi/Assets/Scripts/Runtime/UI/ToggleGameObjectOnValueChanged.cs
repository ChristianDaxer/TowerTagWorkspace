using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleGameObjectOnValueChanged : MonoBehaviour {
    [SerializeField, Tooltip("The Panel that gets toggle with the toggle button on value change")]
    private GameObject _panel;

    [SerializeField, Tooltip("Background Sprite of the Toggle when isOn. Can be empty if you don't want changes")]
    private Sprite _isOnBackground;

    [SerializeField, Tooltip("Background Sprite of the Toggle when !isOn. Can be empty if you don't want changes")]
    private Sprite _isOffBackground;

    private Toggle _toggle;
    private Image _background;


    private void Awake() {
        _toggle = GetComponent<Toggle>();
        _background = (Image) _toggle.targetGraphic;
    }

    private void OnEnable() {
        _toggle.onValueChanged.AddListener(OnValueChanged);
        _toggle.onValueChanged?.Invoke(_toggle.isOn);
    }

    private void OnValueChanged(bool value) {
        _panel.SetActive(value);
        _background.sprite = value ? _isOnBackground : _isOffBackground;
    }
}