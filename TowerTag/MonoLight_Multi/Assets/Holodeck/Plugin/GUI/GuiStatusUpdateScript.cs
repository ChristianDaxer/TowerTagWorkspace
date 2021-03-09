/******************************************************************************
 * Copyright (C) Holodeck VR GmbH 2017 - 2020.                                *
 * SPREE Interactive SDK Version 4.0.27-RC                                    *
 ******************************************************************************/

using System.Collections;
using UnityEngine;
using Holodeck;
using System;

#pragma warning disable CS0414

public class GuiStatusUpdateScript : MonoBehaviour {

    public GameObject battery;
    public GameObject cameraPlane;
    public GameObject stateText;
    public GameObject stateInfo;
    public GameObject plane;
    public GameObject positionServerInfo;
    public GameObject configServerInfo;
    public GameObject serverInfo;
    public GameObject stopSign;
    public GameObject seethrough;

    private TextMesh stateInfoText;

    public bool showUi = true;
    /// <summary>
    /// Contains information if the UI should currently be shown.
    /// </summary>
    public bool ShowUI
    {
        get
        {
            return showUi;
        }
        set
        {
            showUi = value;
            HandleUI();
        }
    }
    /// <summary>
    /// Called every frame and displays the UI if necessary and as configured.
    /// </summary>
    private void HandleUI()
    {
        if (TrackingAvailableHook.isHeadsetPoseDrifting())
        {
            stateInfoText.text = "TRACKING ISSUES. GAME MIGHT RESTART.";
        }
        else
        {
            UpdateSymbol(GeneralAPI.GetState());
        }

        if (ShowUI)
        {
            if (!GeneralAPI.GetNoCamera())
            {
                if(GeneralAPI.GetHMDType() == HMDType.Neo)
                {
                    Pvr_UnitySDKAPI.BoundarySystem.UPvr_EnableSeeThroughManual(true);
                    seethrough.SetActive(true);
                } else
                {
                    cameraPlane.SetActive(true);
                }
                
                if (!GeneralAPI.GetNoGUI())
                {
                    stateInfo.SetActive(true);
                }
            }

        } else {
            if (GeneralAPI.GetHMDType() == HMDType.Neo)
            {
                seethrough.SetActive(false);
                Pvr_UnitySDKAPI.BoundarySystem.UPvr_EnableSeeThroughManual(false);
            } else
            {
                cameraPlane.SetActive(false);
            }
            stateInfo.SetActive(false);
        }
    }

    /// <summary>
    /// Initializing all variables, adding events and start coroutines
    /// </summary>
    void Start () {
        stateInfoText = stateText.GetComponent<TextMesh>();
        Holodeck.GeneralAPI.HolodeckStateChanged += UpdateSymbol;
        StartCoroutine(UpdateBattery());
        UpdateSymbol(GeneralAPI.GetState());
        if (GeneralAPI.GetNoCamera() || GeneralAPI.GetHMDType() == HMDType.Neo) cameraPlane.SetActive(false);

        if(GeneralAPI.HasHMDInsideOutTracking()) stopSign.SetActive(true);
        if(GeneralAPI.GetNoGUI() || GeneralAPI.GetHMDType() == HMDType.Neo)
        {
            stateInfo.SetActive(false);
            serverInfo.SetActive(false);
        }

        if (GeneralAPI.GetHMDType() == HMDType.Neo)
        {
            stateInfo.transform.localPosition = new Vector3(0, 0f, 0.587f);
            stateInfoText.fontSize = 20;
            plane.transform.localScale = new Vector3(1, 1, 0.025f);
        }
        else seethrough.SetActive(false);
    }

    /// <summary>
    /// Remove events on destroy
    /// </summary>
    private void OnDestroy()
    {
        Holodeck.GeneralAPI.HolodeckStateChanged -= UpdateSymbol;
    }

    WaitForSeconds delay = new WaitForSeconds(10.0f);
    /// <summary>
    /// Updates the current battery level all 10 seconds
    /// </summary>
    /// <returns></returns>
    IEnumerator UpdateBattery()
    {
        while (true)
        {
            if (SystemInfo.batteryLevel > 0.1f)
            {
                battery.SetActive(false);
            }
            else
            {
                battery.SetActive(true);
            }
            yield return delay;
        }
    }

    /// <summary>
    /// Check every update if the door should be closed or opened
    /// </summary>
    /// <returns></returns>
    void Update()
    {
        bool hide;
        try
        {
            hide = (GeneralAPI.GetState() == HolodeckState.Ready && SafetyAPI.PlayerIsInside() && IdAPI.HasPosition(IdAPI.GetPlayerId()) && SafetyAPI.PositionServerConnected() && SafetyAPI.LastPlayerPositionWithinTimeout());
            ShowUI = !hide;
        }
        catch (System.Exception e)
        {
            Debug.LogError("[HDVR] GuiStatusUpdateScript - ControlDoor(): " + e);
            ShowUI = false;
        }

        positionServerInfo.SetActive(!SafetyAPI.PositionServerConnected() || !IdAPI.HasPosition(IdAPI.GetPlayerId()) || !SafetyAPI.LastPlayerPositionWithinTimeout());
        configServerInfo.SetActive(!SafetyAPI.ConfigServerConnected());
    }


    /// <summary>
    /// Update the symbol, depending on the Holodeck state
    /// </summary>
    /// <param name="state"></param>
    void UpdateSymbol(HolodeckState state)
    {
        switch(state) {
            case HolodeckState.Ready:
                stateInfoText.text = "PLEASE RETURN TO THE PLAY AREA!";
                break;
            case HolodeckState.Startup:
                stateInfoText.text = "CONNECTING TO THE VR WORLD...";
                break;
            case HolodeckState.ConnectionError:
                stateInfoText.text = "FAILED TO CONNECT! PLEASE CONTACT AN OPERATOR.";
                break;
            case HolodeckState.CalibrationError:
                stateInfoText.text = "FAILED TO CALIBRATE! PLEASE CONTACT AN OPERATOR";
                break;
            case HolodeckState.PositionStreamError:
                stateInfoText.text = "FAILED TO RECEIVE POSITIONS! PLEASE CONTACT AN OPERATOR";
                break;
            case HolodeckState.Configured:
                stateInfoText.text = "PLEASE PROCEED TO THE PLAY AREA!";
                break;
            default:
                stateInfoText.text = "GETTING YOU INTO THE VR WORLD...";
                break;
        } 
    }

    /// <summary>
    /// Getter and Setter of the camera plane
    /// </summary>
    bool CameraShown
    {
        get
        {
            return cameraPlane.activeSelf;
        }
        set
        {
            if(!Holodeck.GeneralAPI.GetNoCamera()) cameraPlane.SetActive(value);
        }
    }
}
