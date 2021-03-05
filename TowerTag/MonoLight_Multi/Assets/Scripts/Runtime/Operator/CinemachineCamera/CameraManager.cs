using System;
using System.Linq;
using JetBrains.Annotations;
using TowerTag;
using TowerTagSOES;
using UnityEngine;
using VRNerdsUtilities;

namespace OperatorCamera {
    public class CameraManager : SingletonMonoBehaviour<CameraManager>, ICameraManager {

        public event Action<Pillar> GoalPillarGettingClaimed;
        public event Action<Pillar> GoalPillarClaimAborted;

        public enum CameraMode {
            Undefined = 0,
            Automatic = 1,
            Arena = 2,
            Follow = 3,
            Ego = 4,
            Free = 5
        }

        [SerializeField] private CameraModeBase[] _modes;
        [SerializeField] private TargetGroupManager _targetGroupManager;
        public float TimeSinceLastCut { get; set; }
        public TargetGroupManager TargetGroupManager => _targetGroupManager;
        public CameraMode CurrentCameraMode { get; set; } = CameraMode.Undefined;

        private bool _hardFocusOnPlayer;
        public bool HardFocusOnPlayer {
            get => _hardFocusOnPlayer;
            set {
                if (_hardFocusOnPlayer != value) {
                    _hardFocusOnPlayer = value;
                    HardFocusOnPlayerChanged?.Invoke(this, value);
                }
            }
        }

        private Pillar[] _goalTowers;
        [SerializeField] private IPlayer _playerToFocus;

        public IPlayer PlayerToFocus {
            get => _playerToFocus;
            set {
                if (_playerToFocus != value) {
                    _playerToFocus = value;
                    PlayerToFocusChanged?.Invoke(this, value);
                    TimeSinceLastCut = 0;
                }
            }
        }

        public event CameraChangeEventHandler CameraModeChanged;
        public event BoolChangedEventHandler HardFocusOnPlayerChanged;
        public event PlayerFocusEventHandler PlayerToFocusChanged;

        private void OnEnable() {
            //SingletonMonoBehaviour also exist on clients! This check is to avoid null refs on client
            if (!SharedControllerType.IsAdmin && !SharedControllerType.Spectator) return;
            if(TargetGroupManager != null) TargetGroupManager.Init();
            GameManager.Instance.MissionBriefingStarted += OnMissionBriefingStarted;
            GameManager.Instance.MatchHasFinishedLoading += OnMatchHasFinishedLoading;
        }

        private void OnDisable() {
            GameManager.Instance.MissionBriefingStarted -= OnMissionBriefingStarted;
            GameManager.Instance.MatchHasFinishedLoading -= OnMatchHasFinishedLoading;
        }

        private void OnMissionBriefingStarted(MatchDescription obj, GameMode gameMode) {
            SwitchCameraMode((int)CameraMode.Arena);
        }

        private void OnMatchHasFinishedLoading(IMatch match) {
            SwitchCameraMode((int)CameraMode.Automatic);
            match.Started += OnMatchStarted;
            match.Finished += OnMatchFinished;
            if(match.GameMode == GameMode.GoalTower)
                match.RoundFinished += OnRoundFinished;
        }

        private void OnMatchFinished(IMatch match) {
            if (match.GameMode == GameMode.GoalTower) {
                _goalTowers = PillarManager.Instance.GetAllGoalPillarsInScene();
                _goalTowers.ForEach(pillar => {
                    pillar.PlayerAttached -= OnPlayerAttachedOnGoalPillar;
                    pillar.PlayerDetached -= OnPlayerDetachedFromGoalPillar;
                });
                match.RoundFinished -= OnRoundFinished;
            }
        }

        private void OnMatchStarted(IMatch match) {
            TimeSinceLastCut = 0;
            if (match.GameMode == GameMode.GoalTower) {
                _goalTowers = PillarManager.Instance.GetAllGoalPillarsInScene();
                _goalTowers.ForEach(pillar => {
                    pillar.PlayerAttached += OnPlayerAttachedOnGoalPillar;
                    pillar.PlayerDetached += OnPlayerDetachedFromGoalPillar;
                });
            }
        }

