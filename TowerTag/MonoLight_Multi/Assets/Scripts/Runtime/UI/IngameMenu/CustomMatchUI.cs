using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using JetBrains.Annotations;
using Network;
using Photon.Realtime;
using TMPro;
using TowerTagSOES;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Home.UI
{
    public class CustomMatchUI : HomeMenuPanel
    {
        [SerializeField] private RectTransform _roomLineContainer;
        [SerializeField] private RoomLine _roomLinePrefab;
        [SerializeField] private Button _joinButton;
        [SerializeField] private RoomListFilter _roomListFilter;
        [SerializeField] private RoomSorter _roomSorter;
        [SerializeField] private TMP_Dropdown _regionDropdown;

        private RoomLine _selected;
        private IPhotonService _photonService;
        private Dictionary<string, RoomLine> RoomInfoList { get; set; } = new Dictionary<string, RoomLine>();

        private new void Awake()
        {
            if (!SharedControllerType.Spectator)
                base.Awake();
            _photonService = ServiceProvider.Get<IPhotonService>();
            PhotonRegionHelper.FillRegionsIntoDropdown(_regionDropdown);
        }

        [ContextMenu("Clear")]
        public void Clear()
        {
            foreach (RoomLine roomLine in RoomInfoList.Values.Where(line => line != null))
            {
                DestroyImmediate(roomLine.gameObject);
            }

            RoomInfoList.Clear();
        }

        [ContextMenu("Test Add rooms")]
        public void TestAddRooms()
        {
            OnRoomListUpdate(new List<RoomInfoUpdate>
            {
                new RoomInfoUpdate
                {
                    Name = "roomA", RemovedFromList = false, AllowTeamChange = true,
                    GameMode = "DeathMatch", HostPing = 90, Map = "Cebitus", MaxPlayers = 4, PinLocked = true,
                    Pin = StringEncoder.EncodeString("1234")
                },
                new RoomInfoUpdate
                {
                    Name = "roomA1", RemovedFromList = false, AllowTeamChange = true,
                    GameMode = "GoalTower", HostPing = 90, Map = "Elbtunnel", MaxPlayers = 8
                },
                new RoomInfoUpdate
                {
                    Name = "roomA2", RemovedFromList = false, AllowTeamChange = true,
                    GameMode = "Elimination", HostPing = 90, Map = "Cebitus", MaxPlayers = 6
                },
                new RoomInfoUpdate
                {
                    Name = "roomA3", RemovedFromList = false, AllowTeamChange = true,
                    GameMode = "DeathMatch", HostPing = 90, Map = "- RANDOM -", MaxPlayers = 4
                },
                new RoomInfoUpdate
                {
                    Name = "roomA4", RemovedFromList = false, AllowTeamChange = true,
                    GameMode = "GoalTower", HostPing = 90, Map = "Elbtunnel", MaxPlayers = 8
                },
                new RoomInfoUpdate
                {
                    Name = "roomA5", RemovedFromList = false, AllowTeamChange = true,
                    GameMode = "UserVote", HostPing = 90, Map = "Cebitus", MaxPlayers = 6
                },
                new RoomInfoUpdate
                {
                    Name = "roomA6", RemovedFromList = false, AllowTeamChange = true,
                    GameMode = "GoalTower", HostPing = 90, Map = "Elbtunnel", MaxPlayers = 2
                },
                new RoomInfoUpdate
                {
                    Name = "roomA7", RemovedFromList = false, AllowTeamChange = true,
                    GameMode = "Elimination", HostPing = 90, Map = "Everest", MaxPlayers = 2
                },
                new RoomInfoUpdate
                {
                    Name = "roomA8", RemovedFromList = false, AllowTeamChange = true,
                    GameMode = "DeathMatch", HostPing = 90, Map = "Cebitus", MaxPlayers = 4
                },
                new RoomInfoUpdate
                {
                    Name = "roomA9", RemovedFromList = false, AllowTeamChange = true,
                    GameMode = "GoalTower", HostPing = 90, Map = "Sneaky", MaxPlayers = 2
                },
                new RoomInfoUpdate
                {
                    Name = "roomA10", RemovedFromList = false, AllowTeamChange = true,
                    GameMode = "Elimination", HostPing = 90, Map = "Cebitus", MaxPlayers = 6
                },
            });
        }

        [ContextMenu("Test Remove room A")]
        public void TestRemoveRoomB()
        {
            OnRoomListUpdate(new List<RoomInfoUpdate>
            {
                new RoomInfoUpdate {Name = "roomA", RemovedFromList = true}
            });
        }

        private struct RoomInfoUpdate
        {            
            public string HostName { get; set; }
            public string Name { get; set; }
            public string GameMode { get; set; }
            public string Map { get; set; }
            public string RoomState { get; set; }
            public int PlayerCount { get; set; }
            public int MaxPlayers { get; set; }
            public int CurrentPlayers { get; set; }
            public int MinRank { get; set; }
            public int MaxRank { get; set; }
            public int HostPing { get; set; }
            public bool AllowTeamChange { get; set; }
            public bool UserVote { get; set; }
            public bool RemovedFromList { get; set; }

            public bool PinLocked { get; set; }
            public string Pin { get; set; }

            public static RoomInfoUpdate FromRoomInfo(RoomInfo roomInfo)
            {
                Hashtable properties = roomInfo.CustomProperties;
                string hostName = properties.ContainsKey(RoomPropertyKeys.HostName)
                    ? (string) properties[RoomPropertyKeys.HostName]
                    : "";
                string gameMode = properties.ContainsKey(RoomPropertyKeys.GameMode)
                    ? ((GameMode) properties[RoomPropertyKeys.GameMode]).ToString()
                    : "UserVote";
                string map = properties.ContainsKey(RoomPropertyKeys.Map)
                    ? (string) properties[RoomPropertyKeys.Map]
                    : "- RANDOM -";
                string roomState = properties.ContainsKey(RoomPropertyKeys.RoomState)
                    ? ((RoomConfiguration.RoomState) properties[RoomPropertyKeys.RoomState]).ToString()
                    : "";
                int minRank = properties.ContainsKey(RoomPropertyKeys.MinRank)
                    ? (byte) properties[RoomPropertyKeys.MinRank]
                    : 0;
                int maxRank = properties.ContainsKey(RoomPropertyKeys.MaxRank)
                    ? (byte) properties[RoomPropertyKeys.MaxRank]
                    : 0;
                int hostPing = properties.ContainsKey(RoomPropertyKeys.HostPing)
                    ? (int) properties[RoomPropertyKeys.HostPing]
                    : 0;
                int maxPlayers = properties.ContainsKey(RoomPropertyKeys.MaxPlayers)
                    ? (byte) properties[RoomPropertyKeys.MaxPlayers]
                    : roomInfo.MaxPlayers;
                int currentPlayers = properties.ContainsKey(RoomPropertyKeys.CurrentPlayers)
                    ? (byte) properties[RoomPropertyKeys.CurrentPlayers]
                    : roomInfo.PlayerCount;
                string pin = properties.ContainsKey(RoomPropertyKeys.PIN)
                    ? (string) properties[RoomPropertyKeys.PIN]
                    : "";
                bool allowTeamChange = properties.ContainsKey(RoomPropertyKeys.AllowTeamChange)
                                       && (bool) properties[RoomPropertyKeys.AllowTeamChange];
                bool userVote = properties.ContainsKey(RoomPropertyKeys.UserVote)
                                && (bool) properties[RoomPropertyKeys.UserVote];
                return new RoomInfoUpdate
                {
                    HostName = hostName,
                    Name = roomInfo.Name,
                    GameMode = gameMode,
                    RoomState = roomState,
                    Map = map,
                    PlayerCount = roomInfo.PlayerCount,
                    MaxPlayers = maxPlayers,
                    CurrentPlayers = currentPlayers,
                    MinRank = minRank,
                    MaxRank = maxRank,
                    HostPing = hostPing,
                    AllowTeamChange = allowTeamChange,
                    UserVote = userVote,
                    RemovedFromList = roomInfo.RemovedFromList,
                    PinLocked = !string.IsNullOrEmpty(pin),
                    Pin = pin
                };
            }
        }

        private void Select(RoomLine roomLine)
        {
            if (_selected != null && _selected != roomLine) _selected.Deselect();
            _selected = roomLine;
            _joinButton.interactable = true;
        }

        [UsedImplicitly]
        public void OnJoinButtonPressed()
        {
            if (_selected == null) return;
            if (_selected.Data.PinLocked)
            {
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
                JoinSelectedRoom();
        }

        private void JoinSelectedRoom()
        {
            string roomName = _selected.Data.RoomName;
            ConfigurationManager.Configuration.Room = roomName;
            ServiceProvider.Get<IPhotonService>().JoinRoom(roomName);
        }

        private void ValidatePassword(string text)
        {
            bool valid = string.Equals(StringEncoder.DecodeString(_selected.Data.Pin), text);
            if (valid)
                JoinSelectedRoom();
            else
            {
                MessageQueue.Singleton.AddErrorMessage(
                    "THE ENTERED PIN IS WRONG",
                    "ERROR");
            }
        }


        [UsedImplicitly]
        public void OnCreateButtonPressed()
        {
            UIController.SwitchPanel(HubUIController.PanelType.CreateMatch);
        }

        [UsedImplicitly]
        public void OnBackButtonPressed()
        {
            UIController.SwitchPanel(HubUIController.PanelType.MainMenu);
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            base.OnRoomListUpdate(roomList);
            OnRoomListUpdate(roomList.Select(RoomInfoUpdate.FromRoomInfo).ToList());
        }

        private void OnRoomListUpdate(List<RoomInfoUpdate> roomList)
        {
            int ping = _photonService.NetworkingClient.LoadBalancingPeer.RoundTripTime / 2;
            roomList.ForEach(roomInfo =>
            {
                var roomLineData = new RoomLine.RoomLineData
                {
                    HostName = roomInfo.HostName,
                    RoomName = roomInfo.Name,
                    GameMode = roomInfo.GameMode,
                    Map = roomInfo.Map,
                    RoomState = roomInfo.RoomState,
                    PlayerCount = roomInfo.PlayerCount,
                    MaxPlayers = roomInfo.MaxPlayers,
                    CurrentPlayers = roomInfo.CurrentPlayers,
                    MinRank = roomInfo.MinRank,
                    MaxRank = roomInfo.MaxRank,
                    Ping = roomInfo.HostPing + ping,
                    PinLocked = roomInfo.PinLocked,
                    Pin = roomInfo.Pin
                };
                if (!RoomInfoList.ContainsKey(roomInfo.Name) || RoomInfoList[roomInfo.Name] == null)
                {
                    RoomLine roomLine = _roomLinePrefab.Create(roomLineData, _roomLineContainer, Select);
                    RoomInfoList.Add(roomInfo.Name, roomLine);
                }
                else
                {
                    if (roomInfo.RemovedFromList)
                    {
                        if (RoomInfoList.ContainsKey(roomInfo.Name))
                        {
                            if (_selected != null && _selected.Data.RoomName == roomInfo.Name)
                            {
                                _selected = null;
                                _joinButton.interactable = false;
                            }
#if UNITY_EDITOR
                            DestroyImmediate(RoomInfoList[roomInfo.Name].gameObject);
#else
                            Destroy(RoomInfoList[roomInfo.Name].gameObject);
#endif
                            RoomInfoList.Remove(roomInfo.Name);
                        }
                    }
                    else
                        RoomInfoList[roomInfo.Name].UpdateRoomInfo(roomLineData);
                }
            });
            UpdateCurrentList();
        }

        public void UpdateCurrentList()
        {
            var style = RoomLine.BackgroundStyle.Light;
            int childOrder = 0;

            RoomInfoList = _roomSorter.SortRoomsByCurrentOrder(RoomInfoList);
            foreach (RoomLine roomLine in RoomInfoList.Values)
            {
                roomLine.gameObject.SetActive(_roomListFilter.IsRoomLineValidForFilterSettings(roomLine.Data));
                if (roomLine.gameObject.activeSelf)
                {
                    roomLine.transform.SetSiblingIndex(childOrder++);
                    roomLine.SetBackgroundStyle(style);
                    style = style == RoomLine.BackgroundStyle.Light
                        ? RoomLine.BackgroundStyle.Dark
                        : RoomLine.BackgroundStyle.Light;
                }
                else if (roomLine == _selected)
                {
                    _joinButton.interactable = false;
                    _selected = null;
                }
            }
        }

        [UsedImplicitly]
        public void OnRegionChanged()
        {
            Clear();
            ConnectionManagerHome.Instance.ChangeRegion(_regionDropdown.captionText.text, UIController,
                HubUIController.PanelType.FindMatch);
        }
    }
}