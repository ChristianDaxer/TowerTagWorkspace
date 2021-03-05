using System;
using System.Collections;
using Home.UI;
using JetBrains.Annotations;
using Photon.Pun;
using TowerTag;
using UnityEngine;
using UnityEngine.UI;

public class ReadyTowerUiController : MonoBehaviour {
    public delegate void ReadyTowerUiControllerAction(ReadyTowerUiController sender);

    public delegate void ReadyTowerUiToggleDelegate(ReadyTowerUiController sender, bool newValue);

    public static event ReadyTowerUiControllerAction ReadyTowerUiInstantiated;
    public static event ReadyTowerUiToggleDelegate VoteForStartButtonPressed;
    public static event ReadyTowerUiToggleDelegate RequestTeamChangeButtonPressed;

    [SerializeField] private ReadyTowerUIPUNCallback _punCallbacks;

    [Header("UI")] [SerializeField] private Toggle _voteForStartButton;
    [SerializeField] private Toggle _requestTeamChangeButton;
    [SerializeField] private Text _requestTeamChangeText;
    [SerializeField] private Canvas _readyTowerUiOverlayCanvas;
    [SerializeField] private GameObject _projector;
    [SerializeField] private BadaboomHyperactionPointer _pointerPrefab;
    [SerializeField] private ReadyTowerUiGameModeSelectionController _gameModeSelectionController;
    [SerializeField] private Animator _animator;
    [SerializeField] private Text _roomName;

    [Header("Sound")] [SerializeField] private AudioSource _audio;

    [SerializeField] private AudioClip _spawnSound;
    [SerializeField] private AudioClip _despawnSound;

    private const int WallFallDownWaitingTime = 1;
    private const float ToggleRTUIDelay = 0.25f;

    private ReadyTowerUiGameModeButton _gtButton;

    //Todo: Remove this area after testing
    private IPlayer _player;


    [Header("Debug UI")] [SerializeField, Tooltip("Debug: Check this field at runtime.")]
    private BadaboomHyperactionPointerNeeded _badaboomHyperactionPointerNeeded;

    private BadaboomHyperactionPointer _pointer;

    public bool ReadyTowerUIActive => _readyTowerUiOverlayCanvas.gameObject.activeSelf;
    private Coroutine _ownerChangeCoroutine;
    private bool _spawned;
    private static readonly int DespawnImmediately = Animator.StringToHash("DespawnImmediately");
    public bool IsUIActive { get; set; }

    public PillarWall[] Walls { get; set; }


    private void Awake() {
		_voteForStartButton.interactable = PlayerManager.Instance.GetParticipatingPlayersCount() >= 2;
        _player = PlayerManager.Instance.GetOwnPlayer();
        _punCallbacks.GetInitialRoomProperties();
        ReadyTowerUiInstantiated?.Invoke(this);
        // init game modes
        if (_gameModeSelectionController != null) {
            foreach (var mode in _gameModeSelectionController.GameModeButtons) {
                mode.Init(true);
            }
        }

        //_roomName.text = "ROOMNAME: " + PhotonNetwork.CurrentRoom.Name.ToUpper(); // cut

        _player?.PlayerNetworkEventHandler.RequestHubSceneTimer();
    }

    private void OnEnable() {
        // Register Listener
        GameManager.Instance.MissionBriefingStarted += OnMissionBriefingStarted;
		GameManager.Instance.BasicCountdownStarted += OnBasicCountdownStarted;
        GameManager.Instance.BasicCountdownAborted += OnBasicCountdownAborted;
        PlayerManager.Instance.PlayerAdded += OnPlayerAdded;
        PlayerManager.Instance.PlayerRemoved += OnPlayerRemoved;
        ReadyTowerUIPUNCallback.AllowTeamChangeToggled += OnAllowTeamChangeChanged;
        ReadyTowerUIPUNCallback.UserVoteToggled += OnUserVoteChanged;
        ReadyTowerUIPUNCallback.UserVoteToggled += _gameModeSelectionController.OnUserVotingToggled;
        ReadyTowerUIPUNCallback.NewCurrentGameModeSelected += OnNewSelectedGameMode;

        PlayerManager.Instance.GetAllConnectedPlayers(out var players, out var count);
        for (int i = 0; i < count; i++)
            players[i].PlayerTeamChanged += OnTeamChanged;

        OnAllowTeamChangeChanged(ReadyTowerUIPUNCallback.AllowTeamChange);
        IngameHubUIController.VRIngameUIToggled += OnIngameHubUiControllerToggled;
    }

