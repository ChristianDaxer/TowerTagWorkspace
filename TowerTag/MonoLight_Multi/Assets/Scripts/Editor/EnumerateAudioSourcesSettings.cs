using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;
using UnityEngine.SceneManagement;

using UnityEditor;
using UnityEditor.SceneManagement;

public class EnumerateAudioSourcesSettings : ScriptableObject
{
    public const string SettingsPath = "Assets/ScriptableObjects/EnumerateAudioSourcesSettings.asset";
    public readonly string[] PrefabSearchPaths = new string[1]
    {
        "Assets/Prefabs",
    };

    private class AudioSourceInfo
    {

    }

    [System.Serializable]
    private class AudioSourceInstanceInfo
    {
        [SerializeField] public AudioSource audioSource;
        public AudioSourceInstanceInfo (AudioSource audioSource) => this.audioSource = audioSource;
    }

    [System.Serializable]
    private class AudioSourceInSceneInfo : AudioSourceInstanceInfo
    {
        [SerializeField] public SceneAsset sceneAsset;
        public AudioSourceInSceneInfo (AudioSource audioSource, SceneAsset sceneAsset) : base (audioSource) => this.sceneAsset = sceneAsset;
    }

    [System.Serializable]
    private class ReferencedAudioSourceInSceneInfo : AudioSourceInSceneInfo
    {
        [SerializeField] public Component component;

        [SerializeField] private string[] cachedComponentFieldsReferencingSource;
        [SerializeField] private string[] cachedComponentPropertiesReferencingSource;

        private FieldInfo[] componentFieldsReferencingSource;
        private PropertyInfo[] componentPropertiesReferencingSource;

        public ReferencedAudioSourceInSceneInfo (
            AudioSource audioSource, 
            SceneAsset sceneAsset, 
            Component component,
            FieldInfo[] componentFieldsReferencingSource,
            PropertyInfo[] componentPropertiesReferencingSource) : base(audioSource, sceneAsset)
        {
            this.component = component;

            this.cachedComponentFieldsReferencingSource = componentFieldsReferencingSource.Select(componentFieldInfo => componentFieldInfo.Name).ToArray();
            this.cachedComponentPropertiesReferencingSource = componentPropertiesReferencingSource.Select(componentPropertyInfo => componentPropertyInfo.Name).ToArray();

            this.componentFieldsReferencingSource = componentFieldsReferencingSource;
            this.componentPropertiesReferencingSource = componentPropertiesReferencingSource;
        }

        public bool TryGetComponentFieldsReferencingSource (out FieldInfo[] fieldInfos)
        {
            fieldInfos = null;

            if (component == null)
                return false;

            Type type = component.GetType();

            if (componentFieldsReferencingSource == null)
            {
                if (cachedComponentFieldsReferencingSource == null || cachedComponentFieldsReferencingSource.Length == 0)
                    return false;

                componentFieldsReferencingSource = cachedComponentFieldsReferencingSource.Select(cachedComponentField =>
                {
                    FieldInfo fieldInfo = type.GetField(cachedComponentField);
                    if (fieldInfo == null)
                    {
                        Debug.LogErrorFormat("Unable to retrieve field: \"{0}\" from type: \"{1}\", it no longer exists.", cachedComponentField, type.FullName);
                        return null;
                    }

                    return fieldInfo;
                }).ToArray();

                fieldInfos = componentFieldsReferencingSource;
                if (fieldInfos.Length > 0)
                    return true;
            }

            fieldInfos = componentFieldsReferencingSource;
            return true;
        }

        public bool TryGetComponentPropertiesReferencingSource (out PropertyInfo[] propertyInfos)
        {
            propertyInfos = null;
            if (component == null)
                return false;

            Type type = component.GetType();

            if (componentFieldsReferencingSource == null)
            {
                if (cachedComponentFieldsReferencingSource == null || cachedComponentFieldsReferencingSource.Length == 0)
                    return false;

                componentPropertiesReferencingSource = cachedComponentPropertiesReferencingSource.Select(cachedComponentProperty =>
                {
                    PropertyInfo propertyInfo = type.GetProperty(cachedComponentProperty);
                    if (propertyInfo == null)
                    {
                        Debug.LogErrorFormat("Unable to retrieve property: \"{0}\" from type: \"{1}\", it no longer exists.", cachedComponentProperty, type.FullName);
                        return null;
                    }

                    return propertyInfo;
                }).ToArray();

                propertyInfos = componentPropertiesReferencingSource;
                if (propertyInfos.Length > 0)
                    return true;
            }

            propertyInfos = componentPropertiesReferencingSource;
            return true;
        }
    }

