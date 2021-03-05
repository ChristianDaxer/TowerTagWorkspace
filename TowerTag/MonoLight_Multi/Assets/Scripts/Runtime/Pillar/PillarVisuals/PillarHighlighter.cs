using TowerTag;
using UnityEngine;
using UnityEngine.Serialization;

public class PillarHighlighter : Highlighter {
    [SerializeField] private ChargeableCollider _owner;

    [FormerlySerializedAs("visualParent")] [SerializeField]
    private GameObject _visualParent;

    private void Start() {
        _visualParent.SetActive(false);
    }

    protected override void ChangeHighlight(bool highlight, IPlayer highlightRequester) {
        if (highlight && _owner.Chargeable.CanAttach(highlightRequester)) {
            _visualParent.SetActive(true);

            Toggle(true);
        }
        else {
            _visualParent.SetActive(false);
            Toggle(false);
        }
    }

    public override bool IsAllowedToHighlight(IPlayer highlightRequester) {
        return _owner.Chargeable.CanAttach(highlightRequester);
    }

    private void OnDestroy() {
        _owner = null;
        _visualParent = null;
    }
}