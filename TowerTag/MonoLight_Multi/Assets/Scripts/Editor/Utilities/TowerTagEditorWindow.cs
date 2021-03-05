using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CustomBuildPipeline;
using TowerTag;
using TowerTagSOES;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public class TowerTagEditorWindow : EditorWindow {
    private TowerTagBuildPipeline _buildPipeline;
    private TeamManager _teamManager;
    private TowerTagSettings _towerTagSettings;
    private SceneAsset _initSceneAsset;
    private Vector2 _scrollPos;
    private static int _mapIndex;

    private static SceneComposer[] sceneComposers;

    private static Dictionary<string, string> _mapOptions = new Dictionary<string, string> {
        {"EL_Cebitus", "Assets/Scenes/GameScenes/Maps/3v3_EL_Cebitus.unity"},
        {"EL_Everest", "Assets/Scenes/GameScenes/Maps/1v1_EL_Everest.unity"},
        {"EL_Maze", "Assets/Scenes/GameScenes/Maps/2v2_EL_Maze.unity"},
        {"EL_Rig", "Assets/Scenes/GameScenes/Maps/4v4_EL_Rig.unity"},
        {"EL_Elbtunnel", "Assets/Scenes/GameScenes/Maps/4v4_EL_Elbtunnel.unity"},
        {"GT_Sneaky", "Assets/Scenes/GameScenes/Maps/4v4_GT_Sneaky.unity"},
        {"GT_Dome", "Assets/Scenes/GameScenes/Maps/3v3_GT_Dome.unity"},
        {"GT_Millerntor", "Assets/Scenes/GameScenes/Maps/2v2_GT_Millerntor.unity"},
        {"DM_Cebitus", "Assets/Scenes/GameScenes/Maps/3v3_DM_Cebitus.unity"},
        {"DM_Shield", "Assets/Scenes/GameScenes/Maps/4v4_DM_Shield.unity"}
    };

    [MenuItem("Window/Tower Tag")]
    public static void ShowWindow() {
        GetWindow<TowerTagEditorWindow>();
    }

    private void OnGUI() {
        // asset references
        _scrollPos = GUILayout.BeginScrollView(_scrollPos, false, false);

        EditorGUILayout.LabelField("Assets", EditorStyles.boldLabel);
        _buildPipeline = _buildPipeline
            ? _buildPipeline
            : (TowerTagBuildPipeline) AssetDatabase.LoadAssetAtPath(
                "Assets/ScriptableObjects/CustomBuildPipeline/TowerTagBuildPipeline.asset",
                typeof(TowerTagBuildPipeline));
        _towerTagSettings = _towerTagSettings
            ? _towerTagSettings
            : (TowerTagSettings) AssetDatabase.LoadAssetAtPath(
                "Assets/ScriptableObjects/GameManagement/TowerTagSettings.asset",
                typeof(TowerTagSettings));
        _teamManager = _teamManager
            ? _teamManager
            : (TeamManager) AssetDatabase.LoadAssetAtPath($"Assets/ScriptableObjects/Player/Team/TeamManager_{_buildPipeline.HomeType.ToString().ToLower()}",
                typeof(TeamManager));

        EditorGUILayout.ObjectField("Build Pipeline", _buildPipeline, typeof(TowerTagBuildPipeline), false);
        EditorGUILayout.ObjectField("Team Manager", _teamManager, typeof(TeamManager), false);
        EditorGUILayout.ObjectField("Tower Tag Settings", _towerTagSettings, typeof(TowerTagSettings), false);
        /*
        EditorGUILayout.ObjectField("Start Scene", EditorSceneManager.playModeStartScene, typeof(SceneAsset), false);
        EditorGUILayout.Space();
        */

        /*
        EditorGUILayout.LabelField("Run Tower Tag", EditorStyles.boldLabel);
        if (!EditorApplication.isPlaying)
            ShowStartOptions();
        else {
            ShowStopOptions();
        }

        //ShowSplashScreenOptions();
        ShowSceneOptions();
        */
        ShowBuildOptions();

        ShowReconfigureOptions();
        ShowSceneCompositionOptions();

        GUILayout.EndScrollView();
    }

    private void ShowStopOptions() {
        Color backgroundColor = GUI.backgroundColor;
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Stop")) {
            EditorApplication.isPlaying = false;
        }

        GUI.backgroundColor = backgroundColor;
    }

    private void ShowStartOptions() {
        Color backgroundColor = GUI.backgroundColor;
        GUI.backgroundColor = Color.green;

        if (sceneComposers != null && sceneComposers.Length > 0)
        {
            string[] scenes = sceneComposers.Select(sceneComposer => sceneComposer.compositionName).ToArray();

            if (GUILayout.Button("Start Operator")) {
                SharedControllerType.Singleton.Set(this, ControllerType.Admin);
                EditorSceneManager.playModeStartScene = (SceneAsset) AssetDatabase.LoadAssetAtPath(
                    "Assets/Scenes/GameScenes/StartUpScene.unity",
                    typeof(SceneAsset));
                EditorApplication.isPlaying = true;
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Start VR")) {
                SharedControllerType.Singleton.Set(this, ControllerType.VR);
                sceneComposers[_buildPipeline.GetCompositionIndice(0)].Load();
                /* SEAN CONNOR commented this out for scene composing.
                EditorSceneManager.playModeStartScene = (SceneAsset) AssetDatabase.LoadAssetAtPath(
                    "Assets/Scenes/GameScenes/StartUpScene.unity",
                    typeof(SceneAsset));
                */
                EditorApplication.isPlaying = true;
            }

            {
                EditorGUILayout.LabelField("Scene Composition: ", GUILayout.Width(120));
                int newCompositionIndice = EditorGUILayout.Popup(_buildPipeline.GetCompositionIndice(0), scenes, GUILayout.Width(100));
                if (newCompositionIndice != _buildPipeline.GetCompositionIndice(0))
                    _buildPipeline.SetCompositionIndice(0, newCompositionIndice);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Start FPS")) {
                SharedControllerType.Singleton.Set(this, ControllerType.NormalFPS);
                sceneComposers[_buildPipeline.GetCompositionIndice(1)].Load();
                /* SEAN CONNOR commented this out for scene composing.
                EditorSceneManager.playModeStartScene = (SceneAsset) AssetDatabase.LoadAssetAtPath(
                    "Assets/Scenes/GameScenes/StartUpScene.unity",
                    typeof(SceneAsset));
                */
                EditorApplication.isPlaying = true;
            }

            {
                EditorGUILayout.LabelField("Scene Composition: ", GUILayout.Width(120));
                int newCompositionIndice = EditorGUILayout.Popup(_buildPipeline.GetCompositionIndice(1), scenes, GUILayout.Width(100));
                if (newCompositionIndice != _buildPipeline.GetCompositionIndice(1))
                    _buildPipeline.SetCompositionIndice(1, newCompositionIndice);
            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Start Spectator")) {
                SharedControllerType.Singleton.Set(this, ControllerType.Spectator);
                EditorSceneManager.playModeStartScene = (SceneAsset) AssetDatabase.LoadAssetAtPath(
                    "Assets/Scenes/GameScenes/StartUpScene.unity",
                    typeof(SceneAsset));
                EditorApplication.isPlaying = true;
            }
        }

        GUI.backgroundColor = backgroundColor;
        EditorGUILayout.Space();
    }

    [InitializeOnLoad]
    public class SceneHelper {
        static SceneHelper() {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange change) {
            if (change == PlayModeStateChange.ExitingPlayMode) {
                EditorSceneManager.playModeStartScene = null;
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            }
        }
    }

    private static void ShowSceneOptions() {
        EditorGUILayout.LabelField("Scenes", EditorStyles.boldLabel);

        if (GUILayout.Button("StartUp")) {
            EditorSceneManager.OpenScene("Assets/Scenes/GameScenes/StartUpScene.unity");
        }
        
        if (GUILayout.Button("License Activation")) {
            EditorSceneManager.OpenScene("Assets/Scenes/GameScenes/License Activation.unity");
        }
        
        if (GUILayout.Button("Tutorial")) {
            EditorSceneManager.OpenScene("Assets/Scenes/GameScenes/Tutorial.unity");
        }

        if (GUILayout.Button("Hub")) {
            EditorSceneManager.OpenScene("Assets/Scenes/GameScenes/Hubscene_Obsidian.unity");
        }

        if (GUILayout.Button("Main Menu")) {
            EditorSceneManager.OpenScene("Assets/Scenes/GameScenes/Main_Menu.unity");
        }

        if (GUILayout.Button("Commendations")) {
            EditorSceneManager.OpenScene("Assets/Scenes/GameScenes/Commendations.unity");
        }

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Map");
        _mapIndex = EditorGUILayout.Popup(_mapIndex, _mapOptions.Keys.ToArray());
        if (GUILayout.Button("Open")) {
            EditorSceneManager.OpenScene(_mapOptions.Values.ToArray()[_mapIndex]);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
    }

    private static bool GetAllSceneComposers (out SceneComposer[] sceneComposers)
    {
        sceneComposers = null;
        string[] assets = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(SceneComposer).Name));

        if (assets == null || assets.Length == 0)
            return false;

        sceneComposers = assets
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .Select(path => AssetDatabase.LoadAssetAtPath<SceneComposer>(path))
            .Where(asset => asset != null).OrderBy(sceneComposer => sceneComposer.compositionName).ToArray();
        return true;
    }

    private static bool GetSceneCompositionManager (out SceneCompositionManager manager)
    {
        manager = null;

        string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(SceneCompositionManager).Name));
        if (guids == null || guids.Length == 0)
            return false;

        manager = AssetDatabase.LoadAssetAtPath<SceneCompositionManager>(AssetDatabase.GUIDToAssetPath(guids[0]));
        return true;
    }

    private static SceneCompositionManager manager;
    private void ShowSceneCompositionOptions ()
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space(5);

        EditorGUILayout.LabelField("Platform Build Scene Settings", EditorStyles.boldLabel);
        if (PlatformConfigScriptableObject.GetFirstInstanceOfPlatformConfig(_buildPipeline.HomeType, out var platformConfig)) {
            var reconfigureTaskBuildSceneSettings = platformConfig.reconfigureTasks.Where(reconfigureTask => reconfigureTask is PlatformReconfigureTaskBuildSceneSettings).FirstOrDefault();
            if (reconfigureTaskBuildSceneSettings != null)
                Editor.CreateEditor(reconfigureTaskBuildSceneSettings).OnInspectorGUI();
        }
        /*
        if (GUILayout.Button("Refresh Compositions")) {
            GetAllSceneComposers(out sceneComposers);

            if (manager != null)
                manager.Refresh();
        }
        */
        /*
        EditorGUILayout.LabelField("Scene Compositions", EditorStyles.boldLabel);
        if (manager == null)
            GetSceneCompositionManager(out manager);

        if (GUILayout.Button("Refresh Compositions")) {
            GetAllSceneComposers(out sceneComposers);

            if (manager != null)
                manager.Refresh();
        }

        if (sceneComposers != null)
        {
            if (manager != null)
            {
                SceneCompositionManagerEditor[] editors = Resources.FindObjectsOfTypeAll<SceneCompositionManagerEditor>();
                if (editors.Length > 0)
                    editors[0].Draw(manager);
            }

            for (int sci = 0; sci < sceneComposers.Length; sci++) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(sceneComposers[sci].compositionName);

                if (GUILayout.Button("Load")) {
                    sceneComposers[sci].Load();
                }

                if (GUILayout.Button("Stage")) {
                    sceneComposers[sci].Stage();
                }

                if (GUILayout.Button("Select")) {
                    Selection.objects = new UnityEngine.Object[1] { sceneComposers[sci] };
                }

                EditorGUILayout.EndHorizontal();
            }
        }
        */
    }

    private void ShowReconfigureOptions ()
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space(5);

        EditorGUILayout.LabelField("Reconfigure Options", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Reconfigure")) {
            if (PlatformConfigScriptableObject.GetFirstInstanceOfPlatformConfig(_buildPipeline.HomeType, out var targetPlatformConfig)) {
                if (targetPlatformConfig == null)
                    Debug.LogErrorFormat("Unable to reconfigure for platform: \"{0}\", there is no {1} for that platform.", _buildPipeline.HomeType, nameof(PlatformConfigScriptableObject));
                else targetPlatformConfig.Reconfigure(_buildPipeline.HomeType, null);
            }
        }

        if (GUILayout.Button("Select Platform Config")) {
            if (PlatformConfigScriptableObject.GetFirstInstanceOfPlatformConfig(_buildPipeline.HomeType, out var platformConfig))
            {
                Selection.activeObject = platformConfig;
                PlatformConfigScriptableObject.ShowInspector();
            }
        }

        if (GUILayout.Button("Select Scene Build Settings")) {
            if (PlatformConfigScriptableObject.GetFirstInstanceOfPlatformConfig(_buildPipeline.HomeType, out var platformConfig)) {
                var reconfigureTaskBuildSceneSettings = platformConfig.reconfigureTasks.Where(reconfigureTask => reconfigureTask is PlatformReconfigureTaskBuildSceneSettings).FirstOrDefault();
                if (reconfigureTaskBuildSceneSettings != null)
                {
                    Selection.activeObject = reconfigureTaskBuildSceneSettings;
                    PlatformConfigScriptableObject.ShowInspector();
                }
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    private void ShowBuildOptions() {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space(5);

        Color backgroundColor = GUI.backgroundColor;

        EditorGUILayout.LabelField("Build", EditorStyles.boldLabel);

        string previousVersion = _buildPipeline.SharedVersion.Value;
        _buildPipeline.SharedVersion.Set(this, EditorGUILayout.TextField("Version", _buildPipeline.SharedVersion));
        if (_buildPipeline.SharedVersion.Value != previousVersion) EditorUtility.SetDirty(_buildPipeline.SharedVersion);
        if (_buildPipeline.PhotonServerSettings.AppSettings.AppVersion != _buildPipeline.SharedVersion.Value) {
            _buildPipeline.PhotonServerSettings.AppSettings.AppVersion = _buildPipeline.SharedVersion.Value;
            EditorUtility.SetDirty(_buildPipeline.PhotonServerSettings);
        }

        bool previousDevBuild = _buildPipeline.DevelopmentBuild;
        _buildPipeline.DevelopmentBuild = EditorGUILayout.Toggle("Development Build", _buildPipeline.DevelopmentBuild);
        if (_buildPipeline.DevelopmentBuild != previousDevBuild) EditorUtility.SetDirty(_buildPipeline);

        _buildPipeline.BasicMode = EditorGUILayout.Toggle("Basic Mode", _buildPipeline.BasicMode);
        _buildPipeline.Home = EditorGUILayout.Toggle("Home Mode", _buildPipeline.Home);
        if(_buildPipeline.Home)
        {
            _buildPipeline.HomeType = (HomeTypes) EditorGUILayout.EnumPopup(_buildPipeline.HomeType);
        }
        else
        {
            if (_buildPipeline.HomeType != HomeTypes.Undefined)
                _buildPipeline.HomeType = HomeTypes.Undefined;
        }

        bool previousHologateSetting = _buildPipeline.HologateSetting;
        _buildPipeline.HologateSetting = EditorGUILayout.Toggle("Hologate", _buildPipeline.HologateSetting);
        if (_buildPipeline.HologateSetting != previousHologateSetting) {
            PlayerSettings.SplashScreen.show = !_buildPipeline.HologateSetting;
            EditorUtility.SetDirty(TowerTagSettings.Singleton);
        }
        
        bool previousSteamEditorIdSetting = _buildPipeline.SteamEditorId;
        _buildPipeline.SteamEditorId = EditorGUILayout.Toggle("Steam Editor Id", _buildPipeline.SteamEditorId);
        if (_buildPipeline.SteamEditorId != previousSteamEditorIdSetting) {
            EditorUtility.SetDirty(TowerTagSettings.Singleton);
        }

        // add/remove dev tag to version number
        if (!_buildPipeline.DevelopmentBuild && _buildPipeline.SharedVersion.Value.Contains("_dev")) {
            _buildPipeline.SharedVersion.Set(this,
                _buildPipeline.SharedVersion.Value.Replace("_dev", ""));
            EditorUtility.SetDirty(_buildPipeline.SharedVersion);
        }

        if (_buildPipeline.DevelopmentBuild && !_buildPipeline.SharedVersion.Value.Contains("_dev")) {
            _buildPipeline.SharedVersion.Set(this, _buildPipeline.SharedVersion.Value + "_dev");
            EditorUtility.SetDirty(_buildPipeline.SharedVersion);
        }
        var logo = new PlayerSettings.SplashScreenLogo[1];
        // set splash Screen Logo
        if (_buildPipeline.BasicMode) {
            if (_buildPipeline.BasicSprite != null)
            {
                logo[0] = PlayerSettings.SplashScreenLogo.Create(2f, _buildPipeline.BasicSprite);
                SetSplashImage(logo);
            }
            else {
                Debug.LogError("Can't find Basic Splash-Screen Sprite!");
            }
        }
        else if (_buildPipeline.Home)
        {
            if (_buildPipeline.ProSprite != null) {
                logo[0] = PlayerSettings.SplashScreenLogo.Create(2f, _buildPipeline.HomeSprite);
                SetSplashImage(logo);
            }
            else {
                Debug.LogError("Can't find Home Splash-Screen Sprite!");
            }

        }else{
            if (_buildPipeline.ProSprite != null) {
                logo[0] = PlayerSettings.SplashScreenLogo.Create(2f, _buildPipeline.ProSprite);
                SetSplashImage(logo);
            }
            else {
                Debug.LogError("Can't find Pro Splash-Screen Sprite!");
            }
        }

        // build button
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("Build: " + _buildPipeline.SharedVersion.Value)) {
            if (Process.GetProcessesByName("TowerTag").Length > 0) {
                Debug.LogError("Can't start to build, another TowerTag instance is still running");
                return;
            }

            BuildOptions developmentBuildOptions = _buildPipeline.DevelopmentBuild
                ? BuildOptions.ShowBuiltPlayer | BuildOptions.Development
                : BuildOptions.ShowBuiltPlayer;
            string buildPath = "../Builds/Tower Tag " + _buildPipeline.SharedVersion.Value;
            if (_buildPipeline.BasicMode) buildPath += "_basic";
            if (TowerTagSettings.IsHomeTypeViveport) buildPath += "_VP";
            Build(developmentBuildOptions, _buildPipeline.SharedVersion.Value, buildPath,
                _buildPipeline.ExecutableName);
        }

        // finish
        GUI.backgroundColor = backgroundColor;
        if (GUILayout.Button("Open Builds Folder")) {
            string buildPath = Application.dataPath + @"\..\..\Builds";
            buildPath = buildPath.Replace(@"/", @"\");
            Application.OpenURL($"file://{buildPath}");
        }

        EditorGUILayout.Space();
    }

    private static void SetSplashImage(PlayerSettings.SplashScreenLogo[] logo)
    {
        if (PlayerSettings.SplashScreen.logos[0].logo != logo[0].logo ||
            PlayerSettings.virtualRealitySplashScreen != logo[0].logo.texture)
        {
            // set flat splash screen
            PlayerSettings.SplashScreen.logos = logo;
            // set vr splash screen
            PlayerSettings.virtualRealitySplashScreen = logo[0].logo.texture;
        }
    }

    private void Build(BuildOptions buildOptions, string applicationVersionName,
        string buildPath, string executableName) {
        Debug.Log("Build " + applicationVersionName);

        if (_buildPipeline == null) return;
        SharedControllerType.Singleton.Set(this, ControllerType.VR);

        var buildPlayerOptions = new BuildPlayerOptions {
            scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
            options = buildOptions,
            locationPathName = Path.Combine(buildPath, executableName + ".exe"),
            target = BuildTarget.StandaloneWindows64
        };

        BuildReport buildReport = BuildPipeline.BuildPlayer(buildPlayerOptions);

        // Build Report
        TimeSpan buildTime = buildReport.summary.buildEndedAt - buildReport.summary.buildStartedAt;
        Debug.Log("Build Result: " + buildReport.summary.result + " (Build time: " + buildTime + ")");

        if (buildReport.summary.result == BuildResult.Succeeded && _buildPipeline.DevelopmentBuild) {
            string[] batches = Directory.GetFiles(Application.dataPath + "/../../BatchScripts", "*.bat");
            foreach (string batch in batches) {
                string filename = Path.GetFileName(batch);
                if (!File.Exists(buildPath + "/" + filename))
                    File.Copy(batch, buildPath + "/" + filename);
            }
        }
    }
}