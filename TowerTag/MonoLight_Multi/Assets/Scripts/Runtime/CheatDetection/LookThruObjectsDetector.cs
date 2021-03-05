using TowerTag;
using TowerTagSOES;
using UnityEngine;

/// <summary>
/// Check if the players head collides with objects or chaperone bounds to prevent cheating.
/// This should prevent that the user can go outside of the chaperone bounds or is hiding in objects (like the pillar).
/// </summary>
public class LookThruObjectsDetector : MonoBehaviour {
    [SerializeField] private float _headRadius;

    [SerializeField,
     Tooltip("Layers used for collision calculations to determine if player looks through objects.")]
    private LayerMask _clippingObjectsLayerMask;

    private IPlayer _player;

    private void Start() {
        if (!SharedControllerType.IsPlayer) {
            enabled = false;
            return;
        }

        _player = GetComponentInParent<IPlayer>();
    }

    private void OnDisable() {
        if(_player != null)
            _player.IsInTower = false;
    }

    /// <summary>
    /// Check for collisions and activate/deactivate effects if needed.
    /// </summary>
    private void Update() {
        bool colliding = Physics.CheckSphere(transform.position, _headRadius, _clippingObjectsLayerMask,
            QueryTriggerInteraction.Collide);
        if(_player != null)
            _player.IsInTower = colliding;
    }
}