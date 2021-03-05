#if !UNITY_ANDROID
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Runtime.Friending;
using UnityEngine;

namespace Steamworks {
    public class SteamFriendsManager : BaseFriendsManager {

        [UsedImplicitly] private Callback<PersonaStateChange_t> _callbackSteamPersonaStateChange;

        private List<FriendLineInfo> SteamFriendList { get; } = new List<FriendLineInfo>();


        

        private void OnEnable() {
            RegisterSteamFriendsCallbacks();
        }

        [ContextMenu("TestSteamFriendsManager")]
        public void TestSteamFriends() {
            //Check Steam log in
            if (!SteamManager.Initialized) return;

            // Get users current friends
            var steamFriends = GetCurrentActiveFriends();

            if (steamFriends == null || steamFriends.Count <= 0) {
                Debug.LogWarning("no friend data is available or user is currently not logged in.");
                return;
            }

            // list all friends
            foreach (var friend in steamFriends) {
                Debug.LogError($"Steam-Friend with ID {friend.UserId} and Name {friend.Name}");
            }
        }

        /// <summary>
        /// Get a List of Users Steam Friends.
        /// </summary>
        /// <returns>Returns a List of current Steam Friends. Can be NULL to indicate that no data is available</returns>
        public override List<FriendLineInfo> GetCurrentActiveFriends() {
            if (!SteamManager.Initialized) {
                Debug.LogWarning("SteamManager is not initialized");
                return null;
            }

            // Returns -1 if the current user is not logged in to Steam.

            var friendCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
            if (friendCount == -1) {
                Debug.LogError("current Users Steam-Account is not logged in");
                return null;
            }

            var currentSteamFriends = new List<FriendLineInfo>();

            for (var i = 0; i < friendCount; i++) {
                var cSteamFriend =
                    new CSteamID(SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate).m_SteamID);
                var friendPersonaState = SteamFriends.GetFriendPersonaState(cSteamFriend);

                var friend = new FriendLineInfo(
                    cSteamFriend.m_SteamID, SteamFriends.GetFriendPersonaName(cSteamFriend),
                    friendPersonaState == EPersonaState.k_EPersonaStateOnline,
                    false,
                    "", null);

                currentSteamFriends.Add(friend);
                UpdateFriendList(friend);
            }

            return currentSteamFriends;
        }

        private void RegisterSteamFriendsCallbacks() {
            // called every time when a friends' status changes
            _callbackSteamPersonaStateChange = Callback<PersonaStateChange_t>.Create(OnSteamFriendPersonaStateChange);
        }

        private void OnSteamFriendPersonaStateChange(PersonaStateChange_t param) {
            if (!SteamFriendList.Exists(friend => friend.UserId == param.m_ulSteamID)) {
                //Add missing friend id to friend list
                UpdateFriendList(new FriendLineInfo(param.m_ulSteamID));
            }

            var steamFriend = SteamFriendList.FirstOrDefault(friend => friend.UserId == param.m_ulSteamID);

            if (steamFriend == null) {
                Debug.LogWarning("Can't find steam friend Line in current context.");
                return;
            }

            // check state of name & status; return when nothing changed
            if ((param.m_nChangeFlags & EPersonaChange.k_EPersonaChangeStatus) == 0 &&
                (param.m_nChangeFlags & EPersonaChange.k_EPersonaChangeName) == 0) return;

            var steamFriendData = GetCurrentActiveFriends()
                .FirstOrDefault(friend => friend.UserId == param.m_ulSteamID);
            if (steamFriendData == null) {
                // TODO handle null data exceptions
                // Debug.LogError("Can't find Friend in users current steam Friend List.");
                return;
            }

            var friendLineInfo = new FriendLineInfo(param.m_ulSteamID, steamFriend.Name, steamFriendData.IsInGame,
                false, "", null);

            UpdateFriendList(friendLineInfo);
            RaisedFriendUpdateEvent(FriendList);
        }

        protected override void UpdateFriendList(FriendLineInfo newFriend) {
            if (SteamFriendList.Exists(friend => friend.UserId == newFriend.UserId)) {
                for (var i = 0; i < SteamFriendList.Count; i++) {
                    if (SteamFriendList[i].UserId != newFriend.UserId) continue;
                    SteamFriendList[i] = newFriend;
                }
            }
            else {
                SteamFriendList.Add(newFriend);
            }
        }

        private static FriendGameInfo_t GetUserGameInfo(ulong userId) {
            SteamFriends.GetFriendGamePlayed(new CSteamID(userId), out var friendGameInfoResult);
            return friendGameInfoResult;
        }

        [UsedImplicitly]
        public override bool IsUserInGame(ulong appId, ulong userId) {
            return GetUserGameInfo(userId).m_gameID.m_GameID == (ulong) SteamApps.GetAppBuildId();
        }

        public override void OnFriendsManagerState(bool enabled)
        {
        }

        public override void Init()
        {
            SteamAPI.Init();
        }

        public override bool IsInitialized()
        {
            return SteamManager.Initialized;
        }

        public override void RegisterCallbackOnInitialized(OnPlatformInitializedCallback callback)
        {
            if (!SteamManager.Initialized)
            {
                _waitOnPlatformInit = StartCoroutine(InternalOnInitialized(callback));
            }
            else
            {
                callback.Invoke(this);
            }
        }

        public override void UnregisterCallbackOnInitialized(OnPlatformInitializedCallback callback)
        {
            if (_waitOnPlatformInit != null)
                StopCoroutine(_waitOnPlatformInit);
        }

        private System.Collections.IEnumerator InternalOnInitialized(OnPlatformInitializedCallback callback)
        {
            while (!SteamManager.Initialized)
            {
                yield return new WaitForEndOfFrame();
            }

            callback.Invoke(this);

            yield return null;
        }
    }

/*    // Get Friends current game info
    SteamFriends.GetFriendGamePlayed(new CSteamID(friendData.UserId), out var friendGameInfoResult);
    if (friendGameInfoResult.m_gameID.m_GameID != (ulong) SteamApps.GetAppBuildId()) return false;*/
}
#endif