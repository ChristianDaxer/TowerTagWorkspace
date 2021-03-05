using UI;
using UnityEngine;
#if !UNITY_ANDROID
using Valve.VR;
#endif

public class DisconnectOnOpenVRFailure : MonoBehaviour {
    [SerializeField] private MessageQueue _messageQueue;

#if !UNITY_ANDROID
    private void Update() {
        if (OpenVR.Input == null && !TowerTagSettings.Hologate) {
            _messageQueue.AddErrorMessage(
                "There was an error with your vr device");
            VRController.DeactivateOpenVR();
            ConnectionManager.Instance.Disconnect();
            TTSceneManager.Instance.LoadConnectScene(true);
            enabled = false;
        }
    }
#else
    //TODO check if there can be any failure with Quest, but not to my knownledge
#endif
}