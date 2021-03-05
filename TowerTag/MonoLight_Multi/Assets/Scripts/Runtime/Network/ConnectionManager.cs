using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExitGames.Client.Photon;
using GameManagement;
using Home.UI;
using JetBrains.Annotations;
using Network;
using Photon.Pun;
using Photon.Realtime;
#if (UNITY_STANDALONE || UNITY_EDITOR) && !UNITY_ANDROID
using Steamworks;
#endif
using TowerTag;
using TowerTagSOES;
using UI;
using UnityEngine;
using IPlayer = TowerTag.IPlayer;
using Player = Photon.Realtime.Player;

public class ConnectionManager : SingletonMonoBehaviourPunCallbacks<ConnectionManager>
{
    public delegate void OnConnectedToMasterDelegate();
    public delegate void RoomDelegate(ConnectionManager connectionManager, IRoom room);

    public delegate void LobbyDelegate(ConnectionManager connectionManager);

    public delegate void ConnectionStateDelegate(ConnectionManager connectionManager, ConnectionState previousState,
        ConnectionState newState);

    public delegate void ErrorDelegate(ConnectionManager connectionManager, MessagesAndErrors.ErrorCode errorCode);

    public event Action<ConnectionManager, Player> MasterClientSwitched;

    // own State Representation
    public enum ConnectionState
    {
        Disconnected,
        Connecting,
        ConnectedToServer,
        MatchMaking,
        ConnectedToGame,
        Undefined
    }

    // events:
    public event ConnectionStateDelegate ConnectionStateChanged;
    public event RoomDelegate JoinedRoom;
    public event LobbyDelegate JoinedLobby;
    public event ErrorDelegate ErrorOccured;

    public OnConnectedToMasterDelegate _onConnectedToMasterDelegate;

    // Member
    [Header("Connection Manager")] [SerializeField]
    private PunLogLevel _logLevel = PunLogLevel.ErrorsOnly;

    [SerializeField] private bool _connectAutomaticallyAtStartup;

    public string GameVersion { get; set; } // public getter is used in build

    [SerializeField,
     Tooltip("Current Version of the Main Menu (saved in Player Prefs), increment + 1 in Case of Menu Updates")]
    private int _mainMenuVersion = 1;

    [SerializeField, Tooltip("Show Main Menu always. Also on Auto Start")]
    private bool _showMainMenuAlways;

    private enum ConnectionMode
    {
        Offline,
        Cloud,
        LAN
    }

    [SerializeField] private ConnectionMode _connectionMode;
    [SerializeField] private string _nonChinaAppID = "81d08d16-27b3-4b59-9ae0-774842438288";
    [SerializeField] private string _nonChinaVoiceAppID = "b8c03042-dcb1-493f-8995-ce8e892886e8";
    [SerializeField] private string _chinaAppID = "295bcb6a-d36d-4191-8771-d64ade3c2d39";
    [SerializeField] private string _chinaVoiceAppID = "a3091ac8-603e-4629-bb7e-91a1023435f3";

    private const int SendRate = 20;

    private const int SerializationRate = 10;

    [SerializeField] private MessageQueue _overlayMessageQueue;

    public bool Rejoining { get; private set; }

    private ConnectionState _internalState = ConnectionState.Undefined;
    private Coroutine _rejoinRoomCoroutine;

    private bool _queryPending;

    private IPhotonService _photonService;
    private IMatchMaker _matchMaker;

    public ConnectionState ConnectionManagerState
    {
        get => _internalState;
        set
        {
            if (_internalState != value)
            {
                ConnectionState previousState = _internalState;
                _internalState = value;
                ConnectionStateChanged?.Invoke(this, previousState, value);
            }
        }
    }

    public static bool IsRegionAlreadySelected => !ConfigurationManager.Configuration.PreferredRegion.Equals("best");

    private IPhotonService PhotonService => _photonService ?? (_photonService = ServiceProvider.Get<IPhotonService>());

    private ISceneService _sceneService;

    private new void Awake()
    {
        base.Awake();
        StartCoroutine(Init());
    }


#if (UNITY_STANDALONE || UNITY_EDITOR) && !UNITY_ANDROID
    private HAuthTicket hAuthTicket;
    private void OnConnectedToServer() => SteamUser.CancelAuthTicket(hAuthTicket);
    public string GetSteamAuthTicket(out HAuthTicket hAuthTicket)
    {
        byte[] ticketByteArray = new byte[1024];
        uint ticketSize;
        hAuthTicket = SteamUser.GetAuthSessionTicket(ticketByteArray, ticketByteArray.Length, out ticketSize);
        System.Array.Resize(ref ticketByteArray, (int)ticketSize);
        StringBuilder sb = new StringBuilder();
        for(int i=0; i < ticketSize; i++)
            sb.AppendFormat("{0:x2}", ticketByteArray[i]);
        return sb.ToString();
    }
#endif

