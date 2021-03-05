using TowerTag;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ChargeableCollider : MonoBehaviour {
    [SerializeField]
    private Collider _collider;
    public Chargeable Chargeable { get; set; }

    public Collider Collider => _collider;
}