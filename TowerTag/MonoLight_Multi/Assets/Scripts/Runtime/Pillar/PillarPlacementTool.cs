using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class PillarPlacementTool : MonoBehaviour {
    [SerializeField, Tooltip("Place the Pillar Prefab here")]
    private GameObject _pillarPrefab;

    [SerializeField, Tooltip("Start placement at this Point")]
    private Vector3 _startPoint = new Vector3(0, 0, 0);

    [SerializeField, Tooltip("How many Prefabs will be placed")]
    private int _count;

    [SerializeField, Tooltip("Range between Pillars")]
    private int _range;

    /// <summary>
    /// Context Menu-Method to Start the replacing
    /// </summary>
    [ContextMenu("Place Pillars in Scene")]
    public void FindAndReplacePillar() {
#if UNITY_EDITOR
        if (_pillarPrefab == null) {
            Debug.LogError("Check Serialized Field: no Pillar Prefab found.");
            return;
        }

        IEnumerable<Pillar> list = FindObjectsOfType<Pillar>().Where(pillar => pillar.transform.parent == transform);
        if (list.Any()) {
            Debug.LogWarning("PillarPlacementTool: Please delete old Pillars.");
            return;
        }

        InstantiatePillar(_startPoint);
#endif
    }

    private void InstantiatePillar(Vector3 start) {
#if UNITY_EDITOR
        var spawnPillar = false;
        var modulo = 0;
        int divisor = _count;
        var row = 0;
        while (divisor >= 1) {
            divisor /= 2;
            modulo++;
        }

        for (var i = 0; i < _count; i++) {
            if (i % modulo == 0 && spawnPillar) {
                row++;
            }

            var position = new Vector3(start.x + row * _range, start.y, start.z + (i % modulo) * _range);
            var newPillar = PrefabUtility.InstantiatePrefab(_pillarPrefab) as GameObject;
            if (!spawnPillar) {
                if (newPillar != null)
                    newPillar.GetComponent<Pillar>().IsSpawnPillar = true;
                spawnPillar = true;
            }

            if (newPillar == null)
                continue;
            newPillar.transform.SetParent(transform);
            newPillar.transform.localPosition = position;
        }

#endif
    }
}