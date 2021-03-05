using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(RebindFieldsToTarget))]
public class RemindFieldsToTargetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        RebindFieldsToTarget rebinder = target as RebindFieldsToTarget;
        if (GUILayout.Button("Collect Object References"))
            rebinder.CollectPotentialObjectsToReference(true);

        if (GUILayout.Button("Rebind"))
            rebinder.Rebind();

        if ((rebinder.rebindMapping == null || rebinder.rebindMapping.Length == 0) &&
            (rebinder.rebindArrayMapping == null || rebinder.rebindArrayMapping.Length == 0))
            EditorGUILayout.LabelField("No rebindings available.", EditorStyles.boldLabel);

        else
        {
            if (rebinder.rebindArrayMapping != null)
            {
                for (int i = 0; i < rebinder.rebindArrayMapping.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Referencing Component", GUILayout.Width(150));
                    EditorGUILayout.ObjectField(rebinder.rebindArrayMapping[i].component, typeof(Component), true);
                    EditorGUILayout.EndHorizontal();

                    if (rebinder.rebindArrayMapping[i].defaultValues == null)
                        continue;

                    bool isArray = rebinder.rebindArrayMapping[i].ValueType.IsArray;

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Variable Name: ", GUILayout.Width(100));
                    EditorGUILayout.LabelField(rebinder.rebindArrayMapping[i].variableName);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.LabelField("Previous", GUILayout.Width(65));
                    {
                        Object[] defaults = rebinder.rebindArrayMapping[i].defaultValues;
                        for (int oi = 0; oi < defaults.Length; oi++)
                            EditorGUILayout.ObjectField(defaults[oi], defaults[oi].GetType(), true);
                    }
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.Space(10);

                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.LabelField("New", GUILayout.Width(30));

                    {
                        Object[] defaultsOverriden = rebinder.rebindArrayMapping[i].overridingValues;
                        for (int oi = 0; oi < defaultsOverriden.Length; oi++)
                        {
                            EditorGUILayout.BeginHorizontal();
                            Object newObj = EditorGUILayout.ObjectField(defaultsOverriden[oi], defaultsOverriden[oi].GetType(), true);
                            if (newObj != defaultsOverriden[oi])
                            {
                                Object prefabObj = PrefabUtility.GetCorrespondingObjectFromSource(newObj);
                                rebinder.rebindArrayMapping[i].overridingValues[oi] = prefabObj;
                                EditorUtility.SetDirty(rebinder);
                            }

                            if (GUILayout.Button("Prefab"))
                            {
                                string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(newObj);
                                Selection.objects = new Object[1] { AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) };
                            }

                            EditorGUILayout.EndHorizontal();
                        }
                    }

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();

                }
            }

            if (rebinder.rebindMapping != null)
            {
                EditorGUILayout.Space(5);
                EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 5), Color.gray);
                EditorGUILayout.Space(5);

                for (int i = 0; i < rebinder.rebindMapping.Length; i++)
                {
                    if (rebinder.rebindMapping[i].defaultValue == null)
                        continue;

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Referencing Component", GUILayout.Width(150));
                    EditorGUILayout.ObjectField(rebinder.rebindMapping[i].component, typeof(Component), true);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Variable Name: ", GUILayout.Width(100));
                    EditorGUILayout.LabelField(rebinder.rebindMapping[i].variableName);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.ObjectField(rebinder.rebindMapping[i].defaultValue, typeof(Object), true);
                    EditorGUILayout.Space(10);
                    EditorGUILayout.LabelField("New", GUILayout.Width(30));
                    Object obj = EditorGUILayout.ObjectField(rebinder.rebindMapping[i].overridingValue as Object, rebinder.rebindMapping[i].ValueType, true);

                    if (obj != rebinder.rebindMapping[i].overridingValue)
                    {
                        Object prefabObj = PrefabUtility.GetCorrespondingObjectFromSource(obj);
                        rebinder.rebindMapping[i].overridingValue = prefabObj;
                        EditorUtility.SetDirty(rebinder);
                    }

                    if (GUILayout.Button("Prefab"))
                    {
                        string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj);
                        Selection.objects = new Object[1] { AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) };
                    }

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space(5);
                    EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 5), Color.gray);
                    EditorGUILayout.Space(5);
                }
            }
        }
    }
}

