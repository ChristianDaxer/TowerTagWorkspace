using System;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using Action = BehaviorDesigner.Runtime.Tasks.Action;

namespace AI {
    /// <summary>
    /// Initializes Global Shared Variables used between multiple Tasks.
    /// The values are taken from the AI Parameter SO.
    /// </summary>
    [Serializable, TaskCategory("TT Bot")]
    public class InitializeVariables : Action {
        [SerializeField] private SharedBotBrain _botBrain;
        [SerializeField] private SharedFloat _shootChance, _jumpChance, _counterFireChance,
            _hideChance, _fleeUnderFireChance, _defaultAimImprecision, _aimCorrectionFactor;
        [SerializeField] private SharedInt _minShootFrequency, _maxShootFrequency;

        public override void OnStart() {

            //Debug.Log("#### BOT Parameters updated");
            _shootChance.SetValue(_botBrain.Value.AIParameters.ShootChance);
            _jumpChance.SetValue(_botBrain.Value.AIParameters.JumpChance);
            _counterFireChance.SetValue(_botBrain.Value.AIParameters.CounterFireChance);
            _hideChance.SetValue(_botBrain.Value.AIParameters.HideChance);
            _fleeUnderFireChance.SetValue(_botBrain.Value.AIParameters.FleeUnderFireChance);
            _defaultAimImprecision.SetValue(_botBrain.Value.AIParameters.AimImprecision);
            _aimCorrectionFactor.SetValue(_botBrain.Value.AIParameters.AimCorrectionFactor);
            _minShootFrequency.SetValue(_botBrain.Value.AIParameters.MinShootFrequency);
            _maxShootFrequency.SetValue(_botBrain.Value.AIParameters.MaxShootFrequency);
        }
    }
}