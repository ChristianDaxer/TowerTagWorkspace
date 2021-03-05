using System;
using TowerTag;

public delegate void MatchAction(IMatch match);

public delegate void RoundFinishAction(IMatch match, TeamID roundWinningTeamID);

public delegate void MatchTimeAction(IMatch match, int time);

public delegate void StatsAction(MatchStats stats);

public enum GameMode {
    Elimination = 0x1,
    DeathMatch = 0x2,
    GoalTower = 0x4,
    UserVote = 0x16
}

public interface IMatch {
    #region Properties

    /// <summary>
    /// ID of this Match(assigned by MatchConfigurator (through ctor) to create new Match with MatchConfigurator.CreateMatchFromMatchID(matchID) on remote clients after deserialization).
    /// </summary>
    int MatchID { get; }

    /// <summary>
    /// Is the current Match active (activated at StartMatch/StartRound, deactivated at StopMatch/RoundFinished/MatchFinished)?
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Is the match already loaded?
    /// </summary>
    bool IsLoaded { get; set; }

    /// <summary>
    /// Name of the scene this Match takes place in.
    /// </summary>
    string Scene { get; }

    /// <summary>
    /// Score/Statistics for this Match.
    /// </summary>
    MatchStats Stats { get; }

    /// <summary>
    /// The type of this match. Determines the rule set.
    /// </summary>
    GameMode GameMode { get; }

    /// <summary>
    /// Timespan this match should run (set when configuring the Match).
    /// </summary>
    int MatchTimeInSeconds { get; }

    /// <summary>
    /// Time the Match should start (set by MatchStartsAt).
    /// </summary>
    int MatchStartAtTimestamp { get; }

    /// <summary>
    /// Time the match has finished (set by MatchStartsAt and RoundStartsAt).
    /// </summary>
    int MatchFinishedAtTimestamp { get; }

    /// <summary>
    /// Time the next round should start (set by MatchStartsAt and RoundStartsAt).
    /// </summary>
    int RoundStartAtTimestamp { get; }

    /// <summary>
    /// Time the round has finished (set by MatchStartsAt and RoundStartsAt).
    /// </summary>
    int RoundFinishedAtTimestamp { get; }

    /// <summary>
    /// Number of rounds this match was played (0 during the first round).
    /// </summary>
    int RoundsStarted { get; }

    /// <summary>
    /// Whether the match is currently paused.
    /// </summary>
    bool Paused { get; }

    MatchDescription MatchDescription { get; }

    /// <summary>
    /// Not synced! To validate on every client, if the match has been started or not. RoundsStarted can be different on master and remote!
    /// </summary>
    bool MatchStarted { get; set; }

    #endregion

    #region Core

    /// <summary>
    /// Initialize match. Prepare stats and pillars. Register callbacks.
    /// </summary>
    void InitMatchOnMaster();

    /// <summary>
    /// Prepare for MatchStart because it will start in some time.
    /// </summary>
    /// <param name="startTimestamp">Photon timestamp for when the Match will start.</param>
    /// <param name="finishTimestamp">Photon timestamp for when the Match is scheduled to finish.</param>
    void StartMatchAt(int startTimestamp, int finishTimestamp);

    /// <summary>
    /// Start the Match now.
    /// </summary>
    void StartMatch();

    /// <summary>
    /// Prepare for RoundStart because next round will start in some time.
    /// </summary>
    /// <param name="startTimestamp">Photon timestamp for when the next Round will start.</param>
    /// <param name="finishTimestamp">Photon timestamp for when the next Round is scheduled to finish.</param>
    void StartNewRoundAt(int startTimestamp, int finishTimestamp);

    /// <summary>
    /// Start a new Round now.
    /// </summary>
    void StartNewRound();

    /// <summary>
    /// Finish the current round.
    /// </summary>
    void FinishRoundOnClients();

    /// <summary>
    /// Finish the match.
    /// </summary>
    void FinishMatch();

    /// <summary>
    /// Pause the match.
    /// </summary>
    void Pause();

    /// <summary>
    /// Resume the match from a pause.
    /// </summary>
    void Resume();

    /// <summary>
    /// Stop the Match and trigger cleanup.
    /// </summary>
    void StopMatch();

    #endregion

    #region Serialization

    bool Serialize(BitSerializer stream);

    #endregion

    #region Handle Player

    void GetRegisteredPlayers(out IPlayer[] players, out int count);
    int GetRegisteredPlayerCount();

    #endregion

    #region events

    /// <summary>
    /// Triggered when Match has finished Initialization, which is after the match scene was loaded.
    /// </summary>
    event MatchAction Initialized;

    /// <summary>
    /// Triggered when the start of a match was scheduled.
    /// </summary>
    event MatchTimeAction StartingAt;

    /// <summary>
    /// Triggered when the match has started.
    /// </summary>
    event MatchAction Started;

    /// <summary>
    /// Triggered when the match has finished.
    /// </summary>
    event MatchAction Finished;

    /// <summary>
    /// Triggered when the match was stopped and cleaned up.
    /// </summary>
    event MatchAction Stopped;

    /// <summary>
    /// Triggered when the start of a new round was scheduled.
    /// </summary>
    event MatchTimeAction RoundStartingAt;

    /// <summary>
    /// Triggered when a new round has started.
    /// </summary>
    event MatchAction RoundStarted;

    /// <summary>
    /// Triggered when a round has finished.
    /// </summary>
    event RoundFinishAction RoundFinished;

    /// <summary>
    /// Triggered when the <see cref="MatchStats"/> have been updated.
    /// </summary>
    event StatsAction StatsChanged;

    #endregion

    #region Debug

    // returns internal values as string to print it in DebugUI
    string PrintMatch();

    #endregion
}