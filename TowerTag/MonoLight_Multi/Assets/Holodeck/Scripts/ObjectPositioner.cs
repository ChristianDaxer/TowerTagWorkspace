/******************************************************************************
 * Copyright (C) Holodeck VR GmbH 2017 - 2020.                                *
 * SPREE Interactive SDK Version 4.0.27-RC                                    *
 ******************************************************************************/
using UnityEngine;
using Holodeck;

public class ObjectPositioner : MonoBehaviour
{
    #region INSPECTOR_VARIABLES
    [Tooltip("For safety reasons its recommened not do destroy objects if the sender is lost, because the physical object might be still in the holodeck")]
    public bool autodestroy = false;
	[Tooltip("Tag Type: If empty, standard enemy. Coresponding to tag string on Holodeck-Server")]
	public string type;
    [Tooltip("Use this if you want to apply Rotation directly to this Object")]
    public bool applyRotation = false;
    [Tooltip("Use this if you want to enable the players id for this object")]
    public bool allowPlayerID = false;
    #endregion

    #region VARIABLES
    //Storing the rotation for later use
    public Quaternion rotation;
    //ID is set by the EnemyManager Script
    public int id;
    public bool gotInitialPosition = false;
    #endregion

    #region UNITY_METHODS
    void Start()
    {
        Position();
    }
    /// <summary>
    /// Update the positon each UpdateCall
    /// </summary>
    void Update()
    {
        Position();
    }
    #endregion

    #region POSITIONING
    /// <summary>
    /// Positioning of objects, called each UnityUpdate
    /// </summary>
    /// 
    void Position()
    {
        //Proceed if the instance id is not the player id
		if (IdAPI.GetPlayerId() != id || allowPlayerID)
        {    
            //Proceed if the id has a position
			if (IdAPI.HasPosition(id))
            {
                //Get position and orientation data
                Vector3 _trackingOrigin = GeneralAPI.GetTrackingSpaceOrigin();
                Quaternion _trackingOrient = GeneralAPI.GetTrackingSpaceOrientation();
                Vector3 _position = IdAPI.GetLocalPosition(id, true);

                // Apply neck model for players (type not set)
                if(type == "")
                {
                    _position -= rotation * DebugAPI.GetTrackerOffset() + CalibrationAPI.GetNeckTranslation(rotation);
                }

                //Apply the position to enemy the gameobject
                _position = gotInitialPosition ? Vector3.Slerp(transform.localPosition, _position, Time.unscaledDeltaTime * 20) : _position;

                if (IdAPI.HasRotation(id))
                {
                    rotation = gotInitialPosition ? Quaternion.Slerp(rotation, IdAPI.GetRotation(id), Time.unscaledDeltaTime * 5) : IdAPI.GetRotation(id);
                }
                //Apply the position to the enemy gameobject
                transform.localPosition = _position;

                //Apply the trackingspace rotation to the enemy gameobject
                transform.localRotation = _trackingOrient;

                //Proceed if id has rotation and rotation should be applied
                if (IdAPI.HasRotation(id) && applyRotation)
                {
                    transform.localRotation = rotation;
                    //Apply target rotation to the enemy gameobject
                    transform.localRotation = IdAPI.GetRotation(id);
                }

                gotInitialPosition = true;
            } else
            {
                //Proceed if autodestroy is enabled and id has no position
                if (autodestroy)
                {
                    //Destroy the enemy gameobject
                    Destroy(gameObject);
                }
			}
		} else
        {
            //Proceed if autodestroy is enabled and gameobject has no id
            if (autodestroy)
            {
                //Destroy the enemy gameobject
                Destroy(gameObject);
            }
        }
    }
    #endregion
}