// These are separate structs for easy serialization.
[System.Serializable]
public struct RebindMap
{
    public string variableName;

    public FieldInfo fieldInfo;
    public PropertyInfo propertyInfo;

    public Component component;

    public System.Type ValueType { get { return defaultValue.GetType(); } }

    public Object defaultValue;
    public Object overridingValue;
}

[System.Serializable]
public struct RebindMapArray
{
    public string variableName;

    public FieldInfo fieldInfo;
    public PropertyInfo propertyInfo;

    public Component component;

    public System.Type ValueType { get { return defaultValues.GetType(); } }

    public Object[] defaultValues;
    public Object[] overridingValues;
}

public class RebindFieldsToTarget : MonoBehaviour
{
    public GameObject dummyHierarchy;

    [HideInInspector]
    public RebindMap[] rebindMapping;
    [HideInInspector]
    public RebindMapArray[] rebindArrayMapping;

    private struct ComponentsContainer
    {
        public bool isArray;
        public List<Object> components;
    }

    private bool FieldIsObjectType (FieldInfo fieldInfo)
    {
        return 
            fieldInfo.FieldType.IsSubclassOf(typeof(Object)) || 
            fieldInfo.FieldType == typeof(Object) || 
            (fieldInfo.FieldType.IsArray && 
                (fieldInfo.FieldType.GetElementType().IsSubclassOf(typeof(Object)) || fieldInfo.FieldType.GetElementType() == typeof(Object)));
    }

    private bool PropertyIsObjectType (PropertyInfo propertyInfo)
    {
        return 
            propertyInfo.PropertyType.IsSubclassOf(typeof(Object)) || 
            propertyInfo.PropertyType == typeof(Object) || 
            (propertyInfo.PropertyType.IsArray && 
                (propertyInfo.PropertyType.GetElementType().IsSubclassOf(typeof(Object)) || propertyInfo.PropertyType.GetElementType() == typeof(Object)));
    }

    private bool IsBlacklistedType (System.Type type)
    {
        return
            type.IsSubclassOf(typeof(Mesh)) || type == typeof(Mesh) ||
            type.IsSubclassOf(typeof(Material)) || type == typeof(Mesh);
    }

