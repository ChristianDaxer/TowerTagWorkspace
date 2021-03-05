using TowerTag;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Action/ForcedRestart")]
public class ForcedRestartGameAction : GameAction<ForcedRestartParameters> {
    protected override byte EventCode => 6;
    protected override byte DenyEventCode => 7;

    protected override bool IsValid(int senderId, ForcedRestartParameters parameters) {
        return parameters.TriggeredBy == PhotonService.MasterClient.ActorNumber;
    }

    protected override void Execute(int senderId, ForcedRestartParameters parameters) {
        PlayerManager.GetAllConnectedPlayers(out var players, out var count);
        for (int i = 0; i < players.Length; i++)
            players[i].RestartClient();
    }

    protected override void Deny(int senderId, ForcedRestartParameters parameters) {
        Debug.LogWarning("Forced Restart Denial. Ignored");
    }

    protected override void Rollback(int senderId, ForcedRestartParameters parameters) {
        Debug.LogWarning("Forced Restart Rollback. Ignored");
    }

    public void TriggerForcedRestart() {
        Trigger(new ForcedRestartParameters());
    }
}

public class ForcedRestartParameters : GameActionParameter {
    protected override object[] SerializeParameters() {
        return new object[0];
    }

    protected override void DeserializeParameters(object[] objects) { }
}