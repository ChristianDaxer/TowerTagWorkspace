using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using OperatorCamera;
using TMPro;
using TowerTag;
using TowerTagSOES;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VRNerdsUtilities;
using IPlayer = TowerTag.IPlayer;

public class AdminController : SingletonMonoBehaviour<AdminController> {
    #region General Options
    [field: Header("General Options")]

    [SerializeField] private OperatorUIManager _uiManager;

    [SerializeField, Tooltip("The Name of the Axis in the InputManager for the Button to Pause the match")]
    private string _pauseButtonInputAxisName = "Pause";

    [SerializeField, Tooltip("The Name of the Axis in the InputManager for the Button to trigger an emergency stop")]
    private string _emergencyStopButtonInputAxisName = "EmergencyStop";

    [SerializeField, Tooltip("The default name a player gets when no name is entered")]
    protected string _playerDefaultName = "VR-Nerd";

    [SerializeField] private ForcedRestartGameAction _forcedGameAction;

    #endregion

    #region UI Elements

    [Space, Header("UI Elements")] [SerializeField, Tooltip("The message queue for the message overlay")]
    private MessageQueue _overlayMessageQueue;

    [SerializeField, Tooltip("Drag the Emergency Ui Canvas here")]
    private Canvas _emergencyUiCanvas;

    [SerializeField] private CustomizeDropdown _mapDropdown;

    [SerializeField, Tooltip("Drag the button to start the match or to load the hub scene here")]
    private Button _startMatchButton;

    [SerializeField, Tooltip("Drag the text of the button to start the match or to load the hub scene here")]
    private TMP_Text _startMatchButtonText;

    [SerializeField, Tooltip("The button text which should be displayed if the button starts the match")]
    private string _startMatchString = "START MATCH";

    [SerializeField, Tooltip("The button text which should be displayed if the button aborts the mission briefing")]
    private string _abortBriefingText = "ABORT";

    [SerializeField, Tooltip("The button text which should be displayed if the button loads the hub scene")]
    private string _pauseMatchString = "PAUSE MATCH";

    [SerializeField, Tooltip("The button text which should be displayed if the button resumes the match")]
    private string _resumeMatchString = "RESUME MATCH";

    [SerializeField, Tooltip("Drag the text of the button to pause or resume the match here")]
    private TMP_Text _forceRestartButtonText;

    [SerializeField, Tooltip("The button text which should be displayed if the button pauses the match")]
    private string _restartText = "FORCED RESTART";

    [SerializeField, Tooltip("The button text which should be displayed if the button pauses the match")]
    private string _endMatchText = "END MATCH";

    [SerializeField, Tooltip("Drag the text of the button to pause or resume the match here")]
    private TMP_Text _backToMainButtonText;

    [SerializeField, Tooltip("The button text which should be displayed if the button pauses the match")]
    private string _backToMainText = "BACK TO MAIN MENU";

    [SerializeField, Tooltip("The button text which should be displayed if the button pauses the match")]
    private string _emergencyText = "EMERGENCY";

    [SerializeField] private TeamBoxController _teamBoxControllerFire;
    [SerializeField] private TeamBoxController _teamBoxControllerIce;

    //[SerializeField, Tooltip("Drag the texts for the Team Scores here")]
    //private Text[] teamScoresText;

    [SerializeField, Tooltip("Drag the toggle for the automatic match start here")]
    private Toggle _automaticStartToggle;

    #endregion

    #region Properties

    /// <summary>
    /// Starts the Match automatically if the amount of players required by the MatchUp is ready
    /// </summary>
    private bool AutomaticStart {
        get => _automaticStart;
        set {
            BalancingConfiguration.Singleton.MatchAutoStart = value;
            _automaticStart = value;
            _automaticStartToggle.isOn = value;
            if (_automaticStart) GameManager.Instance.CheckAllPlayersReadyOnMaster();
        }
    }

    public bool UserVote {
        get => _userVote;
        set {
            if (_userVote != value) {
                _userVote = value;
                _uiManager.ToggleUserVoteMode(value);
                GameManager.Instance.CheckAllPlayersReadyOnMaster();
            }
        }
    }

    public bool AllowTeamChange {
        get => _allowTeamChange;
        set {
            if (!_allowTeamChange.Equals(value)) {
                _allowTeamChange = value;
            }
        }
    }

