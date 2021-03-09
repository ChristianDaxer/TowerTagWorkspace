/******************************************************************************
 * Copyright (C) Holodeck VR GmbH 2017 - 2020.                                *
 * SPREE Interactive SDK Version 4.0.27-RC                                    *
 ******************************************************************************/
// uncomment to support ovrinput controllers, only works after importing the oculus integration from the asset store
//#define USE_CONTROLLER

using UnityEngine;
using Holodeck;
using UnityEngine.XR;

[RequireComponent(typeof(Camera))]
public class PlayerPositioner : MonoBehaviour
{
    #region PUBLIC_INSPECTOR_VARIABLES
    [Header("--- CONNECTION SETTINGS ---")]
    public ConnectionSettings connectionMode = ConnectionSettings.DEFAULT;
    [Tooltip("Standard IP is 192.168.1.10, the value of this field is only used in 'Connection Custom Mode'")]
    public string holodeckIP = "192.168.1.10";
    [Tooltip("Standard port is 80, the value of this field is only used in 'Connection Custom Mode'")]
    public int holodeckPort = 80;

    [Header("--- PLAYER ID ---")]
    public int ID = 1337;
    public string isUserPresent = "No";

    [Header("--- CONFIGURATION SETTINGS ---")]
    [Tooltip("Disable the seethrough camera to enhance the performance")]
    public bool forceNoCamera = false;
    [Tooltip("Disable the GUI to enhance the performance")]
    public bool forceNoGUI = false;
    [Tooltip("Disable the bounding box warnings")]
    public bool forceNoBoundingBox = false;

    [Header("--- DEBUG SETTINGS ---")]
    [Tooltip("Enable emulated orientation. Should not be active in release")]
    public bool emulatedOrientation = false;
    [Tooltip("Disable the Calibration")]
    public bool disableCalibration = false;
    [Tooltip("Enable this if you want to record data")]
    public bool recordData = false;
    [Tooltip("Enable this if you want to simulate the proximity sensor with Key P")]
    public bool SimulateHeadsetOnOffWithP = false;
    [Tooltip("Enable this if you want the editor to simulate a specific HMD")]
    public bool simulateHeadsetType = false;
    [Tooltip("The HMDType to simulate in the editor")]
    public HMDType headsetType = HMDType.Neo;
    [Tooltip("Enable this if you want to simulate tracking availability when simulating an inside-out HMD")]
    public bool simulateInsideOutTrackingAvailable = true;

    [Header("--- ALL FOUND IDs ---")]
    public int[] allIDS;
    #endregion

    #region PRIVATE_VARIABLES
    private bool controllerUseActivated = false;
    private string defaultIP = "192.168.1.10";
    private int defaultPort = 80;
    private Vector3 distance;
    private static bool prevUserPresent = false;
    private bool userPres = true;
    private string str_address = "";
    #endregion

    #region NEO_2_NECK_MODEL
    private float sphereRadius = 0.11f;
    private Vector3 sphereCenterOffset = new Vector3(-0.0f, -0.0466f, -0.0f);
    private Vector3 forwardPointOnSphere = new Vector3(0.0f, -0.0878f, 0.9961f);
    #endregion

