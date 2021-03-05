using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public interface IPhotonService {
    IPlayer LocalPlayer { get; }
    IPlayer MasterClient { get; }
    string NickName { get; set; }
    bool IsMasterClient { get; }
    bool IsConnectedAndReady { get; }
    ClientState NetworkClientState { get; }
    IRoom CurrentRoom { get; }
    int ServerTimestamp { get; }
    int RoundTripTime { get; }
    bool InRoom { get; }
    LoadBalancingClient NetworkingClient { get; }
    void AddCallbackTarget(object obj);
    void RemoveCallbackTarget(object obj);
    void RaiseEvent(byte eventCode, object[] serialize, RaiseEventOptions raiseEventOptions, SendOptions sendReliably);
    bool ConnectUsingSettings();
    void CreateRoom(string configurationRoom, RoomOptions roomOptions, TypedLobby typedLobby);
    void JoinRoom(string roomName);
    void InstantiateSceneObject(string prefabName, Vector3 position, Quaternion rotation, byte group = 0, object[] data = null);
    void Destroy(GameObject go);
    void SetMasterClient(IPlayer masterClient);
    GameObject Instantiate(string prefabName, Vector3 position, Quaternion rotation);
}

public class PhotonService : IPhotonService {
    public IPlayer LocalPlayer => PhotonNetwork.LocalPlayer;
    public IPlayer MasterClient => PhotonNetwork.MasterClient;
    public bool IsMasterClient => PhotonNetwork.IsMasterClient;
    public bool IsConnectedAndReady => PhotonNetwork.IsConnectedAndReady;
    public ClientState NetworkClientState => PhotonNetwork.NetworkClientState;
    public IRoom CurrentRoom => PhotonNetwork.CurrentRoom;
    public int ServerTimestamp => PhotonNetwork.ServerTimestamp;
    public bool InRoom => PhotonNetwork.InRoom;
    public LoadBalancingClient NetworkingClient => PhotonNetwork.NetworkingClient;
    public int RoundTripTime => PhotonNetwork.NetworkingClient.LoadBalancingPeer.RoundTripTime;

    public string NickName {
        get => PhotonNetwork.NickName;
        set => PhotonNetwork.NickName = value;
    }

    public void AddCallbackTarget(object obj) {
        PhotonNetwork.AddCallbackTarget(obj);
    }

    public void RemoveCallbackTarget(object obj) {
        PhotonNetwork.RemoveCallbackTarget(obj);
    }

    public void RaiseEvent(byte eventCode, object[] serialize, RaiseEventOptions raiseEventOptions,
        SendOptions sendReliably) {
        PhotonNetwork.RaiseEvent(eventCode, serialize, raiseEventOptions, sendReliably);
    }

    public bool ConnectUsingSettings() {
        return PhotonNetwork.ConnectUsingSettings();
    }

    public void CreateRoom(string configurationRoom, RoomOptions roomOptions, TypedLobby typedLobby) {
        PhotonNetwork.CreateRoom(configurationRoom, roomOptions, typedLobby);
    }

    public void JoinRoom(string roomName) {
        PhotonNetwork.JoinRoom(roomName);
    }

    public void InstantiateSceneObject(string prefabName, Vector3 position, Quaternion rotation, byte group = 0, object[] data = null) {
        PhotonNetwork.InstantiateSceneObject(prefabName, position, rotation, group, data);
    }

    public void Destroy(GameObject go) {
        PhotonNetwork.Destroy(go);
    }

    public void SetMasterClient(IPlayer masterClient) {
        PhotonNetwork.SetMasterClient(masterClient as Player);
    }

    public GameObject Instantiate(string prefabName, Vector3 position, Quaternion rotation) {
        return PhotonNetwork.Instantiate(prefabName, position, rotation);
    }
}