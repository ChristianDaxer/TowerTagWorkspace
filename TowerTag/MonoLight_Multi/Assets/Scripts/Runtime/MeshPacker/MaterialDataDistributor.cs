using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[DefaultExecutionOrder(-1)]
[RequireComponent(typeof(PackedMesh))]
[ExecuteAlways]
public class MaterialDataDistributor : MonoBehaviour
{
    [HideInInspector] [SerializeField] private PackedMesh packMesh;
    public PackedMesh PackedMeshReference => packMesh;

    private Material[] materialInstances;

    [HideInInspector] [SerializeField] private bool[] materialMask;
    public bool[] MaterialMask => materialMask;

    public void SetMaterialMask(bool[] mask)
    {
        materialMask = mask;
        #if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        #endif
    }

    private void OnEnable()
    {
        packMesh = GetComponent<PackedMesh>();
    }

    private void CollectMaterialInstances () 
    {
        Material[] materials = Application.isPlaying ? packMesh.TargetMeshRenderer.materials : packMesh.TargetMeshRenderer.sharedMaterials;
        List<Material> selectedMaterials = new List<Material>();

        if (materialMask == null || materialMask.Length == 0)
            materialMask = Enumerable.Repeat(true, materials.Length).ToArray();

        for (int i = 0; i < materials.Length; i++)
        {
            if (!materialMask[i])
                continue;

            selectedMaterials.Add(materials[i]);
            materials[i].SetInt("_TotalInstanceCountInCombinedMesh", packMesh.Asset.InstanceCount);
        }

        materialInstances = selectedMaterials.ToArray();
    }

    public void OnValidate() => packMesh = GetComponent<PackedMesh>();

    protected virtual void OnAwake(int intanceCount) {}
    private void Awake()
    {
        if (!Application.isPlaying)
        {
            GetComponentsInChildren<MonoBehaviour>()
                .Concat(GetComponents<MonoBehaviour>())
                .Where(monoBehaviour => monoBehaviour is ICombinedMesh)
                .Select(monoBehaviour => monoBehaviour as ICombinedMesh)
                .ForEach(combinedMesh => combinedMesh.ApplyDistributor(this));

            packMesh = GetComponent<PackedMesh>();
            #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            #endif
        }

        if (packMesh.TargetMeshRenderer == null && enabled)
        {
            Debug.LogErrorFormat("{0} exists on GameObject: \"{1}\". However, no valid reference to target combined MeshRenderer.", GetType(), gameObject.name);
            enabled = false;
        }

        if (packMesh.Asset.InstanceCount == 0 && enabled)
        {
            Debug.LogErrorFormat("{0} exists on GameObject: \"{1}\". However, the instance count in the combined mesh is currently 0.", GetType(), gameObject.name);
            enabled = false;
        }

        if (!enabled)
            return;

        CollectMaterialInstances();
        OnAwake(packMesh.Asset.InstanceCount);
    }

    protected virtual void OnLateUpdate(Material material) {}
    private void LateUpdate()
    {
        if (packMesh == null || packMesh.Asset == null)
            return;

        if (materialInstances == null)
        {
            CollectMaterialInstances();
            if (materialInstances == null) { 
                Debug.LogErrorFormat("No material instances assigned to {0} attached to GameObject: \"{1}\".", nameof(MaterialDataDistributor), gameObject.name);
                enabled = false;
                return;
            }
        }

        for (int i = 0; i < materialInstances.Length; i++)
        {
            materialInstances[i].SetInt("_TotalInstanceCountInCombinedMesh", packMesh.Asset.InstanceCount);
            OnLateUpdate(materialInstances[i]);
        }
    }
}
