using System.Collections;
using GameManagement;
using Photon.Pun;
using TowerTag;
using TowerTagSOES;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI {
    public abstract class StartUp : MonoBehaviour {
        private ISceneService _sceneService;
        [FormerlySerializedAs("_steamAchievementsPrefab")] [SerializeField]
        protected StatCollector _achievementsPrefab;

        [SerializeField] protected Texture[] _gameTips;

        public abstract void OnInit();
        public abstract void OnInitHome();
        public abstract void OnLoadedSceneHome();
        public abstract IEnumerator OnPlatformLoadLevel(string sceneName);
        public abstract IEnumerator PlatformLoadNextScene();
        
        public static bool Finished { get; private set; } 

        private void Start() {
            StartCoroutine(Init());
            StartCoroutine(LoadNextScene());
        }

        private IEnumerator Init() {
            yield return null; // Wait a frame if we are scene compositing.

            if (!GameInitialization.Initialized) {
                yield return new WaitUntil(() => GameInitialization.Initialized);
            }

            OnInit();

            if (TowerTagSettings.Home) {
                OnInitHome();
            }

            Finished = true;
        }

        private IEnumerator LoadNextScene() {

            if (_sceneService == null)
                _sceneService = ServiceProvider.Get<ISceneService>();

            StartCoroutine(LoadLevel(TTSceneManager.Instance.ConnectScene));
            yield return null;
            /*
            if (TowerTagSettings.BasicMode) {
                Debug.Log("Loading Basic Mode, skip license check!");
                if (!_sceneService.IsInConnectScene) {
                    _sceneService.LoadConnectScene(!BalancingConfiguration.Singleton.AutoStart && !TowerTagSettings.Home);
                    yield break;
                }
            }

            if (TowerTagSettings.Home) {
                if (!PlayerPrefs.HasKey(PlayerPrefKeys.Tutorial) || PlayerPrefs.GetInt(PlayerPrefKeys.Tutorial) == 0)
                    StartCoroutine(StartTutorial());
                else {
                    StartCoroutine(LoadLevel(MySceneManager.Instance.ConnectScene));
                }
            }
            */

            /*
            if (SharedControllerType.IsAdmin) {
                Debug.Log("Loading Admin, skip license check!");
                if (!_sceneService.IsInConnectScene) {
                    _sceneService.LoadConnectScene(false);
                }
            }
            else if (SharedControllerType.Spectator) {
                Debug.Log("Loading Spectator, skip license check!");
                if (!_sceneService.IsInConnectScene) {
                    StartCoroutine(LoadLevel(MySceneManager.Instance.ConnectScene));
                }
            }
            else if (SharedControllerType.PillarOffsetController) {
                Debug.Log("Loading PillarOffsetScene, skip license check!");
                if (!_sceneService.IsInPillarOffsetScene) {
                    _sceneService.LoadPillarOffsetScene();
                }
            }
            */
        }

        protected IEnumerator LoadLevel(string sceneName) {
            yield return new WaitUntil(() => GameInitialization.Initialized);
            yield return OnPlatformLoadLevel(sceneName);
            yield return new WaitForEndOfFrame();
        }

        protected IEnumerator StartTutorial() {
            ConnectionManager.Instance.Connect();
            while (!PhotonNetwork.InLobby)
                yield return null;
            GameManager.Instance.StartTutorial(false);
        }
    }
}