using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using Commendations;
using Rewards;
using TowerTag;
using TowerTagAPIClient.Model;
using TowerTagAPIClient.Store;
using UnityEngine;

[RequireComponent(typeof(ClientStatistics))]
public class StatCollector : TTSingleton<StatCollector> {
    private IPlayer _player;
    private ChargePlayer _currentlyConnectedPlayer;
    private Pillar _currentlyClaimingPillar;
    private float _currentHealValue;
    private float _lastChargeValue;

    [SerializeField] private RopeGameAction ropeGameAction;

    private ClientStatistics clientStatistics;
    private bool IsNextLoginDay =>
        DateTime.Parse(PlayerPrefs.GetString(PlayerPrefKeys.Login)).Date.AddDays(1) == DateTime.Now.Date;

    private bool IsSameLoginDay =>
        DateTime.Parse(PlayerPrefs.GetString(PlayerPrefKeys.Login)).Date.Equals(DateTime.Now.Date);

#region EventRegistration

    private void OnEnable() {
        clientStatistics.Stored += HandleLoginDate;
        GameManager.Instance.MatchHasFinishedLoading += RegisterMatchEvents;
        CommendationsController.LocalPlayerCommendationAwarded += OnLocalPlayerReceivedAward;
        PlayerStatisticsStore.PlayerStatisticsReceived += OnPlayerStatisticsReceived;
    }

    private void OnDisable() {
        clientStatistics.Stored -= HandleLoginDate;
        GameManager.Instance.MatchHasFinishedLoading -= RegisterMatchEvents;
        CommendationsController.LocalPlayerCommendationAwarded -= OnLocalPlayerReceivedAward;
        PlayerStatisticsStore.PlayerStatisticsReceived -= OnPlayerStatisticsReceived;
    }

    protected override void Init() {
        clientStatistics = GetComponent<ClientStatistics>();
        DontDestroyOnLoad(gameObject);
    }

    private void HandleLoginDate() {
        if (!PlayerPrefs.HasKey(PlayerPrefKeys.Login)) {
            AchievementManager.RaiseStatisticWithOne(clientStatistics.keys.LoginRow);
            PlayerPrefs.SetString(PlayerPrefKeys.Login, DateTime.Now.Date.ToString(CultureInfo.CurrentCulture));
        }
        else if (IsNextLoginDay) {
            AchievementManager.RaiseStatisticWithOne(clientStatistics.keys.LoginRow);
            PlayerPrefs.SetString(PlayerPrefKeys.Login, DateTime.Now.Date.ToString(CultureInfo.CurrentCulture));
        }
        else if (!IsSameLoginDay) {
            AchievementManager.SetStatistic(clientStatistics.keys.LoginRow, 1);
            PlayerPrefs.SetString(PlayerPrefKeys.Login, DateTime.Now.Date.ToString(CultureInfo.CurrentCulture));
        }
    }

    private void RegisterMatchEvents(IMatch match) {
        _player = PlayerManager.Instance.GetOwnPlayer();
        if (_player == null) return;

        _player.GunController.TeleportTriggered += OnTeleportTriggered;
        SnipeReward.SnipeShotHit += OnSnipeHit;
        match.Finished += OnMatchFinished;
        HitGameAction.Instance.PlayerGotHit += OnPlayerGotHit;
        ropeGameAction.RopeConnectedToChargeable += OnRopeConnected;
        ropeGameAction.Disconnecting += OnRopeDisconnected;
    }

    private void UnregisterMatchEvents(IMatch match) {
        if (_player == null) return;
        _player.GunController.TeleportTriggered -= OnTeleportTriggered;
        SnipeReward.SnipeShotHit -= OnSnipeHit;
        match.Finished -= OnMatchFinished;
        HitGameAction.Instance.PlayerGotHit -= OnPlayerGotHit;
        ropeGameAction.RopeConnectedToChargeable -= OnRopeConnected;
        ropeGameAction.Disconnecting -= OnRopeDisconnected;
    }

#endregion

    private void OnPlayerStatisticsReceived(PlayerStatistics playerStatistics) {
        PlayerIdManager.GetInstance(out var playerIdManager);
        if (playerStatistics.id.Equals(playerIdManager.GetUserId()))
            StartCoroutine(SetBackendStatistics(() => clientStatistics.StoringDone, playerStatistics));
    }

    private IEnumerator SetBackendStatistics(Func<bool> condition, PlayerStatistics playerStatistics) {
        yield return new WaitUntil(condition);
        if (playerStatistics.level != clientStatistics.Statistics[clientStatistics.keys.Lvl]) {
            AchievementManager.SetStatistic(clientStatistics.keys.Lvl, playerStatistics.level);
        }

        var daysPlayed = TimeSpan.FromSeconds(playerStatistics.totalMatchTime).Days;
        if (daysPlayed > 0) {
            if (Achievements.GetInstance(out var achievements))
                AchievementManager.SetAchievement(achievements.Keys.PlayTime);
        }
        if (playerStatistics.matches != clientStatistics.Statistics[clientStatistics.keys.Matches]) {
            if (PlayerPrefs.HasKey(PlayerPrefKeys.MatchesUpdated) &&
                PlayerPrefs.GetInt(PlayerPrefKeys.MatchesUpdated) == 1) yield break;
            AchievementManager.SetStatistic(clientStatistics.keys.Matches, playerStatistics.matches);
            PlayerPrefs.SetInt(PlayerPrefKeys.MatchesUpdated, 1);
        }
    }

