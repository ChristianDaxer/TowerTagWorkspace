using JetBrains.Annotations;
using TowerTagSOES;
using UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
//using Valve.VR;
using IPlayer = TowerTag.IPlayer;

namespace Home.UI {
    public class IngameHubUIController : HubUIController {
       // [SerializeField] private SteamVR_Action_Boolean _menuButtonAction;
        [SerializeField] private GameObject _ingameUICanvas;
        [SerializeField] private Button _quitButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _leaveMatchButton;
		[FormerlySerializedAs("_abortMatchToggle")] [SerializeField] private Toggle _voteAbortMatchToggle;
        private BadaboomHyperactionPointerNeeded _badaboomHyperactionPointerNeeded;
        [SerializeField] private Vector3 _offset;
        private Camera _vrCamera;

        [SerializeField] private IngameUISettingsController _ingameUISettingsController;
        private Transform _transform;
        private IPlayer _localPlayer;
        private Pillar _currentPillar;
        private bool _menuAnimationIsRunning;
        private bool _menuState = false;

        private static bool IsMenuToggleAllowed => !TTSceneManager.Instance.IsInConnectScene
                                                   && GameManager.Instance.CurrentState != GameManager
                                                       .GameManagerStateMachine.State.MissionBriefing;

        private void Awake() {
            if (!TowerTagSettings.Home)
                Destroy(gameObject);

            if (PlayerHeadBase.GetInstance(out var playerHeadBase))
                _vrCamera = playerHeadBase.HeadCamera;

            _ingameUISettingsController.Init();
            DeactivateButtons();
        }

        private new void OnEnable() {
            base.OnEnable();
            if (_ingameUICanvas)
                _ingameUICanvas.SetActive(false);
            if (SharedControllerType.VR)
            {

                PlayerInput _rightXRController = (PlayerInput)PlayerInputBase.rightHand;
                PlayerInput _leftXRController = (PlayerInput)PlayerInputBase.leftHand;

                if (_rightXRController != null) { 
                    _rightXRController.OnMenuDown += MenuButtonClicked;
                    _rightXRController.OnMenuUp += MenuButtonReleased;
                }

                if (_leftXRController != null) { 
                    _leftXRController.OnMenuDown += MenuButtonClicked;
                    _leftXRController.OnMenuUp += MenuButtonReleased;
                }
            }
            GameManager.Instance.MissionBriefingStarted += OnMissionBriefingStarted;
            TTSceneManager.Instance.CommendationSceneLoaded += OnSceneLoaded;
            TTSceneManager.Instance.HubSceneLoaded += OnSceneLoaded;
            _localPlayer = PlayerManager.Instance.GetOwnPlayer();
            if (_localPlayer == null)
                PlayerManager.Instance.OwnPlayerSet += OwnPlayerReceived;
            else
                _localPlayer.TeleportHandler.CurrentPillarChanged += OnCurrentPillarChanged;
			
			 GameManager.Instance.MatchStarted += OnMatchStatusChanged;
            GameManager.Instance.MatchFinished += OnMatchStatusChanged;
            
            if(GameManager.Instance.CurrentMatch != null)
                OnMatchStatusChanged(GameManager.Instance.CurrentMatch);
        }

        private void Start() {
            _menuAnimationIsRunning = false;
            _transform = transform;
            _badaboomHyperactionPointerNeeded = GetComponent<BadaboomHyperactionPointerNeeded>();
            OverlayCanvasModel = FindObjectOfType<OverlayCanvasModel>();
            if (OverlayCanvasModel != null) {
                OverlayCanvasModel.OnOpen += CheckOverlayCanvas;
                OverlayCanvasModel.OnClose += CheckOverlayCanvas;
                CheckOverlayCanvas();
            }
        }

        private void Update() {
            if (TTSceneManager.Instance.IsInConnectScene) return;
            if (Input.GetKeyDown(KeyCode.Escape))
                ToggleMenu();

            if (_currentPillar == null) return;
            var cameraRotation = _vrCamera.transform.rotation; //vr camera not set
            var rotation = Quaternion.Euler(0, Mathf.RoundToInt(cameraRotation.eulerAngles.y / 90) * 90, 0);
            _transform.position =
                _currentPillar.transform.position +
                rotation * _offset; //tower.transform.InverseTransformVector(_offset);
            _transform.rotation = rotation;
        }

