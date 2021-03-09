/******************************************************************************
 * Copyright (C) Holodeck VR GmbH 2017 - 2020.                                *
 * SPREE Interactive SDK Version 4.0.27-RC                                    *
 ******************************************************************************/

using UnityEngine;
using Holodeck;

public class Controllerwalk : MonoBehaviour
{
    public float[] stages;
    int currstage = 0;

    public KeyCode primaryFloorChange;
    public KeyCode secondaryFloorChange;

    public float mouseSensitivity = 100.0f;
    public float clampAngle = 80.0f;

    private float rotY = 0.0f; // rotation around the up/y axis
    private float rotX = 0.0f; // rotation around the right/x axis


    /// <summary>
    /// Sets the initial local rotation of the GameObject
    /// </summary>
    void Start()
    {
        Vector3 rot = transform.localRotation.eulerAngles;
        rotY = rot.y;
        rotX = rot.x;
    }

    /// <summary>
    /// Checks every Update for movement input
    /// </summary>
    void Update()
    {
        if (GeneralAPI.GetOfflineMode()) UpdateControllerMovement();
    }

    /// <summary>
    /// Camera movement by controller or keyboard input
    /// </summary>
    void UpdateControllerMovement()
    {
        if (Camera.main == null || Camera.main.gameObject == null) return;
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Vector3 currRoteuler = Camera.main.transform.localRotation.eulerAngles;
            rotX = currRoteuler.x;
            rotY = currRoteuler.y;
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = -Input.GetAxis("Mouse Y");

            rotY += mouseX * mouseSensitivity * Time.deltaTime;
            rotX += mouseY * mouseSensitivity * Time.deltaTime;

            Quaternion localRotation = Quaternion.Euler(rotX, rotY, 0.0f);
            Camera.main.gameObject.transform.localRotation = localRotation;
        }
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        float turn = 0.0f;

        if (Input.GetKey(KeyCode.Q))
        {
            turn -= 1.0f;
        }
        if (Input.GetKey(KeyCode.E))
        {
            turn += 1.0f;
        }

        this.transform.localRotation *= Quaternion.Euler(0.0f, turn * 90.0f * Time.deltaTime, 0.0f);

        Vector3 Movevector = (Camera.main.gameObject.transform.forward * v + Camera.main.gameObject.transform.right * h);
        if (Movevector.magnitude > 1.0f)
        {
            Movevector.Normalize();
        }

        this.transform.localPosition += new Vector3(Movevector.x * Time.deltaTime * 1.5f, 0, Movevector.z * Time.deltaTime * 1.5f);

        if (Input.GetKeyDown(primaryFloorChange) || Input.GetKeyDown(secondaryFloorChange))
        {
            if (stages.Length > 0)
            {
                currstage++;
                currstage %= stages.Length;
                this.transform.localPosition = new Vector3(this.transform.localPosition.x, stages[currstage], this.transform.localPosition.z);
            }
        }
    }
}


