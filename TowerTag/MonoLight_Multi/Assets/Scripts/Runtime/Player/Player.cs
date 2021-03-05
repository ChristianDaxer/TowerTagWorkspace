using AI;
using Network;
using Photon.Pun;
using SOEventSystem.Shared;
using System;
using Home;
using Photon.Realtime;
using Runtime.Player;
using TowerTagAPIClient.Model;
using TowerTagAPIClient.Store;
using TowerTagSOES;
using UnityEngine;
using UnityEngine.SceneManagement;
using PHashtable = ExitGames.Client.Photon.Hashtable;

namespace TowerTag {
    public class Player : MonoBehaviourPunCallbacks, IPlayer {
        #region localMember

        private IPhotonService _photonService;

        private IPhotonService PhotonService =>
            _photonService = _photonService ?? ServiceProvider.Get<IPhotonService>();

        public bool IsValid => this != null;
        public GameObject GameObject => IsValid ? gameObject : null;

        public IPhotonView PhotonView => photonView;

        [SerializeField, Tooltip("Shared bool that is true when the game is paused")]
        private SharedBool _gamePaused;

        private bool _isInTower;
        private bool _abortMatchVote;


        [SerializeField, Tooltip("Object to toggle login")]
        private StayLoggedInTrigger _stayLoggedInTrigger;

        private bool _isGunInTower;

        public StayLoggedInTrigger LoggedInTrigger => _stayLoggedInTrigger;
        private bool _isOutOfChaperone;

        public bool IsGunInTower {
            get => _isGunInTower;
            set {
                if (_isGunInTower != value) {
                    _isGunInTower = value;
                    OnGunInTowerChanged(_isGunInTower);
                }
            }
        }

        public bool IsInTower {
            get => _isInTower;
            set {
                if (_isInTower != value) {
                    _isInTower = value;
                    
                    // Toggle Gun
                    OnGunInTowerChanged(_isInTower);
                    
                    // Handle Playerstate
                    PlayerStateHandler.OnCollidingWithPillar(_isInTower);
                    InTowerStateChanged?.Invoke(this, _isInTower);
                    if (IsMe) {
                        var playerProps = new PHashtable {{_playerPropertiesInTowerKey, value}};
                        photonView.Owner.SetCustomProperties(playerProps);
                    }
                }
            }
        }
        
        public bool AbortMatchVote {
            get => _abortMatchVote;
            set {
                if (_abortMatchVote != value) {
                    _abortMatchVote = value;
                    if (IsMe) {
                        var playerProps = new PHashtable {{_playerPropertiesAbortMatchVote, value}};
                        photonView.Owner.SetCustomProperties(playerProps);
                    }
                }
            }
        }

        public bool IsOutOfChaperone {
            get => _isOutOfChaperone;
            set {
                if (_isOutOfChaperone != value) {
                    _isOutOfChaperone = value;
                    OutOfChaperoneStateChanged?.Invoke(this, _isOutOfChaperone);
                }
            }
        }

        public bool IsLateJoiner { get; set; }

        private PlayerStatusController _statusController;

        private PlayerStatusController StatusController {
            get {
                if (_statusController == null)
                    _statusController = GetComponent<PlayerStatusController>();

                return _statusController;
            }
        }

        public event Action<string> PlayerNameChanged;
        public event Action<Status> StatusChanged;
        public event Action<IPlayer, TeamID> PlayerTeamChanged;
        public event Action<int, int> CountdownStarted;
        public event Action<IPlayer, bool> InTowerStateChanged;
        public event Action<IPlayer, bool> OutOfChaperoneStateChanged;

        // Avatar stuff

        public PlayerAvatar PlayerAvatar { get; set; }
        public Collider[] GunCollider { get; set; }
        private PlayerHealth _playerHealth;

        public PlayerHealth PlayerHealth => _playerHealth != null || this == null
            ? _playerHealth
            : _playerHealth = GetComponentInChildren<PlayerHealth>();

