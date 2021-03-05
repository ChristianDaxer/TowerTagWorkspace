using BehaviorDesigner.Runtime.Tasks;
using System;
using UnityEngine;

namespace AI {
    /// <summary>
    /// Look for source/direction of a close shot.
    /// </summary>
    /// 
    [TaskCategory("TT Bot")]
    [Serializable]
    public class TurnToShotSource : LocalMovement {
        [SerializeField] private HearingShot _hearingShot;

        private Vector3 _setPointLocalHeadPosition;
        private Vector3 _setPointLocalGunPosition;
        private Quaternion _setPointLocalHeadRotation;
        private Quaternion _setPointLocalGunRotation;
        private Transform Muzzle => Brain.MuzzleTransform;

        protected override Vector3 SetPointLocalHeadPosition => _setPointLocalHeadPosition;
        protected override Vector3 SetPointLocalGunPosition => _setPointLocalGunPosition;
        protected override Quaternion SetPointLocalHeadRotation => _setPointLocalHeadRotation;
        protected override Quaternion SetPointLocalGunRotation => _setPointLocalGunRotation;

        protected override void CacheSetPointValues() {
            Vector3 shotSource = _hearingShot.HeardShot.SpawnPosition;
            Vector3 worldDirection = shotSource - Brain.BotHead.position;
            Vector3 localDirection = Brain.transform.InverseTransformDirection(worldDirection);
            _setPointLocalHeadPosition = Brain.BotHead.localPosition;
            _setPointLocalGunPosition = _setPointLocalHeadPosition - 0.19f * Vector3.up //gun height
                                        + _setPointLocalHeadPosition.normalized * 0.1f //distance from body
                                        + Vector3.Cross(_setPointLocalHeadPosition, Vector3.up).normalized *
                                        0.3f; //lateral distance from body
            _setPointLocalHeadRotation = Quaternion.LookRotation(localDirection);
            _setPointLocalGunRotation = Quaternion.LookRotation(localDirection) *
                                        Quaternion.Inverse(Muzzle.transform
                                            .localRotation); //hold gun parallel to ground
        }
    }
}