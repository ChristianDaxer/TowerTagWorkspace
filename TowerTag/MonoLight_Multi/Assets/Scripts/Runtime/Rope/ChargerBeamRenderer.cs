using System;
using System.Collections;
using TowerTag;
using UnityEngine;

public abstract class ChargerBeamRenderer : MonoBehaviour, IChargerBeamRenderer
{
    // Events:
    // 
    // RollOutStarted               // gun shots beam out of the gun (when Connect(..) function is called)
    // RollOutFinished              // beam top arrived at target
    // RopeDisconnectsByCollision   // disconnectEvent if the Rope collides with something
    // RollInStarted                // the beam starts to roll back into the gun (when Disconnect() function is called)
    // RollInFinished               // beam top arrived at gun

    // is this rope on a local player or on a remote client?

    private IPlayer Owner { get; set; }

    // teleportTrigger changed Event
    public event Action TeleportTriggered;

    public event Action<float> TensionValueChanged;

    // the beam comes out of the gun and moves toward the target
    public event Action RollingOut;
    public event Action RolledOut;

    // the rope rolls back into the gun
    public event Action RollingIn;

    [SerializeField]
    private RopeGameAction _ropeGameAction;

    private Chargeable _chargeable;

    private void OnEnable()
    {
        _ropeGameAction.RopeConnectedToChargeable += OnRopeConnectedToChargeable;
        _ropeGameAction.Disconnecting += OnDisconnecting;
        _ropeGameAction.AttachFailed += OnAttachFailed;
    }

    private void OnDisable()
    {
        _ropeGameAction.RopeConnectedToChargeable -= OnRopeConnectedToChargeable;
        _ropeGameAction.Disconnecting -= OnDisconnecting;
        _ropeGameAction.AttachFailed -= OnAttachFailed;
    }

    private void Update()
    {
        if (!IsConnected) return;
        if(_chargeable is Claimable claimable && claimable.OwningTeamID == Owner.TeamID)
            UpdateChargeValue(1);
        else
            UpdateChargeValue(_chargeable.CurrentCharge.value);
    }

    private void OnRopeConnectedToChargeable(RopeGameAction sender, IPlayer player, Chargeable target)
    {
        if(player != Owner) return;
        Connect(target);
    }

    private void OnDisconnecting(RopeGameAction sender, IPlayer player, Chargeable target, bool onPurpose)
    {
        if(player != Owner) return;
        Disconnect();
    }


    private void OnAttachFailed(RopeGameAction sender, IPlayer player, Chargeable pillar) {
        if (player != Owner) return;
        StartCoroutine(AttachFailBehaviour(pillar));
    }

    protected abstract IEnumerator AttachFailBehaviour(Chargeable pillar);

    // property to check the tension of the beam (Rope) [0..1]

    public virtual float Tension => -1f;

    public bool IsConnected { get; private set; }

	// connects beam between chargerSpawnAnchor (Gun) and targetAnchor (Pillar)
	public virtual void Connect(Chargeable target)
    {
        _chargeable = target;
        IsConnected = true;

        RollingOut?.Invoke();
    }

    // updates the charge value while charging
    public abstract void UpdateChargeValue(float currentCharge);

    // disconnects (disables) beam
    public virtual void Disconnect()
    {
        IsConnected = false;
        RollingIn?.Invoke();
    }

    public IPlayer GetOwner() {
        return Owner;
    }

    // set if this rope is on a local player instance (or on a remote client)
    public virtual void SetOwner(IPlayer owner)
    {
        Owner = owner;
    }

    // receive OnTeamChanged event to change ropeColors
    public abstract void OnTeamChanged(IPlayer player, TeamID teamID);

    protected void TriggerTeleport() {
        TeleportTriggered?.Invoke();
    }

    protected void OnRolledOut() {
        RolledOut?.Invoke();
    }

    protected void OnTensionValueChanged(float tensionValue) {
        TensionValueChanged?.Invoke(tensionValue);
    }
}