        private RoomOptionsManager _roomOptionsManager;

        public RoomOptionsManager RoomOptionsManager => _roomOptionsManager != null || this == null
            ? _roomOptionsManager
            : _roomOptionsManager = GetComponentInChildren<RoomOptionsManager>();


        public bool IsAlive => PlayerHealth != null && PlayerHealth.IsAlive;
        private GunController _gunController;

        public GunController GunController => _gunController != null || this == null
            ? _gunController
            : _gunController = GetComponentInChildren<GunController>();

        public ChargePlayer ChargePlayer { get; private set; }

        private PlayerStatusController _playerStatus;

        public PlayerStatusController PlayerStatus => _playerStatus.CheckForNull()
                                                      ?? (_playerStatus = GetComponent<PlayerStatusController>());

        private PlayerNetworkEventHandler _playerNetworkEventHandler;

        public PlayerNetworkEventHandler PlayerNetworkEventHandler => _playerNetworkEventHandler || this == null
            ? _playerNetworkEventHandler
            : _playerNetworkEventHandler = GetComponent<PlayerNetworkEventHandler>();

        public ChargeNetworkEventHandler ChargeNetworkEventHandler { get; private set; }

        private int _playerID = -1;

        public int PlayerID {
            get {
                if (_playerID == -1 && photonView != null)
                    _playerID = photonView.ViewID;
                return _playerID;
            }
        }

        private string _membershipID;

        public int OwnerID => PhotonView.OwnerActorNr;

        /// <summary>
        /// The id of a logged-in, returning customer. Used to report statistics to the ranking services.
        /// </summary>
        public string MembershipID {
            get => _membershipID;
            set {
                if (value.Equals(_membershipID)) return;
                _membershipID = value;
                if (IsMe) _playerNetworkEventHandler.SendMemberID(_membershipID);
            }
        }

        public bool IsLoggedIn { get; private set; }

        public bool IsInIngameMenu { get; private set; }

        public bool IsLocal => IsPhotonViewValid && photonView.OwnerActorNr == _photonService.LocalPlayer.ActorNumber;

        public bool IsPhotonViewValid => photonView?.Owner != null;
        public bool IsMe => IsLocal && !IsBot;

        private bool _initialized;

        #endregion

        #region syncedMember

        // handles Teleport stuff

        public TeleportHandler TeleportHandler { get; } = new TeleportHandler();
        public RotatePlayspaceHandler RotatePlayspaceHandler { get; } = new RotatePlayspaceHandler();

        public Pillar CurrentPillar => TeleportHandler?.CurrentPillar;

        // handles internal PlayerState (limbo, gunDisabled, immortal)

        public PlayerStateHandler PlayerStateHandler { get; } = new PlayerStateHandler();

        public PlayerState PlayerState => PlayerStateHandler.PlayerState;

        // name of the player
        private string _playerName;

        public string PlayerName {
            get => _playerName;
            private set {
                if (_playerName != value) {
                    _playerName = value;
                    PlayerNameChanged?.Invoke(_playerName);
                }
            }
        }

        public float GunEnergy { get; set; } = 1;

        public int Rank {
            get => _rank;
            set {
                if (_rank != value) {
                    if (IsMe) {
                        var playerProps = new PHashtable {{_playerPropertiesRankKey, (byte) value}};
                        photonView.Owner.SetCustomProperties(playerProps);
                    }

                    _rank = value;
                }
            }
        }
        // players team

        private TeamID _teamID = TeamID.Fire;

        public TeamID TeamID {
            get => _teamID;
            private set {
                if (_teamID != value) {
                    _teamID = value;
                    if (TowerTagSettings.BasicMode && IsMe) ResetButtonStates();
                    PlayerTeamChanged?.Invoke(this, value);
                }
            }
        }
        // is player ready for the Match to start

        private bool _playerIsReady;