    private void OnIngameHubUiControllerToggled(object sender, bool status, bool immediately) {
        if(!immediately)
            ToggleRTUI(!status, status);
    }

    private void Start() {
        _badaboomHyperactionPointerNeeded = GetComponent<BadaboomHyperactionPointerNeeded>();
        ActivateReadyTowerUI();
        ToggleGoalTower(IsGoalTowerPlayable());
        UpdateRequestTeamChangeText();
    }
	
	private void OnBasicCountdownStarted(float countdownTime)
    {
        ToggleReadyTowerUiButtons(false);
    }

    private void Update() {
        TogglePointerNeededTag();
    }

    private void OnDisable() {
        // Unregister Listener
        GameManager.Instance.MissionBriefingStarted -= OnMissionBriefingStarted;
		GameManager.Instance.BasicCountdownStarted -= OnBasicCountdownStarted;
        GameManager.Instance.BasicCountdownAborted -= OnBasicCountdownAborted;
        PlayerManager.Instance.PlayerAdded -= OnPlayerAdded;
        PlayerManager.Instance.PlayerRemoved -= OnPlayerRemoved;
        ReadyTowerUIPUNCallback.AllowTeamChangeToggled -= OnAllowTeamChangeChanged;
        ReadyTowerUIPUNCallback.UserVoteToggled -= OnUserVoteChanged;
        ReadyTowerUIPUNCallback.UserVoteToggled -= _gameModeSelectionController.OnUserVotingToggled;
        ReadyTowerUIPUNCallback.NewCurrentGameModeSelected -= OnNewSelectedGameMode;

        PlayerManager.Instance.GetAllConnectedPlayers(out var players, out var count);
        for (int i = 0; i < count; i++)
            players[i].PlayerTeamChanged -= OnTeamChanged;

        IngameHubUIController.VRIngameUIToggled -= OnIngameHubUiControllerToggled;
    }
	
	private void OnBasicCountdownAborted()
    {
        ToggleReadyTowerUiButtons(true);
    }

    private void ToggleRTUI(bool status, bool immediately = false)
    {
        if (IsUIActive == status) return;
        if (!status && immediately) {
            _animator.SetTrigger(DespawnImmediately);
            _projector.SetActive(false);
            IsUIActive = status;
            return;
        }

        string param = status ? "Spawn" : "Despawn";
        //We don't want to set the animator trigger when we are already in the state for it.
        if (param.Equals("Spawn") && _animator.GetCurrentAnimatorStateInfo(0).IsName("Spawn")
            || param.Equals("Despawn") && _animator.GetCurrentAnimatorStateInfo(0).IsName("Despawn"))
            return;
        _animator.SetTrigger(param);
        _audio.clip = status ? _spawnSound : _despawnSound;
        _audio.Play();
        IsUIActive = status;
        _projector.SetActive(status);
    }

    private void OnNewSelectedGameMode(GameMode currentGameMode) {
        _gameModeSelectionController.SetGameModeByOperator(currentGameMode);
    }

    #region RoomPropertiesChangedCalls

    private void OnAllowTeamChangeChanged(bool value) {
        _requestTeamChangeButton.interactable = value;
        _requestTeamChangeButton.isOn = false;
        _player.TeamChangeRequested = false;
        _animator.SetTrigger(value ? "TeamChangeInteractable" : "TeamChangeNotInteractable");
    }

