using UnityEngine;

public class PillarColliderSizeInMatch : MonoBehaviour {
    [Header("IMPORTANT: original Collider rad has to be 2 for neighbour detection!")] [SerializeField]
    private SphereCollider _collider;

    [SerializeField, Tooltip("The ingame claim radius is defined here")]
    private float _radiusToClaim;

    // Start is called before the first frame update
    void Start() {
        _collider.radius = _radiusToClaim;
    }
}