using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Network;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using IPlayer = TowerTag.IPlayer;

public class RoomOptionManager : MonoBehaviour
{
    private readonly Dictionary<IPlayer, int> _playerRanks = new Dictionary<IPlayer, int>();

    public static bool IsRoomFull
        => PlayerManager.Instance.GetAllConnectedPlayerCount()
           >= RoomConfiguration.GetMaxPlayersForCurrentRoom();

    public static bool HasRoomTooManyPlayers
        => (PlayerManager.Instance.GetAllConnectedPlayerCount() + 1)
           > RoomConfiguration.GetMaxPlayersForCurrentRoom();
    public static bool RoomWasFull
        => (PlayerManager.Instance.GetAllConnectedPlayerCount() + 1)
           >= RoomConfiguration.GetMaxPlayersForCurrentRoom();

    private void OnEnable()
    {
        GameManager.Instance.MatchConfigurationStarted += OnMatchConfigurationStarted;
        GameManager.Instance.MissionBriefingStarted += OnMissionBriefingStarted;
        GameManager.Instance.MatchHasFinishedLoading += OnMatchHasFinishedLoading;
        GameManager.Instance.BasicCountdownStarted += OnBasicCountdownStarted;
        GameManager.Instance.BasicCountdownAborted += OnBasicCountdownAborted;
        PlayerManager.Instance.PlayerAdded += OnPlayerAdded;
        PlayerManager.Instance.PlayerRemoved += OnPlayerRemoved;
        UpdatePlayerCount();
        InitPlayerEpList();
        UpdateMinMaxRank();
    }

    private void OnBasicCountdownAborted()
    {
        PhotonNetwork.CurrentRoom.IsOpen = !IsRoomFull;
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(RoomPropertyKeys.RoomState))
            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable
                {[RoomPropertyKeys.RoomState] = RoomConfiguration.RoomState.Lobby});
    }

    private void OnBasicCountdownStarted(float countdownTime)
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(RoomPropertyKeys.RoomState)) {
            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable
                {[RoomPropertyKeys.RoomState] = RoomConfiguration.RoomState.Loading});
        }
    }

    private void InitPlayerEpList()
    {
        PlayerManager.Instance.GetAllParticipatingHumanPlayers(out var players, out var count);
        for (int i = 0; i < count; i++)
        {
            if (!_playerRanks.ContainsKey(players[i]))
                _playerRanks.Add(players[i], players[i].Rank);
        }
    }

    private void UpdateMinMaxRank()
    {
        var sort = _playerRanks.Values.ToList();
        if (sort.Count == 0) return;
        sort.Sort();
        var currentRoomCustomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        var newHasTable = new Hashtable();
        if (currentRoomCustomProperties.ContainsKey(RoomPropertyKeys.MinRank)
            && HasPropertyChanged(currentRoomCustomProperties, RoomPropertyKeys.MinRank, (byte) sort[0]))
            newHasTable.Add(RoomPropertyKeys.MinRank, (byte) sort[0]);
        if (currentRoomCustomProperties.ContainsKey(RoomPropertyKeys.MaxRank)
            && HasPropertyChanged(currentRoomCustomProperties, RoomPropertyKeys.MaxRank, (byte) sort[sort.Count - 1]))
            newHasTable.Add(RoomPropertyKeys.MaxRank, (byte) sort[sort.Count - 1]);

        PhotonNetwork.CurrentRoom.SetCustomProperties(newHasTable);
    }

    private bool HasPropertyChanged(Hashtable properties, string key, byte newValue)
    {
        return (byte)  properties[key] != newValue;
    }

    private void OnDisable()
    {
        GameManager.Instance.MatchConfigurationStarted -= OnMatchConfigurationStarted;
        GameManager.Instance.MatchHasFinishedLoading -= OnMatchHasFinishedLoading;
        GameManager.Instance.BasicCountdownStarted -= OnBasicCountdownStarted;
        GameManager.Instance.BasicCountdownAborted -= OnBasicCountdownAborted;
        PlayerManager.Instance.PlayerAdded -= OnPlayerAdded;
        PlayerManager.Instance.PlayerRemoved -= OnPlayerRemoved;
    }

    private void OnPlayerAdded(IPlayer player)
    {
        if (!_playerRanks.ContainsKey(player))
        {
            _playerRanks.Add(player, player.Rank);
            UpdateMinMaxRank();
        }

        UpdatePlayerCount();
    }

    private void OnPlayerRemoved(IPlayer player)
    {
        if (player.IsMe)
        {
            Destroy(this);
            return;
        }

        if (_playerRanks.ContainsKey(player))
        {
            _playerRanks.Remove(player);
            UpdateMinMaxRank();
        }

        UpdatePlayerCount();
        if (RoomWasFull)
        {
            PhotonNetwork.CurrentRoom.IsOpen = true;
        }
    }

    private static void UpdatePlayerCount()
    {
        var currentRoomCustomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        if (currentRoomCustomProperties.ContainsKey(RoomPropertyKeys.CurrentPlayers))
        {
            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable
            {
                [RoomPropertyKeys.CurrentPlayers] = (byte)PlayerManager.Instance.GetAllConnectedPlayerCount()
            }); ;
        }
    }

    private void OnMatchConfigurationStarted()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(RoomPropertyKeys.RoomState)
            && PhotonNetwork.NetworkingClient.State != ClientState.Leaving) {
            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable
                {[RoomPropertyKeys.RoomState] = RoomConfiguration.RoomState.Lobby});
        }
    }

    private void OnMissionBriefingStarted(MatchDescription obj, GameMode gameMode)
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(RoomPropertyKeys.RoomState)) {
            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable
                {[RoomPropertyKeys.RoomState] = RoomConfiguration.RoomState.Loading});
        }
    }

    private void OnMatchHasFinishedLoading(IMatch match)
    {
        match.Started += OnMatchStarted;
    }

    private void OnMatchStarted(IMatch match)
    {
        PhotonNetwork.CurrentRoom.IsOpen = true;
        match.Started -= OnMatchStarted;
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(RoomPropertyKeys.RoomState)) {
            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable
                {[RoomPropertyKeys.RoomState] = RoomConfiguration.RoomState.Match});
        }
    }
}