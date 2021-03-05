using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PlatformLayerTarget {
    [SerializeField] public RuntimePlatform runtimePlayer;
    [SerializeField] public LayerMask layerMask;
}

public class PlatformLayerSwitcher : MonoBehaviour {

    [SerializeField] private PlatformLayerTarget[] serializedPlatformLayerTargets;

    private void Awake() {
        if (serializedPlatformLayerTargets == null || serializedPlatformLayerTargets.Length == 0) {
            enabled = false;
            return;
        }

        for (int i = 0; i < serializedPlatformLayerTargets.Length; i++) {
            if (serializedPlatformLayerTargets[i].runtimePlayer != Application.platform)
                continue;

            int layerIndex = (int) Mathf.Log(serializedPlatformLayerTargets[i].layerMask.value, 2);
            Debug.LogFormat("Applying layer: \"{0}\" to hierarchy: \"{1}\".", LayerMask.LayerToName(layerIndex), gameObject.name);

            var transforms = transform.GetComponentsInChildren<Transform>();
            for (int ti = 0; ti < transforms.Length; ti++)
                transforms[ti].gameObject.layer = layerIndex;

            break;
        }
    }
}
