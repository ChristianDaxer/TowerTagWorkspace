using System;
using TowerTag;

public interface IChargerBeamRenderer {
    event Action TeleportTriggered;
    event Action RollingOut;
    event Action RolledOut;
    event Action RollingIn;
    event Action<float> TensionValueChanged;

    // property to check the tension of the beam (Rope) [0..1]
    float Tension{ get;}

    // connects beam between chargerSpawnAnchor (Gun) and targetAnchor (Pillar)
    void Connect(Chargeable target);

    // updates the charge value while charging
    void UpdateChargeValue(float currentCharge);

    // disconnects (disables) beam
    void Disconnect();

    // set if this rope is on a local player instance (or on a remote client)
    void SetOwner(IPlayer owner);

    // receive OnTeamChanged event to change ropeColors
    void OnTeamChanged(IPlayer player, TeamID teamID);
}
