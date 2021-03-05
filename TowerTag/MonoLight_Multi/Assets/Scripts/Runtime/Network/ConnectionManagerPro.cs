using Network;
using Photon.Pun;
using UI;
using UnityEngine;

public class ConnectionManagerPro : MonoBehaviourPunCallbacks {
    [SerializeField] private MessageQueue _overlayMessageQueue;
    private ConnectionManager _connectionManager;
    private bool _queryPending;

    public void Init(ConnectionManager connectionManager, MessageQueue messageQueue) {
        _connectionManager = connectionManager;
        _overlayMessageQueue = messageQueue;
    }

    public override void OnCreateRoomFailed(short returnCode, string message) {
        Debug.LogWarning($"Failed to create room: {returnCode} | {message}");
        if (!_queryPending) {
            _overlayMessageQueue.AddYesNoMessage(
                "The room you want to create already exists! Do you want to takeover the operation?",
                "Room name already taken?",
                () => { _queryPending = true; },
                () => { _queryPending = false; },
                "Yes",
                ForceCreateRoomAfterFailed,
                "No",
                _connectionManager.ShowRoomRenameAdvice);
        }

        _queryPending = true;
    }

    private void ForceCreateRoomAfterFailed() {
        _queryPending = false;
        PhotonNetwork.JoinOrCreateRoom(ConfigurationManager.Configuration.Room, RoomConfiguration.RoomOptions, null);
    }

    public override void OnConnectedToMaster() {
        if (!_connectionManager.Rejoining)
            _connectionManager.StartMatchmaking();
    }
}
