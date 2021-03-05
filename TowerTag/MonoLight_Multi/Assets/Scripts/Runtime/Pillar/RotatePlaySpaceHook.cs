using TowerTag;
using UnityEngine;


public class RotatePlaySpaceHook : Claimable
{
    [SerializeField] private PillarTurningPlateSlotObject _slotObject;

    public PillarTurningPlateSlotObject SlotObject => _slotObject;

    public override ChargeableType ChargeableType => ChargeableType.RotatePlaySpaceHook;
    public override int ID { get; set; }
    protected override bool ManageLocally => true;

    public override bool CanAttach(IPlayer player)
    {
        return true;
    }

    public override bool CanTryToAttach(IPlayer player)
    {
        return true;
    }

    public override bool CanCharge(IPlayer player)
    {
        return CanAttach(player) && player.TeamID != OwningTeamID;
    }

    private new void Awake()
    {
        base.Awake();
        ID = (int) _slotObject.TurningSlot;
    }
}