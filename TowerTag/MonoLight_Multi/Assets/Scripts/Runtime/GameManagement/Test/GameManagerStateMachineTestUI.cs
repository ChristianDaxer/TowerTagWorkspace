using Photon.Pun;
using System;
using TowerTag;
using UnityEngine;

public class GameManagerStateMachineTestUI : MonoBehaviour {
    public delegate void GameManagerStateMachineTestUiAction(bool active);

    public static event GameManagerStateMachineTestUiAction StateMachineTestUiToggled;

    [SerializeField] private bool _visible = true;
    [SerializeField] private KeyCode _keyCode = KeyCode.F6;
    [SerializeField] private bool _printStatesToLog;
    [SerializeField] private MatchDescription _matchToStart;
    private string _testString = "-";

    [SerializeField] private Rect _viewRect;
    private const int ButtonRowHeight = 50;
    private static GameManager GameManager => GameManager.Instance;
    private string _matchEvents = "MatchEvents: ";

    private Vector2 _buttonsScrollViewPos = Vector2.zero;

    private void Start() {
        if (!Debug.isDebugBuild)
            enabled = false;
    }

    // Update is called once per frame
    private void Update() {
#if UNITY_EDITOR
        _testString = GameManager.PrintCurrentState(_printStatesToLog);
#endif
        if (Input.GetKeyDown(_keyCode)) {
            _visible = !_visible;
            StateMachineTestUiToggled?.Invoke(_visible);
        }
    }

    private void OnDestroy() {
        if (GameManager != null) {
            GameManager.MatchHasChanged -= RegisterMatchCallbacks;
            UnregisterMatchCallbacks(GameManager.CurrentMatch);
        }
    }

    [ContextMenu("Trigger Mission Briefing")]
    public void TriggerMissionBriefing() {
        var mode = _matchToStart.GameMode.HasFlag(GameMode.Elimination) ? GameMode.Elimination
            : _matchToStart.GameMode.HasFlag(GameMode.DeathMatch) ? GameMode.DeathMatch
            : GameMode.GoalTower;
        GameManager.TriggerMissionBriefingOnMaster(_matchToStart, mode);
    }

    private void OnGUI() {
        if (!_visible)
            return;
        GUI.Box(_viewRect, "");
        GUILayout.BeginArea(_viewRect);
        {
            GUILayout.Label("*** GameManagerStateMachineTestUI (toggle with " + _keyCode + ") ***\n" + _testString);

            if (PhotonNetwork.IsMasterClient) {
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("LoadHub"))
                        GameManager.TriggerMatchConfigurationOnMaster();

                    if (GUILayout.Button("LoadMatch")) {
                        TriggerMissionBriefing();
                    }

                    string paused = GameManager.IsPaused() ? "Resume Match" : "Pause Match";
                    if (GUILayout.Button(paused))
                        GameManager.SetPauseOnMaster(!GameManager.IsPaused());
                }
                GUILayout.EndHorizontal();

                _buttonsScrollViewPos = GUILayout.BeginScrollView(_buttonsScrollViewPos,
                    GUILayout.Width(_viewRect.width - 10), GUILayout.Height(ButtonRowHeight));
                {
                    GUILayout.BeginHorizontal();
                    var values =
                        (GameManager.GameManagerStateMachine.State[]) Enum.GetValues(typeof(GameManager.GameManagerStateMachine.State));
                    foreach (GameManager.GameManagerStateMachine.State state in values) {
#pragma warning disable 618 // 
                        if (GUILayout.Button(state.ToString()))
                            GameManager.ChangeState(state);
#pragma warning restore 618
                    }

                    GUILayout.EndHorizontal();
                }

                GUILayout.EndScrollView();

                DrawMatchScoreManipulationUI();
            }

            GUILayout.Label(_matchEvents);
        }
        GUILayout.EndArea();
    }

    private void DrawMatchScoreManipulationUI() {
        GUILayout.BeginVertical("Match Score Manipulation", "window", GUILayout.Width(200));

        GUILayout.BeginHorizontal();
        GUILayout.Label("My Team");
        if (GUILayout.Button("+1", GUILayout.Width(22), GUILayout.Height(22))) {
            IPlayer player = PlayerManager.Instance.GetOwnPlayer();

            if (player != null) {
                GameManager.Instance.CurrentMatch.Stats.AddTeamPoint(player.TeamID);
            }
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Enemy Team");
        if (GUILayout.Button("+1", GUILayout.Width(22), GUILayout.Height(22))) {
            IPlayer player = PlayerManager.Instance.GetOwnPlayer();

            if (player != null) {
                TeamID enemyTeam = TeamManager.Singleton.GetEnemyTeamIDOfPlayer(player);
                GameManager.Instance.CurrentMatch.Stats.AddTeamPoint(enemyTeam);
            }
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    private void RegisterMatchCallbacks(IMatch match) {
        // clear incoming events list
        _matchEvents = "MatchEvents: ";

        match.Initialized += OnMatchInitialized;
        match.StartingAt += OnMatchStartsAt;
        match.Started += OnMatchStarted;
        match.Finished += OnMatchFinished;
        match.Stopped += OnMatchStopped;

        match.RoundStartingAt += OnRoundStartsAt;
        match.RoundStarted += OnRoundStarted;
        match.RoundFinished += OnRoundFinished;

        match.StatsChanged += OnMatchStatsChanged;
    }

    private void UnregisterMatchCallbacks(IMatch match) {
        // clear incoming events list
        _matchEvents = "MatchEvents: - ";

        if (match == null)
            return;

        match.Initialized -= OnMatchInitialized;
        match.StartingAt -= OnMatchStartsAt;
        match.Started -= OnMatchStarted;
        match.Finished -= OnMatchFinished;
        match.Stopped -= OnMatchStopped;

        match.RoundStartingAt -= OnRoundStartsAt;
        match.RoundStarted -= OnRoundStarted;
        match.RoundFinished -= OnRoundFinished;

        match.StatsChanged -= OnMatchStatsChanged;
    }

    private void OnMatchInitialized(IMatch match) {
        _matchEvents += "MatchInitialized | ";
    }

    private void OnMatchStartsAt(IMatch match, int timestamp) {
        _matchEvents += "MatchStartsAt: " + timestamp + " | ";
    }

    private void OnMatchStarted(IMatch match) {
        _matchEvents += "MatchStarted | ";
    }

    private void OnMatchFinished(IMatch match) {
        _matchEvents += "MatchFinished | ";
    }

    private void OnMatchStopped(IMatch match) {
        _matchEvents += "MatchStopped | ";
    }

    private void OnRoundStartsAt(IMatch match, int timestamp) {
        _matchEvents += "RoundStartsAt " + timestamp + " | ";
    }

    private void OnRoundStarted(IMatch match) {
        _matchEvents += "RoundStarted | ";
    }

    private void OnRoundFinished(IMatch match, TeamID teamID) {
        _matchEvents += "RoundFinished | ";
    }

    private void OnMatchStatsChanged(MatchStats stats) {
        _matchEvents += "MatchStatsChanged | ";
    }
}