using Network;
using OperatorCamera;
using SOEventSystem.Shared;
using TMPro;
using TowerTag;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class PlayerLineController : MonoBehaviour {
    #region UI Objects

    [Header("UI Objects")] [SerializeField, Tooltip("Textfield for the player name")]
    private TMP_InputField _playerNameInputField;

    [SerializeField, Tooltip("Button to edit the player name")]
    private Button _editNameButton;

    [SerializeField, Tooltip("The Image for the (camera) focus icon")]
    private Image _focusImage;

    [SerializeField, Tooltip("Button to toggle the focus")]
    private Button _focusButton;

    [SerializeField, Tooltip("The image for the TalkTo icon")]
    private Image _talkToImage;

    [SerializeField, Tooltip("Button to toggle the TalkTo button")]
    private Button _talkToButton;

    [SerializeField, Tooltip("The image visualizing if there is mic input")]
    private Image _micInputImage;

    [SerializeField, Tooltip("The image for the player option menu")]
    private Image _menuImage;

    [SerializeField, Tooltip("Button to toggle the player option menu")]
    private Button _menuButton;

    [SerializeField, Tooltip("The player option menu prefab")]
    private GameObject _playerOptionMenuPrefab;

    [SerializeField, Tooltip("Textfield which shows the current player status")]
    private TMP_Text _statusText;

    private Image _frame;

    #endregion

    #region Materials and Sprites

    //0 = false color, 1 = true color
    [SerializeField] private Color[] _micInputStatusColors;

    #endregion

    #region Properties

    private int _position;

    /// <summary>
    /// Is the player according to the player line currently focused by the camera?
    /// </summary>
    [SerializeField] private bool _focus;

    public bool Focus {
        private get => _focus;
        set {
            _focusImage.material = value
                ? TeamMaterialManager.Singleton.GetFlatUI(Player.TeamID)
                : TeamMaterialManager.Singleton.GetFlatUIDark(Player.TeamID);

            _focus = value;
        }
    }

    /// <summary>
    /// Is the player channeled to the operator, so he can talk to him?
    /// </summary>
    [SerializeField] private bool _talkTo;

    public bool TalkTo {
        private get => _talkTo;
        set {
            if (Player.IsBot) return;
            _talkToImage.material = value
                ? TeamMaterialManager.Singleton.GetFlatUI(Player.TeamID)
                : TeamMaterialManager.Singleton.GetFlatUIDark(Player.TeamID);

            _talkTo = value;
        }
    }

    #endregion

    [SerializeField] private SharedBool _editingPlayerName;

    /// <summary>
    /// The player according to the player line
    /// </summary>
    public IPlayer Player {
        get => _player;
        set {
            UnregisterEventListeners();
            _player = value;
            _playerNameInputField.text = value.PlayerName;
            _playerNameInputField.textComponent.color = TeamManager.Singleton.Get(_player.TeamID).Colors.UI;
            _playerNameInputField.textComponent.ForceMeshUpdate();
            RegisterEventListeners();
        }
    }

    private IPlayer _player;
    private GameObject _menu;

    #region Init

    private void Start() {
        if (_player != null && _player.Status != null)
            OnStatusChanged(_player.Status);
    }


    private void OnEnable() {
        RegisterEventListeners();
        _frame = GetComponent<Image>();
    }

    private void OnDisable() {
        UnregisterEventListeners();
    }

    private void RegisterEventListeners() {
        if (Player != null) {
            Player.PlayerTeamChanged += UpdateTeamColor;
            Player.PlayerNameChanged += OnPlayerNameChanged;
            Player.StatusChanged += OnStatusChanged;
            Player.ParticipatingStatusChanged += OnParticipatingStatusChanged;
            Player.ReceivingMicInputStatusChanged += OnReceivingMicInputStatusChanged;
        }

        SceneManager.sceneLoaded += ToggleMenuButton;
    }

    //Colorize the mic input icon
    private void OnReceivingMicInputStatusChanged(IPlayer player, bool newValue) {
        if (_micInputImage != null)
            _micInputImage.color = _micInputStatusColors[newValue ? 1 : 0];
    }

    private void UnregisterEventListeners() {
        if (Player != null) {
            Player.PlayerTeamChanged -= UpdateTeamColor;
            Player.PlayerNameChanged -= OnPlayerNameChanged;
            Player.StatusChanged -= OnStatusChanged;
            Player.ParticipatingStatusChanged -= OnParticipatingStatusChanged;
            Player.ReceivingMicInputStatusChanged -= OnReceivingMicInputStatusChanged;
        }

        SceneManager.sceneLoaded -= ToggleMenuButton;
    }

    #endregion

    private void Update() {
        //Force the deselect with enter
        if (_playerNameInputField.isFocused) {
            if (Input.GetKeyDown(KeyCode.Return)) {
                OnInputFieldDeselect();
            }
        }
    }

    /// <summary>
    /// Callback when the participating status of the player has changed
    /// </summary>
    /// <param name="player"></param>
    /// <param name="isParticipating">New status</param>
    private void OnParticipatingStatusChanged(IPlayer player, bool isParticipating) {
        UpdateTeamColor(player, player.TeamID);

        _editNameButton.interactable = isParticipating;
        _focusButton.interactable = isParticipating;
        if (!Player.IsBot)
            _talkToButton.interactable = isParticipating;

        if (!isParticipating && Focus)
            OnFocusIconClick();
        if (!isParticipating && TalkTo)
            OnTalkToIconClicked();
    }

    /// <summary>
    /// Colorize everything of the player line
    /// </summary>
    /// <param name="player"></param>
    /// <param name="teamID">New team id</param>
    public void UpdateTeamColor(IPlayer player, TeamID teamID) {
        Material uiMaterial = TeamMaterialManager.Singleton.GetFlatUI(teamID);
        Material uiMaterialDark = TeamMaterialManager.Singleton.GetFlatUIDark(teamID);
        _menuImage.material = uiMaterial;
        var dropdown = GetComponentInChildren<CustomizeDropdown>();
        if (dropdown != null) dropdown.TeamID = teamID;

        if (Player.IsParticipating) {
            _playerNameInputField.textComponent.color = uiMaterial.color;
            _frame.material = TeamMaterialManager.Singleton.GetFlatUI(teamID);
            _focusImage.material = Focus ? uiMaterial : uiMaterialDark;
            if (_talkToImage != null) _talkToImage.material = TalkTo ? uiMaterial : uiMaterialDark;
        }
        else {
            _playerNameInputField.textComponent.color = uiMaterialDark.color;
            _frame.material = uiMaterialDark;
            _focusImage.material = uiMaterialDark;
            if (_talkToImage != null) _talkToImage.material = uiMaterialDark;
        }
    }

    /// <summary>
    /// Switch the admin follow came to this player
    /// </summary>
    public void OnFocusIconClick() {
        Focus = !Focus;
        CameraManager.Instance.SetHardFocusOnPlayer(Player, Focus);
    }

    /// <summary>
    /// Start a direct voice link
    /// </summary>
    public void OnTalkToIconClicked() {
        if (TalkTo)
            VoiceChatPlayer.Instance.CloseDirectChannelToOperator(Player);
        else
            VoiceChatPlayer.Instance.OpenDirectChannelToOperator(Player);
    }

    /// <summary>
    /// We use the Button here, to make the player line draggable when left mouse button is hold
    /// </summary>
    public void OnEditNameButtonClicked() {
        // Go into edit mode
        _playerNameInputField.interactable = true;
        _playerNameInputField.Select();
        _editNameButton.enabled = false;
        _editingPlayerName.Set(this, true);
    }

    /// <summary>
    /// Sets the new Player name and resets editing values
    /// </summary>
    public void OnInputFieldDeselect() {
        if (_playerNameInputField.text == string.Empty)
            _playerNameInputField.text = AdminController.Instance.PlayerDefaultName;

        AdminController.SetPlayerName(Player, _playerNameInputField.text);
        _editNameButton.enabled = true;
        _editingPlayerName.Set(this, false);
        _playerNameInputField.DeactivateInputField();
    }

    private void OnPlayerNameChanged(string newName) {
        _playerNameInputField.text = newName;
    }


    private void OnStatusChanged(Status status) {
        if (Player.IsBot) return;

        _statusText.text = status.StatusText;
        _statusText.color = status.StatusColor;
    }

    /// <summary>
    /// The menu button should not be interactable when in Match
    /// </summary>
    /// <param name="newScene"></param>
    /// <param name="arg1"></param>
    private void ToggleMenuButton(Scene newScene, LoadSceneMode arg1) {
        ToggleMenuButton(newScene.name == TTSceneManager.Instance.CurrentHubScene);
    }

    public void ToggleMenuButton(bool setActive) {
        _menuButton.interactable = setActive;
        _menuImage.material = setActive ? TeamMaterialManager.Singleton.GetFlatUI(_player.TeamID) : TeamMaterialManager.Singleton.GetFlatUIDark(_player.TeamID);
    }


    //Showing the PlayerOptions
    public void SpawnPlayerOptionMenu() {
        if (_menu == null) {
            _menu = InstantiateWrapper.InstantiateWithMessage(_playerOptionMenuPrefab, _menuButton.transform.parent);
        }
    }
}