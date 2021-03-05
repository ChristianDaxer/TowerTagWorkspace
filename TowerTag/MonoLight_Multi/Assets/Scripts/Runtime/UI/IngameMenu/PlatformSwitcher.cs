using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Home.UI
{
    public class PlatformSwitcher : MonoBehaviour
    {
        // these ifdefs avoid referencing assets not available in other platforms
        // basically having ifdef switches here avoids needing ifdefs in the rest of the code
        // and assets not intended for the given platform won't be referenced in the build.
#if UNITY_STANDALONE || UNITY_EDITOR
        [SerializeField] private GameObject _steamAlternative;
        [SerializeField] private GameObject _viveportAlternative;
#endif
#if UNITY_ANDROID || UNITY_STANDALONE || UNITY_EDITOR
        [SerializeField] private GameObject _oculusAlternative;        
#endif


        void Awake()
        {
            switch(TowerTagSettings.HomeType)
            {
#if UNITY_STANDALONE || UNITY_EDITOR
                case HomeTypes.SteamVR:
                    if (_steamAlternative)
                    {
                        InstantiateWrapper.InstantiateWithMessage(_steamAlternative, transform);
                    }
                    return;
                case HomeTypes.Viveport:
                    if (_viveportAlternative)
                    {
                        InstantiateWrapper.InstantiateWithMessage(_viveportAlternative, transform);
                    }
                    return;
#endif
#if UNITY_ANDROID || UNITY_STANDALONE || UNITY_EDITOR
                case HomeTypes.Oculus:
                    if (_oculusAlternative)
                    {
                        InstantiateWrapper.InstantiateWithMessage(_oculusAlternative, transform);
                    }
                    return;
#endif
                default:
                    Debug.LogError($"[${gameObject.name}]: Unexpected HomeType ${TowerTagSettings.HomeType} - alternative not instantiated!");
                    return;
            }
        }
    }

}
