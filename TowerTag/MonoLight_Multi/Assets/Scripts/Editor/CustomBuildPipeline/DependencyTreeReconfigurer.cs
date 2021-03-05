using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.Linq.Expressions;

public class DependencyTreeReconfigurer
{
    private readonly bool debug;
    public DependencyTreeReconfigurer (bool debug) {
        this.debug = debug;
    }
    public static bool ContainsPlatformPostfix (string paltfromPrefabPostfix, string prefabFileNameWithoutExtension)
    {
        if (prefabFileNameWithoutExtension.Length < paltfromPrefabPostfix.Length)
            return false;

        if (prefabFileNameWithoutExtension.Substring(prefabFileNameWithoutExtension.Length - paltfromPrefabPostfix.Length, paltfromPrefabPostfix.Length).ToLower() != paltfromPrefabPostfix)
            return false;

        return true;
    }

    public static string RemovePlatformPostfixIfAvailable (string prefabFileNameWithoutExtension)
    {
        int indexOfLastUnderscore = prefabFileNameWithoutExtension.LastIndexOf('_');
        if (indexOfLastUnderscore == -1)
            return prefabFileNameWithoutExtension;
        string subStr = prefabFileNameWithoutExtension.Substring(indexOfLastUnderscore + 1, prefabFileNameWithoutExtension.Length - indexOfLastUnderscore - 1).ToLower();

        string[] homeTypeStrings = System.Enum.GetNames(typeof(HomeTypes));
        for (int i = 0; i < homeTypeStrings.Length; i++)
            if (subStr == homeTypeStrings[i].ToLower())
                return prefabFileNameWithoutExtension.Substring(0, prefabFileNameWithoutExtension.Length - homeTypeStrings[i].Length - 1);

        return prefabFileNameWithoutExtension;
    }

    public static bool BuildPlatformObjectPath (string platformObjPostfix, string objectPath, out string platformObjectPath) { 
        platformObjectPath = null;
        if (string.IsNullOrEmpty(objectPath))
            return false;

        string objFileNameWithoutExtension = Path.GetFileNameWithoutExtension(objectPath);
        string extension = Path.GetExtension(objectPath);

        if (ContainsPlatformPostfix(platformObjPostfix, objFileNameWithoutExtension))
        {
            platformObjectPath = objectPath;
            return true;
        }

        objFileNameWithoutExtension = RemovePlatformPostfixIfAvailable(objFileNameWithoutExtension);
        platformObjectPath = Path.Combine(Path.GetDirectoryName(objectPath), $"{objFileNameWithoutExtension}{platformObjPostfix}{extension}").Replace('\\', '/');
        return true;
    }

    public static  bool HasValidPlatformObject(string platformObjPostfix, ref Dependency dependency, out string platformObjectPath) => BuildPlatformObjectPath(platformObjPostfix, dependency.containingObjectPath, out platformObjectPath) && File.Exists(platformObjectPath);
    private bool HasValidPlatformObject(string platformObjPostfix, string objectPath, out string platformObjectPath) => BuildPlatformObjectPath(platformObjPostfix, objectPath, out platformObjectPath) && File.Exists(platformObjectPath);

