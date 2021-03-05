using SOEventSystem.Shared;
using System;
using JetBrains.Annotations;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "TowerTag/Balancing Config")]
public class BalancingConfiguration : ScriptableObjectSingleton<BalancingConfiguration> {
    [Header("General")] [SerializeField] private bool _allowAntiClaim;

    [SerializeField] private bool _autoStart;
    [SerializeField] private string _autoStartSceneName;

    // gets multiplied with moving direction of Bullets (which hit a wall) to apply a force (as ForceVector) to the wall
    // (bigger force multiplier cause walls to wobble more)
    [SerializeField] private float _bulletWallHitForceMultiplier = 80f;
    [SerializeField] private float _chargerBeamLength = 25f; // 15
    [SerializeField] private float _claimRollbackSpeed = 1f; //
    [SerializeField] private float _claimRollbackTimeout = 1f; //

    // Timespan used to compensate Network Latency (used as offset for MatchStartsAt/RoundSTartsAt/ResumeAt startTimes to ensure the values arrive before the startTimes)
    [SerializeField] private int _countdownDelay = 2;

    [SerializeField] private float _energyRegenerationTimeout; // 0

    [SerializeField] private float _energyRegenerationPerSecond = 0.25f; // 0.25
    [SerializeField] private float _energyToChargePerSecond = 0.1f; // 0.1
    [SerializeField] private float _energyToHealPlayerPerSecond = 0.05f;
    [SerializeField] private float _energyToFireProjectile = 0.05f; // 0.05

    [SerializeField] private float _energyToTeleport; // 0
    // just for Balancing (disable reading external BalancingConfig file (and update the values here) before deploy)

    [Header("GunController")]

    // GunController
    [SerializeField]
    private float _fireProjectileTimeOut = 0.15f; // 0.2f

    [Header("Startup")] [SerializeField] private int _gameMode = 2; //17;                            // gameMode [0: normal 1: 1vs1, 1: 2vs2]

    // claim times (see above) of goal pillars will be multiplied by this factor
    [SerializeField] private float _goalPillarClaimTimeFactor = 3f;
    [SerializeField] private float _goalPillarPercentageValue = 1f;

    // GoalPillarMode: decide how many goal pillars have to be claimed to win a round:
    // - All: all goal pillars in scene have to be claimed by a team (ignoring numberOfGoalPillarsToClaimPercentageValue)
    // - FixedNumber: numberOfGoalPillarsToClaimPercentageValue will be interpreted as int value (3 means you have to claim/hold 3 goalPillars to win a round)
    // - Percentage: numberOfGoalPillarsToClaimPercentageValue will be interpreted as percent of the number of goal pillars in scene
    //   ([0..1]: 0.67f means you have to claim/hold 2/3 of the goal pillars in the scene)
    [SerializeField] private GameModeHelper.GoalPillarPercentageValueMode _goalPillarPercentageValueMode =
        GameModeHelper.GoalPillarPercentageValueMode.All;

    [Header("Damage Model")] [SerializeField]
    private float _hubButtonTimeout = 1f;

    [Header("Admin UI")] [SerializeField] private bool _matchAutoStart;

    [Header("Match Countdowns & Timings")]

    // Countdown Timespans (Countdown at MatchStart/RoundStart/Resume from Pause)
    [SerializeField]
    private int _matchStartCountdownTimeInSec = 5;

    [SerializeField] private int _matchTimeInSeconds = 300;
    [SerializeField] private int _initialMatchTimeInSeconds = 300;
    [SerializeField] private float _maxTeleportDuration = 1f;

    [Header("Gun Energy")]

    // Gun Energy
    [SerializeField]
    private float _noEnergyMultiplier = 2.5f; // 2.5

    [Header("Pillars")] [SerializeField] private float _pillarClaimTimeIfNotOwnedByATeam = 1f; // but everybody is owned by the A-Team!

    [SerializeField] private float _pillarClaimTimeIfOwnedByATeam = 2f; // 4

