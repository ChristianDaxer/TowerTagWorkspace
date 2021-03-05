using System.Collections;
using SOEventSystem.Shared;
using UnityEngine;

namespace UI {
    /// <summary>
    /// Component to hide and show operator UI canvases.
    ///
    /// <author>Ole Jürgensen (ole@vrnerds.de)</author>
    /// </summary>
    public class ToggleOperatorUI : MonoBehaviour {
        public delegate void UIToggle(bool value);

        public event UIToggle SpectatorUIToggled;

        [SerializeField, Tooltip("the operator UI canvas.")]
        private Canvas _operatorUICanvas;

        [SerializeField, Tooltip("the spectator UI canvas.")]
        private Canvas _spectatorUiCanvas;

        [SerializeField, Tooltip("Hot-keys input system asset")]
        private HotKeys _hotKeys;

        [SerializeField, Tooltip("True when the in-game bug reporting tool is currently being used.")]
        private SharedBool _reportingBug;

        [SerializeField, Tooltip("If true, all UI elements will not be shown on the bug report screenshot")]
        private bool _hideUIDuringBugReport;

        [SerializeField] private float _transitionToggle = 0.2f;

        [Header("SpectatorUI")] [SerializeField]
        private GameObject _spectatorParent;

        [SerializeField] private float _closedValueSpectatorY;
        [SerializeField] private bool _spectatorActive;
        private Coroutine _spectatorCoroutine;


        [Header("OperatorUI")] [SerializeField]
        private GameObject _operatorParent;

        [SerializeField] private float _closedValueOperatorY;
        [SerializeField] private bool _operatorActive;
        [SerializeField] private TeamBoxController _teamBoxControllerFire;
        [SerializeField] private TeamBoxController _teamBoxControllerIce;
        private Coroutine _operatorCoroutine;
        private bool _toggleable = true;

        private bool OperatorActive {
            get => _operatorActive;
            set {
                if (!_toggleable)
                    return;
                SlideOperatorUI(value);
                _operatorActive = value;
            }
        }

        private bool SpectatorActive {
            get => _spectatorActive;
            set {
                if (!_toggleable)
                    return;
                SpectatorUIToggled?.Invoke(value);
                SlideSpectatorUI(value);
                _spectatorActive = value;
            }
        }

        private void Start() {
            if(TowerTagSettings.Hologate)
                _operatorParent.SetActive(false);
            GameManager.Instance.MissionBriefingStarted += OnMissionBriefingStarted;
            GameManager.Instance.MatchSceneLoading += OnMatchSceneLoading;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneLoaded;
            _hotKeys.OperatorUIToggled += OnOperatorUIToggled;
            _hotKeys.SpectatorUIToggled += OnSpectatorUIToggled;
            _reportingBug.ValueChanged += ReportingBugChanged;
        }

        private void OnDestroy() {
            GameManager.Instance.MissionBriefingStarted -= OnMissionBriefingStarted;
            GameManager.Instance.MatchSceneLoading -= OnMatchSceneLoading;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= SceneLoaded;
            _hotKeys.OperatorUIToggled -= OnOperatorUIToggled;
            _hotKeys.SpectatorUIToggled -= OnSpectatorUIToggled;
            _reportingBug.ValueChanged -= ReportingBugChanged;
        }

        public void OnOperatorUIToggled() {
            OperatorActive = !OperatorActive;
            //_operatorUICanvas.gameObject.SetActive(!_operatorUICanvas.gameObject.activeSelf);
        }

        private void OnSpectatorUIToggled() {
            SpectatorActive = !SpectatorActive;
            //_spectatorUiCanvas.gameObject.SetActive(!_spectatorUiCanvas.gameObject.activeSelf);
        }

        private void OnMissionBriefingStarted(MatchDescription obj, GameMode gameMode) {
            SpectatorActive = false;
            OperatorActive = false;
            _toggleable = false;
        }

        private void OnMatchSceneLoading() {
            _toggleable = true;
        }


