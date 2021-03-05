using System.Linq;
using TowerTag;

///<summary>
///GameMode "Goal Pillar":
/// <list type="bullet">
/// <item>Round is over when one team claimed the goal pillar of an enemy team.</item>
/// <item>Match is over when match time has run out.</item>
/// <item>If a player gets killed, he gets respawned on a spawn pillar and reactivated after a small amount of time (respawn timeout).</item>
/// <item>A team scores when claiming a goal pillar (_pointsForWinningRound) or killing an enemy player (_pointsForKillingPlayer).</item>
///</list>
/// <br/>
///     Attention:  SpawnPillars can't get claimed to avoid problems with respawning on SpawnPillars
///                (SpawnPillars claimable attribute is set to false in InitPillarsLocal)!
///</summary>
public sealed class GoalPillarMatch : Match {
    #region Properties

    /// <summary>
    /// Score/Statistics for this Match.
    /// </summary>
    public override MatchStats Stats => _stats;

    protected override bool CountGoalPillars => true;

    public override GameMode GameMode => GameMode.GoalTower;

    #endregion

    #region Member

    /// <summary>
    /// Points a Team gets for winning a round
    /// </summary>
    private int _pointsForWinningRound;

    /// <summary>
    /// Timeout players have to wait before they get activated again when respawned.
    /// </summary>
    private int _respawnTimeoutInSeconds;

    /// <summary>
    /// Number of goal pillar a team has to conquer/hold to win the current round.
    /// This value is calculated by WhereEnoughGoalPillarsCapturedByTeamToWinTheRound function.
    /// </summary>
    private int _numberOfGoalPillarsNeededToWinARound;

    /// <summary>
    /// Multiplier for time needed to claim a goal pillar (multiplied with timeToClaimIfOwned/timeToClaimIfNotOwned).
    /// </summary>
    private float _goalPillarClaimTimeFactor;

    /// <summary>
    /// Score/Statistics for this Match.
    /// </summary>
    private GoalPillarStats _stats = new GoalPillarStats();

    /// <summary>
    /// GoalPillarPercentageValueMode currently used for this match.
    /// </summary>
    private readonly GameModeHelper.GoalPillarPercentageValueMode _goalPillarPercentageValueMode;

    /// <summary>
    /// Value to decide how much goal pillar a team has to conquer/hold to win the current round.
    /// (see GameModeHelper.GoalPillarPercentageValueMode enum and WhereEnoughGoalPillarsCapturedByTeamToWinTheRound function)
    /// - is ignored when goalPillarPercentageValueMode is All
    /// - use range of [0..1] if goalPillarPercentageValueMode is Percentage (can be used across multiple scenes with different numbers of goalPillars)
    /// - use range of [0..GoalPillarsInScene] if goalPillarPercentageValueMode is fixedNumber
    /// </summary>
    private readonly float _goalPillarPercentageValue;

    #endregion

    /// <summary>
    /// Create a new TeamDeathMatch_LMS instance. Creates Match with values from BalancingConfiguration.
    /// </summary>
    public GoalPillarMatch(MatchDescription matchDescription, IPhotonService photonService)
        : base(matchDescription, photonService) {
        BalancingConfiguration conf = BalancingConfiguration.Singleton;
        MatchTimeInSeconds = conf.MatchTimeInSeconds;
        _respawnTimeoutInSeconds = conf.TeamDeathMatchRespawnTimeoutInSec;
        _goalPillarClaimTimeFactor = conf.GoalPillarClaimTimeFactor;
        _goalPillarPercentageValueMode = conf.GoalPillarPercentageValueMode;
        _goalPillarPercentageValue = conf.GoalPillarPercentageValue;
    }

    #region Implemented IGameMode Interface

    protected override void InitStats() {
        PlayerManager.Instance.GetParticipatingPlayers(out var players, out var count);
        _stats = new GoalPillarStats(players, count);
    }

    protected override void InitPillars() {
        ScenePillars = PillarManager.Instance.GetAllPillars();

        int numberOfGoalPillarsInScene = PillarManager.Instance.GetNumberOfGoalPillarsInScene();
        _numberOfGoalPillarsNeededToWinARound =
            GameModeHelper.CalculateNumberOfGoalPillarsNeededToWinARound(numberOfGoalPillarsInScene,
                _goalPillarPercentageValueMode, _goalPillarPercentageValue);

        // init pillar with values from Balancing config
        BalancingConfiguration conf = BalancingConfiguration.Singleton;
        foreach (Pillar pillar in ScenePillars) {
            //if (pillar.IsSpawnPillar) {
            //    // SpawnPillars can be claimed now
            //    pillar.IsClaimable = false;
            //}

            // init Pillar with values from BalancingConfig
            pillar.ChargeFallbackSpeed = conf.ClaimRollbackSpeed;

            // GoalPillar take longer to claim
            float factor = pillar.IsGoalPillar ? _goalPillarClaimTimeFactor : 1;
            pillar.TimeToClaimIfNotOwned = conf.PillarClaimTimeIfNotOwnedByATeam * factor;
            pillar.TimeToClaimIfOwned = conf.PillarClaimTimeIfOwnedByATeam * factor;

            // register pillars
            pillar.OwningTeamChanged += OnPillarOwningTeamChanged;
        }
    }

    public override void StartNewRoundAt(int startTimestamp, int finishTimestamp) {
        PillarManager.Instance.ResetPillarOwningTeamForAllPillars();
        base.StartNewRoundAt(startTimestamp, finishTimestamp);
    }

    #endregion

    #region Serialization