    // points a team gets (in GameModes where winning a round is independent of killing players) if a team member kills an enemy player
    [SerializeField] private float _projectileSpeed = 35; // 20
    [SerializeField] private float _respawnTeleportSpeed = 100f;
    [SerializeField] private int _resumeFromPauseCountdownTime = 5;
    [SerializeField] private int _roundStartCountdownTimeInSec = 3;
    [SerializeField] private bool _showHubButton;
    [SerializeField] private int _showMatchStatsTimeoutInSec = 3;

    // Timespans the Match/Round Scores are displayed
    [SerializeField] private int _showRoundStatsTimeoutInSec = 5;

    // Timeout players have to wait before they get activated again when respawned.
    // respawn timeout in simple DeathMatch (not LMS)
    [SerializeField] private int _teamDeathMatchRespawnTimeoutInSec = 5;
    [SerializeField] private int _goalPillarRespawnTimeout = 3;
    [SerializeField] private float _teleportCurveStrength = 0.5f; // 0.75
    [SerializeField] private float _teleportHeightFactor = 3f; // 1

    [Header("Teleport")]

    // Teleport
    [SerializeField]
    private float _teleportSpeed = 50f;

    [SerializeField] private float _teleportTriggerSpeed = 2.0f; // 2.7f;                // 3.5
    [SerializeField] private bool _useFadeBlackWhenDie = true; // delete this
    [SerializeField] private bool _useGlitchEffectWhenDie = true; // delete this

    //*** Game modes with goal pillars ***
    // some game modes allow to enable a special goal pillar rule (see TeamDeathMatch_LMS for example)

    // gets multiplied with base damage made by Bullets to calculate damage per hit (collision with Bullet)
    [SerializeField] private float _wallDamagePerHitMultiplier = 0.10001f;

    public float FireProjectileTimeOut {
        get => _fireProjectileTimeOut;
        set => _fireProjectileTimeOut = value;
    }

    public float ProjectileSpeed {
        get => _projectileSpeed;
        set => _projectileSpeed = value;
    }

    public float ChargerBeamLength {
        get => _chargerBeamLength;
        set => _chargerBeamLength = value;
    }

    public float TeleportSpeed {
        get => _teleportSpeed;
        set => _teleportSpeed = value;
    }

    public float RespawnTeleportSpeed {
        get => _respawnTeleportSpeed;
        set => _respawnTeleportSpeed = value;
    }

    public float MaxTeleportDuration {
        get => _maxTeleportDuration;
        set => _maxTeleportDuration = value;
    }

    public float TeleportHeightFactor {
        get => _teleportHeightFactor;
        set => _teleportHeightFactor = value;
    }

    public float TeleportCurveStrength {
        get => _teleportCurveStrength;
        set => _teleportCurveStrength = value;
    }

    public float TeleportTriggerSpeed {
        get => _teleportTriggerSpeed;
        set => _teleportTriggerSpeed = value;
    }

    public float NoEnergyMultiplier {
        get => _noEnergyMultiplier;
        set => _noEnergyMultiplier = value;
    }

    public float EnergyRegenerationPerSecond {
        get => _energyRegenerationPerSecond;
        set => _energyRegenerationPerSecond = value;
    }

    public float EnergyRegenerationTimeout {
        get => _energyRegenerationTimeout;
        set => _energyRegenerationTimeout = value;
    }

    public float EnergyToFireProjectile {
        get => _energyToFireProjectile;
        set => _energyToFireProjectile = value;
    }

    public float EnergyToChargePerSecond {
        get => _energyToChargePerSecond;
        set => _energyToChargePerSecond = value;
    }

    public float EnergyToHealPlayerPerSecond {
        get => _energyToHealPlayerPerSecond;
        set => _energyToHealPlayerPerSecond = value;
    }

    public float EnergyToTeleport {
        get => _energyToTeleport;
        set => _energyToTeleport = value;
    }

    public bool UseGlitchEffectWhenDie {
        get => _useGlitchEffectWhenDie;
        set => _useGlitchEffectWhenDie = value;
    }

