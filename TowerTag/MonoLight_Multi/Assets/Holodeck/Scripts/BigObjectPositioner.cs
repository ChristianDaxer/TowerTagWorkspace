/******************************************************************************
 * Copyright (C) Holodeck VR GmbH 2017 - 2020.                                *
 * SPREE Interactive SDK Version 4.0.27-RC                                    *
 ******************************************************************************/

using UnityEngine;
using System.Collections.Generic;

public class BigObjectPositioner : MonoBehaviour
{

	bool buffersinitialized = false;

    #region ANCHORS

    [Tooltip("A Gameobject with an ObjectPositioner component. Will be used to update the position of the big object.")]
    public GameObject anchor_Position;

    [Tooltip("A Gameobject with an ObjectPositioner component. Will be used to update the orientation of the big object relative to Anchor_Position.")]
    public GameObject anchor_Rotation;

    #endregion

    #region STORED STARTING GEOMETRY

    // v1
    private Vector3 positionAnchorToBigObject;
    // v2
    private Vector3 positionAnchorToRotationAnchor;

    // Rotation angle (angle between v1 and v2). 
    private Quaternion rotationToPositionAnchor;

    // magnitude
    float offsetLength;

    // StartPosition of big object in scene
    Vector3 startPosition;

    // StartOrientation of big object
    Quaternion startOrientation;

    /// <summary>
    /// Save directions and necessary distance of anchors to the BigObject at scene start. 
    /// During runtime, when the anchors are moved, the BigObject will adjust relative to the anchors to maintain the 
    /// starting alignment of the anchors to the BigObject. 
    /// 
    /// Height will be ignored for alignment calculation. 
    /// The height position of the BigObject will be determined by the Preset Position Y of the ObjectPositioner component of the anchor_Position
    /// GameObject.
    /// </summary>
    void SaveStartGeometry()
    {
        
        // get v1
        positionAnchorToBigObject = gameObject.transform.position - anchor_Position.transform.position;

        // get magnitude
        offsetLength = positionAnchorToBigObject.magnitude;
        positionAnchorToBigObject.Normalize();

        //get v2
        positionAnchorToRotationAnchor = anchor_Rotation.transform.position - anchor_Position.transform.position;

        //Flatten to avoid unwanted vertical rotations
        positionAnchorToRotationAnchor.y = 0;
        positionAnchorToRotationAnchor.Normalize();

        //Get rotation/angle v2v1
        Vector3 postionAnchorToBigObject_Flat = positionAnchorToBigObject;
        postionAnchorToBigObject_Flat.y = 0;
        rotationToPositionAnchor = Quaternion.FromToRotation(positionAnchorToRotationAnchor, postionAnchorToBigObject_Flat);

        startPosition = transform.position;
        startOrientation = transform.rotation;
    }

    #endregion

    #region POSITION FILTER MEMBERS

    List<Vector3> anchor_Position_Buffer;
    List<Vector3> anchor_Rotation_Buffer;

    Vector3 anchor_Position_filtered;
    Vector3 anchor_Rotation_filtered;

    [Tooltip("Size of the buffers used to filter the anchor position data. Positions in the buffers will be averaged for smoothing.")]
    public int BufferSize = 21;
    [Tooltip("Distance ignored before new anchor positions will influence the BigObject. Use to reduce jitter and limited drifts.")]
    public float DeadzoneRadius = .025f;

    #endregion

    #region POSITION FILTERING

    /// <summary>
    /// Regular smoothing by averaging.
    /// </summary>
    /// <param name="vecList"></param>
    /// <returns></returns>
    Vector3 Filter_Average(List<Vector3> vecList)
    {
        Vector3 avgVector = Vector3.zero;

        for (int i = 0; i < vecList.Count; i++)
        {
            avgVector += vecList[i];
        }

        avgVector /= vecList.Count;

        return avgVector;
    }

    /// <summary>
    /// DecimalPlace: 1-> round to 0.0, 2-> round to 0.00 and so on.
    /// </summary>
    Vector3 FilterPosition_Round(Vector3 position, int decimalPlace)
    {
        Vector3 filteredVector = position;

        // 10 -> first decimal position, 100 -> second decimal position, and so on
        filteredVector *= 100.0f;
        filteredVector = new Vector3(Mathf.Round(filteredVector.x), Mathf.Round(filteredVector.y), Mathf.Round(filteredVector.z));
        filteredVector /= 100.0f;


        return filteredVector;
    }

