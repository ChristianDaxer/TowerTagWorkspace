using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MaterialDataDistributor))]
public class MaterialDataDistributorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        MaterialDataDistributor distributor = target as MaterialDataDistributor;
        if (distributor == null)
            return;

        if (distributor.PackedMeshReference == null)
            distributor.OnValidate();

        if (distributor.PackedMeshReference == null)
        {
            EditorGUILayout.LabelField(string.Format("No valid reference to {0} attached to GameObject: \"{0}\".", nameof(PackedMesh), distributor.gameObject.name));
            return;
        }

        MeshPackerAsset asset = distributor.PackedMeshReference.Asset;

        Material[] materials = asset.Materials;
        if (materials == null)
            return;

        bool[] materialMask = distributor.MaterialMask;

        bool changed = false;
        if (materialMask == null || materialMask.Length < materials.Length)
        {
            materialMask = new bool[materials.Length].Select(boolean => true).ToArray();
            changed = true;
        }

        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i] == null)
                continue;

            EditorGUILayout.BeginHorizontal();
            bool mask = EditorGUILayout.Toggle(materialMask[i], GUILayout.Width(25));
            if (mask != materialMask[i])
            {
                materialMask[i] = mask;
                changed = true;
            }
            EditorGUILayout.LabelField(materials[i].name);
            if (GUILayout.Button("Select"))
                Selection.activeObject = materials[i];
            EditorGUILayout.EndHorizontal();
        }

        if (changed)
            distributor.SetMaterialMask(materialMask);
    }
}