    public GameMode CurrentGameMode {
        get => _currentGameMode;
        set {
            if (_currentGameMode != value)
                _currentGameMode = MatchManager.GetCurrentSelectedGameMode();
        }
    }


    public string PlayerDefaultName => _playerDefaultName;

    #endregion

    #region private fields

    //followedPlayer for the Camera Control

    private IPlayer CurrentlyFollowedPlayer { get; set; }

    [SerializeField] private CameraManager _cameraManager;

    public CameraManager CamManager => _cameraManager;
    private MatchManager MatchManager { get; set; }

    private bool _automaticRestart;

    private bool _automaticStart;

    private bool _userVote;

    private bool _allowTeamChange;
    private GameMode _currentGameMode;
    private bool _started;

    private bool _queryPending;

    private static readonly int _normal = Animator.StringToHash("Normal");

    private static readonly int _highlighted = Animator.StringToHash("Highlighted");

    #endregion

    #region Init

    private new void Awake() {
        if (!SharedControllerType.IsAdmin)
            Destroy(gameObject);
        else {
            base.Awake();
        }
    }

    private void OnEnable() {
        SceneManager.sceneLoaded += SceneLoaded;
        PlayerManager.Instance.PlayerAdded += OnPlayerAdded;
        PlayerManager.Instance.PlayerRemoved += OnPlayerRemoved;
        GameManager.Instance.AllPlayersReadyStatusChanged += OnAllPlayersReadyStatusChanged;
        GameManager.Instance.MatchSceneLoading += OnMatchSceneLoading;
        GameManager.Instance.MissionBriefingStarted += OnMissionBriefingStarted;
        GameManager.Instance.MatchConfigurationStarted += OnMatchConfigurationStarted;
        CameraManager.Instance.PlayerToFocusChanged += OnPlayerToFocusChanged;
    }

    private void OnDisable() {
        SceneManager.sceneLoaded -= SceneLoaded;
        PlayerManager.Instance.PlayerAdded -= OnPlayerAdded;
        PlayerManager.Instance.PlayerRemoved -= OnPlayerRemoved;

        GameManager.Instance.AllPlayersReadyStatusChanged -= OnAllPlayersReadyStatusChanged;
        GameManager.Instance.MatchSceneLoading -= OnMatchSceneLoading;
        GameManager.Instance.MissionBriefingStarted -= OnMissionBriefingStarted;
        GameManager.Instance.MatchConfigurationStarted -= OnMatchConfigurationStarted;
        if (CameraManager.Instance != null) CameraManager.Instance.PlayerToFocusChanged -= OnPlayerToFocusChanged;
    }

    private void OnMatchConfigurationStarted() {
        _startMatchButtonText.text = _startMatchString;
    }

    private void OnMissionBriefingStarted(MatchDescription matchDescription, GameMode gameMode) {
        _startMatchButtonText.text = _abortBriefingText;
    }

    private void OnMatchSceneLoading() {
        _overlayMessageQueue.AddVolatileMessage("Loading...");
    }

    private void Start() {
        MatchManager = GetComponent<MatchManager>();

        // Emergency Handling
        GameManager.Instance.EmergencyReceived += ReceiveEmergency;
        _emergencyUiCanvas.gameObject.SetActive(false);

        SetDragAndDropCellsEnabled(true);

        AutomaticStart = BalancingConfiguration.Singleton.MatchAutoStart;

        GameManager.Instance.PauseReceived += OnPauseReceived;

        //need to wait on start to let the dropdown initialize todo handle reactive instead
        Invoke(nameof(CheckIfMatchIsStartable), 0.1f);
    }

    #endregion

    /// <summary>
    /// Input recognition
    /// </summary>
    private void Update() {
        if (Input.GetButtonDown(_emergencyStopButtonInputAxisName)) {
            TriggerEmergency();
        }

        if (Input.GetButtonDown(_pauseButtonInputAxisName)) {
            TogglePause();
        }
    }

    private void OnAllPlayersReadyStatusChanged(bool ready) {
        PlayerManager.Instance.GetParticipatingPlayers(out var players, out var count);
        if (AutomaticStart
            && ready
            && GameManager.Instance.CurrentState == GameManager.GameManagerStateMachine.State.Configure
            && PlayerManager.Instance.GetParticipatingFirePlayerCount() >= MatchManager.GetMatchUpFromDropdown().MaxPlayers / 2
            && PlayerManager.Instance.GetParticipatingIcePlayerCount() >= MatchManager.GetMatchUpFromDropdown().MaxPlayers / 2
        ) {
            StartMatch();
        }
    }


