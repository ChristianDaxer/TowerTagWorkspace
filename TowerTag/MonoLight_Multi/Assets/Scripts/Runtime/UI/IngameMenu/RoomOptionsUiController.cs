using System.Collections.Generic;
using System.Linq;
using Home.UI;
using JetBrains.Annotations;
using Runtime.Player;
using TowerTag;
using UnityEngine;

namespace Runtime.UI.IngameMenu
{
    public class RoomOptionsUiController : HomeMenuPanel
    {
        [SerializeField] private RectTransform _roomLineContainer;
        [SerializeField] private RoomOptionsPlayerLine _playerLinePrefab;

        private Dictionary<string, RoomOptionsPlayerLine> PlayerList { get; set; } =
            new Dictionary<string, RoomOptionsPlayerLine>();

        public IPlayer OwnPlayer => _ownPlayer;

        private RoomOptionsManager _roomOptionsManager;
        private IPlayer _ownPlayer;

        public override void OnEnable()
        {
            base.OnEnable();
            _ownPlayer = PlayerManager.Instance.GetOwnPlayer();
            if (_ownPlayer == null)
            {
                Debug.LogError("Room options manager can't find own player.");
                return;
            }

            _roomOptionsManager = _ownPlayer.RoomOptionsManager;
            _roomOptionsManager.CurrentPlayersInRoomUpdated += OnCurrentPlayersInRoomUpdated;
            var playerList = new List<IPlayer>();
            _roomOptionsManager.CurrentPlayersInRoom.ForEach(player => playerList.Add(player.Value.Player));
            FillRoomOptionsPanelWithPlayersInRoom(playerList);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            CleanUpPanelLines();
            _roomOptionsManager.CurrentPlayersInRoomUpdated -= OnCurrentPlayersInRoomUpdated;
        }

        private void OnCurrentPlayersInRoomUpdated(object sender,
            Dictionary<string, RoomOptionsManager.PlayerOptions> playerOptions)
        {
            foreach (var player in playerOptions)
            {
                if (player.Value != null)
                    CreatePlayerLine(player.Value.Player);
            }
        }

        private void FillRoomOptionsPanelWithPlayersInRoom(List<IPlayer> playerInRoom)
        {
            foreach (var player in playerInRoom)
            {
                CreatePlayerLine(player);
            }
        }

        private void CreatePlayerLine(IPlayer player)
        {
            if (player.IsBot) return;
            if (!PlayerList.ContainsKey(player.MembershipID))
            {
                var playerLine = _playerLinePrefab.Create(_roomLineContainer, player);
                if (_roomOptionsManager == null)
                {
                    Debug.LogError("Cant find roomoptions manager");
                }

                if (_roomOptionsManager.CurrentPlayersInRoom[player.MembershipID] != null)
                {
                    playerLine.SetButtonsStates(_roomOptionsManager.CurrentPlayersInRoom[player.MembershipID].Mute);
                    playerLine.SomeRoomOptionButtonPressed += SomeButtonOnPlayerLinePressed;
                }

                PlayerList.Add(player.MembershipID, playerLine);
            }
        }

        private void SomeButtonOnPlayerLinePressed(object sender, IPlayer player,
            RoomOptionsPlayerLine.RoomOptionAction buttonAction, bool buttonStatus)
        {
            // Check player
            if (!_roomOptionsManager.CurrentPlayersInRoom.ContainsKey(player.MembershipID) ||
                !PlayerList.ContainsKey(player.MembershipID))
            {
                Debug.LogError("Can't find player in current Room.");
                return;
            }

            switch (buttonAction)
            {
                case RoomOptionsPlayerLine.RoomOptionAction.Mute:
                    _roomOptionsManager.TogglePlayerVoice(buttonStatus, player);
                    break;
                case RoomOptionsPlayerLine.RoomOptionAction.Kick:
                    break;
                case RoomOptionsPlayerLine.RoomOptionAction.Report:
                    break;
                default:
                    Debug.LogWarning("Some Error on RoomOptionsUiController");
                    break;
            }
        }

        private void CleanUpPanelLines()
        {
            foreach (var playerLine in PlayerList.ToList())
            {
                if (PlayerList.TryGetValue(playerLine.Key, out var playerLineValue))
                {
                    playerLineValue.SomeRoomOptionButtonPressed -= SomeButtonOnPlayerLinePressed;
#if UNITY_EDITOR
                    DestroyImmediate(playerLineValue.gameObject);

#else
                Destroy(playerLineValue.gameObject);
#endif
                }
            }

            PlayerList.Clear();
        }

        public void CleanUpObsoletePlayerLine(IPlayer player)
        {
            if (PlayerList != null && PlayerList.TryGetValue(player.MembershipID, out var playerLineValue))
            {
                playerLineValue.SomeRoomOptionButtonPressed -= SomeButtonOnPlayerLinePressed;
#if UNITY_EDITOR
                DestroyImmediate(playerLineValue.gameObject);
#else
                Destroy(playerLineValue.gameObject);
#endif
                PlayerList.Remove(player.MembershipID);
            }
        }

        [UsedImplicitly]
        public void OnBackButtonClicked()
        {
            UIController.SwitchPanel(HubUIController.PanelType.MainMenu);
        }

        [ContextMenu("AddTestPlayer")]
        public void AddTestPlayer()
        {
            //_roomOptionsManager.AddTestPlayer(_ownPlayer);
            CreatePlayerLine(PlayerManager.Instance.GetOwnPlayer());
        }
    }
}