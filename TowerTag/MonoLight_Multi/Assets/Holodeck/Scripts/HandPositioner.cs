/******************************************************************************
 * Copyright (C) Holodeck VR GmbH 2017 - 2020.                                *
 * SPREE Interactive SDK Version 4.0.27-RC                                    *
 ******************************************************************************/

using UnityEngine;

namespace Holodeck
{
    public class HandPositioner : MonoBehaviour
    {
        public HandIdentifier handIdentifier;
        public int ID;
        public HolodeckInputType inputType;
        public Quaternion orientation;
        public Vector3 position;
        public Gestures gesture;

        /// <summary>
        /// Calls the main methods each update
        /// </summary>
        void Update()
        {
            Position();
            Rotation();
            Gesture();
        }

        /// <summary>
        /// Sets the current position of the hand, called each update
        /// </summary>
        void Position()
        {
            this.transform.localPosition = InputAPI.getPosition(handIdentifier);
        }

        /// <summary>
        /// Sets the current rotation of the hand, called each update
        /// </summary>
        void Rotation()
        {
            this.transform.localRotation = InputAPI.getOrientation(handIdentifier);
        }

        /// <summary>
        /// Sets the current gesture of the hand, called each update
        /// </summary>
        void Gesture()
        {
            this.gesture = InputAPI.getGesture(handIdentifier);
        }
    }
}