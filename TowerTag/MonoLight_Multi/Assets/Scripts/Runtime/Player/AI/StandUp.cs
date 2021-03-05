using BehaviorDesigner.Runtime.Tasks;
using System;
using UnityEngine;
using Action = BehaviorDesigner.Runtime.Tasks.Action;

namespace AI {
    [Serializable, TaskCategory("TT Bot")]
    public class StandUp : Action {
        [SerializeField] private SharedBotBrain _botBrain;
        [SerializeField] private float _movementTime = 0.5f;
        [SerializeField] private AnimationCurve _animationCurve;
        private float _startHeadHeight;
        private float _startGunHeight;
        private float _startTime;
        private float _setpointGunHeight;

        private float _setpointHeadHeight;
        private Transform Head => _botBrain.Value.BotHead;
        private Transform Gun => _botBrain.Value.BotWeapon;
        private AIInputController InputController => _botBrain.Value.InputController;

        public override void OnStart() {
            _startTime = Time.time;
            _startHeadHeight = Head.position.y;
            _startGunHeight = Gun.position.y;
            InputController.Release();
            if (_botBrain.Value.Player.CurrentPillar != null) {
                _setpointHeadHeight = _botBrain.Value.Player.CurrentPillar.transform.position.y
                                      + _botBrain.Value.StandingHeight;
            }

            _setpointGunHeight = _setpointHeadHeight - 0.19f;
        }

        public override TaskStatus OnUpdate() {
            float t = Mathf.Clamp01((Time.time - _startTime) / _movementTime);

            Vector3 headPosition = Head.position;
            headPosition = new Vector3(
                headPosition.x,
                Mathf.Lerp(_startHeadHeight, _setpointHeadHeight, _animationCurve.Evaluate(t)),
                headPosition.z);
            Head.position = headPosition;
            Vector3 gunPosition = Gun.position;
            gunPosition = new Vector3(
                gunPosition.x,
                Mathf.Lerp(_startGunHeight, _setpointGunHeight, _animationCurve.Evaluate(t)),
                gunPosition.z);
            Gun.position = gunPosition;

            if (t < 1)
                return TaskStatus.Running;

            return TaskStatus.Success;
        }
    }
}