    #region Scene Managment

    /// <summary>
    /// Gets called when either the "Start Match" or "End Match" Button is pressed
    /// </summary>
    public void OnStartMatchButtonPressed() {
        //startMatchButton.interactable = false;

        // Check if the GameManager is ready to switch the Game State
        if (GameManager.Instance.CurrentState == GameManager.GameManagerStateMachine.State.Configure) {
            // We are in the Hub Scene
            StartMatch();
        }
        else if (GameManager.Instance.CurrentState == GameManager.GameManagerStateMachine.State.PlayMatch
                 || GameManager.Instance.CurrentState == GameManager.GameManagerStateMachine.State.Countdown
                 || GameManager.Instance.CurrentState == GameManager.GameManagerStateMachine.State.Paused
                 || GameManager.Instance.CurrentState == GameManager.GameManagerStateMachine.State.Commendations
        ) {
            // We are in the Game Scene
            OnPauseButtonPressed();
        }
        else if (GameManager.Instance.CurrentState == GameManager.GameManagerStateMachine.State.MissionBriefing) {
            GameManager.Instance.TriggerAbortMissionBriefingOnMaster();
        }
        else {
            Debug.LogError("GameManager is not in the correct State, current State: " +
                           GameManager.Instance.CurrentState);
        }
    }

    [UsedImplicitly]
    public void TriggerOnForcedRestartButtonPressed() {
        // Check if the GameManager is ready to switch the Game State
        if (GameManager.Instance.CurrentState == GameManager.GameManagerStateMachine.State.Configure) {
            if (!_queryPending) {
                _overlayMessageQueue.AddYesNoMessage(
                    "You are going to restart all clients including the operator.",
                    "Are You Sure?",
                    () => { _queryPending = true; },
                    () => { _queryPending = false; },
                    "OK",
                    StartForcedRestartCoroutine,
                    "CANCEL");
            }

            _queryPending = true;
        }
        else if (GameManager.Instance.CurrentState == GameManager.GameManagerStateMachine.State.PlayMatch
                 || GameManager.Instance.CurrentState == GameManager.GameManagerStateMachine.State.Countdown
                 || GameManager.Instance.CurrentState == GameManager.GameManagerStateMachine.State.Paused
                 || GameManager.Instance.CurrentState == GameManager.GameManagerStateMachine.State.Commendations
                 || GameManager.Instance.CurrentState == GameManager.GameManagerStateMachine.State.Offboarding
        ) {
            GameManager.Instance.TriggerMatchConfigurationOnMaster();
        }
        else {
            Debug.LogError("GameManager is not in the correct State, current State: " +
                           GameManager.Instance.CurrentState);
            //overlay.HideOverlayMessage();
        }
    }

    private void StartForcedRestartCoroutine() {
        StartCoroutine(OnForcedRestartButtonPressed());
    }

    private IEnumerator OnForcedRestartButtonPressed() {
        _forcedGameAction.TriggerForcedRestart();

        //Delay to guarantee the forced restart on the clients
        yield return new WaitForSeconds(1);
        Process.Start(Application.dataPath + "/../TowerTag.exe", "-vrmode None -admin -autostart");
        Application.Quit();
    }

    [UsedImplicitly]
    public void OnBackToMainButtonPressed() {
        // Check if the GameManager is ready to switch the Game State
        if (GameManager.Instance.CurrentState == GameManager.GameManagerStateMachine.State.Configure) {
            // We are in the Hub Scene
            OnCloseButton();
        }
        else if (GameManager.Instance.CurrentState == GameManager.GameManagerStateMachine.State.PlayMatch
                 || GameManager.Instance.CurrentState == GameManager.GameManagerStateMachine.State.Countdown
                 || GameManager.Instance.CurrentState == GameManager.GameManagerStateMachine.State.Paused
                 || GameManager.Instance.CurrentState == GameManager.GameManagerStateMachine.State.Commendations
        ) {
            // We are in the Game Scene
            TriggerEmergency();
        }
        else {
            Debug.LogError("GameManager is not in the correct State, current State: " +
                           GameManager.Instance.CurrentState);
            //overlay.HideOverlayMessage();
        }
    }

