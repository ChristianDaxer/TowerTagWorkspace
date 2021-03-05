using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(SceneCompositionManager))]
public class SceneCompositionManagerEditor : Editor
{
    public void Draw (SceneCompositionManager manager)
    {
        if (GUILayout.Button("Update Config List"))
            manager.Refresh();

        if (manager.configs != null)
        {
            for (int i = 0; i < manager.configs.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(manager.configs[i].sceneComposer.compositionName);
                HomeTypes platform = (HomeTypes)EditorGUILayout.EnumPopup("Platform", manager.configs[i].platfrom);
                if (platform != manager.configs[i].platfrom)
                {
                    manager.configs[i].platfrom = platform;
                    EditorUtility.SetDirty(manager);
                    AssetDatabase.SaveAssets();
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        else
        {
            EditorGUILayout.LabelField("No scene composers.", EditorStyles.boldLabel);
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        SceneCompositionManager manager = target as SceneCompositionManager;
        Draw(manager);
    }
}
#endif

public struct SceneComposerConfig
{
    public HomeTypes platfrom;
    public SceneComposer sceneComposer;
}

[CreateAssetMenu(fileName = "NewSceneCompositionManager", menuName = "ScriptableObjects/New Scene Composition Manager", order = 1)]
public class SceneCompositionManager : ScriptableObject
{
    [HideInInspector]
    public SceneComposerConfig[] configs;

    public void Refresh ()
    {
        if (configs == null)
            configs = new SceneComposerConfig[0];
        List<SceneComposer> allSceneComposers = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(SceneComposer).Name)).Select(guid => AssetDatabase.LoadAssetAtPath<SceneComposer>(AssetDatabase.GUIDToAssetPath(guid))).ToList();

        List<SceneComposerConfig> serializedConfigs = configs.ToList();
        List<SceneComposer> serializedSceneComposers = serializedConfigs.Select(config => config.sceneComposer).ToList();

        var serializedIndices = serializedConfigs.Select(config => config.sceneComposer).Select(sceneComposer => allSceneComposers.IndexOf(sceneComposer)).ToArray();

        for (int i = 0; i < serializedIndices.Length; i++)
        {
            if (serializedIndices[i] == -1)
                serializedConfigs.RemoveAt(serializedIndices[i]);
        }

        for (int i = 0; i < allSceneComposers.Count; i++)
            if (!serializedSceneComposers.Contains(allSceneComposers[i]))
                serializedConfigs.Add(new SceneComposerConfig { platfrom = HomeTypes.Undefined, sceneComposer = allSceneComposers[i] });

        configs = serializedConfigs.ToArray();
    }
}