    private void SetMemberValue (
        Object platformObject, 
        string prevPlatformObjectPath, 
        Object referencingObject,
        ref ReferenceInfo referenceInfo,
        ref Dependency prevDependency) {
        try {

            /*
            System.Object value = null;
            System.Type type = null;

            if (referenceInfo.referencingMemberIsField) {
                FieldInfo fieldInfo = (referenceInfo.referencingMember as FieldInfo);
                type = fieldInfo.FieldType;
                ReflectionUtilities.GetFieldValue(fieldInfo, referencingObject, out value);
            }

            else {
                PropertyInfo propertyInfo = (referenceInfo.referencingMember as PropertyInfo);
                type = propertyInfo.PropertyType;
                ReflectionUtilities.GetPropertyValue(propertyInfo, referencingObject, out value);
            }
            */

            // if (value == null || (value as UnityEngine.Object) != platformObject) {
            if (referenceInfo.referencingMemberIsField)
            {

                FieldInfo fieldInfo = (referenceInfo.referencingMember as FieldInfo);
                fieldInfo.SetValue(referencingObject, platformObject);
                EditorUtility.SetDirty(referencingObject);
            }

            else
            {
                PropertyInfo propertyInfo = (referenceInfo.referencingMember as PropertyInfo);
                propertyInfo.SetValue(referencingObject, platformObject);
                EditorUtility.SetDirty(referencingObject);
            }

            if (debug) Debug.LogFormat(
                $"Applied platform specific object: \"{prevPlatformObjectPath}\" to the following:\n" +
                $"\tField: \"{referenceInfo.referencingMember.Name}\"\n" +
                $"\tField Type: \"{(referenceInfo.referencingMemberIsField ? (referenceInfo.referencingMember as FieldInfo).FieldType.FullName : (referenceInfo.referencingMember as PropertyInfo).PropertyType.FullName)}\"\n" +
                $"\tReferencing Object Type: \"{referencingObject.GetType().FullName}\"\n" +
                $"\tReference Object Type: \"{prevDependency.dependencyUnityObj.GetType().FullName}\"\n" +
                ((referencingObject is GameObject) ?
                    $"\tScene: \"{((referencingObject as GameObject).scene.path)}\"\n" :
                    ((referencingObject is Component) ?
                        $"\tScene: \"{((referencingObject as Component).gameObject.scene.path)}\"\n" :
                        "\tScene: Unknown\n")));
            // }

        } catch (System.Exception exception) {
            Debug.LogError(
                    $"The following exception occurred while attempting to set member value using the following data:\n" +
                    $"\tField: \"{referenceInfo.referencingMember.Name}\"\n" +
                    $"\tField Type: \"{(referenceInfo.referencingMemberIsField ? (referenceInfo.referencingMember as FieldInfo).FieldType.FullName : (referenceInfo.referencingMember as PropertyInfo).PropertyType.FullName)}\"\n" +
                    $"\tReferencing Object Type: \"{referencingObject.GetType().FullName}\"\n" +
                    $"\tReference Object Type: \"{prevDependency.dependencyUnityObj.GetType().FullName}\"\n" +
                    ((referencingObject is GameObject) ?
                        $"\tScene: \"{((referencingObject as GameObject).scene.path)}\"\n" :
                        ((referencingObject is Component) ?
                            $"\tScene: \"{((referencingObject as Component).gameObject.scene.path)}\"\n" :
                            "\tScene: Unknown\n")));
            Debug.LogException(exception);
        }
    }

    private object Cast(System.Type Type, object data)
    {
        var DataParam = Expression.Parameter(typeof(object), "data");
        var Body = Expression.Block(Expression.Convert(Expression.Convert(DataParam, data.GetType()), Type));

        var Run = Expression.Lambda(Body, DataParam).Compile();
        var ret = Run.DynamicInvoke(data);
        return ret;
    }

