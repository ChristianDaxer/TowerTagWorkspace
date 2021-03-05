using Hub;
using JetBrains.Annotations;
using TowerTag;
using UnityEngine;

namespace PopUpMenu {
    [CreateAssetMenu(menuName = "UI Elements/PopUpMenu Option/ResetPlayer")]
    public class ResetPlayer : PlayerOption {
        public override void OptionOnClick(IPlayer player) {
            if (player == null)
                return;

            ResetPlayerOnHubLane(player);
        }

        public static void ResetPlayerOnHubLane([NotNull] IPlayer player) {
            Pillar targetPillar = player.CurrentPillar == null
                ? PillarManager.Instance.FindSpawnPillarForPlayer(player)
                : player.CurrentPillar.gameObject.GetComponentInParent<HubLaneController>()?.SpawnPillar;

            if (targetPillar == null) {
                Debug.LogError("Could not find spawn pillar to reset player to. Disconnecting player");
                player.PlayerNetworkEventHandler.SendDisconnectPlayer("Internal error. Please try to reconnect.");
                return;
            }

            TeleportHelper.TeleportPlayerRequestedByGame(
            player, targetPillar, TeleportHelper.TeleportDurationType.Immediate);
            player.ResetPlayerHealthOnMaster();
            player.PlayerStateHandler.SetPlayerStateOnMaster(PlayerState.Alive);
        }
    }
}