    private readonly List<AudioSourceInSceneInfo> unreferencedAudioSourcesInScenesInfo = new List<AudioSourceInSceneInfo>();
    private readonly List<ReferencedAudioSourceInSceneInfo> referencedAudioSourcesInScenesInfo = new List<ReferencedAudioSourceInSceneInfo>();

    private readonly Dictionary<SceneAsset, (MonoBehaviour, int)> scriptInstantiatedAudioSources = new Dictionary<SceneAsset, (MonoBehaviour, int)>();

    private readonly Dictionary<Component, (FieldInfo[], PropertyInfo[])> cachedComponentsInScene = new Dictionary<Component, (FieldInfo[], PropertyInfo[])>();

    private static EnumerateAudioSourcesSettings _settings;
    public static EnumerateAudioSourcesSettings Settings
    {
        get
        {
            if (_settings == null)
                _settings = Create();
            return _settings;
        }
    }

    public static EnumerateAudioSourcesSettings Create ()
    {
        EnumerateAudioSourcesSettings settings = null;
        if ((settings = AssetDatabase.LoadAssetAtPath<EnumerateAudioSourcesSettings>(SettingsPath)) == null)
        {
            settings = EnumerateAudioSourcesSettings.CreateInstance<EnumerateAudioSourcesSettings>();
            AssetDatabase.CreateAsset(settings, SettingsPath);
        }

        return settings;
    }

    public void AppendSceneAudioSource (SceneAsset sceneAsset, AudioSource audioSource)
    {
        Dictionary<Component, (List<FieldInfo>, List<PropertyInfo>)> referencingMembers = new Dictionary<Component, (List<FieldInfo>, List<PropertyInfo>)>();

        cachedComponentsInScene.ForEach(componentMembersPair =>
        {
            // Loop through all FIELDS within the component and determine if it references our AudioSource.
            componentMembersPair.Value.Item1.ForEach(fieldInfo =>
            {
                if (audioSource != (UnityEngine.Object)fieldInfo.GetValue(componentMembersPair.Key))
                    return;

                if (!referencingMembers.TryGetValue(componentMembersPair.Key, out var value))
                    referencingMembers.Add(componentMembersPair.Key, (new List<FieldInfo>() { fieldInfo }, new List<PropertyInfo>()));
                else value.Item1.Add(fieldInfo);
            });

            // Loop through all PROPERTIES within the component and determine if it references our AudioSource.
            componentMembersPair.Value.Item2.ForEach(propertyInfo =>
            {
                if (audioSource != (UnityEngine.Object)propertyInfo.GetValue(componentMembersPair.Key))
                    return;

                if (!referencingMembers.TryGetValue(componentMembersPair.Key, out var value))
                    referencingMembers.Add(componentMembersPair.Key, (new List<FieldInfo>(), new List<PropertyInfo>() { propertyInfo }));
                else value.Item2.Add(propertyInfo);
            });
        });

        if (referencingMembers.Count == 0)
        {
            unreferencedAudioSourcesInScenesInfo.Add(new AudioSourceInSceneInfo(audioSource, sceneAsset));
            return;
        }

        referencingMembers.ForEach(componentReferencingMembers =>
        {
            referencedAudioSourcesInScenesInfo.Add(new ReferencedAudioSourceInSceneInfo(
                audioSource,
                sceneAsset,
                componentReferencingMembers.Key,
                componentReferencingMembers.Value.Item1.ToArray(),
                componentReferencingMembers.Value.Item2.ToArray()));
        });
    }

    public void Cache ()
    {
        Type audioSourceType = typeof(AudioSource);
        var scenes = EditorBuildSettings.scenes;
        for (int sceneBuildIndex = 0; sceneBuildIndex < scenes.Length; sceneBuildIndex++)
        {
            var scene = scenes[sceneBuildIndex];

            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);
            EditorSceneManager.LoadScene(sceneBuildIndex, LoadSceneMode.Single);
            var activeScene = SceneManager.GetActiveScene();

            var componentsInScene = FindObjectsOfType<Component>();
            var referencerInScene = componentsInScene.FirstOrDefault(component => component is EnumerateAudioSourcesInScene);
            if (referencerInScene == null)
            {
                GameObject go = new GameObject("EnumerateAudioSourcesInScene");
                go.AddComponent<EnumerateAudioSourcesInScene>();
            }

            var audioSourcesInScene = componentsInScene
                .Select(component => component as AudioSource)
                .Where(audioSource => audioSource != null);
        }
    }
}
