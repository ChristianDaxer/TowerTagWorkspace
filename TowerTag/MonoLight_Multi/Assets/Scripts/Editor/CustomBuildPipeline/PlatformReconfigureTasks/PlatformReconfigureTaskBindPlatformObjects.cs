using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Linq.Expressions;

[CustomEditor(typeof(PlatformReconfigureTaskBindPlatformObjects))]
public class PlatformReconfigureTaskBindPlatformObjectsEdtior : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var task = target as PlatformReconfigureTaskBindPlatformObjects;
        if (task == null)
            return;
    }
}

[CreateAssetMenu(fileName = "ConfigureTaskBindPlatformObjects", menuName = "ScriptableObjects/Platform Build Tasks/Configure Task Bind Platform Objects", order = 1)]
public class PlatformReconfigureTaskBindPlatformObjects : PlatformSceneReconfigureTaskScriptableObject
{
    private const string enumeratingTaskDescription = "({2}/{3} {4}% Remaining): Building dependency tree for component: \"{0}\" in scene: \"{1}\".";
    private const string reconfiguringTaskDescription = "({1}/{2} {3}% Remaining): Attempting to reconfiguring dependency tree starting at leaf: \"{0}\".";

    private string taskDescription;

    public override string SceneTaskDescription => taskDescription;

    public bool useCoroutines = true;
    public bool debug = false;
    public bool saveScene = true;

