/******************************************************************************
 * Copyright (C) Holodeck VR GmbH 2017 - 2020.                                *
 * SPREE Interactive SDK Version 4.0.27-RC                                    *
 ******************************************************************************/

using UnityEngine;
using System.Collections.Generic;

namespace Holodeck
{
    /// <summary>
    /// Positions the ui elements in a ring around the camera
    /// </summary>
    internal class GuiPositioner : MonoBehaviour
    {
        public float menuDistance = 1;
        List<Canvas> uis = new List<Canvas>();
        // Use this for initialization
        void Start()
        {
            Canvas[] initialGuis = GetComponentsInChildren<Canvas>();
            uis.AddRange(initialGuis);
            PositionUis();
        }

        void Update()
        {
            //positions the ui according to camera
            transform.parent.position = Camera.main.transform.position;
        }

        /// <summary>
        /// Adds this to the list of uis and repositions the elements
        /// </summary>
        /// <param name="ui">the new ui</param>
        public void AddCanvas(Canvas ui)
        {
            if (!ui.gameObject.activeSelf) return;
            uis.Add(ui);
            PositionUis();
        }

        /// <summary>
        /// Removes this to the list of uis and repositions the elements
        /// </summary>
        /// <param name="ui">the new ui</param>
        public void RemoveCanvas(Canvas ui)
        {
            uis.Remove(ui);
            PositionUis();
            ui.gameObject.SetActive(false);
        }


        /// <summary>
        /// Positions the uis in a circle
        /// </summary>
        void PositionUis()
        {
            float uiAngle = 45;
            for (int m = 0; m < uis.Count; m++)
            {
                float currentAngle = (m - uis.Count / 2.0f) * uiAngle;
                float radius = menuDistance;
                Canvas ui = uis[m];
                ui.transform.SetParent(transform);
                Vector3 pos;
                pos.x = radius * Mathf.Sin(currentAngle * Mathf.Deg2Rad);
                pos.z = radius * Mathf.Cos(currentAngle * Mathf.Deg2Rad);
                pos.y = 0;

                ui.transform.localPosition = pos;
                ui.transform.localRotation = Quaternion.AngleAxis(currentAngle, Vector3.up);
            }
        }
    }
}