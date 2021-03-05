using System;
using System.IO;
using System.Linq;
using CustomBuildPipeline;
using TowerTagSOES;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEngine;

[CustomEditor(typeof(TowerTagBuildPipeline))]
public class TowerTagBuildPipelineEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        serializedObject.Update();

        var towerTagBuildPipeline = serializedObject.targetObject as TowerTagBuildPipeline;
        if (towerTagBuildPipeline == null) {
            return;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

        SharedVersion sharedVersion = towerTagBuildPipeline.SharedVersion;
        if (sharedVersion == null) {
            return;
        }

        string previousVersion = sharedVersion.Value;
        sharedVersion.Set(this, EditorGUILayout.TextField("Version Number", sharedVersion));
        if (sharedVersion.Value != previousVersion) EditorUtility.SetDirty(sharedVersion);
        if (towerTagBuildPipeline.PhotonServerSettings.AppSettings.AppVersion != sharedVersion.Value) {
            towerTagBuildPipeline.PhotonServerSettings.AppSettings.AppVersion = sharedVersion.Value;
            EditorUtility.SetDirty(towerTagBuildPipeline.PhotonServerSettings);
        }

        TowerTagSettings.BasicMode = EditorGUILayout.Toggle("Basic Mode", TowerTagSettings.BasicMode);

        bool previousHologate = TowerTagSettings.Hologate;
        TowerTagSettings.Hologate = EditorGUILayout.Toggle("Hologate", TowerTagSettings.Hologate);
        if (TowerTagSettings.Hologate != previousHologate) EditorUtility.SetDirty(TowerTagSettings.Singleton);


        EditorGUILayout.PropertyField(serializedObject.FindProperty("_developmentBuild"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Build Application", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Have you updated the changelog? \n No?! \n U serious?!");
        EditorGUILayout.LabelField("No?!");
        EditorGUILayout.LabelField("U serious?!");
        string applicationVersionName = PlayerSettings.productName + " " + PlayerSettings.bundleVersion;
        if (GUILayout.Button("Build: " + applicationVersionName)) {
            BuildOptions developmentBuildOptions = towerTagBuildPipeline.DevelopmentBuild
                ? BuildOptions.ShowBuiltPlayer | BuildOptions.Development
                : BuildOptions.ShowBuiltPlayer;
            string buildPath = "../Builds/" + applicationVersionName;
            Build(developmentBuildOptions, applicationVersionName, buildPath, towerTagBuildPipeline.ExecutableName);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void Build(BuildOptions buildOptions, string applicationVersionName, string buildPath,
        string executableName) {
        Debug.Log("Build " + applicationVersionName);

        var towerTagBuildPipeline = serializedObject.targetObject as TowerTagBuildPipeline;
        if (towerTagBuildPipeline == null) return;
        SharedControllerType controllerType = towerTagBuildPipeline.ControllerType;
        controllerType.Set(this, ControllerType.VR);

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

        if (towerTagBuildPipeline.DevelopmentBuild) {
            string[] batches = Directory.GetFiles(Application.dataPath + "/../../BatchScripts", "*.bat");
            foreach (string batch in batches) {
                string filename = Path.GetFileName(batch);
                if (!File.Exists(buildPath + "/" + filename))
                    File.Copy(batch, buildPath + "/" + filename);
            }
        }
    }

    [PostProcessBuild(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
        string directoryPath = Path.GetDirectoryName(pathToBuiltProject);
        //if (TowerTagSettings.Hologate) {
        //    if (Directory.Exists(directoryPath + "/PhotonServer")) {
        //        Directory.Delete(directoryPath + "/PhotonServer",true);
        //        Directory.CreateDirectory(directoryPath + "/PhotonServer");
        //    }
        //    FileUtil.CopyFileOrDirectory(Application.dataPath + "/../../PhotonServer", directoryPath);
        //    Debug.Log($"Splash Screen = {PlayerSettings.SplashScreen.show}");
        //}
    }
}