    private IEnumerator Init()
    {
        Debug.LogFormat("{0} is initializing.", typeof(ConnectionManager).Name);

        if(!GameInitialization.Initialized)
            yield return new WaitUntil(() => 
                GameInitialization.Initialized && StartUp.Finished);

        _photonService = ServiceProvider.Get<IPhotonService>();
        _matchMaker = ServiceProvider.Get<IMatchMaker>();
        _sceneService = ServiceProvider.Get<ISceneService>();

        if (PhotonNetwork.IsConnected)
            Disconnect();
        else ConnectionManagerState = ConnectionState.Disconnected;
            

        // Config Photon Network

        // these to are just for debug purpose, should stay constant in release
        PhotonNetwork.SendRate = SendRate;
        PhotonNetwork.SerializationRate = SerializationRate;

        // cache up MonoBehaviours to speedup RPCs
        PhotonNetwork.UseRpcMonoBehaviourCache = true;

        // set log level for debug purpose
        PhotonNetwork.LogLevel = _logLevel;
        // we handle scene sync by our own
        PhotonNetwork.AutomaticallySyncScene = false;

        // ToDo: generate global unique userIDs and check if necessary at all
        // after upgraded to PUNVoice 1.13.1, Photon wants a global unique userID -> i use a simple solution but it is not safe to be global unique!!!
        // should be unique between different PCs (using deviceUniqueIdentifier and time the userID is created)
        // but not for multiple instances on the same PC (could be initializing/connecting at the same time, rare but possible)
        string userID = PlayerProfileManager.CurrentPlayerProfile.PlayerGUID + DateTime.Now.Ticks;
        //Debug.Log("ConnectionManager.Init: generated userID: " + userID);
        if (TowerTagSettings.Home) {
            PlayerIdManager.GetInstance(out var playerIdManager);
            if (!playerIdManager.IsReady)
                yield return new WaitUntil(() => playerIdManager.IsReady);

            userID = playerIdManager.GetUserId();
        }

        //ViveportSdkManager has to do the authentication
        if (!TowerTagSettings.IsHomeTypeViveport && !TowerTagSettings.IsHomeTypeOculus) {
#if (UNITY_EDITOR || UNITY_STANDALONE) && !UNITY_ANDROID
            string ticket = GetSteamAuthTicket(out hAuthTicket);
            Debug.LogFormat("Authenticating user for Steam with Photon using ID: \"{0}\" and ticket: \"{1}\".", userID, ticket);
            PhotonNetwork.AuthValues = new AuthenticationValues(userID);
            PhotonNetwork.AuthValues.AuthType = CustomAuthenticationType.Steam;

            PhotonNetwork.AuthValues.UserId = userID;
            PhotonNetwork.AuthValues.AddAuthParameter("ticket", ticket);
#endif
        }

        if (TowerTagSettings.Home) {
            var homeManager = gameObject.AddComponent<ConnectionManagerHome>();
            homeManager.Init(this);
        }

        else {
            var proManager = gameObject.AddComponent<ConnectionManagerPro>();
            proManager.Init(this, _overlayMessageQueue);
        }
    }


    private new void OnEnable()
    {
        base.OnEnable();
        if (_sceneService != null)
            _sceneService.ConnectSceneLoaded += OnConnectSceneLoaded;
    }

    private new void OnDisable()
    {
        base.OnDisable();
        if (_sceneService != null)
            _sceneService.ConnectSceneLoaded -= OnConnectSceneLoaded;
    }

    private void OnConnectSceneLoaded()
    {
        // Connect or load the next scene
        if (_connectAutomaticallyAtStartup || BalancingConfiguration.Singleton.AutoStart)
        {
            // Load Scene to measure Pillar offsets
            if (SharedControllerType.PillarOffsetController)
            {
                TTSceneManager.Instance.LoadPillarOffsetScene();
            }
            else
            {
                // Check player pref for main menu version
                if (PlayerPrefs.HasKey(PlayerPrefKeys.MainMenuVersion)
                    && PlayerPrefs.GetInt(PlayerPrefKeys.MainMenuVersion) == _mainMenuVersion
                    && !_showMainMenuAlways)
                {
                    Connect();
                }
                else
                {
                    // set current Main Menu Version
                    PlayerPrefs.SetInt(PlayerPrefKeys.MainMenuVersion, _mainMenuVersion);
                }
            }
        }
    }

