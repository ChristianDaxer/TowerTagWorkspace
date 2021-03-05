using System;

public static class PlayerProfileManager {
    // currently handled Profile
    private static PlayerProfile _currentPlayerProfile;

    public static PlayerProfile CurrentPlayerProfile {
        get {
            if (_currentPlayerProfile == null) {
                CreateNew();
            }

            return _currentPlayerProfile;
        }
    }

    // Save to/Load from disk
    public static string FileName;
    public static string Path;

    public static void CreateNew() {
        _currentPlayerProfile = new PlayerProfile {PlayerGUID = Guid.NewGuid().ToString()};
    }

    public static bool LoadFromFile() {
        return LoadFromFile(Path, FileName);
    }

    public static bool WriteToFile() {
        return WriteToFile(Path, FileName);
    }


    private static bool LoadFromFile(string path, string fileName) {
        string dataPath = path + fileName;
        var newProfile = Serializer.DeserializeFromXML<PlayerProfile>(dataPath);

        if (newProfile != null) {
            _currentPlayerProfile = newProfile;
            return true;
        }

        return false;
    }

    private static bool WriteToFile(string path, string fileName) {
        string dataPath = path + fileName;
        return Serializer.SerializeToXML(dataPath, _currentPlayerProfile);
    }
}