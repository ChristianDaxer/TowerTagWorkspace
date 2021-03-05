using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class OculusChaperone : IVRChaperone
{
    private OculusPlatformServices platformServices;

    public OculusChaperone(OculusPlatformServices _platformServices)
    {
        platformServices = _platformServices;
    }

    public bool IsInitialized => platformServices.InputSubsystem != null && platformServices.InputSubsystem.running;

    ChaperoneCalibrationState IVRChaperone.GetCalibrationState()
    {
        switch (platformServices.InputSubsystem.GetTrackingOriginMode())
        {
            case TrackingOriginModeFlags.TrackingReference:
            case TrackingOriginModeFlags.Floor:
            case TrackingOriginModeFlags.Device:
                return ChaperoneCalibrationState.OK;
            default:
                return ChaperoneCalibrationState.Error;
        }
    }

    bool IVRChaperone.GetPlayAreaSize(ref float x, ref float y)
    {
        List<Vector3> boundaryPoints = new List<Vector3>();
        if(!platformServices.InputSubsystem.TryGetBoundaryPoints(boundaryPoints))
        {
            return false;
        }
        if (boundaryPoints.Count == 0)
        {
            return false;
        }
        float minx = boundaryPoints[0].x, maxx = boundaryPoints[0].x;
        float miny = boundaryPoints[0].y, maxy = boundaryPoints[0].y;

        for (int i = 0; i < boundaryPoints.Count; i++)
        {
            minx = Mathf.Min(boundaryPoints[i].x, minx);
            maxx = Mathf.Max(boundaryPoints[i].x, maxx);

            miny = Mathf.Min(boundaryPoints[i].y, miny);
            maxy = Mathf.Max(boundaryPoints[i].y, maxy);
        }

        x = maxx - minx;
        y = maxy - miny;
        return true;
    }
}
