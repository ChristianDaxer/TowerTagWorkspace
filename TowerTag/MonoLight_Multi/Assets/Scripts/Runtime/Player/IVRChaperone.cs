using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Interface that abstracts what is currently being accessed of
 * the OpenVR implementations from both SteamVR and Oculus
 */
public enum ChaperoneCalibrationState
{
	OK = 1,
	Warning = 100,
	Warning_BaseStationMayHaveMoved = 101,
	Warning_BaseStationRemoved = 102,
	Warning_SeatedBoundsInvalid = 103,
	Error = 200,
	Error_BaseStationUninitialized = 201,
	Error_BaseStationConflict = 202,
	Error_PlayAreaInvalid = 203,
	Error_CollisionBoundsInvalid = 204,
}
public interface IVRChaperone
{
    bool GetPlayAreaSize(ref float x, ref float y);

    ChaperoneCalibrationState GetCalibrationState();
	bool IsInitialized { get; }

}
