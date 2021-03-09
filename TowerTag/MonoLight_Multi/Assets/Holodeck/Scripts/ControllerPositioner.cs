/******************************************************************************
 * Copyright (C) Holodeck VR GmbH 2017 - 2020.                                *
 * SPREE Interactive SDK Version 4.0.27-RC                                    *
 ******************************************************************************/
#pragma warning disable CS0414
using UnityEngine;

namespace Holodeck
{
    /// <summary>
    /// This class provides the controller data for one Oculus Quest controller.
    /// </summary>
    public class ControllerPositioner : MonoBehaviour
    {

        #region PUBLIC_VARIABLES
        [Header("CONTROLLER SETTINGS")]
        [Tooltip("Please select the type of controller")]
        public ControllerType controllerType = ControllerType.None;
        [Tooltip("Disable to set controller position manually in editor, enable for build")]
        public bool useTracking = true;
        [Tooltip("Simulate controller tracking availability in editor")]
        public bool simulateControllerTracked = false;
        #endregion

        #region PRIVATE_VARIABLES
#if USEController
        private OVRInput.Controller controller;
#endif
        private Vector3 position;
        private Quaternion rotation;
        private int playerId;
        private bool controllerIsTracked = true;
        #endregion

        #region UNITY_METHODS
        /// <summary>
        /// Run at start event
        /// </summary>
        private void Start()
        {
            //Parse the Holodeck Controller Type to OVRInput Type
#if USEController
            controller = (OVRInput.Controller) controllerType;
#endif
        }

        public bool checkControllerPresent()
        {
#if USEController
            if (controller == OVRInput.Controller.None) controller = (OVRInput.Controller)controllerType;
            // hide controller if it is not being tracked
            bool trackedThisFrame = simulateControllerTracked || OVRInput.GetControllerPositionValid(controller);
            if (trackedThisFrame != controllerIsTracked)
            {
                controllerIsTracked = trackedThisFrame;
                this.gameObject.SetActive(controllerIsTracked);
            }
            return trackedThisFrame;
#else
            return false;
#endif
        }

        /// <summary>
        /// run each update and provide the controller data for network transfer
        /// </summary>
        void Update()
        {
            // get controller pose
#if USEController
            Vector3 controllerPosition = OVRInput.GetLocalControllerPosition(controller);
            Quaternion controllerRotation = OVRInput.GetLocalControllerRotation(controller);
#else
            Vector3 controllerPosition = Vector3.zero;
            Quaternion controllerRotation = Quaternion.identity;
#endif

            // apply controller pose
            if (useTracking)
            {
                transform.localPosition = Quaternion.Inverse(Camera.main.transform.localRotation) * controllerPosition - Quaternion.Inverse(Camera.main.transform.localRotation) * Camera.main.transform.localPosition;
                transform.localRotation = Quaternion.Inverse(Camera.main.transform.localRotation) * controllerRotation;
            }

            // store controller pose
            position = transform.position;
            rotation = transform.rotation;

            // network share controller pose
            playerId = IdAPI.GetPlayerId();
            ControllerAPI.ProvideMyController(playerId, position, rotation, controllerType);
        }
#endregion
    }
}
