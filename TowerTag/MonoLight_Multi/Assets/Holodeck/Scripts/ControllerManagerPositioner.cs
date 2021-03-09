using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerManagerPositioner : MonoBehaviour
{
    void Update()
    {
        // should be invariant to parent localRotation and recline correction
        Quaternion correctedOrientation = Quaternion.Inverse(transform.parent.localRotation) * (transform.parent.localRotation * Quaternion.Inverse(Pvr_UnitySDKSensor.Instance.HeadPose.Orientation));
        this.transform.localRotation = correctedOrientation;
        // static offset into neck
        //TODO: magic numbers
        this.transform.localPosition = new Vector3(0.0f, -0.0878f, -0.9961f) * 0.11f;
    }
}
