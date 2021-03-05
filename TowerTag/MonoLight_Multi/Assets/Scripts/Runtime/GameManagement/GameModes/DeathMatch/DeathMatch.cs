using TowerTag;

///<summary>
///      GameMode simple "Team Death Match":
///      - Match is over when match time has run out.
///      - If a player gets killed, he gets respawned on a spawn pillar and reactivated after a small amount of time (respawn timeout).
///      - A team scores if one team member kills an enemy player.
///      
///      Attention:  SpawnPillars can't get claimed to avoid problems with respawning on SpawnPillars
///                  (SpawnPillars claimable attribute is set to false in InitPillarsLocal)!
/// </summary>
public class DeathMatch : Match {
    #region Properties

    /// <summary>
    /// Score/Statistics for this Match.
    /// </summary>
    public override MatchStats Stats => _stats;

    protected override bool CountGoalPillars => false;
    public override GameMode GameMode => GameMode.DeathMatch;

    #endregion

    #region Member

    /// <summary>
    /// Timeout players have to wait before they get activated again when respawned.
    /// </summary>
    private int _respawnTimeoutInSeconds;

    /// <summary>
    /// Score/Statistics for this Match.
    /// </summary>
    private DeathMatchStats _stats = new DeathMatchStats();

    #endregion

    #region Core

    /// <summary>
    /// Creates Match with values from BalancingConfiguration.
    /// </summary>
    public DeathMatch(MatchDescription matchDescription, IPhotonService photonService)
        : base(matchDescription, photonService) {
        BalancingConfiguration conf = BalancingConfiguration.Singleton;
        _respawnTimeoutInSeconds = conf.TeamDeathMatchRespawnTimeoutInSec;
    }

    protected override void InitStats() {
        GetPlayers(out var players, out var count);
        _stats = new DeathMatchStats(players, count);
    }

    protected override void InitPillars() {
        ScenePillars = PillarManager.Instance.GetAllPillars();

        BalancingConfiguration conf = BalancingConfiguration.Singleton;
        foreach (Pillar pillar in ScenePillars) {
            // init Pillar with values from BalancingConfig
            pillar.ChargeFallbackSpeed = conf.ClaimRollbackSpeed;

            // GoalPillar take longer to claim
            pillar.TimeToClaimIfNotOwned = conf.PillarClaimTimeIfNotOwnedByATeam;
            pillar.TimeToClaimIfOwned = conf.PillarClaimTimeIfOwnedByATeam;

            // register pillars
            pillar.OwningTeamChanged += OnPillarOwningTeamChanged;
        }
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
    protected override void OnPlayerDied(PlayerHealth healthOfKilledPlayer, IPlayer damageDealer,
        byte colliderType) {
        if (!PhotonService.IsMasterClient) return;

        if (!IsActive) return;

        if (healthOfKilledPlayer == null || healthOfKilledPlayer.Player == null) {
            Debug.LogError("Cannot handle player death: player or player health is null");
            return;
        }

        if (!healthOfKilledPlayer.IsActive) return;

        if (damageDealer == null) {
            Debug.LogWarning(
                "enemyWhoAppliedDamage is null, so we can't add him to the current Frag (instead playerID -1 and teamID -2 is send)!");
        }

        // cache player from DamageModel -> just for convenience
        IPlayer player = healthOfKilledPlayer.Player;

        // count single kill
        int enemyPlayerID = damageDealer?.PlayerID ?? -1;
        _stats.AddFrag(player.PlayerID, enemyPlayerID, healthOfKilledPlayer.EnemiesWhoAppliedDamageOnMasterOnly);

        // add points to killing players team
        _stats.AddTeamPoint(TeamManager.Singleton.GetEnemyTeamIDOfPlayer(player));

        int respawnAt = HelperFunctions.AddTimeInSecondsToPhotonServerTimestamp(PhotonService.ServerTimestamp,
            GameManager.Instance.MatchStartCountdownTimeInSec);
        respawnAt = HelperFunctions.AddTimeInSecondsToPhotonServerTimestamp(respawnAt,
            GameManager.Instance.CountdownDelay);
        RespawnPlayer(player, respawnAt, CountdownType.ResumeMatch, TeleportHelper.TeleportDurationType.Respawn);

        // trigger sync & refresh of Scoreboard
        OnGameStatsChanged();
    }

    protected override (bool finished, TeamID winningTeamID) GetRoundStatus() {
        return (false, TeamID.Neutral);
    }

    #endregion

    #endregion
}