using System.Collections.Generic;
using Photon.Pun;
using TowerTag;
using TowerTagSOES;
using UnityEngine;

public class TeamChangeManager : MonoBehaviour {
    private readonly List<IPlayer> _icePlayer = new List<IPlayer>();
    private readonly List<IPlayer> _firePlayer = new List<IPlayer>();

    private void OnEnable() {
        Init();
    }

    private void OnDisable() {
        CleanUp();
    }

    private void Init() {
        PlayerManager.Instance.GetParticipatingPlayers(out var players, out var count);
        for (int i = 0; i < count; i++)
        {
            players[i].TeamChangeRequestChanged += OnTeamChangeRequested;
            players[i].PlayerTeamChanged += OnTeamChanged;
            if (players[i].TeamChangeRequested) {
                List<IPlayer> teamList = players[i].TeamID == TeamID.Fire ? _firePlayer : _icePlayer;
                teamList.Add(players[i]);
            }
        }

        PlayerManager.Instance.PlayerAdded += OnPlayerAdded;
        PlayerManager.Instance.PlayerRemoved += OnPlayerRemoved;
    }

    private void CleanUp() {
        PlayerManager.Instance.GetParticipatingPlayers(out var players, out var count);

        for (int i = 0; i < count; i++)
        {
            players[i].TeamChangeRequestChanged -= OnTeamChangeRequested;
            players[i].PlayerTeamChanged -= OnTeamChanged;
        }

        PlayerManager.Instance.PlayerAdded -= OnPlayerAdded;
        PlayerManager.Instance.PlayerRemoved -= OnPlayerRemoved;
        _icePlayer.Clear();
        _firePlayer.Clear();
    }

    private void OnPlayerAdded(IPlayer player) {
        player.TeamChangeRequestChanged += OnTeamChangeRequested;
        player.PlayerTeamChanged += OnTeamChanged;
    }

    private void OnPlayerRemoved(IPlayer player) {
        player.TeamChangeRequestChanged -= OnTeamChangeRequested;
        player.PlayerTeamChanged -= OnTeamChanged;
    }

    private void OnTeamChanged(IPlayer player, TeamID obj) {
        if (_icePlayer.Contains(player)) _icePlayer.Remove(player);
        if (_firePlayer.Contains(player)) _firePlayer.Remove(player);
    }

    private void OnTeamChangeRequested(IPlayer player, bool newState) {
        List<IPlayer> teamList = player.TeamID == TeamID.Fire ? _firePlayer : _icePlayer;
        List<IPlayer> enemyTeamList = player.TeamID == TeamID.Fire ? _icePlayer : _firePlayer;
        int enemyTeamCount = player.TeamID == TeamID.Fire
            ? PlayerManager.Instance.GetParticipatingIcePlayerCount()
            : PlayerManager.Instance.GetParticipatingFirePlayerCount();
        if (newState) {
            teamList.Add(player);
            if (!PhotonNetwork.IsMasterClient || GameManager.Instance.MatchCountdownRunning) return;
            if (enemyTeamList.Count > 0) {
                if (SharedControllerType.IsAdmin)
                    AdminController.Instance.SwapTeamOfPlayerLines(player, enemyTeamList[0]);
                GameManager.Instance.ChangePlayerTeam(player, player.TeamID == TeamID.Fire ? TeamID.Ice : TeamID.Fire);
                GameManager.Instance.ChangePlayerTeam(enemyTeamList[0], enemyTeamList[0].TeamID == TeamID.Fire ? TeamID.Ice : TeamID.Fire);
            }
            else if (enemyTeamCount < (byte)PhotonNetwork.CurrentRoom.CustomProperties[RoomPropertyKeys.MaxPlayers] / 2) {
                if (SharedControllerType.IsAdmin)
                    AdminController.Instance.SwapTeamOfPlayerLines(player);
                GameManager.Instance.ChangePlayerTeam(player, player.TeamID == TeamID.Fire ? TeamID.Ice : TeamID.Fire);
            }
        }
        else {
            teamList.Remove(player);
        }
    }
}