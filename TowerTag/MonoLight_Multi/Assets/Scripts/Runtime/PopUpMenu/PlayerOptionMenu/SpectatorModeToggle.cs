using TowerTag;
using UnityEngine;

namespace PopUpMenu {
    [CreateAssetMenu(menuName = "UI Elements/PopUpMenu Option/SpectatorModeToggle")]
    public class SpectatorModeToggle : PlayerOption {
        [SerializeField] private string _textWhenActive = "Disable Player";
        [SerializeField] private string _textWhenInactive = "Enable Player";

        public override void OptionOnClick(IPlayer player) {
            if (player != null) {
                player.IsParticipating = !player.IsParticipating;
                if(!player.IsParticipating)
                    ResetPlayer.ResetPlayerOnHubLane(player);
            }
        }

        public override void UpdateButtonText(IPlayer player) {
            ButtonText = player.IsParticipating ? _textWhenActive : _textWhenInactive;
        }
    }
}