        private void ReportingBugChanged(object sender, bool value) {
            if (_reportingBug && _hideUIDuringBugReport) {
                if (_operatorUICanvas.gameObject.activeSelf) _reportingBug.ValueChanged += ReopenAdminUIAfterBugReport;
                _operatorUICanvas.gameObject.SetActive(false);
                if (_spectatorUiCanvas.gameObject.activeSelf)
                    _reportingBug.ValueChanged += ReopenSpectatorUIAfterBugReport;
                _spectatorUiCanvas.gameObject.SetActive(false);
            }
        }

        private void ReopenAdminUIAfterBugReport(object sender, bool value) {
            _operatorUICanvas.gameObject.SetActive(true);
            _reportingBug.ValueChanged -= ReopenAdminUIAfterBugReport;
        }

        private void ReopenSpectatorUIAfterBugReport(object sender, bool value) {
            _spectatorUiCanvas.gameObject.SetActive(true);
            _reportingBug.ValueChanged -= ReopenSpectatorUIAfterBugReport;
        }

        /// <summary>
        /// Gets Called whenever a new Scene is Loaded
        /// </summary>
        /// <param name="scene">The new scene that is loaded</param>
        /// <param name="mode">The mode is which the scene is loaded</param>
        private void SceneLoaded(UnityEngine.SceneManagement.Scene scene,
            UnityEngine.SceneManagement.LoadSceneMode mode) {
            if (TTSceneManager.Instance.IsInCommendationsScene || TTSceneManager.Instance.IsInOffboardingScene) {
                OperatorActive = false;
                SpectatorActive = false;
                return;
            }

            if (TTSceneManager.Instance.IsInHubScene) {
                OperatorActive = true;
                SpectatorActive = false;
                return;
            }

            OperatorActive = false;
            SpectatorActive = true;
        }


        private void SlideOperatorUI(bool activate) {
            if (_operatorCoroutine != null)
                StopCoroutine(_operatorCoroutine);

            if (_operatorParent == null) return;

            _operatorCoroutine = StartCoroutine(SlideObject(_operatorParent, activate, true, _closedValueOperatorY));
            if (activate) {
                _teamBoxControllerFire.ShowFilledSlots();
                _teamBoxControllerIce.ShowFilledSlots();
            }
            else {
                _teamBoxControllerFire.HideSlots();
                _teamBoxControllerIce.HideSlots();
            }
        }

        private void SlideSpectatorUI(bool activate) {
            if (_spectatorCoroutine != null) {
                StopCoroutine(_spectatorCoroutine);
            }

            _spectatorCoroutine = StartCoroutine(SlideObject(_spectatorParent, activate, true, _closedValueSpectatorY));
        }

        private IEnumerator SlideObject(GameObject objectToSlide, bool setActive, bool vertical, float min) {
            var gObjectRectTransform = objectToSlide.GetComponent<RectTransform>();
            Vector3 closedPos;
            Vector3 openPos;

            Vector2 anchoredPosition = gObjectRectTransform.anchoredPosition;
            if (vertical) {
                closedPos = new Vector3(anchoredPosition.x, min, 0);
                openPos = new Vector3(anchoredPosition.x, 0, 0);
            }
            else {
                closedPos = new Vector3(min, gObjectRectTransform.anchoredPosition.y, 0);
                openPos = new Vector3(0, anchoredPosition.y, 0);
            }

            if (setActive) {
                while (Vector3.Distance(gObjectRectTransform.anchoredPosition, openPos) >= 0.1f) {
                    gObjectRectTransform.anchoredPosition = Vector3.Lerp(gObjectRectTransform.anchoredPosition, openPos,
                        _transitionToggle);
                    yield return null;
                }

                gObjectRectTransform.anchoredPosition = openPos;
            }
            else {
                while (Vector3.Distance(gObjectRectTransform.anchoredPosition, closedPos) >= 0.1f) {
                    gObjectRectTransform.anchoredPosition = Vector3.Lerp(gObjectRectTransform.anchoredPosition,
                        closedPos, _transitionToggle);
                    yield return null;
                }

                gObjectRectTransform.anchoredPosition = closedPos;
            }

            yield return null;
        }
    }
}