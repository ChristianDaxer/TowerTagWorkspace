using BehaviorDesigner.Runtime.Tasks;
using System;
using UnityEngine;

namespace AI {
    /// <summary>
    /// Returns success if in commendation scene.
    /// </summary>
    [TaskCategory("TT Bot")]
    [Serializable]
    public class InCommendationScene : Conditional {
        [SerializeField] private SharedBotBrain _botBrain;
        private Animator BotAnimator => _botBrain.Value.BotAnimator;

        public override TaskStatus OnUpdate() {
            if (TTSceneManager.Instance.IsInCommendationsScene && !BotAnimator.enabled)
                return TaskStatus.Success;
            else if (TTSceneManager.Instance.IsInCommendationsScene && BotAnimator.enabled)
                return TaskStatus.Running;


            return TaskStatus.Failure;


        }
    }
}