    public IEnumerable<string> SearchForAssets (string searchQuery) => 
        AssetDatabase.FindAssets(searchQuery)
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid));

    // private void EnumerateComponent(Component component, DependencyTreeBuilder dependencyTreeBuilder, Dictionary<string, Dependency> dependencyLut, List<string> sceneObjDependencies, string platformObjPostfix)
    // {
    //     dependencyTreeBuilder.EnumerateDependenciesForObject(component, dependencyLut, (dependencyGuid) => {
    //         if (!dependencyLut.TryGetValue(dependencyGuid, out var dependency)) //     return;

    //         if (!HasValidPlatformObject(platformObjPostfix, ref dependency, out var platformObjectPath))
    //             return;

    //         if (debug) {
    //             if (dependency.referencingDependencyGuids == null || dependency.referencingDependencyGuids.Count == 0)
    //                 return;

    //             if (!reconfiguredGuids.Contains(dependency.dependencyGuid))
    //                 for (int i = 0; i < dependency.referencingDependencyGuids.Count; i++) {
    //                     var reference = dependency.referencingDependencyGuids[i];
    //                     if (reference.referencingObjectIsContainer)
    //                         continue;

    //                     if (!dependencyLut.TryGetValue(reference.referencingGuid, out var refDependency))
    //                         continue;

    //                     string referencingInfo = dependency.referencingMember != null ? $"{(dependency.referencingMemberIsField ? "field" : "property")}: \"{dependency.referencingMember.Name}\" " : "(No Referencing Member)";
    //                     string referencedByInfo = (refDependency.dependencyUnityObj is Component) ?
    //                         $"\tReferenced by component: \"{refDependency.dependencyUnityObj.GetType().FullName}\" at {referencingInfo} attached to GameObject: \"{(refDependency.dependencyUnityObj as Component).gameObject.name}\"" :
    //                         $"\tReferenced by object: \"{refDependency.dependencyUnityObj.GetType().FullName}\" at {referencingInfo}";

    //                     Debug.Log(
    //                         $"Found object reference: \"{dependency.containingObjectPath}\" that can be reconfigured the following platform object: \"{platformObjectPath}\". Below is some additional information:\n" +
    //                         $"{referencedByInfo}\n" +
    //                         $"\tRoot referenced by component: \"{component.GetType().FullName}\" attached to GameObject: \"{component.gameObject.name}\" in scene: \"{component.gameObject.scene.path}\""
    //                     );
    //                 }
    //         }

    //         reconfigureCircularDependencyLookup.Clear();

    //         bool updatedDependencyChain = false, appliedMemberValue = false;
    //         WalkUp(dependencyLut, dependency, platformObjPostfix, ref updatedDependencyChain, ref appliedMemberValue);
    //     });
    // }

    private DependencyTreeReconfigurer dependencyTreeReconfigurer;
    private string cachedPlatformObjPostfix;
    public void WalkUpCallback(DependencyTreeEnumerator enumerator, Dependency thisDependency, ReferenceInfo referenceInfo, Dependency nextDependency) {

        if (referenceInfo.referencingMember == null)
            return;

        if (referenceInfo.dissociativeReference)
            return;

        bool updatedDependencyChain = false, appliedMemberValue = false;
        dependencyTreeReconfigurer.TryApplyPlatformValue(
            enumerator.Dependencies, 
            cachedPlatformObjPostfix, 
            nextDependency.dependencyUnityObj, 
            ref thisDependency, 
            ref referenceInfo, 
            ref nextDependency, 
            ref updatedDependencyChain, 
            ref appliedMemberValue);
    }

    private IEnumerator ReconfigureScene (UnityEngine.SceneManagement.Scene scene, string platformObjPostfix, System.Action<bool> sceneReconfigureCallback)
    {
        var components = scene.GetRootGameObjects().SelectMany(gameObject => gameObject
            .GetComponents<Component>()
            .Concat(gameObject.GetComponentsInChildren<Component>(true)
            .Distinct()
            .Where(component => !DependencyTreeBuilder.IsBlacklistedComponent(component)))).ToArray();
        // var components = GameObject.FindObjectsOfType<Component>().Where(component => !DependencyTreeBuilder.IsBlacklistedComponent(component)).ToArray();

        if (components.Length == 0)
        {
            sceneReconfigureCallback(true);
            yield break;
        }

        if (debug) Debug.LogFormat("Found {0} components in the scene.", components.Length);

        Dictionary<string, Dependency> dependencyLut = new Dictionary<string, Dependency>();
        List<string> leavesToReprocess = new List<string>();

        DependencyTreeBuilder dependencyTreeBuilder = new DependencyTreeBuilder();

        for (int ci = 0; ci < components.Length; ci++) {
            Component component = components[ci];

            if (PlatformConfigScriptableObject.ShowTaskDescription) { 
                taskDescription = string.Format(enumeratingTaskDescription, component.GetType().Name, component.gameObject.scene.path, ci, components.Length, (ci / (float)components.Length) * 100);
                PlatformConfigScriptableObject.RepaintInspectorGUI();
            }

            if (useCoroutines)
                yield return null;
            dependencyTreeBuilder.EnumerateDependenciesForObject(component, dependencyLut);
        }

        dependencyTreeReconfigurer = new DependencyTreeReconfigurer(debug);
        cachedPlatformObjPostfix = platformObjPostfix;

        var leafKeys = dependencyLut.Where(keyValuePair => keyValuePair.Value.isLeaf).Select(keyValuePair => keyValuePair.Key).ToArray();
        for (int i = 0; i < leafKeys.Length; i++) {

            if (!dependencyTreeReconfigurer.TryStartTraverseUpThroughDependencyTree(
                dependencyLut,
                leafKeys[i],
                WalkUpCallback,
                out var traverse))
                continue;

            while (traverse.Next()) {
                if (useCoroutines)
                    yield return null;

                if (PlatformConfigScriptableObject.ShowTaskDescription) {
                    Dependency dependency = dependencyLut[leafKeys[i]];
                    taskDescription = string.Format(reconfiguringTaskDescription, dependency.dependencyUnityObj.name, i, leafKeys.Length, (i / (float)leafKeys.Length) * 100);
                    PlatformConfigScriptableObject.RepaintInspectorGUI();
                }
            }

            #if PROFILE_BUILD_DEPENDENCY_TREE
            break;
            #endif
        }

        // while (leavesToReprocess.Count == 0) {
        //     string[] cachedLeavesToReprocess = leavesToReprocess.ToArray();
        //     for (int i = 0; i < cachedLeavesToReprocess.Length; i++)
        //     {
        //         Dependency dependency = dependencyLut[leafKeys[i]];
        //         reconfigureCircularDependencyLookup.Clear();

        //         if (PlatformConfigScriptableObject.ShowTaskDescription) {
        //             taskDescription = string.Format(reprocessingTaskDescription, dependency.dependencyUnityObj.name, i, leafKeys.Length, (i / (float)leafKeys.Length) * 100);
        //             PlatformConfigScriptableObject.RepaintInspectorGUI();
        //         }

        //         yield return null;

        //         bool updatedDependencyChain = false, appliedMemberValue = false;
        //         WalkUp(
        //             dependencyLut,
        //             leavesToReprocess,
        //             dependency,
        //             platformObjPostfix,
        //             ref updatedDependencyChain,
        //             ref appliedMemberValue);
        //         }
        // }

        EditorSceneManager.MarkSceneDirty(scene);
        if (saveScene) EditorSceneManager.SaveScene(scene);

        if (debug) {
            Debug.LogFormat("Closing scene: \"{0}\".", scene.path);
        }

        EditorSceneManager.CloseScene(scene, false);
        if (sceneReconfigureCallback != null)
            sceneReconfigureCallback(true);
    }

    private IEnumerator ReconfigureCoroutine (HomeTypes homeType, SceneAsset sceneAsset, System.Action<bool> taskCallback)
    {
        string platformObjPostfix = $"_{homeType.ToString().ToLower()}";
        string sceneAssetPath = AssetDatabase.GetAssetPath(sceneAsset);
        bool success = true;

        if (debug) Debug.LogFormat("Opening scene: \"{0}\".", sceneAssetPath);
        var scene = EditorSceneManager.OpenScene(sceneAssetPath, OpenSceneMode.Single);

        yield return ReconfigureScene(scene, platformObjPostfix, (successfulReconfigure) =>
        {
            Debug.LogFormat("Successfully reconfigured scene: \"{0}\".", scene.path);
            success &= successfulReconfigure;
        });

        if (!success)
        {
            Debug.LogFormat("Failed to reconfigure scene: \"{0}\".", scene.path);

            if (taskCallback != null)
                taskCallback(false);

            yield break;
        }

        if (taskCallback != null)
            taskCallback(success);
    }

    public override IEnumerator ReconfigureScene(HomeTypes homeType, SceneWrapper sceneWrapper, System.Action<IPlatformReconfigureTaskDescriptor> startTaskCallback, System.Action<bool> taskCallback)
    {
        if (skip)
            yield break;

        yield return ReconfigureCoroutine(homeType, sceneWrapper.GetStagedSceneAsset(), taskCallback);
    }
}
