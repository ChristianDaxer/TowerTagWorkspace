using System.Collections;
using JetBrains.Annotations;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Home.UI {
    public class OfflineHubUIController : HubUIController {
        public delegate void OfflineHubButtonPressed(OfflineHubUIController sender);

        public static event OfflineHubButtonPressed QuickJoinButtonPressed;

        [Header("Buttons")] [SerializeField] private Button _quickJoinButton;
        [SerializeField] private Button _botMatchButton;
        [SerializeField] private Button _steamFriendsButton;
        [SerializeField] private Button _customMatchButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _crossButton;
        [SerializeField] private Button _quitButton;
        [SerializeField] private Button _detailsButton;

        [Header("PanelComponents")] [SerializeField]
        private Canvas _offlineUIOverlayCanvas;
		
        [Header("UI Controller References")] [SerializeField]
        private IngameUISettingsController _ingameUISettingsController;


        private BadaboomHyperactionPointerNeeded _badaboomHyperactionPointerNeeded;

        private void Awake() {
            _ingameUISettingsController.Init();
            if (!TowerTagSettings.IsHomeTypeSteam && !TowerTagSettings.IsHomeTypeOculus) {
                _steamFriendsButton.gameObject.SetActive(false);
            }

            StartCoroutine(TryToSpawnPointer());
        }

        private new void OnEnable() {
            base.OnEnable();
            _animator.SetTrigger(Spawn);
            SwitchPanel(PanelType.GTC, false);
            // check Pointer
            if (!BadaboomHyperactionPointer.GetInstance(out _pointer)) {
                _pointer = InstantiateWrapper.InstantiateWithMessage(_pointerPrefab);
            }
        }

        private void Start() {
            _badaboomHyperactionPointerNeeded = GetComponent<BadaboomHyperactionPointerNeeded>();
            OverlayCanvasModel = FindObjectOfType<OverlayCanvasModel>();
            if (OverlayCanvasModel != null) {
                CheckOverlayCanvas();
            }
        }

        private void Update() {
            if (OverlayCanvasModel != null) {
                CheckOverlayCanvas();
            }
        }

        private void OnDestroy() {
            if (OverlayCanvasModel != null) {
                OverlayCanvasModel.OnOpen -= CheckOverlayCanvas;
                OverlayCanvasModel.OnClose -= CheckOverlayCanvas;
            }
        }

        private IEnumerator TryToSpawnPointer() {
            if (!_animator.GetCurrentAnimatorStateInfo(0).IsName(SpawnedState)
                || _animator.GetCurrentAnimatorStateInfo(0).IsName(SpawnedState)
                && (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime) % 1 < 0.9f) {
                yield return new WaitUntil(() => _animator.GetCurrentAnimatorStateInfo(0).IsName(SpawnedState)
                                                 && (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime) % 1 >= 0.9f);
            }

            if (!BadaboomHyperactionPointer.GetInstance(out _pointer)) {
                TogglePointerNeededTag(this, true, false);
            }
        }

        public override void TogglePointerNeededTag(object sender, bool status, bool immediately) {
            if (_badaboomHyperactionPointerNeeded == null)
                return;
            _badaboomHyperactionPointerNeeded.enabled = _offlineUIOverlayCanvas.isActiveAndEnabled;
            if (_pointer == null && _badaboomHyperactionPointerNeeded)
                _pointer = InstantiateWrapper.InstantiateWithMessage(_pointerPrefab);
        }

        protected override void ActivateButtons() {
            _quickJoinButton.interactable = true;
            _botMatchButton.interactable = true;
            _customMatchButton.interactable = true;
            _steamFriendsButton.interactable = true;
            _settingsButton.interactable = true;
            _detailsButton.interactable = true;
            _crossButton.interactable = true;
            _quitButton.interactable = true;
        }

        protected override void DeactivateButtons() {
            _quickJoinButton.interactable = false;
            _botMatchButton.interactable = false;
            _customMatchButton.interactable = false;
            _steamFriendsButton.interactable = false;
            _settingsButton.interactable = false;
            _detailsButton.interactable = false;
            _crossButton.interactable = false;
            _quitButton.interactable = false;
        }

        [UsedImplicitly]
        public void OnQuickJoinButtonPressed() {
            QuickJoinButtonPressed?.Invoke(this);
            MessageQueue.Singleton.AddVolatileMessage(
                "Trying to connect to a server.",
                "Quick Join",
                null,
                null,
                null,
                5);
        }

        [UsedImplicitly, ContextMenu("Custom Match")]
        public void OnCustomMatchButtonPressed() {
            SwitchPanel(PanelType.FindMatch);
        }

        [UsedImplicitly]
        public void OnTrainingButtonPressed() {
            SwitchPanel(PanelType.Training);
        }

        [UsedImplicitly, ContextMenu("Return to Main Panel")]
        public void OnReturnToMainPanelButtonPressed() {
            SwitchPanel(PanelType.MainMenu);
        }

        [UsedImplicitly, ContextMenu("Settings")]
        public void OnSettingsButtonPressed() {
            SwitchPanel(PanelType.Settings);
        }

        [UsedImplicitly, ContextMenu("SteamFriends")]
        public void OnSteamFriendsButtonPressed() {
            SwitchPanel(PanelType.Friends);
        }

        [UsedImplicitly, ContextMenu("Statistics")]
        public void OnDetailsButtonPressed() {
            SwitchPanel(PanelType.Statistics);
        }

        [UsedImplicitly]
        public void OnCloseButtonPressed() {
            MessageQueue.Singleton.AddYesNoMessage(
                "Do you really want to quit...",
                "Quit Game",
                null,
                null,
                "YES",
                Application.Quit);
        }
    }
}