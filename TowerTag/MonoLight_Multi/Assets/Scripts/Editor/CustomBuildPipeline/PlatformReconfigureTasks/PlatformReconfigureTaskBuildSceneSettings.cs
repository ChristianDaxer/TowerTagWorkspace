using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.EditorCoroutines.Editor;

[CustomEditor(typeof(PlatformReconfigureTaskBuildSceneSettings))]
public class PlatformReconfigureTaskBuildSceneSettingsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var task = target as PlatformReconfigureTaskBuildSceneSettings;
        if (task == null)
            return;

        EditorGUILayout.LabelField("Platform Scenes", EditorStyles.boldLabel);
        if (GUILayout.Button(task.Baking ? "Cancel Baking All Staged Scenes" : "Bake All Staged Scenes (Slow)"))
            task.BakeAll();

        var scenes = task.Scenes;
        if (scenes != null)
        {
            for (int i = 0; i < scenes.Length; i++)
                scenes[i].Present();
        }

        EditorGUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Create Scene Reference"))
            task.CreateSceneReference();

        if (GUILayout.Button("Create Composed Scene Reference"))
            task.CreateComposedSceneReference();

        EditorGUILayout.EndHorizontal();
    }
}

[CustomEditor(typeof(SceneWrapper))]
public class SceneWrapperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Test"))
            Debug.Log("Test");
    }
}

public abstract class SceneWrapper 
{
    public PlatformReconfigureTaskBuildSceneSettings _container { get; private set; }
    public SceneWrapper (PlatformReconfigureTaskBuildSceneSettings container) => this._container = container;
    public abstract string Name { get; }
    public abstract string StagedScenePath { get; }
    public abstract string[] SourceScenePaths { get; }

    [SerializeField] protected bool includeInBuild = true;
    public bool IncludeInBuild => includeInBuild;

    public abstract SceneAsset Stage();
    public abstract void Present();
    public abstract void Select();
    public SceneAsset GetStagedSceneAsset () => AssetDatabase.LoadAssetAtPath<SceneAsset>(StagedScenePath);

    protected void PresentBegin ()
    {
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space(5);
        EditorGUILayout.BeginHorizontal();
        PresentButtons();
        EditorGUILayout.Space(5);
    }

    protected void PresentEnd()
    {
        EditorGUILayout.BeginVertical();
        PresentCheckMark();
        PresentRemove();
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }

    private void PresentSort () {
        GUILayout.ExpandWidth(false);
        EditorGUILayout.BeginVertical();
        if (GUILayout.Button("▲", GUILayout.Width(25)))
            _container.MoveUp(this);
        if (GUILayout.Button("▼", GUILayout.Width(25)))
            _container.MoveDown(this);
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(10);
    }

    private void PresentButtons () {
        PresentSort();
        EditorGUILayout.BeginVertical();
        if (GUILayout.Button("Open Staged")) {
            string stagedScenePath = StagedScenePath;
            if (!string.IsNullOrEmpty(stagedScenePath))
                EditorSceneManager.OpenScene(stagedScenePath);
        }

        if (GUILayout.Button("Select"))
            Select();

        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical();
        if (GUILayout.Button("Stage"))
            Stage();

        if (GUILayout.Button("Reconfigure")) {
            if (CustomBuildPipeline.TowerTagBuildPipeline.GetFirstInstance(out var pipeline))
                if (PlatformConfigScriptableObject.GetFirstInstanceOfPlatformConfig(pipeline.HomeType, out var platformConfig)) {
                    var wrappedSceneReconfigureTasks = _container.SceneReconfigureTasks
                        .Select(sceneReconfigureTask =>
                        {
                            var wrapper = PlatformSceneReconfigureTaskWrapper.CreateInstance<PlatformSceneReconfigureTaskWrapper>();
                            wrapper.Setup(sceneReconfigureTask, this);
                            return wrapper;
                        });
                        
                    if (!wrappedSceneReconfigureTasks.Any(wrapper => (!wrapper.ValidSceneAsset || !wrapper.ValidPlatformSceneReconfigureTask)))
                        platformConfig.ExecuteSpecificTasks(wrappedSceneReconfigureTasks.ToArray(), pipeline.HomeType, null);
                }
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical();
        if (GUILayout.Button("Open Source Scene")) {
            string[] sourceSceneAssetPaths = SourceScenePaths;
            if (sourceSceneAssetPaths != null && sourceSceneAssetPaths.Length > 0) {
                EditorSceneManager.OpenScene(sourceSceneAssetPaths[0], OpenSceneMode.Single);
                for (int i = 1; i < sourceSceneAssetPaths.Length; i++)
                    EditorSceneManager.OpenScene(sourceSceneAssetPaths[i], OpenSceneMode.Additive);
            }
        }

        if (GUILayout.Button("Bake Lighting")) {
            _container.BakeScene(StagedScenePath);
        }

        EditorGUILayout.EndVertical();
    }

    private void PresentCheckMark () {
        GUILayout.ExpandWidth(false);
        includeInBuild = EditorGUILayout.Toggle(includeInBuild);
    }

    private void PresentRemove () {
        GUILayout.ExpandWidth(false);
        if (GUILayout.Button("X", GUILayout.Width(25))) {
            int result = EditorUtility.DisplayDialogComplex("Remove Scene", "Are you sure you want to remove this scene?", "Yes", "Cancel", "Remove Scene");
            if (result == 0 || result == 2)
                _container.Remove(this);
        }
    }
}

public class SceneTarget : SceneWrapper {
    private SceneAsset _sceneAsset;
    public SceneTarget(PlatformReconfigureTaskBuildSceneSettings container) : base(container) { }
    public SceneTarget(PlatformReconfigureTaskBuildSceneSettings container, SceneAsset sceneAsset) : base(container) => this._sceneAsset = sceneAsset;

