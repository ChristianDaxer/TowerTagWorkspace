using System.Collections;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using TowerTag;
using UnityEngine.UI;
using static OperatorCamera.CameraManager;

namespace OperatorCamera {
    public class EgoViewMode : CameraModeBase {
        [Space] [Header("Mode Specific Components")] [SerializeField]
        private LookAtTargetEgoView _egoViewLookAtTarget;

        [SerializeField, Tooltip("The game object that displays the spectated player name")]
        private GameObject _playerNameDisplay;

        [SerializeField, Tooltip("The Image of the player name background for the player name")]
        private Image _playerNameFrame;

        [SerializeField, Tooltip("The text field for the player name")]
        private TMP_Text _playerNameText;

        [SerializeField] private AutomaticMode _automaticMode;

        //Cache for the current following avatar. Visibility gets toggled when entering or exiting ego view
        private PlayerAvatar _currentAvatar;
        private float _waitingForChangeAfterFocusDied = 1f;

        private void Update() {
            if (!IsActive || HardFocusOnPlayer || PlayerToFocus == null) return;
            if (TimeSinceLastCut >= _minSecForCut && !PlayerToFocus.IsAlive) {
                PlayerToFocus = GetRandomPlayer();
            }
        }

        protected override void Activate() {
            if (PlayerToFocus == null) PlayerToFocus = GetRandomPlayer();
            _virtualCamera.enabled = true;
            _virtualCamera.Priority = 100;
            _playerNameDisplay.SetActive(true);
            SetFocusOnPlayer(PlayerToFocus);
        }

        protected override void Deactivate() {
            _virtualCamera.enabled = false;
            _virtualCamera.Priority = 0;
            _playerNameDisplay.SetActive(false);
            SetFocusOnPlayer(null);
            _currentAvatar = null;
        }

        protected override void OnPlayerGotHit(ShotData shotData, IPlayer targetPlayer, Vector3 hitPoint,
            DamageDetectorBase.ColliderType targetType) {
            if (HardFocusOnPlayer || !IsActive) return;

            if (TimeSinceLastCut >= _minSecForCut && PlayerToFocus != shotData.Player) {
                PlayerToFocus = shotData.Player;
            }
            else if (!targetPlayer.IsAlive && targetPlayer == PlayerToFocus) {
                StartCoroutine(DelayedFocusChange(GetRandomPlayer()));
            }
        }

        private IEnumerator DelayedFocusChange(IPlayer player) {
            TimeSinceLastCut = 0 - _waitingForChangeAfterFocusDied;
            yield return new WaitForSeconds(_waitingForChangeAfterFocusDied);

            PlayerToFocus = player;
        }

        private void SetFocusOnPlayer([CanBeNull] IPlayer playerToFocus) {
            Transform playerHeadTransform = null;
            PlayerAvatar oldAvatar = _currentAvatar;

            if (playerToFocus != null && playerToFocus.PlayerAvatar != null) {
                _currentAvatar = playerToFocus.PlayerAvatar;
                playerHeadTransform = _currentAvatar.AvatarMovement.HeadSourceTransform;
                SetUIComponents(playerToFocus);
            }
            _virtualCamera.m_Follow = playerHeadTransform;
            _egoViewLookAtTarget.PlayerHead = playerHeadTransform;
            if (_currentAvatar != null) {
                _currentAvatar.PlayerAvatarParent.SetActive(playerHeadTransform == null);
                _currentAvatar.gameObject.GetComponentInChildren<NameBadge>(true).Badge.enabled = playerHeadTransform == null;
            }

            if (oldAvatar != null) {
                oldAvatar.PlayerAvatarParent.SetActive(true);
                oldAvatar.gameObject.GetComponentInChildren<NameBadge>(true).Badge.enabled = true;
            }
        }

        /// <summary>
        /// The ego view has a UI for the player name. Setting up colors and player name
        /// </summary>
        /// <param name="playerToFocus"></param>
        private void SetUIComponents([NotNull] IPlayer playerToFocus) {
            Color teamColor = TeamManager.Singleton.Get(playerToFocus.TeamID).Colors.UI;
            _playerNameDisplay.SetActive(true);
            _playerNameFrame.color = teamColor;
            _playerNameText.color = teamColor;
            _playerNameText.text = playerToFocus.PlayerName;
        }

        protected override void OnPlayerToFocusChanged(CameraManager sender, IPlayer player) {
            if (sender.CurrentCameraMode == CamMode || _automaticMode.CurrentRunningMode == CamMode)
                SetFocusOnPlayer(player);
        }

        protected override void OnPlayerRemoved(IPlayer player) {
            base.OnPlayerRemoved(player);
            if (!IsActive) return;
            if (player == PlayerToFocus) {
                PlayerToFocus = GetRandomPlayer();
                if (PlayerToFocus == null)
                    CameraManager.Instance.SwitchCameraMode((int)CameraMode.Undefined);
            }
        }
    }
}