using BehaviorDesigner.Runtime.Tasks;
using System;
using UnityEngine;
using Action = BehaviorDesigner.Runtime.Tasks.Action;

namespace AI {
    //reverses head tilt
    [TaskCategory("TT Bot")]
    [Serializable]
    public class UntiltHead : Action {

        [SerializeField] private SharedBotBrain _botBrain;
        [SerializeField] private float _rotationTime = 1f;

        private Transform Head => _botBrain.Value.BotHead;
        private Quaternion _targetRotation;
        private float _timePassed;
        private const float FloatTolerance = 0.01f;

        public override void OnStart() {

            _targetRotation = Quaternion.Euler(0, Head.eulerAngles.y, 0); //reset rotation on x- and z-axis of the head of the bot
            _timePassed = 0;

        }

        public override TaskStatus OnUpdate() {
            if (Head.eulerAngles.x < FloatTolerance && Head.eulerAngles.z < FloatTolerance)
                return TaskStatus.Success; //don't need to change rotation

            _timePassed += Time.deltaTime;
            float step = _timePassed / _rotationTime;
            Head.rotation = Quaternion.Slerp(Head.rotation, _targetRotation, step);

            if (_timePassed > _rotationTime) {
                return TaskStatus.Success;
            }

            return TaskStatus.Running;
        }

    }
}