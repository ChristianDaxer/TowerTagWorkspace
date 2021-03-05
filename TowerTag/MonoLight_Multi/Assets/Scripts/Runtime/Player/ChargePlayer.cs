using System.Linq;
using Photon.Pun;
using TowerTag;
using UnityEngine;

/// <summary>
/// An implementation of the <see cref="Chargeable"/> interface for players.
/// A damaged player can be charged by a team mate. Charging a player will restore its health.
/// </summary>
/// <author>Ole Jürgensen (ole@vrnerds.de)</author>
[RequireComponent(typeof(PlayerHealth))]
public sealed class ChargePlayer : Chargeable {
    public IPlayer Owner => _playerHealth.Player;
    private const int MaxRopeLength = 30;


    /// <summary>
    /// The Player ID of the associated player.
    /// </summary>
    public override int ID {
        get => _playerHealth.Player.PlayerID;
        set => throw new UnityException("Cannot set ID of ChargePlayer: the player ID is used");
    }

    [SerializeField] private RopeGameAction ropeGameAction;

    /// <summary>
    /// Player(s) they finished the Charging-Process on the IChargeable
    /// </summary>

    public override ChargeableType ChargeableType => ChargeableType.Player;

    protected override float TimeToCharge =>
        CurrentCharge.value <= 0.5f ? _timeToCharge / 2f : _timeToCharge;

    private PlayerHealth _playerHealth;


    private new void Awake() {
        base.Awake();

        _playerHealth = GetComponent<PlayerHealth>();

        if (PlayerRigBase.GetInstance(out var playerRig) && playerRig.TryGetPlayerRigTransform(PlayerRigTransformOptions.Head, out var head)) {
            HealthVignette vignette = head.GetComponent<HealthVignette>();

            if (vignette) {
                vignette.InitHealthVignette(this);
            }
        }
    }

    public override bool CanAttach(IPlayer player) {
        return CanTryToAttach(player) && AttachedPlayers.Count == 0; // only one healer allowed;
    }

    public override bool CanTryToAttach(IPlayer player) {
        return CanCharge(player);
    }

    public override bool CanCharge(IPlayer player) {
        return player != null
               && player.PlayerID != _playerHealth.Player.PlayerID // no self heal
               && player.TeamID == _playerHealth.Player.TeamID  // no enemy heal
               && _playerHealth.IsAlive
               && player.IsAlive // no revive
               && !_playerHealth.IsAtFullHealth // no healing by dead players
               && IsPlayerInRange(player); // cannot heal players at full health
    }

    private new void OnEnable() {
        _playerHealth.HealthChanged += OnHealthChanged;
        _playerHealth.PlayerDied += OnPlayerDied;
        ChargeSet += OnChargeSet;
    }

    private new void OnDisable() {
        _playerHealth.HealthChanged -= OnHealthChanged;
        _playerHealth.PlayerDied -= OnPlayerDied;
        ChargeSet -= OnChargeSet;
    }

    private void OnHealthChanged(PlayerHealth playerHealth, int newHealth, IPlayer other, byte colliderType) {
        if (_playerHealth == null || _playerHealth.Player == null)
            return;

        CurrentCharge = (_playerHealth.Player.TeamID, _playerHealth.HealthFraction);
        if (_playerHealth.IsAtFullHealth) {
            foreach (IPlayer attachedPlayer in AttachedPlayers.ToArray()) {
                if (!attachedPlayer.IsMe)
                    continue;
                GunController gunController = attachedPlayer.GunController;
                if (gunController != null)
                    gunController.RequestRopeDisconnect(true);
            }
        }
    }

    private void OnPlayerDied(PlayerHealth playerHealth, IPlayer enemyWhoAppliedDamage, byte colliderType) {
        foreach (IPlayer attachedPlayer in AttachedPlayers.ToArray()) {
            if (!attachedPlayer.IsMe)
                continue;
            GunController gunController = attachedPlayer.GunController;
            if (gunController != null)
                gunController.RequestRopeDisconnect(false);
        }
    }

    private void OnChargeSet(Chargeable chargeable, TeamID team, float value) {
        if (!PhotonNetwork.IsMasterClient)
            return;
        int healingAmount = Mathf.RoundToInt(value * _playerHealth.MaxHealth) - _playerHealth.CurrentHealth;
        if (healingAmount <= 0)
            return;

        IPlayer healingPlayer = AttachedPlayers.FirstOrDefault();

        if (healingPlayer == null || healingPlayer.GameObject == null || Owner.GameObject == null) {
            Debug.LogWarning($"Cannot process healing of {this}, because no player is charging");
            return;
        }

        if (!IsPlayerInRange(healingPlayer)) {
            ropeGameAction.DisconnectRope(this, healingPlayer);
            return;
        }

        if (!_playerHealth.IsAlive) {
            Debug.LogWarning("Cannot heal dead player");
            return;
        }

        if (_playerHealth.Player.TeamID != healingPlayer.TeamID) {
            Debug.LogWarning("Cannot heal enemy player");
            return;
        }

        _playerHealth.AddHealth(healingAmount, healingPlayer);
    }

    private bool IsPlayerInRange(IPlayer healingPlayer) {
        if (healingPlayer == null || healingPlayer.GameObject == null || Owner.GameObject == null) {
            Debug.LogWarning($"Cannot process healing of {this}, because Owner or healing player not found");
            return false;
        }

        return Vector3.Distance(healingPlayer.GameObject.transform.position,
                                Owner.GameObject.transform.position) < MaxRopeLength;
    }
}