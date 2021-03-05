using System;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AI {
    /// <summary>
    /// Action task to look around by rotating a certain angle.
    /// </summary>
    /// <author>Ole Jürgensen (ole@vrnerds.de)</author>
    [TaskCategory("TT Bot")]
    [Serializable]
    public class LookAround : LocalMovement {
        [SerializeField] private float _maximumAnglePerStep;
        [SerializeField] private float _minDistanceFromTower = 0.55f;
        [SerializeField] private float _maxDistanceFromTower = 0.65f;

        private Vector3 _setpointLocalHeadPosition;
        private Vector3 _setpointLocalGunPosition;
        private Quaternion _setpointLocalHeadRotation;
        private Quaternion _setpointLocalGunRotation;
        private Transform Muzzle => Brain.MuzzleTransform;

        protected override Vector3 SetPointLocalHeadPosition => _setpointLocalHeadPosition;
        protected override Vector3 SetPointLocalGunPosition => _setpointLocalGunPosition;
        protected override Quaternion SetPointLocalHeadRotation => _setpointLocalHeadRotation;
        protected override Quaternion SetPointLocalGunRotation => _setpointLocalGunRotation;

        protected override void CacheSetPointValues() {
            float distanceFromTower = Random.Range(_minDistanceFromTower, _maxDistanceFromTower);
            float angle = Random.Range(-_maximumAnglePerStep, _maximumAnglePerStep);
            Vector3 floorPosition = Vector3.ProjectOnPlane(Brain.BotHead.localPosition, Vector3.up);
            floorPosition = floorPosition == Vector3.zero ? Vector3.forward : floorPosition;
            _setpointLocalHeadPosition = Quaternion.AngleAxis(angle, Vector3.up)
                                         * floorPosition.normalized * distanceFromTower
                                         + Brain.StandingHeight * Vector3.up;
             _setpointLocalHeadRotation = Quaternion.LookRotation(floorPosition);
            _setpointLocalGunRotation = Quaternion.LookRotation(floorPosition) * Quaternion.Inverse(Muzzle.transform.localRotation);//hold gun parallel to ground
            _setpointLocalGunPosition = _setpointLocalHeadPosition - 0.19f * Vector3.up
                                        + floorPosition.normalized * 0.3f
                                        + Vector3.Cross(_setpointLocalHeadPosition, Vector3.up).normalized * 0.3f;
        }
    }
}