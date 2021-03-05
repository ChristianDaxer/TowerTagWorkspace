using UnityEngine;

public class InBulletTrail : MonoBehaviour {
    [SerializeField] private LayerMask _playerLayerMask;

    [SerializeField] private float _maxRadius = 2f;
    [SerializeField] private float _minRadius = 1f;
    private bool _aimingOnPlayer;

    private void Awake()
    {
        enabled = ConfigurationManager.Configuration.SingleButtonControl;
    }

    private void Update() {
        const float radius = 2f;
        if (Physics.SphereCast(transform.position, radius, transform.forward, out RaycastHit _, Mathf.Infinity, _playerLayerMask)) {
            if (_aimingOnPlayer) return;
            PillarManager.Instance.GetAllPillars().ForEach(pillar => {
                var sphereCollider = pillar.AnchorTransform.GetComponent<SphereCollider>();
                if (sphereCollider != null)
                    sphereCollider.radius = _minRadius;
            });
            _aimingOnPlayer = true;
        }
        else {
            if (!_aimingOnPlayer) return;
            PillarManager.Instance.GetAllPillars().ForEach(pillar => {
                var sphereCollider = pillar.AnchorTransform.GetComponent<SphereCollider>();
                if (sphereCollider != null)
                    sphereCollider.radius = _maxRadius;
            });
            _aimingOnPlayer = false;
        }
    }
}
