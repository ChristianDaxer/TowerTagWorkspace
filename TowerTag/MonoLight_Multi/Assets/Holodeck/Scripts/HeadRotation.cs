/******************************************************************************
 * Copyright (C) Holodeck VR GmbH 2017 - 2020.                                *
 * SPREE Interactive SDK Version 4.0.27-RC                                    *
 ******************************************************************************/

using UnityEngine;

public class HeadRotation : MonoBehaviour {

    public ObjectPositioner parentPositioner;

    /// <summary>
    /// Gets the ObjectPositioner and sets it to the parent GameObject
    /// </summary>
    void Start () {
		if (parentPositioner == null) parentPositioner = transform.parent.GetComponent<ObjectPositioner>();
    }
	
	/// <summary>
    /// Gets the rotation of the parent GameObject and sets it to the transform each Update
    /// </summary>
	void Update () {

        if (parentPositioner != null) {
            transform.localRotation = parentPositioner.rotation;
        }
	}
}
