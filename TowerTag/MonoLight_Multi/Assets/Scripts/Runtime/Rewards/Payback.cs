using TowerTag;
using UnityEngine;

namespace Rewards {
    [CreateAssetMenu(menuName = "Rewards/Payback")]
    public class Payback : Reward {
        private IPlayer _killedBy;
        private bool _playerSurvivedCurrentRound = true;

        public override bool HandleHit(ShotData shotData, IPlayer targetPlayer, Vector3 hitPoint, DamageDetectorBase.ColliderType colliderType) {

            //When the player I killed killed me in last round
            if (_killedBy == targetPlayer && !targetPlayer.PlayerHealth.IsAlive && shotData.Player.IsMe)
            {
                _killedBy = null;
                return true;
            }

            if (targetPlayer.IsMe && !targetPlayer.PlayerHealth.IsAlive) {
                _killedBy = shotData.Player;
                _playerSurvivedCurrentRound = false;
            }

            return false;
        }

        public override void OnStartMatchAt(IMatch match, int time) {
            if(_playerSurvivedCurrentRound)
                _killedBy = null;
            _playerSurvivedCurrentRound = true;
        }

        public override void OnMatchFinished(IMatch match)
        {
            _playerSurvivedCurrentRound = true;
            _killedBy = null;
        }
    }
}