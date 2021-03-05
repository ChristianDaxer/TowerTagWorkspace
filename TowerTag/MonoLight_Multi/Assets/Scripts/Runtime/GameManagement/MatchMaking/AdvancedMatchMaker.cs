using Network;
using Photon.Pun;
using TowerTagSOES;

public class AdvancedMatchMaker : IMatchMaker
{
    public bool StartMatchMaking() {
        // Only Admins can create a Room, clients can only join:
        // but there is an exception for debugging (if you start from Editor you also can create a room for testing)
#if UNITY_EDITOR
        return PhotonNetwork.JoinOrCreateRoom(ConfigurationManager.Configuration.Room, RoomConfiguration.RoomOptions, null);
#else
        return TowerTagSettings.BasicMode ? PhotonNetwork.JoinOrCreateRoom(ConfigurationManager.Configuration.Room, RoomConfiguration.RoomOptions, null)
            : SharedControllerType.IsAdmin ? PhotonNetwork.CreateRoom(ConfigurationManager.Configuration.Room, RoomConfiguration.RoomOptions, null)
            : PhotonNetwork.JoinRoom(ConfigurationManager.Configuration.Room);

#endif
    }

    public bool StartRandomMatchMaking() {
        return PhotonNetwork.JoinRandomRoom();
    }
}
