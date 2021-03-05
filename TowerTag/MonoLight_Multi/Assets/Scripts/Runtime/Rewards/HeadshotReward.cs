using Photon.Pun;
using TowerTag;
using UnityEngine;
using ColliderType = DamageDetectorBase.ColliderType;

namespace Rewards {
    [CreateAssetMenu(menuName = "Rewards/Headshot")]
    public class HeadshotReward : Reward
    {
        public static int HeadshotRewardsEarned;
        public override bool HandleHit(ShotData shotData, IPlayer targetPlayer, Vector3 hitPoint,
            ColliderType colliderType) {
            if (colliderType == ColliderType.Head) {
                if (PhotonNetwork.IsMasterClient && GameManager.Instance.CurrentMatch != null)
                    GameManager.Instance.CurrentMatch.Stats.AddHeadshot(shotData.Player);
                if (!targetPlayer.PlayerHealth.IsAlive && shotData.Player.IsMe)
                {
                    HeadshotRewardsEarned++;
                    return true;
                }
            }

            return false;
        }
    }
}