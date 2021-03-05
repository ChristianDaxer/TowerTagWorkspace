using Cinemachine;
using System.Collections.Generic;
using System.Linq;
using TowerTag;
using UnityEngine;
using UnityEngine.Serialization;
using static OperatorCamera.CameraManager;

namespace OperatorCamera {
    public abstract class CameraModeBase : MonoBehaviour {
        [FormerlySerializedAs("CamMode")] [SerializeField] private CameraMode _camMode;
        [SerializeField] protected CinemachineVirtualCamera _virtualCamera;
        [SerializeField] private HitGameAction _hitGameAction;
        [SerializeField] protected int _minSecForCut = 4;
        [SerializeField] protected int _maxSecTillCut = 8;
        public CameraMode CamMode => _camMode;

        protected float TimeSinceLastCut {
            get => CameraManager.Instance.TimeSinceLastCut;
            set => CameraManager.Instance.TimeSinceLastCut = value;
        }
        protected bool HardFocusOnPlayer {
            get => CameraManager.Instance.HardFocusOnPlayer;
            set => CameraManager.Instance.HardFocusOnPlayer = value;
        }

        protected IPlayer PlayerToFocus {
            get { return CameraManager.Instance != null ? CameraManager.Instance.PlayerToFocus : null; }
            set => CameraManager.Instance.PlayerToFocus = value;
        }

        protected int GetPlayerCount() => PlayerManager.Instance.GetParticipatingPlayersCount();
        protected void GetPlayers (out IPlayer[] players, out int count) { PlayerManager.Instance.GetParticipatingPlayers(out players, out count); }

        private bool _isActive;

        public bool IsActive {
            get => _isActive;
            set {
                if (value == _isActive)
                    return;

                if (!value)
                    Deactivate();
                else
                    Activate();
                _isActive = value;
            }
        }

        protected void OnEnable() {
            CameraManager.Instance.PlayerToFocusChanged += OnPlayerToFocusChanged;
            GameManager.Instance.MatchHasFinishedLoading += OnMatchHasFinishedLoading;
            if (TTSceneManager.Instance != null) {
                TTSceneManager.Instance.CommendationSceneLoaded += DeactivateMode;
                TTSceneManager.Instance.HubSceneLoaded += DeactivateMode;
            }

            PlayerManager.Instance.PlayerAdded += OnPlayerAdded;
            PlayerManager.Instance.PlayerRemoved += OnPlayerRemoved;
            _hitGameAction.PlayerGotHit += OnPlayerGotHit;
        }

        protected void OnDisable() {
            if (TTSceneManager.Instance != null) {
                TTSceneManager.Instance.CommendationSceneLoaded -= DeactivateMode;
                TTSceneManager.Instance.HubSceneLoaded -= DeactivateMode;
            }
            if (CameraManager.Instance != null)
                CameraManager.Instance.PlayerToFocusChanged -= OnPlayerToFocusChanged;

            GameManager.Instance.MatchHasFinishedLoading -= OnMatchHasFinishedLoading;
            PlayerManager.Instance.PlayerAdded -= OnPlayerAdded;
            PlayerManager.Instance.PlayerRemoved -= OnPlayerRemoved;
            _hitGameAction.PlayerGotHit -= OnPlayerGotHit;
        }

        private void DeactivateMode() {
            IsActive = false;
        }


        protected virtual void OnPlayerToFocusChanged(CameraManager sender, IPlayer player) {
        }

        protected virtual void OnMatchHasFinishedLoading(IMatch obj) {
        }

        protected IPlayer GetRandomPlayer() {
            if (GetPlayerCount() > 0) {
                GetPlayers(out var players, out var count);
                List<IPlayer> otherPlayers =
                    players.Take(count).Where(player => player.IsAlive && player != PlayerToFocus && player.IsParticipating).ToList();
                if (otherPlayers.Count > 0)
                    return otherPlayers[Random.Range(0, otherPlayers.Count)];
            }

            Debug.LogWarning("No Player to follow found!");
            return null;
        }

        protected virtual void OnPlayerRemoved(IPlayer player) { }
        protected virtual void OnPlayerAdded(IPlayer player) { }

        protected abstract void OnPlayerGotHit(ShotData shotData, IPlayer targetPlayer, Vector3 hitPoint, DamageDetectorBase.ColliderType targetType);

        /// <summary>
        /// Activate the new camera mode
        /// </summary>
        protected abstract void Activate();

        /// <summary>
        /// Deactivate the current camera mode
        /// </summary>
        protected abstract void Deactivate();
    }
}