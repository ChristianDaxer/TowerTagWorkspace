using System.Collections;
using TowerTag;
using TowerTagSOES;
using UnityEngine;

    public class PlaySpaceManager : MonoBehaviour {

        private Chaperone _chaperone;
        private Configuration _configuration;
        private bool _initialized;
        private bool _initCoroutineIsRunning;


        private void OnEnable() {
            // Early Returns

            if (!TowerTagSettings.Home) {
                gameObject.SetActive(false);
                return;
            }

            if (!SharedControllerType.VR)
            {
                gameObject.SetActive(false);
                return;
            }
        }

        private void Start() {
            _configuration = ConfigurationManager.Configuration;
            if (SharedControllerType.VR) {
                StartCoroutine(InitPlaySpaceManager());
            }
            else if (SharedControllerType.NormalFPS) {
                DeactivateSmallPlayArea();
            }
        }

        private IEnumerator InitPlaySpaceManager() {
            yield return new WaitUntil(IsOpenVrChaperoneInitialized);
            float x = 0;
            float y = 0;
            if (TowerTagSettings.Chaperone.GetPlayAreaSize(ref x, ref y)) {
                bool roomScale = Mathf.Max(x, y) > 1.01f;
                // set Users Playspace configuration in TT to small play Area if open vr room scale = disabled
                SetUsersPlayAreaConfigSettings(roomScale, _configuration.SmallPlayArea);
            }
        }

        private bool IsOpenVrChaperoneInitialized() {
            return TowerTagSettings.PlatformSet && TowerTagSettings.Chaperone != null && TowerTagSettings.Chaperone.IsInitialized
                   && TowerTagSettings.Chaperone.GetCalibrationState() == ChaperoneCalibrationState.OK;
        }

        private void SetUsersPlayAreaConfigSettings(bool openVrRoomScaleActive, bool smallPlayAreaActive) {
            if (PlayerPrefs.HasKey(PlayerPrefKeys.SmallPlayArea)
                && PlayerPrefs.GetInt(PlayerPrefKeys.SmallPlayArea) == 1) {
                return;
            }

            if (smallPlayAreaActive || openVrRoomScaleActive) return;
            Debug.Log("PlaySpaceManager: Small Play Area automatically activated.");
            _configuration.SmallPlayArea = true;
            PlayerPrefs.SetInt(PlayerPrefKeys.SmallPlayArea, 1);
            ConfigurationManager.WriteConfigToFile();
        }

        private void DeactivateSmallPlayArea() {
            _configuration.SmallPlayArea = false;
            ConfigurationManager.WriteConfigToFile();
        }
    }