    public void CollectPotentialObjectsToReference(bool applyNewValues)
    {
        // Get all components in the dummy hierarchy and cast them to Unity objects.
        List<Object> components = dummyHierarchy.GetComponentsInChildren<Component>().Cast<Object>().ToList();

        // We use these mappings to map a field or property to a set of associated components inside of the dummy hierarchy.
        Dictionary<FieldInfo, ComponentsContainer> fieldToComponentsMapping = new Dictionary<FieldInfo, ComponentsContainer>();
        Dictionary<PropertyInfo, ComponentsContainer> propertyToComponentsMapping = new Dictionary<PropertyInfo, ComponentsContainer>();

        // Get all components in the scene.
        List<Component> allComponentsExceptForDummyComponents = GameObject.FindObjectsOfType<Component>().ToList();

        // Subtract all dummy components from all components in the scene.
        for (int i = 0; i < components.Count; i++)
            allComponentsExceptForDummyComponents.Remove(components[i] as Component);

        List<Object> componentsAndGameObjects = new List<Object>(components.Count);

        // Loop through dummy components and add both components and gameobjects into a single list.
        for (int i = 0; i < components.Count; i++)
        {
            GameObject go = (components[i] as Component).gameObject;
            // Don't add duplicate gameobjects.
            if (componentsAndGameObjects.Contains(go))
                continue;

            componentsAndGameObjects.Add(go);
            componentsAndGameObjects.Add(components[i]);
        }

        List<FieldInfo> serializedFields = new List<FieldInfo>();

        // Loop through all components in the scene except for the dummy components.
        allComponentsExceptForDummyComponents.ForEach(component =>
        {
            // Get all fields in the component that are public & private.
            List<FieldInfo> fields = component.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).ToList();

            System.Type baseType = component.GetType().BaseType;

            // Walk up the inheritance tree.
            while (baseType != typeof(object))
            {
                // Get all fields in the base class that are public and private and add them to the list.
                fields.AddRange(baseType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
                baseType = baseType.BaseType;
            }

            // Remove any duplicate fields. A field of a sepcific type and access 
            // from class A and class B can be considered as a the same field.
            fields.Distinct();

            for (int i = 0; i < fields.Count; i++)
            {
                // Ignore any fields that are (private & and do not have a SerializeField attribute) or 
                // any fields that are not an UnityEngine.object type, or any fields that are blacklisted.
                if ((!fields[i].IsPublic && fields[i].GetCustomAttribute<SerializeField>() == null) || !FieldIsObjectType(fields[i]) || IsBlacklistedType(fields[i].FieldType))
                    continue;

                // If we are already mapping fields to certain components, just add the component to the mapping.
                if (fieldToComponentsMapping.ContainsKey(fields[i]))
                {
                    fieldToComponentsMapping[fields[i]].components.Add(component);
                    serializedFields.Add(fields[i]);
                    continue;
                }

                fieldToComponentsMapping.Add(fields[i], new ComponentsContainer 
                { 
                    components = new List<Object>() { component }, 
                    isArray = fields[i].FieldType.IsArray 
                });

                serializedFields.Add(fields[i]);
            }
        });

        List<PropertyInfo> serializedProperties = new List<PropertyInfo>();

        // We do basically the same here as what we were doing for fields, but instead this time we do them for properties.
        allComponentsExceptForDummyComponents.ForEach(c =>
        {
            PropertyInfo[] properties = c.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance);
            for (int i = 0; i < properties.Length; i++)
            {
                if (!PropertyIsObjectType(properties[i]) || IsBlacklistedType(properties[i].PropertyType))
                    continue;

                if (propertyToComponentsMapping.ContainsKey(properties[i]))
                {
                    propertyToComponentsMapping[properties[i]].components.Add(c);
                    serializedProperties.Add(properties[i]);
                    continue;
                }

                propertyToComponentsMapping.Add(properties[i], new ComponentsContainer
                {
                    components = new List<Object>() { c },
                    isArray = properties[i].PropertyType.IsArray
                });

                serializedProperties.Add(properties[i]);
            }
        });

        List<FieldInfo> fieldsReferencingDummyObjects = new List<FieldInfo>();

        // Now lets loop through all our selected fields.
        foreach (FieldInfo fieldInfo in serializedFields)
        {
            // Branch into fields that are arrays or single objects.
            if (fieldInfo.FieldType.IsArray)
            {
                try
                {
                    ComponentsContainer componentsMatchingFields = fieldToComponentsMapping[fieldInfo];

                    // Loop through all components that have this field.
                    for (int i = 0; i < componentsMatchingFields.components.Count; i++)
                    {
                        // Get the array from the field.
                        Object[] referenceArray = fieldInfo.GetValue(componentsMatchingFields.components[i]) as Object[];
                        if (referenceArray == null)
                            continue;

                        // Loop through all the refefences and determine if we have a reference to one of our dummy objects. 
                        for (int ri = 0; ri < referenceArray.Length; ri++)
                        {
                            if (componentsAndGameObjects.Contains(referenceArray[ri]))
                                fieldsReferencingDummyObjects.Add(fieldInfo); // Store them for later.
                        }
                    }

                } catch { continue; } // I know this looks bad, but this can avoid issues with deprecated fields throwing exceptions.

                continue;
            }

            try
            {
                ComponentsContainer componentsMatchingFields = fieldToComponentsMapping[fieldInfo];
                for (int i = 0; i < componentsMatchingFields.components.Count; i++)
                {
                    Object reference = fieldInfo.GetValue(componentsMatchingFields.components[i]) as Object;
                    if (reference == null)
                        continue;

                    // Determine if our single reference is one of our dummy objects.
                    if (componentsAndGameObjects.Contains(reference))
                        fieldsReferencingDummyObjects.Add(fieldInfo);
                }
            } catch { continue; }

        }

