using System;
using BehaviorDesigner.Runtime.Tasks;
using TowerTag;
using UnityEngine;

namespace AI {
    /// <summary>
    /// This Conditional evaluates if the currently chosen enemy player's health is below a given threshold.
    /// </summary>
    [TaskCategory("TT Bot")]
    [Serializable]
    public class EnemyLowHealth : Conditional {
        [SerializeField] private SharedBotBrain _botBrain;

        private IPlayer EnemyPlayer => _botBrain.Value.EnemyPlayer;

        private float MaxHealth => _botBrain.Value.Player.PlayerHealth.MaxHealth;
        private float Threshold => _botBrain.Value.AIParameters.LowHealthThreshold * MaxHealth;
        private bool FocusLowHealthEnemy => _botBrain.Value.AIParameters.FocusLowHealthEnemy;


        public override TaskStatus OnUpdate() {
            if (FocusLowHealthEnemy && EnemyPlayer != null
                                    && EnemyPlayer.PlayerHealth != null
                                    && EnemyPlayer.PlayerHealth.CurrentHealth <= Threshold)
                return TaskStatus.Success;

            return TaskStatus.Failure;
        }
    }
}