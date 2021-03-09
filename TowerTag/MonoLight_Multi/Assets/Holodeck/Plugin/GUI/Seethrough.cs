/******************************************************************************
 * Copyright (C) Holodeck VR GmbH 2017 - 2020.                                *
 * SPREE Interactive SDK Version 4.0.27-RC                                    *
 ******************************************************************************/
 #pragma warning disable CS0649
using UnityEngine;

namespace Holodeck
{
    /// <summary>
    /// Displays the real world on a plane for the user to look through
    /// No references due to added to the door prefab
    /// </summary>
    internal class Seethrough : MonoBehaviour
    {
        public Renderer LocalRenderer;
        public bool showInEditor = false;

        /// <summary>
        /// Stop Webcam and remove the texture
        /// </summary>
        void OnDisable()
        {
            if(LocalRenderer != null)
            {
                LocalRenderer.material.mainTexture = null;
            }

            if(DebugAPI.IsCompatibilityMode()) UnityMainLoopHook.holoFixedUpdate -= WebCam.RenderToTexture;

            // if (GeneralAPI.HasHMDInsideOutTracking()) WebCam.Stop();
        }

        /// <summary>
        /// Activate the webcam texture
        /// </summary>
        void OnEnable()
        {
            if (DebugAPI.IsCompatibilityMode()) UnityMainLoopHook.holoFixedUpdate += WebCam.RenderToTexture;

            LocalRenderer = this.GetComponent<Renderer>();
            if (!GeneralAPI.GetNoCamera() && GeneralAPI.GetHMDType() != HMDType.Neo)
            {
                if ((!Application.isEditor || showInEditor) && WebCam.HasCamera)
                {
                    LocalRenderer.material.mainTexture = WebCam.CameraTexture;
                }
            }
            else
            {
                this.gameObject.SetActive(false);
            }
        }
    }

}
