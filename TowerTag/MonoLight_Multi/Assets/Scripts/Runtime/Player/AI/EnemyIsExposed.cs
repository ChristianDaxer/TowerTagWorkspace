using System;
using BehaviorDesigner.Runtime.Tasks;
using TowerTag;
using UnityEngine;
using Tooltip = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;

namespace AI {
    /// <summary>
    /// Conditional Task that evaluates whether an enemy is in plain sight.
    /// </summary>
    [Serializable]
    [TaskCategory("TT Bot")]
    public class EnemyIsExposed : Conditional {
        [SerializeField] private SharedBotBrain _botBrain;

        [SerializeField,
         Tooltip("if enemy head distance from tower is above this value, enemy is NOT in cover")]
        private float _distanceFromTowerThreshold;

        private IPlayer SelectedEnemy => _botBrain.Value.EnemyPlayer;

        public override TaskStatus OnUpdate() {
            if (SelectedEnemy == null || SelectedEnemy.CurrentPillar == null) return TaskStatus.Failure;

            if (_botBrain.Value.PlayerIsVisible(SelectedEnemy, out RaycastHit _)) {
                Vector3 enemyHeadPosition = SelectedEnemy.PlayerAvatar.Targets.Head.position;
                Vector3 enemyHeight = enemyHeadPosition.y * Vector3.up;

                // todo project onto view plane first. Distance in view direction should not matter
                if (Vector3.Distance(enemyHeadPosition, SelectedEnemy.CurrentPillar.transform.position + enemyHeight) >
                    _distanceFromTowerThreshold) {
                    return TaskStatus.Success;
                }
            }

            return TaskStatus.Failure;
        }
    }
}