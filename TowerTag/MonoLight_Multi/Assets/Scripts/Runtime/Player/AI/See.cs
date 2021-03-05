using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections;
using System.Linq;
using TowerTag;
using UnityEngine;
using Action = BehaviorDesigner.Runtime.Tasks.Action;

namespace AI {
    /// <summary>
    /// Simple task to handle the visual input of the bot.
    /// This Task never stops on its own.
    /// </summary>
    /// <author>Ole Jürgensen (ole@vrnerds.de)</author>
    [Serializable]
    [TaskCategory("TT Bot")]
    public class See : Action {

        [SerializeField] private SharedBotBrain _sharedBotBrain;

        private BotBrain BotBrain => _sharedBotBrain.Value;
        private ShotManager ShotManager => _sharedBotBrain.Value.ShotManager;

        public override TaskStatus OnUpdate() {
            FindPlayers();
            return TaskStatus.Running;
        }

        /// <summary>
        /// Finds currently visible Players and fired shots. Body, Head and Gun visibility all count towards player visibility.
        /// If bot recognizes occupied enemy pillars in its neighbourhood, enemies on these pillars are 'seen' by the bot
        /// </summary>
        private void FindPlayers() {
            PlayerManager.Instance.GetParticipatingPlayers(out var players, out var count);
            for (int i = 0; i < count; i++)
            {
                if (players[i].TeamID != BotBrain.Player.TeamID
                    && players[i].PlayerHealth.IsAlive
                    && BotBrain.PlayerIsVisible(players[i])) {
                    StartCoroutine(SeePlayer(players[i]));
                }
            }

            foreach (Shot shot in ShotManager.Shots) {
                if (shot != null && BotBrain.ShotIsVisible(shot))
                    StartCoroutine(SeeShot(shot));
            }

            if (BotBrain.AIParameters.RecognizeOccupiedPillars && BotBrain.Player.CurrentPillar != null) {
                //currently: bot knows about enemies on occupied neighboring pillars, might be too powerful
                //TODO: check if pillar is in bot's line of sight
                Pillar[] pillars = PillarManager.Instance.GetNeighboursByPlayer(BotBrain.Player);
                for (int i = 0; i < pillars.Length; i++)
                {
                    if (!pillars[i].IsOccupied || pillars[i].OwningTeamID == BotBrain.Player.TeamID)
                        continue;
                    IPlayer enemy = pillars[i].Owner;
                    if (!enemy.PlayerHealth.IsAlive)
                        continue;
                    StartCoroutine(SeePlayer(enemy));
                }
                /*
                PillarManager.Instance.GetNeighboursByPlayer(BotBrain.Player)
                    .Where(pillar => pillar.IsOccupied && pillar.OwningTeamID != BotBrain.Player.TeamID)
                    .Select(pillar => pillar.Owner)
                    .Where(enemy => enemy.PlayerHealth.IsAlive)
                    .ForEach(enemy => StartCoroutine(SeePlayer(enemy)));
                */
            }
        }

        private WaitForSeconds SeePlayerReaction => seePlayerReaction == null ? seePlayerReaction = new WaitForSeconds(BotBrain.AIParameters.ReactionTime) : seePlayerReaction;
        private WaitForSeconds seePlayerReaction;
        private WaitForSeconds SeeShotReaction => seeShotReaction == null ? seeShotReaction = new WaitForSeconds(BotBrain.AIParameters.ReactionTime) : seeShotReaction;
        private WaitForSeconds seeShotReaction;

        private IEnumerator SeePlayer(IPlayer player) {
            yield return SeePlayerReaction;
            if (player != null)
               BotBrain.SeePlayer(player);
        }

        private IEnumerator SeeShot(Shot shot) {
            yield return SeeShotReaction;
            if (shot != null)
                BotBrain.SeeShot(shot);
        }
    }
}