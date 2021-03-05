using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR;
//using Valve.VR;

/// <summary>
/// Activates and deactivates VR depending on the set flag. use for debugging.
/// <author>Ole Jürgensen</author>
/// <date></date>
/// </summary>
public class VRController : MonoBehaviour
{
    private static VRController _instance;

    private static VRController Instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<VRController>();
            return _instance;
        }
    }

    public static void ActivateOpenVR()
    {
        if (Instance == null)
        {
            Debug.LogError("Cannot activate OpenVR: no VRController found. Please add one to the scene.");
            return;
        }

        Instance.StartCoroutine(ActivateXRDevice("OpenVR", true));
    }

    public static void DeactivateOpenVR()
    {
        if (Instance == null)
        {
            Debug.LogError("Cannot deactivate OpenVR: no VRController found. Please add one to the scene.");
            return;
        }

        Instance.StartCoroutine(ActivateXRDevice("None", false));
    }

    private static IEnumerator ActivateXRDevice(string device, bool vrEnabled)
    {
        if (vrEnabled && string.Compare(XRSettings.loadedDeviceName, device, StringComparison.OrdinalIgnoreCase) == 0)
        {
            Debug.Log("XR Device OpenVR already loaded.");
        }
        else if (vrEnabled)
        {
            XRSettings.LoadDeviceByName(device);
            yield return null;
            XRSettings.enabled = true;
        }
        else if(string.Compare(XRSettings.loadedDeviceName, device, StringComparison.OrdinalIgnoreCase) == 1){
            XRSettings.LoadDeviceByName(device);
           // Destroy(FindObjectOfType<SteamVR_Behaviour>());
            yield return null;
            XRSettings.enabled = false;
        }
    }
}