    public override string Name => _sceneAsset != null ? _sceneAsset.name : "null";

    public SceneAsset SerializedSceneAsset => _sceneAsset;

    public override string StagedScenePath => SerializedSceneAsset != null ? $"Assets/Scenes/Staged/{SerializedSceneAsset.name} (DO NOT EDIT).unity" : null;

    public override string[] SourceScenePaths => _sceneAsset != null ? new string[1] { AssetDatabase.GetAssetPath(_sceneAsset) } : new string[0];

    public override SceneAsset Stage()
    {
        string stagedScenePath = StagedScenePath;
        if (string.IsNullOrEmpty(stagedScenePath))
            return null;

        AssetDatabase.DeleteAsset(stagedScenePath);

        Scene stageScene = EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(_sceneAsset), OpenSceneMode.Single);

        if (!Directory.Exists(Path.GetDirectoryName(stagedScenePath)))
            Directory.CreateDirectory(Path.GetDirectoryName(stagedScenePath));

        EditorSceneManager.SaveScene(stageScene, stagedScenePath);
        // EditorSceneManager.CloseScene(stageScene, false);

        AssetDatabase.Refresh();

        return AssetDatabase.LoadAssetAtPath<SceneAsset>(stagedScenePath);
    }

    public override void Present()
    {
        PresentBegin();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Scene Asset", EditorStyles.boldLabel, GUILayout.Width(300));
        var newSceneAsset = EditorGUILayout.ObjectField(_sceneAsset, typeof(SceneAsset), false, GUILayout.Width(300)) as SceneAsset;
        EditorGUILayout.EndVertical();

        if (newSceneAsset != _sceneAsset)
        {
            _sceneAsset = newSceneAsset;
            _container.Dirty();
        }
        PresentEnd();
    }

    public override void Select()
    {
        Selection.activeObject = SerializedSceneAsset;
    }
}

public class ComposedSceneTarget : SceneWrapper {
    private SceneComposer _sceneComposer;
    public SceneComposer sceneComposer => _sceneComposer;

    public override string Name => _sceneComposer != null ? _sceneComposer.name : "null";

    public override string StagedScenePath => _sceneComposer != null ? _sceneComposer.StagedScenePath : null;

    public override string[] SourceScenePaths
    {
        get
        {
            var sceneAssetPaths = _sceneComposer.scenes.Select(sceneAsset => AssetDatabase.GetAssetPath(sceneAsset));
            if (sceneAssetPaths.Any(sceneAssetPath => string.IsNullOrEmpty(sceneAssetPath)))
                return new string[0];
            return sceneAssetPaths.ToArray();
        }
    }

    public ComposedSceneTarget (PlatformReconfigureTaskBuildSceneSettings container) : base (container) {}
    public ComposedSceneTarget(PlatformReconfigureTaskBuildSceneSettings container, SceneComposer sceneComposer) : base(container) => this._sceneComposer = sceneComposer;

    public override SceneAsset Stage() => _sceneComposer.Stage();

