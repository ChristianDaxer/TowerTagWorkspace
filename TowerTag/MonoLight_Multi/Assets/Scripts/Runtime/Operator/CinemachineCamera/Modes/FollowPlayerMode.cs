using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using TowerTag;
using static OperatorCamera.CameraManager;

namespace OperatorCamera {
    public class FollowPlayerMode : CameraModeBase {
        [Space] [Header("Mode Specific Components")] [SerializeField]
        private GameObject _followPlayerCamManagerPrefab;
        [SerializeField] private AutomaticMode _automaticMode;

        private readonly float _waitingForChangeAfterFocusDied = 2f;

        //List of all existing player follow cameras
        private readonly List<FollowPlayerCamManager> _followPlayerCams = new List<FollowPlayerCamManager>();
        private FollowPlayerCamManager _currentActiveFollowCam;

        private void Update() {
            if (!IsActive || HardFocusOnPlayer || PlayerToFocus == null) return;
            if (TimeSinceLastCut >= _minSecForCut && !PlayerToFocus.IsAlive) {
                PlayerToFocus = GetRandomPlayer();
            }
        }

        protected override void Activate() {
            if (PlayerToFocus == null) PlayerToFocus = GetRandomPlayer();
            SetFocusOnPlayer(PlayerToFocus);
        }

        protected override void Deactivate() {
            if (_currentActiveFollowCam != null) {
                _currentActiveFollowCam.SetActive(false);
                _currentActiveFollowCam = null;
            }
        }

        protected override void OnPlayerGotHit(ShotData shotData, IPlayer targetPlayer, Vector3 hitPoint, DamageDetectorBase.ColliderType targetType) {
            if (HardFocusOnPlayer || !IsActive) return;

            if (TimeSinceLastCut >= _minSecForCut && PlayerToFocus != shotData.Player) {
                PlayerToFocus = shotData.Player;
            } else if (!targetPlayer.IsAlive && targetPlayer == PlayerToFocus) {
                StartCoroutine(DelayedFocusChange(GetRandomPlayer()));
            }
        }

        IEnumerator DelayedFocusChange(IPlayer player) {
            TimeSinceLastCut = 0 - _waitingForChangeAfterFocusDied;
            yield return new WaitForSeconds(_waitingForChangeAfterFocusDied);

            PlayerToFocus = player;
        }

        private void SetFocusOnPlayer([CanBeNull] IPlayer player) {
            FollowPlayerCamManager followCamOfPlayer = GetFollowPlayerCamManagerByPlayer(player);
            if (_currentActiveFollowCam != null)
                _currentActiveFollowCam.SetActive(false);
            if (followCamOfPlayer != null)
                followCamOfPlayer.SetActive(true);
            _currentActiveFollowCam = followCamOfPlayer;
        }

        protected override void OnPlayerToFocusChanged(CameraManager sender, IPlayer player) {
            if (sender.CurrentCameraMode == CamMode || _automaticMode.CurrentRunningMode == CamMode)
                SetFocusOnPlayer(player);
        }

        protected override void OnPlayerAdded(IPlayer player) {
            base.OnPlayerAdded(player);
            GameObject followObjectParent = InstantiateWrapper.InstantiateWithMessage(_followPlayerCamManagerPrefab, transform);
            FollowPlayerCamManager followCamManager = followObjectParent.GetComponent<FollowPlayerCamManager>();
            _followPlayerCams.Add(followCamManager);
            followCamManager.FollowingPlayer = player;
        }

        protected override void OnPlayerRemoved(IPlayer player) {
            base.OnPlayerRemoved(player);
            FollowPlayerCamManager followCamOfPlayer = GetFollowPlayerCamManagerByPlayer(player);
            if (followCamOfPlayer != null) {
                if (followCamOfPlayer == _currentActiveFollowCam) {
                    _currentActiveFollowCam = null;
                    PlayerToFocus = GetRandomPlayer();
                    if(PlayerToFocus == null)
                        CameraManager.Instance.SwitchCameraMode((int)CameraMode.Undefined);
                }

                _followPlayerCams.Remove(followCamOfPlayer);
                Destroy(followCamOfPlayer.gameObject);
            }
        }

        [CanBeNull]
        private FollowPlayerCamManager GetFollowPlayerCamManagerByPlayer(IPlayer player) {
            return _followPlayerCams.FirstOrDefault(cam => cam.FollowingPlayer == player);
        }
    }
}