using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "PlatformSceneReconfigureTaskTransferLightmaps", menuName = "ScriptableObjects/Platform Build Tasks/Platform Scene Reconfigure Task Transfer Lightmaps", order = 1)]
public class PlatformSceneReconfigureTaskTransferLightmaps : PlatformSceneReconfigureTaskScriptableObject {
    private string sceneTaskDescriptionFormat = "Transferring lightmaps from source scene: \"{0}\" to staged scene: \"{1}\".";
    private string sceneTaskDescription = "";
    public override string SceneTaskDescription => sceneTaskDescription;

    // Currently this does not seem to work.
    public override IEnumerator ReconfigureScene(HomeTypes homeType, SceneWrapper sceneWrapper, Action<IPlatformReconfigureTaskDescriptor> startTaskCallback, Action<bool> taskCallback)
    {
        yield break; // Exiting out early since, I'd need to take a different approach.

        string[] sourceAssetPaths = sceneWrapper.SourceScenePaths;
        if (sourceAssetPaths == null || sourceAssetPaths.Length == 0)
        {
            Debug.LogErrorFormat("Unable to transfer lightmaps for staged scene: \"{0}\".", sceneWrapper.StagedScenePath);

            if (taskCallback != null)
                taskCallback(false);
            yield break;
        }

        string firstSourceAssetPath = sourceAssetPaths[0];

        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(firstSourceAssetPath);
        string sourceAssetContainingFolder = Path.GetDirectoryName(firstSourceAssetPath);
        string lightmapAssetPath = Path.Combine(sourceAssetContainingFolder, fileNameWithoutExtension, "LightingData.asset");

        if (!File.Exists(lightmapAssetPath))
        {
            Debug.LogWarningFormat("There are no lightamps associated with source scene asset: \"{0}\".", firstSourceAssetPath);
            if (taskCallback != null)
                taskCallback(true);
            yield break;
        }

        sceneTaskDescription = string.Format(sceneTaskDescriptionFormat, firstSourceAssetPath, sceneWrapper.StagedScenePath);

        string stagedSceneFileNameWithoutExtension = Path.GetFileNameWithoutExtension(sceneWrapper.StagedScenePath);
        string stagedSceneLightmapFolderPath = Path.Combine(Path.GetDirectoryName(sceneWrapper.StagedScenePath), stagedSceneFileNameWithoutExtension);
        string stagedSceneLightingDataFilePath = Path.Combine(stagedSceneLightmapFolderPath, "LightingData.asset");

        if (!Directory.Exists(stagedSceneLightmapFolderPath))
            Directory.CreateDirectory(stagedSceneLightmapFolderPath);

        var lightingDataAsset = AssetDatabase.LoadAssetAtPath<LightingDataAsset>(lightmapAssetPath);
        var copyOfLightingDataAsset = Instantiate(lightingDataAsset);
        Lightmapping.lightingDataAsset = copyOfLightingDataAsset;
        AssetDatabase.CreateAsset(copyOfLightingDataAsset, stagedSceneLightingDataFilePath);

        Debug.LogFormat("Successfully transferred lightmaps from source scene: \"{0}\" to staged scene: \"{1}\".", firstSourceAssetPath, sceneWrapper.StagedScenePath);
    }
}