    // Connect to Photon Servers
    public void Connect()
    {
        // TODO (Micha):q&d fix to enable on button join if join Room failed -> refactor
        if (ConnectionManagerState == ConnectionState.MatchMaking)
        {
            StartMatchmaking();
            return;
        }

        // please disconnect first!!!
        if (PhotonNetwork.IsConnected)
        {
            Debug.LogWarning("You are still connected! Please disconnect before you try to connecting again.");
            return;
        }

        // set current state to connecting...
        ConnectionManagerState = ConnectionState.Connecting;

        Configuration conf = ConfigurationManager.Configuration;

        // read connection settings from config
        _connectionMode = conf.PlayInLocalNetwork ? ConnectionMode.LAN : ConnectionMode.Cloud;

        // play offline, don't connect to cloud or Lan Server
        if (_connectionMode == ConnectionMode.Offline)
        {
            PhotonNetwork.OfflineMode = true;
            return;
        }

        Debug.LogFormat("{0}: Connecting...", typeof(ConnectionManager).Name);

        PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = GameVersion;
        // play on local LAN Server
        if (_connectionMode == ConnectionMode.LAN)
        {
            Debug.Log("Connecting over LAN.");

            PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer = false;
            PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "";
            PhotonNetwork.PhotonServerSettings.AppSettings.Server = conf.ServerIp;
            PhotonNetwork.PhotonServerSettings.AppSettings.Port = conf.ServerPort;
            _photonService.ConnectUsingSettings();
            return;
        }

        if (ConfigurationManager.Configuration.PreferredRegion == "cn")
        {
            PhotonNetwork.PhotonServerSettings.AppSettings.Server = "ns.photonengine.cn";
            PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = _chinaAppID;
            PhotonNetwork.PhotonServerSettings.AppSettings.AppIdVoice = _chinaVoiceAppID;
        }
        else
        {
            PhotonNetwork.PhotonServerSettings.AppSettings.Server = null;
            PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = _nonChinaAppID;
            PhotonNetwork.PhotonServerSettings.AppSettings.AppIdVoice = _nonChinaVoiceAppID;
        }

        // play on cloud
        if (_connectionMode == ConnectionMode.Cloud)
        {
            Debug.Log("Trying to connect to cloud.");

            PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer = true;
            if (ConfigurationManager.Configuration.PreferredRegion != "cn")
                PhotonNetwork.PhotonServerSettings.AppSettings.Server = "";
            PhotonNetwork.PhotonServerSettings.AppSettings.Port = 0;
            PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = conf.PreferredRegion == "best"
                ? ""
                : conf.PreferredRegion;

            PhotonService.ConnectUsingSettings();
        }
    }

    private static bool ValidateRoomName(string roomName)
    {
        return roomName != "MyTestRoom"
               && roomName != null
               && roomName.All(char.IsLetterOrDigit)
               && roomName.Length >= 5;
    }

    public void StartMatchmaking()
    {
        if (!ValidateRoomName(ConfigurationManager.Configuration.Room))
        {
            UnityEngine.Debug.LogWarning(
                $"Tried to connect with illegal room name {ConfigurationManager.Configuration.Room}");
            ShowRoomRenameAdvice();
            return;
        }

        if ((SharedControllerType.IsAdmin || TowerTagSettings.BasicMode) &&
            string.IsNullOrEmpty(ConfigurationManager.Configuration.LocationName))
        {
            UnityEngine.Debug.LogWarning(
                $"Tried to connect with illegal location name {ConfigurationManager.Configuration.LocationName}");
            ShowLocationRenameAdvice();
            return;
        }

        if (!_photonService.IsConnectedAndReady
            || _photonService.NetworkClientState == ClientState.Joined
            || _photonService.NetworkClientState == ClientState.Joining)
            return;

        ConnectionManagerState = ConnectionState.MatchMaking;

        if (TowerTagSettings.Home)
            _matchMaker.StartRandomMatchMaking();
        else
            _matchMaker.StartMatchMaking();
    }

    public void LeaveRoom()
    {
        if (!_photonService.IsConnectedAndReady)
            return;
        PhotonNetwork.LeaveRoom();
        TTSceneManager.Instance.LoadConnectScene(false);
        Rejoining = false;
    }

    /// <summary>
    /// Disconnect from Photon Server.
    /// </summary>
    public void Disconnect()
    {
        if (!PhotonNetwork.IsConnected)
        {
            return;
        }

        PhotonNetwork.Disconnect();
        Rejoining = false;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"Disconnected from Photon Server: {cause}. Previous state {ConnectionManagerState}");
        if (ConnectionManagerState == ConnectionState.Disconnected)
            return;
        ConnectionManagerState = ConnectionState.Disconnected;

