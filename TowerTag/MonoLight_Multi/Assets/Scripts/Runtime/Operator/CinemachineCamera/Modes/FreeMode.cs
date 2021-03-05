using Cinemachine;
using TowerTag;
using UnityEngine;

namespace OperatorCamera {
    public class FreeMode : CameraModeBase {
        [Space] [Header("Mode Specific Components")] [SerializeField]
        private MoveFreeCam _moveCamObject;
        protected override void OnPlayerGotHit(ShotData shotData, IPlayer targetPlayer, Vector3 hitPoint, DamageDetectorBase.ColliderType targetType) {
        }

        protected override void Activate() {
            if (Camera.main != null) {
                GameObject o = Camera.main.gameObject;
                Transform objTransform = _moveCamObject.transform;
                objTransform.position = o.transform.position;
                var cinemachineComponent = _virtualCamera.GetCinemachineComponent<CinemachinePOV>();
                Vector3 eulerAngles = o.transform.eulerAngles;
                _moveCamObject.enabled = true;
                _virtualCamera.enabled = true;
                cinemachineComponent.m_HorizontalAxis.Value = eulerAngles.y;
                cinemachineComponent.m_VerticalAxis.Value = eulerAngles.x;
            }
        }

        protected override void Deactivate() {
            _moveCamObject.enabled = false;
            _virtualCamera.enabled = false;
        }
    }
}