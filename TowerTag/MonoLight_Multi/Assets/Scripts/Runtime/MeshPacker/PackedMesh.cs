using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PackedMesh : MonoBehaviour
{
    [SerializeField] private MeshPackerAsset asset;
    public MeshPackerAsset Asset => asset;

    [SerializeField] private string guid;
    public string Guid => guid;

    [SerializeField] private MeshRenderer targetMeshRenderer;
    public MeshRenderer TargetMeshRenderer => targetMeshRenderer;
    public void SetTargetMeshRenderer(MeshRenderer targetMeshRenderer) => this.targetMeshRenderer = targetMeshRenderer;

#if UNITY_EDITOR
    public StaticEditorFlags unpackedMeshStaticFlags;
#endif
    public void SetData(string guid, MeshPackerAsset asset)
    {
        this.guid = guid;
        this.asset = asset;
    }


    public void ToggleUnpackedRenderers (bool toggle)
    {
        var meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();

        if (asset.FilterByType)
            meshRenderers = meshRenderers.Where(meshRenderer => asset.TypeWhiteList.Any(type => meshRenderer.GetComponent(type) != null)).ToArray();

        meshRenderers.ForEach(meshRenderer =>
        {
            meshRenderer.enabled = toggle;

#if UNITY_EDITOR
            GameObjectUtility.SetStaticEditorFlags(meshRenderer.gameObject, unpackedMeshStaticFlags);
#endif

            Collider collider = meshRenderer.GetComponent<Collider>();
            if (collider != null)
                collider.enabled = !asset.DisableCollider;
        });
    }
}
