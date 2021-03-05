using System.Collections;
using JetBrains.Annotations;
using UI;
using UnityEngine;

public class MiniMapController : MonoBehaviour {
    [SerializeField, Tooltip("The parent transform of the minimap")]
    private RectTransform _miniMap;

    [SerializeField, Tooltip("Script which toggle the spectator and operator UI")]
    private ToggleOperatorUI _toggleUI;

    //Lerp values
    private Vector2 _startScale;
    private Vector2 _maximizedScale;
    private Vector2 _openPosition;
    private Vector2 _closedPosition;
    private Vector2 _hiddenPosition;
    private bool _open = true;
    private bool _maximized;
    private const float XPositionClosed = -477.9f;
    private const float LerpValue = 0.2f;
    private float _lerpValueToggle;
    private float _lerpValueHide;

    private Coroutine _minimapSizeCoru;
    private Coroutine _minimapToggleCoru;

    private void Start() {
        _openPosition = _miniMap.anchoredPosition;
        _closedPosition = new Vector2(XPositionClosed, _openPosition.y);
        _startScale = _miniMap.localScale;
        _hiddenPosition = new Vector2(_openPosition.x - _miniMap.sizeDelta.x * _startScale.x, _openPosition.y);
        _maximizedScale = Vector2.one;
    }

    private void OnEnable() {
        _toggleUI.SpectatorUIToggled += OnSpectatorUIToggled;
    }

    private void OnSpectatorUIToggled(bool value) {
        if (_maximized)
            ToggleMiniMapScaling();
        if (_minimapToggleCoru != null)
            StopCoroutine(_minimapToggleCoru);
        _minimapToggleCoru = StartCoroutine(TranslateMinimapOnSpectatorUIToggle(value));
    }

    //used on the flap of the minimap button
    [UsedImplicitly]
    public void ToggleMiniMap() {
        if (_maximized)
            ToggleMiniMapScaling();
        if (_minimapToggleCoru != null)
            StopCoroutine(_minimapToggleCoru);
        _minimapToggleCoru = StartCoroutine(TranslateMinimap());
    }

    [UsedImplicitly]
    public void ToggleMiniMapScaling() {
        if (_minimapSizeCoru != null)
            StopCoroutine(_minimapSizeCoru);

        _minimapSizeCoru = StartCoroutine(ScaleMiniMap());
    }

    /// <summary>
    /// Hides the mini map completely
    /// </summary>
    /// <param name="setActive">Spectator view toggle</param>
    /// <returns></returns>
    private IEnumerator TranslateMinimapOnSpectatorUIToggle(bool setActive) {
        Vector2 statusPosition = _open ? _openPosition : _closedPosition;
        if (!setActive) {
            while (Vector2.Distance(_miniMap.anchoredPosition, _hiddenPosition) > 0.1f) {
                _miniMap.anchoredPosition = Vector2.Lerp(_miniMap.anchoredPosition, _hiddenPosition, LerpValue);
                yield return null;
            }

            _miniMap.anchoredPosition = _hiddenPosition;
        } else {
            while (Vector2.Distance(_miniMap.anchoredPosition, statusPosition) > 0.1f) {
                _miniMap.anchoredPosition = Vector2.Lerp(_miniMap.anchoredPosition, statusPosition, LerpValue);
                yield return null;
            }

            _miniMap.anchoredPosition = statusPosition;
        }
        _minimapToggleCoru = null;
    }

    /// <summary>
    /// On hidden status the flap is still visible
    /// </summary>
    /// <returns></returns>
    private IEnumerator TranslateMinimap() {
        bool currentStatus = _open;
        _open = !_open;
        if (currentStatus) {
            while (Vector2.Distance(_miniMap.anchoredPosition, _closedPosition) > 0.1f) {
                _miniMap.anchoredPosition = Vector2.Lerp(_miniMap.anchoredPosition, _closedPosition, LerpValue);
                yield return null;
            }

            _miniMap.anchoredPosition = _closedPosition;
        } else {
            while (Vector2.Distance(_miniMap.anchoredPosition, _openPosition) > 0.1f) {
                _miniMap.anchoredPosition = Vector2.Lerp(_miniMap.anchoredPosition, _openPosition, LerpValue);
                yield return null;
            }

            _miniMap.anchoredPosition = _openPosition;
        }

        _minimapToggleCoru = null;
    }

    /// <summary>
    /// Changes the scale of the minimap
    /// </summary>
    /// <returns></returns>
    private IEnumerator ScaleMiniMap() {
        bool currentStatus = _maximized;
        _maximized = !_maximized;
        if (!currentStatus) {
            while (Vector2.Distance(_miniMap.localScale, _maximizedScale) > 0.1f) {
                _miniMap.localScale = Vector2.Lerp(_miniMap.localScale, _maximizedScale, LerpValue);
                yield return null;
            }

            _miniMap.localScale = _maximizedScale;
        } else {
            while (Vector2.Distance(_miniMap.localScale, _startScale) > 0.1f) {
                _miniMap.localScale = Vector2.Lerp(_miniMap.localScale, _startScale, LerpValue);
                yield return null;
            }

            _miniMap.localScale = _startScale;
        }

        _minimapSizeCoru = null;
    }
}
