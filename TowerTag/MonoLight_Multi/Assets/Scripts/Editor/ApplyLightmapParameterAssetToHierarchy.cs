using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ApplyLightmapParameterAssetToHierarchy : EditorWindow
{
    public LightmapParameters lightmapParameters;
    public float scaleInLightmap = 1;
    [MenuItem("Unity/Apply Lightmap Parameter Asset to Selected Hierarchy")]
    public static void Open ()
    {
        var window = ApplyLightmapParameterAssetToHierarchy.CreateWindow<ApplyLightmapParameterAssetToHierarchy>();
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Scale in Lightmap");
        scaleInLightmap = EditorGUILayout.FloatField(scaleInLightmap);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Lightmap Parameters");
        lightmapParameters = EditorGUILayout.ObjectField(lightmapParameters, typeof(LightmapParameters), false) as LightmapParameters;
        EditorGUILayout.EndHorizontal();

        if (!GUILayout.Button("Apply"))
            return;

        if (lightmapParameters == null)
            return;

        GameObject gameObject = Selection.activeObject as GameObject;
        if (gameObject == null)
            return;

        gameObject.GetComponents<MeshRenderer>()
            .Concat(gameObject.GetComponentsInChildren<MeshRenderer>())
            .Where(meshRenderer => meshRenderer.gameObject.isStatic)
            .Distinct().ForEach(meshRenderer =>
            {
                SerializedObject so = new SerializedObject(meshRenderer);
                so.FindProperty("m_LightmapParameters").objectReferenceValue = lightmapParameters;
                so.FindProperty("m_ScaleInLightmap").floatValue = scaleInLightmap; 
                so.ApplyModifiedProperties();
            });
    }
}