        /*
        for (int i = 0; i < fieldsReferencingDummyObjects.Count; i++)
            Debug.Log(fieldsReferencingDummyObjects[i].Name);
        */

        List<PropertyInfo> propertiesReferencingDummyObjects = new List<PropertyInfo>();

        // Now we do the exact same thing for properties.
        foreach (PropertyInfo propertyInfo in serializedProperties)
        {
            if (propertyInfo.GetSetMethod() == null || !propertyInfo.GetSetMethod().IsPublic)
                continue;

            if (propertyInfo.PropertyType.IsArray)
            {
                try
                {
                    ComponentsContainer componentsMatchingProperties = propertyToComponentsMapping[propertyInfo];
                    for (int i = 0; i < componentsMatchingProperties.components.Count; i++)
                    {
                        Object[] referenceArray = propertyInfo.GetValue(componentsMatchingProperties.components[i]) as Object[];
                        if (referenceArray == null)
                            continue;

                        for (int ri = 0; ri < referenceArray.Length; ri++)
                        {
                            if (componentsAndGameObjects.Contains(referenceArray[ri]))
                                propertiesReferencingDummyObjects.Add(propertyInfo);
                        }
                    }

                } catch { continue; }

                continue;
            }

            try
            {
                ComponentsContainer componentsMatchingProperties = propertyToComponentsMapping[propertyInfo];
                for (int i = 0; i < componentsMatchingProperties.components.Count; i++)
                {
                    Object reference = propertyInfo.GetValue(componentsMatchingProperties.components[i]) as Object;
                    if (reference == null)
                        continue;

                    if (componentsAndGameObjects.Contains(reference))
                        propertiesReferencingDummyObjects.Add(propertyInfo);
                }

            } catch { continue; }
        }

        int mappingCount = 0;
        int arrayMappingCount = 0;

        // This isn't great, but here were looping through all the selected fields and summing the component
        // counts so we know how many bindings we have.
        for (int i = 0; i < fieldsReferencingDummyObjects.Count; i++)
        {
            ComponentsContainer container = fieldToComponentsMapping[fieldsReferencingDummyObjects[i]];
            if (container.isArray)
                arrayMappingCount += container.components.Count;
            else mappingCount += container.components.Count;
        }
        for (int i = 0; i < propertiesReferencingDummyObjects.Count; i++)
        {
            ComponentsContainer container = propertyToComponentsMapping[propertiesReferencingDummyObjects[i]];
            if (container.isArray)
                arrayMappingCount += container.components.Count;
            else mappingCount += container.components.Count;
        }

        if (rebindMapping == null || rebindMapping.Length != mappingCount)
        {
            RebindMap[] newMapping = new RebindMap[mappingCount];
            rebindMapping = newMapping;
        }

        if (rebindArrayMapping == null || rebindArrayMapping.Length != arrayMappingCount)
        {
            RebindMapArray[] newMapping = new RebindMapArray[arrayMappingCount];
            rebindArrayMapping = newMapping;
        }

        Debug.LogFormat("Mapped {0} object references and {1} array references between target prefab and containing prefab.", mappingCount, arrayMappingCount);

        int arrayMappingIndex = 0;
        int mappingIndex = 0;

