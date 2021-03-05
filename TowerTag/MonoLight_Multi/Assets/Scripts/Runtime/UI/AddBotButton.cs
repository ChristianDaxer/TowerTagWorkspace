using System.Collections;
using TowerTag;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class AddBotButton : MonoBehaviour {
    [FormerlySerializedAs("_endPos")] [SerializeField] private Vector2 _closedPos;
    [SerializeField] private float _lerpDuration;
    [SerializeField] private TeamID _teamID;

    private Button _button;
    private RectTransform _rectTransform;
    private Vector2 _openPos;
    private Coroutine _coroutine;
    private bool _open;

    private void Start() {
        _rectTransform = gameObject.GetComponent<RectTransform>();
        _openPos = _rectTransform.anchoredPosition;
        _button = gameObject.GetComponent<Button>();
        _open = true;
    }

    public void Open() {
        if (_open) return;
        if(_coroutine != null) StopCoroutine(_coroutine);
        _open = true;
        _coroutine = StartCoroutine(LerpAddBotButton());
    }

    public void Close() {
        if (!_open) return;
        if(_coroutine != null) StopCoroutine(_coroutine);
        _open = false;
        _coroutine = StartCoroutine(LerpAddBotButton());
    }

    private IEnumerator LerpAddBotButton() {
        float timer = 0;
        Vector2 start = _rectTransform.anchoredPosition;
        Vector2 end = _open ? _openPos : _closedPos;

        while (timer <= _lerpDuration) {
            timer += Time.deltaTime;
            _rectTransform.anchoredPosition = Vector2.Lerp(start, end, timer/ _lerpDuration);
            yield return null;
        }

        _rectTransform.anchoredPosition = end;
        _button.interactable = _open;
        _coroutine = null;
    }

    public void AddBot() {
        BotManager.Instance.CheckForNull()?.AddBot(_teamID);
    }
}
