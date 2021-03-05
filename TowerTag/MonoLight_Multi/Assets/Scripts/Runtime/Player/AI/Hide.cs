using System;
using BehaviorDesigner.Runtime.Tasks;
using TowerTag;
using UnityEngine;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using Random = UnityEngine.Random;

namespace AI {
    /// <summary>
    /// Handles positional movement and rotation of Bot head and gun for hiding from the current enemy.
    /// Bot will hide behind the pillar with respect to enemy position.
    /// Note: all transform changes happen in local space.
    /// </summary>
    [TaskCategory("TT Bot")]
    [Serializable]
    public class Hide : Action {
        [SerializeField] private SharedBotBrain _botBrain;
        [SerializeField] private float _minTime;
        [SerializeField] private float _maxTime;

        private float Speed => _botBrain.Value.AIParameters.MovementSpeed;
        private float RotationSpeed => _botBrain.Value.AIParameters.RotationSpeed;
        private Transform Head => _botBrain.Value.BotHead;
        private Transform Gun => _botBrain.Value.BotWeapon;
        private IPlayer Enemy => _botBrain.Value.EnemyPlayer;
        private float _endTime;
        private float HeadHeight => _botBrain.Value.CrouchingHeight;
        private float HeightDeviation => _botBrain.Value.HeightRange;

        private float _targetHeight;

        private const float HeadDistanceFromTower = 0.75f;
        private const float GunDistanceFromTower = 0.65f;

        public override void OnStart() {
            _endTime = Time.time + Random.Range(_minTime, _maxTime);
            _targetHeight = Random.Range(HeadHeight - HeightDeviation, HeadHeight + HeightDeviation);
        }

        public override TaskStatus OnUpdate() {
            if (Enemy == null || Enemy.ChargePlayer == null || _botBrain.Value.CurrentPillar)
                return TaskStatus.Failure;

            if (_botBrain != null && _botBrain.Value != null)
            {
                Vector3 enemyWorldDirection = Vector3.ProjectOnPlane(
                    Enemy.ChargePlayer.AnchorTransform.position - _botBrain.Value.CurrentPillar.transform.position,
                    Vector3.up);
                Vector3 enemyLocalDirection = _botBrain.Value.transform.InverseTransformDirection(enemyWorldDirection);
                Vector3 localOffsetDirection = -enemyLocalDirection.normalized;
                Vector3 setPointHeadPosition = localOffsetDirection * HeadDistanceFromTower
                                               + _targetHeight * Vector3.up;
                Vector3 setPointGunPosition = localOffsetDirection * GunDistanceFromTower
                                              + (_targetHeight - 0.19f) * Vector3.up;
                Quaternion setPointRotation = Quaternion.LookRotation(enemyLocalDirection);
                Vector3 gunLocalPosition = Gun.localPosition;
                gunLocalPosition = AIUtil.HorizontalSlerp(
                    gunLocalPosition,
                    setPointGunPosition,
                    Speed * Time.deltaTime / Vector3.Distance(gunLocalPosition, setPointGunPosition));
                Gun.localPosition = gunLocalPosition;
                Vector3 headLocalPosition = Head.localPosition;
                headLocalPosition = AIUtil.HorizontalSlerp(
                    headLocalPosition,
                    setPointHeadPosition,
                    Speed * Time.deltaTime / Vector3.Distance(headLocalPosition, setPointHeadPosition));
                Head.localPosition = headLocalPosition;
                Quaternion headLocalRotation = Head.localRotation;
                headLocalRotation = Quaternion.Slerp(
                    headLocalRotation,
                    setPointRotation,
                    RotationSpeed * Time.deltaTime / Quaternion.Angle(headLocalRotation, setPointRotation));
                Head.localRotation = headLocalRotation;
                Quaternion gunLocalRotation = Gun.localRotation;
                gunLocalRotation = Quaternion.Slerp(
                    gunLocalRotation,
                    setPointRotation,
                    RotationSpeed * Time.deltaTime / Quaternion.Angle(gunLocalRotation, setPointRotation));
                Gun.localRotation = gunLocalRotation;
            }
            // check if finished
            return Time.time < _endTime ? TaskStatus.Running : TaskStatus.Success;
        }
    }
}