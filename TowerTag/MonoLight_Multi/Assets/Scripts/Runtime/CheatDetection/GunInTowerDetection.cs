using TowerTag;
using TowerTagSOES;
using UnityEngine;

public class GunInTowerDetection : MonoBehaviour {
    [SerializeField] private Vector3 _center;
    [SerializeField] private Vector3 _boxSize;
    [SerializeField] private IPlayer _player;

    [SerializeField,
     Tooltip("Layers used for collision calculations to determine, " +
             "if player looks through objects or has left the chaperone.")]
    private LayerMask _clippingObjectsLayerMask;

    private void Awake() {
        _player = GetComponentInParent<Player>();
        if (!SharedControllerType.IsPlayer || _player == null) {
            enabled = false;
        }
    }

    private void Start() {
        Vector3 localScale = transform.localScale;
        _boxSize = Vector3.Scale(_boxSize, localScale);
        _center = Vector3.Scale(_center, localScale);
    }

    /// <summary>
    /// Check for collisions and activate/deactivate effects if needed.
    /// </summary>
    private void FixedUpdate() {
        Transform thisTransform;
        bool currentlyColliding = Physics.CheckBox(
            transform.position + (thisTransform = transform).TransformDirection(_center), _boxSize / 2,
            thisTransform.rotation, _clippingObjectsLayerMask,
            QueryTriggerInteraction.Collide);

        if (currentlyColliding != _player.IsGunInTower) {
            _player.IsGunInTower = currentlyColliding;
        }
    }
}