    private void SetArrayMemberValue(
        Object platformObject,
        string prevPlatformObjectPath,
        Object referencingObject,
        ref ReferenceInfo referenceInfo,
        ref Dependency prevDependency) {

        try {

            IEnumerable<Object> arrayValue = null;
            System.Type type = null;

            if (referenceInfo.referencingMemberIsField) {
                FieldInfo fieldInfo = (referenceInfo.referencingMember as FieldInfo);
                arrayValue = fieldInfo.GetValue(referencingObject) as IEnumerable<Object>;
                type = fieldInfo.FieldType;
            }
            else {
                PropertyInfo propertyInfo = (referenceInfo.referencingMember as PropertyInfo);
                arrayValue = propertyInfo.GetValue(referencingObject) as IEnumerable<Object>;
                type = propertyInfo.PropertyType;
            }

            if (arrayValue != null) {
                var arrayElementValue = arrayValue.ElementAt(referenceInfo.referencingEnumerableIndex) as UnityEngine.Object;
                if (arrayElementValue == null || arrayElementValue != platformObject) {
                    if (type.IsArray) {

                        var nonGenericArray = arrayValue as Object[];

                        System.Array copyOfArray = System.Array.CreateInstance(type.GetElementType(), nonGenericArray.Length);
                        System.Array.Copy(nonGenericArray, copyOfArray, copyOfArray.Length);

                        System.Type elementType = type.GetElementType();
                        copyOfArray.SetValue(Cast(elementType, platformObject), referenceInfo.referencingEnumerableIndex);

                        if (referenceInfo.referencingMemberIsField) { 
                            (referenceInfo.referencingMember as FieldInfo).SetValue(referencingObject, copyOfArray);
                        }

                        else if (ReflectionUtilities.PropertyIsNotMaterialOrMesh(referenceInfo.referencingMember as PropertyInfo)) { 
                            (referenceInfo.referencingMember as PropertyInfo).SetValue(referencingObject, copyOfArray);
                        }

                        EditorUtility.SetDirty(referencingObject);
                    }

                    else {
                        var genericList = arrayValue as List<Object>;

                        System.Type genericType = type.GenericTypeArguments[0];

                        System.Array copyOfList = System.Array.CreateInstance(genericType, genericList.Count);
                        System.Array.Copy(genericList.ToArray(), copyOfList, copyOfList.Length);

                        copyOfList.SetValue(Cast(genericType, platformObject), referenceInfo.referencingEnumerableIndex);

                        if (referenceInfo.referencingMemberIsField)
                            (referenceInfo.referencingMember as FieldInfo).SetValue(referencingObject, Enumerable.ToList(copyOfList as IEnumerable<Object>)); 

                        else if (ReflectionUtilities.PropertyIsNotMaterialOrMesh(referenceInfo.referencingMember as PropertyInfo))
                            (referenceInfo.referencingMember as PropertyInfo).SetValue(referencingObject, Enumerable.ToList(copyOfList as IEnumerable<Object>));

                        EditorUtility.SetDirty(referencingObject);
                    }

                    if (debug) Debug.LogFormat(
                        $"Applied platform specific object: \"{prevPlatformObjectPath}\" to the following:\n" +
                        $"\tEnumerable Field: \"{referenceInfo.referencingMember.Name}\"\n" +
                        $"\tEnumerable Field Type: \"{(referenceInfo.referencingMemberIsField ? (referenceInfo.referencingMember as FieldInfo).FieldType.FullName : (referenceInfo.referencingMember as PropertyInfo).PropertyType.FullName)}\"\n" +
                        $"\tElement Index: \"{referenceInfo.referencingEnumerableIndex}\"\n" +
                        $"\tReferencing Object Type: \"{referencingObject.GetType().FullName}\"\n" +
                        $"\tReference Object Type: \"{prevDependency.dependencyUnityObj.GetType().FullName}\"\n" +
                        ((referencingObject is GameObject) ?
                            $"\tScene: \"{((referencingObject as GameObject).scene.path)}\"\n" :
                            ((referencingObject is Component) ?
                                $"\tScene: \"{((referencingObject as Component).gameObject.scene.path)}\"\n" :
                                "\tScene: Unknown\n")));
                }
            }

        } catch (System.Exception exception) {
            Debug.LogError(
                    $"The following exception occurred while attempting to set member array value using the following data:\n" +
                    $"\tEnumerable Field: \"{referenceInfo.referencingMember.Name}\"\n" +
                    $"\tEnumerable Field Type: \"{(referenceInfo.referencingMemberIsField ? (referenceInfo.referencingMember as FieldInfo).FieldType.FullName : (referenceInfo.referencingMember as PropertyInfo).PropertyType.FullName)}\"\n" +
                    $"\tElement Index: \"{referenceInfo.referencingEnumerableIndex}\"\n" +
                    $"\tReferencing Object Type: \"{referencingObject.GetType().FullName}\"\n" +
                    $"\tReference Object Type: \"{prevDependency.dependencyUnityObj.GetType().FullName}\"\n" +
                    ((referencingObject is GameObject) ?
                        $"\tScene: \"{((referencingObject as GameObject).scene.path)}\"\n" :
                        ((referencingObject is Component) ?
                            $"\tScene: \"{((referencingObject as Component).gameObject.scene.path)}\"\n" :
                            "\tScene: Unknown\n")));
            Debug.LogException(exception);
        }
    }

