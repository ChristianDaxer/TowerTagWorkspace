/******************************************************************************
 * Copyright (C) Holodeck VR GmbH 2017 - 2020.                                *
 * SPREE Interactive SDK Version 4.0.27-RC                                    *
 ******************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using System.Linq;


// DO NOT TOUCH THIS PLEASE IT IS IMPORTANT
/// <summary>
/// This class contains management of the tracking status on inside-out devices.
/// </summary>
public class TrackingAvailableHook
{
    private static int AVAILABLE_REQUIRED_FRAMES = 40;
    private static int MAX_MISSED_FRAMES = 5;
    private static int RESET_REQUIRED_FRAMES_START = 20;
    private static int RESET_REQUIRED_FRAMES_PLAYING = 360;
    private static int missedFrames = AVAILABLE_REQUIRED_FRAMES;
    private static int availableFrames = 0;

    private static bool wasNotPlaying = true;

    private static uint DRIFT_HISTORY_LENGTH = 30;

    private static int _FRAMERATE_ = 72;
    private static int latencyMs = 100;
    private static uint LATENCY_COMPENSATE_FRAMES = (uint)(latencyMs / 1000.0f * _FRAMERATE_);

    private static Holodeck.RingBuffer<Vector3> cameraPositions = new Holodeck.RingBuffer<Vector3>(DRIFT_HISTORY_LENGTH);
    private static Holodeck.RingBuffer<Vector3> externalPositions = new Holodeck.RingBuffer<Vector3>(DRIFT_HISTORY_LENGTH);

    private static int framesWithDrift = 0;
    private static int framesWithNoDrift;
    private static int MAX_FRAMES_WITH_DRIFT = 20;
    private static int FRAMES_REQUIRED_COMPATIBILITY_MODE = _FRAMERATE_ * 5;
    private static int FRAMES_REQUIRED_FOR_DRIFT_RESET = 15;
    private static float DRIFT_MAX_DIFFERENCE = 0.9f;

    [RuntimeInitializeOnLoadMethod]
    static void LoadAssemblyName()
    {
        Holodeck.DebugAPI.SetAssemblyName(Assembly.GetExecutingAssembly().FullName);
        Holodeck.GeneralAPI.HolodeckStateChanged += Init;
    }

    /// <summary>
    /// Returns true if the headset pose is drifting, e.g. on MDSP crash.
    /// </summary>
    public static bool isHeadsetPoseDrifting()
    {
        return framesWithDrift > MAX_FRAMES_WITH_DRIFT;
    }

    /// <summary>
    /// Gets the tracking mode on neo 2 headsets from system properties
    /// </summary>
    /// <returns>The current tracking mode. 1 = 3dof, 3 = 6dof</returns>
    public static int GetTrackingMode()
    {
        int mode = 0;
        try
        {
            AndroidJavaClass jc = new AndroidJavaClass("com.jointhespree.neo2crashrecovery.CrashRecovery");
            AndroidJavaObject param = new AndroidJavaObject("java.lang.String", "persist.pvrservice.trackingmode");
            mode = System.Int32.Parse(jc.CallStatic<String>("getSystemProperty", param));
        } catch(Exception e)
        {
            Debug.Log("[HDVR] " + e.Message);
        }


        return mode;
        
    } 

    /// <summary>
    /// Initializes the inside out tracking monitoring
    /// </summary>
    /// <param name="state"></param>
    public static void Init(Holodeck.HolodeckState state)
    {
        if(state == Holodeck.HolodeckState.Configured)
        {
            if (Holodeck.GeneralAPI.GetHMDType() == Holodeck.HMDType.Neo)
            {
                Holodeck.UnityMainLoopHook.holostart += Start;
                Holodeck.UnityMainLoopHook.holoUpdate += Update;
                //Holodeck.GeneralAPI.HolodeckStateChanged -= Init;
            }
        } else if(state == Holodeck.HolodeckState.Ready)
        {
            wasNotPlaying = false;
        } else if(state == Holodeck.HolodeckState.None)
        {
            wasNotPlaying = true;
        }
    }
    /// <summary>
    /// Sets up monitoring of the camera and recovers if the camera becomes accessible.
    /// The camera being accessible indicates a crash in the qvrservice on neo2 devices.
    /// </summary>
    public static void Start()
    {
        //AndroidJavaClass jc = new AndroidJavaClass("com.jointhespree.neo2crashrecovery.CrashRecovery");
        //AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        //AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        //AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");
        //jc.CallStatic("initCameraCrashRecovery", context);
        //Pvr_UnitySDKAPI.Sensor.UPvr_InitPsensor();
    }
    /// <summary>
    /// Checks every update if recovery should be executed and presses the confirm button to reset the tracking if required.
    /// </summary>
    private static void Update()
    {
        if (Pvr_UnitySDKAPI.Sensor.UPvr_GetPsensorState() != 0 && !Application.isEditor)
        {
            return;
        }
        
        // only add to buffer if external position is available to avoid desync
        // also only count frames where pico is reporting a good pose over their api to specifically target MDSP
        if(Holodeck.IdAPI.HasExternalPosition(Holodeck.IdAPI.GetPlayerId()) && Pvr_UnitySDKAPI.Sensor.UPvr_Get6DofSensorQualityStatus() == 3)
        {
            Vector3 external = Holodeck.IdAPI.GetRawPosition();
            Vector3 camera = Pvr_UnitySDKSensor.Instance.HeadPose.Position;

            externalPositions.AddValue(external);
            cameraPositions.AddValue(camera);
        }

        //Debug.Log("missed: " + missedFrames + " avail: " + availableFrames);
        int needFrames = wasNotPlaying ? RESET_REQUIRED_FRAMES_START : RESET_REQUIRED_FRAMES_PLAYING;
        if (!Application.isEditor && missedFrames >= needFrames)
        {
            Debug.Log("[HDVR] Resetting movement tracking");
            AndroidJavaClass jc = new AndroidJavaClass("com.jointhespree.neo2crashrecovery.CrashRecovery");
            jc.CallStatic("pressConfirmButton"); // might throw native access exception
        }
    }

    /// <summary>
    /// Returns if tracking is currently available on an inside-out headset.
    /// It will not be available if the sensors are covered or the headset pose is drifting.
    /// This should not be called directly, as it is called from within the dll using reflection.
    /// </summary>
    /// <returns>True if tracking is available, false if not</returns>
    public static bool IsTrackingAvailable()
    {
        if (Holodeck.GeneralAPI.GetHMDType() == Holodeck.HMDType.Neo) // Neo 2
        {
            bool isHeadsetPoseDrifting = false;
            // check velocities for MDSP drift
            if (externalPositions.Count == DRIFT_HISTORY_LENGTH && cameraPositions.Count == DRIFT_HISTORY_LENGTH)
            {
                List<float> velocitiesExternal  = new List<float>();
                List<float> velocitiesCamera = new List<float>();

                Vector3[] positionsExternal = externalPositions.GetAll();
                Vector3[] positionsCamera = cameraPositions.GetAll();

                //calculate latency compensated velocity curves
                for (int i = 0; i < DRIFT_HISTORY_LENGTH - LATENCY_COMPENSATE_FRAMES - 1; i++)
                {
                    velocitiesExternal.Add((positionsExternal[LATENCY_COMPENSATE_FRAMES + i + 1] - positionsExternal[LATENCY_COMPENSATE_FRAMES + i]).magnitude);
                    velocitiesCamera.Add((positionsCamera[i + 1] - positionsCamera[i]).magnitude);
                }

                // compare average velocity
                float externalAverage = velocitiesExternal.Average() + 0.0001f; // adding constant to avoid NaN/Infinity
                float cameraAverage = velocitiesCamera.Average() + 0.0001f;

                // get difference as clamped factor
                float difference = cameraAverage > externalAverage ? Math.Abs(1 - externalAverage / cameraAverage) : 0;

                if (difference > DRIFT_MAX_DIFFERENCE)
                {
                    // frame drifting
                    framesWithDrift++;
                    framesWithNoDrift = 0;
                } else
                {
                    //frame not drifting
                    if(framesWithNoDrift++ > FRAMES_REQUIRED_FOR_DRIFT_RESET) framesWithDrift = 0;
                }

                if (framesWithDrift > MAX_FRAMES_WITH_DRIFT)
                {
                    //headset is drifting
                    isHeadsetPoseDrifting = true;

                    // enable compatibility mode
                    if (framesWithDrift > FRAMES_REQUIRED_COMPATIBILITY_MODE)
                    {
                        Holodeck.DebugAPI.SetCompatibilityMode(true);
                    }
                }

            }

            // check api as well, but always return false if headset is drifting
            if (Pvr_UnitySDKAPI.Sensor.UPvr_Get6DofSensorQualityStatus() == 3 && Application.isFocused) { // pico has state 3 if the headset is being tracked
                //reset missed frames if enough good frames have been received
                if (++availableFrames >= AVAILABLE_REQUIRED_FRAMES) missedFrames = 0;

                // return false if headset is drfting
                if (isHeadsetPoseDrifting) return false;

                //headset currently not drifting, return api state
                return (missedFrames < MAX_MISSED_FRAMES && availableFrames > AVAILABLE_REQUIRED_FRAMES);
            } 
        } else // QUEST
        {
            List<UnityEngine.XR.XRNodeState> list = new List<UnityEngine.XR.XRNodeState>();
            UnityEngine.XR.InputTracking.GetNodeStates(list);
            foreach (UnityEngine.XR.XRNodeState node in list)
            {
                try
                {
                    if (node.nodeType == UnityEngine.XR.XRNode.Head && node.tracked)
                    {
                        missedFrames = 0;
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
            }
        }
        availableFrames = 0;
        return (++missedFrames < MAX_MISSED_FRAMES);
    }
}