        public bool PlayerIsReady {
            get => _playerIsReady;
            set {
                if (_playerIsReady != value) {
                    _playerIsReady = value;
                    ReadyStatusChanged?.Invoke(this, _playerIsReady);
                }
            }
        }

        // is player ready for the Match to start

        private bool _isParticipating = true;

        public bool IsParticipating {
            get => _isParticipating;
            set {
                if (value != _isParticipating) {
                    _isParticipating = value;
                    ParticipatingStatusChanged?.Invoke(this, _isParticipating);
                }
            }
        }

        //On Master
        private bool _receivingMicInput;

        public bool ReceivingMicInput {
            get => _receivingMicInput;
            set {
                if (_receivingMicInput != value) {
                    _receivingMicInput = value;
                    ReceivingMicInputStatusChanged?.Invoke(this, _receivingMicInput);
                }
            }
        }

        public event PropertyChangedHandler ReadyStatusChanged;

        public GameMode VoteGameMode {
            get => _voteGameMode;
            set {
                if (_voteGameMode != value)
                {
                    var previous = _voteGameMode;
                    _voteGameMode = value;
                    GameModeVoted?.Invoke(this, (_voteGameMode, previous));
                    if (!IsMe) return;
                    var playerProps = new PHashtable {{_playerPropertiesGameModeKey, value}};
                    photonView.Owner.SetCustomProperties(playerProps);
                }
            }
        }

        public bool StartVotum {
            get => _startVotum;
            set {
                if (_startVotum != value) {
                    _startVotum = value;
                    StartNowVoteChanged?.Invoke(this, _startVotum);
                    if (!IsMe) return;
                    var playerProps = new PHashtable {{_playerPropertiesStartVotumKey, value}};
                    photonView.Owner.SetCustomProperties(playerProps);
                }
            }
        }

        public bool TeamChangeRequested {
            get => _teamChangeRequested;
            set {
                if (_teamChangeRequested != value) {
                    _teamChangeRequested = value;
                    TeamChangeRequestChanged?.Invoke(this, _teamChangeRequested);
                    if (!IsMe) return;
                    var playerProps = new PHashtable {{_playerPropertiesTeamChangeKey, value}};
                    photonView.Owner.SetCustomProperties(playerProps);
                }
            }
        }

        public event Action<IPlayer, (GameMode newVote, GameMode previousVote)> GameModeVoted;

        public event PropertyChangedHandler ParticipatingStatusChanged;
        public event Action<IPlayer, bool> StartNowVoteChanged;

        public event PropertyChangedHandler ReceivingMicInputStatusChanged;
        public event Action<IPlayer, bool> TeamChangeRequestChanged;

        public Status Status { get; private set; }

        public bool IsBot { get; set; }

        public BotBrain.BotDifficulty BotDifficulty { get; set; }

        public string SelectedAIParameters { get; set; } = "Default";

        public string DefaultName { get; set; }

        public bool HasRopeAttached => AttachedTo != null;

        public Chargeable AttachedTo { get; set; }

        private AlignmentTarget _cachedPlayerAlignmentTarget;
        public AlignmentTarget PlayerAlignmentTarget =>
            (IsValid ? 
                (_cachedPlayerAlignmentTarget == null ? (_cachedPlayerAlignmentTarget = GetComponentInChildren<AlignmentTarget>()) : _cachedPlayerAlignmentTarget) 
                : null);

        private string _playerPropertiesNameKey;

        #endregion

        #region MasterToClientSync

        private bool _keysInitialized;
        private BitSerializer _writeStream = new BitSerializer(new BitWriterNoAlloc(new byte[12]));
        private string _playerPropertiesTeamKey;
        private string _playerPropertiesPillarKey;
        private string _playerPropertiesPlayerStateKey;
        private string _playerPropertiesLoginStateKey;
        private BitSerializer _readStream = new BitSerializer(new BitReaderNoAlloc(new byte[4]));
        private string _playerPropertiesRotationKey;
        private string _playerPropertiesReadyKey;
        private string _playerPropertiesParticipatingKey;
        private string _playerPropertiesGunEnergyKey;
        private string _playerPropertiesGameModeKey;
        private string _playerPropertiesStartVotumKey;
        private string _playerPropertiesTeamChangeKey;
        private bool _teamChangedOnMaster;
        private string _playerPropertiesIsBotKey;
        private string _playerPropertiesRankKey;
        private string _playerPropertiesInTowerKey;
        private string _playerPropertiesAbortMatchVote;
        private bool _rotationChangedOnMaster;

