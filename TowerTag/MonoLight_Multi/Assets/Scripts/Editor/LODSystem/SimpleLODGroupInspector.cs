using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Custom Inspector for SimpleLODGroup Component.
/// Draws Buttons to select LODLevel (activate/deactivate corresponding LODParent GameObjects) for this LODGroup.
/// </summary>
[CustomEditor(typeof(SimpleLODGroup))]
public class SimpleLODGroupInspector : Editor
{
    public override void OnInspectorGUI()
    {
        var simpleLODGroup = (SimpleLODGroup)target;

        DrawDefaultInspector();

        // Draw Buttons to select LODLevel
        GUILayout.BeginHorizontal();
        {
            // remember old/default UI-Color
            Color defaultColor = GUI.color;
            for (var i = 0; i < simpleLODGroup.LODLevelParents.Length; i++)
            {
                // switch Color to green for currently set LODLevel
                GUI.color = (i == simpleLODGroup.CurrentLODLevel) ? Color.green : defaultColor;

                // switch to LODLevel in SimpleLODGroup Component
                if (GUILayout.Button("LOD_" + i))
                {
                    simpleLODGroup.SwitchToLOD(i);
                    EditorUtility.SetDirty(simpleLODGroup);
                    EditorSceneManager.MarkSceneDirty(simpleLODGroup.gameObject.scene);
                }
            }
            // reset UI Color to default
            GUI.color = defaultColor;
        }
        GUILayout.EndHorizontal();
    }
}
