using BehaviorDesigner.Runtime.Tasks;
using System;
using UnityEngine;


namespace AI {
    /// <summary>
    /// Checks if the BotBrain, LocalPlayer and CurrentPillar for the current Bot Client are initialized and if Bot is alive.
    /// Behaviour tree won't further evaluate if Bot is not alive (except for hub scene).
    /// </summary>
    [Serializable, TaskCategory("TT Bot")]
    public class IsReady : Conditional {
        [SerializeField] private SharedBotBrain _botBrain;
        private Animator BotAnimator => _botBrain.Value.BotAnimator;

        public override TaskStatus OnUpdate() {


            if (TTSceneManager.Instance.IsInHubScene && BotAnimator.enabled) {
                BotAnimator.Rebind();
                BotAnimator.enabled = false;

            }

            return _botBrain != null
                   && _botBrain.Value != null
                   && _botBrain.Value.Player != null
                   && _botBrain.Value.Player.CurrentPillar != null
                ? TaskStatus.Success
                : TaskStatus.Failure;
        }
    }
}