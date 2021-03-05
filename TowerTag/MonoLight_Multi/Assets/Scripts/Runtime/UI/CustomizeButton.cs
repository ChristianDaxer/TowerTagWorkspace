using JetBrains.Annotations;
using TMPro;
using TowerTag;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class CustomizeButton : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private Image _image;
    [SerializeField] private Image _background;
    [SerializeField] private Sprite _defaultSprite;
    [SerializeField] private Sprite _highlightedSprite;
    [SerializeField] private Material _defaultMaterial;
    [SerializeField] private Material _disabledMaterial;
    [SerializeField] private Material _pressedMaterial;

    [UsedImplicitly]
    public void SetDefault() {
        _text.color = TeamManager.Singleton.TeamIce.Colors.UI;
        _image.material = _defaultMaterial;
        _background.enabled = false;
        _image.sprite = _defaultSprite;
    }

    [UsedImplicitly]
    public void SetHighlighted() {
        _text.color = Color.black;
        _image.material = _defaultMaterial;
        _background.enabled = true;
        _background.material = _defaultMaterial;
        _image.sprite = _highlightedSprite;
    }

    [UsedImplicitly]
    public void SetDisabled() {
        _text.color = TeamManager.Singleton.TeamIce.Colors.DarkUI;
        _image.material = _disabledMaterial;
        _background.enabled = false;
        _image.sprite = _defaultSprite;
    }

    [UsedImplicitly]
    public void SetPressed() {
        _text.color = Color.black;
        _image.material = _pressedMaterial;
        _background.enabled = true;
        _background.material = _pressedMaterial;
        _image.sprite = _highlightedSprite;
    }
}