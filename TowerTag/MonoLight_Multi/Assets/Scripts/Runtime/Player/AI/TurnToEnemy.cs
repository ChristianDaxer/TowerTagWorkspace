using System;
using BehaviorDesigner.Runtime.Tasks;
using TowerTag;
using UnityEngine;
using Action = BehaviorDesigner.Runtime.Tasks.Action;

namespace AI {
    /// <summary>
    /// Look for source/direction of a close shot.
    /// </summary>
    /// 
    [TaskCategory("TT Bot")]
    [Serializable]
    public class TurnToEnemy : Action {
        [SerializeField] private SharedBotBrain _botBrain;
        [SerializeField] private float _rotationTime = 0.8f;
        [SerializeField] private AnimationCurve _animationCurve;

        private IPlayer Enemy => _botBrain.Value.EnemyPlayer;

        private Transform Head => _botBrain.Value.BotHead;
        private float _startTime;


        public override void OnStart() {
            _startTime = Time.time;
        }

        public override TaskStatus OnUpdate() {
            float lerpValue = (Time.time - _startTime) / _rotationTime;
            float animationValue = _animationCurve.Evaluate(lerpValue);

            // Rotate head to look into enemy direction
            GameObject enemy = Enemy.GameObject;
            if (enemy == null) return TaskStatus.Failure;
            Vector3 targetDirection = enemy.transform.position - Head.position;
            Head.rotation = Quaternion.Slerp(Head.rotation, Quaternion.LookRotation(targetDirection), animationValue);

            if (lerpValue < 1) return TaskStatus.Running;
            return TaskStatus.Success;
        }
    }
}