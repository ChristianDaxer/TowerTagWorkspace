using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class TextHoverInfoText : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField, TextArea] private string _infoText;
    [SerializeField] private GameObject _infoTextPrefab;
    [SerializeField] private float _hoverDelay = 1;
    private Coroutine _infoTextCoroutine;
    private GameObject _currentInfoGameObject;

    public void OnPointerEnter(PointerEventData eventData) {
        if (string.IsNullOrEmpty(_infoText)) return;

        if (_infoTextCoroutine != null)
            StopCoroutine(_infoTextCoroutine);
        _infoTextCoroutine = StartCoroutine(DelayedInfoPopUp());
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (_infoTextCoroutine != null)
            StopCoroutine(_infoTextCoroutine);
        if (_currentInfoGameObject != null) {
            Destroy(_currentInfoGameObject);
            _currentInfoGameObject = null;
        }
    }

    private IEnumerator DelayedInfoPopUp() {
        yield return new WaitForSeconds(_hoverDelay);
        _currentInfoGameObject = InstantiateWrapper.InstantiateWithMessage(_infoTextPrefab, transform);
        _currentInfoGameObject.transform.localScale = new Vector3(1.5f,1.5f,1.5f);
        _currentInfoGameObject.GetComponentInChildren<TextMeshProUGUI>().text = _infoText;
        _infoTextCoroutine = null;
    }
}