using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using TowerTag;
using UnityEngine;

public class ScoreBoardPlayerLineController : MonoBehaviour {
    [SerializeField] private PlayerStatsScoreBoard _playerLinePrefab;
    [SerializeField] private Transform _teamFirePlayerLineContainer;
    [SerializeField] private Transform _teamIcePlayerLineContainer;

    private readonly Dictionary<int, PlayerStatsScoreBoard> _playerLineByID =
        new Dictionary<int, PlayerStatsScoreBoard>();

    private void Start() {
        foreach (PlayerStatsScoreBoard playerLine in _playerLineByID.Values) {
            Destroy(playerLine.gameObject);
        }

        _playerLineByID.Clear();
        PlayerManager.Instance.GetAllConnectedPlayers(out var players, out var count);
        for (int i = 0; i < count; i++)
            OnPlayerAdded(players[i]);
        PlayerManager.Instance.PlayerAdded += OnPlayerAdded;
        PlayerManager.Instance.PlayerRemoved += RemovePlayerLine;
    }

    private void OnDestroy() {
        PlayerManager.Instance.GetAllConnectedPlayers(out var players, out var count);
        for (int i = 0; i < count; i++)
            OnPlayerRemoved(players[i]);
        PlayerManager.Instance.PlayerAdded -= OnPlayerAdded;
        PlayerManager.Instance.PlayerRemoved -= RemovePlayerLine;
    }

    private void OnPlayerAdded([NotNull] IPlayer player) {
        AddPlayerLine(player);
        player.ParticipatingStatusChanged += OnParticipatingStatusChanged;
        player.PlayerTeamChanged += OnTeamChanged;
    }

    private void OnPlayerRemoved([NotNull] IPlayer player) {
        RemovePlayerLine(player);
        player.ParticipatingStatusChanged -= OnParticipatingStatusChanged;
        player.PlayerTeamChanged -= OnTeamChanged;
    }

    private void OnTeamChanged(IPlayer player, TeamID arg2)
    {
        RemovePlayerLine(player);
        AddPlayerLine(player);
    }

    private void OnParticipatingStatusChanged(IPlayer player, bool newValue) {
        if (newValue)
            AddPlayerLine(player);
        else
            RemovePlayerLine(player);
    }

    private void RemovePlayerLine(IPlayer player) {
        try {
            if (_playerLineByID.ContainsKey(player.PlayerID)) {
                Destroy(_playerLineByID[player.PlayerID].gameObject);
                _playerLineByID.Remove(player.PlayerID);
            }
        }
        catch (Exception e) {
            Debug.LogException(e);
        }
    }

    private void AddPlayerLine([NotNull] IPlayer player) {
        try {
            if (!player.IsParticipating) return;

            if (_playerLineByID.ContainsKey(player.PlayerID)) {
                Debug.LogWarning("Cannot add player line to scoreboard: there already is a line for the player");
                return;
            }

            PlayerStatsScoreBoard line = _playerLinePrefab.Create(player,
                player.TeamID == TeamID.Fire ? _teamFirePlayerLineContainer : _teamIcePlayerLineContainer);
            _playerLineByID[player.PlayerID] = line;
        }
        catch (Exception e) {
            Debug.LogException(e);
        }
    }
}