    private void OnUserVoteChanged(bool value) {
        _requestTeamChangeButton.gameObject.SetActive(value);
        _voteForStartButton.gameObject.SetActive(value);
        _animator.SetTrigger(value ? "ButtonsOn" : "ButtonsOff");
        if (!value) {
            _player.ResetButtonStates();
            _player.VoteGameMode = GameMode.UserVote;
        }
    }

    #endregion
	
	private void ToggleReadyTowerUiButtons(bool status)
    {
        _requestTeamChangeButton.gameObject.SetActive(status);
        _voteForStartButton.gameObject.SetActive(status);
        _requestTeamChangeButton.interactable = status;
        _voteForStartButton.interactable = status;
        _animator.SetTrigger(status ? "ButtonsOn" : "ButtonsOff");
    }

    private void OnPlayerAdded(IPlayer player) {
        ToggleGoalTower(IsGoalTowerPlayable());
        player.PlayerTeamChanged += OnTeamChanged;
        UpdateRequestTeamChangeText();
    }

    private void UpdateRequestTeamChangeText()
    {
        _requestTeamChangeText.text =
            TeamManager.Singleton.IsTeamFull(_player.TeamID == TeamID.Fire ? TeamID.Ice : TeamID.Fire)
                ? "REQUEST TEAM CHANGE"
                : "CHANGE TEAM";
    }

    private void OnPlayerRemoved(IPlayer player) {
		_voteForStartButton.interactable = PlayerManager.Instance.GetParticipatingPlayersCount() >= 2;
        ToggleGoalTower(IsGoalTowerPlayable());
        player.PlayerTeamChanged -= OnTeamChanged;
        UpdateRequestTeamChangeText();
    }

    private void OnTeamChanged(IPlayer player, TeamID teamID) {
        ToggleGoalTower(IsGoalTowerPlayable());
        UpdateRequestTeamChangeText();
    }

    private void ToggleGoalTower(bool status) {
        _gameModeSelectionController.ToggleGoalTowerGameMode(status);
    }

    private bool IsGoalTowerPlayable() {
        try {
            if (_gtButton == null)
                _gtButton = _gameModeSelectionController.GetButtonByGameMode(GameMode.GoalTower);
            return _gtButton.IsGameModePlayable();
        }
        catch {
            throw new Exception("Can't toggle GT mode, because there is no GT button");
        }
    }

    private void OnMissionBriefingStarted(MatchDescription obj, GameMode gameMode) {
		ToggleReadyTowerUiButtons(false);
        ToggleRTUI(false);
    }

    public void ActivateReadyTowerUI() {
        if (Walls != null) {
            foreach (var wall in Walls) {
                StartCoroutine(LetTheWallFallDown(wall));
            }
        }
    }

    private void TogglePointerNeededTag() {
        if (_badaboomHyperactionPointerNeeded == null)
            return;
        _badaboomHyperactionPointerNeeded.enabled = _readyTowerUiOverlayCanvas.isActiveAndEnabled;
        if (_pointer == null && _badaboomHyperactionPointerNeeded.enabled &&
            !BadaboomHyperactionPointer.GetInstance(out _pointer))
            _pointer = InstantiateWrapper.InstantiateWithMessage(_pointerPrefab);
    }

    [UsedImplicitly]
    public void OnVoteForStartButtonPressed(bool newValue) {
        VoteForStartButtonPressed?.Invoke(this, newValue);
    }

    [UsedImplicitly]
    public void OnRequestTeamChangeButtonPressed(bool newValue) {
        RequestTeamChangeButtonPressed?.Invoke(this, newValue);
    }

    private IEnumerator LetTheWallFallDown(PillarWall wall) {
        yield return new WaitForSeconds(WallFallDownWaitingTime);
        wall.SetDamage(1);
        if (ReadyTowerUIActive) yield break;
        yield return new WaitForSeconds(ToggleRTUIDelay);
        ToggleRTUI(true);
    }
}