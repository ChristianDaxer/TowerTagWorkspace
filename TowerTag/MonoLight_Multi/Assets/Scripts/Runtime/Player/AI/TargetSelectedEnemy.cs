using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using TowerTag;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AI {
    /// <summary>
    /// Task to choose a target point attacking the currently selected enemy.
    /// Prefers direct hits at the opponent, followed by shooting down walls and suppressing fire left and right.
    /// As for the body part, uses the priorities defined by <see cref="BotBrain"/>.
    /// </summary>
    /// <author>Ole Jürgensen (ole@vrnerds.de)</author>
    [Serializable, TaskCategory("TT Bot")]
    public class TargetSelectedEnemy : Conditional {
        [SerializeField] private SharedBotBrain _botBrain;
        [SerializeField] private SharedVector3 _target;
        [SerializeField] private SharedBool _newBurst;

        private IPlayer SelectedEnemy => _botBrain.Value.EnemyPlayer;
        private bool SuppressingFire => _botBrain.Value.AIParameters.SuppressingFire;
        private bool ShootDownWalls => _botBrain.Value.AIParameters.ShootDownWalls;
        private float MinSuppressingFireOffset => _botBrain.Value.MinSuppressingFireOffset;
        private float MaxSuppressingFireOffset => _botBrain.Value.MaxSuppressingFireOffset;
        public Vector3 LastTargetPosition { get; private set; } //last target position

        public override TaskStatus OnUpdate() {
            LastTargetPosition = _target.Value;

            // don't shoot at dead or disconnected players
            if (SelectedEnemy == null || SelectedEnemy.PlayerHealth == null || !SelectedEnemy.PlayerHealth.IsAlive)
                return TaskStatus.Failure;

            // shoot at enemy directly
            if (_botBrain.Value.PlayerIsVisible(SelectedEnemy, out RaycastHit raycastHit)) {
                _target.SetValue(raycastHit.point);
                return TaskStatus.Success;
            }

            // find obstacle
            Vector3 guessedPosition = SelectedEnemy.PlayerAvatar.Targets.Body.position;
            _botBrain.Value.PlayerIsVisibleAt(SelectedEnemy, guessedPosition, out RaycastHit hitInfo);

            // shoot down walls, only if they are not busted
            if (ShootDownWalls && hitInfo.collider != null
            && hitInfo.collider.GetComponent<PillarWall>() != null
            && hitInfo.collider.GetComponentInParent<Pillar>()?.Owner == SelectedEnemy
            && !hitInfo.collider.GetComponent<WallViewRigidBody>().IsDown) {
                _target.SetValue(hitInfo.collider.transform.position + 1.2f * Vector3.up); //add offset to target position, since position is at wall base
                return TaskStatus.Success;
            }

            // suppressing fire (shoot left and right of tower)
            if (SuppressingFire
                              //&& (hitInfo.collider != null  //if suppressing fire is selected in AI Parameters SO
                              //&& hitInfo.collider.GetComponentInParent<Pillar>()?.Owner == SelectedEnemy)
                              //before, we used guessed position for the visibility check, but pillar was not always raycasted
                              //now we just check if the enemy's pillar is visible to the bot
                              && _botBrain.Value.PlayerPillarIsVisible(SelectedEnemy)) {
                if (_newBurst.Value) {
                    Vector3 targetDirection = guessedPosition - _botBrain.Value.BotPosition;
                    Vector3 offsetDirection = Vector3.Cross(targetDirection, Vector3.up).normalized;
                    Vector3 offset = Random.Range(MinSuppressingFireOffset, MaxSuppressingFireOffset)
                                 * AIUtil.RandomSign()
                                 * offsetDirection;


                    _target.SetValue(guessedPosition + offset);//Body Collider Position with lateral offset
                }

                return TaskStatus.Success;
            }

            return TaskStatus.Failure;
        }


    }
}