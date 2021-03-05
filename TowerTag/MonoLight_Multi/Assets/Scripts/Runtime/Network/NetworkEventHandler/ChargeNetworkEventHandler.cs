using System.Linq;
using Photon.Pun;
using TowerTag;
using UnityEngine;

public class ChargeNetworkEventHandler : MonoBehaviourPun {

    [SerializeField] private bool _useEncryption = true;
    public void SendOptionChargeToMaster(IPlayer player, int optionID) {
        int playerID = -1;
        if (player != null) {
            playerID = player.PlayerID;
        }

        photonView.RpcSecure(nameof(ReceiveOptionChargeEvent), PhotonNetwork.MasterClient, _useEncryption, playerID, optionID);
    }

    [PunRPC]
    private void ReceiveOptionChargeEvent(int playerID, int optionID) {
        if (!TTSceneManager.Instance.IsInHubScene) return;
        if (!PhotonNetwork.IsMasterClient) return;

        IPlayer player = PlayerManager.Instance.GetPlayer(playerID);
        if (player != null && player.CurrentPillar != null) {
            StayLoggedInTrigger loggedInTrigger = player.LoggedInTrigger;
            loggedInTrigger.Options.FirstOrDefault(option => option.ID == optionID)?.OnOptionClaimed.Invoke(player);
        }
    }
}