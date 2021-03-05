using System.Collections.Generic;
using TMPro;
using TowerTag;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsScoreBoard : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI _name, _score, _outs, _assist;
    [SerializeField] private Image _background;
    [SerializeField] private bool _isMe;
    [SerializeField] private TeamID _teamID;
    [SerializeField] private bool _isOut;

    [SerializeField, Tooltip("Image in the PlayerLine to show if someone is out")]
    private Image _outIcon;

    private IPlayer _player;
    private PlayerStats _currentStats;

    public PlayerStatsScoreBoard Create(IPlayer player, Transform container) {
        PlayerStatsScoreBoard playerStatsScoreBoard = InstantiateWrapper.InstantiateWithMessage(this, container);
        playerStatsScoreBoard.Init(player);
        return playerStatsScoreBoard;
    }

    public void CreatePostMatch(IPlayer player, PreviousMatchResults.FinalPlayerStats playerStats, Transform container) {
        PlayerStatsScoreBoard playerStatsScoreBoard = InstantiateWrapper.InstantiateWithMessage(this, container);
        playerStatsScoreBoard._teamID = player.TeamID;
        playerStatsScoreBoard._player = player;
        playerStatsScoreBoard._isMe = player.IsMe;
        playerStatsScoreBoard._name.text = playerStats.Name;
        playerStatsScoreBoard._score.text = playerStats.Score.ToString();
        playerStatsScoreBoard._outs.text = playerStats.Outs.ToString();
        playerStatsScoreBoard._assist.text = playerStats.Assists.ToString();
        playerStatsScoreBoard.Colorize();
    }

    private void Init(IPlayer player) {
        UnregisterEventListeners();
        _player = player;
        _isMe = _player.IsMe;
        _isOut = !_player.PlayerHealth.IsAlive;
        _teamID = _player.TeamID;
        _name.text = _player.PlayerName;
        if (GameManager.Instance.CurrentMatch != null) {
            OnMatchStatsChanged(GameManager.Instance.CurrentMatch.Stats);
        }

        RegisterEventListeners();
        Refresh();
    }

    private void OnEnable() {
        RegisterEventListeners();
    }

    private void OnValidate() {
        if (gameObject.scene.buildIndex == -1) return;
        Refresh();
    }

    private void Refresh() {
        Colorize();

        if (_player != null) _name.text = _player.PlayerName;
        _score.text = _currentStats.Kills.ToString();
        _outs.text = _currentStats.Deaths.ToString();
        _assist.text = _currentStats.Assists.ToString();
    }

    private void Colorize() {
        if (TeamManager.Singleton == null) return;
        Color teamColor = TeamManager.Singleton.Get(_teamID).Colors.UI;
        teamColor = _isOut ? new Color(teamColor.r, teamColor.g, teamColor.b, 0.25f) : teamColor;
        Color foregroundColor = _isMe ? Color.black : teamColor;
        Color backgroundColor = _isMe ? teamColor : Color.black;
        _name.color = foregroundColor;
        _score.color = foregroundColor;
        _outs.color = foregroundColor;
        _assist.color = foregroundColor;
        _outIcon.color = foregroundColor;
        _background.gameObject.SetActive(_isMe);
        _outIcon.gameObject.SetActive(_isOut);
        _background.color = backgroundColor;

    }

    private void OnDisable() {
        UnregisterEventListeners();
    }

    private void RegisterEventListeners() {
        if (_player == null) return;

        if (GameManager.Instance.CurrentMatch != null) {
            GameManager.Instance.CurrentMatch.StatsChanged += OnMatchStatsChanged;
        }

        _player.PlayerNameChanged += SetChangedPlayerName;
        _player.PlayerTeamChanged += OnPlayerTeamChanged;
        if (_player.PlayerHealth != null) {
            _player.PlayerHealth.PlayerRevived += OnPlayerRevived;
            _player.PlayerHealth.PlayerDied += OnPlayerDied;
        }
    }

    private void UnregisterEventListeners() {
        if (GameManager.Instance.CurrentMatch != null) {
            GameManager.Instance.CurrentMatch.StatsChanged -= OnMatchStatsChanged;
        }

        if (_player != null) {
            _player.PlayerNameChanged -= SetChangedPlayerName;
            _player.PlayerTeamChanged -= OnPlayerTeamChanged;
            if (_player.PlayerHealth != null) {
                _player.PlayerHealth.PlayerRevived -= OnPlayerRevived;
                _player.PlayerHealth.PlayerDied -= OnPlayerDied;
            }
        }
    }

    private void OnPlayerTeamChanged(IPlayer player, TeamID teamID) {
        _teamID = _player.TeamID;
        Refresh();
    }

    private void OnPlayerDied(PlayerHealth playerHealth, IPlayer enemyWhoAppliedDamage, byte colliderType) {
        _isOut = true;
        Refresh();
    }

    private void OnPlayerRevived(IPlayer player) {
        _isOut = false;
        Refresh();
    }

    private void SetChangedPlayerName(string newName) {
        Refresh();
    }

    /// <summary>
    /// Update player stats
    /// </summary>
    /// <param name="stats"></param>
    private void OnMatchStatsChanged(MatchStats stats) {
        Dictionary<int, PlayerStats> playerStats = stats?.GetPlayerStats();
        if (playerStats != null && playerStats.TryGetValue(_player.PlayerID, out _currentStats))
        {
            Refresh();
        }
    }
}