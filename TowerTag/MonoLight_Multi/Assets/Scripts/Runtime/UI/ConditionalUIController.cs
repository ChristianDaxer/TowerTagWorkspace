using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ConditionalUIController : MonoBehaviour {
    [SerializeField, Tooltip("These elements will be active when the toggle is on")]
    private GameObject[] _ifOnElements;

    [SerializeField, Tooltip("These elements will be active when the toggle is off")]
    private GameObject[] _ifOffElements;

    private Toggle _toggle;

    private void Start() {
        _toggle = GetComponent<Toggle>();
        ToggleElements(_toggle.isOn);
        _toggle.onValueChanged.AddListener(ToggleElements);
    }

    private void ToggleElements(bool isOn) {
        foreach (GameObject conditionalElement in _ifOnElements) {
            conditionalElement.SetActive(isOn);
        }

        foreach (GameObject conditionalElement in _ifOffElements) {
            conditionalElement.SetActive(!isOn);
        }
    }

    private void OnDestroy() {
        if (_toggle != null) {
            _toggle.onValueChanged?.RemoveListener(ToggleElements);
        }
    }
}