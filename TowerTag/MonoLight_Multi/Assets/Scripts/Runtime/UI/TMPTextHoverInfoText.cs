using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventTrigger))]
public class TMPTextHoverInfoText : MonoBehaviour {
    [SerializeField, TextArea] private string _infoText;
    [SerializeField] private GameObject _infoTextPrefab;
    [SerializeField] private float _hoverDelay;
    [SerializeField] private int _textSize = 18;

    [Header("Optional")] [SerializeField] private bool _useCustomValues;
    [SerializeField] private Vector2 _minCustomAnchor;
    [SerializeField] private Vector2 _maxCustomAnchor;
    [SerializeField] private Vector2 _customPivot;

    private Coroutine _infoTextCoroutine;
    private GameObject _currentInfoGameObject;
    private EventTrigger _trigger;

    private void Awake() {
        _trigger = GetComponent<EventTrigger>();
    }

    private void OnEnable() {
        EventTrigger.Entry down = new EventTrigger.Entry {eventID = EventTriggerType.PointerEnter};
        down.callback.AddListener(OnPointerEnter);
        _trigger.triggers.Add(down);
        EventTrigger.Entry up = new EventTrigger.Entry {eventID = EventTriggerType.PointerExit};
        up.callback.AddListener(OnPointerExit);
        _trigger.triggers.Add(up);
    }

    private void OnDisable() {
        if (_trigger.triggers.Count <= 0) return;
        _trigger.triggers.ForEach(entry => entry.callback.RemoveAllListeners());
        _trigger.triggers.Clear();
    }

    private void OnPointerEnter(BaseEventData data) {
        if (string.IsNullOrEmpty(_infoText)) return;

        if (_infoTextCoroutine != null)
            StopCoroutine(_infoTextCoroutine);
        _infoTextCoroutine = StartCoroutine(DelayedInfoPopUp());
    }

    private void OnPointerExit(BaseEventData data) {
        if (_infoTextCoroutine != null)
            StopCoroutine(_infoTextCoroutine);
        if (_currentInfoGameObject != null) {
            Destroy(_currentInfoGameObject);
            _currentInfoGameObject = null;
        }
    }

    private IEnumerator DelayedInfoPopUp() {
        yield return new WaitForSeconds(_hoverDelay);
        _currentInfoGameObject = InstantiateWrapper.InstantiateWithMessage(_infoTextPrefab, transform.parent);
        if (_useCustomValues) {
            RectTransform rectTransform = _currentInfoGameObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = _minCustomAnchor;
            rectTransform.anchorMax = _maxCustomAnchor;
            rectTransform.pivot = _customPivot;
            rectTransform.anchoredPosition = Vector2.zero;
        }

        TextMeshProUGUI text = _currentInfoGameObject.GetComponentInChildren<TextMeshProUGUI>();
        text.text = _infoText;
        text.fontSize = _textSize;
        _infoTextCoroutine = null;
    }
}