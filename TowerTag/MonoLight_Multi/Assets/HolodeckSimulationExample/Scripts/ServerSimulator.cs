/******************************************************************************
 * Copyright (C) Holodeck VR GmbH 2017 - 2020.                                *
 * SPREE Interactive SDK Version 4.0.27-RC                                    *
 ******************************************************************************/

using UnityEngine;
using Holodeck;
using System.Linq;
using System;

public class ServerSimulator : MonoBehaviour
{
    //Instance Access
    public static ServerSimulator instance;

    [Header("Player Settings")]
    [Tooltip("Configure the PlayerID here")]
    public int PlayerID = 1337;
    [Tooltip("Configure the FOV here (Standard = 64)")]
    public float fov = 64f;

    [Header("Security Settings")]
    [Tooltip("Origin of the Bounding Box")]
    public Vector3 boundingboxMin = new Vector3(0.0f, 0.0f, 0.0f);
    [Tooltip("Maximum Size of the Bounding Box")]
    public Vector3 boundingboxMax = new Vector3(20.0f, 3.0f, 15.0f);
    [Tooltip("Set up the Warning Distance")]
    public Vector3 warningdistance = new Vector3(2.0f, 0.0f, 2.0f);

    [Header("General Settings")]
    [Tooltip("Configure the HeadOffset here")]
    public Vector3 HeadOffset = Vector3.zero;
    [Tooltip("Configure the Sender Types")]
    public Sender[] senderTypes;

    bool ServerInfoavailable = false;

    /// <summary>
    /// Setting the instance
    /// </summary>
    void OnEnable()
    {
        Holodeck.GeneralAPI.HolodeckStateChanged += MakeAvailable;
        instance = this;
        // has to be active before initialising more of the backend
        Holodeck.DebugAPI.IsSimulation(true);
    }

    private void MakeAvailable(HolodeckState state)
    {
        if(state == HolodeckState.Startup)
        {
            ServerInfoavailable = true;
            Debug.LogError("SERVER SIMULATOR ACTIVE!! PLEASE REMOVE FOR REGULAR TESTS!!");
        }
    }

    /// <summary>
    ///  Updating the ServerSimulator
    /// </summary>
    void Update()
    {
        if (ServerInfoavailable)
        {
            ConfigData config = new ConfigData();
            config.playerID = PlayerID;
            config.fieldOfView = fov;
            config.boundingbox1 = boundingboxMin;
            config.boundingbox2 = boundingboxMax;
            config.warningDistance = warningdistance;
            config.fromHeadToSender = HeadOffset;
            config.senderTypes = senderTypes.ToList<Sender>();
            ServerInfoavailable = false;
            DebugAPI.SimulateConfig(config);
        }
    }
}

