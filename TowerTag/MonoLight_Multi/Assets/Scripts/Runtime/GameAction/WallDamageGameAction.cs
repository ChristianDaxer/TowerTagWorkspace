using Photon.Pun;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Action/Wall Damage")]
public class WallDamageGameAction : GameAction<WallDamageParameter> {

    [SerializeField, Tooltip("Manager to retrieve pillar walls from their id")]
    private PillarWallManager _pillarWallManager;

    protected override byte EventCode => 4;
    protected override byte DenyEventCode => 5;
    protected override bool IsValid(int senderId, WallDamageParameter parameters) {
        return parameters.TriggeredBy == PhotonNetwork.MasterClient.ActorNumber;
    }

    protected override void Execute(int senderId, WallDamageParameter parameters) {
        PillarWall pillarWall = _pillarWallManager.GetPillarWall(parameters.WallID);
        if(pillarWall != null)
            pillarWall.SetDamage(parameters.WallDamage);
        else {
            Debug.LogWarning($"PillarWall not in the PillarWallManager: {parameters.WallID}");
        }
    }

    protected override void Deny(int senderId, WallDamageParameter parameters) {
        Debug.LogError($"SenderID is not Master: {senderId}");
    }

    //If needed save the old damageValue in parameters and change in Execute
    protected override void Rollback(int senderId, WallDamageParameter parameters) {
        Debug.LogError("Rollback not implemented in WallDamageGameAction");
    }

    public void TriggerWallDamage(string wallID, float wallDamage) {
        Trigger(new WallDamageParameter(wallID, wallDamage));
    }
}

public class WallDamageParameter : GameActionParameter {
    public string WallID { get; private set; }
    public float WallDamage { get; private set; }

    public WallDamageParameter(string wallId, float wallDamage) {
        WallID = wallId;
        WallDamage = wallDamage;
    }

    public WallDamageParameter() {
    }

    protected override object[] SerializeParameters() {
        return new object[] {WallID, WallDamage};
    }

    protected override void DeserializeParameters(object[] objects) {
        WallID = (string) objects[0];
        WallDamage = (float) objects[1];
    }
}
