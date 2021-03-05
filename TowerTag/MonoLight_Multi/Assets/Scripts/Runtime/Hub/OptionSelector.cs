using System;
using Photon.Pun;
using TowerTag;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public sealed class UnityPlayerEvent : UnityEvent<IPlayer> { }

public class OptionSelector : Chargeable {
    public override ChargeableType ChargeableType => ChargeableType.Option;
    public override int ID { get; set; }
    public UnityPlayerEvent OnOptionClaimed;
    private StayLoggedInTrigger _loggedInTrigger;

    [SerializeField] private GameObject _particleParent;
    private Player _player;
    protected override bool ManageLocally => true;

    private new void Awake() {
        base.Awake();
        _player = GetComponentInParent<Player>();
    }

    public override bool CanAttach(IPlayer player) {
        return true;
    }

    public override bool CanTryToAttach(IPlayer player) {
        return true;
    }

    public override bool CanCharge(IPlayer player) {
        return true;
    }

    private new void OnEnable() {
        base.OnEnable();
        if (_loggedInTrigger == null)
            _loggedInTrigger = GetComponentInParent<StayLoggedInTrigger>();

        ColorChanger.ChangeColorInChildRendererComponents(_particleParent,
            TeamManager.Singleton.TeamNeutral.Colors.Effect);
        ChargeSet += OnChargeSet;
        PlayerDetached += OnPlayerDetached;
    }

    private new void OnDisable() {
        base.OnDisable();
        ChargeSet -= OnChargeSet;
        PlayerDetached -= OnPlayerDetached;
    }

    private void OnPlayerDetached(Chargeable chargeable, IPlayer player) {
        if (chargeable.AttachedPlayers.Count == 0) {
            _loggedInTrigger.OptionCharging = false;
            CurrentCharge = (teamID: TeamID.Neutral, value: 0);
        }
    }

    private void OnChargeSet(Chargeable chargeable, TeamID team, float value) {
        _loggedInTrigger.OptionCharging = IsBeingCharged;
        _loggedInTrigger.InfoText.text = $"KEEP CLAIMING!\n{Mathf.RoundToInt(CurrentCharge.value * 100)}%";
    }

    protected override void FinishChargingOnManager() {
        Trigger();
    }

    public void Trigger() {
        CurrentCharge = (teamID: TeamID.Neutral, value: 0);
        OnOptionClaimed.Invoke(_player);
        _player.ChargeNetworkEventHandler.SendOptionChargeToMaster(_player, ID);
        _player.GunController.CheckForNull()?.RequestRopeDisconnect(true);
    }

    public void LogOutPlayer(IPlayer player) {
        if (PhotonNetwork.IsMasterClient)
            player.LogOut();

        if(_loggedInTrigger.gameObject.activeInHierarchy)
            _loggedInTrigger.StartCoroutine(_loggedInTrigger.DisplayConfirmationText("LOGGED OUT!"));
    }

    public void StayLoggedIn(IPlayer player) {
        if(_loggedInTrigger.gameObject.activeInHierarchy)
            _loggedInTrigger.StartCoroutine(_loggedInTrigger.DisplayConfirmationText("YOU WILL STAY LOGGED IN!"));
    }
}