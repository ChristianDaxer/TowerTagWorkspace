using System;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace AI {
    /// <summary>
    /// Triggers respective Animation according to whether the team of the bot has won or lost the match
    /// </summary>
    [TaskCategory("TT Bot")]
    [Serializable]
    public class TriggerBotAnimation : BehaviorDesigner.Runtime.Tasks.Action {
        [SerializeField] private SharedBotBrain _botBrain;
        private static readonly int _win = Animator.StringToHash("Win");
        private static readonly int _draw = Animator.StringToHash("Draw");
        private static readonly int _loss = Animator.StringToHash("Loss");
        private Animator BotAnimator => _botBrain.Value.BotAnimator;

        public override void OnStart() {
            if (TTSceneManager.Instance.IsInCommendationsScene && !BotAnimator.enabled
            ) //Animator is not already activated
            {
                BotAnimator.enabled = true;
            }
        }

        public override TaskStatus OnUpdate() {
            MatchStats currentMatchStats = GameManager.Instance.CurrentMatch.Stats;
            if (currentMatchStats != null &&
                currentMatchStats.WinningTeamID == _botBrain.Value.Player.TeamID) {
                BotAnimator.SetTrigger(_win);
                //Debug.Log("### WIN");
                return TaskStatus.Success;
            }

            if (currentMatchStats != null && currentMatchStats.Draw) {
                BotAnimator.SetTrigger(_draw);
                //Debug.Log("### DRAW");
                return TaskStatus.Success;
            }

            if (currentMatchStats != null) {
                BotAnimator.SetTrigger(_loss);
                //Debug.Log("### LOSS");
                return TaskStatus.Success;
            }

            return TaskStatus.Running;
        }
    }
}