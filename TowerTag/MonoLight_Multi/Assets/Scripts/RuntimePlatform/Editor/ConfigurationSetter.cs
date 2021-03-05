using UnityEditor;
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.Linq;

#if UNITY_MOBILE
public class ConfigurationSetter : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        ConfigurationManager.Path = Application.persistentDataPath + "/";
        ConfigurationManager.FileName = "Config.xml";

        string dataPath = ConfigurationManager.Path + ConfigurationManager.FileName;
        if (!File.Exists(dataPath)) Debug.LogWarning("Can't find file: " + dataPath);

        RuntimeConfigScriptableObject[] objects = AssetDatabase.FindAssets("t:RuntimeConfigScriptableObject").Select(guid => AssetDatabase.LoadAssetAtPath<RuntimeConfigScriptableObject>(AssetDatabase.GUIDToAssetPath(guid))).ToArray();
        if (objects.Length == 0)
        {
            Debug.LogErrorFormat($"There are no {typeof(RuntimeConfigScriptableObject).FullName} in the project.");
            return;
        }

        else if (objects.Length > 1)
        {
            Debug.LogErrorFormat($"There is more than one {typeof(RuntimeConfigScriptableObject).FullName} in the project.");
            return;
        }

        RuntimeConfigScriptableObject obj = objects[0];
        if (Serializer.DeSerializeFromXMLAsDataContract(dataPath, out Configuration newConfig))
        {
            Debug.LogFormat($"Applied config from path: \"{dataPath}\" to: {typeof(BaseConfigScriptableObject).FullName}");
            obj.ApplyConfig(newConfig);

            EditorUtility.SetDirty(obj);
            AssetDatabase.SaveAssets();
        }
    }
}
#endif