    private void OnLocalPlayerReceivedAward(Commendation commendation) {
        if (commendation.DisplayName == "HOTTEST FIRE")
            AchievementManager.RaiseStatisticWithOne(clientStatistics.keys.Fire);
        if (commendation.DisplayName == "COLDEST ICE")
            AchievementManager.RaiseStatisticWithOne(clientStatistics.keys.Ice);
        if (commendation.DisplayName == "M.V.P.")
            AchievementManager.RaiseStatisticWithOne(clientStatistics.keys.MVP);
    }

    private void OnRopeConnected(RopeGameAction sender, IPlayer player, Chargeable chargeable) {
        if (!player.IsMe) return;
        switch (chargeable) {
            case ChargePlayer chargePlayer: {
                _currentlyConnectedPlayer = chargePlayer;
                _lastChargeValue = chargePlayer.CurrentCharge.value;
                chargePlayer.ChargeSet += OnChargeSet;
                if (chargePlayer.Owner.PlayerHealth.HealthFraction <= 0.2f)
                    AchievementManager.RaiseStatisticWithOne(clientStatistics.keys.HealedLowPlayer);
                break;
            }
            case Pillar pillar when pillar.OwningTeamID != player.TeamID:
                pillar.OwningTeamChanged += PillarClaimed;
                _currentlyClaimingPillar = pillar;
                break;
        }
    }

    private void OnRopeDisconnected(RopeGameAction sender, IPlayer player, Chargeable target, bool onPurpose) {
        if (player != null && !player.IsMe) return;
        if (target == _currentlyConnectedPlayer) {
            SetHealValue();
            ResetPlayerChargeValues();
        }
        else if (target == _currentlyClaimingPillar) {
            ResetPillarChargeValues();
        }
    }

    private void ResetPillarChargeValues() {
        _currentlyClaimingPillar.OwningTeamChanged -= PillarClaimed;
        _currentlyClaimingPillar = null;
    }

    private void ResetPlayerChargeValues() {
        _currentlyConnectedPlayer.ChargeSet -= OnChargeSet;
        _currentHealValue = 0;
        _currentlyConnectedPlayer = null;
    }

    private void SetHealValue() {
        var healingAmount = Mathf.RoundToInt(_currentHealValue * _player.PlayerHealth.MaxHealth);

        if (_currentHealValue > 0)
            AchievementManager.RaiseStatisticWithValue(clientStatistics.keys.HealthHealed, healingAmount);
    }

    private void PillarClaimed(Claimable claimable, TeamID oldTeam, TeamID newTeam, IPlayer[] attachedPlayers) {
        if (newTeam != _player.TeamID && !attachedPlayers.Any(player => player.IsMe)) return;
        AchievementManager.RaiseStatisticWithOne(clientStatistics.keys.Claims);
        if (_currentlyClaimingPillar == null) {
            Debug.LogWarning("SteamStartCollector: Missing current claimed Pillar. This should not have happened.");
            var pillar = claimable.GetComponent<Pillar>();
            if (pillar == null) return;
            _currentlyClaimingPillar = pillar;
        }

        _currentlyClaimingPillar.OwningTeamChanged -= PillarClaimed;
        _currentlyClaimingPillar = null;
    }

    private void OnChargeSet(Chargeable chargeable, TeamID team, float value) {
        if (chargeable != _currentlyConnectedPlayer) return;
        var chargeAmount = value - _lastChargeValue;

        if (chargeAmount > 0) {
            _lastChargeValue = value;
            _currentHealValue += chargeAmount;
        }
        else if (chargeAmount < -0.1f) {
            //Sometimes we receive small reductions. To avoid counting health reductions higher than the min of .15 (Weapon hit)
            //This if is needed.
            _lastChargeValue = value;
        }
    }

    private void OnSnipeHit() {
        if (_player != null)
            AchievementManager.RaiseStatisticWithOne(clientStatistics.keys.Snipe);
    }

    private void OnMatchFinished(IMatch match) {
        if (_player == null) return;
        if (match.Stats.WinningTeamID == _player.TeamID) {
            AchievementManager.RaiseStatisticWithOne(clientStatistics.keys.Win);
            AchievementManager.RaiseStatisticWithOne(clientStatistics.keys.WinStreak);
        }
        else {
            AchievementManager.SetStatistic(clientStatistics.keys.WinStreak, 0);
        }

        AchievementManager.RaiseStatisticWithOne(clientStatistics.keys.Matches);
        UnregisterMatchEvents(match);
    }

    private void OnTeleportTriggered(IPlayer player, Pillar target) {
        AchievementManager.RaiseStatisticWithOne(clientStatistics.keys.Tele);
    }

    private void OnPlayerGotHit(ShotData shotData, IPlayer targetPlayer, Vector3 hitPoint,
        DamageDetectorBase.ColliderType targetType) {
        if (targetPlayer.IsMe && targetType == DamageDetectorBase.ColliderType.Head) {
            AchievementManager.RaiseStatisticWithOne(clientStatistics.keys.HeadShotsTaken);
        }
    }

    private double ConvertToUnixTimestamp(DateTime date) {
        DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        TimeSpan diff = date.ToUniversalTime() - origin;
        return Math.Floor(diff.TotalSeconds);
    }

    [ContextMenu("Reset")]
    protected virtual void ResetStatsAndAchievements () {}
}