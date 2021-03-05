using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Linq;
using TowerTag;
using UnityEngine;
using Action = BehaviorDesigner.Runtime.Tasks.Action;

namespace AI {
    /// <summary>
    /// Tasks selects an enemy to focus on out of the known enemies in its BotBrain.
    /// Selection is based on distance to the bot and is limited by the max vision distance of the bot.
    /// </summary>
    [Serializable, TaskCategory("TT Bot")]
    public class SelectEnemy : Action {
        [SerializeField] private SharedBotBrain _botBrain;

        private float MaxVisionDistance => _botBrain.Value.VisualRange;
        private Vector3 BotPosition => _botBrain.Value.BotPosition;

        public override TaskStatus OnUpdate() {
            IPlayer[] knownEnemies = _botBrain.Value.KnownEnemies();
            if (knownEnemies.Length == 0)
                return TaskStatus.Failure;

            if (!_botBrain.Value.Player.PlayerHealth.IsAlive && !TTSceneManager.Instance.IsInHubScene)
                return TaskStatus.Failure;

            // browse known enemies for most attractive one
            IPlayer selectedEnemy = null;
            int length = knownEnemies.Length;

            for (int i = 0; i < length; i++)
            {
                if (knownEnemies[i].IsAlive && ((knownEnemies[i].ChargePlayer.AnchorTransform.position - BotPosition).sqrMagnitude) < (MaxVisionDistance * MaxVisionDistance))
                {
                    selectedEnemy = knownEnemies[i];
                    break;
                }
            }

            if (selectedEnemy == null)
                return TaskStatus.Failure;

            _botBrain.Value.EnemyPlayer = selectedEnemy;
            return TaskStatus.Success;
        }

        private float Score(IPlayer enemy) {
            float distance = Vector3.Distance(enemy.ChargePlayer.AnchorTransform.position, BotPosition);
            if (distance > MaxVisionDistance)
                return -1; // enemy is too far away

            return 1 - distance / MaxVisionDistance;
        }
    }
}