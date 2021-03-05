
using UnityEngine;
using UnityEngine.UI;

namespace UI{
    public class OperatorCanvasButtonManager : MonoBehaviour {
        [SerializeField] private Button _printButton;
        [SerializeField] private Button _backToHubButton;
        [SerializeField] private Button _offboardingButton;

        private void Start() {
            TTSceneManager.Instance.HubSceneLoaded += OnHubSceneLoaded;
            TTSceneManager.Instance.CommendationSceneLoaded += OnCommendationSceneLoaded;
            TTSceneManager.Instance.OffboardingSceneLoaded += OnOffboardingSceneLoaded;
            if (TTSceneManager.Instance.IsInHubScene)
                OnHubSceneLoaded();
        }
        private void OnDestroy() {
            if (TTSceneManager.Instance == null) return;
            TTSceneManager.Instance.HubSceneLoaded -= OnHubSceneLoaded;
            TTSceneManager.Instance.CommendationSceneLoaded -= OnCommendationSceneLoaded;
            TTSceneManager.Instance.OffboardingSceneLoaded -= OnOffboardingSceneLoaded;
        }

        private void OnOffboardingSceneLoaded() {
            SetButtonActive(false, true, false);

        }

        private void OnHubSceneLoaded() {
            SetButtonActive(false, false, true);

        }

        private void OnCommendationSceneLoaded() {
            SetButtonActive(true,true,true);
        }

        private void SetButtonActive(bool printButtonActive, bool backToHubButtonActive, bool offboardingButtonActive) {
            _printButton.gameObject.SetActive(printButtonActive);
            _backToHubButton.gameObject.SetActive(backToHubButtonActive);
            _offboardingButton.gameObject.SetActive(offboardingButtonActive);
        }
    }
}