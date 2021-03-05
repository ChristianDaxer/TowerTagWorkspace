using UnityEngine;

public class ThemeInitializer : MonoBehaviour {
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private MapTheme _mapTheme;

    private void OnEnable() {
        _mapTheme.Init(_audioSource);
        if (GameManager.Instance.CurrentMatch != null) {
            GameManager.Instance.CurrentMatch.Started += OnMatchStarted;
            GameManager.Instance.PauseReceived += OnPauseReceived;
            if (GameManager.Instance.CurrentMatch.MatchStarted)
                OnMatchStarted(GameManager.Instance.CurrentMatch);
        }
    }

    private void OnDisable() {
        if (GameManager.Instance.CurrentMatch != null) {
            GameManager.Instance.CurrentMatch.Started -= OnMatchStarted;
            GameManager.Instance.PauseReceived -= OnPauseReceived;
        }
    }

    private void OnPauseReceived(bool active) {
        if (active)
            _audioSource.Pause();
        else
            _audioSource.UnPause();
    }

    private void OnMatchStarted(IMatch match) {
        if (_audioSource.isPlaying)
            _audioSource.Stop();
        _audioSource.Play();
    }
}