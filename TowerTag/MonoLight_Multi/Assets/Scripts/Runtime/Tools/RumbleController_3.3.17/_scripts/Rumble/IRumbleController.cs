public interface IRumbleController {
    // OneShots
    void TriggerShootProjectile();
    void TriggerShootProjectileEmpty();
    void TriggerShootChargerBeam();
    void TriggerPlayerWasHit();

    // toggle on/off (enable: on, !enable: off)
    void ToggleShootProjectile(bool enable);
    void ToggleChargerBeamLaser(bool enable);
    void ToggleCharge(bool enable);
    void ToggleHealPlayer(bool enable);
    void ToggleHighlightPillar(bool enable);
}