    /// <summary>
    /// Gets Called whenever a new Scene is Loaded
    /// </summary>
    /// <param name="scene">The new scene that is loaded</param>
    /// <param name="mode">The mode is which the scene is loaded</param>
    private void SceneLoaded(Scene scene, LoadSceneMode mode) {
        Destroy(DragAndDropItem.Icon);

        if (TTSceneManager.Instance.IsInHubScene || scene.name == TTSceneManager.Instance.CurrentHubScene) {
            // Switch UI to Admin
            _startMatchButtonText.text = _startMatchString;
            SetDragAndDropCellsEnabled(true);
            _forceRestartButtonText.text = _restartText;
            _backToMainButtonText.text = _backToMainText;
        }
        else {
            // Switch UI to Spectator
            _startMatchButtonText.text = _pauseMatchString;
            _forceRestartButtonText.text = _endMatchText;
            _backToMainButtonText.text = _emergencyText;
            SetDragAndDropCellsEnabled(false);
        }
    }

    /// <summary>
    /// Start the match
    /// </summary>
    private void StartMatch() {
        if (MatchManager.IsCurrentSelectedGameModeUserVote()) {
            GameManager.Instance.StartBasicMatch();
            return;
        }
        BalancingConfiguration.Singleton.MatchTimeInSeconds = MatchManager.CurrentMatchTime;
        MatchDescription matchDescription = MatchManager.GetCurrentMatchDescription();
        if (matchDescription != null) GameManager.Instance.TriggerMissionBriefingOnMaster(matchDescription, _currentGameMode);
    }

    public void OnCloseButton() {
        if (!_queryPending) {
            _overlayMessageQueue.AddYesNoMessage(
                "This will disconnect you and abort any running match.",
                "Are You Sure?",
                () => { _queryPending = true; },
                () => { _queryPending = false; },
                "OK",
                LoadMainMenu,
                "CANCEL");
        }

        _queryPending = true;
    }

    private void LoadMainMenu() {
        _queryPending = false;
        Debug.Log(name + ":" + GetType().Name + " - " + "");
        ConnectionManager.Instance.Disconnect();
        TTSceneManager.Instance.LoadConnectScene(true);
    }

    #endregion

    #region Player Event Listeners

    /// <summary>
    /// Gets called after a player is added to the game
    /// </summary>
    /// <param name="player">The players whose added to the game</param>
    private void OnPlayerAdded(IPlayer player) {
        try {
            if (player == null)
                return;


            player.ParticipatingStatusChanged += PlayerSwitchedParticipationStatus;
            player.PlayerTeamChanged += PlayerSwitchedTeam;

            // Check if all players can be spawned on the currently chosen map
            CheckIfMatchIsStartable();
        }
        catch (Exception e) {
            Debug.LogException(e);
        }
    }

    /// <summary>
    /// Gets called when a player gets removed from the game
    /// </summary>
    /// <param name="player">The player whose got removed</param>
    private void OnPlayerRemoved(IPlayer player) {
        if (player == null)
            return;

        player.PlayerTeamChanged -= PlayerSwitchedTeam;
        player.ParticipatingStatusChanged -= PlayerSwitchedParticipationStatus;

        // Check if all players can be spawned on the currently chosen map
        CheckIfMatchIsStartable();
    }

    public void RequestTeamChange(IPlayer player, TeamID teamID) {
        if (player.TeamID == teamID) return;
        // move to different team, if team is full
        if (TeamManager.Singleton.Get(teamID).GetPlayerCount() > TowerTagSettings.MaxTeamSize) {
            Debug.Log("Assigning new team for new player, because configured team was full");
            TeamID otherTeamID = teamID == TeamID.Fire
                ? TeamID.Ice
                : TeamID.Fire;
            player.SetTeam(otherTeamID);
        }
        else {
            player.SetTeam(teamID);
        }
    }

    private void PlayerSwitchedParticipationStatus(IPlayer player, bool isParticipating) {
        CheckIfMatchIsStartable();
    }

    /// <summary>
    /// Gets called when a player switches the Team
    /// </summary>
    private void PlayerSwitchedTeam(IPlayer player, TeamID teamID) {
        // Check if all players can be spawned on the currently chosen map
        CheckIfMatchIsStartable();
    }

    #endregion


    #region UI Dropdown Managment

    [UsedImplicitly]
    public void ResetButtonToNormal(Animator buttonAnimator) {
        if (buttonAnimator.gameObject.GetComponent<Button>().interactable) {
            buttonAnimator.ResetTrigger(_highlighted);
            buttonAnimator.SetTrigger(_normal);
        }
    }

