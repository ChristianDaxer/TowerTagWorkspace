using UnityEngine;
using UnityEngine.Serialization;

public class SimpleLODGroup : MonoBehaviour {
    [Header("BoundingSphere")]
    [FormerlySerializedAs("center"), Tooltip("Center of Bounding sphere in local coordinates."), SerializeField]
    private Vector3 _center;

    [FormerlySerializedAs("radius"), Tooltip("Radius of Bounding sphere in world coordinates."), SerializeField]
    private float _radius;

    [Header("LOD Levels")]
    [FormerlySerializedAs("LODLevelParents")]
    [Tooltip("LODParents (group all objects of a certain LOD Level under the respective LODParent), " +
             "index of LODParent in array has to match the LODLevel of the LODParent ([LOD_0, LOD_1 .. LOD_N])!"),
     SerializeField]
    private GameObject[] _lodLevelParents;

    public GameObject[] LODLevelParents => _lodLevelParents;

    /// <summary>
    /// Current set LOD Level.
    /// </summary>
    public int CurrentLODLevel => _currentLOD;

    [SerializeField, HideInInspector] private int _currentLOD;

    /// <summary>
    /// Switch LODGroup to given LOD Level (deactivate currently active LODParent and activate LODParent associated to new LODLevel).
    /// </summary>
    /// <param name="lodLevel">LODParent index of LODParents array to activate.</param>
    public void SwitchToLOD(int lodLevel) {
        if (_lodLevelParents == null) {
            Debug.LogError("SimpleLODGroup.SwitchToLOD: LODs Array is null! on GameObject: " + gameObject.name);
            return;
        }

        if (lodLevel >= 0 && lodLevel < _lodLevelParents.Length) {
            foreach (GameObject lod in _lodLevelParents) {
                lod.SetActive(false);
            }

            // cache new Level (so we know which LODParent to deactivate later)
            _currentLOD = lodLevel;

            // activate new LODParent
            if (_lodLevelParents[lodLevel] != null) {
                _lodLevelParents[lodLevel].SetActive(true);
            }
        }
        else {
            Debug.LogError("SimpleLODGroup.SwitchToLOD: LOD Level to set is not valid (LOD to set: " + lodLevel +
                           ", number of LOD elements: " + _lodLevelParents.Length + " )! on GameObject: " +
                           gameObject.name);
        }
    }

    /// <summary>
    /// Draw BoundingSphere in Editor if current LODGroup gameObject is selected.
    /// </summary>
    private void OnDrawGizmosSelected() {
        Gizmos.DrawWireSphere(transform.TransformPoint(_center), _radius);
    }
}