    /// <summary>
    /// Serialize the internal state:
    /// - call it with writeStream to write the internal state to stream
    /// - call it with readStream to deserialize the internal state from stream
    /// </summary>
    /// <param name="stream">Stream to read from or write your data to.</param>
    /// <returns>True if succeeded read/write, false otherwise.</returns>
    public override bool Serialize(BitSerializer stream) {
        bool success = base.Serialize(stream);
        success = success && stream.Serialize(ref _respawnTimeoutInSeconds, 0,
                      BitCompressionConstants.MaxRespawnTimeoutInSec);
        success = success && stream.Serialize(ref _pointsForWinningRound, 0,
                      BitCompressionConstants.MaxPointsForWinningARound);
        success = success && stream.Serialize(ref _goalPillarClaimTimeFactor,
                      BitCompressionConstants.MinGoalPillarClaimTimeFactor,
                      BitCompressionConstants.MaxGoalPillarClaimTimeFactor,
                      BitCompressionConstants.GoalPillarClaimTimeFactorResolution);
        success = success && stream.Serialize(ref _numberOfGoalPillarsNeededToWinARound, 0,
                      BitCompressionConstants.MaxNumberOfGoalPillarsNeededToWinARound);

        return success;
    }

    #endregion

    #region Helper on Masterclient only

    #region Player was killed

    /// <summary>
    /// Callback if a Player was killed.
    /// This function should only get called on Master client (is ignored on remote clients).
    /// </summary>
    /// <param name="healthOfKilledPlayer">DamageModel of killed Player.</param>
    /// <param name="damageDealer">The enemy who shot.</param>
    /// <param name="colliderType">Collider that was hit by the shot (see DamageDetector).</param>
    protected override void OnPlayerDied(PlayerHealth healthOfKilledPlayer, IPlayer damageDealer, byte colliderType) {
        if (!PhotonService.IsMasterClient)
            return;

        if (!IsActive)
            return;
        if (healthOfKilledPlayer == null || healthOfKilledPlayer.Player == null) {
            Debug.LogError("Cannot handle player death: player or player health is null");
            return;
        }

        if (!healthOfKilledPlayer.IsActive)
            return;

        if (damageDealer == null) {
            Debug.LogWarning(
                "EnemyWhoAppliedDamage is null, so we can't add him to the current Frag (instead playerID -1 and teamID -2 is send)!");
        }

        IPlayer player = healthOfKilledPlayer.Player;
        int enemyPlayerID = damageDealer?.PlayerID ?? -1;
        _stats.AddFrag(player.PlayerID, enemyPlayerID, healthOfKilledPlayer.EnemiesWhoAppliedDamageOnMasterOnly);

        int respawnAt = HelperFunctions.AddTimeInSecondsToPhotonServerTimestamp(PhotonService.ServerTimestamp,
            BalancingConfiguration.Singleton.GoalPillarRespawnTimeout);
        respawnAt = HelperFunctions.AddTimeInSecondsToPhotonServerTimestamp(respawnAt,
            GameManager.Instance.CountdownDelay);

        RespawnPlayer(player, respawnAt, CountdownType.StartRound, TeleportHelper.TeleportDurationType.Respawn);
        // trigger sync & refresh of Scoreboard
        OnGameStatsChanged();
    }

    #endregion

    #region Pillar was claimed

    /// <summary>
    /// Callback if a Pillar was claimed by a Team.
    /// </summary>
    /// <param name="claimable">The chargeable underlying the pillar</param>
    /// <param name="oldTeamID">The Team who owned the Pillar before it was claimed.</param>
    /// <param name="newTeamID">The Team who claimed the Pillar.</param>
    /// <param name="newOwner">The Player who claimed the Pillar</param>
    /// ///
    protected override void OnPillarOwningTeamChanged(Claimable claimable, TeamID oldTeamID, TeamID newTeamID,
        IPlayer[] newOwner) {
        if (!PhotonService.IsMasterClient)
            return;

        // only check this if we are still playing (gameMode.isActive == true)
        if (!IsActive)
            return;

        // calculate pillar distribution to view it in UI
        CalculateNumberOfPillarsOwnedByTeams();

        // if the claimed pillar is no goal pillar return to business
        Pillar pillar = PillarManager.Instance.GetPillarByID(claimable.ID);
        if (pillar == null || !pillar.IsGoalPillar)
            return;

        (bool roundFinished, TeamID roundWinningTeamID) = GetRoundStatus();
        if (roundFinished) {
            FinishRoundOnMaster(roundWinningTeamID);
        }
    }

    protected override (bool finished, TeamID winningTeamID) GetRoundStatus() {
        Pillar[] allGoalPillarsInScene = PillarManager.Instance.GetAllGoalPillarsInScene();
        if (allGoalPillarsInScene.Length == 0) return (false, TeamID.Neutral);

        if (allGoalPillarsInScene.Count(pillar => pillar.OwningTeamID != TeamID.Fire) == 0)
            return (true, TeamID.Fire);

        if (allGoalPillarsInScene.Count(pillar => pillar.OwningTeamID != TeamID.Ice) == 0)
            return (true, TeamID.Ice);

        return (false, TeamID.Neutral);
    }

    protected override void FinishRoundOnMaster(TeamID winningTeam) {
        base.FinishRoundOnMaster(winningTeam);
        // Disable all players
        GetPlayers(out var players, out var count);
        for (int i = 0; i < count; i++)
            players[i].PlayerStateHandler.SetPlayerStateOnMaster(PlayerState.DeadButNoLimbo);
    }

    #endregion

    #endregion
}