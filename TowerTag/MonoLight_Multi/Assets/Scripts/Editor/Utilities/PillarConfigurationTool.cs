using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class PillarConfigurationTool : EditorWindow {
    /// <summary>
    /// Show PillarID settings group.
    /// </summary>
    private bool _showPillarIDOptions;

    /// <summary>
    /// Show Info Box if some errors occured when checking Pillars in scene.
    /// </summary>
    private bool _showErrorMessage;

    /// <summary>
    /// Error Message which errors occured when checking Pillars in scene.
    /// </summary>
    private string _errorMessage;


    /// <summary>
    /// Show wall position preset settings group.
    /// </summary>
    private bool _showWallPosOptions;

    /// <summary>
    /// Force matching of Pillars by position instead of PillarID when applying PillarSceneConfiguration to Pillars of a scene.
    /// </summary>
    private bool _forceMatchPillarsByPositionWhenApplyingWallPositions;

    private readonly GUIContent _forceMatchPillarsByPositionUIContent = new GUIContent(
        "Force match Pillars by Position when Loading Wall positions: ",
        "If you want to match the corresponding Pillars (pillars in scene <-> saved pillars) " +
        "by their world position, set this true. If set to false we try to match first by pillarID " +
        "and if this is not unique (PillarIDs not unique in pillars in scene or saved Pillars) " +
        "we try to match by pillar position.");

    /// <summary>
    /// Name of the WallsParent GameObject as path from the pillar parent down to the WallsParent
    /// (see default path for example (PillarAssets/Walls means the PillarAssets is child
    /// of the given Pillar and has the wallsParent as child with the name Walls).
    /// </summary>
    private string _nameOfWallParentAsPath = "PillarAssets/Walls";

    private readonly GUIContent _nameOfWallParentUIContent = new GUIContent(
        "Name of the walls parent GameObject as path from pillarParent: ",
        "Name of the WallsParent GameObject as path from the Pillar parent down to the WallsParent " +
        "(see default path for example (PillarAssets/Walls means the PillarAssets is child of the given Pillar " +
        "and has the wallsParent as child with the name Walls).");


    /// <summary>
    /// Show or hide List of Pillars in Scene
    /// </summary>
    private bool _showPillars;

    private Vector2 _pillarListScrollPos;
    private Vector2 _uiScrollPos;

    [MenuItem("PillarGame/PillarConfigurationTool")]
    public static void ShowWindow() {
        GetWindow(typeof(PillarConfigurationTool));
    }

    /// <summary>
    /// Draw UI
    /// </summary>
    private void OnGUI() {
        // draw UI for Pillar checks & set PillarIDs
        _uiScrollPos = GUILayout.BeginScrollView(_uiScrollPos);
        _showPillarIDOptions = EditorGUILayout.BeginToggleGroup("Check Pillars & set Pillar IDs", _showPillarIDOptions);
        if (_showPillarIDOptions) {
            if (_showErrorMessage)
                EditorGUILayout.HelpBox(_errorMessage, MessageType.Error);

            if (GUILayout.Button("Check Pillars")) {
                if (CheckForOverlappingPillars() && CheckIfPillarIDsAreUnique() && CheckPillarMember())
                    Debug.Log("Check open Scene: no doubled Pillars/PillarIDs found!");
            }

            if (GUILayout.Button("Check Pillars for all scenes in buildSettings")) {
                CheckPillarsForAllEnabledBuildSettingsScenes();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Set Pillar IDs")) {
                SetPillarIDs();
            }

            if (GUILayout.Button("Set Pillar IDs for all scenes in BuildSettings")) {
                ApplyPillarIDsForAllEnabledBuildSettingsScenes();
            }

            GUILayout.Space(20);
        }

        EditorGUILayout.EndToggleGroup();

//  draw UI for WallPosition Presets and copy wallPositions from one scene to another 
        _showWallPosOptions = EditorGUILayout.BeginToggleGroup("Wall Positions:", _showWallPosOptions);
        if (_showWallPosOptions) {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(_nameOfWallParentUIContent);
                _nameOfWallParentAsPath = GUILayout.TextField(_nameOfWallParentAsPath);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(_forceMatchPillarsByPositionUIContent);
                _forceMatchPillarsByPositionWhenApplyingWallPositions =
                    GUILayout.Toggle(_forceMatchPillarsByPositionWhenApplyingWallPositions, "");
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(20);
        }

        EditorGUILayout.EndToggleGroup();

// draw a List of all Pillars in scene
        _showPillars = EditorGUILayout.BeginToggleGroup("Show Pillar Values: ", _showPillars);
        if (_showPillars) {
            if (GUILayout.Button("Select all Pillars")) {
                SelectAll();
            }

            ShowPillars();
        }

        EditorGUILayout.EndToggleGroup();
        GUILayout.EndScrollView();
    }

    /// <summary>
    /// Selects all PillarScripts in the current scene, so you can Multi-Object-edit them.
    /// </summary>
    private static void SelectAll() {
        Pillar[] pillars = FindObjectsOfType<Pillar>();
        Selection.objects = pillars;
    }

    /// <summary>
    /// Show a List of all Pillars in the current scene.
    /// </summary>
    private void ShowPillars() {
        Pillar[] pillars = FindObjectsOfType<Pillar>();
        pillars = pillars.OrderBy(x => x.ID).ToArray();

        _pillarListScrollPos = GUILayout.BeginScrollView(_pillarListScrollPos);
        foreach (Pillar pillar in pillars) {
            if (pillar == null) continue;

            Pillar p = pillar;

            GUILayout.BeginHorizontal();
            GUILayout.Label(p.name + "-> Team: " + p.OwningTeam + "/ ID: " + p.ID);

            GUILayout.FlexibleSpace();
            GUILayout.Label("c: " + PrintBool(p.IsClaimable) + " | s: " + PrintBool(p.IsSpawnPillar) + " | g: " +
                            PrintBool(p.IsGoalPillar) + " | spec: " + PrintBool(p.IsSpectatorPillar) + " | aAC: " +
                            " | aT: " + PrintBool(p.AllowTeleportWithoutTeamMatch));
            if (GUILayout.Button("o")) {
                Selection.activeGameObject = p.gameObject;
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();
    }

    /// <summary>
    /// Formats a bool value in a compact string.
    /// </summary>
    /// <param name="input">Bool value to show as compact formatted string.</param>
    /// <returns>Compact formatted string of bool value ("x" if true, "-" if false)</returns>
    static string PrintBool(bool input) {
        return input ? "x" : "-";
    }

    /// <summary>
    /// Convenience Func to Call SetPillarIDs(int buildIndex) for current active scene.
    /// </summary>
    private void SetPillarIDs() {
        SetPillarIDs(SceneManager.GetActiveScene().buildIndex);
    }


    /// <summary>
    /// Calculate & sets unique IDs for all Pillars in the current active scene (with buildIndex).
    /// </summary>
    /// <param name="buildIndex">Build index of the current active scene. Used to calculate the PillarIDs.</param>
    private void SetPillarIDs(int buildIndex) {
        int currentScene = buildIndex;
        if (currentScene == -1) {
            _showErrorMessage = true;
            _errorMessage =
                "You have to add the scene in the build settings before you create PillarIDs! The created IDs are not unique";
        }
        else {
            _showErrorMessage = false;
            _errorMessage = "";
        }

        Pillar[] pillars = FindObjectsOfType<Pillar>();

        var offsetIndex = 9000000;
        // prevent int overflow
        if (currentScene > -1 && currentScene < 2147) {
            offsetIndex = (currentScene + 10) * 1000000;
        }
        else {
            _showErrorMessage = true;
            _errorMessage = "The scene index is not valid for PillarID creation! The created IDs are not unique!!!";
        }

        // prevent int overflow
        if (!(pillars.Length < 483646)) {
            _showErrorMessage = true;
            _errorMessage = "To many Pillars in this scene to create unique IDs!";
        }

        Assert.IsTrue(currentScene > -1, "Scene is not in Build settings (scene index < 0)");
        Assert.IsTrue(currentScene < 2147, "SceneIndex to big for ID creation");
        Assert.IsTrue(pillars.Length < 483646, "To many Pillars in this scene to create unique IDs!");

        for (var i = 0; i < Mathf.Min(pillars.Length, 483645); i++) {
            pillars[i].ID = offsetIndex + (i + 1);
            EditorUtility.SetDirty(pillars[i]);
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    /// <summary>
    /// Calls SetPillarIDs() for every enabled scene in BuildSettings
    /// </summary>
    private void ApplyPillarIDsForAllEnabledBuildSettingsScenes() {
        // Func: 
        //  - open Scene
        //  - set pillar IDs
        //  - save scene
        void Func(EditorBuildSettingsScene sceneFromBuildSettings) {
            Scene scene = EditorSceneManager.OpenScene(sceneFromBuildSettings.path);
            SetPillarIDs(scene.buildIndex);
            EditorSceneManager.SaveScene(scene);
        }

        ExecuteForAllActiveScenes(Func);
    }

    /// <summary>
    /// Calls CheckForOverlappingPillars(), CheckIfPillarIDsAreUnique(), CheckPillarMember() for every enabled scene in BuildSettings
    /// </summary>
    private void CheckPillarsForAllEnabledBuildSettingsScenes() {
        // Func: 
        //  - open Scene
        //  - check IDs & distances between Pillars
        //  - save scene
        void Func(EditorBuildSettingsScene sceneFromBuildSettings) {
            EditorSceneManager.OpenScene(sceneFromBuildSettings.path);
            Debug.Log("Check Scene: " + sceneFromBuildSettings.path);
            if (CheckForOverlappingPillars() && CheckIfPillarIDsAreUnique() && CheckPillarMember())
                Debug.Log("   -> seems OK!");
        }

        ExecuteForAllActiveScenes(Func);
    }

    /// <summary>
    /// Check if distance between Pillars is bigger than minDistance.
    /// </summary>
    /// <param name="minDistance">Minimal distance between Pillars.</param>
    /// <returns>true if  distance between Pillars is bigger than minDistance for all Pillars in current scene, false otherwise</returns>
    private bool CheckForOverlappingPillars(float minDistance = 1) {
        // check minDistance between two Pillars
        bool Func(Pillar a, Pillar b) {
            if (Vector3.Distance(a.transform.position, b.transform.position) <= minDistance) {
                Debug.LogError("   -> Found overlapping Pillars (distance < " + minDistance + "): P1(" + a.ID +
                               ") <-> P2(" + b.ID + ")");
                return false;
            }

            return true;
        }

        return ExecuteForEveryPairOfPillarsInScene("CheckForOverlappingPillars", Func);
    }

    /// <summary>
    /// Check if PillarIDs are unique for all Pillars in the current scene.
    /// </summary>
    /// <returns>true if unique in this scene, else otherwise</returns>
    private bool CheckIfPillarIDsAreUnique() {
        // check if PillarIDs of two given pillars are unequal
        bool Func(Pillar a, Pillar b) {
            if (a.ID == b.ID) {
                Debug.LogError("   -> Found Pillars with same ID (" + a.ID + ")");
                return false;
            }

            return true;
        }

        return ExecuteForEveryPairOfPillarsInScene("CheckIfPillarIDsAreUnique", Func);
    }

    /// <summary>
    /// Check if "static" member (means member not set at runtime: teleportTransform, anchorTransform) of the Pillars set (!= null).
    /// </summary>
    /// <returns>true if all check member set (!= null), false otherwise</returns>
    private bool CheckPillarMember() {
        // check for reference type member which should be set at compile time in scene (teleportTransform/anchorTransform)
        bool Func(Pillar pillar) {
            if (pillar == null) {
                Debug.LogError("Pillar is null");
                return false;
            }

            if (pillar.TeleportTransform == null) {
                Debug.LogError("Teleport transform is null");
                return false;
            }

            if (pillar.AnchorTransform == null) {
                Debug.LogError("Anchor transform is null");
                return false;
            }

            return true;
        }

        return ExecuteForEveryPillarInScene("CheckPillarMember", Func);
    }

    #region Helper

    /// <summary>
    /// Iterate over every pair of Pillars (no doubleChecks (p1, p2) - (p2, p1) and no check for identity (p1,p1)) and execute given Function.
    /// </summary>
    /// <param name="funcName">Nice formatted Name of the Function to execute to show in console messages.</param>
    /// <param name="func">Function to execute.</param>
    /// <returns>true if the given function had returned true for every Pillar, else otherwise</returns>
    private static bool ExecuteForEveryPairOfPillarsInScene(string funcName, Func<Pillar, Pillar, bool> func) {
        // get all pillars
        Pillar[] pillars = FindObjectsOfType<Pillar>();
        if (pillars == null) {
            Debug.LogError($"Cannot execute {funcName} for every pillar: No pillars found");
            return false;
        }

        // iterate over every pair (no doubleChecks (p1, p2) <-> (p2, p1) and no check for identity (p1,p1))
        var foundProblem = false;
        for (var i = 0; i < pillars.Length; i++) {
            for (int k = (i + 1); k < pillars.Length; k++) {
                foundProblem = !func(pillars[i], pillars[k]) || foundProblem;
            }
        }

        return !foundProblem;
    }

    /// <summary>
    /// Execute a given function for every Pillar in current scene.
    /// </summary>
    /// <param name="funcName">Nice formatted Name of the Function to execute to show in console messages.</param>
    /// <param name="func">Function to execute.</param>
    /// <returns>true if the given function had returned true for every Pillar, else otherwise</returns>
    private static bool ExecuteForEveryPillarInScene(string funcName, Func<Pillar, bool> func) {
        Scene scene = SceneManager.GetActiveScene();
        string sceneName = scene.name;

        // get all pillars
        Pillar[] pillars = FindObjectsOfType<Pillar>();
        if (pillars == null) {
            Debug.LogError($"Cannot execute {funcName} for every pillar: no pillars found");
            return false;
        }

        // iterate over every pair (no doubleChecks (p1, p2) <-> (p2, p1) and no check for identity (p1,p1))
        var foundProblem = false;
        for (var i = 0; i < pillars.Length; i++) {
            if (pillars[i] == null) {
                Debug.LogError(funcName + ": Pillar(" + i + ") is null in active scene (" + sceneName + ")!");
                return false;
            }

            foundProblem = !func(pillars[i]) || foundProblem;
        }

        return !foundProblem;
    }

    /// <summary>
    /// Iterate over every enabled scene in buildSettings and execute given function.
    /// </summary>
    /// <param name="func">Function to execute for every scene.</param>
    private static void ExecuteForAllActiveScenes(Action<EditorBuildSettingsScene> func) {
        // get currently opened Scene
        Scene currentScene = SceneManager.GetActiveScene();
        // cache path to current scene to open it afterwards
        string currentScenePath = currentScene.path;
        // save this scene in case you've got unsaved changes
        EditorSceneManager.SaveScene(currentScene);
        // get all scenes from buildSettings
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        foreach (EditorBuildSettingsScene scene in scenes) {
// only execute for activated scenes (in BuildSettings)
            if (scene.enabled) {
                func(scene);
            }
        }

        // Open old scene (in which we began)
        EditorSceneManager.OpenScene(currentScenePath);
    }

    #endregion

}