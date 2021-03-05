using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Profiling;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class FlipAllMaterialsToShaderX : EditorWindow
{
    public enum SortingMode
    {
        DiagonalSize,
        CompressionSize
    }

    [MenuItem("Unity/Flip Specified Materials to Shader")]
    private static void Open ()
    {
        FlipAllMaterialsToShaderX window = FlipAllMaterialsToShaderX.GetWindow<FlipAllMaterialsToShaderX>();
        window.Show();
    }

    private string[] FindTextures ()
    {
        // Get all the materials in the project.
        List<string> allMaterials = AssetDatabase.FindAssets("t:Material").Select(eachMaterialGuid => AssetDatabase.GUIDToAssetPath(eachMaterialGuid)).ToList();

        // Get the number of scenes in the build settings.
        int sceneCount = EditorSceneManager.sceneCountInBuildSettings;
        List<string> materialsInScenes = new List<string>();

        // Loop through the scenes.
        for (int si = 0; si < sceneCount; si++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(si);

            // Get all dependencies required by scene.
            string[] sceneDependencies = AssetDatabase.GetDependencies(scenePath, true);

            // Intersect all scene dependencies with all textures to just give you materials used in the scene.
            var materialsReferencedIntoScenes = sceneDependencies.Intersect(allMaterials);
            if (materialsReferencedIntoScenes.Count() > 0)
                materialsInScenes.AddRange(materialsReferencedIntoScenes);
        }

        // Remove any duplicates and return an array.
        return materialsInScenes.Distinct().ToArray();
    }

    string[] materials;
    string[] searchMaterials;
    private Vector2 scrollPosition;

    private string search;
    private bool wholeWord;
    private bool exclude;
    private Shader targetShader;
    private void OnGUI()
    {
        if (GUILayout.Button("Find Materials"))
            searchMaterials = materials = FindTextures();

        if (searchMaterials != null)
        {
            EditorGUILayout.BeginHorizontal();
            string newSearch = EditorGUILayout.TextField(search);
            EditorGUILayout.LabelField("Whole Word", GUILayout.Width(75));
            wholeWord = EditorGUILayout.Toggle(wholeWord, GUILayout.Width(25));
            EditorGUILayout.LabelField("Exclude", GUILayout.Width(75));
            exclude = EditorGUILayout.Toggle(exclude, GUILayout.Width(25));
            EditorGUILayout.EndHorizontal();
            if (newSearch != search)
            {
                search = newSearch;
                searchMaterials = materials.Where(materialPath =>
                {
                    Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                    return wholeWord ?
                        (exclude ? material.shader.name.ToLower() != search.ToLower() : material.shader.name.ToLower() == search.ToLower()) :
                        (exclude ? !material.shader.name.ToLower().Contains(search.ToLower()) : material.shader.name.ToLower().Contains(search.ToLower()));

                }).ToArray();
            }
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            for (int ti = 0; ti < searchMaterials.Length; ti++)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Select", GUILayout.Width(50)))
                    Selection.objects = new Object[1] { AssetDatabase.LoadAssetAtPath<Material>(searchMaterials[ti]) };
                EditorGUILayout.LabelField(searchMaterials[ti]);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }
        EditorGUILayout.BeginHorizontal();
        targetShader = EditorGUILayout.ObjectField(targetShader, typeof(Shader), false) as Shader;
        if (GUILayout.Button("Switch To Shader"))
        {
            if (targetShader != null)
            {
                for (int mi = 0; mi < searchMaterials.Length; mi++)
                {
                    Material material = AssetDatabase.LoadAssetAtPath<Material>(searchMaterials[mi]);
                    material.shader = targetShader;
                }
            }
        }
        EditorGUILayout.EndHorizontal();
    }
}
