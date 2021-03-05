using System.Collections;
using TowerTag;
using UnityEngine;
using static OperatorCamera.CameraManager;

namespace OperatorCamera {
    public sealed class AutomaticMode : CameraModeBase {
        public CameraMode CurrentRunningMode { get; private set; } = CameraMode.Undefined;

        private static bool GoalPillarGettingClaimed => CameraManager.Instance.TargetGroupManager.GoalPillarGettingClaimed;

        protected override void OnMatchHasFinishedLoading(IMatch match) {
            match.Started += OnMatchStarted;
            match.RoundFinished += OnRoundFinished;
            match.Finished += OnMatchFinished;
        }

        private void OnMatchFinished(IMatch match) {
            if (IsActive)
                StartCoroutine(DelayedModeChange(CameraMode.Arena, 3));
            match.Started -= OnMatchStarted;
            match.RoundFinished -= OnRoundFinished;
            match.Finished -= OnMatchFinished;
        }

        private void OnRoundFinished(IMatch match, TeamID roundWinningTeamID) {
            if (IsActive)
                StartCoroutine(DelayedModeChange(CameraMode.Arena, 3));
        }

        private void OnMatchStarted(IMatch match) {
            if (IsActive)
                SwitchModeIntern(CameraMode.Arena);

            if (match.GameMode == GameMode.GoalTower) {
                PillarManager.Instance.GetAllGoalPillarsInScene().ForEach(goalTower => {
                    goalTower.PlayerAttached += OnPlayerAttachedToGoalTower;
                });
            }
        }

        private void OnPlayerAttachedToGoalTower(Chargeable chargeable, IPlayer player) {
            if (!IsActive)
                return;
            if (chargeable is Pillar goalTower && player.TeamID != goalTower.OwningTeamID) {
                SwitchModeIntern(CameraMode.Arena);
            }
        }

        private void Update() {
            if (!IsActive && GameManager.Instance.CurrentMatch != null
                          && !GameManager.Instance.CurrentMatch.IsActive)
                return;

            if (!GoalPillarGettingClaimed && TimeSinceLastCut >= _maxSecTillCut) {
                RandomCameraCut();
            }
        }

        private void RandomCameraCut() {
            float random;
            switch (CurrentRunningMode) {
                case CameraMode.Arena:
                    random = Random.Range(0.0f, 1.0f);
                    SwitchModeIntern(random < 0.7 ? CameraMode.Follow : CameraMode.Ego);
                    break;
                case CameraMode.Follow:
                    random = Random.Range(0.0f, 1.0f);
                    if (random <= 0.3f && GetPlayerCount() > 0)
                        CameraManager.Instance.PlayerToFocus = GetRandomPlayer();
                    else if (random <= 0.6f) {
                        SwitchModeIntern(CameraMode.Ego);
                    } else if (random <= 1.0f) {
                        SwitchModeIntern(CameraMode.Arena);
                    }

                    break;
                case CameraMode.Ego:
                    random = Random.Range(0.0f, 1.0f);
                    if (random <= 0.25f && GetPlayerCount() > 0)
                        CameraManager.Instance.PlayerToFocus = GetRandomPlayer();
                    else if (random <= 0.6f) {
                        SwitchModeIntern(CameraMode.Follow);
                    } else if (random <= 1.0f) {
                        SwitchModeIntern(CameraMode.Arena);
                    }

                    break;
            }
        }

        private void SwitchModeIntern(CameraMode newMode) {
            CameraManager.Instance.ChangeCameraMode(CurrentRunningMode, newMode);
            CurrentRunningMode = newMode;
            TimeSinceLastCut = 0;
        }

        protected override void Activate() {
            SwitchModeIntern(CameraMode.Arena);
        }

        protected override void Deactivate() {
            SwitchModeIntern(CameraMode.Undefined);
        }

        protected override void OnPlayerGotHit(ShotData shotData, IPlayer targetPlayer, Vector3 hitPoint, DamageDetectorBase.ColliderType targetType) {
            if (!IsActive)
                return;

            if (TimeSinceLastCut >= _minSecForCut) {
                if (CurrentRunningMode == CameraMode.Follow && shotData.Player == PlayerToFocus) {
                    SwitchModeIntern(CameraMode.Ego);
                }
                if (CurrentRunningMode != CameraMode.Arena && TimeSinceLastCut >= _minSecForCut) {
                    PlayerToFocus = shotData.Player;
                    SwitchModeIntern(Random.Range(0.0f, 1.0f) <= 0.5f ? CameraMode.Follow : CameraMode.Ego);
                }
            }
        }

        IEnumerator DelayedModeChange(CameraMode mode, int secDelay) {
            yield return new WaitForSeconds(secDelay);

            SwitchModeIntern(mode);
        }
    }
}