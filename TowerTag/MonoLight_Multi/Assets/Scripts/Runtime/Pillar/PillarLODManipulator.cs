using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class PillarLODManipulator : MonoBehaviour
{
    [SerializeField] private LODGroup[] lodGroups;

    private void OnEnable()
    {
        // Debug.Log("Setting LOD reference point.");
        for (int i = 0; i < lodGroups.Length; i++)
            lodGroups[i].localReferencePoint = lodGroups[i].transform.InverseTransformPoint(transform.position);
    }
}