        // Loop through all selected fields.
        for (int i = 0; i < fieldsReferencingDummyObjects.Count; i++)
        {
            // Get components mapped to each field.
            ComponentsContainer componentsUsingField = fieldToComponentsMapping[fieldsReferencingDummyObjects[i]];

            // Loop through all components mapped to the field.
            foreach (Object component in componentsUsingField.components)
            {
                // Get the previously stored value.
                object previousValue = fieldsReferencingDummyObjects[i].GetValue(component);

                // Branch if our reference is a reference to an array.
                if (previousValue.GetType().IsArray)
                {
                    rebindArrayMapping[arrayMappingIndex].defaultValues = previousValue as Object[];

                    if (applyNewValues)
                    {
                        rebindArrayMapping[arrayMappingIndex].overridingValues = new Object[rebindArrayMapping[arrayMappingIndex].defaultValues.Length];
                        System.Array.Copy(rebindArrayMapping[arrayMappingIndex].defaultValues, rebindArrayMapping[arrayMappingIndex].overridingValues, rebindArrayMapping[arrayMappingIndex].defaultValues.Length);
                    }

                    // Store data.
                    rebindArrayMapping[arrayMappingIndex].variableName = fieldsReferencingDummyObjects[i].Name;
                    rebindArrayMapping[arrayMappingIndex].fieldInfo = fieldsReferencingDummyObjects[i];
                    rebindArrayMapping[arrayMappingIndex].component = component as Component;
                    arrayMappingIndex++;
                }

                else
                {
                    rebindMapping[mappingIndex].defaultValue = previousValue as Object;
                    if (applyNewValues)
                        rebindMapping[mappingIndex].overridingValue = previousValue as Object;

                    rebindMapping[mappingIndex].variableName = fieldsReferencingDummyObjects[i].Name;
                    rebindMapping[mappingIndex].fieldInfo = fieldsReferencingDummyObjects[i];
                    rebindMapping[mappingIndex].component = component as Component;
                    mappingIndex++;
                }
            }
        }

        // Now do the same thing for properties.
        for (int i = 0; i < propertiesReferencingDummyObjects.Count; i++)
        {
            ComponentsContainer componentsUsingProperty = propertyToComponentsMapping[propertiesReferencingDummyObjects[i]];
            foreach (Object component in componentsUsingProperty.components)
            {
                object previousValue = propertiesReferencingDummyObjects[i].GetValue(component);

                if (previousValue.GetType().IsArray)
                {
                    rebindArrayMapping[arrayMappingIndex].defaultValues = previousValue as Object[];
                    if (applyNewValues)
                    {
                        rebindArrayMapping[arrayMappingIndex].overridingValues = new Object[rebindArrayMapping[arrayMappingIndex].defaultValues.Length];
                        System.Array.Copy(rebindArrayMapping[arrayMappingIndex].defaultValues, rebindArrayMapping[arrayMappingIndex].overridingValues, rebindArrayMapping[arrayMappingIndex].defaultValues.Length);
                    }

                    rebindArrayMapping[arrayMappingIndex].variableName = propertiesReferencingDummyObjects[i].Name;
                    rebindArrayMapping[arrayMappingIndex].propertyInfo = propertiesReferencingDummyObjects[i];
                    rebindArrayMapping[arrayMappingIndex].component = component as Component;
                    arrayMappingIndex++;
                }

                else
                {
                    rebindMapping[mappingIndex].defaultValue = previousValue as Object;
                    if (applyNewValues)
                        rebindMapping[mappingIndex].overridingValue = previousValue as Object;

                    rebindMapping[mappingIndex].variableName = propertiesReferencingDummyObjects[i].Name;
                    rebindMapping[mappingIndex].propertyInfo = propertiesReferencingDummyObjects[i];
                    rebindMapping[mappingIndex].component = component as Component;
                    mappingIndex++;
                }
            }
        }

