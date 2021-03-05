using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleAnimationHandler : MonoBehaviour
{
    [SerializeField] private Toggle _toggle;
    [SerializeField] private Text _mainText;
    [SerializeField] private Text[] _textsToColorize;
    [SerializeField] private MeshRenderer _frame;
    [SerializeField] private Material _textMaterial;
    [SerializeField] private Material _enabledImageMaterial;
    [SerializeField] private Material _disabledImageMaterial;
    [SerializeField] private string _isOffText;
    [SerializeField] private string _isOnText;

    private bool _interactable = true;

    private void Awake() {
        _mainText.text = _toggle.isOn ? _isOnText : _isOffText;

    }

    [UsedImplicitly]
    public void OnToggleTriggered(bool newValue) {
        _mainText.text = newValue ? _isOnText : _isOffText;
        _mainText.material = newValue ? null : _textMaterial;
        if (_textsToColorize.Length >= 0) {
            _textsToColorize.ForEach(text => text.material = newValue ? null : _textMaterial);
        }
    }

    public void OnInteractableStateChanged() {
        var interactable = _toggle.interactable;
        if(interactable != _interactable) {
            _frame.material = interactable ? _enabledImageMaterial : _disabledImageMaterial;
            _mainText.material = interactable ? _textMaterial : null;
            if (_textsToColorize.Length >= 0) {
                _textsToColorize.ForEach(text => text.material = interactable ? _textMaterial : null);
            }

            _interactable = interactable;
        }
    }

}