        public void UpdatePlayerProperties() {
            if (!PhotonService.IsMasterClient)
                return;

            if (!_keysInitialized) InitPropertyKeys();

            var playerProps = new PHashtable();

            if (_teamChangedOnMaster) {
                _teamChangedOnMaster = false;
                playerProps.Add(_playerPropertiesTeamKey, TeamID);
            }

            if (!photonView.Owner.CustomProperties.ContainsKey(_playerPropertiesLoginStateKey)
                || (bool) photonView.Owner.CustomProperties[_playerPropertiesLoginStateKey] != IsLoggedIn) {
                playerProps.Add(_playerPropertiesLoginStateKey, IsLoggedIn);
            }

            if (TeleportHandler.TeleportedSinceLastSync) {
                // TODO (Micha): compress member for TeleportInfo
                _writeStream.Reset();
                TeleportHandler.Serialize(_writeStream);
                playerProps.Add(_playerPropertiesPillarKey, _writeStream.GetData());
            }

            if (PlayerStateHandler.PlayerStateChangedSinceLastSync) {
                _writeStream.Reset();
                PlayerStateHandler.Serialize(_writeStream);
                playerProps.Add(_playerPropertiesPlayerStateKey, _writeStream.GetData());
            }

            if (_rotationChangedOnMaster) {
                _rotationChangedOnMaster = false;
                playerProps.Add(_playerPropertiesRotationKey, transform.rotation);
            }

            if (!photonView.Owner.CustomProperties.ContainsKey(_playerPropertiesParticipatingKey)
                || (bool) photonView.Owner.CustomProperties[_playerPropertiesParticipatingKey] != IsParticipating) {
                playerProps.Add(_playerPropertiesParticipatingKey, IsParticipating);
            }

            if (!photonView.Owner.CustomProperties.ContainsKey(_playerPropertiesReadyKey)
                || (bool) photonView.Owner.CustomProperties[_playerPropertiesReadyKey] != PlayerIsReady) {
                playerProps.Add(_playerPropertiesReadyKey, PlayerIsReady);
            }

            if (playerProps.Count > 0)
                photonView.Owner.SetCustomProperties(playerProps);
        }

        private ReadyTowerUiController[] _readyTowerUiController;

