using System;
using System.Collections;
using System.Collections.Generic;
using AI;
using Home.UI;
using JetBrains.Annotations;
using Network;
using Photon.Pun;
using Photon.Realtime;
using TowerTag;
using UI;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Player = Photon.Realtime.Player;

public class ConnectionManagerHome : MonoBehaviourPunCallbacks
{
    public static ConnectionManagerHome Instance { get; private set; }

    public delegate void ErrorDelegate(ConnectionManagerHome connectionManager, MessagesAndErrors.ErrorCode errorCode);

    public event ErrorDelegate ErrorOccured;

    private TypedLobby _typedLobby = new TypedLobby("TTLobby", LobbyType.SqlLobby);

    private Dictionary<string, RoomInfo> RoomInfoList { get; } = new Dictionary<string, RoomInfo>();

    public TypedLobby Lobby => _typedLobby;

    //Debug UI
    [SerializeField] private bool _showIMGUI;
    private readonly Rect _viewRect = new Rect(10, 180, 500, 400);
    private bool _joiningRoom;
    private bool _firstRandomJoinTry = true;
    private string _roomNameInput;
    private string _maxPlayerInput;
    private Vector2 _scrollPos;
    private bool _creatingRoom;
    private RoomOptionManager _roomOptionManager;

    public void Init(ConnectionManager connectionManager)
    {
        if (Instance == null)
            Instance = this;
        if (!ConnectionManager.IsRegionAlreadySelected)
        {
            ConfigurationManager.Configuration.PreferredRegion = PhotonRegionHelper.GetRegionCodeByName("GLOBAL / EU");
            ConfigurationManager.WriteConfigToFile();
        }

        ConnectionManager.Instance.Connect();

        RegisterEventListeners();
    }

    private void RegisterEventListeners()
    {
        OfflineHubUIController.QuickJoinButtonPressed += OnQuickJoinButtonPressed;
        TrainingsUIController.StartBotMatchButtonPressed += CreateBotMatchRoom;
    }

    [UsedImplicitly]
    public bool JoinSpecificRoom(string roomName)
    {
        ConfigurationManager.Configuration.Room = roomName;
        return PhotonNetwork.JoinRoom(roomName);
    }

    private bool JoinRandomRoom()
    {
        string pin = "";
        string sqlFilter = $"{RoomPropertyKeys.PIN} = \"{pin}\" " +
                           $"AND {RoomPropertyKeys.CurrentPlayers} BETWEEN 0 AND 8";
        return PhotonNetwork.JoinRandomRoom(null, 0, MatchmakingMode.FillRoom, _typedLobby, sqlFilter);
    }

    [UsedImplicitly]
    public bool CreateRoom(string roomName, int maxPlayer = 0)
    {
        if (RoomInfoList.ContainsKey(roomName))
        {
            Debug.LogWarning("Can't create room, name already taken");
            return false;
        }

        ConfigurationManager.Configuration.Room = roomName;
        RoomOptions options = RoomConfiguration.RoomOptions;

        if (maxPlayer > 0 && maxPlayer <= TowerTagSettings.MaxUsersPerRoom)
            options.MaxPlayers = (byte) maxPlayer;
        return PhotonNetwork.CreateRoom(roomName, options);
    }

    [UsedImplicitly]
    public bool CreateRoom(string roomName, RoomOptions options)
    {
        if (RoomInfoList.ContainsKey(roomName))
        {
            Debug.LogWarning("Can't create room, name already taken");
            return false;
        }

        return PhotonNetwork.CreateRoom(roomName, options);
    }

    public override void OnConnectedToMaster()
    {
// #if UNITY_EDITOR
        // _typedLobby = new TypedLobby("EditorTTLobby", LobbyType.SqlLobby);
// #endif
        PhotonNetwork.JoinLobby(_typedLobby);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        switch (cause)
        {
            case DisconnectCause.InvalidAuthentication:
            case DisconnectCause.ServerTimeout:
            case DisconnectCause.ClientTimeout:
                StartCoroutine(TryToReconnect());
                break;
        }
    }

    bool _messageDisplayed;

