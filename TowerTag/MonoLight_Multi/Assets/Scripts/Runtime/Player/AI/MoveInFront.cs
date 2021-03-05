using BehaviorDesigner.Runtime.Tasks;
using System;
using UnityEngine;

namespace AI {
    /// <summary>
    /// Handles positional movement and rotation of Bot head and gun and moves bot in front of it's current tower.
    /// Used in commendation scene.
    /// Note: all transform changes happen in local space.
    /// </summary>
    [TaskCategory("TT Bot")]
    [Serializable]
    public class MoveInFront : LocalMovement {

        private float HeadHeight => Brain.StandingHeight;
        private Vector3 _setpointLocalHeadPosition;
        private Vector3 _setpointLocalGunPosition;
        private Quaternion _setpointLocalRotation;

        private const float HeadDistanceFromTower = 0.75f;

        protected override Vector3 SetPointLocalHeadPosition => _setpointLocalHeadPosition;
        protected override Vector3 SetPointLocalGunPosition => _setpointLocalGunPosition;
        protected override Quaternion SetPointLocalHeadRotation => _setpointLocalRotation;
        protected override Quaternion SetPointLocalGunRotation => _setpointLocalRotation;


        protected override void CacheSetPointValues() {

            Pillar currentPillar = Brain.Player.CurrentPillar;
            Transform currentPillarTransform = currentPillar.transform;
            Vector3 targetPosition = HeadDistanceFromTower * currentPillarTransform.forward; //move in front of the tower
            _setpointLocalHeadPosition = new Vector3(targetPosition.x, HeadHeight, targetPosition.z);
            _setpointLocalGunPosition = new Vector3(targetPosition.x, HeadHeight - 0.19f, targetPosition.z);
            _setpointLocalRotation = currentPillarTransform.rotation;

        }
    }

}
