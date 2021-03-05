using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TowerTag;

/// <summary>
/// This script updates the UI on the screen of a player
/// </summary>
public class UpdateUIName : MonoBehaviour {

    [SerializeField, Tooltip("The textfield with the player name")] private TextMeshProUGUI _name;
    [SerializeField, Tooltip("The border of the image")] private Image _frame;
    [SerializeField, Tooltip("The health displaying hearth icon")] private Image _health;
    [SerializeField] private GameObject _gameVersionOverlayPrefab;

    private GameObject _gameVersionOverlay;
    private IPlayer _player;
    private bool _visible = true;

    private void Start() {
        _player = GetComponentInParent<IPlayer>();
        if (_player == null) return;

        RegisterListeners();

        //Initial values
        UpdateName(_player.PlayerName);
        UpdateTeamColor(_player, _player.TeamID);
        UpdateHealth(_player.PlayerHealth, _player.PlayerHealth.CurrentHealth, null, 0);
    }

    private void OnEnable() {
        RegisterListeners();
    }

    private void OnDisable() {
        UnregsiterListeners();
    }

    private void RegisterListeners() {
        if (_player == null) return;

        _player.PlayerNameChanged += UpdateName;
        _player.PlayerTeamChanged += UpdateTeamColor;
        _player.PlayerHealth.HealthChanged += UpdateHealth;
        _player.PlayerHealth.PlayerDied += PlayerDied;
    }

    private void UnregsiterListeners() {
        if (_player == null) return;

        _player.PlayerNameChanged -= UpdateName;
        _player.PlayerTeamChanged -= UpdateTeamColor;
        _player.PlayerHealth.HealthChanged -= UpdateHealth;
        _player.PlayerHealth.PlayerDied -= PlayerDied;
    }

    private void Update() {
        //For capture mode! Deactivate the whole UI of the screen!
        if (Input.GetKeyDown(KeyCode.F10)) {
            _visible = !_visible;
            _frame.gameObject.SetActive(_visible);

            if (_gameVersionOverlay == null)
                _gameVersionOverlay = GameObject.Find(_gameVersionOverlayPrefab.name);
            if (_gameVersionOverlay != null)
                _gameVersionOverlay.SetActive(_visible);
        }
    }

    private void UpdateName(string newName) {
        _name.text = newName.ToUpper();
    }

    private void UpdateTeamColor(IPlayer player, TeamID teamID) {
        _name.color = TeamManager.Singleton.Get(teamID).Colors.UI;
        _frame.material = TeamMaterialManager.Singleton.GetFlatUI(teamID);
    }

    private void UpdateHealth(PlayerHealth playerHealth, int newHealth, IPlayer other, byte colliderType) {
        _health.fillAmount = playerHealth.HealthFraction;
    }

    private void PlayerDied(PlayerHealth dmgMdl, IPlayer enemyWhoAppliedDamage, byte colliderType) {
        _health.fillAmount = 0.0f;
    }
}