    /// <summary>
    /// Lessen constant drift or jitter of the object by ignoring small movements of the position data. 
    /// Makes the object move only if a certain movement threshold is reached.
    /// </summary>
    /// 
    /// <param name="oldPosition">The last anchor position used for calculation.</param>
    /// <param name="newPosition">The current anchor position.</param>
    /// <param name="radius">The distance in any direction from oldPosition to newPosition that will be ignored.</param>
    /// <returns>The position to be used for calculation.</returns>
    Vector3 FilterPosition_DeadZone(Vector3 oldPosition, Vector3 newPosition, float radius)
    {

        float magnitude = (newPosition - oldPosition).magnitude;

        if (magnitude > radius)
        {
            return newPosition;
        }
        else
        {
            return oldPosition;
        }
    }

    void InitializeBuffers()
    {
		
		for (int i = 0; i < BufferSize; i++)
        {
            anchor_Position_Buffer.Add(startPosition);
            anchor_Rotation_Buffer.Add(startPosition);
        }
		buffersinitialized = true;
    }


    void UpdateFilterBuffers()
    {
        anchor_Position_Buffer.RemoveAt(0);
        anchor_Position_Buffer.Add(anchor_Position.transform.position);

        anchor_Rotation_Buffer.RemoveAt(0);
        anchor_Rotation_Buffer.Add(anchor_Rotation.transform.position);
    }


    //LIST OF FILTERING POSSIBILITIES
    //Average Only

    //Average + Average

    //average + average && median

    //Filter Average deadZone (1-avg(9) -> deltaCheck)

    //Filter Average deadZone (1-avg(9) && median -> deltaCheck)

    // median zum verwerfen vor/nachschalten
    //__________________________________________________________
    #endregion

    /// <summary>
    /// Translate and rotate the gameobject this script is attached to by using the two anchor positions, filtered or unfiltered.
    /// </summary>
    private void Move(Vector3 newAnchorPosition, Vector3 newAnchorRotation)
    {
        Vector3 newV2 = newAnchorRotation - newAnchorPosition;
        newV2.y = 0;
        newV2.Normalize();

        Vector3 newV1 = new Vector3();
        newV1.y = 0;
        newV1 = rotationToPositionAnchor * newV2;

        Quaternion houseOffsetRotation = Quaternion.FromToRotation(positionAnchorToRotationAnchor, newV2);
        Vector3 newPosition = (houseOffsetRotation * positionAnchorToBigObject) * offsetLength + newAnchorPosition;

        //UPDATE POSITION
        transform.position = newPosition;

        //UPDATE ORIENTATION
        Quaternion newOrientation = Quaternion.FromToRotation(positionAnchorToRotationAnchor, newV2);
        transform.rotation = newOrientation * startOrientation;
    }

    // Use this for initialization
    void Start()
    {
		anchor_Position_Buffer = new List<Vector3>();
        anchor_Rotation_Buffer = new List<Vector3>();

        // Bind Anchors
        if (anchor_Rotation == null)
        {
            anchor_Rotation = gameObject;
        }
        if (anchor_Position == null)
        {
            anchor_Position = gameObject;
        }
    }

	// Update is called once per frame
    void Update()
    {
		if (buffersinitialized) {

            UpdateFilterBuffers ();

			// AVERAGE + DEADZONE
			Vector3 position_filtered = Filter_Average (anchor_Position_Buffer);
			Vector3 rotationPoint_filtered = Filter_Average (anchor_Rotation_Buffer);

			position_filtered = FilterPosition_DeadZone (anchor_Position_filtered, position_filtered, DeadzoneRadius);
			anchor_Position_filtered = position_filtered;

			rotationPoint_filtered = FilterPosition_DeadZone (anchor_Rotation_filtered, rotationPoint_filtered, DeadzoneRadius);
			anchor_Rotation_filtered = rotationPoint_filtered;

			Move (anchor_Position_filtered, anchor_Rotation_filtered);
		}
    }
}
