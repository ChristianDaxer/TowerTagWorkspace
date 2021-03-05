using JetBrains.Annotations;
using UnityEngine;

public abstract class MonoStereoCameraSwitcher : MonoBehaviour {
    protected enum CamMode {
        Mono,
        Stereo
    }
    [SerializeField] protected CamMode _currentMode;

    protected virtual void Awake() {
        
    }

    protected virtual void OnEnable() {
        GameManager.Instance.MissionBriefingStarted += SetToStereo;
        GameManager.Instance.MatchSceneLoading += SetToMono;
        GameManager.Instance.MatchConfigurationStarted += SetToMono;
    }

    protected virtual void OnDisable() {
        GameManager.Instance.MissionBriefingStarted -= SetToStereo;
        GameManager.Instance.MatchSceneLoading -= SetToMono;
        GameManager.Instance.MatchConfigurationStarted -= SetToMono;
    }

    public abstract void SetToStereo(MatchDescription matchDescription, GameMode gameMode);
    public abstract void SetToMono();
}
