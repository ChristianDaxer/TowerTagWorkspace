using System.IO;
using System.Linq;

using JetBrains.Annotations;

public static class ConfigurationManager {
    private static Configuration _config;

    public delegate void ConfigurationManagerAction();

    public static event ConfigurationManagerAction ConfigurationUpdated;

    [NotNull] public static Configuration Configuration {
        get
        {
#if UNITY_ANDROID
            if (ConfigurationHolder.GetInstance(out var configurationHolder))
                return configurationHolder.configScriptableObject.Config;
            return null;
#else
            return _config ?? (_config = new Configuration());
#endif
        }
    }

    public static string FileName = "Config.xml";
    public static string Path;

    public static bool LoadConfigFromFile() {
#if UNITY_ANDROID
        return true;
#else
        return LoadFromFile(Path, FileName);
#endif
    }

    private static bool LoadFromFile(string path, string fileName) {
        string dataPath = path + fileName;
        if (!File.Exists(dataPath)) Debug.LogWarning("Can't find file: " + dataPath);

        if (Serializer.DeSerializeFromXMLAsDataContract(dataPath, out Configuration newConfig)) {
            _config = newConfig;
            return true;
        }

        Debug.LogWarning("Could not load Config from " + dataPath);
        return false;
    }

    public static bool WriteConfigToFile() {
#if UNITY_ANDROID
        return true;
#else
        return WriteToFile(Path, FileName);
#endif
    }

    private static bool WriteToFile(string path, string fileName) {
        string dataPath = path + fileName;
        if(!File.Exists(dataPath)) Debug.LogWarning("Creating new config file: " + dataPath);
        ConfigurationUpdated?.Invoke();
        return Serializer.SerializeToXMLAsDataContract(dataPath, Configuration);
    }
}