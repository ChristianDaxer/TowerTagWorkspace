using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public static class DetermineIfSelectedObjectsIsReferencedInBuildScenes {
    [MenuItem("Unity/Determine if Selected Objects is Referenced in Build Scenes")]
    private static void Determine () {

        Object[] selection = Selection.objects;
        int sceneCount = SceneManager.sceneCountInBuildSettings;

        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        for (int si = 0; si < sceneCount; si++) {

            var scenePath = SceneUtility.GetScenePathByBuildIndex(si);
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            List<Object> referencingObjects = new List<Object>();
            var components = GameObject.FindObjectsOfType<Component>();

            Dictionary<string, Dependency> dependencyLut = new Dictionary<string, Dependency>();

            foreach (var component in components) {

                if (component == null)
                    continue;

                DependencyTreeBuilder dependencyTreeBuilder = new DependencyTreeBuilder();
                dependencyTreeBuilder.EnumerateDependenciesForObject(component, dependencyLut, null);
                foreach (var keyValuePair in dependencyLut) {
                    string dependencyGuid = keyValuePair.Key;
                    if (!dependencyLut.TryGetValue(dependencyGuid, out var dependency))
                        return;

                    Object value = dependency.dependencyUnityObj;
                    for (int oi = 0; oi < selection.Length; oi++) {
                        if (dependency.dependencyUnityObj != selection[oi])
                            continue;

                        referencingObjects.Add(keyValuePair.Value.containingObject);
                        EditorGUIUtility.PingObject(keyValuePair.Value.containingObject);

                        foreach (var referencingKeyValuePair in dependency.referencingDependencyGuids) { 
                            if (dependencyLut.TryGetValue(referencingKeyValuePair.Key, out var referencingDependency)) {
                                Debug.LogFormat("The selected object: \"{0}\" matches value: \"{1}\" of type: {2} which is referenced by {3} on Object of type: {4} attached to contained by: \"{5}\" in scene: \"{6}\".", 
                                    selection[oi],
                                    dependency.dependencyUnityObj, 
                                    dependency.dependencyUnityObj.GetType().FullName,
                                    referencingKeyValuePair.Value.referencingMemberIsField ? "a field" : "a property", 
                                    referencingDependency.dependencyUnityObj.GetType(), 
                                    referencingDependency.containingObject.name,
                                    scenePath);
                                continue;
                            }
                        }
                    }
                }
            }

            if (referencingObjects.Count == 0) {
                Debug.LogFormat("No references to selected objects in scene: \"{0}\".", scenePath);
                break;
                continue;
            }

            Debug.LogFormat("Found {0} references to selected objects in scene: \"{1}\", stopping scene enumeration to notify user.", referencingObjects.Count, scenePath);
            Selection.objects = referencingObjects.Select(obj => (obj is Component) ? (obj as Component).gameObject : obj).ToArray();
            break;
        }
    }
}
