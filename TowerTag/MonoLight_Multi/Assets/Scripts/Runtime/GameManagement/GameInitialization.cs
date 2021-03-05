using System;
using System.Text;
using CustomBuildPipeline;
using Hologate;
using JetBrains.Annotations;
using Network;
using Photon.Realtime;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using Rewards;
using Toornament;
using TowerTag;
using TowerTagSOES;
using UnityEditor;
using UnityEngine;
using IPlayer = TowerTag.IPlayer;
using Player = TowerTag.Player;

// Initialization:
// Phase 1: Initialize local Manager
// Phase 2: Initialize Game Manager (when connected to a room)
// Phase 3: Create PlayerPrefab (when scene/SpawnManager is initialized)

namespace GameManagement {
    [RequireComponent(typeof(MatchTimer), typeof(RewardController), typeof(ConnectionManager))]
    public class GameInitialization : TTSingleton<GameInitialization> {
        [Header("Version")] [SerializeField, Tooltip("Sets the version for unity player settings as well as photon")]
        //string globalGameVersion;
        private SharedVersion _globalGameVersion;

        private IPhotonService _photonService;

        public string GlobalGameVersion => _globalGameVersion;

        [Space] [Header("Configuration")] [SerializeField]
        private string _configurationFileName = "Config.xml";

        [SerializeField] private TeamManager _teamManager;
        [SerializeField] private MatchSequence _basicMatchSequence;

        public delegate void OnGameInitializedAndJoinedRoom();
        public OnGameInitializedAndJoinedRoom _onGameInitializedAndJoinedRoom;

        public delegate void OnFinishedInitializing ();
        public OnFinishedInitializing _onFinishedInitializing;

        [SerializeField, UsedImplicitly] private TowerTagSettings _towerTagSettings;

        [Space] [Header("PlayerProfile")] [SerializeField]
        private string _playerProfileFileName = "PlayerProfile.xml";

        [Space] [Header("Toornament")] [SerializeField]
        private string _toornamentProfileFileName = "ToornamentProfile.xml";

        [SerializeField] private ToornamentImgui _toornamentImguiPrefab;

        [Space] [Header("Controller Type")] [SerializeField]
        private SharedControllerType _controllerType;

        [Space] [Header("Prefabs")] [SerializeField, Tooltip("Operator Prefab")]
        private AdminController _adminPrefab;

        [SerializeField, Tooltip("Instantiated for every player")]
        private Player _playerPrefab;

        [SerializeField, Tooltip("Instantiated for non-player spectators")]
        private GameObject _spectatorPrefab;

        [SerializeField, Tooltip("Prefab for handling network actions of the game manager ")]
        private GameManagerPhotonView _gameManagerPhotonViewPrefab;

        [SerializeField] private GameObject _hologateIntroVideoPrefab;

        [Space] [Header("Database")] [SerializeField]
        private GameObject _decalDatabasePrefab;
        [SerializeField]
        private SoundDatabase _soundDatabasePrefab;

        [Space] [Header("ScreenShotHandler")] [SerializeField]
        private GameObject _screenShotHandlerPrefab;

        private SoundDatabase _soundDatabase;

        private GameObject AdminVoiceInstance { get; set; }

        protected override void Init() {}

        private void LoadConfig()
        {
            ConfigurationManager.Path = Application.persistentDataPath + "/";
            ConfigurationManager.FileName = _configurationFileName;
            ConfigurationManager.LoadConfigFromFile();
        }

        private void Start() {
            _photonService = ServiceProvider.Get<IPhotonService>();
            if (TowerTagSettings.Hologate)
                InstantiateWrapper.InstantiateWithMessage(_hologateIntroVideoPrefab);
            LoadConfig();
            SetGameVersion(GlobalGameVersion);
            InitLocalManager();
        }

        protected void OnDestroy() {
            if (AdminVoiceInstance != null)
                _photonService.Destroy(AdminVoiceInstance);
            Customization.Instance.UseDefaultColors(_teamManager);
            _teamManager.Init();
        }

