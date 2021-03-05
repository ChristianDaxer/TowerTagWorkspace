using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OculusMonoStereoCameraSwitcher : MonoStereoCameraSwitcher
{
    [SerializeField]
    private OVRCameraRig _cameraRig;

    protected override void Awake() {
        base.Awake();

        _cameraRig.usePerEyeCameras = (_currentMode == CamMode.Stereo);
    }

    public override void SetToMono() {
        _cameraRig.usePerEyeCameras = false;
        _currentMode = CamMode.Mono;
    }

    public override void SetToStereo(MatchDescription matchDescription, GameMode gameMode) {
        _cameraRig.usePerEyeCameras = true;
        _currentMode = CamMode.Stereo;
    }

    protected override void OnEnable() {
        base.OnEnable();
    }

    protected override void OnDisable() {
        base.OnDisable();
    }
}
