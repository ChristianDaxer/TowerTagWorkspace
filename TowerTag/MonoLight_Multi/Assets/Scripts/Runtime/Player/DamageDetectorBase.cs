using System;
using TowerTag;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable, RequireComponent(typeof(Collider))]
public class DamageDetectorBase : MonoBehaviour {
    [SerializeField] private ColliderType _detectorType;
    [FormerlySerializedAs("_colliders")] [SerializeField] private Collider[] _collider;
    public ColliderType DetectorType => _detectorType;
    public IPlayer Player { get; private set; }

    public Collider[] Collider {
        get => _collider;
        private set => _collider = value;
    }

    public enum ColliderType {
        Undefined = 0,
        Head = 1,
        Body = 2,
        Weapon = 3
    }

    public virtual void Init(IPlayer player) {
        Player = player;
        Collider = GetComponents<Collider>();
    }
}