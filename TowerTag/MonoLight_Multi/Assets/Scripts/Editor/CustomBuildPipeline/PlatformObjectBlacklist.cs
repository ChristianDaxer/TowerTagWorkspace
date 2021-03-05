using System.Linq;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlatformObjectBlacklist))]
public class PlatformObjectBlacklistEditor : Editor
{
    private string[] cachedTypes;
    private string filter;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var obj = target as PlatformObjectBlacklist;
        if (obj == null)
            return;

        if (GUILayout.Button("Refresh"))
            cachedTypes = PlatformObjectBlacklist.GetAllTypes(filter);

        string[] serializedBlacklistedTypes = obj.SerializedBlacklistedTypeNames;
        List<string> list = new List<string>();
        int count = list.Count;

        if (serializedBlacklistedTypes != null && serializedBlacklistedTypes.Length > 0)
        {
            list = obj.SerializedBlacklistedTypeNames.ToList();
            count = list.Count();

            if (count != 0)
            {
                for (int i = 0; i < serializedBlacklistedTypes.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Remove", GUILayout.Width(100)))
                        list.Remove(serializedBlacklistedTypes[i]);
                    EditorGUILayout.LabelField(serializedBlacklistedTypes[i]);
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        string newFilter = EditorGUILayout.TextField(filter);
        if (newFilter != filter)
        {
            cachedTypes = PlatformObjectBlacklist.GetAllTypes(newFilter);
            filter = newFilter;
        }

        if (cachedTypes != null && cachedTypes.Length > 0)
        {
            for (int i = 0; i < cachedTypes.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Add", GUILayout.Width(50)))
                    if (!list.Contains(cachedTypes[i]))
                        list.Add(cachedTypes[i]);
                EditorGUILayout.LabelField(cachedTypes[i]);
                EditorGUILayout.EndHorizontal();
            }
        }

        if (list.Count != count)
            obj.ApplyBlacklistedTypes(list.ToArray());
    }
}

[CreateAssetMenu(fileName = "PlatformObjectBlacklist", menuName = "ScriptableObjects/Platform Build Tasks/Platform Object Blacklist", order = 1)]
public class PlatformObjectBlacklist : ScriptableObject
{
    [SerializeField][HideInInspector] private string[] serializedBlacklistedTypeNames;
    public string[] SerializedBlacklistedTypeNames => serializedBlacklistedTypeNames;

    private IEnumerable<Type> cachedBlacklistedTypes;

    public void ApplyBlacklistedTypes (string[] blacklistedTypes)
    {
        this.serializedBlacklistedTypeNames = blacklistedTypes;
        EditorUtility.SetDirty(this);
    }

    public bool InBlacklist (Type type)
    {
        if (cachedBlacklistedTypes == null)
            cachedBlacklistedTypes = serializedBlacklistedTypeNames
                .SelectMany(typeName => AppDomain.CurrentDomain.GetAssemblies()
                    .Select(assembly => assembly.GetType(typeName))
                    .Where(assemblyType => assemblyType != null)).ToArray();
        return cachedBlacklistedTypes.Contains(type);
    }

    public static string[] GetAllTypes (string filter)
    {
        Type[] types = new Type[1] {
            typeof(UnityEngine.Object)
        };

        string filterLower = !string.IsNullOrEmpty(filter) ? filter.ToLower() : null;
        return filterLower == null ? 

            AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type =>
            {
                bool found = false;
                for (int i = 0; i < types.Length; i++)
                    found |= type.IsSubclassOf(types[i]);
                return found;
            })
            .Select(type => type.FullName).ToArray() :

            AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => {
                bool found = false;
                for (int i = 0; i < types.Length; i++)
                    found |= type.IsSubclassOf(types[i]);
                return found;
            })
            .Select(type => type.FullName)

            .Where(fullTypeName => fullTypeName.ToLower().Contains(filterLower)).ToArray();
    }
}
