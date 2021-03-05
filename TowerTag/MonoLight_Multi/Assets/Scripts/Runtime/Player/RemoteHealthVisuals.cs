using TowerTag;
using UnityEngine;

public class RemoteHealthVisuals : MonoBehaviour, IHealthVisuals {
    [SerializeField] private FloatVisuals _healthVisual;
    [SerializeField] private string _explodingAvatarEffectDatabaseName;
    [SerializeField] private GameObject _gunParent;

    public AvatarVisuals GhostVisuals { private get; set; }

    public void OnHealthChanged(PlayerHealth sender, int newValue, IPlayer other, byte colliderType) {
        _healthVisual.SetValue(sender.HealthFraction);
    }

    public void OnTookDamage(PlayerHealth playerHealth, IPlayer other) { }

    public void OnPlayerDied(PlayerHealth playerHealth, IPlayer other, byte colliderType) {

        if (playerHealth == null) {
            Debug.LogError("Cannot visualize remote player health change: PlayerHealth is null");
            return;
        }

        // create rag doll decal
        ITeam ownTeam = TeamManager.Singleton.Get(playerHealth.Player.TeamID);

        if (ownTeam == null) {
            Debug.LogError($"Cannot visualize remote player health change: No team with id {playerHealth.Player.TeamID}");
            return;
        }

        Transform healthTransform = playerHealth.transform;
        GameObject ragDoll = EffectDatabase.PlaceDecal(
            _explodingAvatarEffectDatabaseName, healthTransform.position, healthTransform.rotation);

        if (ragDoll == null) {
            Debug.LogError("Failed to place decal: rag doll is null");
            return;
        }

        var avatar = ragDoll.GetComponent<ExplodingAvatar>();
        var playerAnchors = playerHealth.Player.PlayerAvatar.PlayerAvatarParent.GetComponentInChildren<AvatarAnchor>();

        if (avatar == null) {
            Debug.LogError("Failed to visualize player death: avatar is null");
            return;
        }

        if (playerAnchors == null) {
            Debug.LogError("Failed to visualize player death: player anchors is null");
            return;
        }

        //set color for explosion effect (exploding Players team color)
        ColorChanger.ChangeColorInChildRendererComponents(avatar._explosionEffectParent, ownTeam.Colors.Effect);
        ITeam opponentTeam = TeamManager.Singleton.Get(ownTeam.ID == TeamID.Fire ? TeamID.Ice : TeamID.Fire);
        ColorChanger.ChangeColorInChildLightComponents(avatar._explosionEffectParent, opponentTeam.Colors.ContrastLights);

        // trigger (rigid body) explosion
        avatar.InitAvatar(playerAnchors.HeadTransform, playerAnchors.BodyTransform, playerHealth.Player.TeamID);
        avatar.Explode();

        if (GhostVisuals != null)
            GhostVisuals.OnSetActive(false, _gunParent);
    }

    public void OnSetActive(PlayerStateHandler sender, PlayerState newPlayerState) {
        if (GhostVisuals != null)
        {
            GhostVisuals.OnSetActive(!newPlayerState.IsImmortal, _gunParent);
        }
    }
}