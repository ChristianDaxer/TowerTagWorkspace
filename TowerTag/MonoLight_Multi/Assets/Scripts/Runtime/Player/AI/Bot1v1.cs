using BehaviorDesigner.Runtime.Tasks;
using System;
using TowerTag;

namespace AI {
    /// <summary>
    /// Checks whether there is only one bot left in each team.
    /// Bots will behave more aggressively/choose to fight instead of hiding, when this condition is true.
    /// </summary>
    [TaskCategory("TT Bot")]
    [Serializable]
    public class Bot1V1 : Conditional {

        public override TaskStatus OnUpdate() {
            var playersAlive = 0;
            var botsAlive = 0;

            PlayerManager.Instance.GetParticipatingPlayers(out var players, out var count);
            for (int i = 0; i < count; i++)
            {
                if (players[i] != null && players[i].PlayerHealth.IsAlive) {
                    playersAlive++;
                    if (playersAlive > 2)
                        return TaskStatus.Failure;
                } else
                    continue;
                if (players[i].IsBot)
                    botsAlive++;
            }

            return botsAlive == playersAlive ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}