    [UsedImplicitly]
    public void ResetSliderToNormal(Animator sliderAnimator) {
        sliderAnimator.ResetTrigger(_highlighted);
        sliderAnimator.SetTrigger(_normal);
    }

    #endregion

    #region Player Getters & Setters

    /// <summary>
    /// Set the name of a specific player
    /// </summary>
    /// <param name="player">The player whose name should be set</param>
    /// <param name="newName">The new name the player should get</param>
    public static void SetPlayerName(IPlayer player, string newName) {
        // Set the name on master because the Admin can only exist as the Master
        player.SetName(newName);
    }

    #endregion

    #region Player List Managment

    /// <summary>
    /// Set the name of a team and aldo update the Admin UI accordingly.
    /// </summary>
    /// <param name="teamId">The Id of the team the name of which should be changes</param>
    /// <param name="teamName">The new name of the team</param>
    public void SetTeamName(TeamID teamId, string teamName) {
        if (teamId == TeamID.Fire) _teamBoxControllerFire.SetTeamName(teamName);
        else if (teamId == TeamID.Ice) _teamBoxControllerIce.SetTeamName(teamName);
        else {
            Debug.LogError($"Cannot set team name for teamID {teamId}");
        }
    }

    /// <summary>
    /// Sets the names of the players in a team to the given ones.
    /// Fails and does nothing in case the passed number of names does not match the number of players in the team.
    /// </summary>
    /// <param name="teamId">The team of which the player names should be changes</param>
    /// <param name="playerNames">The new names of the players</param>
    public static void SetTeamPlayerNames(TeamID teamId, string[] playerNames) {
        PlayerManager.Instance.GetParticipatingPlayers(out var players, out var count);
        IPlayer[] teamPlayers = players.Take(count)
            .Where(player => player.TeamID == teamId)
            .ToArray();
        if (teamPlayers.Length != playerNames.Length) {
            ToornamentImgui.Instance.ShowAsText(
                "Cannot set player names, as there are not " + playerNames.Length + " players.");
            return;
        }

        for (var i = 0; i < playerNames.Length; i++) {
            IPlayer player = teamPlayers[i];
            string playerName = playerNames[i];
            player.SetName(playerName);
        }
    }


    /// <summary>
    /// Returns a PlayerLineController Object to a Player which identifies a PlayerLine GameObject
    /// </summary>
    /// <param name="player">The Player whose PlayerLineController is searched</param>
    /// <returns>The PlayerLineController to the Player</returns>
    [CanBeNull]
    private PlayerLineController GetPlayerLineController([NotNull] IPlayer player) {
        if (player.TeamID == TeamID.Fire) return _teamBoxControllerFire.GetPlayerLine(player);
        if (player.TeamID == TeamID.Ice) return _teamBoxControllerIce.GetPlayerLine(player);
        return null;
    }

    #endregion

    #region Drag&Drop Managment

    /// <summary>
    /// Gets called if a player was dragged and dropped in another cell
    /// </summary>
    /// <param name="desc"></param>
    // ReSharper disable once UnusedMember.Local - Called by Drag & Drop plugin
    private void OnItemPlace(DragAndDropCell.DropDescriptor desc) {
        var playerLineController = desc.Item.GetComponent<PlayerLineController>();
        IPlayer player = playerLineController.Player;
        var sourceSheet = desc.SourceCell.GetComponentInParent<TeamBoxController>();
        var destinationSheet = desc.DestinationCell.GetComponentInParent<TeamBoxController>();

        Debug.Log(desc.Item.GetComponent<PlayerLineController>().Player.PlayerName + " is dropped from " +
                  sourceSheet.name + " to " + destinationSheet.name);

        // If item dropped between different sheets
        if (TTSceneManager.Instance.IsInHubScene) {
            if (destinationSheet != sourceSheet) {
                GameManager.Instance.ChangePlayerTeam(player, destinationSheet.TeamID);
            }
        }
        else {
            // If we are not in the Hub Scene, snap the dragged object back to its source
            Destroy(DragAndDropItem.Icon);
            desc.SourceCell.PlaceItem(desc.Item.gameObject);
        }
    }

