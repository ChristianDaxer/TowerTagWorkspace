using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;
using System.Diagnostics;

[CustomEditor(typeof(PlatformConfigScriptableObject))]
public class PlatformConfigScriptableObjectEditor : Editor {
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var obj = target as PlatformConfigScriptableObject;
        if (obj == null)
            return;

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space(5);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Status: ({(obj.Processing ? "Processing" : "Idle")})", EditorStyles.boldLabel, GUILayout.Width(125));

        if (!obj.Processing)
        {
            if (GUILayout.Button("Reconfigure", GUILayout.Width(100)))
                obj.Reconfigure(obj._homeType, (success) =>
                {

                });
            EditorGUILayout.EndHorizontal();
        }

        else
        {
            if (GUILayout.Button("Cancel", GUILayout.Width(100)))
                obj.Cancel();

            EditorGUILayout.LabelField($"Task: {obj.TaskIndex+1}/{obj.TaskCount}", EditorStyles.boldLabel, GUILayout.Width(75));
            EditorGUILayout.LabelField($"{((obj.TaskIndex / (float)obj.TaskCount) * 100).ToString()}%", EditorStyles.boldLabel, GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();

            if (PlatformConfigScriptableObject.ShowTaskDescription)
                EditorGUILayout.LabelField($"Task Description: \"{(obj.Processing && obj.CurrentTaskDescriptor != null ? obj.CurrentTaskDescriptor.TaskDescription : "No Task")}\".", EditorStyles.wordWrappedLabel);
        }
    }
}

