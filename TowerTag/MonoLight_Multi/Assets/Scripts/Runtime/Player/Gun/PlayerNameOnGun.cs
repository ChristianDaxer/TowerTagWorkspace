using System.Collections;
using TMPro;
using TowerTag;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameOnGun : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI[] _playerNameTexts;
    [SerializeField] private Image[] _ui;
    [SerializeField] private Image[] _healthBar;
    [SerializeField] private int _minTextSize;
    [SerializeField] private int _maxTextSize;

    private IPlayer _player;
    private IEnumerator _searchingForPlayerCoroutine;

    private void Awake() {
        _player = GetComponentInParent<IPlayer>();
        if (_player == null) {
            SetPlayerName(PlayerProfileManager.CurrentPlayerProfile.PlayerName);
        }
    }

    private void OnEnable() {
        if (_player != null) {
            _player.PlayerNameChanged += OnPlayerNameChanged;
            _player.PlayerTeamChanged += OnTeamChange;
            _player.PlayerHealth.HealthChanged += OnPlayerHealthChanged;
            OnPlayerNameChanged(_player.PlayerName);
            OnTeamChange(_player, _player.TeamID);
        }

        if (TowerTagSettings.Home && TTSceneManager.Instance.IsInConnectScene) {
            foreach (var settingsController in Resources.FindObjectsOfTypeAll<IngameUISettingsController>()) {
                if (settingsController.gameObject.scene.name == null) continue;
                settingsController.IngameSettingsSaveButtonPressed += OnIngameSettingsSaveButtonPressed;
            }
        }
    }

    private void OnIngameSettingsSaveButtonPressed(object sender) {
        if (PlayerProfileManager.CurrentPlayerProfile != null)
            SetPlayerName(PlayerProfileManager.CurrentPlayerProfile.PlayerName);
    }

    private void OnDisable() {
        if (_player != null) {
            _player.PlayerNameChanged -= OnPlayerNameChanged;
            _player.PlayerTeamChanged -= OnTeamChange;
            _player.PlayerHealth.HealthChanged -= OnPlayerHealthChanged;
        }

        if (TowerTagSettings.Home) {
            foreach (var settingsController in Resources.FindObjectsOfTypeAll<IngameUISettingsController>()) {
                if (settingsController.gameObject.scene.name != null)
                    settingsController.IngameSettingsSaveButtonPressed -= OnIngameSettingsSaveButtonPressed;
            }
        }
    }

    private void OnPlayerHealthChanged(PlayerHealth playerHealth, int newHealth, IPlayer other, byte colliderType) {
        _healthBar.ForEach(bar => bar.fillAmount = playerHealth.HealthFraction);
    }

    private void OnPlayerNameChanged(string newName) {
        SetPlayerName(newName);
    }

    private void OnTeamChange(IPlayer player, TeamID teamID) {
        _playerNameTexts.ForEach(text => {
            text.color = TeamManager.Singleton.Get(teamID).Colors.UI;
            //have to do this, because for unknown reason the sizes get reset on start
            text.fontSizeMin = _minTextSize;
            text.fontSizeMax = _maxTextSize;
        });
        _ui.ForEach(text => text.color = TeamManager.Singleton.Get(teamID).Colors.UI);
    }

    private void SetPlayerName(string playerName) {
        if (_playerNameTexts.Length < 1) {
            Debug.LogError("Can't find player name text objects");
            return;
        }

        _playerNameTexts.ForEach(text => text.text = playerName);
    }
}