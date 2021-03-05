using Photon.Voice.PUN;
using Photon.Voice.Unity;
using TowerTagSOES;
using UnityEngine;
using UnityEngine.UI;

public class AudioInputVisualization : MonoBehaviour {
    [SerializeField, Tooltip("The duration to wait between every check")]
    private float _waitTimePerCheck = 1;

    [SerializeField] private Image _visualization;
    [SerializeField] private Color _activeColor;
    [SerializeField] private Color _inactiveColor;


    private Recorder _voiceRecorder;

    private float _timeSinceLastCheck;
    private bool _inputActive;

    public void Init(Image visualization, Color activeColor, Color inactiveColor, float waitTime = 1) {
        _visualization = visualization;
        _activeColor = activeColor;
        _inactiveColor = inactiveColor;
        _waitTimePerCheck = waitTime;
    }

    private void Start() {
        _voiceRecorder = PhotonVoiceNetwork.Instance.PrimaryRecorder;
        _visualization.color = _inactiveColor;
        if (SharedControllerType.IsAdmin || SharedControllerType.Spectator || _voiceRecorder == null) enabled = false;
    }

    private void Update() {
        _timeSinceLastCheck += Time.deltaTime;
        if (_timeSinceLastCheck <= _waitTimePerCheck) {
            if (_inputActive != !(_voiceRecorder.LevelMeter.CurrentPeakAmp <= _voiceRecorder.VoiceDetectionThreshold)) {
                _inputActive = !(_voiceRecorder.LevelMeter.CurrentPeakAmp <= _voiceRecorder.VoiceDetectionThreshold);
                _visualization.color = _inputActive ? _activeColor : _inactiveColor;
            }

            _timeSinceLastCheck = 0;
        }
    }
}