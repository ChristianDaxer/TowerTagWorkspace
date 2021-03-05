using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using OperatorCamera;
using TMPro;
using TowerTag;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SpectatorUiController : MonoBehaviour
{
    [SerializeField] private float _pauseMatchTimeBlinkFrequency = 0.5f;

    #region UI Elements

    [Header("UI Elements")] [SerializeField]
    private TMP_Text _timeText;

    [SerializeField] private TMP_Text _teamFireScoreText;
    [SerializeField] private TMP_Text _teamIceScoreText;
    [SerializeField] private Slider _teamFirePillarShareSlider;
    [SerializeField] private Slider _teamIcePillarShareSlider;
    [SerializeField] private TMP_Text _teamFireNameText;
    [SerializeField] private TMP_Text _teamIceNameText;
    [SerializeField] private Transform _teamFireController;
    [SerializeField] private Transform _teamIceController;


    [Header("UI Prefabs")] [SerializeField]
    private GameObject _spectatorPlayerLinePrefab;

    #endregion

    #region Properties

    private bool MatchTimeVisible
    {
        get => _timeText.enabled;
        set => _timeText.enabled = value;
    }

    #endregion

    #region private fields

    private float _targetTime;

    private readonly Dictionary<IPlayer, PlayerLineSpectatorController> _playerLineSpectatorControllers =
        new Dictionary<IPlayer, PlayerLineSpectatorController>();

    private IPlayer _focusedPlayer;
    private Color _fireTeamColor;
    private Color _iceTeamColor;

    #endregion

    private void Awake()
    {
        _fireTeamColor = TeamManager.Singleton.TeamFire.Colors.UI;
        _iceTeamColor = TeamManager.Singleton.TeamIce.Colors.UI;
    }

    private void OnEnable()
    {
        PlayerManager.Instance.PlayerAdded += OnPlayerAdded;
        PlayerManager.Instance.PlayerRemoved += OnPlayerRemoved;
        TeamManager.Singleton.TeamFire.NameChanged += TeamFireNameChanged;
        TeamManager.Singleton.TeamIce.NameChanged += TeamIceNameChanged;
        GameManager.Instance.MatchHasFinishedLoading += MatchHasFinishedLoading;
        GameManager.Instance.SceneWasLoaded += SceneHasFinishedLoading;
        GameManager.Instance.PauseReceived += PauseMatch;
        CameraManager.Instance.PlayerToFocusChanged += OnPlayerToFocusChanged;
        SceneManager.sceneLoaded += OnSceneLoaded;
        MatchTimer.CurrentTimerStateChanged += OnCurrentMatchTimerStateChanged;
    }


    private void Start()
    {
        ResetAllStats();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            MessageQueue.Singleton.AddYesNoMessage(
                "This will disconnect you and abort any running match.",
                "Are You Sure?",
                null,
                null,
                "OK",
                LoadMainMenu,
                "CANCEL");
        }

        UpdateTime();
    }

    /// <summary>
    /// Disconnect the spectator and load find match ui.
    /// </summary>
    private void LoadMainMenu()
    {
        ConnectionManager.Instance.LeaveRoom();
    }

    private void OnDisable()
    {
        PlayerManager.Instance.PlayerAdded -= OnPlayerAdded;
        PlayerManager.Instance.PlayerRemoved -= OnPlayerRemoved;
        TeamManager.Singleton.TeamFire.NameChanged -= TeamFireNameChanged;
        TeamManager.Singleton.TeamIce.NameChanged -= TeamIceNameChanged;
        GameManager.Instance.MatchHasFinishedLoading -= MatchHasFinishedLoading;
        GameManager.Instance.SceneWasLoaded -= SceneHasFinishedLoading;
        GameManager.Instance.PauseReceived -= PauseMatch;
        if (CameraManager.Instance != null) CameraManager.Instance.PlayerToFocusChanged -= OnPlayerToFocusChanged;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        MatchTimer.CurrentTimerStateChanged -= OnCurrentMatchTimerStateChanged;
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        if (arg0.name.Equals(TTSceneManager.Instance.CurrentHubScene))
        {
            ResetAllStats();
        }
        else if (GameManager.Instance.CurrentMatch != null && GameManager.Instance.CurrentMatch.MatchStarted)
        {
            StartCoroutine(UpdateWithDelay(1));
        }
    }

    private IEnumerator UpdateWithDelay(int delay)
    {
        yield return new WaitForSeconds(delay);
        GameStatsChanged(GameManager.Instance.CurrentMatch.Stats);
        UpdateTime();
    }

    private void OnPlayerToFocusChanged(CameraManager sender, IPlayer player)
    {
        if (_focusedPlayer == player) return;
        if (_focusedPlayer != null)
        {
            PlayerLineSpectatorController playerLine = GetPlayerLineController(_focusedPlayer);
            if (playerLine != null) playerLine.Focus = false;
        }

        if (player != null)
        {
            PlayerManager.Instance.GetParticipatingPlayers(out var players, out var count);
            players.Where(p => p != null && player != p)
                .Select(GetPlayerLineController)
                .Where(line => line != null)
                .ForEach(line => line.Focus = false);
            PlayerLineSpectatorController playerLine = GetPlayerLineController(player);
            if (playerLine != null) playerLine.Focus = true;
        }

        _focusedPlayer = player;
    }

    private void OnPlayerAdded(IPlayer player)
    {
        if (player == null)
            return;

        // Add players lines
        if (player.IsParticipating) AddPlayerLine(player);
        player.ParticipatingStatusChanged += TogglePlayerLine;
    }

    private void OnPlayerRemoved(IPlayer player)
    {
        if (player == null)
            return;

        RemovePlayerLine(player);
        player.ParticipatingStatusChanged -= TogglePlayerLine;
    }

    #region Team Functions

    private void TeamFireNameChanged(ITeam team, string newName)
    {
        _teamFireNameText.text = newName;
    }

    private void TeamIceNameChanged(ITeam team, string newName)
    {
        _teamIceNameText.text = newName;
    }

    #endregion

    #region PlayerLine Functions

    /// <summary>
    /// Adds a PlayerLine for a specific Player
    /// </summary>
    /// <param name="player">The Player whose PlayerLine should be added</param>
    /// <returns>The added PlayerLine GameObject</returns>
    [UsedImplicitly]
    public void AddPlayerLine([NotNull] IPlayer player)
    {
        if (_playerLineSpectatorControllers.ContainsKey(player))
        {
            Debug.LogWarning($"Cannot add spectator ui player line for {player}: already added");
            return;
        }

        // Instantiate the PlayerLine and set it as a child
        Transform teamController = GetTeamController(player.TeamID);
        GameObject newPlayerLineObject = InstantiateWrapper.InstantiateWithMessage(_spectatorPlayerLinePrefab, teamController);
        var newPlayerLineController = newPlayerLineObject.GetComponent<PlayerLineSpectatorController>();
        _playerLineSpectatorControllers.Add(player, newPlayerLineController);
        newPlayerLineController.SpecUiController = this;
        newPlayerLineController.Player = player;
        newPlayerLineController.PlayerNumber = teamController.childCount;

        // Set normal player text material
        newPlayerLineController.Focus = false;

        // TODO: Reflect real game stats. Make player line get these values and update by itself
        newPlayerLineController.Kills = 0;
        newPlayerLineController.Deaths = 0;
        newPlayerLineController.Assists = 0;
    }

    private Transform GetTeamController(TeamID teamID)
    {
        if (teamID != TeamID.Fire && teamID != TeamID.Ice)
        {
            Debug.LogError($"Cannot find team controller: invalid teamID {teamID}");
        }

        return teamID == TeamID.Fire ? _teamFireController : _teamIceController;
    }

    /// <summary>
    /// Removes a PlayerLine to a specific Player
    /// </summary>
    /// <param name="player">The Player whose PlayerLine should be removed</param>
    [UsedImplicitly]
    public void RemovePlayerLine(IPlayer player)
    {
        if (!_playerLineSpectatorControllers.ContainsKey(player))
        {
            Debug.LogWarning("Cannot remove player line: Not found");
            return;
        }

        Destroy(_playerLineSpectatorControllers[player].gameObject);
        _playerLineSpectatorControllers.Remove(player);
        StartCoroutine(UpdatePlayerNumbers());
    }

    [UsedImplicitly]
    public void TogglePlayerLine(IPlayer player, bool newValue)
    {
        if (newValue)
        {
            AddPlayerLine(player);
        }
        else
            RemovePlayerLine(player);
    }

    /// <summary>
    /// Relocate a playerLine to a different team
    /// </summary>
    /// <param name="playerLine">The playerLine to relocate</param>
    /// <param name="newTeamId">The teamId where the playerLine should be relocated</param>
    public void SwitchTeamOfPlayerLine(PlayerLineSpectatorController playerLine, TeamID newTeamId)
    {
        playerLine.transform.SetParent(GetTeamController(newTeamId));
    }

    /// <summary>
    /// Returns a PlayerLineController Object to a Player which identifies a PlayerLine GameObject
    /// </summary>
    /// <param name="player">The Player whose PlayerLineController is searched</param>
    /// <returns>The PlayerLine to the Player or null if not found</returns>
    [CanBeNull]
    [UsedImplicitly]
    public PlayerLineSpectatorController GetPlayerLineController([NotNull] IPlayer player)
    {
        Transform teamController = GetTeamController(player.TeamID);
        for (var currentLine = 0; currentLine < teamController.childCount; currentLine++)
        {
            var playerLineController = teamController.GetChild(currentLine)
                .GetComponent<PlayerLineSpectatorController>();
            if (playerLineController != null && playerLineController.Player == player)
            {
                return playerLineController;
            }
        }

        // Error Handling
        Debug.LogWarning("Couldn't find a playerLine for playerId " + player);
        return null;
    }

    #endregion

    #region Team Scores

    [UsedImplicitly]
    public void SetTeamPoints(TeamID teamId, int points)
    {
        SetTeamScoreText(teamId, points.ToString());
    }

    [UsedImplicitly]
    public void SetTeamScoreText(TeamID teamId, string text)
    {
        switch (teamId)
        {
            case TeamID.Fire:
                _teamFireScoreText.text = text;
                break;
            case TeamID.Ice:
                _teamIceScoreText.text = text;
                break;
            default:
                Debug.LogError("No TeamScoreText for team id " + teamId + " found");
                break;
        }
    }

    #endregion

    private void MatchHasFinishedLoading(IMatch match)
    {
        //To ensure the spectator UI is correct
        StartCoroutine(UpdatePlayerNumbers());

        // To control the match clock
        match.Started += MatchStarted;
        match.StartingAt += InitializePillarDistribution;
        match.Stopped += MatchStopped;
        match.Finished += MatchFinished;

        // To control the Kill/Deaths/Assists Stats
        match.StatsChanged += GameStatsChanged;
    }

    /// <summary>
    /// Iterates through the children of the team boxes and give them player numbers
    /// </summary>
    /// <returns></returns>
    private IEnumerator UpdatePlayerNumbers()
    {
        //To ensure added / removed player lines are instantiated or destroyed
        yield return new WaitForEndOfFrame();
        int playerNumber = 1;
        for (int i = 0; i < _teamFireController.childCount; i++)
        {
            PlayerLineSpectatorController playerLineSpectatorController =
                _teamFireController.GetChild(i).GetComponent<PlayerLineSpectatorController>();
            playerLineSpectatorController.PlayerNumber = playerNumber;
            playerNumber++;
        }

        playerNumber = 1;
        for (int i = 0; i < _teamIceController.childCount; i++)
        {
            PlayerLineSpectatorController playerLineSpectatorController =
                _teamIceController.GetChild(i).GetComponent<PlayerLineSpectatorController>();
            playerLineSpectatorController.PlayerNumber = playerNumber;
            playerNumber++;
        }
    }

    private void SceneHasFinishedLoading(string sceneName)
    {
        if (sceneName == TTSceneManager.Instance.CurrentHubScene
            || sceneName == TTSceneManager.Instance.CommendationsScene)
        {
            DisableMatchTime();
        }
    }

    /// <summary>
    /// Event listener to pause the match
    /// </summary>
    [ContextMenu("Pause Match")]
    private void PauseMatch()
    {
        InvokeRepeating(nameof(ToggleMatchTimerVisibility), 0f, _pauseMatchTimeBlinkFrequency);
    }

    private void PauseMatch(bool paused)
    {
        if (paused)
        {
            PauseMatch();
        }
        else
        {
            ResumeMatch();
        }
    }

    [ContextMenu("Resume Match")]
    private void ResumeMatch()
    {
        CancelInvoke(nameof(ToggleMatchTimerVisibility));
        MatchTimeVisible = true;
    }

    #region Match Clock

    private void InitializePillarDistribution(IMatch match, int startTime)
    {
        //Update the TowerSlider On StartMatch
        MatchStats matchStats = match.Stats;
        if (matchStats == null)
        {
            Debug.LogError("Cannot initialize pillar distribution: match stats not of expected type.");
            return;
        }

        MatchStats.TeamStats[] teamStats = matchStats.GetTeamStats().Values.ToArray();

        // Iterate through teams
        for (var i = 0; i < teamStats.Length; i++)
        {
            if (matchStats.NumberOfPillarsInScene <= 0)
            {
                Debug.LogError("Total number of Pillars in Scene is " + matchStats.NumberOfPillarsInScene +
                               ", this causes incorrect pillarShare values");
                // Don't prevent the DivideByZeroException to get the Report in Unity Performance Reporting
            }

            float teamPillarShare = (float) teamStats[i].CapturedPillars / matchStats.NumberOfPillarsInScene;
            SetPillarShare(teamStats[i].TeamID, teamPillarShare);
        }
    }

    private static void MatchStarted(IMatch match)
    {
    }

    /// <summary>
    /// MatchFinished gets called before MatchStopped, but only if the match ends by time and not by user interaction
    /// </summary>
    /// <param name="match"></param>
    private void MatchFinished(IMatch match)
    {
        SetTimeText("00:00");
        Debug.Log("Match Finished at " + DateTime.UtcNow);
    }

    /// <summary>
    /// MatchFinished gets called before MatchStopped, but only if the match ends by time and not by user interaction
    /// </summary>
    private static void MatchStopped(IMatch match)
    {
        Debug.Log("Match Stopped at " + DateTime.UtcNow);
    }

    private void UpdateTime()
    {
        MatchTimer matchTimer = GameManager.Instance.MatchTimer;
        var remainingTime = Mathf.CeilToInt(matchTimer.GetCurrentTimerInSeconds());
        if (!TTSceneManager.Instance.IsInHubScene && remainingTime >= 0)
        {
            SetRemainingTime(remainingTime);
        }
    }

    private void DisableMatchTime()
    {
        //_timeText.fontStyle = FontStyle.Normal;
        SetTimeText("--:--");
    }

    private void SetRemainingTime(int seconds)
    {
        int min = seconds / 60;
        int sec = seconds % 60;
        SetRemainingTime(min, sec);
    }

    private void SetRemainingTime(int minutes, int seconds)
    {
        //string matchTimeString = String.Format("{0,2}:{1,2:D2}", minutes, seconds);
        string remainingTimeString = minutes.ToString("D2") + ":" + seconds.ToString("D2");
        SetTimeText(remainingTimeString);
    }

    private void SetTimeText(string text)
    {
        _timeText.text = text;
    }

    private void ToggleMatchTimerVisibility()
    {
        MatchTimeVisible = !MatchTimeVisible;
    }

    private void OnCurrentMatchTimerStateChanged(MatchTimer sender, MatchTimer.TimerState newTimerState)
    {
        switch (newTimerState)
        {
            case MatchTimer.TimerState.Undefined:
            case MatchTimer.TimerState.CountdownTimer:
            case MatchTimer.TimerState.TimerIsPaused:
            case MatchTimer.TimerState.WaitForCountdown:
                _timeText.color = _fireTeamColor;
                break;
            case MatchTimer.TimerState.MatchTimer:
                _timeText.color = _iceTeamColor;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newTimerState), newTimerState,
                    "Invalid TimerState of the MatchTimer");
        }
    }

    #endregion

    #region Stats

    /// <summary>
    /// Event listener which updates the UI according to changed team and players stats.
    /// </summary>
    private void GameStatsChanged(MatchStats stats)
    {
        if (stats == null)
        {
            Debug.LogError("stats are null");
            return;
        }

        SetTeamPoints(TeamID.Fire, stats.GetTeamStats()[TeamID.Fire].Points);
        SetTeamPoints(TeamID.Ice, stats.GetTeamStats()[TeamID.Ice].Points);

        // update pillar share
        foreach (MatchStats.TeamStats singleTeamStats in stats.GetTeamStats().Values)
        {
            if (stats.NumberOfPillarsInScene <= 0)
            {
                Debug.LogError($"Total number of Pillars in Scene is {stats.NumberOfPillarsInScene}, " +
                               "this causes incorrect pillarShare values");
            }

            float teamPillarShare = (float) singleTeamStats.CapturedPillars / stats.NumberOfPillarsInScene;
            SetPillarShare(singleTeamStats.TeamID, teamPillarShare);
        }

        // Update KDA Stats
        foreach (PlayerStats singlePlayerStats in stats.GetPlayerStats().Values)
        {
            IPlayer player = PlayerManager.Instance.GetPlayer(singlePlayerStats.PlayerID);
            if (player == null || !player.IsParticipating) continue;
            PlayerLineSpectatorController playerLine = GetPlayerLineController(player);
            if (playerLine == null) continue;
            playerLine.Kills = singlePlayerStats.Kills;
            playerLine.Deaths = singlePlayerStats.Deaths;
            playerLine.Assists = singlePlayerStats.Assists;
        }
    }

    private void SetPillarShare(TeamID teamId, float pillarShare)
    {
        if (teamId == TeamID.Fire) _teamFirePillarShareSlider.value = Mathf.Clamp01(pillarShare);
        if (teamId == TeamID.Ice) _teamIcePillarShareSlider.value = Mathf.Clamp01(pillarShare);
    }

    [UsedImplicitly]
    public void ResetAllStats()
    {
        ResetTeamStats();
        ResetPlayerStats();
    }

    private void ResetTeamStats()
    {
        SetTeamPoints(TeamID.Fire, 0);
        SetPillarShare(TeamID.Fire, 0f);
        SetTeamPoints(TeamID.Ice, 0);
        SetPillarShare(TeamID.Ice, 0f);
    }

    private void ResetPlayerStats()
    {
        foreach (PlayerLineSpectatorController playerLine in _playerLineSpectatorControllers.Values)
        {
            playerLine.ResetPlayerStats();
        }
    }

    #endregion
}