using System;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public struct MemberArrayWrapper
{
    public MemberInfo memberInfo;
    public bool isField;

    public FieldInfo AsField => memberInfo as FieldInfo;
    public PropertyInfo AsProperty => memberInfo as PropertyInfo;
}

public static class ReflectionUtilities
{
    public static bool FieldIsArray (FieldInfo fieldInfo)
    {
        return
            fieldInfo.FieldType.IsArray ||
            (fieldInfo.FieldType.IsGenericType && fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(List<>));
    }

    public static bool PropertyIsArray (PropertyInfo propertyInfo)
    {
        return
            propertyInfo.PropertyType.IsArray ||
            (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(List<>));
    }

    public static bool FieldIsType (FieldInfo fieldInfo, Type targetType)
    {
        return 
            // Derrives from target type.
            fieldInfo.FieldType.IsSubclassOf(targetType) || 
            fieldInfo.FieldType == targetType || 
            // Array of target type.
            (fieldInfo.FieldType.IsArray && 
                (fieldInfo.FieldType.GetElementType().IsSubclassOf(targetType) || fieldInfo.FieldType.GetElementType() == targetType)) ||
            // List<TargetType>
            (fieldInfo.FieldType.IsGenericType && fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(List<>));
    }

    public static bool PropertyIsType (PropertyInfo propertyInfo, Type targetType)
    {
        return 
            propertyInfo.PropertyType.IsSubclassOf(targetType) || 
            propertyInfo.PropertyType == targetType || 
            (propertyInfo.PropertyType.IsArray && 
                (propertyInfo.PropertyType.GetElementType().IsSubclassOf(targetType) || propertyInfo.PropertyType.GetElementType() == targetType)) ||
            (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(List<>));
    }

