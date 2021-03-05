using System;
using AI;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable, TaskCategory("TT Bot")]
public class ShouldEngage : Conditional {
    [SerializeField] private SharedBotBrain _botBrain;
    private Vector3 BotPosition => _botBrain.Value.BotPosition;
    [SerializeField] private AnimationCurve _engageProbability;

    public override TaskStatus OnUpdate() {
        if (_botBrain.Value.EnemyPlayer == null || _botBrain.Value.EnemyPlayer.PlayerAvatar == null)
            return TaskStatus.Failure;

        float enemyDistance = Vector3.Distance(_botBrain.Value.EnemyPlayer.PlayerAvatar.Targets.Head.position,
            BotPosition);
        return Random.value < _engageProbability.Evaluate(enemyDistance / _botBrain.Value.VisualRange)
            ? TaskStatus.Success
            : TaskStatus.Failure;
    }
}