    public override void Present()
    {
        PresentBegin();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Scene Composition", EditorStyles.boldLabel, GUILayout.Width(300));
        if (_sceneComposer != null)
            EditorGUILayout.LabelField(_sceneComposer.compositionName, GUILayout.Width(300));
        var newSceneComposer = EditorGUILayout.ObjectField(_sceneComposer, typeof(SceneComposer), false, GUILayout.Width(300)) as SceneComposer;
        EditorGUILayout.EndVertical();

        if (newSceneComposer != _sceneComposer)
        {
            _sceneComposer = newSceneComposer;
            _container.Dirty();
        }

        PresentEnd();
    }

    public override void Select()
    {
        Selection.activeObject = _sceneComposer;
    }
}

[System.Serializable]
public struct SerializedSceneTarget
{
    public int index;
    public SceneAsset sceneAsset;
}

[System.Serializable]
public struct SerializedComposedSceneTarget
{
    public int index;
    public SceneComposer sceneComposer;
}

[CreateAssetMenu(fileName = "ConfigureTaskBuildSceneSettings", menuName = "ScriptableObjects/Platform Build Tasks/Configure Task Build Scene Settings", order = 1)]
public class PlatformReconfigureTaskBuildSceneSettings : PlatformReconfigureTaskScriptableObject, IPlatformReconfigureTask, IPlatformReconfigureTaskDescriptor, ISerializationCallbackReceiver
{
    [SerializeField] public PlatformSceneReconfigureTaskScriptableObject[] sceneReconfigureTasks;
    public PlatformSceneReconfigureTaskScriptableObject[] SceneReconfigureTasks => sceneReconfigureTasks;
    private SceneWrapper[] _scenes;

    [UnityEngine.Serialization.FormerlySerializedAs("serializedSceneTargets")]
    [HideInInspector][SerializeField] private SerializedSceneTarget[] _serializedSceneTargets;

    [UnityEngine.Serialization.FormerlySerializedAs("serializedComposedSceneTargets")]
    [HideInInspector][SerializeField] private SerializedComposedSceneTarget[] _serializedComposedSceneTargets;

    public SceneWrapper[] Scenes => _scenes;

    private string taskStagingDescriptionFormat = "Staging {0}: \"{1}\".";
    private string taskSavingDescriptionFormat = "Saving \"{0}\".";
    private string taskSceneReconfigureDescriptionFormat = "Reconfiguring staged scene \"{0}\":\n{1}";
    private string taskDescription;
    public string TaskDescription => taskDescription;

    public void Dirty () => EditorUtility.SetDirty(this);

    private void Append (SceneWrapper sceneWrapper)
    {
        var newSceneArray = new SceneWrapper[_scenes == null ? 1 : _scenes.Length + 1];
        newSceneArray[newSceneArray.Length - 1] = sceneWrapper;
        if (_scenes != null)
            System.Array.Copy(_scenes, newSceneArray, Scenes.Length);
        _scenes = newSceneArray;
        Dirty();
    }

    private bool IndexOf (SceneWrapper sceneWrapper, out int indexOf)
    {
        indexOf = 0;
        for (; indexOf < _scenes.Length; indexOf++)
            if (_scenes[indexOf] == sceneWrapper)
                break;

        if (indexOf > _scenes.Length - 1)
        {
            indexOf = -1;
            return false;
        }

        return true;
    }

    private void Swap (int from, int to)
    {
        var temp = _scenes[to];
        _scenes[to] = _scenes[from];
        _scenes[from] = temp;
    }
    public void Remove (SceneWrapper sceneWrapper)
    {
        var sceneList = _scenes.ToList();
        sceneList.Remove(sceneWrapper);
        _scenes = sceneList.ToArray();
        Dirty();
    }

    public void MoveUp (SceneWrapper sceneWrapper)
    {
        if (!IndexOf(sceneWrapper, out var indexOf))
            return;

        int targetIndex = indexOf - 1;
        if (targetIndex < 0)
            return;

        Swap(indexOf, targetIndex);
        Dirty();
    }

    public void MoveDown (SceneWrapper sceneWrapper)
    {
        if (!IndexOf(sceneWrapper, out var indexOf))
            return;

        int targetIndex = indexOf + 1;
        if (targetIndex > _scenes.Length - 1)
            return;

        Swap(indexOf, targetIndex);
        Dirty();
    }

    public void CreateSceneReference()
    {
        SceneTarget sceneTarget = new SceneTarget(this);
        Append(sceneTarget);
    }

