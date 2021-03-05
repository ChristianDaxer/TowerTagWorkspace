using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Runtime.Friending;
#if !UNITY_ANDROID
using Steamworks;
#endif
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Home.UI
{
    public class FriendsUiController : HomeMenuPanel
    {
        public enum FriendsUiButton
        {
            JoinFriend,
            InviteFriend
        }

        public delegate void FriendUiControllerButtonAction(object sender, FriendsUiButton button, string roomName);

        public event FriendUiControllerButtonAction FriendUiButtonClicked;

        [SerializeField] private RectTransform _roomLineContainer;
        [SerializeField] private FriendPanelLine _roomLinePrefab;
        [SerializeField] private Button _joinButton;
        [SerializeField] private Button _inviteButton;
        [SerializeField] private TMPro.TextMeshProUGUI _noteLine;
        

        private FriendPanelLine _selected;

        public FriendPanelLine Selected => _selected;

        private FriendsManager _friendsManager;

#if !UNITY_ANDROID
        [UsedImplicitly] private Callback<PersonaStateChange_t> _callbackSteamPersonaStateChange;
#endif

        private Dictionary<ulong, FriendPanelLine> FriendList { get; set; } =
            new Dictionary<ulong, FriendPanelLine>();

        public override void OnEnable()
        {
            base.OnEnable();

            _joinButton.interactable = false;
            _inviteButton.interactable = false;
            _friendsManager = gameObject.GetComponent<FriendsManager>();
        }

        public override void OnDisable()
        {
            base.OnDisable();

            CleanUpPanelLines();
        }

        private void Select(FriendPanelLine friendLine)
        {
            if (_selected != null && _selected != friendLine) _selected.Deselect();
            _selected = friendLine;
            _joinButton.interactable = friendLine.Data.IsInRoom;
            _inviteButton.interactable = FriendsManager.CanPlayerInviteToMatch(friendLine.Data.UserId);
        }

        public void UpdateSelectedFriendLine()
        {
            if (_selected == null) return;
            _joinButton.interactable = _selected.Data.IsInRoom;
            _inviteButton.interactable = FriendsManager.CanPlayerInviteToMatch(_selected.Data.UserId);
        }

        public void FillUIPanelWithFriendLines(IEnumerable<FriendLineInfo> currentFriends)
        {
            var friendLineInfos = currentFriends.ToList();


            if (friendLineInfos.ToList().Count <= 0)
            {
                return;
            }


            friendLineInfos.ForEach(friend =>
            {
                if (friend == null)
                {
                    Debug.LogWarning("Null Reference Exception in Fill Ui Panel with Friend Lines");
                    return;
                }

                if (!friend.IsInGame) return;

                var friendLineInfo = new FriendLineInfo(
                    friend.UserId,
                    friend.Name,
                    friend.IsInGame,
                    friend.IsInRoom,
                    friend.RoomName,
                    friend.FriendsPlayerStatistics);

                if (friendLineInfo.IsInRoom)
                {
                    var roomInfo = _friendsManager.GetFriendsRoomInfo(friend.RoomName);
                    if (roomInfo != null)
                        friendLineInfo.RoomInfo = (RoomLine.RoomLineData) roomInfo;
                }

                if (FriendList.ContainsKey(friendLineInfo.UserId))
                {
                    FriendList[friendLineInfo.UserId].UpdateFriendLineInfo(friendLineInfo);
                }
                else
                {
                    var friendLine = _roomLinePrefab.Create(friendLineInfo, _roomLineContainer, Select);
                    FriendList.Add(friendLineInfo.UserId, friendLine);
                }
            });

            UpdateCurrentFriendLineStyle();
        }

        private void UpdateCurrentFriendLineStyle()
        {
            if (FriendList.Count <= 0) return;

            // Sort bei online flag
            FriendList = FriendList.OrderByDescending(element => element.Value.Data.IsInGame)
                .ToDictionary(i => i.Key, j => j.Value);

            var childIndex = 0;

            // set background dependent on Online flag
            foreach (var friendLine in FriendList.Values)
            {
                var style = friendLine.Data.IsInGame ? RoomLine.BackgroundStyle.Light : RoomLine.BackgroundStyle.Dark;
                friendLine.transform.SetSiblingIndex(childIndex++);
                friendLine.SetBackgroundStyle(style);
                if (friendLine != _selected || friendLine.gameObject.activeSelf) continue;
                _joinButton.interactable = false;
                _inviteButton.interactable = false;
                _selected = null;
            }
        }

        public void CleanUpPanelLines()
        {
            foreach (var friendLine in FriendList.ToList())
            {
#if UNITY_EDITOR
                DestroyImmediate(FriendList[friendLine.Key].gameObject);
#else
                Destroy(FriendList[friendLine.Key].gameObject);
#endif
                FriendList.Remove(friendLine.Value.Data.UserId);
            }
        }

        public void UpdateVersionNotes(string notes)
        {
            _noteLine.text = notes;
        }

#region Buttons

        [UsedImplicitly]
        public void OnBackButtonPressed()
        {
            UIController.SwitchPanel(HubUIController.PanelType.MainMenu);
        }

        [UsedImplicitly]
        public void OnJoinFriendButtonPressed()
        {
            if (_selected == null) return;
            if(_friendsManager.CanPlayerJoinMatch(_selected.Data.UserId))
                FriendUiButtonClicked?.Invoke(this, FriendsUiButton.JoinFriend, _selected.Data.RoomName);
            else {
                MessageQueue.Singleton.AddErrorMessage(
                    "FRIENDS ROOM YOU WANT TO JOIN IS FULL",
                    "ERROR");
            }
        }

        [UsedImplicitly]
        public void OnInviteFriendButtonPressed()
        {
            if (_selected == null) return;
            FriendUiButtonClicked?.Invoke(this, FriendsUiButton.InviteFriend, _selected.Data.RoomName);
        }

#endregion
    }
}