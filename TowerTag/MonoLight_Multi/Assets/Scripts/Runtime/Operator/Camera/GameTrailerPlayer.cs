using System.Collections;
using TowerTag;
using UnityEngine;
using UnityEngine.Video;

namespace Hologate {
    public class GameTrailerPlayer : MonoBehaviour {
        [SerializeField] private VideoPlayer _trailer;
        private float _timer;
        private const float TrailerWaitingTime = 300;
        private bool _playerConnected;
        private Coroutine _trailerCoroutine;

        private void OnEnable() {
            PlayerManager.Instance.PlayerAdded += OnPlayerAddedOrRemoved;
            PlayerManager.Instance.PlayerRemoved += OnPlayerAddedOrRemoved;
        }

        private void OnDisable() {
            PlayerManager.Instance.PlayerAdded -= OnPlayerAddedOrRemoved;
            PlayerManager.Instance.PlayerRemoved -= OnPlayerAddedOrRemoved;
        }

        private void OnPlayerAddedOrRemoved(IPlayer player) {
            _playerConnected = PlayerManager.Instance.GetParticipatingPlayersCount() > 0;
            if (_trailer.isPlaying) {
                _trailer.Stop();
                if(_trailerCoroutine != null)
                    StopCoroutine(_trailerCoroutine);
                _trailer.gameObject.SetActive(false);
            }
        }

        void Update() {
            if (!_playerConnected && !_trailer.isPlaying) {
                _timer += Time.deltaTime;
                if (_timer >= TrailerWaitingTime) {
                    _trailerCoroutine = StartCoroutine(PlayTrailer());
                }
            }
        }

        private IEnumerator PlayTrailer() {
            _trailer.gameObject.SetActive(true);
            _timer = 0;
            _trailer.Play();
            while (_trailer.isPlaying)
                yield return null;
            _trailer.gameObject.SetActive(false);
            _trailerCoroutine = null;
        }


    }
}