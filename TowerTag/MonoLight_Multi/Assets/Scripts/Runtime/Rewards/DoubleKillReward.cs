using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using TowerTag;
using UnityEngine;

namespace Rewards {
    [CreateAssetMenu(menuName = "Rewards/DoubleKill")]
    public class DoubleKillReward : Reward {
        private readonly Dictionary<IPlayer, int> _playerKills = new Dictionary<IPlayer, int>();

        public override bool HandleHit(ShotData shotData, IPlayer targetPlayer, Vector3 hitPoint,
            DamageDetectorBase.ColliderType colliderType) {
            if (!targetPlayer.PlayerHealth.IsAlive) {
                if (_playerKills.ContainsKey(targetPlayer)) _playerKills[targetPlayer] = 0;
                if (!_playerKills.ContainsKey(shotData.Player)) {
                    _playerKills.Add(shotData.Player,0);
                    shotData.Player.PlayerHealth.PlayerDied += OnPlayerDied;
                }

                _playerKills[shotData.Player]++;

                if (_playerKills[shotData.Player] == 2) {
                    if (PhotonNetwork.IsMasterClient && GameManager.Instance.CurrentMatch != null)
                        GameManager.Instance.CurrentMatch.Stats.AddDoubleKill(shotData.Player);
                    if(shotData.Player.IsMe)
                        return true;
                }

            }
            return false;
        }

        public override void OnPlayerDied(PlayerHealth playerHealth, IPlayer enemyWhoAppliedDamage, byte colliderType) {
            _playerKills[playerHealth.Player] = 0;
        }

        /// <summary>
        /// Resetting the kill streak for the new round
        /// </summary>
        /// <param name="match"></param>
        /// <param name="time"></param>
        public override void OnStartMatchAt(IMatch match, int time) {
            _playerKills
                .Where(kv => kv.Key != null && kv.Key.PlayerHealth != null)
                .ForEach(value => value.Key.PlayerHealth.PlayerDied -= OnPlayerDied);
            _playerKills.Clear();
        }

        public override void OnMatchFinished(IMatch match) {
            _playerKills.Where(kv => kv.Key != null && kv.Key.PlayerHealth != null)
                        .ForEach(value => value.Key.PlayerHealth.PlayerDied -= OnPlayerDied);
            _playerKills.Clear();
        }
    }
}