    /// <summary>
    /// Swaps two playerlines or moves one playerline from one to the other team. Gets triggered by the team change
    /// request from the RTUI
    /// </summary>
    /// <param name="playerOne">Player who wants to change the team</param>
    /// <param name="playerTwo">If not null player one will swap with this player</param>
    public void SwapTeamOfPlayerLines(IPlayer playerOne, IPlayer playerTwo = null) {
        PlayerLineController playerOneLine = GetPlayerLineController(playerOne);
        if (playerOneLine == null) return;
        DragAndDropCell oldCell = playerOneLine.gameObject.GetComponentInParent<DragAndDropCell>();
        DragAndDropCell newCell;
        if (playerTwo != null) {
            newCell = GetPlayerLineController(playerTwo)?.GetComponentInParent<DragAndDropCell>();
            if (newCell != null) oldCell.SwapItems(oldCell, newCell);
        }
        else {
            TeamBoxController enemyTeamBox = playerOne.TeamID == TeamID.Fire ? _teamBoxControllerIce : _teamBoxControllerFire;
            newCell = enemyTeamBox.GetFirstFreeSlot()?.Cell;
            if (newCell != null) {
                newCell.PlaceItem(playerOneLine.gameObject);
                oldCell.GetComponentInParent<AdminPlayerLineSlot>().SetVisible(false);
                newCell.GetComponentInParent<AdminPlayerLineSlot>().SetVisible(true);
            }
        }
    }

    /// <summary>
    /// Enables or disables all DragAndDropCells on the Admin Panel
    /// </summary>
    /// <param name="cellsEnabled">Should the DragAndDrop Cells be enabled?</param>
    private void SetDragAndDropCellsEnabled(bool cellsEnabled) {
        _teamBoxControllerIce.SetDragAndDropCellsEnabled(cellsEnabled);
        _teamBoxControllerFire.SetDragAndDropCellsEnabled(cellsEnabled);
    }

    #endregion

    #region Emergency Handling

    /// <summary>
    /// Event listener for Emergency
    /// </summary>
    private void ReceiveEmergency() {
        _emergencyUiCanvas.gameObject.SetActive(true);
    }

    /// <summary>
    /// Trigger the emergency state if the emergency button input axis is pressed
    /// </summary>
    private static void TriggerEmergency() {
        GameManager.Instance.SetEmergencyStateOnMaster();
    }

    #endregion

    #region Start Handling

    /// <summary>
    /// Set the start match button interactable or not if the match is startable or not
    /// </summary>
    private void CheckIfMatchIsStartable() {
        bool isMapSelectable = MatchManager.IsSelectedMapStartable();
        _mapDropdown.CurrentSelectionValid = isMapSelectable;
        _startMatchButton.interactable = isMapSelectable;
        if (!UserVote) _automaticStartToggle.interactable = isMapSelectable;
        if (!isMapSelectable) {
            AutomaticStart = false;
        }
    }

    #endregion

    #region Pause Handling

    private void OnPauseReceived(bool paused) {
        _startMatchButton.interactable = true;
    }

    /// <summary>
    /// Event Listener for the Pause Button
    /// </summary>
    private void OnPauseButtonPressed() {
        TogglePause();
    }

    /// <summary>
    /// Pause the Match if a match is running and the pause button input axis is pressed
    /// </summary>
    private void TogglePause() {
        bool pause = !GameManager.Instance.IsPaused();
        GameManager.Instance.SetPauseOnMaster(pause);
        _startMatchButtonText.text = pause ? _resumeMatchString : _pauseMatchString;
    }

    #endregion

    #region FocusHandling

    public void OnPlayerToFocusChanged(CameraManager sender, [CanBeNull] IPlayer player) {
        if (CurrentlyFollowedPlayer != null) {
            PlayerLineController playerLine = GetPlayerLineController(CurrentlyFollowedPlayer);
            if (playerLine != null) playerLine.Focus = false;
        }

        if (player != null) {
            PlayerManager.Instance.GetParticipatingPlayers(out var players, out var count);
            players.Take(count).Where(p => p != player)
                .Select(GetPlayerLineController)
                .Where(line => line != null)
                .ForEach(line => line.Focus = false);

            PlayerLineController playerLine = GetPlayerLineController(player);
            if (playerLine != null) playerLine.Focus = true;
        }

        CurrentlyFollowedPlayer = player;
    }

    #endregion

    public void ToggleTalkToIcon(IPlayer player, bool active) {
        if (player == null)
            return;

        PlayerLineController playerLineController = GetPlayerLineController(player);
        if (playerLineController != null)
            playerLineController.TalkTo = active;
    }
}