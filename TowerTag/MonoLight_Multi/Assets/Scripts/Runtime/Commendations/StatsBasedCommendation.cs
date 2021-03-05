using Commendations;
using UnityEngine;

[CreateAssetMenu(menuName = "TowerTag/Commendations/Stats Based")]
public class StatsBasedCommendation : PerformanceBasedCommendation {
    [Header("computed quantities")]
    [SerializeField, Tooltip("Impact of accuracy in percent, i.e., the number of hits over the number of shots. " +
                             "The accuracy is usually a single digit number")]
    private float _accuracyImpact;

    [SerializeField, Tooltip("Impact of score/out ratio. ")]
    private float _scoreOverOutImpact;

    [SerializeField, Tooltip("Impact of assist/out ratio. ")]
    private float _assistOverOutImpact;

    [SerializeField, Tooltip("Impact of pillar claims per second alive")]
    private float _claimsPerSecondImpact;

    [SerializeField, Tooltip("Impact of goal-pillar claims per second alive")]
    private float _goalPillarsPerSecondImpact;

    [SerializeField, Tooltip("Impact of teleports per second alive")]
    private float _teleportsPerSecondImpact;

    [Header("base quantities")] [SerializeField]
    private float _KillsImpact;

    [SerializeField] private float _AssistsImpact;
    [SerializeField] private float _DeathsImpact;
    [SerializeField] private float _ShotsFiredImpact;
    [SerializeField] private float _HitsDealtImpact;
    [SerializeField] private float _HitsTakenImpact;
    [SerializeField] private float _DamageDealtImpact;
    [SerializeField] private float _DamageTakenImpact;
    [SerializeField] private float _HealthHealedImpact;
    [SerializeField] private float _HealingReceivedImpact;
    [SerializeField] private float _GoalPillarsClaimedImpact;
    [SerializeField] private float _PillarsClaimedImpact;
    [SerializeField] private float _TeleportsImpact;
    [SerializeField] private float _HeadShotsImpact;
    [SerializeField] private float _SniperShotsImpact;
    [SerializeField] private float _DoublesImpact;
    [SerializeField] private float _PlayTimeImpact;

    protected override float Performance(PlayerStats playerStats) {
        float accuracy = (float) playerStats.HitsDealt / (playerStats.ShotsFired + 1); // expect 0.01 - 0.05
        float playTime = Mathf.Max(1, playerStats.PlayTime); // expect 240 - 480
        float scoreOverOut = (float) playerStats.Kills / (playerStats.Deaths + 1); // expect 0 - 10
        float assistOverOut = (float) playerStats.Assists / (playerStats.Deaths + 1); // expect 0 - 5
        float claimsPerSecond = playerStats.PillarsClaimed / playTime; // expect 0.01 - 0.1
        float teleportsPerSecond = playerStats.Teleports / playTime; // expect 0.03 - 0.3
        float goalPillarsPerSecond = playerStats.GoalPillarsClaimed / playTime; // expect 0 - 0.03

        return _accuracyImpact * accuracy
               + _scoreOverOutImpact * scoreOverOut
               + _assistOverOutImpact * assistOverOut
               + _claimsPerSecondImpact * claimsPerSecond
               + _teleportsPerSecondImpact * teleportsPerSecond
               + _goalPillarsPerSecondImpact * goalPillarsPerSecond
               + _KillsImpact * playerStats.Kills
               + _AssistsImpact * playerStats.Assists
               + _DeathsImpact * playerStats.Deaths
               + _ShotsFiredImpact * playerStats.ShotsFired
               + _HitsDealtImpact * playerStats.HitsDealt
               + _HitsTakenImpact * playerStats.HitsTaken
               + _DamageDealtImpact * playerStats.DamageDealt
               + _DamageTakenImpact * playerStats.DamageTaken
               + _HealthHealedImpact * playerStats.HealthHealed
               + _HealingReceivedImpact * playerStats.HealingReceived
               + _GoalPillarsClaimedImpact * playerStats.GoalPillarsClaimed
               + _PillarsClaimedImpact * playerStats.PillarsClaimed
               + _TeleportsImpact * playerStats.Teleports
               + _HeadShotsImpact * playerStats.HeadShots
               + _SniperShotsImpact * playerStats.SniperShots
               + _DoublesImpact * playerStats.Doubles
               + _PlayTimeImpact * playerStats.PlayTime;
    }
}