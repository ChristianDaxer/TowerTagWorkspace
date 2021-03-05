using UnityEngine;

public class VRDebugHelper : MonoBehaviour {
    public void OnGUI()
    {
        if (GUILayout.Button("Activate VR"))
        {
            VRController.ActivateOpenVR();
        }

        if (GUILayout.Button("Deactivate VR"))
        {
            VRController.DeactivateOpenVR();
        }
    }
}