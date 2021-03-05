using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Custom Inspector for LODSwitcher Component.
/// Draws Buttons to select LODLevel for all LODGroups set in LODSwitcherComponent.
/// Draws Button to search for LODGroups in scene and set it in the LODSwitcherComponent.
/// </summary>
[CustomEditor(typeof(SimpleLODSwitcher))]
public class SimpleLODSwitcherInspector : Editor
{
    /// <summary>
    /// Draw Custom Inspector UI (additional to default Inspector UI)
    /// Draws Buttons to select LODLevel for all LODGroups set in LODSwitcherComponent.
    /// Draws Button to search for LODGroups in scene and set it in the LODSwitcherComponent.
    /// </summary>
    public override void OnInspectorGUI()
    {
        var lodSwitcher = (SimpleLODSwitcher)target;

        DrawDefaultInspector();

        GUILayout.Space(20);

        // reset GUI.changed, to check later if a GUI element changed value
        GUI.changed = false;
        // Draw Buttons to select LODLevel for all LODGroups set in LODSwitcherComponent.
        GUILayout.BeginHorizontal();
        {
            // remember old/default UI-Color
            Color defaultColor = GUI.color;
            for (int i = 0; i < lodSwitcher.MaxLodLevels; i++)
            {
                // switch Color to green for currently set LODLevel
                GUI.color = (i == lodSwitcher.CurrentLodLevel) ? Color.green : defaultColor;

                // switch to LODLevel for all LODGroups set in LODSwitcher Component
                if (GUILayout.Button("LOD_" + i))
                {
                    lodSwitcher.SwitchAllLodGroupsToLodLevel(i);
                    SetDirty(lodSwitcher);
                }
            }
            // reset UI Color to default
            GUI.color = defaultColor;
        }
        GUILayout.EndHorizontal();
    }

    private static void SetDirty(SimpleLODSwitcher lodSwitcher)
    {
        EditorUtility.SetDirty(lodSwitcher);
        EditorSceneManager.MarkSceneDirty(lodSwitcher.gameObject.scene);
    }
}
