using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteamVRMonoStereoCameraSwitcher : MonoStereoCameraSwitcher
{
    [SerializeField] private GameObject _monoCamera;
    [SerializeField] private GameObject _stereoCameraParent;

    protected override void Awake() {
        base.Awake();

        if (_monoCamera == null || _stereoCameraParent == null) {
            enabled = false;
            return;
        }

        if (_currentMode == CamMode.Mono) {
            SetToMono();
        }
        else if (_currentMode == CamMode.Stereo) {
            if (GameManager.Instance.CurrentMatch != null)
                SetToStereo(GameManager.Instance.MatchDescription, GameMode.UserVote);
        }
    }

    public override void SetToMono() {
        _stereoCameraParent.SetActive(false);
        _monoCamera.SetActive(true);
        _currentMode = CamMode.Mono;
    }

    public override void SetToStereo(MatchDescription matchDescription, GameMode gameMode) {
        _monoCamera.SetActive(false);
        _stereoCameraParent.SetActive(true);
        _currentMode = CamMode.Stereo;
    }

    protected override void OnEnable() {
        base.OnEnable();
    }

    protected override void OnDisable() {
        base.OnDisable();
    }
}
