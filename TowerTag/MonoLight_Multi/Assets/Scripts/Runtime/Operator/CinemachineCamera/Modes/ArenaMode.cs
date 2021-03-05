using System.Linq;
using Cinemachine;
using TowerTag;
using UnityEngine;

namespace OperatorCamera {
    public sealed class ArenaMode : CameraModeBase {

        protected override void OnPlayerGotHit(ShotData shotData, IPlayer targetPlayer, Vector3 hitPoint, DamageDetectorBase.ColliderType targetType) {
        }

        protected override void Activate() {
            if (TTSceneManager.Instance.IsInCommendationsScene || TTSceneManager.Instance.IsInHubScene) return;

            _virtualCamera.enabled = true;
            CameraManager.Instance.TargetGroupManager.UpdatePlayerAndTargets();
            PlayerToFocus = null;
            HardFocusOnPlayer = false;
        }

        protected override void Deactivate() {
            _virtualCamera.enabled = false;
        }

        protected override void OnMatchHasFinishedLoading(IMatch match) {
            CinemachineSmoothPath smoothPath = FindObjectsOfType<CinemachineSmoothPath>()?
                .FirstOrDefault(dollyTrack => dollyTrack.gameObject.CompareTag("ArenaDollyTrack"));
            if (smoothPath != null) {
                _virtualCamera.GetCinemachineComponent<CinemachineTrackedDolly>().m_Path = smoothPath;
            }
            else {
                Debug.LogWarning("No DollyTrack for intro scene found!");
            }
        }
    }
}