        // Phase 1: Initialize local Manager
        private void InitLocalManager() {

            Debug.LogFormat("{0}: Initializing...", typeof(GameInitialization).Name);

            // print Infos about machine we are running on to logfile (for debugging purpose)
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("***** Client Info *****");
            stringBuilder.AppendLine("Date: " + DateTime.Now);
            stringBuilder.AppendLine("TT Game version: " + GlobalGameVersion);
            stringBuilder.AppendLine("Device: " + SystemInfo.deviceName);
            stringBuilder.AppendLine("CPU: " + SystemInfo.processorType + " | " + SystemInfo.processorFrequency +
                                     " MHZ | " +
                                     SystemInfo.processorCount + " cores");
            stringBuilder.AppendLine("Memory: " + SystemInfo.systemMemorySize + " MB RAM");
            stringBuilder.AppendLine("Graphics: " + SystemInfo.graphicsDeviceVendor + " | " +
                                     SystemInfo.graphicsDeviceName +
                                     " | " + SystemInfo.graphicsMemorySize + " MB RAM | " +
                                     SystemInfo.graphicsDeviceVersion +
                                     " | " + SystemInfo.graphicsDeviceType + " | Shader level: " +
                                     SystemInfo.graphicsShaderLevel);
            stringBuilder.AppendLine($"Resolution: {Screen.currentResolution} | full screen: {Screen.fullScreen}");
            stringBuilder.AppendLine($"Available resolutions: {string.Join(", ", Screen.resolutions)}");
            stringBuilder.AppendLine("OS: " + SystemInfo.operatingSystem);

            if (_decalDatabasePrefab != null) {
               InstantiateWrapper.InstantiateWithMessage(_decalDatabasePrefab, transform);
            }

            if (_soundDatabasePrefab != null) {
                _soundDatabase = InstantiateWrapper.InstantiateWithMessage(_soundDatabasePrefab, transform);
                _soundDatabase.Init();
            }

            if (_screenShotHandlerPrefab != null) {
                InstantiateWrapper.InstantiateWithMessage(_screenShotHandlerPrefab, transform);
            }

            // ConfigurationManager
            ConfigurationManager.Path = Application.persistentDataPath + "/";
            ConfigurationManager.FileName = _configurationFileName;
            ConfigurationManager.LoadConfigFromFile();
            //ConfigurationManager.WriteConfigToFile();

            // customize logos and materials
            Customization.Instance.CustomizeLogos(ConfigurationManager.Configuration.CustomizeLogo);

            if (ConfigurationManager.Configuration.CustomizeColors) {
                Customization.Instance.CustomizeColors(_teamManager);
            }
            else {
                Customization.Instance.UseDefaultColors(_teamManager);
            }

            _teamManager.Init();

            // PlayerProfileManager
            PlayerProfileManager.Path = Application.persistentDataPath + "/";
            PlayerProfileManager.FileName = _playerProfileFileName;
            PlayerProfileManager.LoadFromFile();

            // CommandLineArgumentsProcessor: process flags set in bat Files and save the settings to
            // Configuration/BalancingConfiguration/PlayerProfile and passed controller type asset
            CommandLineArgumentsProcessor.ProcessCommandLineArguments(_controllerType);

            // controller Type cant be "admin" if the game is the Basic Version
            if (TowerTagSettings.BasicMode && SharedControllerType.IsAdmin)
                _controllerType.Set(this, ControllerType.VR);

            if (!SharedControllerType.VR && !SharedControllerType.PillarOffsetController) {
                Debug.Log("Setting target frame rate to 60 frames per second");
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 60;
            }

            if(SharedControllerType.IsAdmin) {
                if (TowerTagSettings.Hologate)
                    gameObject.AddComponent<HologateController>();
            }

            // print Infos about Player to logfile (for debugging purpose)
            stringBuilder.AppendLine($"Player: {PlayerProfileManager.CurrentPlayerProfile.PlayerName} | " +
                                     $"GUID: {PlayerProfileManager.CurrentPlayerProfile.PlayerGUID}");
            stringBuilder.AppendLine($"Controller: {_controllerType}");
            Debug.Log(stringBuilder.ToString());

            // Control VR Mode
            /* SEAN CONNOR temporarily disabled for Oculus
            if (_controllerType == ControllerType.VR || _controllerType == ControllerType.PillarOffsetController) {
                VRController.ActivateOpenVR();
            }
            else {
                VRController.DeactivateOpenVR();
            }
                    StartCoroutine(LoadLevel(MySceneManager.Instance.ConnectScene));
            */

            // Toornament manager
            ToornamentProfileHolder.Instance.LoadFromFile(Application.persistentDataPath, _toornamentProfileFileName);
            if (ToornamentProfileHolder.Instance.Initialized) {
                TowerTagToornamentController.Init();
                InstantiateWrapper.InstantiateWithMessage(_toornamentImguiPrefab);
            }
            // ConnectionManager
            ConnectionManager.Instance.JoinedRoom += OnJoinedRoom;
            Initialized = true;

            if (_onFinishedInitializing != null)
                _onFinishedInitializing();
        }

        public static bool Initialized { get; private set; }

