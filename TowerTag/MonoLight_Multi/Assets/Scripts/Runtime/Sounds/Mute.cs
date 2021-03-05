using UnityEngine;

public class Mute : MonoBehaviour {
    [SerializeField] private HotKeys _hotKeys;

    // for testing
    public HotKeys HotKeys {
        set { _hotKeys = value; }
    }

    private void OnEnable() {
        _hotKeys.MuteToggled += OnMuteToggled;
    }

    private void OnDisable() {
        _hotKeys.MuteToggled -= OnMuteToggled;
    }

    private void OnMuteToggled() {
        AudioListener.pause = !AudioListener.pause;
    }
}