    #region UNITY_METHODS
    /// <summary>
    /// Sets the position of Player to the head transform each Update call
    /// </summary>
    void Update()
    {
        //Set the Debug-Settings 
        GeneralAPI.SetNoCamera(forceNoCamera);
        GeneralAPI.SetNoGUI(forceNoGUI);
        GeneralAPI.SetNoBoundingBox(forceNoBoundingBox);

        //Get the players ID
        ID = IdAPI.GetPlayerId();

        //Copy all available IDs into an array
        allIDS = IdAPI.GetIds().ToArray();

        if (IdAPI.HasPosition(ID) && !GeneralAPI.HasHMDInsideOutTracking())
        {
            try
            {
                //Get the local position of the player
                Vector3 _position = IdAPI.GetLocalPosition();

                //Set the player position to the camera game object
                transform.parent.localPosition = _position;

                //Apply the target-rotation to the player if emulated orientation in inspector is active
                if (emulatedOrientation && Application.platform != RuntimePlatform.Android)
                {
                    transform.localRotation = IdAPI.GetRotation(ID);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("[HDVR] PlayerPositioner - Update(): " + e);
            }
        }

        //Activate controller movement if Holodeck is offline
        if (GeneralAPI.GetOfflineMode() && !controllerUseActivated)
        {
            controllerUseActivated = true;
            ActivateController();
        }

        if(Application.isEditor) DebugAPI.SetInsideOutTrackingAvailable(simulateInsideOutTrackingAvailable);

        //Toggle if user is present for debug-only
        if (Input.GetKeyDown(KeyCode.P))
        {
            userPres = !userPres;
            if (userPres) isUserPresent = "Yes";
            else isUserPresent = "No";
        }

        //Record position data
        if (recordData && Input.GetKeyDown(KeyCode.Escape))
        {
            DebugAPI.RecordData();
        }

        //Check if User is present or not (proximity sensor of the gear/pico device)
        CheckForUserPresentAgain();
    }

    /// <summary>
    /// Stopping Holodeck OnDisable
    /// </summary>
    private void OnDisable()
    {
        Holodeck.GeneralAPI.Stop();
    }

    /// <summary>
    /// Stops Holodeck OnDestroy
    /// </summary>
    private void OnDestroy()
    {
        Holodeck.GeneralAPI.Stop();
        // PICO ONLY
        Pvr_UnitySDKAPI.Sensor.UPvr_UnregisterPsensor();
    }

    /// <summary>
    /// Initial values are set
    /// </summary>
    private void OnEnable()
    {
        if (forceNoCamera || forceNoGUI) GeneralAPI.SetNoCamera(true);
        if (forceNoGUI) GeneralAPI.SetNoGUI(true);
        if (forceNoBoundingBox) GeneralAPI.SetNoBoundingBox(true);
        //Set the IP Adress. User defined or default, depending on the selection in inspector
        if (connectionMode == ConnectionSettings.CUSTOM)
        {
            str_address = "";
            System.Net.IPAddress address;
            if (System.Net.IPAddress.TryParse(holodeckIP, out address))
            {
                str_address = address.ToString();
                GeneralAPI.SetHolodeckIP(str_address);
                GeneralAPI.SetHolodeckPort(holodeckPort);
            }
            else
            {
                Debug.LogError("[HDVR] Custom IP Adress is not valid. Standard values are used.");
                connectionMode = ConnectionSettings.DEFAULT;
            }
        }
        if (connectionMode == ConnectionSettings.DEFAULT)
        {
            GeneralAPI.SetHolodeckIP(defaultIP);
            GeneralAPI.SetHolodeckPort(defaultPort);
        }
        //Disable the calibration
        if (disableCalibration)
        {
            CalibrationAPI.DisableCalibration(disableCalibration);
        }

        if (Application.isEditor && simulateHeadsetType) DebugAPI.SimulateHMDType(headsetType);
    }

    private void Awake()
    {
        if(disableCalibration)
        {
            XRDevice.SetTrackingSpaceType(TrackingSpaceType.RoomScale);
        } else
        {
            XRDevice.SetTrackingSpaceType(TrackingSpaceType.Stationary);
        }
        // PICO ONLY
        Pvr_UnitySDKAPI.Sensor.UPvr_InitPsensor();
        //TODO: fix next line
        //Pvr_UnitySDKAPI.ToBService.UPvr_PropertySetHomeKeyAll(Pvr_UnitySDKAPI.PBS_HomeEventEnum.LONG_PRESS, Pvr_UnitySDKAPI.PBS_HomeFunctionEnum.VALUE_HOME_CLEAN_MEMORY, 1, null, null, null);
        
    }

    void Start()
    {
        GeneralAPI.Start();
    }

    #endregion

    #region HELPERS
    /// <summary>
    /// If no player is present Holodeck is started, if user was present Holodeck is stopped
    /// </summary>
    private void CheckForUserPresentAgain()
    {
        if (Application.isEditor)
        {
            if (!SimulateHeadsetOnOffWithP)
            {
                userPres = (UnityEngine.XR.XRDevice.userPresence == UnityEngine.XR.UserPresenceState.Present);
            }
        }
        else
        {
            if(GeneralAPI.GetHMDType() == HMDType.Neo || DebugAPI.IsCompatibilityMode())
            {
                userPres = Pvr_UnitySDKAPI.Sensor.UPvr_GetPsensorState() == 0;
            } else
            {
                userPres = (UnityEngine.XR.XRDevice.userPresence == UnityEngine.XR.UserPresenceState.Present);

            }
           
        }

        if (prevUserPresent == false && userPres)
        {
            Debug.Log("[HDVR] Holodeck Start");
            Holodeck.GeneralAPI.Start();
        }
        else if (prevUserPresent == true && !userPres)
        {
            Debug.Log("[HDVR] Holodeck Stop");
            Holodeck.GeneralAPI.Stop();
        }
        prevUserPresent = userPres;
    }

    /// <summary>
    /// Add Controllerwalk Script to GameObject if System is in OfflineMode
    /// </summary>
    void ActivateController()
    {
        gameObject.AddComponent<Controllerwalk>();
    }
    #endregion
}

#region ENUM_FOR_NETWORK_SETTINGS
public enum ConnectionSettings
{
    DEFAULT = 0,
    CUSTOM = 1
}
#endregion