using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PackedMesh))]
public class PackedMeshEditor : Editor
{
    private Type[] cachedTypesDerrivedFromMonoBehaviour;
    private bool showCachedTypes;

    private List<Type> cachedTypeWhitelist;
    private bool[] cachedTypeMask;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        PackedMesh packedMesh = target as PackedMesh;
        if (packedMesh == null || packedMesh.Asset == null)
            return;

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Disable Unpacked Renderers"))
            packedMesh.ToggleUnpackedRenderers(false);
        if (GUILayout.Button("Enable Unpacked Renderers"))
            packedMesh.ToggleUnpackedRenderers(true);

        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Pack"))
            MeshPackerAsset.Pack(packedMesh.gameObject);

        if (GUILayout.Button("Select Materials"))
            Selection.objects = packedMesh.Asset.Materials;

        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Vertex Transformation Type", GUILayout.Width(175));
            MeshPackerAsset.VertexTransformationType vertexTransformationType = (MeshPackerAsset.VertexTransformationType)EditorGUILayout.EnumPopup(packedMesh.Asset.TransformationType);
            if (vertexTransformationType != packedMesh.Asset.TransformationType)
                packedMesh.Asset.TransformationType = vertexTransformationType;

            EditorGUILayout.EndHorizontal();
        }

        {
            EditorGUILayout.BeginHorizontal();

            bool disableCollider = EditorGUILayout.Toggle(packedMesh.Asset.DisableCollider, GUILayout.Width(30));
            if (disableCollider != packedMesh.Asset.DisableCollider)
                packedMesh.Asset.DisableCollider = disableCollider;

            EditorGUILayout.LabelField("Disable Collider");
            EditorGUILayout.EndHorizontal();
        }

        {
            EditorGUILayout.BeginHorizontal();

            bool applyMeshCollider = EditorGUILayout.Toggle(packedMesh.Asset.ApplyMeshCollider, GUILayout.Width(30));
            if (applyMeshCollider != packedMesh.Asset.ApplyMeshCollider)
                packedMesh.Asset.ApplyMeshCollider = applyMeshCollider;

            EditorGUILayout.LabelField("Apply Mesh Collider");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
        }

        {
            bool filterByType = EditorGUILayout.Toggle(packedMesh.Asset.FilterByType, GUILayout.Width(30));
            if (filterByType != packedMesh.Asset.FilterByType)
                packedMesh.Asset.FilterByType = filterByType;

            EditorGUILayout.LabelField("Filter By Type", GUILayout.Width(100));
            if (GUILayout.Button("Cache MonoBehaviour Targets"))
            {
                cachedTypesDerrivedFromMonoBehaviour = MeshPackerAsset.ListPotentialTargets(packedMesh.gameObject);
                cachedTypeMask = packedMesh.Asset.BuildTypeMask(cachedTypesDerrivedFromMonoBehaviour);
                cachedTypeWhitelist = packedMesh.Asset.TypeWhiteList.ToList();
            }
            EditorGUILayout.EndHorizontal();
        }

        if (cachedTypesDerrivedFromMonoBehaviour != null)
        {
            if (GUILayout.Button($"List MonoBehaviour Targets ({cachedTypesDerrivedFromMonoBehaviour.Length})"))
                showCachedTypes = !showCachedTypes;

            if (showCachedTypes)
            {
                for (int i = 0; i < cachedTypesDerrivedFromMonoBehaviour.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();

                    bool toggled = EditorGUILayout.Toggle(cachedTypeMask[i], GUILayout.Width(50));
                    if (toggled != cachedTypeMask[i])
                    {
                        if (toggled)
                            cachedTypeWhitelist.Add(cachedTypesDerrivedFromMonoBehaviour[i]);
                        else if (cachedTypeWhitelist.Contains(cachedTypesDerrivedFromMonoBehaviour[i]))
                            cachedTypeWhitelist.RemoveAll(type => type == cachedTypesDerrivedFromMonoBehaviour[i]);

                        packedMesh.Asset.SetTypeWhiteList(cachedTypeWhitelist.ToArray());
                        cachedTypeMask[i] = toggled;
                    }

                    EditorGUILayout.LabelField(cachedTypesDerrivedFromMonoBehaviour[i].FullName);
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
    }
}
