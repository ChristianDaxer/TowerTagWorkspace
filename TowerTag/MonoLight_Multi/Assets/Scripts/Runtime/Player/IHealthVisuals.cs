using TowerTag;

public interface IHealthVisuals
{
    void OnHealthChanged(PlayerHealth sender, int newValue, IPlayer other, byte colliderType);

    // only triggered locally (on local client for local damageModel)
    void OnTookDamage(PlayerHealth playerHealth, IPlayer other);

    void OnPlayerDied(PlayerHealth playerHealth, IPlayer other, byte colliderType);
}
