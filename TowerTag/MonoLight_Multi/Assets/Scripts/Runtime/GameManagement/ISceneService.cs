namespace GameManagement {
    public delegate void SceneLoadedHandler();

    public interface ISceneService {
        bool IsInHubScene { get; }
        bool IsInCommendationsScene { get; }
        bool IsInConnectScene { get; }
        bool IsInLicensingScene { get; }
        bool IsInOffboardingScene { get; }
        bool IsInPillarOffsetScene { get; }
        string LicensingScene { get; }
        string PillarOffsetScene { get; }
        string CommendationsScene { get; }
        string OffboardingScene { get; }
        string CurrentHubScene { get; }
        string PreviousScene { get; }

        void LoadScene(string sceneName);
        void LoadConnectScene(bool showOffboardingInstructions);
        void LoadHubScene();
        void LoadLicensingScene();
        void LoadPillarOffsetScene();
        void LoadOffboardingScene();

        event SceneLoadedHandler ConnectSceneLoaded;
        event SceneLoadedHandler HubSceneLoaded;
        event SceneLoadedHandler CommendationSceneLoaded;
        event SceneLoadedHandler OffboardingSceneLoaded;
        event SceneLoadedHandler PillarOffsetSceneLoaded;
    }

    public class SceneService : ISceneService {
        public bool IsInHubScene => TTSceneManager.Instance.IsInHubScene;
        public bool IsInCommendationsScene => TTSceneManager.Instance.IsInCommendationsScene;
        public bool IsInConnectScene => TTSceneManager.Instance.IsInConnectScene;
        public bool IsInLicensingScene => TTSceneManager.Instance.IsInLicensingScene;
        public bool IsInOffboardingScene => TTSceneManager.Instance.IsInOffboardingScene;
        public bool IsInPillarOffsetScene => TTSceneManager.Instance.IsInPillarOffsetScene;
        public string LicensingScene => TTSceneManager.Instance.LicensingScene;
        public string PillarOffsetScene => TTSceneManager.Instance.PillarOffsetScene;
        public string CommendationsScene => TTSceneManager.Instance.CommendationsScene;
        public string OffboardingScene => TTSceneManager.Instance.OffboardingScene;
        public string CurrentHubScene => TTSceneManager.Instance.CurrentHubScene;
        public string PreviousScene => TTSceneManager.Instance.PreviousScene;

        public void LoadScene(string sceneName) {
            TTSceneManager.Instance.LoadScene(sceneName);
        }

        public void LoadConnectScene(bool showOffboardingInstructions) {
            TTSceneManager.Instance.LoadConnectScene(showOffboardingInstructions);
        }

        public void LoadHubScene() {
            TTSceneManager.Instance.LoadHubScene();
        }

        public void LoadLicensingScene() {
            TTSceneManager.Instance.LoadLicensingScene();
        }

        public void LoadPillarOffsetScene() {
            TTSceneManager.Instance.LoadPillarOffsetScene();
        }

        public void LoadOffboardingScene() {
            TTSceneManager.Instance.LoadOffboardingScene();
        }

        public event SceneLoadedHandler ConnectSceneLoaded {
            add {
                if (TTSceneManager.Instance != null) TTSceneManager.Instance.ConnectSceneLoaded += value;
            }
            remove {
                if (TTSceneManager.Instance != null) TTSceneManager.Instance.ConnectSceneLoaded -= value;
            }
        }

        public event SceneLoadedHandler HubSceneLoaded {
            add {
                if (TTSceneManager.Instance != null) TTSceneManager.Instance.HubSceneLoaded += value;
            }
            remove {
                if (TTSceneManager.Instance != null) TTSceneManager.Instance.HubSceneLoaded -= value;
            }
        }

        public event SceneLoadedHandler CommendationSceneLoaded {
            add {
                if (TTSceneManager.Instance != null) TTSceneManager.Instance.CommendationSceneLoaded += value;
            }
            remove {
                if (TTSceneManager.Instance != null) TTSceneManager.Instance.CommendationSceneLoaded -= value;
            }
        }

        public event SceneLoadedHandler OffboardingSceneLoaded {
            add {
                if (TTSceneManager.Instance != null) TTSceneManager.Instance.OffboardingSceneLoaded += value;
            }
            remove {
                if (TTSceneManager.Instance != null) TTSceneManager.Instance.OffboardingSceneLoaded -= value;
            }
        }

        public event SceneLoadedHandler PillarOffsetSceneLoaded {
            add {
                if (TTSceneManager.Instance != null) TTSceneManager.Instance.PillarOffsetSceneLoaded += value;
            }
            remove {
                if (TTSceneManager.Instance != null) TTSceneManager.Instance.PillarOffsetSceneLoaded -= value;
            }
        }
    }
}