using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
#if !UNITY_ANDROID
using Steamworks;
#endif
using UnityEngine;

namespace Runtime.Friending {
    public class PhotonFriendsManager : MonoBehaviourPunCallbacks {
        public delegate void PhotonFriendListUpdate(object sender, List<FriendInfo> currentFriendList);

        public event PhotonFriendListUpdate PhotonFriendListUpdated;

        private BaseFriendsManager _baseFriendsManager;
        private bool _friendListUpdateCoroutineIsRunning;
        private readonly List<string> _currentFriendIdList = new List<string>();

        public List<FriendInfo> PhotonFriendList { get; } = new List<FriendInfo>();

        private void Awake() {
            if (!TowerTagSettings.Home)
                Destroy(this);
        }

        private void Init() {
            if (ConnectionManager.Instance != null)
                ConnectionManager.Instance.JoinedLobby += OnLobbyJoined;
        }

        private static void OnLobbyJoined(ConnectionManager connectionManager) {
            ConnectionManager.Instance.JoinedLobby -= OnLobbyJoined;
        }

        public override void OnEnable() {
            base.OnEnable();

            _baseFriendsManager = BaseFriendsManager.Instance;

            if (_baseFriendsManager == null)
            {
                Debug.LogWarning("Can't find Base Friend Manager.");
                return;
            }

            if (!_baseFriendsManager.IsInitialized())
            {
                Debug.LogWarning("Can't find Steam Manager or User is not logged in.");
                _baseFriendsManager.RegisterCallbackOnInitialized(OnPlatformManagerInitialized);
            }

            Init();
        }

        private void Start()
        {
            _baseFriendsManager = BaseFriendsManager.Instance;

            if (_baseFriendsManager == null)
            {
                Debug.LogWarning("Can't find Base Friend Manager.");
                return;
            }

            if (!_baseFriendsManager.IsInitialized())
            {
                Debug.LogWarning("Can't find Steam Manager or User is not logged in.");
                _baseFriendsManager.RegisterCallbackOnInitialized(OnPlatformManagerInitialized);
            }

            Init();
        }

        public override void OnDisable() {
            base.OnDisable();

            StartFriendListUpdateTick(false);

            if (_baseFriendsManager != null)
            {
                _baseFriendsManager.UnregisterCallbackOnInitialized(OnPlatformManagerInitialized);
            }
        }

        private void OnPlatformManagerInitialized(object sender)
        {
            Init();
        }

        private static void InitSteamFriendListToPhoton(List<string> currentSteamFriends) {
            if (ConnectionManager.Instance.ConnectionManagerState != ConnectionManager.ConnectionState.ConnectedToServer)
                return;
            PhotonNetwork.FindFriends(currentSteamFriends.ToArray());
        }

        public override void OnFriendListUpdate(List<FriendInfo> friendList) {
            base.OnFriendListUpdate(friendList);

            Debug.Log("In photon OnFriendListUpdate - friendlist lenght : " + friendList.Count);

            if (friendList.Count <= 0) {
                Debug.Log("Friend List empty");
                return;
            }

            foreach (var friend in friendList) {
                UpdatePhotonFriendList(friend);
                /*Debug.LogError(
                    $"friend {friend.UserId} is Online: {friend.IsOnline}; Is in Room: {friend.IsInRoom}; RoomName: {friend.Room}");
                var userId = ulong.TryParse(friend.UserId, out var parsedUserId);
                var friendLineData = new FriendLineInfo(parsedUserId);*/
            }

            PhotonFriendListUpdated?.Invoke(this, PhotonFriendList);
        }

        private void UpdatePhotonFriendList(FriendInfo newFriend) {
            if (PhotonFriendList.Exists(friend => friend.UserId == newFriend.UserId)) {
                for (var i = 0; i < PhotonFriendList.Count; i++) {
                    if (PhotonFriendList[i].UserId != newFriend.UserId) continue;
                    PhotonFriendList[i] = newFriend;
                }
            }
            else {
                PhotonFriendList.Add(newFriend);
            }
        }

        public static void InitPhotonFriendList(List<string> currentSteamFriends) {
            InitSteamFriendListToPhoton(currentSteamFriends);
        }

        [ContextMenu("TestPhotonNetworkFindFriends")]
        public void TestPhotonNetworkFindFriends() {
            InitSteamFriendListToPhoton(
                _baseFriendsManager.GetCurrentActiveFriends().Select(
                    friend => friend.UserId.ToString()).ToList());
        }

        public void StartFriendListUpdateTick(bool status) {
            _friendListUpdateCoroutineIsRunning = status;
            if (_friendListUpdateCoroutineIsRunning)
                StartCoroutine(FriendListUpdateTick());
        }

        private IEnumerator FriendListUpdateTick() {
            while (_friendListUpdateCoroutineIsRunning) {
                if (_currentFriendIdList != null && _currentFriendIdList.Count > 0)
                    InitPhotonFriendList(_currentFriendIdList);
                yield return new WaitForSecondsRealtime(1f);
            }
        }

        public void AddFriendId(ulong friendId) {
            if (!_currentFriendIdList.Contains(friendId.ToString()))
                _currentFriendIdList.Add(friendId.ToString());
        }
    }
}