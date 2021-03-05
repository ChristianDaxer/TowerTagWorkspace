using System;
using Network;
using TMPro;
using TowerTag;
using TowerTagSOES;
using UnityEngine;
using UnityEngine.UI;

namespace Home.UI
{
    public class RoomLine : MonoBehaviour
    {
        [SerializeField] private Image _lockedImage;
        [SerializeField] private TMP_Text _roomNameText;
        [SerializeField] private TMP_Text _gameModeText;
        [SerializeField] private TMP_Text _mapText;
        [SerializeField] private TMP_Text _roomStateText;
        [SerializeField] private TMP_Text _playerCountText;
        [SerializeField] private TMP_Text _rankText;
        [SerializeField] private TMP_Text _pingText;
        [SerializeField] private Image _background;
        private RoomLineData _roomLineData;
        private Action<RoomLine> _onSelected;
        private BackgroundStyle _backgroundStyle;
        private bool _selected;
        private bool _interactable;

        private bool IsRoomInLoadingState => _roomStateText.text.Equals(RoomConfiguration.RoomState.Loading.ToString(),
            StringComparison.CurrentCultureIgnoreCase);

        public RoomLineData Data => _roomLineData;

        public struct RoomLineData
        {
            public string HostName { get; set; }
            public string RoomName { get; set; }
            public string GameMode { get; set; }
            public string Map { get; set; }
            public string RoomState { get; set; }
            public int PlayerCount { get; set; }
            public int MaxPlayers { get; set; }
            public int MinRank { get; set; }
            public int MaxRank { get; set; }
            public int Ping { get; set; }
            public bool PinLocked { get; set; }
            public string Pin { get; set; }
            public int CurrentPlayers { get; set; }
        }

        private void OnDisable()
        {
            if (_selected) Deselect();
        }

        public RoomLine Create(RoomLineData data, Transform parent, Action<RoomLine> onSelected)
        {
            RoomLine roomLine = InstantiateWrapper.InstantiateWithMessage(this, parent);
            roomLine.UpdateRoomInfo(data);
            roomLine.SetBackgroundStyle(BackgroundStyle.Light);
            roomLine._onSelected = onSelected;
            roomLine._lockedImage.enabled = roomLine.Data.PinLocked;
            return roomLine;
        }

        public void Select()
        {
            if (!_interactable) return;
            _selected = true;
            Refresh();
            _onSelected?.Invoke(this);
        }

        public void Deselect()
        {
            if (!_interactable) return;
            _selected = false;
            Refresh();
        }

        public void UpdateRoomInfo(RoomLineData data)
        {
            _roomLineData = data;
            _roomNameText.text = data.HostName;
            _gameModeText.text = data.GameMode;
            _mapText.text = data.Map;
            _roomStateText.text = data.RoomState;
            _playerCountText.text = $"{data.CurrentPlayers} / {data.MaxPlayers}";
            _rankText.text = $"{data.MinRank} - {data.MaxRank}";
            _pingText.text = data.Ping.ToString();
            _interactable = SharedControllerType.Spectator
                ? !IsRoomInLoadingState
                : !IsRoomInLoadingState && data.CurrentPlayers < data.MaxPlayers;
            Refresh();
        }


        public void SetBackgroundStyle(BackgroundStyle backgroundStyle)
        {
            _backgroundStyle = backgroundStyle;
            Refresh();
        }

        private void Refresh()
        {
            Color baseColor = TeamManager.Singleton.TeamIce.Colors.UI;

            if (!_interactable)
            {
                _background.color = baseColor * 0.0f;
                _roomNameText.color = TeamManager.Singleton.TeamIce.Colors.DarkUI;
                _gameModeText.color = TeamManager.Singleton.TeamIce.Colors.DarkUI;
                _mapText.color = TeamManager.Singleton.TeamIce.Colors.DarkUI;
                _roomStateText.color = TeamManager.Singleton.TeamIce.Colors.DarkUI;
                _playerCountText.color = TeamManager.Singleton.TeamIce.Colors.DarkUI;
                _rankText.color = TeamManager.Singleton.TeamIce.Colors.DarkUI;
                _pingText.color = TeamManager.Singleton.TeamIce.Colors.DarkUI;
                _lockedImage.color = TeamManager.Singleton.TeamIce.Colors.DarkUI;
                return;
            }

            if (!_selected && _backgroundStyle == BackgroundStyle.Dark)
            {
                _background.color = baseColor * 0.0f;
                _roomNameText.color = TeamManager.Singleton.TeamIce.Colors.UI;
                _gameModeText.color = TeamManager.Singleton.TeamIce.Colors.UI;
                _mapText.color = TeamManager.Singleton.TeamIce.Colors.UI;
                _roomStateText.color = TeamManager.Singleton.TeamIce.Colors.UI;
                _playerCountText.color = TeamManager.Singleton.TeamIce.Colors.UI;
                _rankText.color = TeamManager.Singleton.TeamIce.Colors.UI;
                _pingText.color = TeamManager.Singleton.TeamIce.Colors.UI;
                _lockedImage.color = TeamManager.Singleton.TeamIce.Colors.UI;
            }

            if (!_selected && _backgroundStyle == BackgroundStyle.Light)
            {
                _background.color = baseColor * 0.3f;
                _roomNameText.color = TeamManager.Singleton.TeamIce.Colors.UI;
                _gameModeText.color = TeamManager.Singleton.TeamIce.Colors.UI;
                _mapText.color = TeamManager.Singleton.TeamIce.Colors.UI;
                _roomStateText.color = TeamManager.Singleton.TeamIce.Colors.UI;
                _playerCountText.color = TeamManager.Singleton.TeamIce.Colors.UI;
                _rankText.color = TeamManager.Singleton.TeamIce.Colors.UI;
                _pingText.color = TeamManager.Singleton.TeamIce.Colors.UI;
                _lockedImage.color = TeamManager.Singleton.TeamIce.Colors.UI;
            }

            if (_selected)
            {
                _background.color = baseColor * 0.8f;
                _roomNameText.color = Color.black;
                _gameModeText.color = Color.black;
                _mapText.color = Color.black;
                _roomStateText.color = Color.black;
                _playerCountText.color = Color.black;
                _rankText.color = Color.black;
                _pingText.color = Color.black;
                _lockedImage.color = Color.black;
            }
        }

        public enum BackgroundStyle
        {
            Dark,
            Light
        }
    }
}