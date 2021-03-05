using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapPrefabModifier))]
public class MapPrefabModifierEditor : Editor
{
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        MapPrefabModifier myScript = (MapPrefabModifier)target;
        if (GUILayout.Button("Modify Prefab")) {
            myScript.ModifyPrefab();
        }
    }
}