    private bool ApplyMemberValue (
        ref ReferenceInfo referenceInfo, 
        Object referencingObject, 
        string prevPlatformObjectPath, 
        ref Dependency prevDependency) {

        if (!TryLoad(ref referenceInfo, prevPlatformObjectPath, out var prevPlatformObject))
            return false;

        if (referenceInfo.referencingMemberIsEnumerable)
            SetArrayMemberValue(prevPlatformObject, prevPlatformObjectPath, referencingObject, ref referenceInfo, ref prevDependency);
        else SetMemberValue(prevPlatformObject, prevPlatformObjectPath, referencingObject, ref referenceInfo, ref prevDependency);
        return true;
    }

    private void UpdateDependencyTree (
        Dictionary<string, Dependency> dependencyLut, 
        ref Dependency dependency, 
        Object instance, 
        Object containingInstance, 
        string containingObjectPath) {

        dependency.dependencyUnityObj = instance;
        dependency.containingObject = instance;
        dependency.containingObjectPath = containingObjectPath;

        // Update the dependency tree with the paltform specific version.
        dependencyLut[dependency.dependencyGuid] = dependency;
    }

    private bool TryGetComponentAtHierarchalAddress (Transform root, int[] hierarchalAddress, System.Type type, bool isComponent, out Object obj) {
        Transform transform = root;
        for (int i = hierarchalAddress.Length - 1; i > 0; i--)
            transform = transform.GetChild(hierarchalAddress[i]);

        if (isComponent) { 
            var components = transform.GetComponents(type);

            int componentIndex = hierarchalAddress[0];
            if (components.Count() - 1 < componentIndex) {
                Debug.LogErrorFormat("Unable to retrieve component of type: {0} attached to GameObject: \"{1}\", the component is missing, or the hierarchal address is wrong!", type.FullName, transform.gameObject.name);
                obj = null;
                return false;
            }

            obj = components.ElementAt(componentIndex);
            return true;
        }

        obj = transform.gameObject;
        return true;
    }

    private readonly List<int> listOfChildIndices = new List<int>(10);
    private int[] GetHierarchalAddressOfObject(Object obj) {

        listOfChildIndices.Clear();
        Transform transform = null;

        if (obj is Component) {
            transform = (obj as Component).transform;
            var components = transform.GetComponents(obj.GetType());
            for (int i = 0; i < components.Count(); i++)
            {
                if (components.ElementAt(i) != obj)
                    continue;

                listOfChildIndices.Add(i);
                break;
            }
        }

        else transform = (obj as GameObject).transform;

        Transform parent = transform.parent;

        while (parent != null) {
            for (int i = 0; i < parent.childCount; i++) {

                if (parent.GetChild(i) != transform)
                    continue;

                listOfChildIndices.Add(i);
                break;
            }

            transform = parent;
            parent = parent.parent;
        }

        return listOfChildIndices.ToArray();
   }

    private bool GetCorrespondingObjectInPrefabVariant (Transform prefabVariantRoot, Object targetObj, bool isComponent, out Object correspondingObject) {
        int[] hierarchalAddressOfObj = GetHierarchalAddressOfObject(targetObj);
        return TryGetComponentAtHierarchalAddress(prefabVariantRoot, hierarchalAddressOfObj, targetObj.GetType(), isComponent, out correspondingObject);
    }

