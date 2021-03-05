using OperatorCamera;
using TMPro;
using TowerTag;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class PlayerLineSpectatorController : MonoBehaviour {
    #region UI Objects

    [SerializeField] private TMP_Text _playerNumberText;

    [SerializeField] private TMP_Text _playerNameText;

    [SerializeField] private Image _playerHealthBarFilImage;

    [SerializeField] private TMP_Text _playerKillsText;

    [SerializeField] private TMP_Text _playerDeathsText;

    [SerializeField] private TMP_Text _playerAssistsText;

    [Header("Player Focus")] [SerializeField]
    private Image _focusImage;

    [SerializeField] private Sprite _playerUnfocused;
    [SerializeField] private Sprite _playerFocused;

    #endregion

    private Color TextColor {
        set {
            _playerNameText.color = value;
            _playerKillsText.color = value;
            _playerDeathsText.color = value;
            _playerAssistsText.color = value;
            //When focused, the number is black
            if (!_focus)
                _playerNumberText.color = value;
        }
    }

    #region Properties

    public bool Focus {
        get => _focus;
        set {
            _focus = value;
            _focusImage.sprite = _focus ? _playerFocused : _playerUnfocused;
            _playerNumberText.color = _focus ? Color.black
                : _player.PlayerHealth.IsAlive ? TeamManager.Singleton.Get(_player.TeamID).Colors.UI
                : TeamManager.Singleton.Get(_player.TeamID).Colors.DarkUI;
        }
    }

    public int Kills {
        get => int.Parse(_playerKillsText.text);
        set => _playerKillsText.text = value.ToString();
    }

    public int Deaths {
        get => int.Parse(_playerDeathsText.text);
        set => _playerDeathsText.text = value.ToString();
    }

    public int Assists {
        get => int.Parse(_playerAssistsText.text);
        set => _playerAssistsText.text = value.ToString();
    }

    private IPlayer _player;
    private int _playerNumber;
    private TMP_Text _miniMapNumber;
    private bool _focus;

    public int PlayerNumber {
        get => _playerNumber;
        set {
            if (_playerNumber == value) return;
            _playerNumber = value;
            _playerNumberText.text = value.ToString();
            if (_miniMapNumber != null) _miniMapNumber.text = value.ToString();
        }
    }

    public IPlayer Player {
        get => _player;
        set {
            UnregisterEventListeners();
            _player = value;
            if (_player == null) return;
            _miniMapNumber = Player.GameObject.CheckForNull()?.GetComponentInChildren<PlayerMiniMapIcon>(true).PlayerNumber;
            SetUpUI();
            RegisterEventListeners();
        }
    }

    public SpectatorUiController SpecUiController { get; set; }

    private void SetUpUI() {
        _playerNameText.text = _player.PlayerName;
        _playerNumberText.text = PlayerNumber.ToString();
        if (_player.PlayerHealth != null) {
            TextColor = _player.IsAlive
                ? TeamManager.Singleton.Get(_player.TeamID).Colors.UI
                : TeamManager.Singleton.Get(_player.TeamID).Colors.DarkUI;
            _focusImage.color = _player.IsAlive
                ? TeamManager.Singleton.Get(_player.TeamID).Colors.UI
                : TeamManager.Singleton.Get(_player.TeamID).Colors.DarkUI;
            _playerHealthBarFilImage.fillAmount = _player.PlayerHealth.HealthFraction;
        }
        else {
            TextColor = TeamManager.Singleton.Get(_player.TeamID).Colors.UI;
            _playerHealthBarFilImage.fillAmount = 1;
        }
    }

    #endregion

    #region Init

    private void OnEnable() {
        RegisterEventListeners();
    }

    private void OnDisable() {
        UnregisterEventListeners();
    }

    private void RegisterEventListeners() {
        if (_player != null) {
            _player.PlayerNameChanged += PlayerNameChanged;
            _player.PlayerTeamChanged += TeamChanged;
            _player.PlayerHealth.HealthChanged += HealthChanged;
        }
    }

    private void UnregisterEventListeners() {
        if (_player != null) {
            _player.PlayerNameChanged -= PlayerNameChanged;
            _player.PlayerTeamChanged -= TeamChanged;
            _player.PlayerHealth.HealthChanged -= HealthChanged;
        }
    }

    #endregion

    #region Event Listeners

    private void HealthChanged(PlayerHealth sender, int newHealth, IPlayer other, byte colliderType) {
        TextColor = _player.IsAlive
            ? TeamManager.Singleton.Get(_player.TeamID).Colors.UI
            : TeamManager.Singleton.Get(_player.TeamID).Colors.DarkUI;
        _focusImage.color = _player.IsAlive
            ? TeamManager.Singleton.Get(_player.TeamID).Colors.UI
            : TeamManager.Singleton.Get(_player.TeamID).Colors.DarkUI;
        _playerHealthBarFilImage.fillAmount = _player.PlayerHealth.HealthFraction;
    }


    private void PlayerNameChanged(string newName) {
        _playerNameText.text = newName;
    }

    private void TeamChanged(IPlayer player, TeamID teamID) {
        SpecUiController.SwitchTeamOfPlayerLine(this, teamID);
        TextColor = _player.IsAlive
            ? TeamManager.Singleton.Get(_player.TeamID).Colors.UI
            : TeamManager.Singleton.Get(_player.TeamID).Colors.DarkUI;
        _focusImage.color = _player.IsAlive
            ? TeamManager.Singleton.Get(_player.TeamID).Colors.UI
            : TeamManager.Singleton.Get(_player.TeamID).Colors.DarkUI;
    }

    /// <summary>
    /// Switch the admin follow came to this player
    /// </summary>
    // ReSharper disable once UnusedMember.Global - Button callback
    public void OnFocusButtonClicked() {
        Focus = !Focus;
        CameraManager.Instance.SetHardFocusOnPlayer(Player, Focus);
    }

    #endregion

    public void ResetPlayerStats() {
        Kills = 0;
        Deaths = 0;
        Assists = 0;
    }
}