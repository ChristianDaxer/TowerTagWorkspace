using System.Text;
using UnityEngine;
#if !UNITY_ANDROID
using Valve.VR;
#endif

public class ControllerTypeDetector : MonoBehaviour {
#if !UNITY_ANDROID
    [SerializeField] private SteamVR_Behaviour_Pose _controller;
#endif
    [SerializeField] private ControllerTypeDetector _otherController;

    public static ControllerType CurrentControllerType = ControllerType.Undefined;
    public static ConnectedHmdType CurrentConnectedHmdType = ConnectedHmdType.Undefined;

    public enum ControllerType {
        ViveController,
        OculusTouch,
        WmrController,
        Knuckles,
        ViveCosmos,
        Undefined = 99
    }

    public enum ConnectedHmdType {
        Cv1,
        Vive,
        Wmr,
        Quest,
        RiftS,
        Undefined = 99
    }

    void Awake() {
#if !UNITY_ANDROID
        _controller.onConnectedChangedEvent += OnConnected;
#else
        CurrentConnectedHmdType = ConnectedHmdType.Quest;
        CurrentControllerType = ControllerType.OculusTouch;
#endif
    }

#if !UNITY_ANDROID
    private void OnConnected(SteamVR_Behaviour_Pose fromAction, SteamVR_Input_Sources fromSource, bool deviceConnected) {
        LoadControllerType();
        LoadConnectedDeviceType();
    }


    private void LoadConnectedDeviceType() {
        if (CurrentConnectedHmdType != ConnectedHmdType.Undefined) return;

        switch (SteamVR.instance.hmd_ModelNumber) {
            case "Oculus Quest":
                CurrentConnectedHmdType = ConnectedHmdType.Quest;
                break;
            case "Vive DVT":
                CurrentConnectedHmdType = ConnectedHmdType.Vive;
                break;
            case "Oculus Rift S":
                CurrentConnectedHmdType = ConnectedHmdType.RiftS;
                break;
            case "Oculus Rift CV1":
                CurrentConnectedHmdType = ConnectedHmdType.Cv1;
                break;
            case "WMR":
                CurrentConnectedHmdType = ConnectedHmdType.Wmr;
                break;
            default:
                CurrentConnectedHmdType = ConnectedHmdType.Undefined;
                break;
        }

        Debug.Log(SteamVR.instance.hmd_ModelNumber);
        Debug.Log($"Using controller type: {CurrentControllerType.ToString()}");
    }

    private void LoadControllerType() {
        StringBuilder result = new StringBuilder();
        ETrackedPropertyError error = ETrackedPropertyError.TrackedProp_UnknownProperty;
        OpenVR.System.GetStringTrackedDeviceProperty((uint) _controller.GetDeviceIndex(), ETrackedDeviceProperty.Prop_ControllerType_String, result, 64, ref error);
        if (CurrentControllerType != ControllerType.Undefined) return;
        switch (result.ToString()) {
            case "oculus_touch":
                CurrentControllerType = ControllerType.OculusTouch;
                break;
            case "vive_controller":
                CurrentControllerType = ControllerType.ViveController;
                break;
            case "knuckles":
                CurrentControllerType = ControllerType.Knuckles;
                break;
            case "vive_cosmos_controller":
                CurrentControllerType = ControllerType.ViveCosmos;
                break;
            case "holographic_controller":
                CurrentControllerType = ControllerType.WmrController;
                break;
        }

        if (CurrentControllerType != ControllerType.Undefined)
            Destroy(_otherController);
        Debug.Log($"Using controller type: {CurrentControllerType.ToString()}");
    }

    private void OnDestroy() {
        _controller.onConnectedChangedEvent -= OnConnected;
    }
#endif
}