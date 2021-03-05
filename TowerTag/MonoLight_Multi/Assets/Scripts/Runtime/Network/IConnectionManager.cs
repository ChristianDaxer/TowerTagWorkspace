using Photon.Pun;
using Photon.Realtime;
using IPlayer = TowerTag.IPlayer;

public interface IConnectionManager {
    event ConnectionManager.ConnectionStateDelegate ConnectionStateChanged;
    event ConnectionManager.RoomDelegate JoinedRoom;
    event ConnectionManager.ErrorDelegate ErrorOccured;
    string GameVersion { get; set; } // public getter is used in build
    bool Rejoining { get; }
    ConnectionManager.ConnectionState ConnectionManagerState { get; set; }

    /// <summary>A cached reference to a PhotonView on this GameObject.</summary>
    /// <remarks>
    /// If you intend to work with a PhotonView in a script, it's usually easier to write this.photonView.
    ///
    /// If you intend to remove the PhotonView component from the GameObject but keep this Photon.MonoBehaviour,
    /// avoid this reference or modify this code to use PhotonView.Get(obj) instead.
    /// </remarks>
    IPhotonView photonView { get; }

    void Init(IPhotonService photonService, IMatchMaker matchMaker);
    bool Connect();
    bool StartMatchmaking();
    void LeaveRoom();

    /// <summary>
    /// Disconnect from Photon Server.
    /// </summary>
    void Disconnect();

    void OnDisconnected(DisconnectCause cause);
    void OnJoinRoomFailed(short returnCode, string message);
    void OnJoinRandomFailed(short returnCode, string message);
    void OnConnectedToMaster();
    void OnJoinedLobby();
    void OnCreatedRoom();
    void OnJoinedRoom();
    void OnLeftRoom();
    void OnMasterClientSwitched(Player newMasterClient);
    void OnPlayerEnteredRoom(Player otherPlayer);
    void OnPlayerLeftRoom(Player otherPlayer);

    /// <summary>
    /// Disconnect a specific player from the Photon Server.
    /// </summary>
    /// <param name="player"></param>
    void DisconnectPlayer(IPlayer player);
}