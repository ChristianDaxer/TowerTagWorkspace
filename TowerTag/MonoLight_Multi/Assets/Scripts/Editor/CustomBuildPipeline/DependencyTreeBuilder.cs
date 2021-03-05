// #define PROFILE_BUILD_DEPENDENCY_TREE

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

public struct ReferenceInfo {
    public bool dissociativeReference;
    public MemberInfo referencingMember;
    public bool referencingMemberIsField;

    // Used to determine whether the referencing member is an array.
    public bool referencingMemberIsEnumerable;

    // If the referencing member is an array, this represents the index at which
    // THIS dependency is referenced.
    public int referencingEnumerableIndex;
}

// This container represents a node in the dependency tree and holds information such as information
// about the referencing object and member, the type of referencing member and containing asset path
// such as the path to a prefab or scriptable object.
public struct Dependency
{
    // List of GUIDs representing dependencies that hold a reference to THIS dependency.
    public Dictionary<string, ReferenceInfo> referencingDependencyGuids;
    public List<string> references;

    // The GUID of THIS dependency in the flattened dependency look up table.
    public string dependencyGuid;

    // The path to the containing object or the object itself in the AssetDatabase.
    public string containingObjectPath;

    // A handle to the containing object which can be the object itself or the root
    // GameObject of the prefab non-instance in the AssetDatabase.
    public UnityEngine.Object containingObject;

    // A handle to the referenced instance.
    public UnityEngine.Object dependencyUnityObj;

    public bool isLeaf => 
        references != null ? 
            references.Count == 0 : 
            true; 
}

public class DependencyTreeBuilder
{

    // Simple hash lookup of processed object instances.
    private readonly HashSet<int> objectInstanceIds = new HashSet<int>();

    // Simple look up of object instance Id to the object's Guid, used retrieve an existing dependency if it exists.
    private readonly Dictionary<int, string> instanceIdToDependencyGuid = new Dictionary<int, string>();

    // Look up of referencing instance Ids and already processed implemented members.
    private readonly Dictionary<int, HashSet<MemberInfo>> processedMembers = new Dictionary<int, HashSet<MemberInfo>>();

    private static PlatformObjectBlacklist cachedBlacklist;
    private static bool unableToFindBlacklist = false;

    private Dependency CreateDependency (
        bool referencingObjectIsContainer,
        string referencingDependencyGuid,
        MemberInfo referencingMember,
        bool referencingMemberIsField,
        bool referencingMemberIsArray,
        int referencingArrayIndex,
        string dependencyGuid,
        UnityEngine.Object dependencyUnityObj) {
        UnityEngine.Object containingObject = null;
        string containingObjectPath = null;

        // If the referenced instance is a GameObjec or Component, then most likely its a prefab.
        if (dependencyUnityObj is GameObject || dependencyUnityObj is Component) {
            containingObject = PrefabUtility.IsPartOfAnyPrefab(dependencyUnityObj) ? AssetDatabase.LoadAssetAtPath<GameObject>(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(dependencyUnityObj)) : null;
            containingObjectPath = containingObject != null ? AssetDatabase.GetAssetPath(containingObject) : null;
        }

        // Otherwise the containing object is itself.
        else if (
            dependencyUnityObj is ScriptableObject ||
            dependencyUnityObj is Material ||
            dependencyUnityObj is Mesh ||
            dependencyUnityObj is Shader ||
            dependencyUnityObj is AudioClip) {
            containingObject = dependencyUnityObj;
            containingObjectPath = AssetDatabase.GetAssetPath(dependencyUnityObj);
        }

        return new Dependency {
            referencingDependencyGuids = (referencingDependencyGuid != null ? new Dictionary<string, ReferenceInfo>() 
            { 
                { referencingDependencyGuid, new ReferenceInfo {
                        dissociativeReference = referencingObjectIsContainer,
                        referencingMember = referencingMember,
                        referencingMemberIsField = referencingMemberIsField,
                        referencingMemberIsEnumerable = referencingMemberIsArray,
                        referencingEnumerableIndex = referencingArrayIndex
                    } 
                } 
            } : null),

            dependencyGuid = dependencyGuid,

            containingObjectPath = containingObjectPath,
            containingObject = containingObject,

            dependencyUnityObj = dependencyUnityObj,
        };
    }