        if (Rejoining)
        {
            OnTerminalConnectionFailure(cause);
            return;
        }

        // test Rejoin
        switch (cause)
        {
            case DisconnectCause.ServerTimeout:
            case DisconnectCause.ClientTimeout:
            case DisconnectCause.AuthenticationTicketExpired:
            case DisconnectCause.ExceptionOnConnect:
            case DisconnectCause.Exception:
                Rejoining = true;
                break;
            case DisconnectCause.MaxCcuReached:
                ErrorOccured?.Invoke(this, MessagesAndErrors.ErrorCode.OnPhotonMaxCcuReached);
                break;
            case DisconnectCause.DisconnectByClientLogic:
                Rejoining = false;
                break;
            default:
                OnTerminalConnectionFailure(cause);
                return;
        }

        if (Rejoining && !PhotonNetwork.ReconnectAndRejoin())
        {
            OnTerminalConnectionFailure(cause);
        }
    }

    public void ResetSettingsToStartValues()
    {
        PhotonNetwork.NetworkingClient.State = ClientState.PeerCreated;
        ConnectionManagerState = ConnectionState.Disconnected;
        PhotonNetwork.NetworkingClient.OnStatusChanged(StatusCode.Connect);
    }

    private void OnTerminalConnectionFailure(DisconnectCause cause)
    {
        Debug.LogError($"Connection failed: {cause}. Will not try to reconnect again. Loading connect scene.");
        Rejoining = false;
        ResetSettingsToStartValues();
        ErrorOccured?.Invoke(this, MessagesAndErrors.ErrorCode.OnConnectionFail);
        if (TTSceneManager.Instance != null) TTSceneManager.Instance.LoadConnectScene(true);
    }

    public void ShowRoomRenameAdvice()
    {
        if (!_queryPending)
        {
            _overlayMessageQueue.AddInputFieldMessage(
                "Please note:\n" +
                "1. Enter a unique room name with 5+ characters that no one else will use (e.g. YourNameYourStreet123).\n" +
                "2. Use the same room name on all PCs that you want to play together.",
                "Room name..",
                ConfigurationManager.Configuration.Room,
                "Enter new room name",
                InputFieldHelper.InputFieldType.PlayerName,
                () => { _queryPending = true; },
                () => { _queryPending = false; },
                "Confirm",
                RenameRoom,
                "Cancel",
                null,
                ValidateRoomName);
        }

        _queryPending = true;
    }

    private void ShowLocationRenameAdvice()
    {
        if (!_queryPending)
        {
            _overlayMessageQueue.AddInputFieldMessage(
                "Please set your location name (e.g. YourArcadeName)",
                "Location name...",
                ConfigurationManager.Configuration.LocationName,
                "Enter your location name",
                InputFieldHelper.InputFieldType.PlayerName,
                () => { _queryPending = true; },
                () => { _queryPending = false; },
                "Confirm",
                newName =>
                {
                    RenameLocation(newName);
                    Connect();
                },
                "Cancel");
        }

        _queryPending = true;
    }

    private void RenameRoom(string newName)
    {
        _queryPending = false;
        ConfigurationManager.Configuration.Room = newName;
        ConfigurationManager.WriteConfigToFile();
    }

    private void RenameLocation(string newName)
    {
        _queryPending = false;
        ConfigurationManager.Configuration.LocationName = newName;
        ConfigurationManager.WriteConfigToFile();
    }

    private static IEnumerator RetryJoinRoom()
    {
        yield return new WaitForSeconds(BitCompressionConstants.RetryJoinRoomTime);
        PhotonNetwork.JoinRoom(ConfigurationManager.Configuration.Room);
    }

    private void StartRejoinRoomCoroutine()
    {
        if (_rejoinRoomCoroutine != null) StopCoroutine(_rejoinRoomCoroutine);
        _rejoinRoomCoroutine = StartCoroutine(RetryJoinRoom());
    }

    private void StopRejoinRoomCoroutine()
    {
        if (_rejoinRoomCoroutine != null) StopCoroutine(_rejoinRoomCoroutine);
        _rejoinRoomCoroutine = null;
        if (PhotonNetwork.IsConnected) PhotonNetwork.Disconnect();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"Failed to join room: {returnCode} | {message}");

        // could not join the room because it does not exist
        if (returnCode == ErrorCode.GameDoesNotExist)
        {
            // schedule new attempt to join room. Abort if overlay is closed.
            MessagesAndErrors.ErrorMessage errorMessage = MessagesAndErrors.GetErrorMessage(
                MessagesAndErrors.ErrorCode.OnPhotonJoinRoomFailedRoomDoesNotExist);
            _overlayMessageQueue.AddVolatileButtonMessage(errorMessage.Description, errorMessage.ShortDescription,
                StartRejoinRoomCoroutine, StopRejoinRoomCoroutine, "CANCEL");
        }
        // could not join the room (abstract info)
        else
        {
            ErrorOccured?.Invoke(this, MessagesAndErrors.ErrorCode.OnPhotonJoinRoomFailed);
        }

        //if (_rejoin)
        {
            // client with this userID is already joined -> Game server doesn't know we disconnected (through connection fail) yet
            if (returnCode == ErrorCode.JoinFailedFoundActiveJoiner)
            {
                // disable rejoin and disconnect/connect regularly???
                Debug.Log("Disconnect and try rejoining then!");
                Disconnect();
                PhotonNetwork.ReconnectAndRejoin();
            }

            // we try to join but have to rejoin
            if (returnCode == ErrorCode.JoinFailedFoundInactiveJoiner)
            {
                StartCoroutine(RejoinOncePossible(3));
            }
            // we try to join but not in Room list -> join normally
            else if (returnCode == ErrorCode.JoinFailedWithRejoinerNotFound)
            {
                // ?
                StartMatchmaking();
            }
        }
    }

    // when starting two basic clients simultaneously, sometimes you have to wait a bit to join the room
    private static IEnumerator RejoinOncePossible(float timeout)
    {
        PhotonNetwork.Disconnect();
        float startTime = Time.time;
        while (Time.time - startTime < timeout && PhotonNetwork.NetworkClientState != ClientState.Disconnected)
            yield return null;
        PhotonNetwork.ReconnectAndRejoin();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"Failed to join random room: {returnCode} | {message}");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to master client");
        ConnectionManagerState = ConnectionState.ConnectedToServer;
        if (_onConnectedToMasterDelegate != null)
            _onConnectedToMasterDelegate();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined lobby: " + PhotonNetwork.InLobby + " in Region: " + PhotonNetwork.CloudRegion + " in lobby: " +
                  PhotonNetwork.CurrentLobby.Name);
        JoinedLobby?.Invoke(this);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Created room");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room: PlayerID (" + _photonService.LocalPlayer.ActorNumber + "), isMaster (" +
                  _photonService.LocalPlayer.IsMasterClient + ")");
        ConnectionManagerState = ConnectionState.ConnectedToGame;

        JoinedRoom?.Invoke(this, _photonService.CurrentRoom);

        if (Rejoining)
        {
            Debug.Log("Rejoined!!!");
            Rejoining = false;
        }

        if (_rejoinRoomCoroutine != null) StopRejoinRoomCoroutine();
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Left Photon room");
        ConnectionManagerState = ConnectionState.ConnectedToServer;
    }


    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log($"Master client switched to: {newMasterClient.ActorNumber}({newMasterClient.UserId})");
        MasterClientSwitched?.Invoke(this, newMasterClient);
    }

    public override void OnPlayerEnteredRoom(Player otherPlayer)
    {
        Debug.Log($"Player entered room: {otherPlayer.ActorNumber} ({otherPlayer.UserId})");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("OnPlayerLeftRoom: " + otherPlayer.ActorNumber + "(" + otherPlayer.UserId + ")");
        IPlayer ttPlayer = PlayerManager.Instance.GetPlayer(otherPlayer.ActorNumber);
        if (ttPlayer != null && ttPlayer.GameObject != null) Destroy(ttPlayer.GameObject);
        PlayerManager.Instance.RemovePlayer(otherPlayer.ActorNumber);

        // -> Trigger PlayerCleanup
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.DestroyPlayerObjects(otherPlayer);
    }

    /// <summary>
    /// Disconnect a specific player from the Photon Server.
    /// </summary>
    /// <param name="player"></param>
    [UsedImplicitly]
    public void DisconnectPlayer(IPlayer player)
    {
        if (player?.PhotonView != null)
            PhotonNetwork.CloseConnection(player.PhotonView.Owner);
    }

    private new void OnApplicationQuit()
    {
        base.OnApplicationQuit();
        if (Instance == this)
        {
            if (PhotonNetwork.IsConnected)
            {
                Disconnect();
            }
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            if (PhotonNetwork.IsConnected)
            {
                Disconnect();
            }
        }
    }
}