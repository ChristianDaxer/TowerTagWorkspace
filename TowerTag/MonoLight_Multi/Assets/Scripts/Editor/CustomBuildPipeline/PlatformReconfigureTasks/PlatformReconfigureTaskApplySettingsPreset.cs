using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEditor.Presets;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "ConfigureSettingsWithPreset", menuName = "ScriptableObjects/Platform Build Tasks/Configure Settings With Preset", order = 1)]
public class PlatformReconfigureTaskApplySettingsPreset : PlatformReconfigureTaskScriptableObject, IPlatformReconfigureTask, IPlatformReconfigureTaskDescriptor
{
    private static Object[] cachedSettings = null;
    [SerializeField] protected Preset preset;

    public string TaskDescription => $"Override project settings with preset: \"{preset.GetTargetTypeName()}\".";

    public IEnumerator Reconfigure(HomeTypes homeType, System.Action<IPlatformReconfigureTaskDescriptor> startTaskCallback, System.Action<bool> taskCallback)
    {
        if (startTaskCallback != null)
            startTaskCallback(this);

        PlatformConfigScriptableObject.RepaintInspectorGUI();

        yield return null;

        string targetTypeName = preset.GetTargetTypeName();

        string settingsPath = "ProjectSettings/";
        string settingsType = preset.GetTargetTypeName();
        if (cachedSettings == null)
        {
            cachedSettings = Directory.GetFiles(settingsPath)
                .Select(path => AssetDatabase.LoadAssetAtPath<Object>(path))
                .Where(cachedSetting => cachedSetting != null)
                .ToArray();

            string foundMsg = cachedSettings
                .Select(cachedSetting => cachedSetting.GetType().Name)
                .Aggregate((current, next) => current + "\t" + next + "\n");

            Debug.LogFormat("Cached reference to {0} settings at path: \"{1}\":\n{2}", cachedSettings.Length, settingsPath, foundMsg);
        }

        var settingsObject = cachedSettings.FirstOrDefault(setting => setting != null && setting.GetType().Name == settingsType);

        if (settingsObject == null)
        {
            Debug.LogErrorFormat("Unable to find settings of type: \"{0}\", verify that your preset applies to project settings located under {Project Path}/ProjectSettings/.", settingsType);
            taskCallback(false);
            yield break;
        }

        preset.ApplyTo(settingsObject);
        Debug.LogFormat("Applied preset to settings of type: \"{0}\".", settingsType);
        taskCallback(true);
    }
}