    private IEnumerator TryToReconnect()
    {
        while (Application.internetReachability == NetworkReachability.NotReachable)
        {
            if (!_messageDisplayed)
            {
                MessageQueue.Singleton.AddVolatileMessage("Connection lost. Please check your internet connection!",
                    "Error", null, OnConnectionLostWindowClosed);
                _messageDisplayed = true;
            }

            Debug.LogError("Error. Check internet connection!");
            yield return new WaitForSeconds(1);
        }

        _messageDisplayed = false;
        ConnectionManager.Instance.ResetSettingsToStartValues();
        ConnectionManager.Instance.Connect();
        yield return null;
    }

    private void OnConnectionLostWindowClosed()
    {
        _messageDisplayed = false;
    }

    public override void OnJoinedLobby()
    {
        if (BalancingConfiguration.Singleton.AutoStart)
            JoinSpecificRoom(ConfigurationManager.Configuration.Room);
        Debug.Log("Joined lobby");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        roomList.ForEach(room =>
        {
            if (!RoomInfoList.ContainsValue(room))
                RoomInfoList.Add(room.Name, room);
            else
            {
                if (room.RemovedFromList)
                    RoomInfoList.Remove(room.Name);
                else
                    RoomInfoList[room.Name] = room;
            }
        });
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"Failed to join room: {returnCode} | {message}");
        ErrorOccured?.Invoke(this, MessagesAndErrors.ErrorCode.OnPhotonJoinRoomFailed);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        if (_firstRandomJoinTry)
        {
            _firstRandomJoinTry = false;
            string pin = "";
            string sqlFilter = $"{RoomPropertyKeys.PIN} = \"{pin}\" " +
                               $"AND {RoomPropertyKeys.CurrentPlayers} BETWEEN 0 AND 8";
            PhotonNetwork.JoinRandomRoom(null, 0, MatchmakingMode.FillRoom, _typedLobby, sqlFilter);
        }
        else
        {
            ConfigurationManager.Configuration.Room = PlayerProfileManager.CurrentPlayerProfile.PlayerGUID;
            PhotonNetwork.CreateRoom(ConfigurationManager.Configuration.Room,
                RoomConfiguration.GetRandomRoomOptions());
            _firstRandomJoinTry = true;
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"Failed to create room: {returnCode} | {message}");
        ErrorOccured?.Invoke(this, MessagesAndErrors.ErrorCode.OnPhotonCreateRoomFailed);
        ConfigurationManager.Configuration.Room =
            PlayerProfileManager.CurrentPlayerProfile.PlayerGUID; //+ Random.Range(0, 100);
        PhotonNetwork.CreateRoom(ConfigurationManager.Configuration.Room, RoomConfiguration.RoomOptions,
            _typedLobby);
    }

    public override void OnJoinedRoom()
    {
        ConfigurationManager.Configuration.Room = PhotonNetwork.CurrentRoom.Name;
        Hashtable customRoomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        if (customRoomProperties.ContainsKey(RoomPropertyKeys.HomeMatchType))
        {
            GameManager.Instance.CurrentHomeMatchType =
                (GameManager.HomeMatchType) customRoomProperties[RoomPropertyKeys.HomeMatchType];
        }

        if (!PhotonNetwork.IsMasterClient) return;

        _roomOptionManager = gameObject.AddComponent<RoomOptionManager>();
    }

    private void OnQuickJoinButtonPressed(OfflineHubUIController sender)
    {
        JoinRandomRoom();
    }

    public static string GenerateRandomMatchUID()
    {
        return $"Match_{Guid.NewGuid()}";
    }

    /// <summary>
    /// Home: Creates a room limited to the local player + enemyCount and sets TrainingVsAI true and starts a Coroutine
    /// to spawn the enemy bots
    /// </summary>
    /// <param name="trainingsUIController">Sender</param>
    /// <param name="enemyCount">Count of enemies you want to join the bot match</param>
    /// <param name="botLevel">The difficulty of the bots</param>
    private void CreateBotMatchRoom(TrainingsUIController trainingsUIController, int enemyCount,
        BotBrain.BotDifficulty botLevel)
    {
        string roomName = Guid.NewGuid().ToString();
        ConfigurationManager.Configuration.Room = roomName;
        RoomOptions options = RoomConfiguration.GetTrainingOptions();
        CreateRoom(roomName, options);

        StartCoroutine(BotManagerHome.Instance.SpawnBotForTeamWhenOwnPlayerAvailable(TeamID.Fire, enemyCount,
            () => TTSceneManager.Instance.IsInHubScene, botLevel, true));
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (newMasterClient.IsLocal)
        {
            _roomOptionManager = gameObject.AddComponent<RoomOptionManager>();
        }
        else
        {
            if (_roomOptionManager != null)
                Destroy(_roomOptionManager);
        }
    }

    public override void OnLeftRoom()
    {
        if (_roomOptionManager != null)
            Destroy(_roomOptionManager);
    }

    public void ChangeRegion(string regionName, HubUIController uiController,
        HubUIController.PanelType panelToLoadIn = HubUIController.PanelType.MainMenu)
    {
        StartCoroutine(PhotonRegionHelper.ChangeRegion(regionName, uiController, panelToLoadIn));
    }

    void OnGUI()
    {
        if (!_showIMGUI) return;
        GUI.Box(_viewRect, "");
        GUILayout.BeginArea(_viewRect);
        {
            GUILayout.Label($"*** ConnectionManagerHome");
            GUILayout.Label($"Connected to Master: {PhotonNetwork.IsConnected}");

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Connect to Master"))
                    ConnectionManager.Instance.Connect();
                if (GUILayout.Button("Disconnect from Master"))
                    ConnectionManager.Instance.Disconnect();
                if (PhotonNetwork.InRoom)
                {
                    if (GUILayout.Button("Leave Room"))
                        ConnectionManager.Instance.LeaveRoom();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Join Room"))
                    _joiningRoom = true;
                if (GUILayout.Button("Join Random"))
                    JoinRandomRoom();
                if (GUILayout.Button("Create Room"))
                    _creatingRoom = true;
            }
            GUILayout.EndHorizontal();
            if (_creatingRoom)
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Room Name");
                    _roomNameInput = GUILayout.TextField(_roomNameInput, GUILayout.MinWidth(100));
                    GUILayout.Label("Max Players");
                    _maxPlayerInput = GUILayout.TextField(_maxPlayerInput, GUILayout.MinWidth(10));
                    if (GUILayout.Button("Create"))
                    {
                        _creatingRoom = !CreateRoom(_roomNameInput, Convert.ToInt32(_maxPlayerInput));
                    }
                }
                GUILayout.EndHorizontal();
            }

            if (_joiningRoom)
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Enter Room Name");
                    _roomNameInput = GUILayout.TextField(_roomNameInput, GUILayout.MinWidth(100));
                    if (GUILayout.Button("Join"))
                    {
                        ConfigurationManager.Configuration.Room = _roomNameInput;
                        _joiningRoom = !JoinSpecificRoom(_roomNameInput);
                    }
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.Label("RoomList:");
            int i = 1;
            string rooms = "Number | Name | IsOpen | PlayerCount/MaxPlayer | RemovedFromList | IsVisible\n";
            //_roomInfoList.Sort((r1,r2) => r1.PlayerCount.CompareTo(r2.PlayerCount));
            RoomInfoList.ForEach(room => rooms += $"{i++} | {room.Value.Name} | {room.Value.IsOpen} " +
                                                  $"| {room.Value.PlayerCount}/{room.Value.MaxPlayers} | {room.Value.RemovedFromList} " +
                                                  $"| {room.Value.IsVisible}\n");
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, false, true);
            GUILayout.Label(rooms);
            GUILayout.EndScrollView();
            GUILayout.Label("Waiting time: " + QueueTimerManager.RestWaitingTime);
            GUILayout.Label("Queueing since: " + QueueTimerManager.HubSceneTime);
            if (PhotonNetwork.CurrentRoom != null)
            {
                GUILayout.Label("Current in Room: " + PhotonNetwork.CurrentRoom.PlayerCount + "Max: " +
                                PhotonNetwork.CurrentRoom.MaxPlayers);
            }
        }
        GUILayout.EndArea();
    }
}