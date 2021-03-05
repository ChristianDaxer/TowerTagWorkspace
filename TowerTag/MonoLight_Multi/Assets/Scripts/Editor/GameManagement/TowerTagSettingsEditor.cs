using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TowerTagSettings))]
public class TowerTagSettingsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        var towerTagSettings = serializedObject.targetObject as TowerTagSettings;
        if(towerTagSettings == null)
            return;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("General", EditorStyles.boldLabel);
        bool previousBasicMode = TowerTagSettings.BasicMode;
        TowerTagSettings.BasicMode = EditorGUILayout.Toggle("Basic Mode", TowerTagSettings.BasicMode);
        TowerTagSettings.Hologate = EditorGUILayout.Toggle("Hologate", TowerTagSettings.Hologate);
        if (towerTagSettings != previousBasicMode) {
            EditorUtility.SetDirty(towerTagSettings);
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }
        bool previousHomeMode = TowerTagSettings.Home;
        TowerTagSettings.Home = EditorGUILayout.Toggle("Home Mode", TowerTagSettings.Home);
        if (towerTagSettings != previousHomeMode) {
            EditorUtility.SetDirty(towerTagSettings);
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }

        if (TowerTagSettings.Home)
        {
            HomeTypes previousHomeType = TowerTagSettings.HomeType;
            TowerTagSettings.HomeType = (HomeTypes) EditorGUILayout.EnumPopup(TowerTagSettings.HomeType);
            if (TowerTagSettings.HomeType != previousHomeType) {
                EditorUtility.SetDirty(towerTagSettings);
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            }
        }
        else if(TowerTagSettings.HomeType != HomeTypes.Undefined)
        {
            TowerTagSettings.HomeType = HomeTypes.Undefined;
            EditorUtility.SetDirty(towerTagSettings);
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }
    }
}