    public static (FieldInfo[], PropertyInfo[]) GetAllSerializableFieldsInClassInheritanceHierarchy (Type targetType, Type filterType = null)
    {
        // Get all fields in the component that are public & private.
        List<FieldInfo> fields = targetType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).ToList();
        List<PropertyInfo> properties = targetType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).ToList();

        List<FieldInfo> serializedFields = new List<FieldInfo>();
        List<PropertyInfo> serializedProperties = new List<PropertyInfo>();

        System.Type baseType = targetType.BaseType;

        // Walk up the inheritance tree.
        while (baseType != typeof(object))
        {
            // Get all fields in the base class that are public and private and add them to the list.
            fields.AddRange(baseType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
            properties.AddRange(baseType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(propertyInfo => propertyInfo.GetGetMethod(true) != null));
            baseType = baseType.BaseType;
        }

        // Remove any duplicate fields. A field of a sepcific type and access 
        // from class A and class B can be considered as a the same field.
        var distinctFields = fields.Distinct();
        var distinctProperties = properties.Distinct();

        foreach (var field in distinctFields)
        {
            // Ignore any fields that are (private & and do not have a SerializeField attribute) or 
            // any fields that are not an UnityEngine.object type, or any fields that are blacklisted.
            if ((!field.IsPublic && field.GetCustomAttribute<SerializeField>() == null))
                continue;

            if (filterType != null && !FieldIsType(field, filterType))
                continue;

            serializedFields.Add(field);
        }

        foreach (var property in distinctProperties) {
            if (filterType != null && !PropertyIsType(property, filterType))
                continue;

            serializedProperties.Add(property);
        }

        return (serializedFields.ToArray(), serializedProperties.ToArray());
    }

    public static bool GetFieldValue (FieldInfo fieldInfo, object instance, out object value) {
        value = fieldInfo.GetValue(instance);
        return true;
    }

    public static bool GetPropertyValue (PropertyInfo propertyInfo, object instance, out object value) {
        value = null;
        if (!propertyInfo.CanRead)
            return false;

        try { // Often we are just iterating over properties, so it makes sense to wrap this just in case an exception is thrown.
            value = propertyInfo.GetValue(instance);
        } catch (System.Exception _) {
            value = null; // Set value to null just in case.
            return false; 
        }

        return true;
    }

    public static void EnumerateBothFieldValuesAndPropertyValues ((IEnumerable<object> fieldValues, IEnumerable<object> propertyValues) fieldAndPropertyValues, Action<object, bool> callback) { 
        foreach (var fieldValue in fieldAndPropertyValues.fieldValues)
            callback(fieldValue, true);
        foreach (var propertyValue in fieldAndPropertyValues.propertyValues)
            callback(propertyValue, false);
    }

    public static void EnumerateSystemObjectAsUnityEngineObjectArray (object obj, Action<UnityEngine.Object, int> callback) {
        System.Type valueType = obj.GetType();

        // Determine whether the referencing member is an array or List.
        if (valueType.IsArray || valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(List<>)) {

            // We can iterate over the array or List as a IEnumerable.
            var array = obj as IEnumerable<UnityEngine.Object>;
            if (array == null)
                return;

            for (int i = 0; i < array.Count(); i++) {

                UnityEngine.Object element = array.ElementAt(i);

                if (element == null)
                    continue;

                callback(element, i);
            }
        }
    }

    // Verify that the property is NOT a material(s) or mesh property since this will result in a leak.
    public static bool PropertyIsNotMaterialOrMesh (PropertyInfo propertyInfo) {
        if (propertyInfo.DeclaringType == typeof(Collider) &&
            (propertyInfo.Name == "mesh" || propertyInfo.Name == "material"))
            return false;

        else if (propertyInfo.DeclaringType == typeof(MeshFilter) &&
            propertyInfo.Name == "mesh")
            return false;

        else if (propertyInfo.DeclaringType == typeof(Renderer) &&
        (propertyInfo.Name.ToLower() == "materials" || propertyInfo.Name.ToLower() == "material"))
            return false;

        return true;
    }

    // Verify that the material has the _MainTex property before accessing the .mainTexture material property.
    public static bool ValidateMaterialMainTexShaderProperty(UnityEngine.Object referencingInstance, PropertyInfo propertyInfo) {
        // If the property type is not a material, then it does not have the .mainTexture material property in question.
        if (propertyInfo.DeclaringType != typeof(Material))
            return true;

        // If the property is indeed the .mainTexture member of Material.
        if (propertyInfo.Name != "mainTexture")
            return true;

        // Determine whether the shader has the _MainTex property to avoid exceptions.
        Material material = referencingInstance as Material;
        return material.HasProperty("_MainTex");
    }

    public static bool SafelyGetAllMemberInfoAndValuesOfObjectTypeForObject (
        UnityEngine.Object obj, 
        out (IEnumerable<FieldInfo> fields, IEnumerable<PropertyInfo> properties) fieldsAndProperties, 
        out (IEnumerable<object> fieldValues, IEnumerable<object> propertyValues) fieldAndPropertyValues ) {

        if (!ReflectionUtilities.GetAllUnityObjectReferencesByObject(obj, out fieldsAndProperties))
        {
            fieldAndPropertyValues = (new object[0], new object[0]);
            return false;
        }

        fieldAndPropertyValues.fieldValues = fieldsAndProperties.Item1.Select(fieldInfo => {
            ReflectionUtilities.GetFieldValue(fieldInfo, obj, out var value);
            return value;
        });

        // Ignore any properties that generate instances like mesh/materials.
        fieldsAndProperties.properties = fieldsAndProperties.properties.Where(propertyInfo =>
        {
            return
                propertyInfo.CanRead &&
                propertyInfo.CanWrite &&
                PropertyIsNotMaterialOrMesh(propertyInfo) &&
                ValidateMaterialMainTexShaderProperty(obj, propertyInfo);
        });

        fieldAndPropertyValues.propertyValues = fieldsAndProperties.properties.Select(propertyInfo => // Retrieve values from properties.
        {
            ReflectionUtilities.GetPropertyValue(propertyInfo, obj, out var value);
            return value;
        });;

        return fieldAndPropertyValues.fieldValues.Count() > 0 || fieldAndPropertyValues.propertyValues.Count() > 0;
    }


    public static bool GetMemberValueObjectPath<T> (T memberInfo, object instance, out string objectPath) where T : MemberInfo {
        object value = null;
        objectPath = null;

        if (memberInfo is FieldInfo)
            if (!GetFieldValue(memberInfo as FieldInfo, instance, out value))
                return false;

        else if (memberInfo is PropertyInfo)
            if (!GetPropertyValue(memberInfo as PropertyInfo, instance, out value))
                return false;

        else return false;

        if (value == null)
            return false;

        UnityEngine.Object gameObject = value as UnityEngine.Object;
        if (gameObject == null)
            return false;

        objectPath = AssetDatabase.GetAssetPath(gameObject);
        return !string.IsNullOrEmpty(objectPath);
    }

    public static bool GetAllUnityObjectReferencesByObject (UnityEngine.Object obj, out (IEnumerable<FieldInfo>, IEnumerable<PropertyInfo>) fieldAndProperties) {
        System.Type objType = obj.GetType();
        fieldAndProperties = GetAllSerializableFieldsInClassInheritanceHierarchy(objType, typeof(UnityEngine.Object));
        if (fieldAndProperties.Item1.Count() == 0 && fieldAndProperties.Item2.Count() == 0)
            return false;
        return true;
    }

    // Simply determine whether the Object is a GameObject or Component.
    public static bool ObjectIsGameObjectOrComponent(UnityEngine.Object obj) => obj is GameObject || obj is Component;

    // Try cast the Object to to GameObject or Component.
    public static bool TryGetGameObjectFromObject(UnityEngine.Object obj, out GameObject go) {
        go = null;
        return obj != null &&
            (go = obj is Component ?
            (obj as Component).gameObject :
            obj as GameObject) != null;
    }
}