[CreateAssetMenu(fileName = "PlatformBuildConfig", menuName = "ScriptableObjects/Platform Build Config", order = 1)]
public class PlatformConfigScriptableObject : ScriptableObject
{
    private readonly static Dictionary<HomeTypes, PlatformConfigScriptableObject> cachedPlatformConfigs = new Dictionary<HomeTypes, PlatformConfigScriptableObject>();
    private static PlatformConfigScriptableObject currentPlatformConfig;
    public static bool GetFirstInstanceOfPlatformConfig (HomeTypes homeType, out PlatformConfigScriptableObject platformConfig)
    {
        if (!cachedPlatformConfigs.TryGetValue(homeType, out platformConfig))
        {
            var foundPlatformConfig = AssetDatabase.FindAssets($"t:{nameof(PlatformConfigScriptableObject)}")
                .Select(guid => AssetDatabase.LoadAssetAtPath<PlatformConfigScriptableObject>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(obj => obj != null && obj._homeType == homeType)
                .FirstOrDefault();

            if (foundPlatformConfig == null)
                return false;

            platformConfig = foundPlatformConfig;
            cachedPlatformConfigs.Add(homeType, platformConfig);
        }

        return platformConfig != null;
    }

    public HomeTypes _homeType;

    public static bool ShowTaskDescription => currentPlatformConfig != null ? currentPlatformConfig._showTaskDescription : false;
    [SerializeField] private bool _showTaskDescription;

    public PlatformReconfigureTaskScriptableObject[] reconfigureTasks;

    private IPlatformReconfigureTaskDescriptor _currentTaskDescriptor;
    public IPlatformReconfigureTaskDescriptor CurrentTaskDescriptor => _currentTaskDescriptor;

    public bool Processing => _processing;
    private bool _processing = false;

    private int _taskCount = 0;
    public int TaskCount => _taskCount;

    public int TaskIndex => _taskIndex;
    private int _taskIndex = 0;

    private EditorCoroutine editorCouroutine;

    private static EditorWindow cachedInspectorWindow;
    public static void RepaintInspectorGUI ()
    {
        if (cachedInspectorWindow != null)
            cachedInspectorWindow.Repaint();

        else
        {
            System.Type inspectorWindowType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow");
            if (inspectorWindowType != null)
            {
                cachedInspectorWindow = EditorWindow.GetWindow(inspectorWindowType);
                if (cachedInspectorWindow != null)
                    cachedInspectorWindow.Repaint();
            }

        }

    }

    public void Cancel ()
    {
        UnityEngine.Debug.LogFormat("Canceled platform reconfigure.");
        if (editorCouroutine != null)
            EditorCoroutineUtility.StopCoroutine(editorCouroutine);
        _processing = false;
        currentPlatformConfig = null;
    }

    public static void ShowInspector ()
    {
        System.Type inspectorWindowType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow");
        EditorWindow inspectorWindow = null;
        if (inspectorWindowType != null)
        {
            inspectorWindow = EditorWindow.GetWindow(inspectorWindowType);
            if (inspectorWindow != null)
                inspectorWindow.Show();
        }
    }

    private void Initialize (int taskCount)
    {
        ShowInspector();
        UnityEngine.Debug.LogFormat("Starting reconfigure for platform: \"{0}\".", _homeType);

        _processing = true;

        _taskIndex = 0;
        _taskCount = taskCount;

        currentPlatformConfig = this;
    }

    private void StartTaskCallback(IPlatformReconfigureTaskDescriptor incomingTaskDescriptor) => _currentTaskDescriptor = incomingTaskDescriptor;

    private IEnumerator ExecuteAllTasks (HomeTypes homeType, IPlatformReconfigureTask[] tasks, System.Action<string> taskStatus)
    {
        yield return null;
        Initialize(tasks != null ? tasks.Length : 0);

        IPlatformReconfigureTask[] localScopeTasks = tasks;
        bool success = true;

        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();

        for (int i = 0; i < localScopeTasks.Length; i++)
        {
            IPlatformReconfigureTask localScopeTask = tasks[i];
            yield return localScopeTask.Reconfigure(homeType, StartTaskCallback, (bool taskResult) => success &= taskResult);

            if (!success)
            {
                UnityEngine.Debug.LogFormat("Failed task at index: {0}", i);
                break;
            }

            _taskIndex++;
            if (_taskIndex > TaskCount)
                _taskIndex = TaskCount;

            yield return null;
        }

        stopWatch.Stop();
        UnityEngine.Debug.LogFormat("Finished all reconfigure tasks for platform: \"{0}\" in: {1}:{2}:{3}",
            homeType.ToString(),
            Mathf.FloorToInt(((stopWatch.ElapsedMilliseconds / 1000.0f) / 60.0f) % 60).ToString("00"),
            Mathf.FloorToInt((stopWatch.ElapsedMilliseconds / 1000.0f) % 60).ToString("00"),
            Mathf.FloorToInt(stopWatch.ElapsedMilliseconds % 1000).ToString("000"));


        currentPlatformConfig = null;
        _processing = false;
    }

    public void Reconfigure(HomeTypes homeType, System.Action<string> taskStatus)
    {
        if (_processing)
        {
            UnityEngine.Debug.LogFormat("Reconfigure has not finished for platform: \"{0}\".", homeType);
            return;
        }

        if (reconfigureTasks == null || reconfigureTasks.Length == 0)
            return;

        Selection.activeObject = this;
        editorCouroutine = EditorCoroutineUtility.StartCoroutine(ExecuteAllTasks(homeType, reconfigureTasks.Select(task => task as IPlatformReconfigureTask).ToArray(), taskStatus), this);
    }

    public void ExecuteSpecificTasks (IPlatformReconfigureTask[] tasks, HomeTypes homeType, System.Action<string> taskStatus)
    {
        if (_processing || currentPlatformConfig != null)
        {
            UnityEngine.Debug.LogFormat("Reconfigure has not finished.");
            return;
        }

        if (tasks == null || tasks.Length == 0)
            return;

        Selection.activeObject = this;
        editorCouroutine = EditorCoroutineUtility.StartCoroutine(ExecuteAllTasks(homeType, tasks, taskStatus), this);
    }
}
