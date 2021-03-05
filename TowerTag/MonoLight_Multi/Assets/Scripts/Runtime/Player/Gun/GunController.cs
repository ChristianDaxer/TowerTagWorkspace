using System;
using JetBrains.Annotations;
using TowerTag;
using UnityEngine;

public partial class GunController : MonoBehaviour {
    #region members and properties

    [SerializeField] private float _noEnergyMultiplier = 2.5f;

    private IPlayer Player { get; set; }
    private RayCaster RayCaster { get; set; }

    [SerializeField] private GunControllerStateMachine _stateMachine;
    public GunControllerStateMachine StateMachine => _stateMachine;

    [SerializeField] private ShotGameAction _shotGameAction;
    [SerializeField] private RopeGameAction _ropeGameAction;
    [SerializeField] private Transform _muzzle;
    [SerializeField] private GameObject _railGun;

    public float CurrentEnergy {
        get => (Player != null) ? Player.GunEnergy : 0.0f;
        set {
            value = Mathf.Clamp01(value);
            if (Player == null) return;
            if (Player.GunEnergy != value)
            {
            //if (!Equals(Player.GunEnergy, newValue)) {
                Player.GunEnergy = value;
                EnergyChanged?.Invoke(Player.GunEnergy);
            }
        }
   	}

    public GunDisabledSound ShotDeniedSound { get; private set; }
    private float NoEnergyMultiplier => CurrentEnergy > 0.0f ? 1 : _noEnergyMultiplier;

    private RotatePlaySpaceMovement _rotatePlayspaceHelper;

    public RotatePlaySpaceMovement RotatePlayspaceHelper => _rotatePlayspaceHelper;

    private Chargeable Chargeable { get; set; }

    private TeleportMovement Movement { get; set; }

    public GameObject RailGun => _railGun;

    #endregion

    #region events

    // teleport
    public delegate void TriggerTeleport(IPlayer player, Pillar target);

    public delegate void TriggerRotation(IPlayer player, RotatePlaySpaceHook target);

    public event TriggerTeleport TeleportTriggered;
    public event TriggerTeleport TeleportDenied;
    public event TriggerRotation RotationTriggered;

    // energy changed
    public event Action<float> EnergyChanged;

    // enable/disable gun
    public delegate void ToggleSetActiveEventHandler(bool active);

    public event ToggleSetActiveEventHandler SetActiveTriggered;

    // shoot
    public delegate void GunShotEventHandler();

    public event GunShotEventHandler ShotTriggered;

    // detach
    public delegate void DetachEvent(GunController gunController);

    public event DetachEvent DetachedAccidentally;

    #endregion

    #region Init

    public void Init(IPlayer player) {
        Player = player;
        ShotDeniedSound = GetComponent<GunDisabledSound>();
        Player.PlayerHealth.PlayerRevived += OnPlayerRevived;
        Player.PlayerHealth.PlayerDied += OnPlayerDied;
        Player.TeleportHandler.PlayerTeleporting += OnPlayerTeleporting;
        Player.RotatePlayspaceHandler.RotatingPlaySpace += OnPlayerRotatePlaySpace;
        RayCaster.Init(player);
    }

    private void Awake() {
        Movement = GetComponent<TeleportMovement>();
        _rotatePlayspaceHelper = GetComponent<RotatePlaySpaceMovement>();
        RayCaster = GetComponent<RayCaster>();
    }

    private void OnEnable() {
        if (Movement != null) {
            Movement.Teleported += OnTeleportFinished;
        }

        if (_rotatePlayspaceHelper != null) {
            _rotatePlayspaceHelper.Rotated += OnRotateFinished;
        }
    }

    private void OnDisable() {
        if (Movement != null) {
            Movement.Teleported -= OnTeleportFinished;
        }

        if (_rotatePlayspaceHelper != null) {
            _rotatePlayspaceHelper.Rotated -= OnRotateFinished;
        }
    }

    private void OnPlayerDied(PlayerHealth playerHealth, IPlayer enemyWhoAppliedDamage, byte colliderType) {
        CurrentEnergy = 0;
    }

    private void OnPlayerRevived(IPlayer player) {
        CurrentEnergy = 1;
    }

    private void Start() {
        _noEnergyMultiplier = BalancingConfiguration.Singleton.NoEnergyMultiplier;
        _stateMachine.InitStateMachine(this);
    }

