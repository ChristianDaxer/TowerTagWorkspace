using System;
using UnityEngine;

public class MissionBriefingLookAtCamera : MonoBehaviour {
    private Camera _camera;

    private void Start() {
        _camera = Camera.main;
    }

    private void Update() {
        try {
            transform.LookAt(_camera.transform);
        }
        catch (Exception) {
            // ignored
        }
    }
}