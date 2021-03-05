using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OculusIdleDetector : IdleDetector
{
    protected override bool IsHeadsetInIdleStateImpl()
    {
        return OVRManager.OVRManagerinitialized && OVRManager.instance != null && !OVRManager.instance.isUserPresent;
    }
}
