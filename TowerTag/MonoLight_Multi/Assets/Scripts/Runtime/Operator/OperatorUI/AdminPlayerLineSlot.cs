using System.Collections;
using TowerTag;
using UnityEngine;
using UnityEngine.UI;

public class AdminPlayerLineSlot : MonoBehaviour {
    [SerializeField] private DragAndDropCell _cell;
    [SerializeField] private LayoutElement _layoutElement;
    [SerializeField] private float _preferredHeight;
    [SerializeField] private float _resizeTime;
    [SerializeField] private AnimationCurve _resizeCurve;
    private bool _isVisible;
    private Coroutine _resizeCoroutine;
    public IPlayer Player => Cell.GetItem()?.GetComponent<PlayerLineController>()?.Player;
    public bool IsFree => Cell.GetItem() == null;

    public DragAndDropCell Cell => _cell;

    public void SetVisible(bool visible) {
        if (visible == _isVisible) return;
        _isVisible = visible;
        if (_resizeCoroutine != null) StopCoroutine(_resizeCoroutine);
        _resizeCoroutine = StartCoroutine(ResizeCoroutine());
    }

    private IEnumerator ResizeCoroutine() {
        float startHeight = _isVisible ? 0 : _preferredHeight;
        float endHeight = _isVisible ? _preferredHeight : 0;
        float t = 0;
        while (t <= 1) {
            t += Time.deltaTime / _resizeTime;
            _layoutElement.preferredHeight = Mathf.Lerp(startHeight, endHeight, _resizeCurve.Evaluate(t));
            yield return null;
        }

        if (!_isVisible) transform.SetSiblingIndex(transform.parent.childCount); // move empty slot to end of list
    }

    public void SetCellType(DragAndDropCell.CellType newCellType) {
        Cell.cellType = newCellType;
    }

    public void Clear() {
        Cell.RemoveItem();
        SetVisible(false);
    }

    public void Fill(GameObject item) {
        Cell.PlaceItem(item.gameObject);
        SetVisible(true);
    }
}