        EditorUtility.SetDirty(this);
    }

    private bool FindMatchingObjectInPrefabInstance(Object obj, GameObject prefabInstanceRoot, out Object objInstance)
    {
        objInstance = null;

        if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out string guid, out long localID))
        {
            if (obj is GameObject)
            {
                var gameObjects = prefabInstanceRoot.GetComponentsInChildren<Component>().Select(component => component.gameObject).Distinct();
                foreach (GameObject gameObject in gameObjects)
                {
                    if (PrefabUtility.GetCorrespondingObjectFromSource(gameObject) == obj)
                    {
                        objInstance = gameObject;
                        return true;
                    }
                }

                return false;
            }

            var components = prefabInstanceRoot.GetComponentsInChildren<Component>();
            foreach (Component component in components)
            {
                if (PrefabUtility.GetCorrespondingObjectFromSource(component) == obj)
                {
                    objInstance = component;
                    return true;
                }
            }
        }

        return false;
    }

    private void FindMatchingObjectInPrefabs (Object obj, int objIndex, Component component, FieldInfo fieldInfo, PropertyInfo propertyInfo, GameObject[] gameObjects)
    {
        string referencePrefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj);
        for (int gi = 0; gi < gameObjects.Length; gi++)
        {
            if (!PrefabUtility.IsPartOfAnyPrefab(gameObjects[gi]))
                continue;

            string goPrefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObjects[gi]);
            if (string.IsNullOrEmpty(goPrefabPath))
                continue;

            if (goPrefabPath == referencePrefabPath)
            {
                if (FindMatchingObjectInPrefabInstance(obj, PrefabUtility.GetNearestPrefabInstanceRoot(gameObjects[gi]), out var objInstance))
                {
                    if (fieldInfo != null)
                    {
                        try
                        {
                            if (fieldInfo.FieldType.IsArray)
                            {
                                Object[] objs = fieldInfo.GetValue(component) as Object[];
                                objs[objIndex] = objInstance;
                                fieldInfo.SetValue(component, objs);
                            }

                            else fieldInfo.SetValue(component, objInstance);

                        }
                        catch (System.Exception exception)
                        {
                            Debug.LogErrorFormat("Failed to apply overriding value to field: {0}, the following exception occurred", fieldInfo.Name);
                            Debug.LogException(exception);
                        }
                    }

                    if (propertyInfo != null)
                    {
                        try
                        {
                            if (propertyInfo.PropertyType.IsArray)
                            {
                                Object[] objs = propertyInfo.GetValue(component) as Object[];
                                objs[objIndex] = objInstance;
                                propertyInfo.SetValue(component, objs);
                            }

                            else propertyInfo.SetValue(component, objInstance);
                        } catch (System.Exception exception)
                        {
                            Debug.LogErrorFormat("Failed to apply overriding value to property: {0}, the following exception occurred", propertyInfo.Name);
                            Debug.LogException(exception);
                        }
                    }

                    break;
                }
            }
        }
    }

    public void Rebind ()
    {
        CollectPotentialObjectsToReference(false);
        GameObject[] gameObjects = GameObject.FindObjectsOfType<GameObject>();
        for (int i = 0; i < rebindMapping.Length; i++)
            FindMatchingObjectInPrefabs(rebindMapping[i].overridingValue, 0, rebindMapping[i].component, rebindMapping[i].fieldInfo, rebindMapping[i].propertyInfo, gameObjects);

        for (int i = 0; i < rebindArrayMapping.Length; i++)
            for (int oi = 0; oi < rebindArrayMapping[i].overridingValues.Length; oi++)
                FindMatchingObjectInPrefabs(rebindArrayMapping[i].overridingValues[oi], oi, rebindArrayMapping[i].component, rebindArrayMapping[i].fieldInfo, rebindArrayMapping[i].propertyInfo, gameObjects);
    }
}
#else
public class RebindFieldsToTarget : MonoBehaviour
{
    public void Rebind () {}
    public void CollectPotentialObjectsToReference(bool applyNewValues) {}
}
#endif
