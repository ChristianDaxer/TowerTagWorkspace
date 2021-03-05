using System;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using Action = BehaviorDesigner.Runtime.Tasks.Action;

namespace AI {
    /// <summary>
    /// Selects the current Enemy to store in the Enemy SO, based on which enemy fired the most dangerous shot.
    /// </summary>
    [Serializable, TaskCategory("TT Bot")]
    public class SelectAttackingEnemy : Action {
        [SerializeField] private SharedBotBrain _botBrain;
        [SerializeField] private BeingShotAt _beingShotAt;

        public override TaskStatus OnUpdate() {
            if (_beingShotAt.MostDangerousShot == null) return TaskStatus.Failure;
            _botBrain.Value.EnemyPlayer = _beingShotAt.MostDangerousShot.Player;
            return TaskStatus.Success;
        }
    }
}