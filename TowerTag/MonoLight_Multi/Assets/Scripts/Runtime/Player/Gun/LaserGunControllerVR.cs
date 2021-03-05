using UnityEngine;

/// <summary>
/// Controls the hierarchy position of the laser gun controller transform.
/// When the primary VR controller is switched, the parent is adapted accordingly.
/// </summary>
public class LaserGunControllerVR : MonoBehaviour {
    [SerializeField] private InputControllerVR _inputControllerVR;
}