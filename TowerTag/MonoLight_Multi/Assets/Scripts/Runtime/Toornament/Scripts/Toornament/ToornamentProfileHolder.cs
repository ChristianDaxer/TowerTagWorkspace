namespace Toornament {
    public class ToornamentProfileHolder {
        private static ToornamentProfileHolder _instance;

        public static ToornamentProfileHolder Instance => _instance ?? (_instance = new ToornamentProfileHolder());

        private ToornamentProfile _currentToornamentProfile;
        public string ApiKey => _currentToornamentProfile._apiKey;
        public string ClientId => _currentToornamentProfile._client_id;
        public string ClientSecret => _currentToornamentProfile._client_secret;
        public bool Initialized { get; private set; }

        public void LoadFromFile(string path, string fileName) {
            string dataPath = path + "/" + fileName;
            var newProfile = Serializer.DeserializeFromXML<ToornamentProfile>(dataPath);

            if (newProfile != null) {
                _currentToornamentProfile = newProfile;
                Initialized = true;
                Debug.Log("Loaded toornament profile");
            }
        }
    }
}