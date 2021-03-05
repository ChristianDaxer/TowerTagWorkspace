using System;
using Photon.Pun;
using TowerTag;
using UnityEngine;

public class GameManagerNetworkEventHandler : MonoBehaviourPun {
    [SerializeField] private bool _useEncryption = true;

    public void Init() {
        // client to master
        GameManager.Instance.SceneWasLoaded += OnSendSceneWasLoadedToMaster;
        GameManager.Instance.MatchHasFinishedLoading += OnMatchHasFinishedLoading;
        GameManager.Instance.BasicCountdownStarted += OnBasicCountdownStarted;
        GameManager.Instance.BasicCountdownAborted += OnBasicCountdownAborted;
    }

    private void OnMatchHasFinishedLoading(IMatch match) {
        try {
            IPlayer ownPlayer = PlayerManager.Instance.GetOwnPlayer();
            if (ownPlayer != null) {
                photonView.RpcSecure(nameof(OnReceivedMatchHasFinishedLoading), RpcTarget.MasterClient, _useEncryption,
                    match.MatchID, ownPlayer.PlayerID);
            }
        }
        catch (Exception e) {
            Debug.LogException(e);
        }
    }

    [PunRPC]
    private void OnReceivedMatchHasFinishedLoading(int matchID, int playerID, PhotonMessageInfo info) {
        IPlayer player = PlayerManager.Instance.GetPlayer(playerID);
        if (player == null) {
            Debug.LogWarning($"Received MatchHasFinishedLoading event for player with ID {playerID}, but the player could not be found");
            return;
        }
        if (player.PhotonView.OwnerActorNr != info.Sender.ActorNumber) {
            Debug.LogWarning($"Received MatchHasFinishedLoading event for player {playerID}, " +
                             $"which is owned by the player with actor number {player.PhotonView.OwnerActorNr} " +
                             $"from a different sender with actor number {info.Sender.ActorNumber}");
            return;
        }

        GameManager.Instance.OnReceivedPlayerSyncInfoOnMaster(matchID, player);
    }

    private void OnSendSceneWasLoadedToMaster(string scene) {
        IPlayer ownPlayer = PlayerManager.Instance.GetOwnPlayer();
        if(ownPlayer != null) {
            photonView.RpcSecure(nameof(OnReceiveSceneHasFinishedLoading), RpcTarget.MasterClient, _useEncryption,
                scene, ownPlayer.PlayerID);
        }
    }

    [PunRPC]
    private void OnReceiveSceneHasFinishedLoading(string scene, int playerID) {
        GameManager.Instance.OnReceivedOnSceneLoadedOnMaster(scene, playerID);
    }

    private void OnBasicCountdownStarted(float countdownTime) {
        if(PhotonNetwork.IsMasterClient)
            photonView.RpcSecure(nameof(OnCountdownStartedRPC), RpcTarget.All, _useEncryption, countdownTime);
    }

    [PunRPC]
    private void OnCountdownStartedRPC(float countdownTime) {
        if(!PhotonNetwork.IsMasterClient)
            GameManager.Instance.StartMatchCountdown(countdownTime);
    }

    private void OnBasicCountdownAborted() {
        if(PhotonNetwork.IsMasterClient)
            photonView.RpcSecure(nameof(OnCountdownAbortedRPC), RpcTarget.All, _useEncryption);
    }

    [PunRPC]
    private void OnCountdownAbortedRPC() {
        if(!PhotonNetwork.IsMasterClient)
            GameManager.Instance.AbortMatchCountdown();
    }

    public void SendCurrentMatchTimerToLateJoiner(IPlayer player, int startTimestamp, int endTimestamp, int countdownTimeInSeconds) {
        if(PhotonNetwork.IsMasterClient) {
            photonView.RpcSecure(nameof(ReceiveMatchTimerAsLateJoiner), player.PhotonView.Owner, _useEncryption, startTimestamp, endTimestamp, countdownTimeInSeconds);
        }
    }

    [PunRPC]
    private void ReceiveMatchTimerAsLateJoiner(int startTimestamp, int endTimestamp, int countdownTimeInSeconds) {
        PhotonNetwork.FetchServerTimestamp();
        GameManager.Instance.MatchTimer.StartTimerAt(startTimestamp, endTimestamp, countdownTimeInSeconds, true);
    }

    private void OnDestroy() {
        GameManager.Instance.SceneWasLoaded -= OnSendSceneWasLoadedToMaster;
        GameManager.Instance.MatchHasFinishedLoading -= OnMatchHasFinishedLoading;
        GameManager.Instance.BasicCountdownStarted -= OnBasicCountdownStarted;
        GameManager.Instance.BasicCountdownAborted -= OnBasicCountdownAborted;
    }
}