using TowerTag;
using UI;
using UnityEngine;

namespace PopUpMenu {
    [CreateAssetMenu(menuName = "UI Elements/PopUpMenu Option/KickPlayer")]
    public class KickPlayer : PlayerOption {
        private bool _isPending;
        private IPlayer _playerToKick;
        [SerializeField] private MessageQueue _messageQueue;

        public override void OptionOnClick(IPlayer player) {
            if (player != null && !_isPending) {
                _playerToKick = player;
                _messageQueue.AddYesNoMessage(
                    "Do you really want to kick this Player?",
                    "Kick Player",
                    () => { _isPending = true; },
                    () => { _isPending = false; },
                    "YES",
                    KickPlayerFromMatch);
                _isPending = true;
            }

        }

        private void KickPlayerFromMatch(){
            if(_playerToKick.IsLoggedIn) _playerToKick.LogOut();
            _playerToKick.PlayerNetworkEventHandler.SendDisconnectPlayer("You have been kicked");
            _isPending = false;
        }
    }
}