    public bool UseFadeBlackWhenDie {
        get => _useFadeBlackWhenDie;
        set => _useFadeBlackWhenDie = value;
    }

    public float PillarClaimTimeIfNotOwnedByATeam {
        get => _pillarClaimTimeIfNotOwnedByATeam;
        set => _pillarClaimTimeIfNotOwnedByATeam = value;
    }

    public float PillarClaimTimeIfOwnedByATeam {
        get => _pillarClaimTimeIfOwnedByATeam;
        set => _pillarClaimTimeIfOwnedByATeam = value;
    }

    public float GoalPillarClaimTimeFactor {
        get => _goalPillarClaimTimeFactor;
        set => _goalPillarClaimTimeFactor = value;
    }

    public float ClaimRollbackTimeout {
        get => _claimRollbackTimeout;
        set => _claimRollbackTimeout = value;
    }

    public float ClaimRollbackSpeed {
        get => _claimRollbackSpeed;
        set => _claimRollbackSpeed = value;
    }

    [UsedImplicitly]
    public bool AllowAntiClaim {
        get => _allowAntiClaim;
        set => _allowAntiClaim = value;
    }

    public float WallDamagePerHitMultiplier {
        get => _wallDamagePerHitMultiplier;
        set => _wallDamagePerHitMultiplier = value;
    }

    public float BulletWallHitForceMultiplier {
        get => _bulletWallHitForceMultiplier;
        set => _bulletWallHitForceMultiplier = value;
    }

    [UsedImplicitly]
    public int GameMode {
        get => _gameMode;
        set => _gameMode = value;
    }

    [UsedImplicitly]
    public bool ShowHubButton {
        get => _showHubButton;
        set => _showHubButton = value;
    }

    [UsedImplicitly]

    public float HubButtonTimeout {
        get => _hubButtonTimeout;
        set => _hubButtonTimeout = value;
    }

    public bool AutoStart {
        get => _autoStart;
        set => _autoStart = value;
    }

    public string AutoStartSceneName {
        get => _autoStartSceneName;
        set => _autoStartSceneName = value;
    }

    public int MatchStartCountdownTimeInSec {
        get => _matchStartCountdownTimeInSec;
        set => _matchStartCountdownTimeInSec = value;
    }

    public int RoundStartCountdownTimeInSec {
        get => _roundStartCountdownTimeInSec;
        set => _roundStartCountdownTimeInSec = value;
    }

    public int ResumeFromPauseCountdownTime {
        get => _resumeFromPauseCountdownTime;
        set => _resumeFromPauseCountdownTime = value;
    }

    public int ShowRoundStatsTimeoutInSec {
        get => _showRoundStatsTimeoutInSec;
        set => _showRoundStatsTimeoutInSec = value;
    }

    public int ShowMatchStatsTimeoutInSec {
        get => _showMatchStatsTimeoutInSec;
        set => _showMatchStatsTimeoutInSec = value;
    }

    public int TeamDeathMatchRespawnTimeoutInSec {
        get => _teamDeathMatchRespawnTimeoutInSec;
        set => _teamDeathMatchRespawnTimeoutInSec = value;
    }

    public int GoalPillarRespawnTimeout => _goalPillarRespawnTimeout;

    public GameModeHelper.GoalPillarPercentageValueMode GoalPillarPercentageValueMode {
        get => _goalPillarPercentageValueMode;
        set => _goalPillarPercentageValueMode = value;
    }

    public float GoalPillarPercentageValue {
        get => _goalPillarPercentageValue;
        set => _goalPillarPercentageValue = value;
    }

    public int CountdownDelay {
        get => _countdownDelay;
        set => _countdownDelay = value;
    }

    public int MatchTimeInSeconds {
        get => _matchTimeInSeconds;
        set => _matchTimeInSeconds = value;
    }

    public int InitialMatchTimeInSeconds {
        get => _initialMatchTimeInSeconds;
        set => _initialMatchTimeInSeconds = value;
    }

    public bool MatchAutoStart {
        get => _matchAutoStart;
        set => _matchAutoStart = value;
    }
}