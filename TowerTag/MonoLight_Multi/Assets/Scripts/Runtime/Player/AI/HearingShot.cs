using BehaviorDesigner.Runtime.Tasks;
using System;
using TowerTag;
using UnityEngine;

namespace AI {
    /// <summary>
    /// Conditional evaluates whether there is an enemy shot within the Bot's hearing range.
    /// Incoming shot has to be outside of a minimum angle, not within line of sight.
    /// </summary>
    [Serializable]
    [TaskCategory("TT Bot")]
    public class HearingShot : Conditional {
        [SerializeField] private SharedBotBrain _botBrain;

        private IPlayer Player => _botBrain.Value.Player;
        public Shot HeardShot { get; private set; }
        private float MinimumAngle => _botBrain.Value.MinIncomingSoundAngle;

        public override TaskStatus OnUpdate() {
            HeardShot = _botBrain.Value.RecentlyHeardShot;
            if ((!_botBrain.Value.Player.PlayerHealth.IsAlive && !TTSceneManager.Instance.IsInHubScene)
               || HeardShot == null || HeardShot.TeamID == Player.TeamID) {
                return TaskStatus.Failure; //friendly shot or no shot
            }

            if (Vector3.Angle(HeardShot.Data.Speed, -_botBrain.Value.BotHead.forward) < MinimumAngle)
                return TaskStatus.Failure;

            _botBrain.Value.ClearRecentlyHeardShot();
            return TaskStatus.Success;
        }
    }
}