    #endregion

    private void Update() {
        _stateMachine.UpdateCurrentState();
    }

    public void OnTriggerPressed(GunControllerState.TriggerAction triggerAction) {
        _stateMachine.TriggerPressed(triggerAction);
    }

    public void OnTriggerReleased() {
        _stateMachine.TriggerReleased();
    }

    public void OnGripPressed() {
        _stateMachine.GripPressed();
    }

    public void OnTeleportTriggered() {
        _stateMachine.TeleportTriggered();
    }

    public void OnGripReleased() {
        _stateMachine.GripReleased();
    }

    private void OnTeleportFinished(Transform rootTransform) {
        _stateMachine.TeleportFinished();
        if (_stateMachine.CurrentStateIdentifier.Equals(GunControllerStateMachine.State.Teleport))
            _stateMachine.ChangeState(GunControllerStateMachine.State.Idle);
    }

    private void OnRotateFinished(object sender, RotatePlaySpaceMovement.PlaySpaceDirection turningPlateDirection) {
        _stateMachine.TeleportFinished();
        if (_stateMachine.CurrentStateIdentifier.Equals(GunControllerStateMachine.State.Rotate))
            _stateMachine.ChangeState(GunControllerStateMachine.State.Idle);
        DisconnectRope(true);
    }

    public void OnSetActive(bool active) {
        _stateMachine.SetActive(active);
        SetActiveTriggered?.Invoke(active);
    }

    public Chargeable DoRaycast() {
        if (RayCaster == null) {
            Debug.Log("GunController ray caster is not set!");
            return null;
        }

        return RayCaster.DoRaycast();
    }

    public void ResetRayCaster() {
        if (RayCaster == null) {
            Debug.Log("GunController ray caster is not set!");
            return;
        }

        RayCaster.Reset();
    }

    private void Fire() {
        if (_shotGameAction != null)
            _shotGameAction.Shoot(Player, _muzzle.position, _muzzle.rotation);
        ShotTriggered?.Invoke();
    }

    private void ConnectBeam([NotNull] Chargeable chargeable) {
        if (chargeable == null)
            throw new ArgumentException("Chargeable is null");

        Chargeable = chargeable;
        if (_ropeGameAction != null && Chargeable != null && Player != null)
            _ropeGameAction.ConnectRope(Chargeable, Player);
    }

    public void RequestRopeDisconnect(bool onPurpose) {
        _stateMachine.DisconnectBeam(onPurpose);
    }

    private void TryToAttachRopeAndFail(Chargeable chargeable) {
        Chargeable = chargeable;
        if (chargeable != null)
            _ropeGameAction.TryToAttachRopeAndFail(chargeable, Player);
    }

    private void DisconnectRope(bool onPurpose) {
        if (_ropeGameAction != null)
            _ropeGameAction.DisconnectRope(Chargeable, Player);
        if(!onPurpose) DetachedAccidentally?.Invoke(this);
        Chargeable = null;
    }

    private void Teleport(Pillar target) {
        TeleportTriggered?.Invoke(Player, target);
    }

    private void Teleport(RotatePlaySpaceHook target) {
        RotationTriggered?.Invoke(Player, target);
    }

    private void DenyTeleport(Pillar pillar) {
        TeleportDenied?.Invoke(Player, pillar);
    }
    private void OnPlayerTeleporting(TeleportHandler sender, Pillar origin, Pillar target, float timeToTeleport) {
        if (_stateMachine.CurrentStateIdentifier.Equals(GunControllerStateMachine.State.Charge))
            _stateMachine.ChangeState(GunControllerStateMachine.State.Teleport);
    }

    private void OnPlayerRotatePlaySpace(object sender, RotatePlaySpaceHook target) {
        if (_stateMachine.CurrentStateIdentifier.Equals(GunControllerStateMachine.State.Charge))
            _stateMachine.ChangeState(GunControllerStateMachine.State.Rotate);
    }

    private void OnDestroy() {
        if (Player != null && Player.PlayerHealth != null) {
            Player.PlayerHealth.PlayerRevived -= OnPlayerRevived;
            Player.PlayerHealth.PlayerDied -= OnPlayerDied;
            Player.RotatePlayspaceHandler.RotatingPlaySpace -= OnPlayerRotatePlaySpace;
        }
    }
}