        // Phase 2: Create GameManagerPhotonView prefab if joined to a room
        //  - Attention: create only if not already created (the first Player who joins
        //    a room creates it (isMasterClient) on all other clients this prefab will automatically created by Photon)
        //  - called when Player joined a room
        // ReSharper disable once UnusedMember.Local - Called by Photon
        private void OnJoinedRoom(ConnectionManager connectionManager, IRoom room) {
            Debug.Log($"Joined room: Admin: {SharedControllerType.IsAdmin} " +
                      $"isMaster: {_photonService.IsMasterClient}"/* " +
                      $"isGameManagerPhotonView available: {GameManagerPhotonView.GetInstance(out var instance)}"*/);

            // Instantiate SceneView-PhotonView prefab
            //      - this prefab calls OnPhotonSceneViewInstantiated if it wOnJoinedRoomas initialized on all clients
            //      - ensure that this prefab is not Instantiated more than once (because it survives if Admin left
            //        the room and is instantiated/reproduced remotely)!!!
            if (_photonService.IsMasterClient) {
                _photonService.InstantiateSceneObject(_gameManagerPhotonViewPrefab.name, Vector3.zero,
                    Quaternion.identity);
            }

            if (_onGameInitializedAndJoinedRoom != null)
                _onGameInitializedAndJoinedRoom();
        }

        // Phase 3: Init all Network dependent stuff when GameManagerPhotonView prefab was created
        //  - called directly from PhotonSceneView-GameObject (GameManagerPhotonView)
        public void OnPhotonSceneViewInstantiated(GameObject photonSceneViewGameObject) {
            GameManager.Instance.Init(_photonService, GetComponent<MatchTimer>(), GetComponent<RewardController>(),
                _basicMatchSequence);

            // Init PhotonSceneView-GameObject
            var gameManagerNetworkEventHandler =
                photonSceneViewGameObject.GetComponent<GameManagerNetworkEventHandler>();
            if (gameManagerNetworkEventHandler != null) {
                gameManagerNetworkEventHandler.Init();
            }
            else {
                Debug.LogError("GameManagerNetworkEventHandler not found!");
            }

            // SpawnPlayer || InitAdmin
            if (SharedControllerType.IsAdmin) {
                InitAdmin();
            }
            else if (SharedControllerType.Spectator) {
                InitSpectator();
            }
            else {
                SpawnNetworkPlayer();
            }
        }

        private void SpawnNetworkPlayer() {
            if (_photonService.LocalPlayer == null) {
                Debug.LogError("Failed to spawn network player: PhotonPlayer is not initialized!");
                return;
            }

            if (!_photonService.InRoom) {
                Debug.LogError("Failed to spawn network player: Client is not in Room!");
                return;
            }

            Debug.Log("Instantiate Player");

            // Spawn PlayerPrefab
            GameObject playerGameObject = _photonService.Instantiate(_playerPrefab.GameObject.CheckForNull()?.name,
                Vector3.zero, Quaternion.identity);
            var player = playerGameObject.GetComponent<IPlayer>();

            if (player == null) {
                Debug.LogError("Failed to spawn network player: " +
                               "Could not find Player Component on PlayerPrefab! Abort Player Initialization!");
                return;
            }

            // Init VoiceChat
            if (player.IsMe) {
                var voicePlayer = gameObject.GetComponentInChildren<VoiceChatPlayer>();
                if (voicePlayer != null) {
                    voicePlayer.Init(VoiceChatPlayer.Role.Player, PhotonVoiceNetwork.Instance.GetComponent<Recorder>(),
                        player);
                }
            }
        }

        private void InitSpectator() {
           InstantiateWrapper.InstantiateWithMessage(_spectatorPrefab, transform);
        }

        private void InitAdmin() {
            // Set full-screen resolution to avoid issue with black screen in menu scene
            if (Screen.fullScreen) {
                Resolution maximumResolution = Screen.resolutions[Screen.resolutions.Length - 1];
                Screen.SetResolution(maximumResolution.width, maximumResolution.height, Screen.fullScreen);
            }

            // Create Admin Controller & UI if needed (only if not available)
            if (GetComponentInChildren<AdminController>() == null) {
                Debug.Log("Creating Admin");
               InstantiateWrapper.InstantiateWithMessage(_adminPrefab, transform);
            }

            // Create Admin VoiceChatPrefab if needed (only if not available)
            if (AdminVoiceInstance.IsNull()) {
                Debug.Log("Creating Admin Voice");
                AdminVoiceInstance =
                    _photonService.Instantiate("Admin_VoicePrefab", Vector3.zero, Quaternion.identity);
            }

            if (AdminVoiceInstance != null) {
                var voicePlayer = gameObject.GetComponentInChildren<VoiceChatPlayer>();
                if (voicePlayer != null) {
                    voicePlayer.Init(
                        VoiceChatPlayer.Role.Admin, PhotonVoiceNetwork.Instance.GetComponent<Recorder>(), null);
                }
            }
            else {
                Debug.LogError("Failed to initialize Admin Voice: Could not find Admin Voice Instance!");
            }

            if (!_photonService.IsMasterClient)
                _photonService.SetMasterClient(_photonService.LocalPlayer);
        }

        private static void SetGameVersion(string version) {
#if UNITY_EDITOR
            PlayerSettings.bundleVersion = version;
#endif
            ConnectionManager.Instance.GameVersion = version;
            AnalyticsController.Version = version;
        }
    }
}