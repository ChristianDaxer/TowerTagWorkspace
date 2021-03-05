using TowerTag;
using UnityEngine;

public class MatchSoundController : MonoBehaviour {
    [SerializeField] private AudioClip _winningJingle;
    [SerializeField] private AudioClip _drawJingle;
    [SerializeField] private AudioClip _roundFinishedJingle;
    [SerializeField] private AudioSource _source;

    private void Start() {
        // Check Sound
        CheckSerializedSound();

        // Register Listener
        GameManager.Instance.MatchHasFinishedLoading += OnMatchSceneLoaded;
    }

    private void OnMatchSceneLoaded(IMatch match) {
        match.RoundFinished += OnRoundFinished;
        match.Finished += OnMatchFinished;
    }

    private void OnDisable() {
        // Deregister Listener
        if (GameManager.Instance.CurrentMatch == null) return;
        GameManager.Instance.CurrentMatch.RoundFinished -= OnRoundFinished;
        GameManager.Instance.CurrentMatch.Finished -= OnMatchFinished;
    }

    private void OnDestroy()
    {
        GameManager.Instance.MatchHasFinishedLoading -= OnMatchSceneLoaded;
    }

    private void OnRoundFinished(IMatch match, TeamID roundWinningTeamId) {
        PlayMatchSound(_roundFinishedJingle);
    }

    private void OnMatchFinished(IMatch match) {
        match.RoundFinished -= OnRoundFinished;
        match.Finished -= OnMatchFinished;

        if (match.Stats.Draw) {
            PlayMatchSound(_drawJingle);
            return;
        }

        if (match.Stats.WinningTeamID == PlayerManager.Instance.GetOwnPlayer()?.TeamID)
            PlayMatchSound(_winningJingle);
    }

    private void CheckSerializedSound() {
        if (_source == null)
            Debug.LogError("Oops, something went wrong. Match sound Controller AudioSource can't be found.");
        if (_winningJingle == null)
            Debug.LogError("Oops, something went wrong. Match sound 'Winning Jingle' can't be found.");
        if (_drawJingle == null)
            Debug.LogError("Oops, something went wrong. Match sound 'Draw Jingle' can't be found.");
        if (_roundFinishedJingle == null)
            Debug.LogError("Oops, something went wrong. Match sound 'Round finished Jingle' can't be found.");
    }

    private void PlayMatchSound(AudioClip sound)
    {
        _source.clip = sound;
        _source.Play();
    }
}