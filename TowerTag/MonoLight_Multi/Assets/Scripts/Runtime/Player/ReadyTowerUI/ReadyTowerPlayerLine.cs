using JetBrains.Annotations;
using Photon.Pun;
using Photon.Voice.PUN;
using TowerTag;
using UnityEngine;
using UnityEngine.UI;
using Player = Photon.Realtime.Player;

public class ReadyTowerPlayerLine : MonoBehaviour
{
    [Header("Materials")] [SerializeField] private Material _activeImageMaterial;
    [SerializeField] private Material _disabledImageMaterial;
    [SerializeField] private Material _activeTextMaterial;
    [SerializeField] private Material _disabledTextMaterial;

    [Header("UI Components")] [SerializeField]
    private Text _playerName;

    [SerializeField] private MeshRenderer _dot;
    [SerializeField] private MeshRenderer _circle;
    [SerializeField] private MeshRenderer _triSide;
    [SerializeField] private Button _addBotButton;
    [SerializeField] private Button _removeBotButton;
    [SerializeField] private float _modeLayerGroupZOffset;
    [SerializeField] private Image _playerNameBackground;
    [SerializeField] private Image _speakerDot;
    [SerializeField] private Color _activeDot;
    [SerializeField] private Color _inactiveDot;
    [SerializeField] private RectTransform _layoutGroup;
    [SerializeField] private TeamID _playerLineTeamId = TeamID.Neutral;

    private const string EmptySlotText = "-FREE-";
    private const string AddBotText = "ADD BOT";
    
    public bool IsEmpty => _player == null;
    private IPlayer _player;
    private PhotonVoiceView _photonVoiceView;
    private bool _activeLastFrame;
    private Vector3 _startPosition;
    private Vector3 _hoverAndActivePosition;
    private bool _positionInitialized;
    private bool _playerlineReset;


    private void OnEnable()
    {
        _playerlineReset = false;
        GameManager.Instance.BasicCountdownStarted += OnBasicCountdownStarted;
        GameManager.Instance.BasicCountdownAborted += OnBasicCountdownAborted;
        ConnectionManager.Instance.MasterClientSwitched += OnMasterClientSwitched;
        GameManager.Instance.MissionBriefingStarted += OnMissionBriefingStarted;
        if (_player == null && PhotonNetwork.IsMasterClient)
        {
            SetName(PhotonNetwork.IsMasterClient ? AddBotText : EmptySlotText);
            _addBotButton.gameObject.SetActive(true);
            _addBotButton.onClick.AddListener(OnAddBotButtonClicked);
        }

        if (_player != null && _player.IsBot && PhotonNetwork.IsMasterClient)
        {
            _removeBotButton.gameObject.SetActive(true);
        }
    }

    private void OnDisable()
    {
        GameManager.Instance.BasicCountdownStarted -= OnBasicCountdownStarted;
        GameManager.Instance.BasicCountdownAborted -= OnBasicCountdownAborted;
        GameManager.Instance.MissionBriefingStarted -= OnMissionBriefingStarted;
        _addBotButton.onClick.RemoveListener(OnAddBotButtonClicked);
        if(ConnectionManager.Instance != null)
            ConnectionManager.Instance.MasterClientSwitched -= OnMasterClientSwitched;
    }

    private void OnBasicCountdownStarted(float countdownTime)
    {
        ToggleInteractionButtons(false);
    }

    private void OnBasicCountdownAborted()
    {
        ToggleInteractionButtons(true);
    }

    private void OnMasterClientSwitched(ConnectionManager connectionManager, Player player)
    {
        if (player.IsLocal)
        {
            if(IsEmpty)
            {
                _addBotButton.gameObject.SetActive(true);
                SetName(AddBotText);
                _addBotButton.onClick.AddListener(OnAddBotButtonClicked);
            } else if (_player.IsBot)
            {
                _removeBotButton.gameObject.SetActive(true);
                _removeBotButton.onClick.AddListener(OnRemoveBotButtonClicked);
            }
        }
    }

    private void OnMissionBriefingStarted(MatchDescription matchDescription, GameMode gameMode)
    {
        ToggleInteractionButtons(false);
    }

    private void ToggleInteractionButtons(bool status)
    {
        _addBotButton.gameObject.SetActive(status);
        _removeBotButton.gameObject.SetActive(status);
        if (status)
        {
            _addBotButton.onClick.AddListener(OnAddBotButtonClicked);
            _removeBotButton.onClick.AddListener(OnRemoveBotButtonClicked);
            
        }
        else
        {
            _addBotButton.onClick.RemoveAllListeners();
            _removeBotButton.onClick.RemoveAllListeners();
        }
    }

    private void OnAddBotButtonClicked()
    {
        if (GameManager.Instance.CurrentState == GameManager.GameManagerStateMachine.State.MissionBriefing
            || GameManager.Instance.MatchCountdownRunning)
            return;
        StartCoroutine(
            BotManagerHome.Instance.SpawnBotForTeamWhenOwnPlayerAvailable(
                _playerLineTeamId == TeamID.Neutral ? TeamID.Ice : _playerLineTeamId, 1));
    }

