using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

//https://answers.unity.com/questions/510945/find-materials-that-use-a-certain-shader.html

public class ShaderOccurenceWindow : EditorWindow {
    [MenuItem ("Tools/Shader Occurence")]
    public static void Open () {
        GetWindow<ShaderOccurenceWindow> ();
    }

    Shader shader;
    string shaderName;
    List<string> materials = new List<string> ();
    List<string> shaders = new List<string> ();
    Vector2 scroll;
    Vector2 scroll2;

    void OnGUI () {
        Shader prev = shader;
        string[] allMaterials = AssetDatabase.FindAssets ("t:Material");

        shader = EditorGUILayout.ObjectField (shader, typeof (Shader), false) as Shader;
        if (shader != prev) {
            string shaderPath = AssetDatabase.GetAssetPath (shader);
            materials.Clear ();            
            shaders.Clear ();
            for (int i = 0; i < allMaterials.Length; i++) {
                allMaterials[i] = AssetDatabase.GUIDToAssetPath (allMaterials[i]);
                string[] dep = AssetDatabase.GetDependencies (allMaterials[i]);
                if (ArrayUtility.Contains (dep, shaderPath))
                    materials.Add (allMaterials[i]);
            }
        }

        shaderName = EditorGUILayout.TextField ("Shader Name", shaderName);
        if (GUILayout.Button ("Search")) {
            if (shaderName != null && shaderName.Length > 0) {
            materials.Clear ();            
            shaders.Clear ();
                for (int i = 0; i < allMaterials.Length; i++) {
                    allMaterials[i] = AssetDatabase.GUIDToAssetPath (allMaterials[i]);
                    string[] dep = AssetDatabase.GetDependencies (allMaterials[i]);
                    foreach (var item in dep) {
                        if (allMaterials[i] != item && item.ToLower ().Contains (shaderName.ToLower ()) && !materials.Contains (allMaterials [i])) {
                            Debug.Log (item);
                            materials.Add (allMaterials[i]);
                        }
                    }
                    if (!materials.Contains (allMaterials[i])) {
                        var mat = AssetDatabase.LoadAssetAtPath<Material> (allMaterials[i]);
                        if (mat) {
                            if (mat.shader != null && mat.shader.name.ToLower ().Contains (shaderName.ToLower ())) {
                                materials.Add (allMaterials[i]);
                            }
                        }
                    }
                }
            }
        }
        
        if (GUILayout.Button ("Find Missing Shaders")) {
            materials.Clear ();            
            shaders.Clear ();
            for (int i = 0; i < allMaterials.Length; i++) {
                allMaterials[i] = AssetDatabase.GUIDToAssetPath (allMaterials[i]);
                string[] dep = AssetDatabase.GetDependencies (allMaterials[i]);
                bool hasShader = false;
                foreach (var item in dep) {
                    if (item.ToLower ().Contains (".shader"))
                        hasShader = true;
                }
                if (!hasShader) {
                    var mat = AssetDatabase.LoadAssetAtPath<Material> (allMaterials[i]);
                    if (mat) {
                        if (mat.shader == null) {
                            materials.Add (allMaterials[i]);
                        }
                        if (mat.shader && mat.shader.name.ToLower ().Contains ("error")) {
                            materials.Add (allMaterials[i]);
                        }
                    }
                }
            }
        }
        
        if (GUILayout.Button ("Find Used Shaders")) {
            materials.Clear ();
            shaders.Clear ();
            for (int i = 0; i < allMaterials.Length; i++) {
                allMaterials[i] = AssetDatabase.GUIDToAssetPath (allMaterials[i]);
                string[] dep = AssetDatabase.GetDependencies (allMaterials[i]);
                foreach (var item in dep) {
                    if (item.ToLower ().Contains (".shader") && !shaders.Contains (item)) {
                        //Debug.Log ("Found " + item);
                        shaders.Add (item);
                    }
                }
            }
        }

        if (GUILayout.Button ("Find Unused Shaders")) {
            materials.Clear ();            
            shaders.Clear ();
            List<string> allShaders = new List<string> (AssetDatabase.FindAssets ("t:Shader"));
            List<string> shadersUsed = new List<string> ();
            for (int i = 0; i < allMaterials.Length; i++) {
                allMaterials[i] = AssetDatabase.GUIDToAssetPath (allMaterials[i]);
                string[] dep = AssetDatabase.GetDependencies (allMaterials[i]);
                foreach (var item in dep) {
                    if (item.ToLower ().Contains (".shader") && !shaders.Contains (item)) {
                        //Debug.Log ("Found " + item);
                        shadersUsed.Add (item);
                    }
                }
            }
            
            for (int i = 0; i < allShaders.Count; i++) {
                allShaders[i] = AssetDatabase.GUIDToAssetPath (allShaders[i]);
            }

            foreach (var item in shadersUsed) {
                if (allShaders.Remove (item)) {
                    Debug.Log ("Used " + item);
                }
            }

            shaders = allShaders;
        }
        
        scroll2 = GUILayout.BeginScrollView (scroll2);
        {
            for (int i = 0; i < shaders.Count; i++) {
                GUILayout.BeginHorizontal ();
                {
                    // Debug.Log ("Show " + Path.GetFileNameWithoutExtension (shaders[i]));
                    GUILayout.Label (Path.GetFileNameWithoutExtension (shaders[i]));
                    GUILayout.FlexibleSpace ();
                    if (GUILayout.Button ("Show"))
                        EditorGUIUtility.PingObject (AssetDatabase.LoadAssetAtPath (shaders[i], typeof (Shader)));
                }
                GUILayout.EndHorizontal ();
            }
        }
        GUILayout.EndScrollView ();

        scroll = GUILayout.BeginScrollView (scroll);
        {
            for (int i = 0; i < materials.Count; i++) {
                GUILayout.BeginHorizontal ();
                {
                    GUILayout.Label (Path.GetFileNameWithoutExtension (materials[i]));
                    GUILayout.FlexibleSpace ();
                    if (GUILayout.Button ("Show"))
                        EditorGUIUtility.PingObject (AssetDatabase.LoadAssetAtPath (materials[i], typeof (Material)));
                }
                GUILayout.EndHorizontal ();
            }
        }
        GUILayout.EndScrollView ();
    }
}