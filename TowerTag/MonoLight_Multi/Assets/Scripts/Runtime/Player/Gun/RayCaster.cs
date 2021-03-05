using TowerTag;
using UnityEngine;

public sealed class RayCaster : MonoBehaviour {
    private IPlayer _owner;

    public delegate void HighlightChangeDelegate(Highlighter highlighter, bool showHighlight);

    public event HighlightChangeDelegate HighlightChanged;

    [SerializeField] private Transform _raycastTransform;
    [SerializeField] private float _rayLength;
    [SerializeField] private LayerMask _raycastLayerMask;

    private RaycastHit _hitInfo;
    private Highlighter _lastHighlighter;
    private Chargeable _lastChargeable;

    private GameObject LastTarget { get; set; }

    private RaycastHit[] hits = new RaycastHit[1];
    int rayHits;
    public void Init(IPlayer player) {
        _owner = player;
    }

    private void Start() {
        _rayLength = BalancingConfiguration.Singleton.ChargerBeamLength;
    }

    public Chargeable DoRaycast() {
        if (_raycastTransform == null) {
            Debug.LogWarning("Raycaster: Raycast source transform not set!");
            return null;
        }

        
        rayHits = Physics.RaycastNonAlloc(_raycastTransform.position, _raycastTransform.forward, hits, _rayLength, _raycastLayerMask);

        //bool hit = Physics.Ray(
        //    _raycastTransform.position,
        //    _raycastTransform.forward,
        //    out _hitInfo,
        //    _rayLength,
        //    _raycastLayerMask);
        //return EvaluateHit(hit ? _hitInfo.collider.gameObject : null);

        return EvaluateHit(rayHits > 0 ? hits[0].collider.gameObject : null);
    }

    public void Reset() {
        EvaluateHit(null);
    }

    private Chargeable EvaluateHit(GameObject target) {
        ProcessHighlighting(target);
        Chargeable chargeable = ProcessChargeable(target);
        LastTarget = target;
        return chargeable;
    }

    private void ProcessHighlighting(GameObject target) {
        if (target == LastTarget || _owner == null) return;

        Highlighter highlighter = target == null ? null : target.GetComponent<Highlighter>();

        if (highlighter != _lastHighlighter) {
            if (_lastHighlighter != null) {
                _lastHighlighter.ShowHighlight(false, _owner);
                HighlightChanged?.Invoke(_lastHighlighter, false);
            }

            if (highlighter != null) {
                highlighter.ShowHighlight(true, _owner);
                HighlightChanged?.Invoke(highlighter, highlighter.IsAllowedToHighlight(_owner));
            }
        }

        _lastHighlighter = highlighter;
    }

    private Chargeable ProcessChargeable(GameObject target) {
        if (target == LastTarget) return _lastChargeable;
        _lastChargeable = target == null ? null : target.GetComponent<ChargeableCollider>()?.Chargeable;
        return _lastChargeable;
    }
}