    private bool DependencyAlreadyEnumerated(int instanceId) => instanceIdToDependencyGuid.ContainsKey(instanceId);

    // Retrieve existing dependency vika dependency Id.
    private bool TryGetDependencyByInstanceId (Dictionary<string, Dependency> dependencyLut, int instanceId, out Dependency dependency)
    {
        if (!instanceIdToDependencyGuid.TryGetValue(instanceId, out var dependencyGuid))
        {
            dependency = new Dependency();
            return false;
        }

        return dependencyLut.TryGetValue(dependencyGuid, out dependency);
    }

    // Retrieve an the singleton instance of the PlatformObjectBlacklist scriptable object in the AssetDatabase.
    private static bool GetBlackList (out PlatformObjectBlacklist blacklist)
    {
        blacklist = null;
        if (unableToFindBlacklist)
            return false;

        if (cachedBlacklist == null)
        {
            cachedBlacklist = AssetDatabase.LoadAssetAtPath<PlatformObjectBlacklist>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets($"t:{nameof(PlatformObjectBlacklist)}").First()));
            if (cachedBlacklist == null)
            {
                unableToFindBlacklist = true;
                return false;
            }
        }

        blacklist = cachedBlacklist;
        return true;
    }

    // Verify that the Object type is not in the black list.
    public static bool IsBlacklistedComponent(Object obj) => obj != null && GetBlackList(out var blacklist) ? blacklist.InBlacklist(obj.GetType()) : false;

    // Determine whether the Object is an instance and NOT part of a prefab Asset in the project. See: https://docs.unity3d.com/ScriptReference/PrefabUtility.IsPartOfNonAssetPrefabInstance.html
    private bool ObjectIsSceneInstanceOrPartOfPrefabInstance (Object obj) => PrefabUtility.IsPartOfNonAssetPrefabInstance(obj) || !PrefabUtility.IsPartOfAnyPrefab(obj);

    // Determine whether the referencing object and it's reference are within the same prefab to avoid 
    // creating dependencies for those references.
    private bool ReferenceAndReferencingObjectAreContainedWithinTheSamePrefab (Object referencingObject, GameObject go)
    {
        var referencingPrefabInstancePath = referencingObject != null ? PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(referencingObject) : null;
        var containingPrefabInstancePath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go);

        // If this GameObject exists in a prefab, then any references to members of that prefab should be ignored since those references are not references to external dependencies.
        return (!string.IsNullOrEmpty(containingPrefabInstancePath) && !string.IsNullOrEmpty(referencingPrefabInstancePath) && referencingPrefabInstancePath == containingPrefabInstancePath);
    }

    // Process the member value from a FieldInfo or PropertyInfo.
    private void ProcessMemberValue (
        Dictionary<string, Dependency> dependencyLut,   // Flattened look up table of the entire dependency tree.
        object referencedValue,                         // The value that the FieldInfo or PropertyInfo is holding.
        string referencingGuid,                         // The dependency Guid that implements the member that holds the value.
        int referencingInstanceId,                      // The object instance Id that implements the member that holds the value.[
        MemberInfo referencingMember,                   // The base instance of FieldInfo or PropertyInfo.
        bool isField,                                   // Is the referncing member a FieldInfo or propertyInfo?
        UnityEngine.Object referencingObject,           // A handle to the referencing object instance.
        System.Action<string> innerLoop = null) {       // Handle to callback that provides us with a entry to perform additional processing on newly created dependencies.  

        // Determine whether we've already processed THIS member on THIS object instnace.
        /*
        if (processedMembers.TryGetValue(referencingInstanceId, out var memberHash))
        {
            if (memberHash.Contains(referencingMember))
                return;
            memberHash.Add(referencingMember);
        }

        else processedMembers.Add(referencingInstanceId, new HashSet<MemberInfo>() { referencingMember });
        */

        // Ignore null values.
        if (referencedValue == null)
            return;

        if (referencedValue is Object) {

            // Recursively process potential dependency.
            RecursivelyEnumerateObjectDependencyTree(
                dependencyLut,
                referencingObject,
                false,
                referencingGuid,
                referencingInstanceId,
                referencingMember,
                isField,
                false,
                -1,
                referencedValue as Object,
                innerLoop);
        }

        else {
            ReflectionUtilities.EnumerateSystemObjectAsUnityEngineObjectArray(referencedValue, (element, i) =>
            {
                // Determine whether the referenced Object is a GameObject.
                if (!(element is ScriptableObject) && ReflectionUtilities.TryGetGameObjectFromObject(element as Object, out var go))
                    // Do not enumerate further if the GameObject is contained within the same prefab as the reference object.
                    if (ReferenceAndReferencingObjectAreContainedWithinTheSamePrefab(referencingObject, go))
                        return;

                // Recursively process potential dependency.
                RecursivelyEnumerateObjectDependencyTree(
                    dependencyLut,
                    referencingObject,
                    false,
                    referencingGuid,
                    referencingInstanceId,
                    referencingMember,
                    isField,
                    true,
                    i,
                    element,
                    innerLoop);
            });
        }
    }

    private void EnumerateAcrossObjectMemberValues (
        Dictionary<string, Dependency> dependencyLut,
        Object dependencyObj, 
        int dependencyInstanceId, 
        string dependencyGuid, 
        System.Action<string> innerLoop) {

        if (!ReflectionUtilities.SafelyGetAllMemberInfoAndValuesOfObjectTypeForObject(dependencyObj, out var fieldsAndProperties, out var fieldAndPropertyValues))
            return;

        int fieldIndex = 0;
        foreach (var fieldValue in fieldAndPropertyValues.fieldValues)
            ProcessMemberValue(
                dependencyLut, 
                fieldValue, 
                dependencyGuid, 
                dependencyInstanceId, 
                fieldsAndProperties.fields.ElementAt(fieldIndex++), 
                isField : true, 
                dependencyObj, 
                innerLoop);

        int propertyIndex = 0;
        foreach (var propertyValue in fieldAndPropertyValues.propertyValues)
            ProcessMemberValue(
                dependencyLut, 
                propertyValue, 
                dependencyGuid, 
                dependencyInstanceId, 
                fieldsAndProperties.properties.ElementAt(propertyIndex++), 
                isField : false, 
                dependencyObj, 
                innerLoop);
    }

    private readonly HashSet<int> processedComponents = new HashSet<int>();
    private void EnumerateAcrossComponentsConnectedToObject (
        Dictionary<string, Dependency> dependencyLut,
        UnityEngine.Object referencingObject,
        UnityEngine.Object dependencyUnityObj,
        string referencingDependencyGuid,
        int referencingInstanceId,
        MemberInfo referencingMember,
        bool referencingMemberIsField,
        bool referencingMemberIsEnumerable,
        int referencingEnumerableIndex,
        System.Action<string> innerLoop) {

        if (!ReflectionUtilities.TryGetGameObjectFromObject(dependencyUnityObj, out var go))
            return;

        var components = go.GetComponents<Component>()
            .Concat(go.GetComponentsInChildren<Component>(true))
            .Distinct()
            .Where(component => 
                (component != null && 
                component != dependencyUnityObj &&
                !IsBlacklistedComponent(component))).ToArray();

        if (components == null || components.Length == 0)
            return;

        foreach (var component in components) {

            RecursivelyEnumerateObjectDependencyTree(
                dependencyLut,
                referencingObject,
                string.IsNullOrEmpty(go.scene.path),
                referencingDependencyGuid,
                referencingInstanceId,
                referencingMember,
                referencingMemberIsField,
                referencingMemberIsEnumerable,
                referencingEnumerableIndex,
                component,
                innerLoop);
        }
    }

    #if PROFILE_BUILD_DEPENDENCY_TREE
    private void StartTimer (out Stopwatch timer) { 
        timer = new Stopwatch();
        timer.Start();
    }

    private void StopTimer (Stopwatch timer, Object targetObject) {
        timer.Stop();
        try { 
            UnityEngine.Debug.LogFormat("Finished reconfiguring object: \"{0}\" in: {1}:{2}:{3}",
                targetObject,
                Mathf.FloorToInt(((timer.ElapsedMilliseconds / 1000.0f) / 60.0f) % 60).ToString("00"),
                Mathf.FloorToInt((timer.ElapsedMilliseconds / 1000.0f) % 60).ToString("00"),
                Mathf.FloorToInt(timer.ElapsedMilliseconds % 1000).ToString("000"));
        } catch (System.Exception _) {}
    }
    #endif

    private void AddReferencingDependencyToReferencedDependency (
        Dictionary<string, Dependency> dependencyLut,
        ref Dependency existingDependency,
        bool dissociativeReference,
        string referencingDependencyGuid,
        MemberInfo referencingMember,
        bool referencingMemberIsField,
        bool referencingMemberIsEnumerable,
        int referencingEnumerableIndex) {

        if (string.IsNullOrEmpty(referencingDependencyGuid))
            return;

        ReferenceInfo referenceInfo = new ReferenceInfo
        {
            dissociativeReference = dissociativeReference,
            referencingMember = referencingMember,
            referencingMemberIsField = referencingMemberIsField,
            referencingMemberIsEnumerable = referencingMemberIsEnumerable,
            referencingEnumerableIndex = referencingEnumerableIndex,
        };

        if (existingDependency.referencingDependencyGuids == null) { 
            existingDependency.referencingDependencyGuids = new Dictionary<string, ReferenceInfo>() { { referencingDependencyGuid, referenceInfo } };
            dependencyLut[existingDependency.dependencyGuid] = existingDependency;
        }
        else existingDependency.referencingDependencyGuids.Add(referencingDependencyGuid, referenceInfo);
    }

    private void AddReferencedDependencyToReferencingDependency (Dictionary<string, Dependency> dependencyLut, ref Dependency existingDependency, string referencingDependencyGuid) {
        if (
            string.IsNullOrEmpty(referencingDependencyGuid) || 
            !dependencyLut.TryGetValue(referencingDependencyGuid, out var referencingDependency) || 
            (referencingDependency.references != null && referencingDependency.references.Contains(referencingDependencyGuid)))
            return;

        if (referencingDependency.references == null) {
            referencingDependency.references = new List<string>() { existingDependency.dependencyGuid };
            dependencyLut[referencingDependencyGuid] = referencingDependency;
        }

        else referencingDependency.references.Add(existingDependency.dependencyGuid);
    }

    private void RecursivelyEnumerateObjectDependencyTree(
        Dictionary<string, Dependency> dependencyLut,   // Flattened look up table of the entire dependency tree.
        Object referencingObject,                       // Handle to instance of referencing Object.
        bool dissociativeReference,                     // Flag used to indicate that the referencing dependency is a container.
        string referencingDependencyGuid,               // Guid used to access the referencing Dependency that references this dependency if it's created.
        int referencingInstanceId,                      // Unity instance Id for avoiding circular dependencies.
        MemberInfo referencingMember,                   // Referencing member which can be a FieldInfo or PropertyInfo
        bool referencingMemberIsField,                  // Flag for if the referencing member is a field.
        bool referencingMemberIsEnumerable,             // Flag for if the the referencing member is an array or list.
        int referencingEnumerableIndex,                 // referencingMemberIsArray is true, then this will represent the index at which the array references this dependency.
        UnityEngine.Object dependencyUnityObj,          // Unity object that the referencing dependency depends on.
        System.Action<string> innerLoop) {              // Callback that allows further execution on each dependency right after it's created.

        if (dependencyUnityObj == null)
            return;

#if PROFILE_BUILD_DEPENDENCY_TREE
        StartTimer(out var stopwatch);
#endif

        int dependencyInstanceId = dependencyUnityObj.GetInstanceID();

        if (TryGetDependencyByInstanceId(dependencyLut, dependencyInstanceId, out var existingDependency)) {

            if (string.IsNullOrEmpty(referencingDependencyGuid))
                goto earlyout;

            if (existingDependency.referencingDependencyGuids != null && existingDependency.referencingDependencyGuids.ContainsKey(referencingDependencyGuid)) {
                AddReferencedDependencyToReferencingDependency(dependencyLut, ref existingDependency, referencingDependencyGuid);
                goto earlyout;
            }

            AddReferencingDependencyToReferencedDependency(
                dependencyLut,
                ref existingDependency,
                dissociativeReference,
                referencingDependencyGuid,
                referencingMember,
                referencingMemberIsField,
                referencingMemberIsEnumerable,
                referencingEnumerableIndex);

            AddReferencedDependencyToReferencingDependency(dependencyLut, ref existingDependency, referencingDependencyGuid);

            if (innerLoop != null)
                innerLoop(existingDependency.dependencyGuid);

            goto earlyout;
        }

        // If this GameObject exists in a prefab, then any references to members of that prefab should be ignored since those references are not references to external dependencies.
        /*
        if (!(dependencyUnityObj is ScriptableObject) && ReflectionUtilities.TryGetGameObjectFromObject(dependencyUnityObj, out var go))
            if (ReferenceAndReferencingObjectAreContainedWithinTheSamePrefab(referencingObject, go))
                goto earlyout;
        */

        string dependencyGuid = System.Guid.NewGuid().ToString();

        bool blacklisted = IsBlacklistedComponent(dependencyUnityObj);
        if (!blacklisted)
        {
            var newDependency = CreateDependency(
                dissociativeReference,
                referencingDependencyGuid,
                referencingMember,
                referencingMemberIsField,
                referencingMemberIsEnumerable,
                referencingEnumerableIndex,
                dependencyGuid,
                dependencyUnityObj);

            dependencyLut.Add(dependencyGuid, newDependency);
            instanceIdToDependencyGuid.Add(dependencyInstanceId, dependencyGuid);

            AddReferencedDependencyToReferencingDependency(dependencyLut, ref existingDependency, referencingDependencyGuid);

            if (innerLoop != null)
                innerLoop(dependencyGuid);
        }

        if (!blacklisted)
            EnumerateAcrossObjectMemberValues(
                dependencyLut,
                dependencyUnityObj,
                dependencyInstanceId,
                dependencyGuid,
                innerLoop);

        EnumerateAcrossComponentsConnectedToObject(
            dependencyLut,
            referencingObject: null,
            dependencyUnityObj,
            referencingDependencyGuid,
            referencingInstanceId,
            referencingMember,
            referencingMemberIsField,
            referencingMemberIsEnumerable,
            referencingEnumerableIndex,
            innerLoop);

    earlyout:
        #if PROFILE_BUILD_DEPENDENCY_TREE
        StopTimer(stopwatch, dependencyUnityObj);
        #endif
        return;
    }

    public void EnumerateDependenciesForObject (UnityEngine.Object unityObject, Dictionary<string, Dependency> dependencyLut, System.Action<string> innerLoop = null) => 
        RecursivelyEnumerateObjectDependencyTree(
            dependencyLut,
            null,
            false,
            null,
            -1,
            null,
            true,
            false,
            -1,
            unityObject,
            innerLoop);
}
