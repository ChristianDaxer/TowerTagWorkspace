using BehaviorDesigner.Runtime.Tasks;
using System;
using UnityEngine;
using Action = BehaviorDesigner.Runtime.Tasks.Action;

namespace AI {
    /// <summary>
    /// Abstract Class that implements local movement and rotation for the Bot.
    /// This includes Bot head and gun movement and rotation.
    /// </summary>
    [Serializable, TaskCategory("TT Bot")]
    public abstract class LocalMovement : Action {
        [SerializeField] private SharedBotBrain _botBrain;

        protected BotBrain Brain => _botBrain.Value;

        private float _startTime;
        private float _movementTime;
        private float _rotationTime;
        private float MovementSpeed => Brain.AIParameters.MovementSpeed;
        private float RotationSpeed => Brain.AIParameters.RotationSpeed;

        private Vector3 _startLocalHeadPosition;
        private Vector3 _startLocalGunPosition;
        private Quaternion _startLocalHeadRotation;
        private Quaternion _startLocalGunRotation;
        protected abstract Vector3 SetPointLocalHeadPosition { get; }
        protected abstract Vector3 SetPointLocalGunPosition { get; }
        protected abstract Quaternion SetPointLocalHeadRotation { get; }
        protected abstract Quaternion SetPointLocalGunRotation { get; }
        private bool _valuesInitialized;

        public override void OnStart() {
            _valuesInitialized = false;
        }

        private void InitValues() {
            _startTime = Time.time;
            _startLocalGunPosition = Brain.BotWeapon.localPosition;
            _startLocalGunRotation = Brain.BotWeapon.localRotation;
            _startLocalHeadPosition = Brain.BotHead.localPosition;
            _startLocalHeadRotation = Brain.BotHead.localRotation;

            CacheSetPointValues();

            // this is just an approximation. The movement is on a circular curve and could be pi/2 times as long
            float headDistance = Vector3.Distance(_startLocalHeadPosition, SetPointLocalHeadPosition);
            float gunDistance = Vector3.Distance(_startLocalGunPosition, SetPointLocalGunPosition);
            _movementTime = Mathf.Max(headDistance, gunDistance) / MovementSpeed;

            float headAngle = Quaternion.Angle(_startLocalHeadRotation, SetPointLocalHeadRotation);
            float gunAngle = Quaternion.Angle(_startLocalGunRotation, SetPointLocalGunRotation);
            _rotationTime = Mathf.Max(headAngle, gunAngle) / RotationSpeed;
            _valuesInitialized = true;
        }

        protected abstract void CacheSetPointValues();

        public override TaskStatus OnUpdate() {
            try {
                return Move();
            }
            catch (Exception e) {
                Debug.LogError($"Failed Bot local movement : ${e}");
                return TaskStatus.Failure;
            }
        }

        private TaskStatus Move() {
            if (Brain.Player.GunController.StateMachine.CurrentStateIdentifier ==
                GunController.GunControllerStateMachine.State.Teleport)
                return TaskStatus.Running;
            if (!_valuesInitialized)
                InitValues();
            float movementProgress = _movementTime <= 0 ? 1 : Mathf.Clamp01((Time.time - _startTime) / _movementTime);
            float rotationProgress = _rotationTime <= 0 ? 1 : Mathf.Clamp01((Time.time - _startTime) / _rotationTime);
            float moveAnim = _botBrain.Value.AnimationCurve.Evaluate(movementProgress);
            float rotateAnim = _botBrain.Value.AnimationCurve.Evaluate(rotationProgress);

            Brain.BotHead.localPosition =
                AIUtil.HorizontalSlerp(_startLocalHeadPosition, SetPointLocalHeadPosition, moveAnim);
            Brain.BotHead.localRotation =
                Quaternion.Slerp(_startLocalHeadRotation, SetPointLocalHeadRotation, rotateAnim);
            Brain.BotWeapon.localPosition =
                AIUtil.HorizontalSlerp(_startLocalGunPosition, SetPointLocalGunPosition, moveAnim);
            Brain.BotWeapon.localRotation =
                Quaternion.Slerp(_startLocalGunRotation, SetPointLocalGunRotation, rotateAnim);

            if (movementProgress < 1 || rotationProgress < 1)
                return TaskStatus.Running;
            return TaskStatus.Success;
        }
    }
}