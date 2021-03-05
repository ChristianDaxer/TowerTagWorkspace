using System;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AI {
    [Serializable, TaskCategory("TT Bot")]
    public class Peek : LocalMovement {
        private float HeadHeight => Brain.StandingHeight;
        private const float HeadDistanceFromTower = 0.7f;
        private Vector3 _setpointLocalHeadPosition;
        private Quaternion _setpointLocalRotation;

        protected override Vector3 SetPointLocalHeadPosition => _setpointLocalHeadPosition;
        protected override Vector3 SetPointLocalGunPosition => _setpointLocalHeadPosition - 0.1f * Vector3.up;
        protected override Quaternion SetPointLocalHeadRotation => _setpointLocalRotation;
        protected override Quaternion SetPointLocalGunRotation => _setpointLocalRotation;

        protected override void CacheSetPointValues() {
            int leftRight = AIUtil.RandomSign();
            if (Brain.EnemyPlayer.ChargePlayer.AnchorTransform == null) return;
            Vector3 enemyWorldPosition = Brain.EnemyPlayer.ChargePlayer.AnchorTransform.position;
            Vector3 enemyDirection = transform.InverseTransformDirection(enemyWorldPosition - Brain.BotHead.position);
            Vector3 hidePosition = -Vector3.ProjectOnPlane(enemyDirection, Vector3.up).normalized *
                                   HeadDistanceFromTower + Vector3.up * HeadHeight;
            Vector3 offsetDirection = Vector3.Cross(enemyDirection, Vector3.up).normalized;
            _setpointLocalHeadPosition = hidePosition + Random.Range(
                                             Brain.AIParameters.TowerHuggingMin,
                                             Brain.AIParameters.TowerHuggingMax) * leftRight * offsetDirection;
            _setpointLocalRotation = Quaternion.AngleAxis(leftRight * Brain.AIParameters.HeadTiltAmount, enemyDirection)
                                     * Quaternion.LookRotation(enemyDirection);
        }
    }
}