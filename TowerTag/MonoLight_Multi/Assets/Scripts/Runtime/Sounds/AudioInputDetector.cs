using Photon.Voice.PUN;
using Photon.Voice.Unity;
using TowerTag;
using TowerTagSOES;
using UnityEngine;

public class AudioInputDetector : MonoBehaviour {
    [SerializeField] private PhotonVoiceView _photonVoiceView;

    [SerializeField, Tooltip("Min value of input that has to be reached to trigger")]
    private float _triggerInputLevel = 0.01f;
    [SerializeField, Tooltip("The duration to wait between every check")]
    private float _waitTimePerCheck = 1;

    private Recorder _voiceRecorder;
    private IPlayer _player;

    private float _timeSinceLastCheck;
    private bool _inputActive;

    private void Awake() {
        _player = GetComponent<IPlayer>();
        if (_player.IsBot)
        {
            Destroy(this);
            return;
        }
        _voiceRecorder = _photonVoiceView.RecorderInUse;
        if (SharedControllerType.IsAdmin || SharedControllerType.Spectator || _voiceRecorder == null) enabled = false;
    }

    private void Update() {
        _timeSinceLastCheck += Time.deltaTime;
        if (_timeSinceLastCheck <= _waitTimePerCheck) {
            if (_inputActive != !(_voiceRecorder.LevelMeter.CurrentPeakAmp <= _triggerInputLevel)) {
                _inputActive = !(_voiceRecorder.LevelMeter.CurrentPeakAmp <= _triggerInputLevel);
                _player.PlayerNetworkEventHandler.SendMicInputReceiveChange(_inputActive);
            }
            _timeSinceLastCheck = 0;
        }
    }
}
