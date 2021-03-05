using BehaviorDesigner.Runtime.Tasks;
using System;
using TowerTag;
using UnityEngine;

namespace AI {
    /// <summary>
    /// Handles Bot's ability to recognize incoming shots and pick the most dangerous one.
    /// Selection of 'Most dangerous shot' is based on closeness/distance to Bot and incoming angle of shot relative to Bot's position.
    /// </summary>
    [TaskCategory("TT Bot")]
    [Serializable]
    public class BeingShotAt : Conditional {
        [SerializeField] private SharedBotBrain _botBrain;
        [SerializeField] private float _timeThreshold;
        [SerializeField] private float _thresholdDistance;

        private IPlayer Player => _botBrain.Value.Player;
        private Vector3 Position => Player.ChargePlayer.AnchorTransform.position;

        public Shot MostDangerousShot { get; private set; }

        public override TaskStatus OnUpdate() {
            if (!_botBrain.Value.Player.PlayerHealth.IsAlive && !TTSceneManager.Instance.IsInHubScene)
                return TaskStatus.Failure;
            MostDangerousShot = null;
            float minTime = _timeThreshold;

            foreach (BotBrain.KnownShot knownShot in _botBrain.Value.KnownShots) {
                Shot shot = knownShot.Shot;
                if (shot == null)
                    return TaskStatus.Failure; // shot destroyed or player disconnected
                if (shot.TeamID == Player.TeamID || !shot.isActiveAndEnabled)
                    continue; // friendly fire or destroyed
                Vector3 shotToPlayer = Position - shot.transform.position;
                float angle = Vector3.Angle(shot.Data.Speed, shotToPlayer);
                if (angle > 90 || angle < -90)
                    continue; // flies away
                float minDistance =
                    shotToPlayer.magnitude * Mathf.Sin(angle / 180f * Mathf.PI); // shot will get this close
                if (minDistance > _thresholdDistance)
                    continue; // won't reach
                float timeToReach = shotToPlayer.magnitude * Mathf.Cos(angle / 180f * Mathf.PI)
                                    / shot.Data.Speed.magnitude;
                if (timeToReach < minTime) {
                    minTime = timeToReach;
                    MostDangerousShot = shot;
                }
            }

            if (MostDangerousShot != null && minTime < _timeThreshold)
                return TaskStatus.Success;

            return TaskStatus.Failure;
        }
    }
}