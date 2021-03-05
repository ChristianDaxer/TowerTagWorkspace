using SOEventSystem.Shared;
using TowerTag;
using UnityEngine;

[CreateAssetMenu(menuName = "LocalPlayerstats/KillsInCurrentRound")]
public class KillsInCurrentRound : SharedVariable<int> {
    [SerializeField] private HitGameAction _hitGameAction;

    private new void OnEnable() {
        base.OnEnable();
        _hitGameAction.PlayerGotHit += RaiseValue;
        RegisterListeners(GameManager.Instance);
        if (GameManager.Instance.CurrentMatch != null)
            OnMatchHasFinishedLoading(GameManager.Instance.CurrentMatch);
    }

    private new void OnDisable() {
        base.OnDisable();
        _hitGameAction.PlayerGotHit -= RaiseValue;
        UnregisterListeners(GameManager.Instance);
    }

    private void RegisterListeners(GameManager gameManager) {
        gameManager.MatchHasFinishedLoading += OnMatchHasFinishedLoading;
        if (GameManager.Instance.CurrentMatch != null)
            RemoveListener(GameManager.Instance.CurrentMatch);
    }

    private void UnregisterListeners(GameManager gameManager) {
        gameManager.MatchHasFinishedLoading -= OnMatchHasFinishedLoading;
    }

    private void OnMatchHasFinishedLoading(IMatch match) {
        match.Finished += RemoveListener;
        match.RoundFinished += ResetKillsThisRound;

        IPlayer ownPlayer = PlayerManager.Instance.GetOwnPlayer();
        if (ownPlayer != null) ownPlayer.PlayerHealth.PlayerDied += ResetKillStreak;

        //Reset at the start of a match to be sure it is at 0
        ResetKillsThisRound(match, TeamID.Neutral);
    }

    private void RemoveListener(IMatch match) {
        match.Finished -= RemoveListener;
        match.RoundFinished -= ResetKillsThisRound;
        IPlayer ownPlayer = PlayerManager.Instance.GetOwnPlayer();
        if (ownPlayer != null) ownPlayer.PlayerHealth.PlayerDied -= ResetKillStreak;
    }

    private void RaiseValue(ShotData shotData, IPlayer targetPlayer, Vector3 hitPoint,
        DamageDetectorBase.ColliderType targetType) {
        if (shotData.Player.IsMe && !targetPlayer.PlayerHealth.IsAlive) {
            Set(this, Value + 1);
        }
    }

    /// <summary>
    /// "Kill streaks" gets reset after the player died in non-round-based matches
    /// </summary>
    private void ResetKillStreak(PlayerHealth playerHealth, IPlayer enemy, byte colliderType) {
        Set(this, 0);
    }

    private void ResetKillsThisRound(IMatch match, TeamID teamID) {
        Set(this, 0);
    }
}