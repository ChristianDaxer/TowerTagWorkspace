using System;
using BehaviorDesigner.Runtime.Tasks;
using TowerTag;
using UnityEngine;

namespace AI {
    /// <summary>
    /// Conditional evaluates whether Bot's health is under a given threshold.
    /// Bot will not engage in fight if low health and being shot at.
    /// </summary>
    [TaskCategory("TT Bot")]
    [Serializable]
    public class LowHealth : Conditional {
        [SerializeField] private SharedBotBrain _botBrain;

        private IPlayer Player => _botBrain.Value.Player;

        private float MaxHealth => _botBrain.Value.Player.PlayerHealth.MaxHealth;
        private float Threshold => _botBrain.Value.AIParameters.LowHealthThreshold * MaxHealth;


        public override TaskStatus OnUpdate() {
            if (Player.PlayerHealth.CurrentHealth < Threshold)
                return TaskStatus.Success;

            return TaskStatus.Failure;
        }
    }
}