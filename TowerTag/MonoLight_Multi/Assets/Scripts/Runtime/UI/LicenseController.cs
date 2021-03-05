using System;
using System.Collections;
using System.Linq;
using GameManagement;
using JetBrains.Annotations;
using TowerTagSOES;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class LicenseController : Logger {
        [SerializeField, Tooltip("The message queue for the message overlay")]
        private MessageQueue _overlayMessageQueue;

        [Tooltip(
            "The Text for the Activation Button if the License is activated and the button should deactivate the license")]
        [SerializeField]
        private string _deactivationText = "DEACTIVATE";

        [Space, Header("UI Objects")]
        [SerializeField]
        private Text _activateButtonText;

        [SerializeField] private Button _activateButton;
        [SerializeField] private InputField _productKeyInputField;
        [SerializeField] private InputField _eMailInputField;
        [SerializeField] private Button _startButton;
        [SerializeField] private Image _loadingSpinnerImage;

        private LicenseManager _licenseManager;
        private ISceneService _sceneService;

        private new void Awake() {
            base.Awake();
            _sceneService = ServiceProvider.Get<ISceneService>();
        }

        private void Start() {

            if (GameInitialization.GetInstance(out var gameInitialization))
                _licenseManager = gameInitialization.GetComponent<LicenseManager>();

            if (!_licenseManager) {
                Debug.LogError("Couldn't get " + _licenseManager.GetType().Name + " Script from GameInit Singleton");
            }
#if !UNITY_ANDROID
            // Init the startButton as hidden and show it if we are licensed
            _startButton.gameObject.SetActive(false);

            if (SharedControllerType.IsAdmin || SharedControllerType.Spectator) {
                _startButton.gameObject.SetActive(true);
            } else {
                try {
#if (UNITY_EDITOR && FAKE_LEXACTIVATORDLL_EXCEPTION)
                throw new DllNotFoundException("LexActivator.dll not found Editor test");
#endif
                    //if (_licenseManager.CheckLicenseForAllProductVersion()) {
                    //    _activateButtonText.text = _deactivationText;
                    //    _startButton.gameObject.SetActive(true);
                    //}
                } catch (DllNotFoundException e) {
                    LogError("Exception: " + e);
                    LogError(_licenseManager.dllExceptionLogMessage);
                    _overlayMessageQueue.AddErrorMessage(_licenseManager.dllExceptionOverlayMessage);
                }
            }
#else
            _activateButton.gameObject.SetActive(false);
#endif
        }

        /// <summary>
        /// Activate the product with the license key entered in the according text box
        /// </summary>
        [UsedImplicitly]
        public void OnActivateButtonPressed() {
#if !UNITY_ANDROID
            try {
                // Deactivate product if it is currently activated
                if (_activateButtonText.text == _deactivationText) {
                    if (_licenseManager.DeactivateProduct()) {
                        _startButton.gameObject.SetActive(false);
                        _activateButtonText.text = "Activate";
                        return;
                    }
                }

                // Check for every version if the license is active for it
                StartCoroutine(ActivateLicenseProcedure(_productKeyInputField.text, _eMailInputField.text,
                    OnActivationSuccessful));
            } catch (DllNotFoundException e) {
                LogError("Exception: " + e);
                LogError(_licenseManager.dllExceptionLogMessage);
                _overlayMessageQueue.AddErrorMessage(_licenseManager.dllExceptionOverlayMessage);
            }
#else
#endif
        }

#if !UNITY_ANDROID
        private IEnumerator ActivateLicenseProcedure(string productKey, string email,
            Action successCallback) {

            try {
                _activateButton.gameObject.SetActive(false);
                _loadingSpinnerImage.gameObject.SetActive(true);

#if (UNITY_EDITOR && FAKE_LEXACTIVATORDLL_EXCEPTION)
            throw new DllNotFoundException("LexActivator.dll not found Editor test");
#endif

                if (!_licenseManager._cryptlexProducts
                    .Where(product => product.CryptlexVersion == LicenseManager.CryptlexVersion.V3)
                    .Any(product => _licenseManager.ActivateLicense(product, productKey, email, successCallback))) {
                    _overlayMessageQueue.AddErrorMessage("Failed to activate license");
                }
            } catch (DllNotFoundException e) {
                LogError("Exception: " + e);
                LogError(_licenseManager.dllExceptionLogMessage);
                _overlayMessageQueue.AddErrorMessage(_licenseManager.dllExceptionOverlayMessage);
            } finally {
                _loadingSpinnerImage.gameObject.SetActive(false);
                _activateButton.gameObject.SetActive(true);
            }

            yield return null;
        }
#endif

        public void OnActivationSuccessful() {
            _activateButtonText.text = _deactivationText;
            _startButton.gameObject.SetActive(true);
            _sceneService.LoadConnectScene(!BalancingConfiguration.Singleton.AutoStart);
            // Send a Unity Analytics Custom Event if the product gets activated
        }

        [UsedImplicitly]
        public void OnStartButtonPressed() {
            _sceneService.LoadConnectScene(!BalancingConfiguration.Singleton.AutoStart);
        }

        [UsedImplicitly]
        public void ValidateProductKey(string productKey) {
            productKey = productKey.ToUpper();
            productKey = productKey.Replace('_', '-');
            productKey = productKey.Trim();
            _productKeyInputField.text = productKey;
        }

        /// <summary>
        /// Exit the application.
        /// </summary>
        [UsedImplicitly]
        public void OnExitButtonPressed() {
            _overlayMessageQueue.AddYesNoMessage(
                "QUIT APPLICATION?", "CONFIRM APPLICATION QUIT", null, null, "YES", () => {
                    Debug.LogWarning("User quit application from license-activation scene");
                    Application.Quit();
                });
        }
    }
}