    private bool ObjectIsNonAssetInstance (Object obj) { 

        ReflectionUtilities.TryGetGameObjectFromObject(obj, out var go);
        return 
            (go != null && !string.IsNullOrEmpty(go.scene.path)) || 
            string.IsNullOrEmpty(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj));
    }

    private bool TryLoad(ref ReferenceInfo referenceInfo, string path, out Object obj) {
        obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);

        if (obj == null) {
            Debug.LogErrorFormat("Failed to load platform replacement object at path: \"{0}\" for member reference: \"{1}\" of class: \"{2}\".", path, referenceInfo.referencingMember.Name, referenceInfo.referencingMember.DeclaringType.FullName);
            return false;
        }

        System.Type memberType = referenceInfo.referencingMemberIsField ? (referenceInfo.referencingMember as FieldInfo).FieldType : (referenceInfo.referencingMember as PropertyInfo).PropertyType;
        if (memberType.IsSubclassOf(typeof(Component)) && obj is GameObject) { 
            obj = (obj as GameObject).GetComponent(memberType);
            if (obj == null) {
                Debug.LogErrorFormat("Failed to get component of type: {0}, in prefab: \"{1}\" for member reference: \"{2}\" of class: \"{3}\".", memberType.FullName, path, referenceInfo.referencingMember.Name, referenceInfo.referencingMember.DeclaringType.FullName);
                return false;
            }
        }

        return obj != null;
    }

    public bool TryApplyPlatformValue(
        Dictionary<string, Dependency> dependencyLut,
        string platformObjPostfix, 
        Object referencingObject,
        ref Dependency prevDependency,
        ref ReferenceInfo referenceInfo,
        ref Dependency nextDependency,
        ref bool updatedDependencyTree,
        ref bool appliedMemberValue) {

        if (
            !DependencyTreeReconfigurer.HasValidPlatformObject(platformObjPostfix, ref prevDependency, out var replacementPath) || 
            prevDependency.containingObjectPath == replacementPath)
            return false;

        // If this returns true, then it means that the next dependency is an Object that exists in the scene and we can
        // just modify the field value on the prefab instance in the scene without modifying the prefab asset.
        ReflectionUtilities.TryGetGameObjectFromObject(referencingObject, out var go);
        bool referencingObjIsGameObjectOrComponent = ReflectionUtilities.ObjectIsGameObjectOrComponent(referencingObject);
        if (!referencingObjIsGameObjectOrComponent || ObjectIsNonAssetInstance(referencingObject)) {
            if (referencingObjIsGameObjectOrComponent) {
                if (
                    !ReflectionUtilities.ObjectIsGameObjectOrComponent(prevDependency.dependencyUnityObj) || 
                    !ObjectIsNonAssetInstance(prevDependency.dependencyUnityObj)) 
                    appliedMemberValue |= ApplyMemberValue(ref referenceInfo, referencingObject, replacementPath, ref prevDependency);
            }

            else {
                string objectAssetPath = AssetDatabase.GetAssetPath(referencingObject);
                Object instance = null;
                if (!HasValidPlatformObject(platformObjPostfix, objectAssetPath, out var platformAssetPath) ||
                    (instance = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(platformAssetPath)) == null) {

                    instance = Object.Instantiate(referencingObject);
                    AssetDatabase.CreateAsset(instance, platformAssetPath);
                    Debug.LogFormat("Created new platform instance of: {0} at path: \"{1}\" to path: \"{2}\".", instance.GetType().FullName, objectAssetPath, platformAssetPath);
                }

                appliedMemberValue |= ApplyMemberValue(ref referenceInfo, instance, replacementPath, ref prevDependency);

                if (instance != nextDependency.dependencyUnityObj) {
                    UpdateDependencyTree(dependencyLut, ref nextDependency, instance, instance, platformAssetPath);
                    updatedDependencyTree |= true;
                }

                return true;
            }
        }

        else {

            GameObject outermostPrefabRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(referencingObject);
            bool isPrefabInstance = PrefabUtility.IsPartOfPrefabInstance(referencingObject) && outermostPrefabRoot != null;

            string outermostPrefabAssetPath = null;
            if (!isPrefabInstance) {
                outermostPrefabAssetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(referencingObject);
                outermostPrefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(outermostPrefabAssetPath).transform.root.gameObject;
            }

            else {
                outermostPrefabAssetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(outermostPrefabRoot.transform.root.gameObject);
                outermostPrefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(outermostPrefabAssetPath);
            }

            GameObject instanceOfOutermostPrefab = null;
            if (!HasValidPlatformObject(platformObjPostfix, outermostPrefabAssetPath, out var outermostPlatformPrefabAssetPath) || 
                (instanceOfOutermostPrefab = AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(outermostPlatformPrefabAssetPath)) == null) {

                GameObject instantiatedPrefab = null;
                try {
                    instantiatedPrefab = PrefabUtility.InstantiatePrefab(outermostPrefabRoot) as GameObject;
                    instanceOfOutermostPrefab = PrefabUtility.SaveAsPrefabAsset(instantiatedPrefab, outermostPlatformPrefabAssetPath);
                } catch (System.Exception exception) {

                    Debug.LogErrorFormat("Exception occurred while attempting to save prefab asset to path: \"{0}\".", outermostPlatformPrefabAssetPath);
                    Debug.LogException(exception);

                    if (instantiatedPrefab != null)
                        Object.DestroyImmediate(instantiatedPrefab);

                    return false;
                }

                if (instantiatedPrefab != null) { 
                    Object.DestroyImmediate(instantiatedPrefab);
                    Debug.LogFormat("Created new platform prefab variant of: {0} at path: \"{1}\" to path: \"{2}\".", instanceOfOutermostPrefab.GetType().FullName, outermostPrefabAssetPath, outermostPlatformPrefabAssetPath);
                }
            }

            if (outermostPlatformPrefabAssetPath == replacementPath)
                return false;

            if (!GetCorrespondingObjectInPrefabVariant(instanceOfOutermostPrefab.transform, nextDependency.dependencyUnityObj, nextDependency.dependencyUnityObj is Component, out Object correspondingDependencyUnityObjInVariant)) {
                Debug.LogErrorFormat("Unable to find corresponding object of type: {0} between prefab: \"{1}\" and prefab variant: \"{2}\".", nextDependency.dependencyUnityObj.GetType().FullName, outermostPrefabAssetPath, outermostPlatformPrefabAssetPath);
                return false;
            }

            appliedMemberValue |= ApplyMemberValue(ref referenceInfo, correspondingDependencyUnityObjInVariant, replacementPath, ref prevDependency);

            if (nextDependency.dependencyUnityObj != correspondingDependencyUnityObjInVariant) { 
                UpdateDependencyTree(dependencyLut, ref nextDependency, correspondingDependencyUnityObjInVariant, instanceOfOutermostPrefab, outermostPlatformPrefabAssetPath);
                updatedDependencyTree |= true;
            }

            return true;
        }

        return false;
    }

    private void RegisterLeavesToReprocess (
        Dictionary<string, Dependency> dependencyLut, 
        List<string> leavesToReprocess,
        string platformObjPostfix, 
        string startingDependencyGuid) { 

        var dependency = dependencyLut[startingDependencyGuid];
        Dictionary<string, int> referenceIndices = new Dictionary<string, int>();
        Dictionary<int, string> levels = new Dictionary<int, string>();
        HashSet<string> processed = new HashSet<string>();

        int currentLevel = 0;
        while (!processed.Contains(dependency.dependencyGuid)) {

            processed.Add(dependency.dependencyGuid);
            if (dependency.isLeaf) { 
                leavesToReprocess.Add(startingDependencyGuid);

                if (!levels.ContainsKey(--currentLevel))
                    break;

                dependency = dependencyLut[levels[currentLevel]];
                continue;
            } 

            int refIndex = 0;
            if (!referenceIndices.ContainsKey(dependency.dependencyGuid))
                referenceIndices.Add(dependency.dependencyGuid, 0);
            else refIndex = referenceIndices[dependency.dependencyGuid]++;

            if (refIndex < dependency.references.Count) {

                referenceIndices[dependency.dependencyGuid] = refIndex;
                dependency = dependencyLut[dependency.references[refIndex]];

                if (!levels.ContainsKey(currentLevel))
                    levels.Add(currentLevel, dependency.dependencyGuid);
                else levels[currentLevel] = dependency.dependencyGuid;

                currentLevel++;
            }

            else  {

                if (!levels.ContainsKey(--currentLevel))
                    break;
                dependency = dependencyLut[levels[currentLevel]];
            }
        }
    }

    // private readonly HashSet<string> reconfigureCircularDependencyLookup = new HashSet<string>();
    // private void WalkUp (
    //     Dictionary<string, Dependency> dependencyLut, 
    //     List<string> leavesToReprocess,
    //     Dependency thisDependency, 
    //     string platformObjPostfix, 
    //     ref bool updatedDependencyChain, 
    //     ref bool appliedMemberValue) {

    //     if (
    //         thisDependency.referencingDependencyGuids == null || 
    //         thisDependency.referencingDependencyGuids.Count == 0)
    //         return;

    //     string[] keys = thisDependency.referencingDependencyGuids.Keys.ToArray();
    //     for (int referencingIndex = 0; referencingIndex < keys.Length; referencingIndex++)
    //     {
    //         var referencingGuid = keys[referencingIndex];
    //         var reference = thisDependency.referencingDependencyGuids[referencingGuid];
    //         if (reference.referencingMember == null)
    //             continue;

    //         bool isCircularDependency = reconfigureCircularDependencyLookup.Contains(referencingGuid);
    //         if (isCircularDependency)
    //             continue;

    //         reconfigureCircularDependencyLookup.Add(referencingGuid);

    //         var nextDependency = dependencyLut[referencingGuid];

    //         if (reference.referencingMember.Name.ToLower().Contains("_roomname"))
    //             Debug.Log("TEST");

    //         if (!reference.dissociativeReference)
    //             ReconfigureDependency(dependencyLut, platformObjPostfix, nextDependency.dependencyUnityObj, ref thisDependency, ref reference, ref nextDependency, ref updatedDependencyChain, ref appliedMemberValue);

    //         if (updatedDependencyChain && nextDependency.references != null) {
    //             /*
    //             RegisterLeavesToReprocess(
    //                 dependencyLut,
    //                 leavesToReprocess,
    //                 platformObjPostfix,
    //                 nextDependency.dependencyGuid);
    //             updatedDependencyChain = false;
    //             */
    //         }

    //         WalkUp(
    //             dependencyLut,
    //             leavesToReprocess,
    //             nextDependency,
    //             platformObjPostfix,
    //             ref updatedDependencyChain,
    //             ref appliedMemberValue);
    //     }
    // }

    private string cachedPlatformObjPostfix;
    public bool TryStartTraverseUpThroughDependencyTree (
        Dictionary<string, Dependency> dependencyLut, 
        string startingDependencyGuid, 
        DependencyTreeEnumerator.EnumerateDependencyTreeDelegate callback,
        out TraverseUpDependencyTree traverseUpEnumerator) { 

        traverseUpEnumerator = new TraverseUpDependencyTree(dependencyLut, startingDependencyGuid, callback);
        return traverseUpEnumerator.Start();

        /*
        bool updatedDependencyChain = false, appliedMemberValue = false;
        WalkUp(
            dependencyLut,
            leavesToReprocess,
            dependency,
            platformObjPostfix,
            ref updatedDependencyChain,
            ref appliedMemberValue);
        */
    }
}
