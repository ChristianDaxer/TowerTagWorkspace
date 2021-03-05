using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Linq;

namespace AI {
    /// <summary>
    /// Checks whether there are only bots left in current round.
    /// Bots will behave more aggressively/choose to fight instead of hiding, when this condition is true.
    /// </summary>
    [TaskCategory("TT Bot")]
    [Serializable]
    public class OnlyBots : Conditional {

        public override TaskStatus OnUpdate() {

            PlayerManager.Instance.GetAllParticipatingHumanPlayers(out var players, out var count);
            return players.Take(count)
                .Any(player => player.PlayerHealth.IsAlive)
                ? TaskStatus.Failure
                : TaskStatus.Success;
        }
    }
}