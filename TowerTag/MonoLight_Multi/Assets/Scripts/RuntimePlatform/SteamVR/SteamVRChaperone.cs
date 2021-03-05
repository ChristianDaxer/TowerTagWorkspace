using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class SteamVRChaperone : IVRChaperone
{
    bool IVRChaperone.IsInitialized => !OpenVR.Chaperone.IsNull();

    ChaperoneCalibrationState IVRChaperone.GetCalibrationState()
    {
        return (ChaperoneCalibrationState)OpenVR.Chaperone.GetCalibrationState();
    }

    bool IVRChaperone.GetPlayAreaSize(ref float x, ref float y)
    {
        return OpenVR.Chaperone.GetPlayAreaSize(ref x, ref y);
    }
}
