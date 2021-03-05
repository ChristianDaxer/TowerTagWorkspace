using TowerTag;
using UnityEngine;

[RequireComponent(typeof(ChargeableCollider))]
public class ChargeableHighlightTrigger : Highlighter {
    [SerializeField] private ChargeableCollider _owner;

#if UNITY_EDITOR
    private void OnValidate() => _owner = GetComponent<ChargeableCollider>();
#endif

    protected override void ChangeHighlight(bool highlight, IPlayer highlightRequester) {
        if (highlight && IsAllowedToHighlight(highlightRequester)) {
            Toggle(true);
        } else {
            Toggle(false);
        }
    }

    public override bool IsAllowedToHighlight(IPlayer highlightRequester) {
        if (_owner != null && _owner.Chargeable != null)
            return _owner.Chargeable.CanAttach(highlightRequester);
        Debug.LogError("ChargeableHighlightTrigger.IsAllowedToHighlight: owner not set!");
        return false;
    }

    private void OnDestroy() {
        _owner = null;
    }
}
