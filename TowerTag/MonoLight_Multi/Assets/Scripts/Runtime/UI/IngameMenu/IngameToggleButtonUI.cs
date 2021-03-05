using System;
using TowerTag;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class IngameToggleButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Toggle _toggle;
    [SerializeField] private Image _background;
    [SerializeField] private Text _label;
    [SerializeField] private Sprite _activeButton;
    [SerializeField] private Sprite _inactiveButton;
    [SerializeField] private Sprite _highlightButton;
    [SerializeField] private Material _defaultMaterial;
    [SerializeField] private Material _highlightMaterial;


    private void Awake()
    {
        _toggle = GetComponent<Toggle>();
    }

    private void OnEnable()
    {
        _toggle.onValueChanged.AddListener(OnValueChanged);
        OnValueChanged(_toggle.isOn);
    }

    private void OnDisable()
    {
        _toggle.onValueChanged.RemoveListener(OnValueChanged);
    }

    private void OnValueChanged(bool value)
    {
        _background.sprite = value ? _activeButton : _inactiveButton;
        _label.color = value ? Color.black : TeamManager.Singleton.TeamIce.Colors.UI;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _background.sprite = _highlightButton;
        _label.color = Color.black;
        _background.material = _highlightMaterial;

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _background.sprite = _toggle.isOn ? _activeButton : _inactiveButton;
        _label.color = _toggle.isOn ? Color.black : TeamManager.Singleton.TeamIce.Colors.UI;
        _background.material = _defaultMaterial;

    }
}