    public void CreateComposedSceneReference ()
    {
        ComposedSceneTarget sceneTarget = new ComposedSceneTarget(this);
        Append(sceneTarget);
    }

    public IEnumerator ProcessScene (HomeTypes homeType, SceneWrapper scene, List<EditorBuildSettingsScene> sceneAssets, System.Action<bool> processedSceneCallback)
    {
        SceneAsset sceneAsset = scene.Stage();
        if (sceneAsset == null)
        {
            if (processedSceneCallback != null)
                processedSceneCallback(false);
            yield break;
        }

        if (scene.IncludeInBuild)
            sceneAssets.Add(new EditorBuildSettingsScene(AssetDatabase.GetAssetPath(sceneAsset), true));

        if (processedSceneCallback != null)
            processedSceneCallback(true);
    }

    public IEnumerator Reconfigure(HomeTypes homeType, System.Action<IPlatformReconfigureTaskDescriptor> startTaskCallback, System.Action<bool> taskCallback)
    {
        if (_scenes == null)
        {
            Debug.LogWarning("No scenes to configure.");
            if (taskCallback != null)
                taskCallback(false);

            yield break;
        }

        if (startTaskCallback != null)
            startTaskCallback(this);

        yield return null;

        List<EditorBuildSettingsScene> sceneAssets = new List<EditorBuildSettingsScene>();

        bool success = true;
        for (int i = 0; i < _scenes.Length; i++)
        {
            if (PlatformConfigScriptableObject.ShowTaskDescription)
            {
                taskDescription = string.Format(taskStagingDescriptionFormat, _scenes[i] is SceneTarget ? "scene" : "scene composition", _scenes[i].Name);
                PlatformConfigScriptableObject.RepaintInspectorGUI();
            }

            yield return null;

            yield return ProcessScene(homeType, _scenes[i], sceneAssets, (completedProcessScene) =>
            {
                success &= completedProcessScene;
                if (success)
                {
                    if (_scenes[i] is SceneTarget)
                        Debug.LogFormat("Successfully staged scene: \"{0}\".", _scenes[i].Name);
                    else Debug.LogFormat("Successfully staged scene composition: \"{0}\".", _scenes[i].Name);
                }
            });

            if (!success)
            {
                if (_scenes[i] is SceneTarget)
                    Debug.LogErrorFormat("Unable to stage scene: \"{0}\".", _scenes[i].Name);
                else Debug.LogErrorFormat("Unable to stage scene composition: \"{0}\".", _scenes[i].Name);

                if (taskCallback != null)
                    taskCallback(false);
                yield break;
            }

            yield return null;
            if (sceneReconfigureTasks != null)
            {
                bool sceneReconfigureTaskSuccess = true;
                for (int ri = 0; ri < sceneReconfigureTasks.Length; ri++)
                {
                    var wrapper = PlatformSceneReconfigureTaskWrapper.CreateInstance<PlatformSceneReconfigureTaskWrapper>();
                    wrapper.Setup(sceneReconfigureTasks[ri], _scenes[i]);

                    yield return wrapper.Reconfigure(homeType, startTaskCallback, (sceneTaskResult) =>
                    {
                        if (PlatformConfigScriptableObject.ShowTaskDescription)
                        {
                            taskDescription = string.Format(taskSceneReconfigureDescriptionFormat, _scenes[i].Name, sceneReconfigureTasks[ri].SceneTaskDescription);
                            PlatformConfigScriptableObject.RepaintInspectorGUI();
                        }

                        sceneReconfigureTaskSuccess &= sceneTaskResult;
                        if (sceneTaskResult)
                            Debug.LogFormat("Successfully executed scene reconfigure task of index: {0} of type: \"{1}\".", ri, sceneReconfigureTasks[ri].name);
                    });

                    if (!sceneReconfigureTaskSuccess)
                    {
                        Debug.LogErrorFormat("Failure occurred while executing scene reconfigure task of index: {0} of type: \"{1}\".", ri, sceneReconfigureTasks[ri].name);

                        if (taskCallback != null)
                            taskCallback(false);

                        yield break;
                    }
                }
            }

            taskDescription = string.Format(taskSavingDescriptionFormat, _scenes[i].Name);
            PlatformConfigScriptableObject.RepaintInspectorGUI();
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }

        EditorBuildSettings.scenes = sceneAssets.ToArray();
        if (taskCallback != null)
            taskCallback(true);
    }

