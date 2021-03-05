using System.Collections.Generic;
using Photon.Pun;
using Runtime.UI.IngameMenu;
using TowerTag;
using UnityEngine;

namespace Runtime.Player {
    public class RoomOptionsManager : MonoBehaviourPunCallbacks {
        public class PlayerOptions {
            private IPlayer _player;

            public IPlayer Player {
                get => _player;
                set => _player = value;
            }

            private bool _mute;

            public bool Mute {
                get => _mute;
                set => _mute = value;
            }
            
            
        }

        public delegate void RoomOptionsManagerAction(object sender,
            Dictionary<string, PlayerOptions> currentPlayersInRoom);

        public event RoomOptionsManagerAction CurrentPlayersInRoomUpdated;

        private readonly Dictionary<string, PlayerOptions> _currentPlayersInRoom = new Dictionary<string, PlayerOptions>();
        public Dictionary<string, PlayerOptions> CurrentPlayersInRoom => _currentPlayersInRoom;

        public IPlayer Owner => _roomOptionsUiController.OwnPlayer;
        [SerializeField] private RoomOptionsUiController _roomOptionsUiController;

        public override void OnEnable() {
            base.OnEnable();
            PlayerManager.Instance.PlayerAdded += OnPlayerAdded;
            PlayerManager.Instance.PlayerRemoved += OnPlayerRemoved;
        }

        public override void OnDisable() {
            base.OnDisable();
            PlayerManager.Instance.PlayerAdded -= OnPlayerAdded;
            PlayerManager.Instance.PlayerRemoved -= OnPlayerRemoved;
            ClearCurrentPlayerList();
        }

        private void OnPlayerAdded(IPlayer player) {
            if (player == PlayerManager.Instance.GetOwnPlayer() || player.IsBot)
                return;
            if (AddPlayerToRoomList(player))
                CurrentPlayersInRoomUpdated?.Invoke(this, CurrentPlayersInRoom);
        }

        private void OnPlayerRemoved(IPlayer player) {
            if (player == PlayerManager.Instance.GetOwnPlayer())
                return;
            if (RemovePlayerFromRoomList(player))
                _roomOptionsUiController.CleanUpObsoletePlayerLine(player);
        }

        private bool AddPlayerToRoomList(IPlayer player)
        {
            if (player.IsBot) return false;
            if (!_currentPlayersInRoom.ContainsKey(player.MembershipID)) {
                _currentPlayersInRoom.Add(player.MembershipID, new PlayerOptions {
                        Player = player,
                        Mute = false
                    }
                );
                CurrentPlayersInRoomUpdated?.Invoke(this, _currentPlayersInRoom);
                return true;
            }
            else
                Debug.LogWarning(
                    "RoomOptionsManager registered player already to current room list or player is null.");

            return false;
        }

        private bool RemovePlayerFromRoomList(IPlayer player) {
            if (player.IsBot)
                return false;

            if (string.IsNullOrEmpty(player.MembershipID)) {
                Debug.LogException(new System.Exception($"{nameof(IPlayer)} {player.PlayerName} membership ID is invalid."));
                return false;
            }

            if (player != null && _currentPlayersInRoom.ContainsKey(player.MembershipID)) {
                _currentPlayersInRoom.Remove(player.MembershipID);
                return true;
            }

            else Debug.LogWarning("RoomOptionsManager can't find IPlayer to remove.");

            return false;
        }

        private void ClearCurrentPlayerList() {
            _currentPlayersInRoom.Clear();
        }

        public void TogglePlayerVoice(bool status, IPlayer player) {
            // check if current players in room list contains the given player
            if (!CurrentPlayersInRoom.ContainsKey(player.MembershipID) ||
                CurrentPlayersInRoom[player.MembershipID].Player == null) {
                Debug.LogError("Can't find player id in current room list");
                return;
            }

            // mute / unmute single player 

            CurrentPlayersInRoom[player.MembershipID].Mute = status;

            if (player.GameObject != null) {
                var speakerManager = player.GameObject.GetComponent<SpeakerManager>();

                if (!status)
                    speakerManager.ActivatePlayerSpeaker();
                else
                    speakerManager.DeactivatePlayerSpeaker();
            }
            else {
                Debug.LogWarning("Can't find remote Player Object for speaker toggle.");
            }
        }

        public void ToggleVoteKick(bool status, IPlayer player)
        {
            if (!CurrentPlayersInRoom.ContainsKey(player.MembershipID) ||
                CurrentPlayersInRoom[player.MembershipID].Player == null) {
                Debug.LogError("Can't find player id in current room list");
                return;
            }
        }

        public void AddTestPlayer(IPlayer player) {
            if (AddPlayerToRoomList(player))
                CurrentPlayersInRoomUpdated?.Invoke(this, CurrentPlayersInRoom);
        }
    }
}