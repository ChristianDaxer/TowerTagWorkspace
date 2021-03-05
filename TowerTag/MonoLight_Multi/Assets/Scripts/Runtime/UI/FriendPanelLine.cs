using System;
using Runtime.Friending;
using TMPro;
using TowerTag;
using UnityEngine;
using UnityEngine.UI;

namespace Home.UI {
    public class FriendPanelLine : MonoBehaviour {
        [SerializeField] private TMP_Text _friendNameText;
        [SerializeField] private TMP_Text _rankText;
        [SerializeField] private TMP_Text _lvlText;
        [SerializeField] private TMP_Text _roomNameText;
        [SerializeField] private Image _lockedImage;
        [SerializeField] private TMP_Text _playerInRoomText;
        [SerializeField] private TMP_Text _minMaxRankText;
        [SerializeField] private TMP_Text _gameModeText;
        [SerializeField] private TMP_Text _mapText;
        [SerializeField] private Image _background;

        private Action<FriendPanelLine> _onSelected;
        private RoomLine.BackgroundStyle _backgroundStyle;
        private bool _selected;
        private bool _interactable;

        public FriendLineInfo Data { get; private set; }

        private enum ShortGameModes {
            EL = 0,
            DT = 1,
            GT = 2,
            UV = 99
        }

        public FriendPanelLine Create(FriendLineInfo friendLineInfo, Transform parent,
            Action<FriendPanelLine> onSelected) {
            FriendPanelLine friendLine = InstantiateWrapper.InstantiateWithMessage(this, parent);
            friendLine.UpdateFriendLineInfo(friendLineInfo);
            friendLine.SetBackgroundStyle(RoomLine.BackgroundStyle.Light);
            friendLine._onSelected = onSelected;
            return friendLine;
        }

        private void OnEnable() {
            _lockedImage.enabled = false;
        }

        private void OnDisable() {
            if (_selected) Deselect();
        }

        public void Select() {
            if (!_interactable) return;
            _selected = true;
            Refresh();
            _onSelected?.Invoke(this);
        }

        public void Deselect() {
            if (!_interactable) return;
            _selected = false;
            Refresh();
        }

        public void UpdateFriendLineInfo(FriendLineInfo data) {
            Data = data;
            if (_friendNameText != null)
                _friendNameText.text = data.Name;

            if (_rankText != null) {
                _rankText.text = data.FriendsPlayerStatistics != null
                    ? data.FriendsPlayerStatistics.ranking.ToString()
                    : "N/A";
            }

            if (_lvlText != null) {
                _lvlText.text = data.FriendsPlayerStatistics != null
                    ? data.FriendsPlayerStatistics.level.ToString()
                    : "N/A";
            }

            if (_roomNameText != null)
                _roomNameText.text = data.RoomName;

            if (_lockedImage != null)
                _lockedImage.enabled = data.RoomInfo.PinLocked;


            if (_playerInRoomText != null)
                _playerInRoomText.text = data.IsInRoom ? $"{data.RoomInfo.CurrentPlayers} / {data.RoomInfo.MaxPlayers}" : "";

            if (_minMaxRankText != null)
                _minMaxRankText.text = data.IsInRoom ? $"{data.RoomInfo.MinRank} - {data.RoomInfo.MaxRank}" : "";

            if (data.RoomInfo.GameMode != null) {
                var gameModeString = data.RoomInfo.GameMode == "- USERVOTE -" ? "UserVote" : data.RoomInfo.GameMode;
                var gameMode = (int) Enum.Parse(typeof(GameMode), gameModeString);
                var shortGameMode = (ShortGameModes) gameMode;
                if (_gameModeText != null)
                    _gameModeText.text = shortGameMode.ToString();
            }
            else {
                if (_gameModeText != null)
                    _gameModeText.text = "";
            }

            if (_mapText != null)
                _mapText.text = data.RoomInfo.Map;
            _interactable = data.IsInGame;
            Refresh();
        }

        public void SetBackgroundStyle(RoomLine.BackgroundStyle backgroundStyle) {
            _backgroundStyle = backgroundStyle;
            Refresh();
        }

        private void Refresh() {
            Color baseColor = TeamManager.Singleton.TeamIce.Colors.UI;

            if (!_interactable) {
                _background.color = baseColor * 0.0f;
                _friendNameText.color = TeamManager.Singleton.TeamIce.Colors.DarkUI;
                _rankText.color = TeamManager.Singleton.TeamIce.Colors.DarkUI;
                _lvlText.color = TeamManager.Singleton.TeamIce.Colors.DarkUI;
                _roomNameText.color = TeamManager.Singleton.TeamIce.Colors.DarkUI;
                _playerInRoomText.color = TeamManager.Singleton.TeamIce.Colors.DarkUI;
                _minMaxRankText.color = TeamManager.Singleton.TeamIce.Colors.DarkUI;
                _gameModeText.color = TeamManager.Singleton.TeamIce.Colors.DarkUI;
                _mapText.color = TeamManager.Singleton.TeamIce.Colors.DarkUI;
                return;
            }

            if (!_selected && _backgroundStyle == RoomLine.BackgroundStyle.Dark) {
                _background.color = baseColor * 0.0f;
                _friendNameText.color = TeamManager.Singleton.TeamIce.Colors.UI;
                _rankText.color = TeamManager.Singleton.TeamIce.Colors.UI;
                _lvlText.color = TeamManager.Singleton.TeamIce.Colors.UI;
                _roomNameText.color = TeamManager.Singleton.TeamIce.Colors.UI;
                _playerInRoomText.color = TeamManager.Singleton.TeamIce.Colors.UI;
                _minMaxRankText.color = TeamManager.Singleton.TeamIce.Colors.UI;
                _gameModeText.color = TeamManager.Singleton.TeamIce.Colors.UI;
                _mapText.color = TeamManager.Singleton.TeamIce.Colors.UI;
            }

            if (!_selected && _backgroundStyle == RoomLine.BackgroundStyle.Light) {
                _background.color = baseColor * 0.3f;
                _friendNameText.color = TeamManager.Singleton.TeamIce.Colors.UI;
                _rankText.color = TeamManager.Singleton.TeamIce.Colors.UI;
                _lvlText.color = TeamManager.Singleton.TeamIce.Colors.UI;
                _roomNameText.color = TeamManager.Singleton.TeamIce.Colors.UI;
                _playerInRoomText.color = TeamManager.Singleton.TeamIce.Colors.UI;
                _minMaxRankText.color = TeamManager.Singleton.TeamIce.Colors.UI;
                _gameModeText.color = TeamManager.Singleton.TeamIce.Colors.UI;
                _mapText.color = TeamManager.Singleton.TeamIce.Colors.UI;
            }

            if (_selected) {
                _background.color = baseColor * 0.8f;
                _friendNameText.color = Color.black;
                _rankText.color = Color.black;
                _lvlText.color = Color.black;
                _roomNameText.color = Color.black;
                _playerInRoomText.color = Color.black;
                _minMaxRankText.color = Color.black;
                _gameModeText.color = Color.black;
                _mapText.color = Color.black;
            }
        }
    }
}