        public override void
            OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, PHashtable changedProperties) {
            if (!_keysInitialized) InitPropertyKeys();
            if (targetPlayer == null || photonView?.Owner == null)
                return;

            if (targetPlayer.ActorNumber != photonView.Owner.ActorNumber)
                return;


            if (changedProperties.ContainsKey(_playerPropertiesGunEnergyKey) && !IsLocal) {
                GunEnergy = (float) changedProperties[_playerPropertiesGunEnergyKey];
            }

            if (changedProperties.ContainsKey(_playerPropertiesRankKey) && !IsLocal) {
                Rank = (byte) changedProperties[_playerPropertiesRankKey];
            }

            if (changedProperties.ContainsKey(_playerPropertiesInTowerKey) && !IsLocal)
                IsInTower = (bool) changedProperties[_playerPropertiesInTowerKey];
            
            if (changedProperties.ContainsKey(_playerPropertiesAbortMatchVote) && !IsLocal)
                AbortMatchVote = (bool) changedProperties[_playerPropertiesAbortMatchVote];

            if (changedProperties.ContainsKey(_playerPropertiesNameKey)) {
                PlayerName = changedProperties[_playerPropertiesNameKey] as string;
                if (string.IsNullOrEmpty(DefaultName)) DefaultName = PlayerName;
            }

            if (changedProperties.ContainsKey(_playerPropertiesLoginStateKey)) {
                if (!TowerTagSettings.Home)
                    IsLoggedIn = (bool) changedProperties[_playerPropertiesLoginStateKey];
            }

            if (changedProperties.ContainsKey(_playerPropertiesIsBotKey)) {
                IsBot = (bool) changedProperties[_playerPropertiesIsBotKey];
            }

            // Login
            if (TowerTagSettings.Home && !IsBot && !IsMe && !IsLoggedIn) {
                LogIn(targetPlayer.UserId);
            }

            if (!IsMe) {
                if (changedProperties.ContainsKey(_playerPropertiesGameModeKey)) {
                    VoteGameMode = (GameMode) changedProperties[_playerPropertiesGameModeKey];
                }

                if (changedProperties.ContainsKey(_playerPropertiesStartVotumKey)) {
                    StartVotum = (bool) changedProperties[_playerPropertiesStartVotumKey];
                }

                if (changedProperties.ContainsKey(_playerPropertiesTeamChangeKey)) {
                    TeamChangeRequested = (bool) changedProperties[_playerPropertiesTeamChangeKey];
                }
            }

            if (!_photonService.IsMasterClient || (PhotonService.IsMasterClient && IsBot)) {
                UpdateValuesFromPlayerProperties(changedProperties);
            }
        }

        private BadaboomHyperactionPointer _pointer;
        private GameMode _voteGameMode = GameMode.UserVote;
        private bool _startVotum;
        private bool _teamChangeRequested;
        private int _rank;

        public void UpdateValuesFromPlayerProperties(PHashtable pHashtable) {
            if (!_keysInitialized) InitPropertyKeys();
            if (pHashtable == null) {
                Debug.Log("Cannot update player properties: received Hashtable is null!");
                return;
            }
            //            Debug.LogError($"Updating {string.Join(", " ,pHashtable.Keys)} of {PlayerName}");

            // Players Team
            if (pHashtable.ContainsKey(_playerPropertiesTeamKey)) {
                var teamID = (TeamID) pHashtable[_playerPropertiesTeamKey];
                if (TeamID != teamID) {
                    TeamID = teamID;
                    // save changes made by operator to local config file. Decided 2018-11-21 https://trello.com/c/HC4xIsbU
                    if (IsMe && TowerTagSettings.Home) {
                        ConfigurationManager.Configuration.TeamID = (int) TeamID;
                        ConfigurationManager.WriteConfigToFile();
                    }
                }
            }

            // base rotation (rotation of play space changed. Important, because position is synced in local coords)
            if (pHashtable.ContainsKey(_playerPropertiesRotationKey)) {
                transform.rotation = (Quaternion) pHashtable[_playerPropertiesRotationKey];
            }

            if (!PhotonService.IsMasterClient || (PhotonService.IsMasterClient && IsBot)) {
                if (pHashtable.ContainsKey(_playerPropertiesPillarKey)) {
                    _readStream.SetData((byte[]) pHashtable[_playerPropertiesPillarKey]);
                    TeleportHandler.Serialize(_readStream);
                }

                if (pHashtable.ContainsKey(_playerPropertiesPlayerStateKey)) {
                    _readStream.SetData((byte[]) pHashtable[_playerPropertiesPlayerStateKey]);
                    PlayerStateHandler.Serialize(_readStream);
                }

                if (pHashtable.ContainsKey(_playerPropertiesParticipatingKey)) {
                    IsParticipating = (bool) pHashtable[_playerPropertiesParticipatingKey];
                }

                if (pHashtable.ContainsKey(_playerPropertiesReadyKey)) {
                    PlayerIsReady = (bool) pHashtable[_playerPropertiesReadyKey];
                }
            }
        }

