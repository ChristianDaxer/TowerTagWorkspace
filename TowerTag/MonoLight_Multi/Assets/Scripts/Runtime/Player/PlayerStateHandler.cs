using System;
using JetBrains.Annotations;
using SOEventSystem.Shared;
using TowerTag;
using UnityEngine;

// todo: make MonoBehaviour, reference shared variables etc. directly, remove all state handling from player script
public class PlayerStateHandler : IDisposable {
    #region local Member

    private IPlayer _owner;
    private PlayerHealth _playerHealth;
    private GunController _gunController;

    /// <summary>
    /// Coroutine to reactivate player state after timeout (called only on Master)
    /// </summary>
    private Coroutine _currentReanimation;

    /// <summary>
    /// PlayerState we use when MatchTimer is paused. Is switched on every locally on clients (dependent of local MatchTimer state).
    /// </summary>
    private readonly PlayerState _playerStatePaused = PlayerState.Dead;

    private SharedBool _gamePaused;

    public delegate void PlayerStateEventHandler(PlayerStateHandler sender, PlayerState newState);

    public event PlayerStateEventHandler PlayerStateChanged;

    /// <summary>
    /// Convenience PlayerState property. Returns playerStatePaused if local MatchTimer is paused
    /// or the normal playerState (synchronized from Master) otherwise.
    /// </summary>
    public PlayerState PlayerState {
        get {
            // if we are in Paused State -> return paused PlayerState
            MatchTimer mT = GameManager.Instance.MatchTimer;
            if (mT != null && (mT.IsPaused || mT.IsResumingFromPause))
                return _playerStatePaused;

            // otherwise return normal playerState
            return _playerState;
        }
    }

    #endregion

    #region synced Member

    /// <summary>
    /// normal playerState (synchronized from Master)
    /// </summary>
    private PlayerState _playerState;


    /// <summary>
    /// has Player state changed sync we last synced to clients? (only valid on master client).
    /// </summary>
    public bool PlayerStateChangedSinceLastSync { get; private set; }

    #endregion

    #region local

    public void Init(
        [NotNull] IPlayer owner,
        [NotNull] PlayerHealth playerHealth,
        GunController gunController,
        [NotNull] SharedBool gamePaused) {
        _gamePaused = gamePaused;

        // just for debugging -> Start
        string playerInfo = owner.PlayerID + "(isLocal: " + owner.IsLocal + ")";

        if (owner.IsLocal && gunController == null)
            Debug.LogError("PlayerStateHandler.Init: gunController is null! -> " + playerInfo);
        // -> End

        _owner = owner;
        _playerHealth = playerHealth;
        _gunController = gunController;

        MatchTimer timer = GameManager.Instance.MatchTimer;
        timer.Paused += UpdateStateOnClient;
        timer.Resumed += UpdateStateOnClient;
        UpdateStateOnClient();
    }

    public void Dispose() {
        OnDestroy();
    }

    public void OnDestroy() {
        if (_currentReanimation != null) {
            StaticCoroutine.StopStaticCoroutine(_currentReanimation);
            _currentReanimation = null;
        }

        _owner = null;
        _playerHealth = null;
        _gunController = null;

        MatchTimer timer = GameManager.Instance.MatchTimer;

        if (timer != null) {
            timer.Paused -= UpdateStateOnClient;
            timer.Resumed -= UpdateStateOnClient;
        }
    }

    #endregion

    #region calledOnMaster

    // just called on master
    public void SetPlayerStateOnMaster(PlayerState newPlayerState) {
        _playerState.Set(newPlayerState, false, false, false);
        UpdatePlayerStateOnMaster();
    }

    // just called on master
    private void UpdatePlayerStateOnMaster() {
        PlayerStateChangedSinceLastSync = true;

        // trigger also local representation of player (could be a host)
        UpdateStateOnClient();
    }

    #endregion

    #region calledOnClient

    public void OnCollidingWithPillar(bool isColliding) {
        _playerState.IsCollidingWithPillar = isColliding;
        UpdateGunControllerActive();
    }

    public void OnPlayerLeftChaperoneBounds(IPlayer player, bool playerLeftChaperoneBounds) {
        _playerState.PlayerLeftChaperoneBounds = playerLeftChaperoneBounds;
        UpdateGunControllerActive();
    }

    public void SetGunInTower(bool isColliding) {
        _playerState.IsGunInPillar = isColliding;
        // do not update status immediately. There is a grace period to get the gun out of the tower without effect.
    }

    public void UpdateGunControllerActive() {
        bool active = !_playerState.ShouldGunControllerBeDisabled();
        if (_gunController != null)
            _gunController.OnSetActive(active);

        else if (_owner != null && _owner.IsMe)
            Debug.LogWarning($"Cannot toggle gun controller: gun controller of {_owner} is null!");
    }

    private void UpdateStateOnClient() {
        if (_owner == null) return;

        if (_owner.IsMe) {
            if (_gamePaused != null) {
                _gamePaused.Set(this, PlayerState.IsInLimbo);
            }

            UpdateGunControllerActive();
        }

        SetDamageModelActive(!PlayerState.IsImmortal);

        PlayerStateChanged?.Invoke(this, PlayerState);
    }

    private void SetDamageModelActive(bool setActive) {
        if (_playerHealth != null) _playerHealth.OnSetActive(setActive);
    }

    #endregion

    #region Synchronisation

    // sync
    public void Serialize(BitSerializer stream) {
        MatchTimer matchTimer = GameManager.Instance.MatchTimer;
        if (matchTimer == null) {
            Debug.LogError("PlayerStateHandler.Serialize: MatchTimer is null!");
        }

        if (stream.IsWriting) {
            PlayerStateChangedSinceLastSync = false;
            _playerState.Serialize(stream);
        }
        else {
            _playerState.Serialize(stream);

            UpdateStateOnClient();
        }
    }

    #endregion
}