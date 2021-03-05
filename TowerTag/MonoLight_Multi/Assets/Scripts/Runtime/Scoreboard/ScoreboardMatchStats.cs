using System;
using System.Text;
using System.Collections.Generic;
using TMPro;
using TowerTag;
using UnityEngine;

public class ScoreboardMatchStats : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI _teamFireScore;
    [SerializeField] private TextMeshProUGUI _teamFireName;
    [SerializeField] private TextMeshProUGUI _teamIceScore;
    [SerializeField] private TextMeshProUGUI _teamIceName;

    [SerializeField, Tooltip("The text field of the play time")]
    private TextMeshProUGUI _time;

    private AdminController _admin;
    private bool _overrideTimerText;
    private Color _iceTeamColor;
    private Color _fireTeamColor;
    private int cachedSecondsRemaining = int.MaxValue;
    private StringBuilder builder = new StringBuilder();

    private void Awake() {
        _iceTeamColor = _teamIceName.color;
        _fireTeamColor = _teamFireName.color;
    }

    private void OnEnable() {
        if (GameManager.Instance.CurrentMatch != null) {
            GameManager.Instance.CurrentMatch.StatsChanged += SetTeamPointsOnStatsChanged;
            GameManager.Instance.PauseReceived += PauseMatch;
            GameManager.Instance.CurrentMatch.Finished += OnMatchFinished;
            MatchTimer.CurrentTimerStateChanged += OnTimerStateChanged;
            _teamFireName.text = TeamManager.Singleton.TeamFire.Name;
            _teamIceName.text = TeamManager.Singleton.TeamIce.Name;
            TeamManager.Singleton.TeamFire.NameChanged += TeamFireNameChanged;
            TeamManager.Singleton.TeamIce.NameChanged += TeamIceNameChanged;
        }
        else {
            Debug.LogError("Failed to initialize Scoreboard: Current match not available");
        }
    }

    private void Start() {
        if (GameManager.Instance.CurrentMatch != null) {
            SetTimeText("--:--");
            SetTeamPointsOnStatsChanged(GameManager.Instance.CurrentMatch.Stats);
        }
        else {
            Debug.LogError("Failed to initialize Scoreboard: Current match not available");
        }
    }

    private void OnDisable() {
        if (GameManager.Instance.CurrentMatch != null) {
            GameManager.Instance.CurrentMatch.StatsChanged -= SetTeamPointsOnStatsChanged;
            GameManager.Instance.CurrentMatch.Finished -= OnMatchFinished;
        }

        GameManager.Instance.PauseReceived -= PauseMatch;
        MatchTimer.CurrentTimerStateChanged -= OnTimerStateChanged;
        TeamManager.Singleton.TeamFire.NameChanged -= TeamFireNameChanged;
        TeamManager.Singleton.TeamIce.NameChanged -= TeamIceNameChanged;
    }

    private void OnTimerStateChanged(MatchTimer sender, MatchTimer.TimerState newTimerState) {
        switch(newTimerState) {
            case MatchTimer.TimerState.Undefined:
            case MatchTimer.TimerState.CountdownTimer:
            case MatchTimer.TimerState.TimerIsPaused:
            case MatchTimer.TimerState.WaitForCountdown:
                _time.color = _fireTeamColor;
                break;
            case MatchTimer.TimerState.MatchTimer:
                _time.color = _iceTeamColor;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newTimerState), newTimerState,
                    "Invalid TimerState of the MatchTimer");
        }
    }

    private void OnMatchFinished(IMatch match) {
        SetTimeText("END");
    }


    private void TeamFireNameChanged(ITeam team, string newName) {
        _teamFireName.text = newName;
    }

    private void TeamIceNameChanged(ITeam team, string newName) {
        _teamIceName.text = newName;
    }

    private void Update() {
        UpdateTime();
    }

    #region UpdateMatchStatsOnScoreboard

    private void PauseMatch(bool paused) {
        if (paused) {
            SetTimeText("PAUSED");
        }
    }

    /// <summary>
    /// Update the points when changed
    /// </summary>
    /// <param name="stats"></param>
    void SetTeamPointsOnStatsChanged(MatchStats stats) {
        MatchStats matchStats = stats;

        if (matchStats == null) {
            Debug.LogWarning("Cannot set team points: No death match stats");
            return;
        }

        Dictionary<TeamID, MatchStats.TeamStats> teamStats = matchStats.GetTeamStats();
        _teamFireScore.text = teamStats.ContainsKey(TeamID.Fire) ? teamStats[TeamID.Fire].Points.ToString() : "-";
        _teamIceScore.text = teamStats.ContainsKey(TeamID.Ice) ? teamStats[TeamID.Ice].Points.ToString() : "-";
    }

    private bool printedError;
    private void UpdateTime() {

        int remainingTime = 0;

        if (GameManager.Instance.MatchTimer != null)
        {
            remainingTime = Mathf.CeilToInt(GameManager.Instance.MatchTimer.GetCurrentTimerInSeconds());
            if (printedError)
                printedError = false;
        }

        else if (!printedError)
        {
            Debug.LogError("Match timer is null!");
            printedError = true;
        }
            

        //If the remaining time hasn't changed since last time, no need to update.
        if (remainingTime == cachedSecondsRemaining)
            return;

        cachedSecondsRemaining = remainingTime;

        //We are probably in pause! Don't update the time
        if (_overrideTimerText) return;

        if (remainingTime > 0)
            ConvertSecToMin(remainingTime);
    }

    private void ConvertSecToMin(int seconds) {
        int min = seconds / 60;
        int sec = seconds % 60;

        if (min < 10)
        {
            builder.Append("0");
        }

        builder.Append(min);
        builder.Append(":");

        if (sec < 10)
        {
            builder.Append("0");
        }
        builder.Append(sec);
        SetTimeText(builder.ToString());
        builder.Clear();
    }

    private void RemainingTimeToString(int minutes, int seconds) {
        string remainingTimeString = minutes.ToString("D2") + ":" + seconds.ToString("D2");
        SetTimeText(remainingTimeString);
    }

    private void SetTimeText(string text) {
        _overrideTimerText = false;
        _time.text = text;
    }

    public void OverrideTimerText(string text) {
        _overrideTimerText = true;
        _time.text = text;
    }

    #endregion
}