        public void Init() {
            if (!_keysInitialized)
                InitPropertyKeys();
            TeleportHandler.Init(this);
            ChargePlayer = GetComponentInChildren<ChargePlayer>();
            ChargeNetworkEventHandler = GetComponent<ChargeNetworkEventHandler>();
            if (IsMe) {
                DefaultName = PlayerProfileManager.CurrentPlayerProfile.PlayerName;
                SetName(DefaultName);
                GunController.EnergyChanged += OnGunEnergyChanged;
                if (PlayerAccount.ReceivedPlayerStatistics)
                    Rank = PlayerAccount.Statistics.level;
            }

            PlayerStateHandler.Init(this,
                PlayerHealth,
                GunController,
                _gamePaused);

            IsParticipating = true;
            InitPlayerFromPlayerProperties();
            PlayerManager.Instance.AddPlayer(this);
            _initialized = true;
        }

        #endregion

        public void InitPropertyKeys() {
            if (_keysInitialized) return;
            _playerPropertiesNameKey = PlayerID + "_" + PlayerPropertyKeys.Name;
            _playerPropertiesTeamKey = PlayerID + "_" + PlayerPropertyKeys.Team;
            _playerPropertiesParticipatingKey = PlayerID + "_" + PlayerPropertyKeys.IsParticipating;
            _playerPropertiesPillarKey = PlayerID + "_" + PlayerPropertyKeys.Pillar;
            _playerPropertiesReadyKey = PlayerID + "_" + PlayerPropertyKeys.IsReady;
            _playerPropertiesRotationKey = PlayerID + "_" + PlayerPropertyKeys.Rotation;
            _playerPropertiesGunEnergyKey = PlayerID + "_" + PlayerPropertyKeys.GunEnergy;
            _playerPropertiesLoginStateKey = PlayerID + "_" + PlayerPropertyKeys.LoginState;
            _playerPropertiesPlayerStateKey = PlayerID + "_" + PlayerPropertyKeys.PlayerState;
            _playerPropertiesGameModeKey = PlayerID + "_" + PlayerPropertyKeys.GameMode;
            _playerPropertiesStartVotumKey = PlayerID + "_" + PlayerPropertyKeys.StartVotum;
            _playerPropertiesTeamChangeKey = PlayerID + "_" + PlayerPropertyKeys.TeamChange;
            _playerPropertiesIsBotKey = PlayerID + "_" + PlayerPropertyKeys.IsBot;
            _playerPropertiesRankKey = PlayerID + "_" + PlayerPropertyKeys.Rank;
            _playerPropertiesInTowerKey = PlayerID + "_" + PlayerPropertyKeys.InTower;
            _playerPropertiesAbortMatchVote = PlayerID + "_" + PlayerPropertyKeys.ReturnToLobby;
            _keysInitialized = true;
        }

        private void OnGunEnergyChanged(float value) {
            if (PhotonNetwork.NetworkingClient.State == ClientState.Leaving) return;
            var playerProps = new PHashtable {{_playerPropertiesGunEnergyKey, value}};
            photonView.Owner.SetCustomProperties(playerProps);
        }

        #region UnityCallbacks

        public override void OnEnable() {
            PhotonService.AddCallbackTarget(this);
            SceneManager.sceneLoaded += CheckLogInState;
            ParticipatingStatusChanged += ChangePlayerState;
            if (IsMe) {
                PlayerStatisticsStore.PlayerStatisticsReceived += OnStatisticsReceived;
                GameManager.Instance.MatchConfigurationStarted += ResetVotes;
            }
        }

        public override void OnDisable() {
            if (IsLoggedIn)
                LogOut();

            PhotonService.RemoveCallbackTarget(this);
            SceneManager.sceneLoaded -= CheckLogInState;
            ParticipatingStatusChanged -= ChangePlayerState;
            PlayerStatisticsStore.PlayerStatisticsReceived -= OnStatisticsReceived;
            GameManager.Instance.MatchConfigurationStarted -= ResetVotes;

            if (_pointer != null)
                _pointer.BHAPState -= OnBadaboomHyperactionPointerStateChanged;
        }

