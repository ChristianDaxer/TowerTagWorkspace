using System.Collections;
using System.Diagnostics;
using TowerTagSOES;
using UnityEngine;

namespace TowerTag
{
    public static class ApplicationFunctions
    {
        public static IEnumerator Restart(bool withAutoStart, int delay = 0) {
            yield return null;
#if !UNITY_EDITOR
            if (SharedControllerType.VR)
            {
                if (withAutoStart)
                    Process.Start(Application.dataPath + "/../TowerTag.exe", "-vrmode OpenVR -vr -autostart");
                else
                    Process.Start(Application.dataPath + "/../TowerTag.exe", "-vrmode OpenVR -vr");
            }
            else if (SharedControllerType.NormalFPS)
            {
                if (withAutoStart)
                    Process.Start(Application.dataPath + "/../TowerTag.exe", "-vrmode None -fps -autostart");
                else
                    Process.Start(Application.dataPath + "/../TowerTag.exe", "-vrmode None -fps");
            }

            Application.Quit();
#else
            yield return null;
#endif
        }
    }
}