        private new void OnDisable() {
            base.OnDisable();
            if (OverlayCanvasModel != null) {
                OverlayCanvasModel.OnOpen -= CheckOverlayCanvas;
                OverlayCanvasModel.OnClose -= CheckOverlayCanvas;
            }

            if (SharedControllerType.VR)
            {
          
                PlayerInput _rightXRController = (PlayerInput)PlayerInputBase.rightHand;
                PlayerInput _leftXRController = (PlayerInput)PlayerInputBase.leftHand;

                if (_rightXRController != null) { 
                    _rightXRController.OnMenuDown -= MenuButtonClicked;
                    _rightXRController.OnMenuUp -= MenuButtonReleased;
                }

                if (_leftXRController != null) { 
                    _leftXRController.OnMenuDown -= MenuButtonClicked;
                    _leftXRController.OnMenuUp -= MenuButtonReleased;
                }
            }
            GameManager.Instance.MissionBriefingStarted -= OnMissionBriefingStarted;
            if (TTSceneManager.Instance != null) {
                TTSceneManager.Instance.CommendationSceneLoaded -= OnSceneLoaded;
                TTSceneManager.Instance.HubSceneLoaded -= OnSceneLoaded;
            }

            PlayerManager.Instance.OwnPlayerSet -= OwnPlayerReceived;
            if (_localPlayer != null)
                _localPlayer.TeleportHandler.CurrentPillarChanged -= OnCurrentPillarChanged;
			
			GameManager.Instance.MatchStarted -= OnMatchStatusChanged;
            GameManager.Instance.MatchFinished -= OnMatchStatusChanged;

            ResetIngameMenuSpawn();
        }
		
		private void OnMatchStatusChanged(IMatch obj)
        {
            _voteAbortMatchToggle.gameObject.SetActive(obj.MatchStarted);
        }

        private void OwnPlayerReceived(IPlayer player) {
            _localPlayer = player;
            _localPlayer.TeleportHandler.CurrentPillarChanged += OnCurrentPillarChanged;
            PlayerManager.Instance.OwnPlayerSet -= OwnPlayerReceived;
        }

        private void OnCurrentPillarChanged(Pillar oldPillar, Pillar newPillar) {
            _currentPillar = newPillar;
        }

        public override void TogglePointerNeededTag(object sender, bool status, bool immediately) {
            _badaboomHyperactionPointerNeeded.enabled = status;
        }

        private void OnSceneLoaded() {
			_voteAbortMatchToggle.gameObject.SetActive(false);
            ResetIngameMenuSpawn();
        }

        private void OnMissionBriefingStarted(MatchDescription obj, GameMode gameMode) {
            ResetIngameMenuSpawn();
        }

        private void MenuButtonReleased(PlayerInputBase fromSource)
        {
            _menuState = false;
            ToggleMenu();
        }
        private void MenuButtonClicked(PlayerInputBase fromSource) {
            _menuState = true;
            ToggleMenu();
        }

        private void ToggleMenu() {
            if (!IsMenuToggleAllowed || _menuAnimationIsRunning)
                return;

            if (SharedControllerType.VR && _menuState) return;

            bool active = IngameUIActive;
            CheckOverlayCanvas();
            SwitchPanel(PanelType.MainMenu);

            _menuAnimationIsRunning = true;

            // Register Animation EventListener
            if (IngameUIActive)
                _menuAnimationEventHandler.IngameMenuDeSpawn += OnIngameMenuDeSpawn;
            else
                _menuAnimationEventHandler.IngameMenuSpawn += OnIngameMenuSpawn;

            // Toggle UI & Player Gun State
            _localPlayer?.GunController.OnSetActive(active);
            ToggleIngameUI(!active);
        }

        private void ResetIngameMenuSpawn() {
            _menuAnimationEventHandler.IngameMenuSpawn -= OnIngameMenuSpawn;
            _menuAnimationEventHandler.IngameMenuSpawn -= OnIngameMenuDeSpawn;
            _menuAnimationIsRunning = false;
            ToggleIngameUI(false, true);
        }

        private void OnIngameMenuSpawn(object sender, bool animationFinished) {
            if (!animationFinished) return;
            _menuAnimationEventHandler.IngameMenuSpawn -= OnIngameMenuSpawn;
            _menuAnimationIsRunning = false;
        }

        private void OnIngameMenuDeSpawn(object sender, bool animationFinished) {
            if (!animationFinished) return;
            _menuAnimationEventHandler.IngameMenuDeSpawn -= OnIngameMenuDeSpawn;
            _menuAnimationIsRunning = false;
        }

        protected override void ActivateButtons() {
            _quitButton.interactable = true;
            _settingsButton.interactable = true;
            _leaveMatchButton.interactable = true;
        }

        protected override void DeactivateButtons() {
            _quitButton.interactable = false;
            _settingsButton.interactable = false;
            _leaveMatchButton.interactable = false;
        }

        [UsedImplicitly]
        public void OnLeaveMatchButtonPressed() {
            MessageQueue.Singleton.AddYesNoMessage(
                "This will disconnect you and abort any running match.",
                "Are You Sure?",
                null,
                null,
                "OK",
                LoadMainMenu,
                "CANCEL");
        }

        [UsedImplicitly]
        public void OnKickReportButtonClicked()
        {
            SwitchPanel(PanelType.KickReport);
        }

        [UsedImplicitly]
        public void OnCrossButtonPressed() {
            ToggleMenu();
        }

        [UsedImplicitly]
        public void OnSettingsButtonPressed() {
            SwitchPanel(PanelType.Settings);
        }

        /// <summary>
        /// Disconnect the player and load the main menu again.
        /// </summary>
        private void LoadMainMenu() {
            Debug.Log(name + ":" + GetType().Name + " - " + "Leave Room and load Main Menu");
            ToggleMenu();
            ConnectionManager.Instance.LeaveRoom();
        }
    }
}