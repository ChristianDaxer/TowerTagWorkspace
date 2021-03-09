/******************************************************************************
 * Copyright (C) Holodeck VR GmbH 2017 - 2020.                                *
 * SPREE Interactive SDK Version 4.0.27-RC                                    *
 ******************************************************************************/

using UnityEngine;
using System.Threading;
using System.Collections;

/// <summary>
/// An Objects especial for debugging purpose, simulates incomming Positions per FixedFrame ( The Treansform Position is used as Input by manipulating the transform the recieved position changes)
/// </summary>
public class DebugPositionGenerator : MonoBehaviour
{
    public int ID = 0;
    public bool Orientation = false;
    public Transform SimulateOnTransform = null;
    public bool SimulateOnPlayer = false;
    public int latencyMs = 0;
    public float yawDriftFactor = 1.0f;

    private Quaternion lastRotation;
    private Vector3 lastPosition;

    /// <summary>
    /// Initializing the async thread position generator
    /// </summary>
    void OnEnable()
    {
        lastRotation = transform.localRotation;
        lastPosition = transform.localRotation * transform.localPosition;
        DisablePvrTracking();
    }
    /// <summary>
    /// Initializing the latency buffer
    /// </summary>
    void InitializeLatencyBuffer()
    {

    }

    /// <summary>
    /// Disables the pico head tracking so the heads pose can be overwritten
    /// </summary>
    void DisablePvrTracking()
    {
        if (SimulateOnTransform)
        {
            if (SimulateOnTransform.GetComponent<Pvr_UnitySDKHeadTrack>()) SimulateOnTransform.GetComponent<Pvr_UnitySDKHeadTrack>().enabled = false;
        }
    }

    /// <summary>
    /// Doing some stuff every fixed update
    /// </summary>
    void FixedUpdate()
    {
        // camera is not available in start()
        if (SimulateOnPlayer && !SimulateOnTransform)
        {
            SimulateOnTransform = Holodeck.DebugAPI.GetMainCamera();
            DisablePvrTracking();
        }

        Vector3 dequeuedPosition;

        if (ID == Holodeck.IdAPI.GetPlayerId())
        {
            dequeuedPosition = transform.position + (transform.rotation * Holodeck.DebugAPI.GetTrackerOffset());
        } else
        {
            dequeuedPosition = transform.position;
        }

        Quaternion dequeuedOrientation = transform.rotation;

        if (SimulateOnTransform != null)
        {
            Quaternion changeInRotation = (Quaternion.Inverse(lastRotation) * transform.localRotation);
            changeInRotation.y *= yawDriftFactor;
            changeInRotation.Normalize();
            SimulateOnTransform.localRotation *= changeInRotation;

            if(Holodeck.GeneralAPI.HasHMDInsideOutTracking())
            {
                Vector3 changeInPosition = transform.localPosition - lastPosition;
                SimulateOnTransform.Translate(Quaternion.Inverse(SimulateOnTransform.localRotation) * changeInPosition, Space.Self);
            }
        }

        lastRotation = transform.localRotation;
        lastPosition = transform.localPosition;

        StartCoroutine(delaySending(ID, dequeuedPosition, dequeuedOrientation)); 
    }

    private IEnumerator delaySending(int id, Vector3 position, Quaternion orientation)
    {
        yield return new WaitForSecondsRealtime(latencyMs / 1000.0f);
        if (Orientation)
        {
            Holodeck.DebugAPI.SimulateID(ID, position, orientation);
        }
        else
        {
            Holodeck.DebugAPI.SimulateID(ID, position);
        }
    }
}

