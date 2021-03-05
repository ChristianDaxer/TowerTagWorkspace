using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Network;
using Photon.Pun;
using TowerTag;
using UnityEngine;

public class ReadyTowerPlayerLineManager : MonoBehaviour {
    [Header("UI Elements")] [SerializeField]
    private GameObject _playerBoxFire;

    [SerializeField] private GameObject _playerBoxIce;

    [Header("Prefabs")] [SerializeField] private ReadyTowerPlayerLine _playerLineFire;
    [SerializeField] private ReadyTowerPlayerLine _playerLineIce;

    private readonly Dictionary<ReadyTowerPlayerLine, IPlayer> _iceDict =
        new Dictionary<ReadyTowerPlayerLine, IPlayer>();

    private readonly Dictionary<ReadyTowerPlayerLine, IPlayer> _fireDict =
        new Dictionary<ReadyTowerPlayerLine, IPlayer>();

    public static int SlotCount;

    private void OnEnable() {
        PlayerManager.Instance.PlayerAdded += OnPlayerAdded;
        PlayerManager.Instance.PlayerRemoved += OnPlayerRemoved;

        PlayerManager.Instance.GetParticipatingPlayers(out var players, out var count);
        for (int i = 0; i < count; i++)
            players[i].PlayerTeamChanged += OnPlayerTeamChanged;
    }

    private void Start() {
        SetUpPlayerLines();
    }

    private void SetUpPlayerLines() {
        if (TowerTagSettings.BasicMode) {
            //- 1 or 2 because of operator or/and spectator.
            CreatePlayerLinesMaxPlayerDependent();
        }
        else if (TowerTagSettings.Home) {
            switch (GameManager.Instance.CurrentHomeMatchType) {
                case GameManager.HomeMatchType.Random:
                case GameManager.HomeMatchType.Custom:
                    CreatePlayerLinesMaxPlayerDependent();
                    break;
                case GameManager.HomeMatchType.TrainingVsAI:
                    CreatePlayerLinesConnectedPlayerDependent();
                    break;
            }
        }
    }

    private void CreatePlayerLinesConnectedPlayerDependent() {
        PlayerManager.Instance.GetParticipatingFirePlayers(out var firePlayers, out var fireCount);
        PlayerManager.Instance.GetParticipatingIcePlayers(out var icePlayers, out var iceCount);
        AddEmptyPlayerLines(_playerBoxFire, _playerLineFire, fireCount, _fireDict, firePlayers);
        AddEmptyPlayerLines(_playerBoxIce, _playerLineIce, iceCount, _iceDict, icePlayers);
    }

    private void CreatePlayerLinesMaxPlayerDependent()
    {
        if (TowerTagSettings.Home)
            SlotCount = RoomConfiguration.GetMaxPlayersForCurrentRoom();
        else
            SlotCount = PhotonNetwork.CurrentRoom.MaxPlayers - (TowerTagSettings.BasicMode ? 1 : 2);

        int teamFireSize = SlotCount / 2;
        //If we have an uneven setup
        int teamIceSize = SlotCount - teamFireSize;
        PlayerManager.Instance.GetParticipatingFirePlayers(out var firePlayers, out var fireCount);
        PlayerManager.Instance.GetParticipatingIcePlayers(out var icePlayers, out var iceCount);
        AddEmptyPlayerLines(_playerBoxFire, _playerLineFire, teamFireSize, _fireDict, firePlayers);
        AddEmptyPlayerLines(_playerBoxIce, _playerLineIce, teamIceSize, _iceDict, icePlayers);
    }

    private void OnDisable() {
        PlayerManager.Instance.PlayerAdded -= OnPlayerAdded;
        PlayerManager.Instance.PlayerRemoved -= OnPlayerRemoved;
        PlayerManager.Instance.GetParticipatingPlayers(out var players, out var count);
        for (int i = 0; i < count; i++)
            players[i].PlayerTeamChanged -= OnPlayerTeamChanged;
    }

    private void AddEmptyPlayerLines(GameObject playerBox, ReadyTowerPlayerLine playerLinePrefab,
        int teamSize, Dictionary<ReadyTowerPlayerLine, IPlayer> teamPlayers, IPlayer[] currentPlayers) {
        for (int i = 0; i < teamSize; i++) {
            ReadyTowerPlayerLine playerLine = InstantiateWrapper.InstantiateWithMessage(playerLinePrefab, playerBox.transform);
            IPlayer player = currentPlayers.Length > i ? currentPlayers[i] : null;
            playerLine.SetPlayer(player);
            teamPlayers.Add(playerLine, player);
        }
    }

    private void OnPlayerTeamChanged(IPlayer player, TeamID newTeam) {
        if (player.IsMe) return;
        TeamID oldTeam = newTeam == TeamID.Fire ? TeamID.Ice : TeamID.Fire;
        RemovePlayerLine(player, oldTeam);
        AddPlayerLine(player, newTeam);
    }

    private void AddPlayerLine(IPlayer player, TeamID teamID) {
        if (player.IsMe) return;
        Dictionary<ReadyTowerPlayerLine, IPlayer> teamDict = teamID == TeamID.Fire ? _fireDict : _iceDict;
        ReadyTowerPlayerLine freeSlot = null;
        if (teamDict.Any(entry => entry.Key.IsEmpty)) {
            freeSlot = teamDict.FirstOrDefault(dict => dict.Key.IsEmpty).Key;
        }
        if (freeSlot == null) {
            StartCoroutine(WaitForFreeSlot(teamDict, player));
            return;
        }

        freeSlot.SetPlayer(player);
        teamDict[freeSlot] = player;
    }

    /// <summary>
    /// Waits a duration of 2 seconds for a free slot. This is useful, when someone swaps with a player of a full team
    /// </summary>
    /// <param name="teamDict">RTPL to player dictionary from the players new team</param>
    /// <param name="player">The switching player</param>
    /// <returns></returns>
    private IEnumerator WaitForFreeSlot(Dictionary<ReadyTowerPlayerLine, IPlayer> teamDict, IPlayer player) {
        ReadyTowerPlayerLine freeSlot = null;
        float timer = 0;
        int maxDuration = 2;
        while (freeSlot == null && timer <= maxDuration) {
            if (teamDict.Any(entry => entry.Key.IsEmpty)) {
                freeSlot = teamDict.FirstOrDefault(dict => dict.Key.IsEmpty).Key;
            }
            timer += Time.deltaTime;
            yield return null;
        }

        if (freeSlot == null) {
            Debug.LogError("Could not find a free slot for the ReadyTowerPlayerLine of " + player);
            yield break;
        }

        freeSlot.SetPlayer(player);
        teamDict[freeSlot] = player;
    }

    private void RemovePlayerLine(IPlayer player, TeamID teamID) {
        Dictionary<ReadyTowerPlayerLine, IPlayer> teamDict = teamID == TeamID.Fire ? _fireDict : _iceDict;
        ReadyTowerPlayerLine playerSlot = teamDict.FirstOrDefault(dict => dict.Value == player).Key;
        if (playerSlot == null) return;
        playerSlot.ResetPlayerLine();
        teamDict[playerSlot] = null;
    }

    private void OnPlayerAdded(IPlayer player) {
        AddPlayerLine(player, player.TeamID);
        player.PlayerTeamChanged += OnPlayerTeamChanged;
    }

    private void OnPlayerRemoved(IPlayer player) {
        RemovePlayerLine(player, player.TeamID);
        player.PlayerTeamChanged -= OnPlayerTeamChanged;
    }
}