        private void OnStatisticsReceived(PlayerStatistics playerStatistics) {
            if (TowerTagSettings.Home) {
                PlayerIdManager.GetInstance(out var playerIdManager);
                if (!playerStatistics.id.Equals(playerIdManager.GetUserId())) return;
            }

            Rank = playerStatistics.level;
        }

        public void SetIsBotFlagInProperties(bool value) {
            var playerProps = new PHashtable {{_playerPropertiesIsBotKey, value}};
            photonView.Owner.SetCustomProperties(playerProps);
        }

        private void ResetVotes() {
            if (!IsMe) return;
            VoteGameMode = GameMode.UserVote;
            AbortMatchVote = false;
            ResetButtonStates();
        }

        private void CheckLogInState(Scene newScene, LoadSceneMode loadSceneMode) {
            if (_stayLoggedInTrigger == null
                || _stayLoggedInTrigger.gameObject == null)
                return;
            if (!TTSceneManager.Instance.IsInHubScene)
                _stayLoggedInTrigger.gameObject.SetActive(false);
            else if (IsLoggedIn && TowerTagSettings.Home)
                _stayLoggedInTrigger.gameObject.SetActive(false);
            else if (IsLoggedIn && (IsMe || SharedControllerType.IsAdmin))
                _stayLoggedInTrigger.gameObject.SetActive(true);
        }

        private void Update() {
            // Debug.Log(transform.position);
            if (_initialized) TeleportHandler.CheckIfWeAreInSync();
        }

        // clean up

        private void OnDestroy() {
            PhotonService.RemoveCallbackTarget(this);
            ResetButtonStates();
            PlayerManager.Instance.RemovePlayer(this);

            // events
            PlayerNameChanged = null;
            PlayerTeamChanged = null;

            // member
            PlayerStateHandler.OnDestroy();

            PlayerAvatar = null;

            _writeStream = null;
            _readStream = null;
        }

        #endregion

        private const float goingInsideTowerDelay = 0.1f;
        private const float goingOutsideTowerDelay = 0.1f;

        private void OnGunInTowerChanged(bool gunInTower) {
            PlayerStateHandler.SetGunInTower(gunInTower);
            Invoke(nameof(ToggleGunActive), gunInTower ? goingInsideTowerDelay : goingOutsideTowerDelay);
        }

        private void ToggleGunActive() {
            PlayerStateHandler.UpdateGunControllerActive();
        }

        private void ChangePlayerState(IPlayer player, bool newValue) {
            PlayerStateHandler.SetPlayerStateOnMaster(
                newValue ? PlayerState.Alive : PlayerState.DeadButNoLimbo);
        }


        public void RequestTeamChange(TeamID teamID) {
            Debug.LogFormat($"Requesting team change: {teamID}");
            PlayerNetworkEventHandler.SendTeamChangeRequest(teamID);
        }

        public void SetStatus(Status status) {
            if (status != Status) {
                Status = status;
                StatusChanged?.Invoke(status);
            }
        }

        [ContextMenu("Update from Photon Player Properties")]
        public void InitPlayerFromPlayerProperties() {
            OnPlayerPropertiesUpdate(photonView.Owner as Photon.Realtime.Player, photonView.Owner.CustomProperties);
        }

        public void InitChaperone(Chaperone chaperone) {
            OutOfChaperoneStateChanged += PlayerStateHandler.OnPlayerLeftChaperoneBounds;
        }

        public void InitBadaboomHyperactionPointer(BadaboomHyperactionPointer bhap) {
            if (_pointer != null && _pointer == bhap)
                return;
            _pointer = bhap;
            bhap.BHAPState += OnBadaboomHyperactionPointerStateChanged;
        }

        private void OnBadaboomHyperactionPointerStateChanged(BadaboomHyperactionPointer sender, bool active) {
            IsInIngameMenu = active;
        }

