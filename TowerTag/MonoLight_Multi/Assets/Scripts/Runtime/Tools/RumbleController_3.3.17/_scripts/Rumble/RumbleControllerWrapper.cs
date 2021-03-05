using TowerTag;
using TowerTagSOES;
using UnityEngine;
using UnityEngine.SceneManagement;

// Wrapper for the RumbleController to translate TowerTag-Events to RumbleController calls
public class RumbleControllerWrapper : MonoBehaviour {
    [SerializeField] private RumbleController _rumbleController;

    [SerializeField] private RopeGameAction _ropeGameAction;

    // current GunEnergy
    float _gunEnergy;
    // current health of Player
    float _currentPlayerHealth;

    private bool _isConnected;

    private IPlayer _owner;
    private Chargeable _currentTarget;
    private bool _isFailAttempt;
    private void OnEnable() {
        _ropeGameAction.RopeConnectedToChargeable += OnRopeConnectedToChargeable;
        _ropeGameAction.Disconnecting += OnRopeDisconnected;
        _ropeGameAction.AttachFailed += OnRopeDisconnected;
    }

    private void OnDisable() {
        _ropeGameAction.RopeConnectedToChargeable -= OnRopeConnectedToChargeable;
        _ropeGameAction.Disconnecting -= OnRopeDisconnected;
        _ropeGameAction.AttachFailed -= OnRopeDisconnected;
    }

    public void Init(IPlayer player) {
        _owner = player;
    }

    private void Update() {
        if (!_isConnected)
            return;
        UpdateCharge(_currentTarget.CurrentCharge.value);
    }

    private void OnRopeConnectedToChargeable(RopeGameAction sender, IPlayer player, Chargeable pillar) {
        if (player == null)
            return;
        if (!SharedControllerType.VR || !player.IsMe)
            return;
        Connected(pillar, player);
    }

    private void OnRopeDisconnected(RopeGameAction sender, IPlayer player, Chargeable target, bool onPurpose) {
        if (player == null)
            return;
        if (!SharedControllerType.VR || !player.IsMe)
            return;
        Disconnect();
    }

    private void OnRopeDisconnected(RopeGameAction sender, IPlayer player, Chargeable target) {
        if (player == null)
            return;
        if (!SharedControllerType.VR || !player.IsMe)
            return;
        _isFailAttempt = true;
        OnRopeDisconnected(sender, player, target, false);
    }

    private void Connected(Chargeable target, IPlayer player) {
        _owner = player;
        _currentTarget = target;
        _isConnected = true;
    }

    // Triggered from GunController
    public void GunEnergyChanged(float newValue) {
        _gunEnergy = newValue;
    }

    public void TriggerShot() {
        if (_gunEnergy > 0.0f) {
            _rumbleController.TriggerShootProjectile();
        } else {
            _rumbleController.TriggerShootProjectileEmpty();
        }
    }

    // quick & dirty fix to disable beam rumble if Pillar is fully charged
    private void UpdateCharge(float currentCharge) {
        if (!SharedControllerType.VR || !_owner.IsMe)
            return;
        if (currentCharge >= 1)
            _rumbleController.ToggleCharge(false);
    }

    // Triggered from Rope
    public void OnStartBeamRollOut() {
        _rumbleController.TriggerShootChargerBeam();
        if (_isFailAttempt) return;
        _rumbleController.ToggleHighlightPillar(false);
        _rumbleController.ToggleChargerBeamLaser(true);
    }

    public void OnFinishBeamRollOut() {
        if (_isFailAttempt) {
            _isFailAttempt = false;
            return;
        }
        _rumbleController.ToggleChargerBeamLaser(false);
        _rumbleController.ToggleCharge(true);
    }
    // gunController & Rope (by collision)
    public void OnDisconnectBeam(Chargeable target, IPlayer player) {
        Disconnect();
    }

    void Disconnect() {
        _isConnected = false;
        _rumbleController.ToggleHighlightPillar(_showHighlight);
        _rumbleController.ToggleChargerBeamLaser(false);
        _rumbleController.ToggleCharge(false);
    }

    // Triggered from damageModel
    // Triggered from DamageModel -> PlayerWasHit
    public void OnTookDamage(PlayerHealth dmgMdl, IPlayer other) {
        _rumbleController.TriggerPlayerWasHit();
    }

    // ToggleHealPlayer ???: me or the other Player i try to charge?
    public void OnHealthChanged(float newValue, IPlayer other) {

    }

    // Triggered from Ray caster -> Highlight
    // Problem: can't decide which kind of Highlighter (Player/Pillar/...)!!!!!!!!!!!!!!!!!!!!!!!
    bool _showHighlight;

    public void OnHighlighterChanged(Highlighter highlighter, bool showHighlight) {
        _showHighlight = showHighlight;
        if (highlighter != null && (!showHighlight || highlighter.IsAllowedToHighlight(_owner))) {
            _rumbleController.ToggleHighlightPillar(showHighlight);
        }
    }

    private void Start() {
        SceneManager.sceneUnloaded += SceneUnloaded;
    }

    private void SceneUnloaded(Scene scene) {
        Stop();
    }

    private void Stop() {
        if (_rumbleController != null)
            _rumbleController.StopAllRumbling();
    }
    private void OnDestroy() {
        _rumbleController = null;
    }
}
