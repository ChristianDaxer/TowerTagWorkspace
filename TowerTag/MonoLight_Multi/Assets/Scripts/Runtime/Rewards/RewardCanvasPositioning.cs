using System.Collections;
using TowerTag;
using UnityEngine;

namespace Rewards {
    public class RewardCanvasPositioning : MonoBehaviour {
        private RewardController _rewardController;
        private Coroutine _coroutine;
        private IPlayer _ownPlayer;

        private void OnEnable() {
            _rewardController = GameManager.Instance.RewardController;
            _ownPlayer = PlayerManager.Instance.GetOwnPlayer();
            if (_ownPlayer != null && _rewardController != null)
                _ownPlayer.TeleportHandler.PlayerTeleporting += StartLerpToNewPosition;
        }

        private void StartLerpToNewPosition(TeleportHandler sender, Pillar origin, Pillar target,
            float timeToTeleport) {
            if (_coroutine != null)
                StopCoroutine(_coroutine);

            _coroutine = StartCoroutine(LerpToNewPosition(timeToTeleport));
        }

        private IEnumerator LerpToNewPosition(float timeToTeleport) {
            float timer = 0;
            while (timer <= timeToTeleport) {
                transform.position = Vector3.Lerp(
                    transform.position,
                    _rewardController.CalculatePositionRelativeToPlayer(), 0.5f);
                timer += Time.deltaTime;
                yield return null;
            }

            _coroutine = null;
        }

        private void OnDisable() {
            if (_coroutine != null)
                StopCoroutine(_coroutine);

            if (_ownPlayer != null)
                _ownPlayer.TeleportHandler.PlayerTeleporting -= StartLerpToNewPosition;
        }
    }
}