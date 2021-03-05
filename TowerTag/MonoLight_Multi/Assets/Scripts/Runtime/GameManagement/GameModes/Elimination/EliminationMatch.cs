using System.Linq;
using TowerTag;

/// <summary>
/// <b>Elimination Match</b>
/// <br/>- Round is over when one team was killed entirely.
/// <br/>- A team scores if it wins a round (when it kills all enemy players).
/// <br/>- Match is over when match time has run out.
/// <br/>- If a Player gets killed, he is teleported onto a spectator pillar as a ghost to watch.
/// </summary>
public class EliminationMatch : Match {
    #region Properties

    public override MatchStats Stats => _stats;
    protected override bool CountGoalPillars => false;
    public override GameMode GameMode => GameMode.Elimination;

    #endregion

    #region Member

    /// <summary>
    /// Score/Statistics for this Match.
    /// </summary>
    private EliminationMatchStats _stats = new EliminationMatchStats();

    #endregion

    #region Core

    /// <summary>
    /// Create a new EliminationMatch instance. Creates Match with values from BalancingConfiguration.
    /// </summary>
    public EliminationMatch(MatchDescription matchDescription, IPhotonService photonService)
        : base(matchDescription, photonService) {
    }

    protected override void InitStats() {
        GetPlayers(out var players, out var count);
        _stats = new EliminationMatchStats(players, count);
    }

    protected override void InitPillars() {
        ScenePillars = PillarManager.Instance.GetAllPillars();

        BalancingConfiguration conf = BalancingConfiguration.Singleton;
        foreach (Pillar pillar in ScenePillars) {
            if (pillar.IsSpawnPillar) {
                // SpawnPillars can be claimed now
                pillar.IsClaimable = true;
            }

            // init Pillar with values from BalancingConfig
            pillar.ChargeFallbackSpeed = conf.ClaimRollbackSpeed;

            // GoalPillar take longer to claim
            pillar.TimeToClaimIfNotOwned = conf.PillarClaimTimeIfNotOwnedByATeam;
            pillar.TimeToClaimIfOwned = conf.PillarClaimTimeIfOwnedByATeam;

            // register pillars
            pillar.OwningTeamChanged += OnPillarOwningTeamChanged;
        }
    }

    public override void StartNewRound() {
        // Reset SpawnPillars (because they could be claimed by other Team now)
        // just to ensure that all spawnPillars are set correctly at the start of a new round
        // (they also get reset in ResetMatchForNewRound() but sometimes they get claimed after Reset (probably by Network latency))
        ScenePillars?
            .Where(pillar => pillar.IsSpawnPillar)
            .ForEach(PillarManager.ResetPillarOwningTeam);
        base.StartNewRound();
    }

    public override void StartNewRoundAt(int startTimestamp, int finishTimestamp) {
        PillarManager.Instance.ResetPillarOwningTeamForAllPillars();
        base.StartNewRoundAt(startTimestamp, finishTimestamp);
    }

    #endregion

    #region Player was killed

    /// <summary>
    /// Callback if a Player was killed.
    /// This function should only get called on Master client (is ignored on remote clients).
    /// </summary>
    /// <param name="playerHealth">DamageModel of killed Player.</param>
    /// <param name="damageDealer">The enemy who shot.</param>
    /// <param name="colliderType">Collider that was hit by the shot (see DamageDetector).</param>
    protected override void OnPlayerDied(PlayerHealth playerHealth, IPlayer damageDealer, byte colliderType) {
        // ignore this call if we are not the Master client
        if (!PhotonService.IsMasterClient)
            return;

        // only execute this call if we are still playing
        if (!IsActive)
            return;

        IPlayer player = playerHealth.Player;
        if (player == null) {
            Debug.LogWarning("Cannot register player death: Player is null");
        }

        System.Diagnostics.Debug.Assert(player != null, nameof(player) + " != null");


        if (damageDealer == null)
            Debug.LogWarning("Damage dealer is null, so we can't add him to the Stats");


        int enemyPlayerID = damageDealer?.PlayerID ?? -1;
        _stats.AddFrag(player.PlayerID, enemyPlayerID, playerHealth.EnemiesWhoAppliedDamageOnMasterOnly);
        if (!player.IsLateJoiner) {
            TeleportHelper.TeleportPlayerToFreeSpectatorPillar(player, TeleportHelper.TeleportDurationType.Respawn);
        }

        player.PlayerStateHandler.SetPlayerStateOnMaster(PlayerState.DeadButNoLimbo);

        // the last player of a team was killed the other team wins the round
        (bool roundFinished, TeamID roundWinningTeamID) = GetRoundStatus();
        if (roundFinished) {
            FinishRoundOnMaster(roundWinningTeamID);
        }

        // trigger sync & refresh of Scoreboard
        OnGameStatsChanged();
    }

    protected override (bool finished, TeamID winningTeamID) GetRoundStatus() {
        if (GetPlayersCount() == 0) return (false, TeamID.Neutral);
        PlayerManager.Instance.GetParticipatingPlayers(out var players, out var count);
        if (players.Take(count).Count(p => p.TeamID == TeamID.Ice && p.IsAlive) == 0) return (true, TeamID.Fire);
        if (players.Take(count).Count(p => p.TeamID == TeamID.Fire && p.IsAlive) == 0) return (true, TeamID.Ice);
        return (false, TeamID.Neutral);
    }

    protected override void FinishRoundOnMaster(TeamID winningTeam) {
        base.FinishRoundOnMaster(winningTeam);
        // Last player can shoot and claim
        PlayerManager.Instance.GetParticipatingPlayers(out var players, out var count);
        for (int i = 0; i < count; i++)
        {
            bool isAlive = players[i].PlayerHealth.IsAlive;
            players[i].PlayerStateHandler.SetPlayerStateOnMaster(
                new PlayerState(!isAlive, !isAlive, false));
        }
    }

    #endregion
}