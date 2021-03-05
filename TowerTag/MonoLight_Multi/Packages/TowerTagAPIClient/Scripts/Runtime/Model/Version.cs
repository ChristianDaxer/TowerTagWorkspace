namespace TowerTagAPIClient.Model {
    public class Version {
        // public Version() { }
        public Version(string Version_type, string Version, bool China)
        {
            version_type = Version_type;
            version = Version;
            china = China;
        }
        public string version_type;
        public string version;
        public bool china;
    }
}