        public void StartCountdown(int startTimeStamp, int countdownType) {
            CountdownStarted?.Invoke(startTimeStamp, countdownType);
        }

        public void RestartClient() {
            if (IsMe) {
                StartCoroutine(ApplicationFunctions.Restart(true));
            }
        }

        #region CalledOnMaster

        public void SetName(string newName) {
            photonView?.Owner?.SetCustomProperties(new PHashtable {{_playerPropertiesNameKey, newName}});
            PlayerName = newName;
        }

        public void LogIn(string membershipID) {
            MembershipID = membershipID;
            IsLoggedIn = !string.IsNullOrEmpty(MembershipID);
            photonView?.Owner?.SetCustomProperties(new PHashtable {{_playerPropertiesLoginStateKey, IsLoggedIn}});
        }

        public void LogOut() {
            if (TowerTagSettings.Home) return;
            SetName(DefaultName);
            MembershipID = null;
            IsLoggedIn = false;
            photonView?.Owner?.SetCustomProperties(new PHashtable {{_playerPropertiesLoginStateKey, IsLoggedIn}});
        }

        public void SetTeam(TeamID teamID) {
            Debug.Log($"Setting Team to {teamID}");
            if (teamID == TeamID)
                return;

            TeamID = teamID;
            if (PhotonService.IsMasterClient) {
                _teamChangedOnMaster = true;
                UpdatePlayerProperties();
            }

            // save changes made by operator to local config file. Decided 2018-11-21 https://trello.com/c/HC4xIsbU
            if (IsMe && !TowerTagSettings.Home) {
                ConfigurationManager.Configuration.TeamID = (int) TeamID;
                ConfigurationManager.WriteConfigToFile();
            }
        }

        public void ResetButtonStates() {
            if (!IsMe) return;
            StartVotum = false;
            TeamChangeRequested = false;
        }

        public void SetRotationOnMaster(Quaternion rotation) {
            if (!PhotonService.IsMasterClient)
                return;

            if (rotation != transform.rotation) {
                transform.rotation = rotation;
                _rotationChangedOnMaster = true;
            }
        }

        public void SetPlayerStatusOnMaster(string statusText) {
            Status status = StatusController.GetStatusByStatusText(statusText);
            if (status != Status) {
                Status = status;
                StatusChanged?.Invoke(status);
            }
        }

        public void ToggleDirectAdminChatOnMaster(bool directChatActive) {
            if (!PhotonService.IsMasterClient)
                return;
            // todo(OJ) extract rpc to component with networking duties
            photonView.RpcSecure(nameof(RpcToggleDirectAdminChat), photonView.Owner, false, directChatActive);
        }

        [PunRPC]
        private void RpcToggleDirectAdminChat(bool directChatActive) {
            if (directChatActive)
                VoiceChatPlayer.Instance.OpenDirectChannelToOperator(this);
            else
                VoiceChatPlayer.Instance.CloseDirectChannelToOperator(this);
        }

        public void ResetPlayerHealthOnMaster() {
            if (!PhotonService.IsMasterClient) {
                Debug.LogWarning("Triggered player health reset on non-master client");
                return;
            }

            var playerHealth = GetComponentInChildren<PlayerHealth>();
            if (playerHealth != null) {
                playerHealth.RestoreMaxHealth();
            }
            else {
                Debug.LogError("Cannot reset player health: Can't find PlayerHealth on Player!");
            }
        }

        #endregion

        public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient) {
            if (TowerTagSettings.Home) return;
            IsParticipating = true;
            LogOut();
        }

        public override string ToString() {
            return $"Player (name = {PlayerName}; id = {PlayerID}; team = {TeamID})";
        }

        [ContextMenu("Force me to Master")]
        private void SwitchMasterClient() {
            PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);
        }

        [ContextMenu("KickPlayer")]
        public void KickPlayerFromMatch() {
            PlayerNetworkEventHandler.SendDisconnectPlayer("You have been kicked");
        }
    }
}