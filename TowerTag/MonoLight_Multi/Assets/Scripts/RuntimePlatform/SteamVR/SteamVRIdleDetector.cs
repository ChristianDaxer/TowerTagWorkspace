using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class SteamVRIdleDetector : IdleDetector
{
    protected override bool IsHeadsetInIdleStateImpl()
    {
        return (OpenVR.System != null) && OpenVR.System.GetTrackedDeviceActivityLevel(OpenVR.k_unTrackedDeviceIndex_Hmd)
            .Equals(EDeviceActivityLevel.k_EDeviceActivityLevel_Standby);
    }
}
