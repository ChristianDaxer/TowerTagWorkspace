using System;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using Action = BehaviorDesigner.Runtime.Tasks.Action;
using Tooltip = UnityEngine.TooltipAttribute;

namespace AI {
    [TaskCategory("TT Bot")]
    [TaskDescription("Sets aiming imprecision. Reduce or reset to AI parameters default value")]
    [Serializable]
    public class SetAimImprecision : Action {
        [SerializeField] private SharedFloat _currentAimingImprecision;
        [SerializeField] private SharedFloat _defaultAimImprecision;
        [SerializeField,
             Tooltip("Aim imprecision is reduced by this divisor")] private SharedFloat _aimCorrectionFactor;
        [SerializeField] private SharedVector3 _target;

        [SerializeField,
         Tooltip("maximum threshold distance the target can move when aiming imprecision is reduced")]
        private float _maxTargetMovementDistance = 0.5f;

        [SerializeField, Tooltip("Maximum divisor by which the aim imprecision can be reduced")]
        private float _maxAimCorrectionFactor = 3;

        [SerializeField] private TargetSelectedEnemy _selectTargetEnemy; //Aim at target task

        public override TaskStatus OnUpdate() {
            if (Vector3.Distance(_selectTargetEnemy.LastTargetPosition, _target.Value) < _maxTargetMovementDistance) {
                _currentAimingImprecision.Value /= _aimCorrectionFactor.Value;
                _currentAimingImprecision.Value = Mathf.Max(
                    _currentAimingImprecision.Value, _defaultAimImprecision.Value / _maxAimCorrectionFactor);
            }

            return TaskStatus.Success;
        }
    }
}