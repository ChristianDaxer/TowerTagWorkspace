/******************************************************************************
 * Copyright (C) Holodeck VR GmbH 2017 - 2020.                                *
 * SPREE Interactive SDK Version 4.0.27-RC                                    *
 ******************************************************************************/
using UnityEngine;
using Holodeck;

#pragma warning disable CS0649

/// <summary>
/// This script creates the floor at runtime. Therefore it receives the Area from 
/// the config server and scales the floor to this size.
/// </summary>
[RequireComponent(typeof(SpawnObjects))]
public class ResizeFloor : MonoBehaviour {

    #region VARIABLES
    [SerializeField]
    private Material floorMaterial;
    private GameObject floor;
    private SpawnObjects so;
    private HolodeckState state;
    private bool floorIsSet = false;
    private Transform objects;
    private GameObject o;
    private Bounds bounds;
    #endregion

    #region UNITY_METHODS
    /// <summary>
    /// Initially registrating the HDVR-State-Delegate, and creating necessary game objects
    /// </summary>
    void Start () {
        GeneralAPI.HolodeckStateChanged += SetHolodeckState;

        so = GetComponent<SpawnObjects>();

        floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        floor.GetComponent<Renderer>().material = floorMaterial;

        o = new GameObject("EnvironmentObjects");
        o.transform.parent = transform;
        objects = o.transform;
    }

	/// <summary>
    /// Checking each frame if the state has changed to ready and the floor is not set
    /// If so, the floor is created, if not a standard size of 10 by 10 meters is created.
    /// </summary>
	void Update () {
        if((state == HolodeckState.Configured) && !floorIsSet)
        {
            bounds = SafetyAPI.GetTrackingArea();
            if (bounds.size.x >= 8.0f && bounds.size.z >= 8.0f)
            {
                SetFinalFloorSize();
            }
            else
            {
                bounds.SetMinMax(Vector3.zero, new Vector3(10.0f, 10.0f, 10.0f));
                SetFinalFloorSize();
            }
        }
        if (state == HolodeckState.Offline && !floorIsSet)
        {
            bounds.SetMinMax(Vector3.zero, new Vector3(10.0f, 10.0f, 10.0f));
        }
	}
    #endregion

    #region SETTERS
    /// <summary>
    /// Called by HDVR State-Machine, changes the state variable
    /// </summary>
    /// <param name="_value"></param>
    private void SetHolodeckState(HolodeckState _value)
    {
        floorIsSet = false;
        state = _value;
    }

    /// <summary>
    /// Sets the floor size to it's final measurments
    /// </summary>
    private void SetFinalFloorSize()
    {
        floorIsSet = true;

        Vector3 playfieldMiddle = new Vector3(bounds.size.x / 2, 0, bounds.size.z / 2);
        floor.transform.localScale = new Vector3(bounds.size.x / 10, 1.0f, bounds.size.z / 10);
        floor.GetComponent<Renderer>().material.mainTextureScale = new Vector2(bounds.size.x, (bounds.size.z));
        floor.transform.parent = objects;
        floor.transform.position = playfieldMiddle;
        so.ResetScene();
        so.CreateObjects(bounds, objects);
    }
    #endregion
}
