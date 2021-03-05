using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEditor;

[CreateAssetMenu(fileName = "NewSceneComposer", menuName = "ScriptableObjects/New Scene Composer", order = 1)]
public class SceneComposer : ScriptableObject
{
    public string compositionName = "New Scene Composition";
    public SceneAsset[] scenes;

    public string StagedScenePath => $"Assets/Scenes/Staged/{compositionName} (DO NOT EDIT).unity";

    public void Load ()
    {
        EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(scenes[0]), OpenSceneMode.Single);
        for (int i = 1; i < scenes.Length; i++)
            EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(scenes[i]), OpenSceneMode.Additive);

        RebindFieldsToTarget[] rebinder = GameObject.FindObjectsOfType<RebindFieldsToTarget>();
        for (int i = 0; i < rebinder.Length; i++)
            rebinder[i].Rebind();
    }

    public SceneAsset StageAndOpen ()
    {
        var sceneAsset = Stage();
        EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(sceneAsset), OpenSceneMode.Single);
        return sceneAsset;
    }

    public SceneAsset Stage ()
    {
        AssetDatabase.DeleteAsset(StagedScenePath);

        Scene stageScene = EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(scenes[0]), OpenSceneMode.Single);
        Scene[] scenesToMerge = new Scene[scenes.Length - 1]; 
        for (int i = 1; i < scenes.Length; i++)
            scenesToMerge[i-1] = EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(scenes[i]), OpenSceneMode.Additive);

        for (int i = 0; i < scenesToMerge.Length; i++)
        {
            EditorSceneManager.MergeScenes(scenesToMerge[i], stageScene);
            EditorSceneManager.CloseScene(scenesToMerge[i], false);
        }

        if (!Directory.Exists(Path.GetDirectoryName(StagedScenePath)))
            Directory.CreateDirectory(Path.GetDirectoryName(StagedScenePath));

        EditorSceneManager.SaveScene(stageScene, StagedScenePath);
        // EditorSceneManager.CloseScene(stageScene, false);

        AssetDatabase.Refresh();

        return AssetDatabase.LoadAssetAtPath<SceneAsset>(StagedScenePath);
    }
}
