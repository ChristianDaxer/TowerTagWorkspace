using System;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AI {
    [TaskCategory("TT Bot")]
    [Serializable]
    public class TakeCover : LocalMovement {
        [SerializeField] private BeingShotAt _beingShotAt;
        private float HeadHeight => Brain.CrouchingHeight;
        private float HeightDeviation => Brain.HeightRange;

        private Vector3 _setPointLocalHeadPosition;
        private Vector3 _setPointLocalGunPosition;
        private Quaternion _setPointLocalRotation;

        private const float HeadDistanceFromTower = 0.75f;
        private const float GunDistanceFromTower = 0.65f;

        protected override Vector3 SetPointLocalHeadPosition => _setPointLocalHeadPosition;
        protected override Vector3 SetPointLocalGunPosition => _setPointLocalGunPosition;
        protected override Quaternion SetPointLocalHeadRotation => _setPointLocalRotation;
        protected override Quaternion SetPointLocalGunRotation => _setPointLocalRotation;

        protected override void CacheSetPointValues() {
            Vector3 shotSpawnPosition = _beingShotAt.MostDangerousShot.SpawnPosition;
            float targetHeight = Random.Range(HeadHeight - HeightDeviation, HeadHeight + HeightDeviation);
            Vector3 shotDirection = transform.InverseTransformDirection(shotSpawnPosition - Brain.BotHead.position);
            Vector3 offsetDirection = Vector3.ProjectOnPlane(-shotDirection, Vector3.up).normalized;
            _setPointLocalHeadPosition = offsetDirection * HeadDistanceFromTower + targetHeight * Vector3.up;
            _setPointLocalGunPosition = offsetDirection * GunDistanceFromTower + (targetHeight - 0.19f) * Vector3.up;
            _setPointLocalRotation = Quaternion.LookRotation(shotDirection);
        }
    }
}