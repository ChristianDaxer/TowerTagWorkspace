using System;
using System.Collections.Generic;
using System.Linq;
using Home.UI;
using Network;
using Photon.Pun;
using Photon.Realtime;
using TowerTagAPIClient;
using TowerTagAPIClient.Model;
using TowerTagAPIClient.Store;
using UI;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Runtime.Friending {
    public class FriendsManager : MonoBehaviourPunCallbacks {
        [SerializeField] private FriendsUiController _friendsUiController;

        private BaseFriendsManager _baseFriendsManager;
        private PhotonFriendsManager _photonFriendsManager;
        private bool _friendsManagerInitialized;
        private List<RoomInfo> _currentPhotonRoomList = new List<RoomInfo>();
        private bool _initCoroutineIsRunning;


        private Dictionary<ulong, FriendLineInfo> CurrentFriendList { get; } =
            new Dictionary<ulong, FriendLineInfo>();


        private void Awake() {
            if (!TowerTagSettings.IsHomeTypeSteam && !TowerTagSettings.IsHomeTypeOculus)
                Destroy(gameObject);

            Init();
        }

        private void Init() {
            if (_friendsManagerInitialized)
                return;

            _baseFriendsManager = BaseFriendsManager.Instance;
            _photonFriendsManager = _baseFriendsManager.GetComponent<PhotonFriendsManager>();

            if (_baseFriendsManager == null)
            {
                Debug.LogWarning("Can't find friends Manager.");
                _friendsManagerInitialized = false;
            }
            else
                _friendsManagerInitialized = true;


            if (_photonFriendsManager == null) {
                Debug.LogWarning("Can't find photon friends Manager.");
                _friendsManagerInitialized = false;
            }
            else
                _friendsManagerInitialized = true;


            if (_friendsUiController == null) {
                Debug.LogWarning("Can't find friends Ui Controller.");
                _friendsManagerInitialized = false;
            }
            else
                _friendsManagerInitialized = true;

            if (_friendsUiController!= null && _baseFriendsManager != null)
            {
                _friendsUiController.UpdateVersionNotes(_baseFriendsManager.VersionNote);
            }
        }

        private void FindFriends() {

            foreach (var friendInfo in _baseFriendsManager.GetCurrentActiveFriends())
            {
                _photonFriendsManager.AddFriendId(friendInfo.UserId);
            }

            _photonFriendsManager.StartFriendListUpdateTick(true);

            RegisterFriendsOnPlayerStatisticsStore(CurrentFriendList.Values);

            _friendsUiController.FillUIPanelWithFriendLines(CurrentFriendList.Values);
        }

        private void RegisterFriendsOnPlayerStatisticsStore(IEnumerable<FriendLineInfo> currentFriends) {
            foreach (var friend in currentFriends) {
                if (!friend.IsInGame) continue;
                if (!PlayerStatisticsStore.PlayerStatistics.ContainsKey(friend.UserId.ToString())) {
                    PlayerStatisticsStore.GetStatistics(Authentication.PlayerApiKey, friend.UserId.ToString());
                }
            }
        }

        public override void OnEnable() {
            base.OnEnable();

            _baseFriendsManager.OnFriendsManagerState(true);
            // Check correct init state
            if (!_friendsManagerInitialized)
                return;

            
            // Register friends manager callback event listener
            _baseFriendsManager.FriendListUpdated += OnSteamFriendListUpdated;
            _photonFriendsManager.PhotonFriendListUpdated += OnPhotonFriendListUpdated;
            _friendsUiController.FriendUiButtonClicked += OnFriendUiButtonClicked;
            PlayerStatisticsStore.PlayerStatisticsReceived += OnFriendsStatisticsUpdated;
            FindFriends();
        }


        public override void OnDisable() {
            base.OnDisable();

            _photonFriendsManager?.StartFriendListUpdateTick(false);
            _baseFriendsManager?.OnFriendsManagerState(false);

            // Check correct init state
            if (!_friendsManagerInitialized)
                return;

            // Deregister friends manager callback event listener
            if (_baseFriendsManager != null)
                _baseFriendsManager.FriendListUpdated += OnSteamFriendListUpdated;
            if (_photonFriendsManager != null)
                _photonFriendsManager.PhotonFriendListUpdated -= OnPhotonFriendListUpdated;
            if (_friendsUiController != null)
                _friendsUiController.FriendUiButtonClicked -= OnFriendUiButtonClicked;
            PlayerStatisticsStore.PlayerStatisticsReceived -= OnFriendsStatisticsUpdated;
        }

        private void OnFriendsStatisticsUpdated(PlayerStatistics playerStatistics) {
            KeyValuePair<ulong, FriendLineInfo> updatedFriendStatistics;
            updatedFriendStatistics =
                CurrentFriendList.FirstOrDefault(friend => friend.Key.ToString() == playerStatistics.id);

            if (updatedFriendStatistics.Value == null) return;
            updatedFriendStatistics.Value.FriendsPlayerStatistics = playerStatistics;
            _friendsUiController.FillUIPanelWithFriendLines(CurrentFriendList.Values);
        }

        private void OnFriendUiButtonClicked(object sender, FriendsUiController.FriendsUiButton button, string roomName) {
            switch (button) {
                case FriendsUiController.FriendsUiButton.JoinFriend:
                    JoinFriendsRoom(roomName);
                    break;
                case FriendsUiController.FriendsUiButton.InviteFriend:
                    InviteFriendToMyRoom();
                    break;
                default:
                    Debug.LogError("Oh No! Something went wrong. Can't find clicked Friend UI Button");
                    break;
            }
        }

        private void JoinFriendsRoom(string roomName) {
            if (_friendsUiController.Selected.Data.RoomInfo.PinLocked) {
                MessageQueue.Singleton.AddInputFieldMessage(
                    "Please enter the pin",
                    null,
                    null,
                    "PIN",
                    InputFieldHelper.InputFieldType.Pin,
                    null,
                    null,
                    "ENTER",
                    ValidatePassword,
                    "ABORT");
            }
            else
                JoinRoom(roomName);
        }

        private void ValidatePassword(string text) {
            var valid = string.Equals(StringEncoder.DecodeString(_friendsUiController.Selected.Data.RoomInfo.Pin),
                text);
            if (valid)
                JoinRoom(_friendsUiController.Selected.Data.RoomInfo.RoomName);
            else {
                MessageQueue.Singleton.AddErrorMessage(
                    "THE ENTERED PIN IS WRONG",
                    "ERROR");
            }
        }

        private static void JoinRoom(string roomName) {
            ConfigurationManager.Configuration.Room = roomName;
            ServiceProvider.Get<IPhotonService>().JoinRoom(roomName);
        }

        public RoomLine.RoomLineData? GetFriendsRoomInfo(string roomName) {
            // check room
            var room = _currentPhotonRoomList.FirstOrDefault(r =>
                r.Name == roomName);

            // room not found
            if (room == null) return null;

            // get room properties
            Hashtable properties = room.CustomProperties;

            //check room properties
            string gameMode = properties.ContainsKey(RoomPropertyKeys.GameMode)
                ? ((GameMode) properties[RoomPropertyKeys.GameMode]).ToString()
                : "- USERVOTE -";
            string map = properties.ContainsKey(RoomPropertyKeys.Map)
                ? (string) properties[RoomPropertyKeys.Map]
                : "- RANDOM -";
            string roomState = properties.ContainsKey(RoomPropertyKeys.RoomState)
                ? ((RoomConfiguration.RoomState) properties[RoomPropertyKeys.RoomState]).ToString()
                : "";
            int currentPlayers = properties.ContainsKey(RoomPropertyKeys.CurrentPlayers)
                ? (byte) properties[RoomPropertyKeys.CurrentPlayers]
                : 0;
            int maxPlayers = properties.ContainsKey(RoomPropertyKeys.MaxPlayers)
                ? (byte) properties[RoomPropertyKeys.MaxPlayers]
                : 0;
            int minRank = properties.ContainsKey(RoomPropertyKeys.MinRank)
                ? (byte) properties[RoomPropertyKeys.MinRank]
                : 0;
            int maxRank = properties.ContainsKey(RoomPropertyKeys.MaxRank)
                ? (byte) properties[RoomPropertyKeys.MaxRank]
                : 0;
            int hostPing = properties.ContainsKey(RoomPropertyKeys.HostPing)
                ? (int) properties[RoomPropertyKeys.HostPing]
                : 0;
            string pin = properties.ContainsKey(RoomPropertyKeys.PIN)
                ? (string) properties[RoomPropertyKeys.PIN]
                : "";

            // return room infos
            return new RoomLine.RoomLineData {
                RoomName = roomName,
                GameMode = gameMode,
                Map = map,
                RoomState = roomState,
                CurrentPlayers = currentPlayers,
                MaxPlayers = maxPlayers,
                MinRank = minRank,
                MaxRank = maxRank,
                Ping = hostPing,
                PinLocked = !string.IsNullOrEmpty(pin),
                Pin = pin
            };
        }

        private void InviteFriendToMyRoom() {
            // TODO handle friend invites
        }

        private void OnSteamFriendListUpdated(object sender, List<FriendLineInfo> updatedFriendList) {
            Debug.Log("In friend manager, list updated callback");
            foreach (var friendInfo in updatedFriendList)
            {
                _photonFriendsManager.AddFriendId(friendInfo.UserId);
            }
        }

        private void OnPhotonFriendListUpdated(object sender, List<FriendInfo> updatedFriendList) {
            // Create new friend list
            var friendInfoList = new List<FriendLineInfo>();

            // Fill Friend List with friend info
            foreach (var friendInfo in updatedFriendList) {
                ulong.TryParse(friendInfo.UserId, out var userId);

                if (CurrentFriendList.ContainsKey(userId)) {
                    if (CurrentFriendList[userId].IsInGame == friendInfo.IsOnline &&
                        CurrentFriendList[userId].IsInRoom == friendInfo.IsInRoom &&
                        CurrentFriendList[userId].RoomName == friendInfo.Room)
                        continue;
                }

                // Get Steam friend Info Data
                var matchingFriendInfo = _baseFriendsManager.GetCurrentActiveFriends()
                    .FirstOrDefault(friend => friend.UserId == userId);


                // Get Friend Nick Name
                var friendName = matchingFriendInfo == null ? "" : matchingFriendInfo.Name;

                // Get friend Statistics

                var playerStatistics = new PlayerStatistics();
                if (PlayerStatisticsStore.PlayerStatistics.ContainsKey(friendInfo.UserId)) {
                    playerStatistics = PlayerStatisticsStore.PlayerStatistics[friendInfo.UserId];
                }
                else if (friendInfo.IsOnline)
                    PlayerStatisticsStore.GetStatistics(Authentication.PlayerApiKey, friendInfo.UserId);

                // Add friend to new friend list
                friendInfoList.Add(new FriendLineInfo(
                    userId,
                    friendName,
                    friendInfo.IsOnline,
                    friendInfo.IsInRoom,
                    friendInfo.Room,
                    playerStatistics));
            }

            // Update current listed Friends in UI
            UpdateCurrentFriendList(friendInfoList);
        }

        private IEnumerable<FriendLineInfo> GetFriendLineDataList() {
            if (!_friendsManagerInitialized) {
                Init();
            }

            var friendLineDataList = new List<FriendLineInfo>();
            var baseFriendList = _baseFriendsManager.GetCurrentActiveFriends();
            var photonFriendList = _photonFriendsManager.PhotonFriendList;

            foreach (var steamFriend in baseFriendList)
            {
                friendLineDataList.Add(new FriendLineInfo(steamFriend.UserId, steamFriend.Name, steamFriend.IsInGame,
                    false, "", null));
            }

            foreach (var photonFriend in photonFriendList) {
                ulong.TryParse(photonFriend.UserId, out var userId);
                if (!friendLineDataList.Exists(friend => friend.UserId == userId)) continue;
                {
                    var friend = friendLineDataList.FirstOrDefault(f => f.UserId == userId);
                    if (friend == null)
                        continue;
                    friend.IsInRoom = photonFriend.IsInRoom;
                    friend.RoomName = photonFriend.Room;
                }
            }

            return friendLineDataList;
        }

        private void UpdateCurrentFriendList(IEnumerable<FriendLineInfo> friendLineDataList) {
            foreach (var friend in friendLineDataList) {
                // Check current Friend list for friend updates
                if (CurrentFriendList.ContainsKey(friend.UserId)) {
                    CurrentFriendList[friend.UserId].Name = friend.Name;
                    CurrentFriendList[friend.UserId].IsInGame = friend.IsInGame;
                    CurrentFriendList[friend.UserId].IsInRoom = friend.IsInRoom;
                    CurrentFriendList[friend.UserId].RoomName = friend.RoomName;
                }
                else {
                    // add new friend
                    CurrentFriendList.Add(friend.UserId, friend);
                }
            }

            // List Friends
            _friendsUiController.FillUIPanelWithFriendLines(CurrentFriendList.Values);
        }

        public bool CanPlayerJoinMatch(ulong friendId) {
            if (!CurrentFriendList.ContainsKey(friendId)) {
                Debug.LogError("Can't find requested friend Id.");
                return false;
            }

            var friendData = CurrentFriendList.FirstOrDefault(friend => friend.Value.UserId == friendId).Value;

            // Check friend Current Online Status
            if (!friendData.IsInGame) return false;

            //TODO Check if Room have enough Space for me;
            RoomInfo room = _currentPhotonRoomList.FirstOrDefault(r => r.Name == friendData.RoomName);

            if (room == null)
                return false;

            Hashtable properties = room.CustomProperties;
            string roomState = properties.ContainsKey(RoomPropertyKeys.RoomState)
                ? ((RoomConfiguration.RoomState) properties[RoomPropertyKeys.RoomState]).ToString()
                : "";
            if (roomState.Equals(RoomConfiguration.RoomState.Loading.ToString(),
                StringComparison.CurrentCultureIgnoreCase))
                return false;

            if (!room.IsOpen || !room.IsVisible || room.PlayerCount >= room.MaxPlayers)
                return false;

            return true;
        }

        public static bool CanPlayerInviteToMatch(ulong friendId) {
            // check player connection state
            if (ConnectionManager.Instance.ConnectionManagerState != ConnectionManager.ConnectionState.ConnectedToGame)
                return false;

            // Get players current room
            var currentRoom = PhotonNetwork.CurrentRoom;
            if (currentRoom == null)
                return false;

            return currentRoom.IsOpen && currentRoom.IsVisible
                                      && (byte) currentRoom.CustomProperties[RoomPropertyKeys.CurrentPlayers]
                                      < RoomConfiguration.GetMaxPlayersForCurrentRoom();
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList) {
            base.OnRoomListUpdate(roomList);
            _currentPhotonRoomList = roomList;
            _friendsUiController.UpdateSelectedFriendLine();
            _friendsUiController.FillUIPanelWithFriendLines(CurrentFriendList.Values);
        }

        private void CleanFriendsUi() {
            if (_friendsManagerInitialized)
                _friendsUiController.CleanUpPanelLines();
        }

#region Testing

        [ContextMenu("ShowMeAllCurrentFriends")]
        public void ListCurrentSteamFriends() {
            _friendsUiController.FillUIPanelWithFriendLines(CurrentFriendList.Values.ToList());
        }

        [ContextMenu("AddTestFriends")]
        public void AddTestFriends() {
            _friendsUiController.CleanUpPanelLines();

            CurrentFriendList.Clear();

            var newFriend = new FriendLineInfo(0000000000, "Thorsten", true, true, "TestRoom", null);

            CurrentFriendList.Add(000000000, newFriend);

            _friendsUiController.FillUIPanelWithFriendLines(CurrentFriendList.Values.ToList());
        }

        [ContextMenu("ClearPanelLinesAndCurrentFriendList")]
        public void ClearPanelLines() {
            CleanFriendsUi();
        }

        [ContextMenu("GetCurrentSteamFriends")]
        public void TryToGetCurrentFriends() {
            if (_baseFriendsManager == null) {
                Debug.LogError("Can't find Steam Friend Manager.");
                return;
            }

            var testCurrentFriendList = _baseFriendsManager.GetCurrentActiveFriends();

            if (testCurrentFriendList == null || testCurrentFriendList.Count <= 0) {
                Debug.LogWarning("Can't find any Steam Friend Data or User is currently not logged in.");
            }
            else {
                foreach (var friend in testCurrentFriendList) {
                    Debug.LogError($"Steam-Friend with ID {friend.UserId} and Name {friend.Name}");
                }
            }
        }

        [ContextMenu("EmptySteamFriendList")]
        public void FillListWithEmptySteamData() {
            // check init state
            if (!_friendsManagerInitialized) return;

            // stop photon update tick
            _photonFriendsManager.StartFriendListUpdateTick(false);

            // clean up current friend data
            CleanFriendsUi();

            // create empty List
            var emptyList = new List<FriendLineInfo> {null, null, null};
            _friendsUiController.FillUIPanelWithFriendLines(emptyList);
        }

#endregion
    }
}