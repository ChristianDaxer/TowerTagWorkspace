using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameManagement;
using JetBrains.Annotations;
using Network;
using Photon.Pun;
using ReadyTowerUI;
using Rewards;
using TowerTag;
using TowerTagSOES;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public sealed partial class GameManager : IGameManager, IDisposable
{
    private static GameManager _instance;

    [NotNull] public static GameManager Instance => _instance ?? (_instance = new GameManager());

    #region Properties

    /// <summary>
    /// Current state of GameManagerStateMachine.
    /// </summary>
    public GameManagerStateMachine.State CurrentState => _stateMachine.CurrentStateIdentifier;

    /// <summary>
    /// Is the Game in PlayMatchState (see GameManagerStateMachine.States).
    /// </summary>
    public bool IsInPlayMatchState => GameManagerStateMachine.State.PlayMatch == CurrentState;

    /// <summary>
    /// Is the Game in ConfigureState (see GameManagerStateMachine.States).
    /// </summary>
    public bool IsInConfigureState => GameManagerStateMachine.State.Configure == CurrentState;

    public bool IsInUndefinedState =>
        GameManagerStateMachine.State.Configure == GameManagerStateMachine.State.Undefined;

    /// <summary>
    /// Is the Game in LoadMatch (see GameManagerStateMachine.States).
    /// </summary>
    public bool IsInLoadMatchState => GameManagerStateMachine.State.LoadMatch == CurrentState;


    /// <summary>
    /// Current Match instance. Can be null after late join.
    /// </summary>
    public IMatch CurrentMatch
    {
        get => _currentMatch;
        private set
        {
            if (_currentMatch == value) return;
            CurrentMatch?.StopMatch();
            _currentMatch = value;
            MatchHasChanged?.Invoke(_currentMatch);
        }
    }

    public bool TrainingVsAI => CurrentHomeMatchType == HomeMatchType.TrainingVsAI;
    public HomeMatchType CurrentHomeMatchType { get; set; }

    public enum HomeMatchType
    {
        Undefined,
        TrainingVsAI,
        Custom,
        Random
    }

    public RoomConfiguration.RoomState CurrentRoomState { get; set; }

    public MatchDescription MatchDescription { get; private set; }

    /// <summary>
    /// Current MatchTimer. Use this to fetch remaining countdown or match times.
    /// </summary>
    public MatchTimer MatchTimer { get; private set; }

    public RewardController RewardController { get; private set; }

    /// <summary>
    /// Object to configure local voiceChat settings like communication channels.
    /// </summary>
    private VoiceChatPlayer VoiceChatPlayer { get; set; }

    /// <summary>
    /// Countdown time before a match starts.
    /// </summary>
    public int MatchStartCountdownTimeInSec { get; private set; }

    /// <summary>
    /// Countdown time before a new round of a match starts.
    /// </summary>
    public int RoundStartCountdownTimeInSec { get; private set; }

    /// <summary>
    /// Countdown time when resuming a match from pause.
    /// </summary>
    private int ResumeFromPauseCountdownTimeInSec { get; set; }

    /// <summary>
    /// Timespan to show the score when a round has finished.
    /// </summary>
    private int ShowRoundStatsTimeoutInSec { get; set; }

    /// <summary>
    /// Timespan to show the score when a match has finished.
    /// </summary>
    private int ShowMatchStatsTimeoutInSec { get; set; }

    /// <summary>
    /// Timespan used to compensate Network latency (used as offset for MatchStartsAt/RoundSTartsAt/ResumeAt startTimes to ensure the values arrive before the startTimes).
    /// </summary>
    public int CountdownDelay { get; private set; }

    /// <summary>
    /// Returns if the get ready countdown has started after waiting for other players
    /// </summary>
    public bool MatchCountdownRunning => _matchCountdownRunning;

    #endregion

    #region Members

    /// <summary>
    /// Serializes internal Game state and sends it to Photons Room properties.
    /// </summary>
    private readonly GameStateToRoomPropertySerializer _gameState = new GameStateToRoomPropertySerializer();

    /// <summary>
    /// StateMachine to represent internal Game states.
    /// </summary>
    private readonly GameManagerStateMachine _stateMachine = new GameManagerStateMachine();

    /// <summary>
    /// Time interval the Master serializes the internal state and sends to RoomProperties (on GameServer).
    /// </summary>
    private float _sendSerializedDataTimeout = 0.1f;

    /// <summary>
    /// Timestamp (Unity.Time.time) when we last triggered Game state serialization on Master.
    /// </summary>
    private float _timeOfLastSend;

    /// <summary>
    /// Current Match instance (synced by GameManagerStateMachine).
    /// </summary>
    private IMatch _currentMatch;

    private IPhotonService _photonService;
    private MatchSequence _basicMatchSequence;

    private bool _matchCountdownRunning;
    private Coroutine _matchCountdownCoroutine;
    private Coroutine _clientWaitForSyncRequestCoroutine;

    private bool _initialized;
    private readonly List<MatchDescription> _previouslyPlayedMatches = new List<MatchDescription>();

    #endregion // Members

    #region Events

    /// <summary>
    /// Local Event triggered when a new scene was loaded  (triggered on all clients).
    /// </summary>
    public event Action<string> SceneWasLoaded;

    /// <summary>
    /// Local Event triggered when a new match was loaded  (triggered on all clients).
    /// </summary>
    public event Action<IMatch> MatchHasFinishedLoading;

    /// <summary>
    /// Local Event triggered when an emergency event was received on local client (triggered on all clients).
    /// </summary>
    public event Action EmergencyReceived;

    /// <summary>
    /// Local Event triggered when an pause event was received on local client (triggered on all clients).
    /// </summary>
    public event Action<bool> PauseReceived;

    /// <summary>
    /// Local Event triggered when a new Match was set to GameManager.currentMatch(triggered on all clients).
    /// </summary>
    public event Action<IMatch> MatchHasChanged;

    /// <summary>
    /// Event triggered when the cumulative ready status of all players changed.
    /// </summary>
    public event Action<bool> AllPlayersReadyStatusChanged;

    public event Action<IPlayer> GameManagerAddedPlayer;

    public delegate void CountdownStartDelegate(float countdownTime);

    public event CountdownStartDelegate BasicCountdownStarted;
    public event Action BasicCountdownAborted;
    public event Action MatchConfigurationStarted;
    public event Action<IMatch> MatchFinished;
    public event Action<IMatch> MatchStarted;
    public event Action<MatchDescription, GameMode> MissionBriefingStarted;
    public event Action MatchSceneLoading;
    public event Action<IMatch> FullSyncRequestCompleted;

    #endregion

    #region Run

    private GameManager()
    {
        PlayerManager.Instance.PlayerAdded += OnPlayerAdded;
        PlayerManager.Instance.PlayerRemoved += OnPlayerRemoved;
    }

    ~GameManager()
    {
        Dispose();
    }

    /// <summary>
    /// Initialize GameManger (set singleton instance and initialize local Member & StateMachine)
    /// </summary>
    public void Init(IPhotonService photonService, MatchTimer matchTimer, RewardController rewardController,
        MatchSequence basicMatchSequence)
    {
        _photonService = photonService;
        _basicMatchSequence = basicMatchSequence;
        MatchTimer = matchTimer;
        RewardController = rewardController;

        if (VoiceChatPlayer == null)
            VoiceChatPlayer = VoiceChatPlayer.Instance;

        BalancingConfiguration configuration = BalancingConfiguration.Singleton;
        if (configuration != null)
        {
            // Countdown Timespans (Countdown at MatchStart/RoundStart/Resume from Pause)
            MatchStartCountdownTimeInSec = configuration.MatchStartCountdownTimeInSec;
            RoundStartCountdownTimeInSec = configuration.RoundStartCountdownTimeInSec;
            ResumeFromPauseCountdownTimeInSec = configuration.ResumeFromPauseCountdownTime;

            // Timespans the Match/Round Scores are displayed
            ShowRoundStatsTimeoutInSec = configuration.ShowRoundStatsTimeoutInSec;
            ShowMatchStatsTimeoutInSec = configuration.ShowMatchStatsTimeoutInSec;

            // To delay the countdown. Ensure the latency compensation is done
            CountdownDelay = configuration.CountdownDelay;

            /*
            // Button Input Axes (for Unity's Input Manager)
            _pauseButtonInputAxis = configuration.pauseButtonInputAxis;
            _emergencyStopButtonInputAxis = configuration.emergencyStopButtonInputAxis;
            */
        }

        _stateMachine.InitStateMachine(this);
        _gameState.Init(photonService);

        _initialized = true;

        TTSceneManager.Instance.ConnectSceneLoaded += OnConnectSceneLoaded;
        // if(MySceneManager.Instance.IsInConnectScene) OnConnectSceneLoaded();
    }

    /// <summary>
    /// Updates internal State.
    /// </summary>
    public void Tick()
    {
        _stateMachine.UpdateCurrentState();
        UpdateGameStateSynchronisation();
    }

    public void ChangeStateToDefault()
    {
        _stateMachine.ChangeStateToDefault();
    }

    public void ChangeStateToTutorial()
    {
        _stateMachine.ChangeState(GameManagerStateMachine.State.Tutorial);
        GameManager.Instance.ForceTutorial = false;
    }

    public void OnApplicationQuit()
    {
        SceneManager.sceneLoaded -= NewSceneWasLoaded;
    }

    #endregion

    #region Player Management

    private void OnPlayerAdded(IPlayer player)
    {
        player.ReadyStatusChanged += OnPlayerReadyStatusChanged;
        if (!_photonService.IsMasterClient) return;
        player.PlayerNetworkEventHandler.SendPillarWallState();
        if (player.IsBot)
        {
            player.SetName($"{player.PlayerName}");
        }

        // kick players that surpass total player limit
        if (PlayerManager.Instance.GetAllConnectedPlayerCount() > RoomConfiguration.GetMaxPlayersForCurrentRoom()) {
            Debug.LogWarning($"Disconnecting {player.PlayerName}: Too many players");
            player.PlayerNetworkEventHandler.SendDisconnectPlayer(
                "Too many players in the room. \nPlease wait for one to quit, or choose another room.");
            return;
        }
		
        CheckAllPlayersReadyOnMaster();
        GameManagerAddedPlayer?.Invoke(player);
    }

    private void OnPlayerRemoved(IPlayer player)
    {
        player.ReadyStatusChanged -= OnPlayerReadyStatusChanged;
        if (_photonService.IsMasterClient)
            CheckAllPlayersReadyOnMaster();
        // todo check if spectator can fill in?
    }

    private void OnPlayerReadyStatusChanged(IPlayer player, bool newValue)
    {
        if (_photonService.IsMasterClient)
            CheckAllPlayersReadyOnMaster();
    }

    public void ChangePlayerTeam(IPlayer player, TeamID team)
    {
        player.SetTeam(team);
        Pillar pillar = PillarManager.Instance.FindSpawnPillarForPlayer(player);
        // if the target team is full, the player is temporarily moved to a virtual pillar.
        if (pillar == null)
        {
            pillar = PillarManager.Instance.GetAllPillars()
                .First(p => p.AllowTeleportWithoutTeamMatch && p.Owner == null);
            Debug.Log($"Temporarily teleporting player {player} onto virtual pillar {pillar}");
            TeleportHelper.TeleportPlayerRequestedByGame(
                player, pillar, TeleportHelper.TeleportDurationType.Immediate);
            StaticCoroutine.StartStaticCoroutine(RespawnPlayer(player, 0.1f));
        }
        else
        {
            TeleportHelper.RespawnPlayerOnSpawnPillar(
                player, TeleportHelper.TeleportDurationType.Immediate);
        }
    }

    /// <summary>
    /// Coroutine that teleports the player to a spawn pillar as soon as one is available.
    /// </summary>
    /// <param name="player">The player to respawn</param>
    /// <param name="intervalSeconds">Time in seconds between attempts</param>
    private static IEnumerator RespawnPlayer([NotNull] IPlayer player, float intervalSeconds = 1f)
    {
        while (player.CurrentPillar == null || !player.CurrentPillar.IsSpawnPillar)
        {
            Pillar freeSpawnPillar = PillarManager.Instance.FindSpawnPillarForPlayer(player);
            if (freeSpawnPillar == null)
                yield return new WaitForSeconds(intervalSeconds);
            else
            {
                TeleportHelper.RespawnPlayerOnSpawnPillar(
                    player, TeleportHelper.TeleportDurationType.Immediate);
                break;
            }
        }
    }

    public void CheckAllPlayersReadyOnMaster()
    {
        if (!_photonService.IsMasterClient) return;

        PlayerManager.Instance.GetParticipatingHumanFirePlayers(out var icePlayers, out var iceCount);
        PlayerManager.Instance.GetParticipatingHumanIcePlayers(out var firePlayers, out var fireCount);

        bool allReady = fireCount > 0;
        allReady &= iceCount > 0;
        allReady &= icePlayers.Take(iceCount).All(p => p.PlayerIsReady) && firePlayers.Take(fireCount).All(p => p.PlayerIsReady);

        if ((TowerTagSettings.BasicMode || SharedControllerType.IsAdmin && AdminController.Instance.UserVote)
            && allReady && !MatchCountdownRunning)
        {
            StartMatchCountdown(TowerTagSettings.BasicModeStartMatchCountdownTime);
        }

        if ((!allReady || PlayerManager.Instance.GetAllParticipatingHumanPlayerCount() <= 0)
            && MatchCountdownRunning && (TowerTagSettings.BasicMode || SharedControllerType.IsAdmin
                && AdminController.Instance.UserVote))
        {
            AbortMatchCountdown();
        }

        if (SharedControllerType.IsAdmin && !AdminController.Instance.UserVote && MatchCountdownRunning)
            AbortMatchCountdown();

        AllPlayersReadyStatusChanged?.Invoke(allReady);
    }

    public bool CheckIfBasicMatchIsStartable() {
        PlayerManager.Instance.GetParticipatingHumanFirePlayers(out var icePlayers, out var iceCount);
        PlayerManager.Instance.GetParticipatingHumanIcePlayers(out var firePlayers, out var fireCount);

        bool allReady = fireCount > 0;
        allReady &= iceCount > 0;
        allReady &= icePlayers.Take(iceCount).All(p => p.PlayerIsReady) && firePlayers.Take(fireCount).All(p => p.PlayerIsReady);

        if (TowerTagSettings.BasicMode && allReady && !MatchCountdownRunning) {
            return true;
        }

        return false;
    }

    public void StartMatchCountdown(float countdownTime)
    {
        _matchCountdownRunning = true;
        _matchCountdownCoroutine = StaticCoroutine.StartStaticCoroutine(StartMatchCountdownCoroutine(countdownTime));
        BasicCountdownStarted?.Invoke(countdownTime);
    }

    public void AbortMatchCountdown()
    {
        _matchCountdownRunning = false;
        if (_matchCountdownCoroutine != null) StaticCoroutine.StopStaticCoroutine(_matchCountdownCoroutine);
        BasicCountdownAborted?.Invoke();
    }

    public void StartMatch()
    {
        if (TowerTagSettings.Home)
            StartHomeMatch();
        else
            StartBasicMatch();
    }

    private IEnumerator StartMatchCountdownCoroutine(float countdownTime)
    {
        yield return new WaitForSeconds(countdownTime);
        _matchCountdownCoroutine = null;
        StartMatch();
    }

    public void StartBasicMatch()
    {
        if (_matchCountdownCoroutine != null)
        {
            StaticCoroutine.StopStaticCoroutine(_matchCountdownCoroutine);
            _matchCountdownCoroutine = null;
        }

        Debug.Log("Starting next viable match from basic match sequence");
        if (!_photonService.IsMasterClient) return;

        var list = VotingObserver.VotedGameModes;
        var gameMode = list.Count > 0 ? list[Random.Range(0, list.Count)] : GameMode.UserVote;
        MatchDescription matchDescription = _basicMatchSequence.Next(PlayerManager.Instance.GetParticipatingPlayersCount(),
            gameMode);

        if (matchDescription == null)
        {
            Debug.LogError("Finished countdown, but cannot start match. No viable match in sequence.");
            AbortMatchCountdown();
            return;
        }

        if (TowerTagSettings.BasicMode)
            BalancingConfiguration.Singleton.MatchTimeInSeconds = TowerTagSettings.BasicModeMatchTime;
        TriggerMissionBriefingOnMaster(matchDescription, gameMode);
    }

    [UsedImplicitly]
    public void StartHomeMatch()
    {
        if (_matchCountdownCoroutine != null)
        {
            StaticCoroutine.StopStaticCoroutine(_matchCountdownCoroutine);
            _matchCountdownCoroutine = null;
        }

        Debug.Log("Starting next viable match from basic match sequence");
        if (!_photonService.IsMasterClient) return;

        MatchDescription matchDescription = null;
        GameMode gameMode = GameMode.UserVote;
        var currentRoomCustomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        if (currentRoomCustomProperties.ContainsKey(RoomPropertyKeys.GameMode))
            gameMode = (GameMode) currentRoomCustomProperties[RoomPropertyKeys.GameMode];
        if (CurrentHomeMatchType == HomeMatchType.Custom && gameMode != GameMode.UserVote)
        {
            string map = "";
            if (currentRoomCustomProperties.ContainsKey(RoomPropertyKeys.Map))
                map = (string) currentRoomCustomProperties[RoomPropertyKeys.Map];

            if (!string.IsNullOrEmpty(map))
                matchDescription = MatchDescriptionCollection.Singleton.GetMatchDescription(gameMode, map);
        }
        else
        {
            var list = VotingObserver.VotedGameModes;
            gameMode = list.Count > 0 ? list[Random.Range(0, list.Count)] : GetRandomGameMode();
            matchDescription = ChooseRandomMatch(desc => desc.GameMode.HasFlag(gameMode));
        }

        if (matchDescription == null)
        {
            Debug.LogError("Finished countdown, but cannot start match. No viable match in sequence.");
            AbortMatchCountdown();
            return;
        }

        if (TowerTagSettings.BasicMode)
            BalancingConfiguration.Singleton.MatchTimeInSeconds = TowerTagSettings.BasicModeMatchTime;
        TriggerMissionBriefingOnMaster(matchDescription, gameMode);
    }

    private GameMode GetRandomGameMode()
    {
        int rnd = Random.Range(1, 3);
        switch (rnd)
        {
            case 1:
                return GameMode.Elimination;
            case 2:
                return GameMode.DeathMatch;
            case 3:
                return GameMode.GoalTower;
            default:
                return GameMode.Elimination;
        }
    }

    public MatchDescription ChooseRandomMatch(Predicate<MatchDescription> filter)
    {
        if (MatchDescriptionCollection.Singleton == null) {
            Debug.LogErrorFormat("Unable to choose random match, invalid reference to: {0} for component: {1} attached to GameObject: \"{2}\".", nameof(MatchDescriptionCollection), nameof(GameManager));
            return null;
        }

        var matchDescriptionCollection = MatchDescriptionCollection.Singleton;
        if (matchDescriptionCollection._matchDescriptions == null || matchDescriptionCollection._matchDescriptions.Length == 0) { 
            Debug.LogErrorFormat("Unable to choose random match, {0} is empty.", nameof(MatchDescriptionCollection));
            return null;
        }

        MatchDescription[] matchDescriptions = matchDescriptionCollection._matchDescriptions
            .Where(IsMatchStartable)
            .Where(desc => filter(desc))
            .ToArray();

        if (matchDescriptions.Length == 0)
            return null;

        // filter out previously played matches. Clear list, if none left
        if (matchDescriptions.All(desc => _previouslyPlayedMatches.Contains(desc)))
            _previouslyPlayedMatches.Clear();
        else
            matchDescriptions = matchDescriptions.Where(desc => !_previouslyPlayedMatches.Contains(desc)).ToArray();

        MatchDescription matchDescription = matchDescriptions[Random.Range(0, matchDescriptions.Length - 1)];
        return matchDescription;
    }

    private bool IsMatchStartable(MatchDescription matchDescription)
    {
        if (matchDescription == null) {
            Debug.LogError("Cannot start NULL match description.");
            return false;
        }
        if (CurrentState != GameManagerStateMachine.State.Configure) return false;
        if (TeamManager.Singleton.TeamIce.GetPlayerCount() > matchDescription.MatchUp.MaxPlayers / 2) return false;
        if (TeamManager.Singleton.TeamFire.GetPlayerCount() > matchDescription.MatchUp.MaxPlayers / 2) return false;

        // Check min Player on non custom or user vote  matches
        GameMode gameMode = GameMode.UserVote;
        var currentRoomCustomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        if (currentRoomCustomProperties.ContainsKey(RoomPropertyKeys.GameMode))
            gameMode = (GameMode) currentRoomCustomProperties[RoomPropertyKeys.GameMode];
        if (CurrentHomeMatchType == HomeMatchType.Custom && gameMode != GameMode.UserVote) return true;
        if (PlayerManager.Instance.GetParticipatingPlayersCount() < matchDescription.MinPlayers) return false;
        return true;
    }

    //Don't see an other way to synchronize the match timer and the timestamps to the late joiner
    private GameManagerPhotonView gameManagerPhotonView;
    public void SyncToLateJoiner(IPlayer player) {

        if (gameManagerPhotonView == null)
        {
            if (!GameManagerPhotonView.GetInstance(out gameManagerPhotonView))
                return;
        }

        gameManagerPhotonView.NetworkEventHandler
            .SendCurrentMatchTimerToLateJoiner(player, MatchTimer.MatchStartAtServerTimestamp,
                MatchTimer.MatchEndAtServerTimestamp, MatchTimer.CountdownTimeInSeconds);
    }

    #endregion

    #region Match Control

    /// <summary>
    /// Set new match (on remote clients) when received by StateMachine serialization.
    /// </summary>
    /// <param name="match">The Match we received by StateMachine serialization.</param>
    private void SetMatch(IMatch match)
    {
        if (CurrentMatch != null)
        {
            CurrentMatch.StatsChanged -= OnStatsChanged;
        }

        CurrentMatch = match;
        if (CurrentMatch != null)
        {
            CurrentMatch.StatsChanged += OnStatsChanged;
        }
    }

    /// <summary>
    /// Load the Match scene and wait until scene load finished.
    /// </summary>
    private void LoadCurrentMatch()
    {
        if (CurrentMatch == null)
        {
            Debug.LogError("Failed to load match scene: No current match");
            return;
        }

        if (_photonService.IsMasterClient)
        {
            _previouslyPlayedMatches.Add(CurrentMatch.MatchDescription);
            PlayerManager.Instance.GetAllParticipatingHumanPlayers(out var players, out var count);
            for (int i = 0; i < count; i++)
                players[i].PlayerIsReady = false;
        }

        // block syncing (so StateMachine does not change State while we load the scene)
        MatchSceneLoading?.Invoke();
        CurrentMatch.IsLoaded = true;
        _stateMachine.BlockIncomingSerialization(true);
        SceneManager.sceneLoaded += NewSceneWasLoaded;
        ActivateAllPlayersOnMaster(false, false);
        TTSceneManager.Instance.LoadScene(CurrentMatch.Scene);

        AnalyticsController.LoadMatch(
            ConfigurationManager.Configuration.Room,
            CurrentMatch.GetRegisteredPlayerCount(),
            CurrentMatch.MatchTimeInSeconds / 60,
            CurrentMatch.Scene,
            SharedControllerType.Singleton.Value.ToString()
        );
    }

    /// <summary>
    /// Initialize the current match.
    /// Updates the <see cref="VoiceChatPlayer"/>, activates and updates players.
    /// Finally, calls <see cref="IMatch.InitMatchOnMaster"/>.
    /// </summary>
    private void InitializeMatch()
    {
        Debug.Log("Initializing Match");

        // q+d hack to check if PlayerManager already synced & filled up with other connected players
        // Check if player joined Match (not Master) & check connected players...should be more then 1 (me) player in room
        if (!_photonService.IsMasterClient && PlayerManager.Instance.GetAllConnectedPlayerCount() <= 1) {
            if (_clientWaitForSyncRequestCoroutine == null) {
                _clientWaitForSyncRequestCoroutine = StaticCoroutine.StartStaticCoroutine(WaitForFullSyncRequest());
                return;
            }
        }

        if (_clientWaitForSyncRequestCoroutine != null)
        {
            StaticCoroutine.StopStaticCoroutine(_clientWaitForSyncRequestCoroutine);
            _clientWaitForSyncRequestCoroutine = null;
        }

        ActivateAllPlayersOnMaster(true, true);

        PlayerManager.Instance.GetAllConnectedPlayers(out var players, out var count);
        for (int i = 0; i < count; i++)
            players[i].InitPlayerFromPlayerProperties();

        if (VoiceChatPlayer.IsInitialized)
            VoiceChatPlayer.ChangeConversationGroups(VoiceChatPlayer.ChatType.TalkInTeam);
        
        if (_photonService.IsMasterClient) CurrentMatch.InitMatchOnMaster();
        MatchHasFinishedLoading?.Invoke(CurrentMatch);
    }

    private IEnumerator WaitForFullSyncRequest() {
        yield return new WaitUntil(() => PlayerManager.Instance.GetAllConnectedPlayerCount() > 1);
        _clientWaitForSyncRequestCoroutine = null;
		FullSyncRequestCompleted?.Invoke(CurrentMatch);
        InitializeMatch();
    }

    /// <summary>
    /// Start a new round of the current match.
    /// Updates the <see cref="MatchTimer"/> to reflect the start and end timestamps.
    /// If the countdown time equals or exceeds the remaining match time, the match is ended prematurely.
    /// Calls <see cref="IMatch.StartMatchAt"/>, if no rounds were played, yet.
    /// Calls <see cref="IMatch.StartNewRoundAt"/> otherwise.
    /// </summary>
    /// <param name="startTimestamp">Photon timestamp for when the round starts</param>
    /// <param name="endTimestamp">Photon timestamp for when the match ends</param>
    /// <param name="countdownTimeInSeconds">Length of the countdown in seconds before the round starts</param>
    private void StartNewRoundAt(int startTimestamp, int endTimestamp, int countdownTimeInSeconds)
    {
        if (CurrentMatch == null)
        {
            Debug.LogError("Cannot start new round: No current match");
            return;
        }

        MatchTimer.StartTimerAt(startTimestamp, endTimestamp, countdownTimeInSeconds);

        if (CurrentMatch.RoundsStarted == 0)
        {
            CurrentMatch.StartMatchAt(startTimestamp, endTimestamp);

            if (_photonService.IsMasterClient)
            {
                AnalyticsController.StartMatch(
                    ConfigurationManager.Configuration.Room,
                    PlayerManager.Instance.GetParticipatingPlayersCount(),
                    BalancingConfiguration.Singleton.MatchAutoStart,
                    BalancingConfiguration.Singleton.MatchTimeInSeconds / 60,
                    CurrentMatch.Scene,
                    CurrentMatch.GameMode.ToString()
                );
            }
        }
        else
        {
            ShotManager.Singleton.Shots.ForEach(shot => ShotManager.Singleton.DestroyShot(shot.ID));
            CurrentMatch.StartNewRoundAt(startTimestamp, endTimestamp);
        }
    }

    private void ResumeMatch() {
        if (_photonService.IsMasterClient) {
            PlayerManager.Instance.GetSpectatingPlayers(out var players, out var count);
            for (int i = 0; i < count; i++)
                players[i].PlayerStateHandler.SetPlayerStateOnMaster(PlayerState.DeadButNoLimbo);
        }

        if (!CurrentMatch.MatchStarted)
        {
            CurrentMatch.StartMatch();
        }
        else if (CurrentMatch.Paused)
            CurrentMatch.Resume();
        else
            CurrentMatch.StartNewRound();
    }

    private void FinishRoundOnClients()
    {
        if (!_photonService.IsMasterClient)
            CurrentMatch?.FinishRoundOnClients();
    }

    /// <summary>
    /// Handle  OnStatsChanged event (from Match) -> Trigger Serialize to send Match/MatchStats to remote clients.
    /// This function should only get called on Master client (is ignored on remote clients).
    /// </summary>
    /// <param name="stats">The new MatchStats (which have changed).</param>
    private void OnStatsChanged(MatchStats stats)
    {
        if (_photonService.IsMasterClient)
        {
            _stateMachine.IsDirty = true;
        }
    }

    private void FinishMatch()
    {
        ActivateAllPlayersOnMaster(false, false);
        MatchTimer.StopTimer();
        CurrentMatch?.FinishMatch();
        PreviousMatchResults.SaveMatchStats(CurrentMatch);
        if (CurrentMatch == null) return;
        if (_photonService.IsMasterClient) {
            if (PlayerManager.Instance.GetAllParticipatingHumanPlayerCount() > 0) {
                MatchStats currentMatchStats = CurrentMatch.Stats;
                AnalyticsController.FinishMatch(
                    ConfigurationManager.Configuration.Room,
                    CurrentMatch.GetRegisteredPlayerCount(),
                    BalancingConfiguration.Singleton.MatchTimeInSeconds / 60,
                    currentMatchStats.RoundsStarted,
                    PlayerManager.Instance.GetAllParticipatingAIPlayerCount(),
                    CurrentMatch.Scene,
                    CurrentMatch.GameMode.ToString()
                );
            }
            else if (PlayerManager.Instance.GetAllParticipatingHumanPlayerCount() <= 0) {
                MatchStats currentMatchStats = CurrentMatch.Stats;
                AnalyticsController.FinishBotMatch(
                    ConfigurationManager.Configuration.Room,
                    CurrentMatch.GetRegisteredPlayerCount(),
                    BalancingConfiguration.Singleton.MatchAutoStart,
                    BalancingConfiguration.Singleton.MatchTimeInSeconds / 60,
                    currentMatchStats.RoundsStarted,
                    CurrentMatch.Scene,
                    CurrentMatch.GameMode.ToString()
                );
            }
        }
        else if (SharedControllerType.VR)
        {
            IPlayer ownPlayer = PlayerManager.Instance.GetOwnPlayer();
            AnalyticsController.PlayerFinishedMatch(
                CurrentMatch.MatchTimeInSeconds,
                CurrentMatch.RoundsStarted,
                CurrentMatch.Scene,
                CurrentMatch.GameMode.ToString(),
                ConfigurationManager.Configuration.Room,
                ownPlayer?.IsLoggedIn ?? false
            );
            if (TowerTagSettings.Home && TrainingVsAI)
            {
                Dictionary<int, PlayerStats> playerStats = CurrentMatch.Stats.GetPlayerStats();
                if (ownPlayer != null)
                {
                    AnalyticsController.TrainingMatchFinished(
                        playerStats[ownPlayer.PlayerID].Kills,
                        playerStats[ownPlayer.PlayerID].Deaths,
                        CurrentMatch.Stats.WinningTeamID == ownPlayer.TeamID,
                        CurrentMatch.MatchDescription.MapName,
                        CurrentMatch.GameMode.ToString()
                    );
                }
            }
        }
    }

    /// <summary>
    /// Trigger Pause state on Master (is synced automatically to clients by StateMachine).
    /// - if you want to Pause (setPause parameter is true)
    ///   - Can only be triggered if we are in the PlayMatchState (to check this use GameManager.IsInPlayMatchState), call is ignored otherwise.
    ///   - Please check MatchTimer.IsPausingAllowed before calling SetPause(true) (because it will be ignored if timer.IsPausingAllowed is false).
    /// - if you want to resume (setPause parameter is false)
    ///   - Can only be triggered if we are in the PauseState (to check this use GameManager.IsPaused), call is ignored otherwise.
    ///   - Please check MatchTimer.IsResumingAllowed before calling SetPause(false) (because it will be ignored if timer.IsResumingAllowed is false).
    /// Can only get called on Master client (will be ignored (with errormessage) when called on non Master client clients).
    /// </summary>
    /// <param name="setPause">True if you want to pause the current Match, false to resume.</param>
    public void SetPauseOnMaster(bool setPause)
    {
        if (!_photonService.IsMasterClient)
        {
            Debug.LogWarning("Tried to trigger pause on non-master client");
            return;
        }

        if (CurrentState == GameManagerStateMachine.State.PlayMatch
            || CurrentState == GameManagerStateMachine.State.Paused)
        {
            Debug.Log(setPause ? "Pausing match" : "Resuming match");
            _stateMachine.SetPauseMatch(setPause);
        }
        else
        {
            Debug.LogWarning("It's not possible to pause because the game is not in the right state, current state: " +
                             CurrentState);
        }
    }

    /// <summary>
    /// Trigger emergency stop on Master (is synced automatically to clients by StateMachine).
    /// Can only get called on Master client (will be ignored (with errormessage) when called on non Master client clients).
    /// </summary>
    public void SetEmergencyStateOnMaster()
    {
        if (!_photonService.IsMasterClient)
        {
            Debug.LogWarning("Triggered Emergency on non-master client");
            return;
        }

        Debug.LogWarning("Triggered Emergency");
        _stateMachine.ChangeState(GameManagerStateMachine.State.Emergency);
    }

    public void TriggerAbortMissionBriefingOnMaster()
    {
        if (!_photonService.IsMasterClient)
        {
            Debug.LogWarning("Tried to abort mission briefing on non-master client");
            return;
        }

        if (CurrentState != GameManagerStateMachine.State.MissionBriefing)
        {
            Debug.LogWarning($"Cannot abort mission briefing in state {CurrentState}");
        }

        Debug.Log("Aborting mission briefing");
        _stateMachine.ChangeState(GameManagerStateMachine.State.Configure);
    }

    /// <summary>
    /// Call this to load the Match (is synced automatically to clients by StateMachine).
    /// Can only get called on Master client (will be ignored (with error message) when called on non Master client clients).
    /// </summary>
    public void TriggerMissionBriefingOnMaster(MatchDescription matchDescription, GameMode mode)
    {
        if (!_photonService.IsMasterClient)
        {
            Debug.LogWarning("Tried to start mission briefing on non-master client");
            return;
        }

        if (!IsMatchStartable(matchDescription))
        {
            Debug.LogError($"Cannot start match {matchDescription}");
            return;
        }

        if (!IsInConfigureState)
        {
            Debug.LogWarning($"Cannot start mission briefing from state {CurrentState}");
            return;
        }

        Debug.Log($"Starting mission briefing for match {matchDescription}");

        MatchDescription = matchDescription;
        SetMatch(MatchConfigurator.CreateMatch(matchDescription, mode, _photonService));
        _stateMachine.ChangeState(GameManagerStateMachine.State.MissionBriefing);
    }

    private void StartMissionBriefing(GameMode mode)
    {
        Debug.Log("Starting mission briefing");
        MissionBriefingStarted?.Invoke(MatchDescription, mode);
    }

    /// <summary>
    /// The state machine has gone to PauseState or resumed from it (through serialization from Master client), so we have to trigger a local event to inform all interested components
    /// (see Add-/RemoveOnPauseReceivedEventListener).
    /// </summary>
    /// <param name="paused">True if we gone to PauseState, false if we have gone back to PlayMatchState.</param>
    private void OnPauseReceived(bool paused)
    {
        if (paused)
            CurrentMatch.Pause();
        PauseReceived?.Invoke(paused);
    }

    /// <summary>
    ///  The state machine has gone to EmergencyState (through serialization from Master client), so we have to trigger a local event to inform all interested components
    ///  (see Add-/RemoveOnEmergencyReceivedEventListener).
    /// </summary>
    private void OnEmergencyReceived()
    {
        Debug.LogWarning("Emergency Triggered");
        EmergencyReceived?.Invoke();
    }

    /// <summary>
    /// Change Player state of all registered players from Master client.
    /// </summary>
    /// <param name="isGunEnabled">Set true to activate the gun (to shoot/claim and teleport), false otherwise.</param>
    /// <param name="isMortal">Set true to deactivate Damage handling, false otherwise.</param>
    private void ActivateAllPlayersOnMaster(bool isGunEnabled, bool isMortal) {
        if (_photonService.IsMasterClient) {

            PlayerManager.Instance.GetParticipatingPlayers(out var players, out var count);
            for (int i = 0; i < count; i++)
                players[i].PlayerStateHandler.SetPlayerStateOnMaster(new PlayerState(!isMortal, !isGunEnabled, false));
        }
    }

    /// <summary>
    /// Is the Match Paused (is the StateMachine in PauseState)?
    /// </summary>
    /// <returns>True if paused, false otherwise.</returns>
    public bool IsPaused()
    {
        return _stateMachine.IsPaused();
    }

    #endregion

    #region Scene Management

    public void TriggerMatchConfigurationOnMaster()
    {
        if (_photonService.IsMasterClient
            && _stateMachine.CurrentStateIdentifier != GameManagerStateMachine.State.Configure)
        {
            _stateMachine.ConfigureMatch();
        }
    }

    /// <summary>
    /// Load the hub scene.
    /// If called outside the configure state, triggers a transition of the state machine, to sync with all clients.
    /// Otherwise proceeds loading the hub scene.
    /// </summary>
    private void ConfigureMatch()
    {
        CurrentMatch?.StopMatch();
        MatchTimer.StopTimer();
        if (TTSceneManager.GetInstance(out var sceneManager) && !sceneManager.IsInHubScene)
        {
            // ActivateAllPlayersOnMaster(false, false);
            _stateMachine.BlockIncomingSerialization(true);
            SceneManager.sceneLoaded += NewSceneWasLoaded;
            sceneManager.LoadHubScene();

            AnalyticsController.LoadHub(
                ConfigurationManager.Configuration.Room,
                PlayerManager.Instance.GetAllConnectedPlayerCount(),
                sceneManager.CurrentHubScene,
                SharedControllerType.Singleton.Value.ToString()
            );
        }
        else
        {
            // ActivateAllPlayersOnMaster(true, true);
        }

        // Late joiners become spectators, if match is full. In basic, all players should participate now.
        if (_photonService.IsMasterClient && (TowerTagSettings.BasicMode || TowerTagSettings.Home)) {
            PlayerManager.Instance.GetAllConnectedPlayers(out var players, out var count);
            for (int i = 0; i < count; i++)
                players[i].IsParticipating = true;
        }

        MatchConfigurationStarted?.Invoke();
    }

    public void LoadOffboarding()
    {
        if (_photonService.IsMasterClient
            && _stateMachine.CurrentStateIdentifier != GameManagerStateMachine.State.Offboarding)
        {
            _stateMachine.LoadOffboarding();
            return;
        }

        SceneManager.sceneLoaded += NewSceneWasLoaded;
    }

    /// <summary>
    /// Init scene when it has finished loading.
    /// </summary>
    /// <param name="newScene">Scene that was loaded by Unity.</param>
    /// <param name="mode">Mode Unity loaded this scene with.</param>
    private void NewSceneWasLoaded(Scene newScene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= NewSceneWasLoaded;
        _stateMachine.BlockIncomingSerialization(false);
        // now is a good time to collect garbage
        Resources.UnloadUnusedAssets();
        GC.Collect();

        // loaded match scene, todo: use MySceneManager event instead
        if (CurrentMatch != null && newScene.name == CurrentMatch.Scene)
        {
            InitializeMatch();
        }

        // loaded hub scene, todo: use MySceneManager event instead
        if (newScene.name == TTSceneManager.Instance.CurrentHubScene)
        {
            InitializeHub(newScene);
        }
    }

    //Todo: PLEASE PLEASE change this so the state changes triggers the scene load
    private void OnConnectSceneLoaded()
    {
        CurrentMatch?.StopMatch();
        _stateMachine.ChangeState(GameManagerStateMachine.State.Undefined);
        MatchTimer.StopTimer();
    }

    private void InitializeHub(Scene newScene)
    {
        HubSceneBehaviour.SetUpHub();

        Debug.Log("Initializing Hub");
        if (VoiceChatPlayer.IsInitialized) VoiceChatPlayer.ChangeConversationGroups(VoiceChatPlayer.ChatType.TalkToAll);
        ActivateAllPlayersOnMaster(true, true);

        PlayerManager.Instance.GetAllConnectedPlayers(out var players, out var count);
        for (int i = 0; i < count; i++)
            players[i].InitPlayerFromPlayerProperties();

        _matchCountdownRunning = false;
        SceneWasLoaded?.Invoke(newScene.name);
    }

    #endregion

    #region ClientToMaster communication

    /// <summary>
    /// A client send us the Info that he is in Sync with the Master. Used For PlayerSyncBarriers to wait till all Players are in Sync.
    /// </summary>
    /// <param name="matchID">The match ID for which the player reportedly loaded the match scene</param>
    /// <param name="player">PhotonPlayer who send the message.</param>
    public void OnReceivedPlayerSyncInfoOnMaster(int matchID, IPlayer player)
    {
        _stateMachine.OnReceivedPlayerSyncInfo(matchID, player);
    }

    /// <summary>
    /// A client send us the Info that he loaded the scene successfully.
    /// </summary>
    /// <param name="scene">Scene that was loaded on remote client.</param>
    /// <param name="playerID">Player ID of the sending player</param>
    public void OnReceivedOnSceneLoadedOnMaster(string scene, int playerID)
    {
        if (!_photonService.IsMasterClient)
            return;

        if (SceneManager.GetActiveScene().name.Equals(scene))
        {
            IPlayer towerTagPlayer = PlayerManager.Instance.GetPlayer(playerID);
            towerTagPlayer?.ResetPlayerHealthOnMaster();
        }
    }

    #endregion

    #region Synchronisation

    /// <summary>
    /// Serialize the current GameManager State.
    /// </summary>
    /// <param name="stream">BitSerializerStream to write to/read from.</param>
    /// <returns>True if the read/write process was successful, false if we have nothing new to Serialize or if error occured on read/write process.</returns>
    public bool Serialize(BitSerializer stream)
    {
        //UnityEngine.Debug.Log("GameManager.Serialize: " + ((stream.isReading) ? "isReading" : "isWriting" + " isDirty: " + _gameManagerStateMachine.isDirty));

        if (stream.IsWriting && !_stateMachine.IsDirty)
            return false;

        // if reading or dirty -> Sync State machine
        return _stateMachine.Serialize(stream);
    }

    /// <summary>
    /// Synchronize the Master client's GameState to clients.
    /// </summary>
    private void UpdateGameStateSynchronisation()
    {
        // decide if it's time to trigger next serialization to sync clients
        if (Time.time - _timeOfLastSend > _sendSerializedDataTimeout)
        {
            _timeOfLastSend = Time.time;
            _gameState.SendStateFromMasterClient();
        }
    }

    #endregion

    #region Debug

    /// <summary>
    /// Print State internals to string (and to console if printToLog is true).
    /// </summary>
    /// <param name="printToLog">Should the returned string also printed to console/logFile?</param>
    /// <returns>String with internal members to view in DebugUI.</returns>
    public string PrintCurrentState(bool printToLog)
    {
        if (!_initialized) return " - ";
        return _stateMachine.PrintCurrentState(printToLog);
    }

    /// <summary>
    /// This Method is only for testing purpose!!! Don't use it in production code!!!
    /// </summary>
    [Obsolete("This Method is only for testing purpose!!! Don't use it in production code!!!")]
    public void ChangeState(GameManagerStateMachine.State state)
    {
        _stateMachine.ChangeState(state);
    }

    public bool IsStateMachineInMatchState()
    {
        return _stateMachine.IsInMatchState();
    }

    public void Dispose()
    {
        PlayerManager.Instance.PlayerAdded -= OnPlayerAdded;
        PlayerManager.Instance.PlayerRemoved -= OnPlayerRemoved;
    }

    #endregion

    public void StartTutorial(bool independentFromPlayerPrefs)
    {
        ConfigurationManager.Configuration.TeamID = 1;
        CurrentMatch = null;
        ForceTutorial = independentFromPlayerPrefs;
        PhotonNetwork.CreateRoom(Guid.NewGuid().ToString(), RoomConfiguration.GetTutorialRoomSettings());
    }
	
	public void StartOnlyLeaderboards()
    {
        ConfigurationManager.Configuration.TeamID = 1;
        CurrentMatch = null;
        //PhotonNetwork.CreateRoom(Guid.NewGuid().ToString(), RoomConfiguration.GetEmptyRoomSettings());
    }

	public void StartMatchmaking()
	{
		ConnectionManager.Instance.JoinedLobby += (ConnectionManager connectionManager) =>
		{
			ConfigurationManager.Configuration.Room = PlayerProfileManager.CurrentPlayerProfile.PlayerGUID;

			RoomConfiguration.RoomOptions.AutostartEnabled = true;
			RoomConfiguration.RoomOptions.IsOpen = true;
			RoomConfiguration.RoomOptions.IsVisible = true;
			ConfigurationManager.Configuration.Room = "TestAutojoin";

			connectionManager.StartMatchmaking();
		};
	}

	public void TriggerMatchFinishedEvent(IMatch match) {
        MatchFinished?.Invoke(match);
    }

    public void TriggerMatchStartedEvent(IMatch match)
    {
        MatchStarted?.Invoke(match);
    }

    public bool ForceTutorial { get; private set; }
}