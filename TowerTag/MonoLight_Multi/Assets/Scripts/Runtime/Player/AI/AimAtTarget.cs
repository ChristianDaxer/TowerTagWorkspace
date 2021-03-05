using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AI {
    /// <summary>
    /// Handles positional movement and rotation of Bot head and gun for aiming at a specific target.
    /// Targets can be Claim Collider or Enemy Players.
    /// Note: all transform changes happen in local space.
    /// </summary>
    [TaskCategory("TT Bot")]
    [Serializable]
    public class AimAtTarget : LocalMovement {
        [SerializeField] private SharedFloat _currentAimingImprecision;
        [SerializeField] private SharedVector3 _target;
        [SerializeField] private bool _peek = true;


        private Transform Gun => Brain.BotWeapon;
        private AIParameters AIParameters => Brain.AIParameters;
        private float TowerHugMin => AIParameters.TowerHuggingMin;
        private float TowerHugMax => AIParameters.TowerHuggingMax;
        private Transform Muzzle => Brain.MuzzleTransform;

        private Vector3 _setpointLocalHeadPosition;
        private Vector3 _setpointLocalGunPosition;
        private Quaternion _setpointLocalHeadRotation;
        private Quaternion _setpointLocalGunRotation;

        protected override Vector3 SetPointLocalHeadPosition => _setpointLocalHeadPosition;
        protected override Vector3 SetPointLocalGunPosition => _setpointLocalGunPosition;
        protected override Quaternion SetPointLocalHeadRotation => _setpointLocalHeadRotation;
        protected override Quaternion SetPointLocalGunRotation => _setpointLocalGunRotation;

        protected override void CacheSetPointValues() {

            Vector3 targetPosition = _target.Value;
            if (_currentAimingImprecision.Value > 0) {
                float gunDistance = Vector3.Distance(_target.Value, Gun.transform.position);
                float imprecisionRadius = gunDistance * Mathf.Tan(_currentAimingImprecision.Value);
                targetPosition += imprecisionRadius * Random.insideUnitSphere;
            }

            Vector3 localTargetDirection =
                transform.InverseTransformDirection(targetPosition - Brain.BotPosition).normalized;
            Vector3 offsetDirection = Vector3.Cross(localTargetDirection, Vector3.up).normalized;
            // choose left/right side whichever is closer to current position
            int leftRight = Vector3.Angle(offsetDirection, Brain.BotHead.localPosition) > 90 ? -1 : 1;
            offsetDirection *= leftRight;

            // setpoint position
            if (_peek) {
                _setpointLocalHeadPosition = offsetDirection * Random.Range(TowerHugMin, TowerHugMax) // next to tower
                                             - TowerHugMax * localTargetDirection
                                             + Brain.StandingHeight * Vector3.up;
                _setpointLocalGunPosition = _setpointLocalHeadPosition
                                            + offsetDirection * 0.3f
                                            - 0.19f * Vector3.up;
            } else {
                _setpointLocalHeadPosition = Brain.BotHead.localPosition;
                _setpointLocalGunPosition = Brain.BotWeapon.localPosition;
            }

            // find setpoint rotation based on setpoint position and target
            Vector3 setpointWorldHeadDirection = targetPosition - transform.TransformPoint(_setpointLocalHeadPosition);
            Vector3 setpointLocalHeadDirection = transform.InverseTransformDirection(setpointWorldHeadDirection);
            Vector3 setpointWorldMuzzleDirection = targetPosition - transform.TransformPoint(_setpointLocalGunPosition);
            Quaternion setpointWorldGunRotation = Quaternion.LookRotation(setpointWorldMuzzleDirection)
                                                  * Quaternion.Inverse(Muzzle.transform.localRotation);

            // setpoint rotation
            if (_peek) {
                _setpointLocalHeadRotation =
                    Quaternion.AngleAxis(leftRight * Brain.AIParameters.HeadTiltAmount, setpointLocalHeadDirection)
                    * Quaternion.LookRotation(setpointLocalHeadDirection);
                _setpointLocalGunRotation = Quaternion.Inverse(transform.rotation) * setpointWorldGunRotation;
            } else {
                _setpointLocalHeadRotation = Quaternion.LookRotation(setpointLocalHeadDirection);
                _setpointLocalGunRotation = Quaternion.Inverse(transform.rotation) * setpointWorldGunRotation;
            }

        }
    }
}