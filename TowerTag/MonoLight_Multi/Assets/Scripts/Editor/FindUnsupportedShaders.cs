using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class FindUnsupportedShaders
{
    private static string[] GetBuildSceneShaderDependencies()
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        List<string> dependencies = new List<string>();
        dependencies.Clear();

        string[] allShaderPaths = AssetDatabase.FindAssets("t:Shader").Select(guid => AssetDatabase.GUIDToAssetPath(guid)).ToArray();

        for (int si = 0; si < sceneCount; si++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(si);

            string[] sceneShaderDependencies = AssetDatabase.GetDependencies(scenePath, true).Where(dependency => AssetDatabase.GetMainAssetTypeAtPath(dependency) == typeof(Shader)).ToArray();
            var shadersReferencedInScene = sceneShaderDependencies.Intersect(allShaderPaths);

            if (shadersReferencedInScene.Count() == 0)
                continue;

            if (dependencies != null)
                dependencies.AddRange(shadersReferencedInScene);
        }


        return dependencies.Distinct().ToArray();
    }

    public static void Find(string[] assetPaths)
    {
        List<Shader> shadersWithTesselation = new List<Shader>();
        List<string> pathToShaderWithTesselation = new List<string>(); 

        for (int i = 0; i < assetPaths.Length; i++)
        {
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(assetPaths[i]);
            string shaderSource;
            try
            {
                shaderSource = File.ReadAllText(assetPaths[i]);
            } catch (System.Exception exception)
            {
                Debug.LogException(exception);
                continue;
            }

            if (!shaderSource.Contains("tesselation") && !shaderSource.Contains("tessellate") && !shaderSource.Contains("geometry") && !shaderSource.Contains("domain"))
                continue;
            pathToShaderWithTesselation.Add(assetPaths[i]);
            shadersWithTesselation.Add(shader);

            EditorGUIUtility.PingObject(shader);
        }

        Debug.LogFormat("Found: {0} shaders with tesselation enabled.", shadersWithTesselation.Count);

        Selection.objects = shadersWithTesselation.ToArray();
    }

    [MenuItem("Unity/Find Quest Unsupported Shaders In Project")]
    public static void FindInProject ()
    {
        string[] assetPaths = AssetDatabase.FindAssets("t:Shader").Select(guid => AssetDatabase.GUIDToAssetPath(guid)).ToArray();
        Find(assetPaths);
    }

    [MenuItem("Unity/Find Quest Unsupported Shaders In Build Scenes")]
    public static void FindInBuildScenes ()
    {
        string[] assetPaths = GetBuildSceneShaderDependencies();
        Find(assetPaths);
    }
}