    private void OnRemoveBotButtonClicked()
    {
        if (GameManager.Instance.CurrentState == GameManager.GameManagerStateMachine.State.MissionBriefing
            || GameManager.Instance.MatchCountdownRunning)
            return;
        if (_player != null && _player.IsBot)
        {
            BotManagerHome.Instance.DestroyBot(new[] {_player}, 1);
            ResetPlayerLine();
            _addBotButton.gameObject.SetActive(true);
            _addBotButton.onClick.AddListener(OnAddBotButtonClicked);
        }
    }

    public void SetPlayer([CanBeNull] IPlayer player)
    {
        _playerlineReset = false;
        if (player == _player) return;

        if (player == null)
        {
            ResetPlayerLine();
            return;
        }

        // Add bot button 
        _addBotButton.gameObject.SetActive(false);
        _addBotButton.onClick.RemoveListener(OnAddBotButtonClicked);
        
        // Remove Bot Button
        if (player.IsBot && PhotonNetwork.IsMasterClient)
        {
            _removeBotButton.gameObject.SetActive(true);
            _removeBotButton.onClick.AddListener(OnRemoveBotButtonClicked);
        }

        SetName(player.PlayerName);
        SetOwnPlayerConfiguration(player.IsMe);
        _player = player;
        OnStartNowStatusChanged(_player, _player.StartVotum);
        OnGameModeVoted(_player, (player.VoteGameMode, GameMode.UserVote));
        _player.PlayerNameChanged += OnPlayerNameChanged;
        _player.StartNowVoteChanged += OnStartNowStatusChanged;
        _player.GameModeVoted += OnGameModeVoted;
        if (_player.IsMe)
        {
            var audioInputVisualization = _speakerDot.gameObject.AddComponent<AudioInputVisualization>();
            audioInputVisualization.Init(_speakerDot, _activeDot, _inactiveDot);
            return;
        }

        _photonVoiceView = _player.GameObject.CheckForNull()?.GetComponent<PhotonVoiceView>();
    }

    private void Update()
    {
        if (IsEmpty || _player.IsMe) return;
        if (_activeLastFrame != _photonVoiceView.IsSpeaking)
        {
            _speakerDot.color = _photonVoiceView.IsSpeaking ? _activeDot : _inactiveDot;
            _activeLastFrame = _photonVoiceView.IsSpeaking;
        }
    }

    [UsedImplicitly]
    public void OnPointerEnter()
    {
        // early return
        if (!_addBotButton.enabled && !_removeBotButton) return;

        // move button 
        if (!_positionInitialized)
            InitPositionValues();
        if(_addBotButton.enabled)
            _addBotButton.transform.localPosition = _hoverAndActivePosition;
        if(_removeBotButton.enabled)
            _removeBotButton.transform.localPosition = _hoverAndActivePosition;
    }

    [UsedImplicitly]
    public void OnPointerExit()
    {
        // early return
        if (!_addBotButton.enabled && !_removeBotButton) return;

        // reset button position
        if(_addBotButton.enabled)
            _addBotButton.transform.localPosition = _startPosition;
        if(_removeBotButton.enabled)
            _removeBotButton.transform.localPosition = _startPosition;
    }

    private void InitPositionValues()
    {
        var localPosition = _addBotButton.transform.localPosition;
        _startPosition = localPosition;
        _hoverAndActivePosition = localPosition + new Vector3(0, 0, _modeLayerGroupZOffset);
        _positionInitialized = true;
    }

    private void SetName(string playerName)
    {
        _playerName.text = playerName?.ToUpper();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_layoutGroup);
    }

    private void SetOwnPlayerConfiguration(bool isMe)
    {
        _triSide.enabled = isMe;
        _playerNameBackground.enabled = isMe;
        _playerName.material = isMe ? null : _activeTextMaterial;
    }

    private void OnPlayerNameChanged(string newName)
    {
        SetName(newName);
    }

    public void ResetPlayerLine()
    {
        if (_playerlineReset)
        {
            // Playerline allready reseted 
            return;
        }
        
        SetName(PhotonNetwork.IsMasterClient ? AddBotText : EmptySlotText);
        _circle.material = _disabledImageMaterial;
        _playerName.material = _disabledTextMaterial;
        _speakerDot.color = _inactiveDot;

        _playerNameBackground.enabled = false;
        _dot.enabled = false;
        _triSide.enabled = false;
        
        _addBotButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        _removeBotButton.gameObject.SetActive(false);

        _player.PlayerNameChanged -= OnPlayerNameChanged;
        _player.StartNowVoteChanged -= OnStartNowStatusChanged;
        _player.GameModeVoted -= OnGameModeVoted;

        _player = null;
        _playerlineReset = true;
    }

    private void OnStartNowStatusChanged(IPlayer player, bool readyStatus)
    {
        _dot.enabled = readyStatus;
    }

    private void OnGameModeVoted(IPlayer player, (GameMode gameMode, GameMode previousGameMode) gameModeData)
    {
        _circle.material = gameModeData.gameMode != GameMode.UserVote ? _activeImageMaterial : _disabledImageMaterial;
    }

    private void OnDestroy()
    {
        if (_player != null)
        {
            _player.PlayerNameChanged -= OnPlayerNameChanged;
            _player.StartNowVoteChanged -= OnStartNowStatusChanged;
            _player.GameModeVoted -= OnGameModeVoted;
        }
    }
}