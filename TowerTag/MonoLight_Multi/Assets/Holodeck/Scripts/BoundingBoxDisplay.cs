/******************************************************************************
 * Copyright (C) Holodeck VR GmbH 2017 - 2020.                                *
 * SPREE Interactive SDK Version 4.0.27-RC                                    *
 ******************************************************************************/

using UnityEngine;

public class BoundingBoxDisplay : MonoBehaviour {
    MeshRenderer[] ms;
    bool adjustTiling = true;

	/// <summary>
    /// Initial finding the MeshRenderer
    /// </summary>
	void Start () {
        ms = this.GetComponentsInChildren<MeshRenderer>();
	}
	
	/// <summary>
    /// Checks every update if the red warning grid should be shown
    /// </summary>
	void Update () {
        if(Holodeck.GeneralAPI.IsHolodeckReady())
        {
            if(adjustTiling)
            {
                Renderer[] allMaterials = transform.GetComponentsInChildren<Renderer>();
                
                for(int i = 0; i < allMaterials.Length; i++ )
                {
                    //Debug.LogWarning(i.ToString() + " " + allMaterials[i].material.name);
                    if(allMaterials[i].material.name.Contains("B"))
                    {
                        allMaterials[i].material.mainTextureScale  = new Vector2(Holodeck.SafetyAPI.GetTrackingArea().size.x, Holodeck.SafetyAPI.GetTrackingArea().size.y);
                    }
                    else
                    {
                        allMaterials[i].material.mainTextureScale = new Vector2(Holodeck.SafetyAPI.GetTrackingArea().size.z, Holodeck.SafetyAPI.GetTrackingArea().size.y);
                    }
                }
                adjustTiling = false;
            }
            this.transform.localScale = Holodeck.SafetyAPI.GetTrackingArea().size;

            this.transform.rotation = Holodeck.GeneralAPI.GetTrackingSpaceOrientation();
            this.transform.position = Holodeck.GeneralAPI.GetTrackingSpaceOrigin() + (Holodeck.GeneralAPI.GetTrackingSpaceOrientation() * Holodeck.SafetyAPI.GetTrackingArea().center);

            for (int i = 0; i < ms.Length; i++)
            {
                ms[i].enabled = Holodeck.SafetyAPI.ShowGrid();
            }
            
        }
	}
}
