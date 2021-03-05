using System;
using Photon.Pun;
using TowerTag;
using UnityEngine;
using ColliderType = DamageDetectorBase.ColliderType;

namespace Rewards {
    [CreateAssetMenu(menuName = "Rewards/SnipeShot")]
    public class SnipeReward : Reward {
        [SerializeField] private float _minDistanceForReward;
        public static event Action SnipeShotHit;
        public override bool HandleHit(ShotData shotData, IPlayer targetPlayer, Vector3 hitPoint,
            ColliderType colliderType) {
            float shotDistance = Vector3.Distance(hitPoint, shotData.SpawnPosition);
            if (shotDistance > _minDistanceForReward) {
                if (PhotonNetwork.IsMasterClient && GameManager.Instance.CurrentMatch != null)
                    GameManager.Instance.CurrentMatch.Stats.AddSniperKill(shotData.Player);
                if(shotData.Player.IsMe) SnipeShotHit?.Invoke();
                if (!targetPlayer.PlayerHealth.IsAlive && shotData.Player.IsMe) {
                    return true;
                }
            }

            return false;
        }
    }
}