        private void OnRoundFinished(IMatch match, TeamID roundWinningTeamID) {
            _goalTowers.ForEach(pillar => GoalPillarClaimAborted?.Invoke(pillar));
        }

        private void OnPlayerAttachedOnGoalPillar(Chargeable chargeable, IPlayer player) {
            if(chargeable is Pillar pillar && pillar.AttachedPlayers.Count <= 1 && pillar.OwningTeamID != player.TeamID) {
                GoalPillarGettingClaimed?.Invoke(pillar);
            }
        }

        private void OnPlayerDetachedFromGoalPillar(Chargeable chargeable, IPlayer player) {
            if (chargeable is Pillar pillar && pillar.AttachedPlayers.Count <= 0 && pillar.OwningTeamID != player.TeamID) {
                GoalPillarClaimAborted?.Invoke(pillar);
            }
        }


        private void Update() {
            if(GameManager.Instance.CurrentMatch != null && GameManager.Instance.CurrentMatch.IsActive)
                TimeSinceLastCut += Time.deltaTime;

            if (Input.GetKey(KeyCode.LeftShift)) {
                if (Input.GetKeyDown(KeyCode.F1)) {
                    SwitchCameraMode((int)CameraMode.Automatic);
                }

                if (Input.GetKeyDown(KeyCode.F2)) {
                    SwitchCameraMode((int)CameraMode.Arena);
                }

                if (Input.GetKeyDown(KeyCode.F3)) {
                    SwitchCameraMode((int)CameraMode.Follow);
                }

                if (Input.GetKeyDown(KeyCode.F4)) {
                    SwitchCameraMode((int)CameraMode.Ego);
                }

                if (Input.GetKeyDown(KeyCode.F5)) {
                    SwitchCameraMode((int)CameraMode.Free);
                }
            }
        }

        /// <summary>
        /// Switching the camera mode and focus a specific player
        /// </summary>
        /// <param name="newModeInt">The new mode</param>
        /// <param name="playerToFocus">If there is a special player we want to follow</param>
        /// <param name="hardFocus">If true, no automatic camera switches are allowed</param>
        public void SwitchCameraMode(int newModeInt, [CanBeNull] IPlayer playerToFocus, bool hardFocus = false) {
            var newMode = (CameraMode) newModeInt;
            if (newMode == CameraMode.Ego || newMode == CameraMode.Follow) {
                HardFocusOnPlayer = hardFocus;
                PlayerToFocus = playerToFocus;
            }

            if (newMode != CurrentCameraMode) {
                ChangeCameraMode(CurrentCameraMode, newMode);
                CameraModeChanged?.Invoke(this, CurrentCameraMode, newMode);
                CurrentCameraMode = newMode;
            }
        }

        /// <summary>
        /// Simple switch of camera mode
        /// </summary>
        /// <param name="newModeInt"></param>
        public void SwitchCameraMode(int newModeInt) {
            var newMode = (CameraMode) newModeInt;
            switch (newMode) {
                case CameraMode.Arena:
                    HardFocusOnPlayer = false;
                    PlayerToFocus = null;
                    break;
                case CameraMode.Ego:
                case CameraMode.Follow:
                    if (newMode == CurrentCameraMode)
                        HardFocusOnPlayer = !HardFocusOnPlayer;
                    break;
            }

            if (newMode != CurrentCameraMode) {
                ChangeCameraMode(CurrentCameraMode, newMode);
                CameraModeChanged?.Invoke(this, CurrentCameraMode, newMode);
                CurrentCameraMode = newMode;
            }
        }

        public void ChangeCameraMode(CameraMode oldMode, CameraMode newMode) {
            CameraModeBase leavingMode = _modes.FirstOrDefault(mode => mode.CamMode == oldMode);
            CameraModeBase enteringMode = _modes.FirstOrDefault(mode => mode.CamMode == newMode);

            if (leavingMode != null) leavingMode.IsActive = false;
            if (enteringMode != null) enteringMode.IsActive = true;
        }

        public void SetHardFocusOnPlayer(IPlayer player, bool active) {
            if (!active) {
                AdminController.Instance.OnPlayerToFocusChanged(this, null);
                SwitchCameraMode((int) CameraMode.Arena);
            }
            else
                SwitchCameraMode((int) CameraMode.Follow, player, true);
        }
    }
}
