using SOEventSystem.Shared;
using TowerTag;
using UnityEngine;
// ReSharper disable UnusedAutoPropertyAccessor.Global

[CreateAssetMenu(menuName = "Shared/TowerTag/Player Health Change")]
public class SharedPlayerHealthChange : SharedVariable<HealthChange> { }

public class HealthChange {
    public float HealthChangeAmount { get; }
    public float NewPlayerHealth { get; }
    public IPlayer AffectedPlayer { get; }
    public IPlayer InducedBy { get; }
    public DamageDetectorBase.ColliderType ColliderType { get; }

    public HealthChange(
        float healthChangeAmount,
        float newPlayerHealth,
        IPlayer affectedPlayer,
        IPlayer inducedBy = null,
        DamageDetectorBase.ColliderType colliderType = DamageDetectorBase.ColliderType.Undefined) {
        HealthChangeAmount = healthChangeAmount;
        NewPlayerHealth = newPlayerHealth;
        AffectedPlayer = affectedPlayer;
        InducedBy = inducedBy;
        ColliderType = colliderType;
    }
}