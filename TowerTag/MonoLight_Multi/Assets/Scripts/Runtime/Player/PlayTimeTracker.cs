using TowerTag;
using UnityEngine;

[RequireComponent(typeof(IPlayer))]
public class PlayTimeTracker : MonoBehaviour {
    private IPlayer _player;
    public float PlayTime { get; private set; }
    private IMatch _match;

    private void Awake() {
        _player = GetComponent<IPlayer>();
    }

    private void OnEnable() {
        GameManager.Instance.MatchHasChanged += OnMatchHasChanged;
        _match = GameManager.Instance.CurrentMatch;
    }

    private void OnMatchHasChanged(IMatch match) {
        PlayTime = 0;
        _match = match;
    }

    private void Update() {
        if (_player?.PlayerHealth != null && _match != null
                                  && _player.PlayerHealth.IsAlive
                                  && GameManager.Instance.MatchTimer.IsMatchTimer
                                  && _match.IsActive) {
            PlayTime += Time.deltaTime;
        }
    }
}