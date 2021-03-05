using System.Collections.Generic;
using Home;
using UnityEngine;

[CreateAssetMenu(menuName = "PlayerStatistic")]
public class PlayerStatistic : ScriptableObject {
    [SerializeField] private string _displayName;
    [SerializeField] private Statistic _currentStat;
    [SerializeField] private StatisticType _type;

    private Dictionary<Statistic, float> _statisticsToValue;
    private bool _initialized;

    private void OnDisable() {
        _initialized = false;
    }

    private void Init() {
        _statisticsToValue = new Dictionary<Statistic, float> {
            {Statistic.Accuracy, PlayerAccount.Statistics.accuracy},
            {Statistic.AliveTime, PlayerAccount.Statistics.aliveTime},
            {Statistic.AssistsPerRound, PlayerAccount.Statistics.assistsPerRound},
            {Statistic.Claims, PlayerAccount.Statistics.claims},
            {Statistic.ClaimsPerSecond, PlayerAccount.Statistics.claimsPerSecond},
            {Statistic.DamageDealtPerSecond, PlayerAccount.Statistics.damageDealtPerSecond},
            {Statistic.DamagePerHit, PlayerAccount.Statistics.damagePerHit},
            {Statistic.DamagePerShot, PlayerAccount.Statistics.damagePerShot},
            {Statistic.DamageTakenPerSecond, PlayerAccount.Statistics.damageTakenPerSecond},
            {Statistic.Doubles, PlayerAccount.Statistics.doubles},
            {Statistic.Draws, PlayerAccount.Statistics.draws},
            {Statistic.HeadShots, PlayerAccount.Statistics.headshots},
            {Statistic.HealthHealed, PlayerAccount.Statistics.healthHealed},
            {Statistic.HealthHealedPerSecond, PlayerAccount.Statistics.healthHealedPerSecond},
            {Statistic.Level, PlayerAccount.Statistics.level},
            {Statistic.Losses, PlayerAccount.Statistics.losses},
            {Statistic.Matches, PlayerAccount.Statistics.matches},
            {Statistic.NextClassXp, PlayerAccount.Statistics.nextClassXP},
            {Statistic.OutsPerRound, PlayerAccount.Statistics.outsPerRound},
            {Statistic.Ranking, PlayerAccount.Statistics.ranking},
            {Statistic.Rating, PlayerAccount.Statistics.rating},
            {Statistic.ScorePerRound, PlayerAccount.Statistics.scorePerRound},
            {Statistic.ShotsPerSecond, PlayerAccount.Statistics.shotsPerSecond},
            {Statistic.SniperShots, PlayerAccount.Statistics.snipershots},
            {Statistic.SurvivalRate, PlayerAccount.Statistics.survivalRate},
            {Statistic.Teleports, PlayerAccount.Statistics.teleports},
            {Statistic.TeleportsPerSecond, PlayerAccount.Statistics.teleportsPerSecond},
            {Statistic.TotalAliveTime, PlayerAccount.Statistics.totalAliveTime},
            {Statistic.TotalAssists, PlayerAccount.Statistics.totalAssists},
            {Statistic.TotalDamageDealt, PlayerAccount.Statistics.totalDamageDealt},
            {Statistic.TotalDamageTaken, PlayerAccount.Statistics.totalDamageTaken},
            {Statistic.TotalDoubles, PlayerAccount.Statistics.totalDoubles},
            {Statistic.TotalGoalPillarsClaimed, PlayerAccount.Statistics.totalGoalPillarsClaimed},
            {Statistic.TotalHeadShots, PlayerAccount.Statistics.totalHeadshots},
            {Statistic.TotalHitsTaken, PlayerAccount.Statistics.totalHitsTaken},
            {Statistic.TotalHealingReceived, PlayerAccount.Statistics.totalHealingReceived},
            {Statistic.TotalHealthHealed, PlayerAccount.Statistics.totalHealthHealed},
            {Statistic.TotalHitsDealt, PlayerAccount.Statistics.totalHitsDealt},
            {Statistic.TotalMatchTime, PlayerAccount.Statistics.totalMatchTime},
            {Statistic.TotalOuts, PlayerAccount.Statistics.totalOuts},
            {Statistic.TotalPillarsClaimed, PlayerAccount.Statistics.totalPillarsClaimed},
            {Statistic.TotalScore, PlayerAccount.Statistics.totalScore},
            {Statistic.TotalShotsFired, PlayerAccount.Statistics.totalShotsFired},
            {Statistic.TotalSniperShots, PlayerAccount.Statistics.totalSnipershots},
            {Statistic.TotalTeleports, PlayerAccount.Statistics.totalTeleports},
            {Statistic.TTClass, PlayerAccount.Statistics.ttclass},
            {Statistic.Xp, PlayerAccount.Statistics.xp},
            {Statistic.Wins, PlayerAccount.Statistics.wins}
        };
        _initialized = true;
    }

    public string DisplayName => _displayName;

    public Statistic CurrentStat => _currentStat;

    public StatisticType Type => _type;

    public float GetValueForLocalPlayer() {
        if (!_initialized)
            Init();
        return _statisticsToValue[CurrentStat];
    }

    public enum Statistic {
        Accuracy,
        AliveTime,
        AssistsPerRound,
        Claims,
        ClaimsPerSecond,
        DamageDealtPerSecond,
        DamagePerHit,
        DamagePerShot,
        DamageTakenPerSecond,
        Doubles,
        Draws,
        HeadShots,
        HealthHealed,
        HealthHealedPerSecond,
        Level,
        Losses,
        Matches,
        NextClassXp,
        OutsPerRound,
        Ranking,
        Rating,
        ScorePerRound,
        ShotsPerSecond,
        SniperShots,
        SurvivalRate,
        Teleports,
        TeleportsPerSecond,
        TotalAliveTime,
        TotalAssists,
        TotalDamageDealt,
        TotalDamageTaken,
        TotalDoubles,
        TotalGoalPillarsClaimed,
        TotalHeadShots,
        TotalHealingReceived,
        TotalHealthHealed,
        TotalHitsDealt,
        TotalHitsTaken,
        TotalMatchTime,
        TotalOuts,
        TotalPillarsClaimed,
        TotalScore,
        TotalShotsFired,
        TotalSniperShots,
        TotalTeleports,
        TTClass,
        Wins,
        Xp
    }

    public enum StatisticType {
        Total,
        Seconds
    }
}