    private EditorCoroutine bakeCoroutine;
    public bool Baking => bakeCoroutine != null;

    private bool bakeCompleted = false;
    private void OnBakeCompleted() => bakeCompleted = true;

    public bool BakeScene (string scenePath)
    {
        if (!File.Exists(scenePath))
        {
            Debug.LogWarningFormat("Cannot bake lighting in scene: \"{0}\", it has not been staged yet.", scenePath);
            return false;
        }

        // Lightmapping.Clear();
        // Lightmapping.ClearDiskCache();
        // Lightmapping.ClearLightingDataAsset();
        EditorSceneManager.OpenScene(scenePath);

        Debug.LogFormat("Starting light bake in scene: \"{0}\".", scenePath);

        // LightmapEditorSettings.lightmapper = LightmapEditorSettings.Lightmapper.ProgressiveGPU;
        Debug.LogFormat("Setting lightmapper to use {0}, GPU light baker is crashing.", LightmapEditorSettings.Lightmapper.ProgressiveCPU);
        LightmapEditorSettings.lightmapper = LightmapEditorSettings.Lightmapper.ProgressiveCPU;
        return Lightmapping.BakeAsync();
    }

    public IEnumerator BakeAllCoroutine ()
    {
        Lightmapping.bakeCompleted += OnBakeCompleted;
        int sceneIndex = 0;
        bakeCompleted = true;

        while (sceneIndex < Scenes.Length)
        {
            string stagedScenePath = Scenes[sceneIndex].StagedScenePath;

            bool success = BakeScene(stagedScenePath);
            if (!success)
            {
                Debug.LogErrorFormat("Failure occurred when attempting to start asynchronous bake for scene: \"{0}\", moving to next scene.", stagedScenePath);
                bakeCompleted = true;
            }

            while (!bakeCompleted)
                yield return null;

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

            bakeCompleted = false;
            sceneIndex++;
        }

        Lightmapping.bakeCompleted -= OnBakeCompleted;
        bakeCoroutine = null;
    }

    public void Cancel ()
    {
        Lightmapping.bakeCompleted -= OnBakeCompleted;
        Lightmapping.ForceStop();

        if (bakeCoroutine == null)
            return;

        EditorCoroutineUtility.StopCoroutine(bakeCoroutine);
        bakeCoroutine = null;
    }

    public void BakeAll ()
    {
        if (Baking)
        {
            Cancel();
            return;
        }

        bakeCoroutine = EditorCoroutineUtility.StartCoroutine(BakeAllCoroutine(), this);
    }

    public void OnBeforeSerialize()
    {
        if (_scenes == null)
            return;

        List<SerializedSceneTarget> listOfSceneTargets = new List<SerializedSceneTarget>();
        List<SerializedComposedSceneTarget> listOfComposedSceneTargets = new List<SerializedComposedSceneTarget>();
        for (int i = 0; i < _scenes.Length; i++)
        {
            if (_scenes[i] == null)
                continue;

            if (_scenes[i] is SceneTarget)
            {
                listOfSceneTargets.Add(new SerializedSceneTarget
                {
                    index = i,
                    sceneAsset = (_scenes[i] as SceneTarget).SerializedSceneAsset
                });

                continue;
            }

            listOfComposedSceneTargets.Add(new SerializedComposedSceneTarget
            {
                index = i,
                sceneComposer = (_scenes[i] as ComposedSceneTarget).sceneComposer
            });
        }

        _serializedSceneTargets = listOfSceneTargets.ToArray();
        _serializedComposedSceneTargets = listOfComposedSceneTargets.ToArray();

        EditorUtility.SetDirty(this);
    }

    public void OnAfterDeserialize()
    {
        SceneWrapper[] deserializedScenes = new SceneWrapper[_serializedSceneTargets.Length + _serializedComposedSceneTargets.Length];

        if (_serializedSceneTargets != null)
            for (int i = 0; i < _serializedSceneTargets.Length; i++)
                deserializedScenes[_serializedSceneTargets[i].index] = new SceneTarget(this, _serializedSceneTargets[i].sceneAsset);

        if (_serializedComposedSceneTargets != null)
            for (int i = 0; i < _serializedComposedSceneTargets.Length; i++)
                deserializedScenes[_serializedComposedSceneTargets[i].index] = new ComposedSceneTarget(this, _serializedComposedSceneTargets[i].sceneComposer);

        _scenes = deserializedScenes.ToArray();
    }
}
