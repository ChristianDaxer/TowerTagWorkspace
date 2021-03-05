using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Home.UI;
using JetBrains.Annotations;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using Runtime.Pillar.PillarCalibration;
using Runtime.UI;
using TMPro;
using TowerTagSOES;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;
using Slider = UnityEngine.UI.Slider;
using Toggle = UnityEngine.UI.Toggle;

namespace UI {
    /// <summary>
    /// Manages the settings panel of the ingame ui (only home version)
    /// </summary>
    [RequireComponent(typeof(AudioUiController))]
    public class IngameUISettingsController : HomeMenuPanel {
        [SerializeField] private AudioClip _saveSound;
        private PlayerProfile _profile;
        private Configuration _config;
        private AudioUiController _audioUiController;

        public delegate void IngameUISettingsAction(object sender);

        public event IngameUISettingsAction IngameSettingsSaveButtonPressed;

        #region UiObjects

        [Header("General Settings")] [SerializeField, Tooltip("General: text field for the players name")]
        private TMP_InputField _playerNameInputField;

        [SerializeField, Tooltip("Toggle Single Button Control")]
        private Toggle _singleButtonControlToggle;

        [SerializeField, Tooltip("Toggle Standing VS RoomScale Play Area")]
        private Toggle _smallPlayArea;

        [SerializeField, Tooltip("Toggle debug Log Shots Per Second")]
        private Toggle _showShotsPerSecond;

        [SerializeField] private GameObject _shotsPerSecondToggle;

        [SerializeField, Tooltip("Button: Pressed to reset SPA")]
        private Button _resetSpaButton;

        [SerializeField, Tooltip("Toggle small play Area rotation direction")]
        private Toggle _invertSmallPlayArea;

        [SerializeField, Tooltip("Toggle custom user play space offset")]
        private Toggle _customPlaySpaceOffset;

        [SerializeField] private Text[] _adjustCustomPlaySpaceText;

        [SerializeField, Tooltip("Button: Pressed to reset offset values")]
        private Button _resetOffsetButton;

        [SerializeField] private SettingsUiOffsetValueGroup _playSpaceOffsetX;

        [SerializeField] private SettingsUiOffsetValueGroup _playSpaceOffsetZ;

        [SerializeField] private SettingsUiOffsetValueGroup _playSpaceOffsetRotation;

        [SerializeField, Tooltip("Region Dropdown")]
        private TMP_Dropdown _regionDropdown;

        [SerializeField] private TMP_Dropdown _preferredHand;
        [SerializeField] private Toggle _bHaptics;

        [Header("Audio Input")] [SerializeField, Tooltip("Audio (input): dropdown for input device")]
        private TMP_Dropdown _inputDevicesDropDown;

        [SerializeField, Tooltip("Audio (input): Slider for mic sensitivity")]
        private Slider _sensitivity;

        [SerializeField, Tooltip("Audio (input): Slider for mic Volume")]
        private Slider _micVolume;

        [SerializeField, Tooltip("Audio (output): Toggle output others on/off")]
        private Toggle _enableVoiceChatToggle;

        [Header("Audio Output")] [SerializeField, Tooltip("Audio (output): value slider for music output")]
        private Slider _masterVolumeSlider;

        [SerializeField, Tooltip("Audio (output): value slider for speech output")]
        private Slider _musicVolumeSlider;

        [SerializeField, Tooltip("Audio (output): value slider for sound output")]
        private Slider _effectsVolumeSlider;

        [SerializeField, Tooltip("Audio (output): value slider for voice ingame")]
        private Slider _announcerVolumeSlider;

        [SerializeField, Tooltip("Audio (output): value slider for voice others")]
        private Slider _teamMatesVolumeSlider;

        [Header("Buttons")] [SerializeField, Tooltip("Button: pressed to close the settings & go back to main panel")]
        private Button _backButton;

        [SerializeField, Tooltip("Button: Pressed to save ui values to config")]
        private Button _saveButton;

        private Dictionary<string, string> _shortToFullInputDeviceName;
        private Recorder _photonVoice;

        #endregion

        public void Init() {
            if (!Debug.isDebugBuild) {
                _shotsPerSecondToggle.SetActive(false);
            }

            _config = ConfigurationManager.Configuration;
            _audioUiController = GetComponent<AudioUiController>();
            SetConfigValuesToAudioEngine();
            FeedRegionDropdown();
            FeedHandDropdown();
            _photonVoice = PhotonVoiceNetwork.Instance.GetComponent<Recorder>();
        }

        private void OnPositionXOffsetButtonPressed(object sender, bool direction) {
            // get direction
            var temp = direction ? -1 : 1;

            // set current offset
            _playSpaceOffsetX.CustomOffset += temp * PillarOffsetManager.Instance.OffsetStepsPosition;

            ApplyTempPillarOffset(_playSpaceOffsetX.CustomOffset, _playSpaceOffsetZ.CustomOffset,
                _playSpaceOffsetRotation.CustomOffset);
        }

        private void OnPositionZOffsetButtonPressed(object sender, bool direction) {
            var temp = direction ? -1 : 1;
            _playSpaceOffsetZ.CustomOffset += temp * PillarOffsetManager.Instance.OffsetStepsPosition;
            ApplyTempPillarOffset(_playSpaceOffsetX.CustomOffset, _playSpaceOffsetZ.CustomOffset,
                _playSpaceOffsetRotation.CustomOffset);
        }

        private void OnRotationOffsetButtonPressed(object sender, bool direction) {
            var temp = direction ? 1 : -1;
            _playSpaceOffsetRotation.CustomOffset += temp * PillarOffsetManager.Instance.OffsetStepsRotation;

            ApplyTempPillarOffset(_playSpaceOffsetX.CustomOffset, _playSpaceOffsetZ.CustomOffset,
                _playSpaceOffsetRotation.CustomOffset);
        }

        private static void ApplyTempPillarOffset(float xPosOffset, float zPosOffset, float rotOffset) {
            // get pos & rot values
            var positionOffset = new Vector3(xPosOffset, 0, zPosOffset);
            var rotationOffset = Quaternion.AngleAxis(-rotOffset, Vector3.up);

            // check pillar offset manager -> force pillar offset
            if (PillarOffsetManager.Instance == null)
                Debug.LogError("Cant find Input Controller for VR Player");
            else
                PillarOffsetManager.Instance.ApplyPillarOffset.ApplyOffset(positionOffset, rotationOffset);
        }

        public override void OnEnable() {
            if (PlayerProfileManager.CurrentPlayerProfile != null) {
                _profile = PlayerProfileManager.CurrentPlayerProfile;
                AnalyticsController.SetAnalyticsUserId(_profile.PlayerGUID);
            }
            else {
                Debug.LogError("currentPlayerProfile is null!");
            }

            if (_config == null)
                Init();

            SetConfigSettingsToUi();

            if (_playerNameInputField == null) {
                Debug.LogError("Can't find player name input field action helper.'");
            }

            // Check Scene -> may enable/disable some ui objects in specific scenes

            if (!TTSceneManager.Instance.IsInConnectScene) {
                ToggleUiElement(false, _playerNameInputField);
                ToggleUiElement(false, _regionDropdown);
                ToggleUiElement(false, _bHaptics);
            }

            ConfigurationManager.ConfigurationUpdated += OnConfigurationUpdated;

            _playSpaceOffsetX.ToggleButtonInteraction(!GameManager.Instance.IsStateMachineInMatchState());
            _playSpaceOffsetZ.ToggleButtonInteraction(!GameManager.Instance.IsStateMachineInMatchState());
            _playSpaceOffsetRotation.ToggleButtonInteraction(!GameManager.Instance.IsStateMachineInMatchState());
            ToggleUiElement(!GameManager.Instance.IsStateMachineInMatchState(), _resetOffsetButton);
            ToggleUiElement(GameManager.Instance.IsStateMachineInMatchState(), _resetSpaButton);
            ToggleUiElement(!GameManager.Instance.IsStateMachineInMatchState(), _customPlaySpaceOffset);
            _adjustCustomPlaySpaceText.ForEach(text =>
                ToggleUiElement(!GameManager.Instance.IsStateMachineInMatchState(), text));

            _playSpaceOffsetX.SettingsUiOffsetValueButtonPressed += OnPositionXOffsetButtonPressed;
            _playSpaceOffsetZ.SettingsUiOffsetValueButtonPressed += OnPositionZOffsetButtonPressed;
            _playSpaceOffsetRotation.SettingsUiOffsetValueButtonPressed += OnRotationOffsetButtonPressed;
        }

        public override void OnDisable() {
            ConfigurationManager.ConfigurationUpdated -= OnConfigurationUpdated;
            _playSpaceOffsetX.SettingsUiOffsetValueButtonPressed -= OnPositionXOffsetButtonPressed;
            _playSpaceOffsetZ.SettingsUiOffsetValueButtonPressed -= OnPositionZOffsetButtonPressed;
            _playSpaceOffsetRotation.SettingsUiOffsetValueButtonPressed -= OnRotationOffsetButtonPressed;

            if (PillarOffsetManager.Instance == null)
                Debug.LogError("Cant find Input Controller for VR Player");
            else {
                if (_config.PillarPositionOffset != new Vector3(
                        _playSpaceOffsetX.CustomOffset,
                        0,
                        _playSpaceOffsetZ.CustomOffset) ||
                    Math.Abs(_config.PillarRotationOffsetAngle - _playSpaceOffsetRotation.CustomOffset) > 0.001)
                    ApplyTempPillarOffset(_config.PillarPositionOffset.x, _config.PillarPositionOffset.z,
                        _config.PillarRotationOffsetAngle);
            }

            // Reset ui all ui objects
            ToggleAllSettingsUiObjects(true);
        }

        private void OnConfigurationUpdated() {
            if (CheckConfigurationChanges())
                SetConfigSettingsToUi();
        }

        private void FeedHandDropdown() {
            _preferredHand.options.Clear();
            var handOptions = new List<TMP_Dropdown.OptionData>();

            if (InputControllerVR.Instance != null)
            {
                var enumValues = Enum.GetValues(typeof(PlayerHand));
                foreach (var enumValue in enumValues)
                    handOptions.Add(new TMP_Dropdown.OptionData(enumValue.ToString().ToUpper()));

                _preferredHand.options = handOptions;

                TMP_Dropdown.OptionData currentOption = _preferredHand.options
                    .FirstOrDefault(option => option.text.Equals(InputControllerVR.Instance.TargetHand.ToString().ToUpper()));

                _preferredHand.value = _preferredHand.options.IndexOf(currentOption);
            }
        }

        private void FeedRegionDropdown() {
            PhotonRegionHelper.FillRegionsIntoDropdown(_regionDropdown);
        }

        /// <summary>
        /// Sets the general settings from the config to the UI
        /// </summary>
        private void SetConfigSettingsToUi() {
            // general settings
            _playerNameInputField.text = _profile.PlayerName;
            _showShotsPerSecond.isOn = _config.ShowShotsPerSecond;

            // Locomotion
            _singleButtonControlToggle.isOn = _config.SingleButtonControl;
            _smallPlayArea.isOn = _config.SmallPlayArea;
            _bHaptics.isOn = _config.EnableHapticHitFeedback;
            _invertSmallPlayArea.isOn = _config.InvertSmallPlayArea;
            _customPlaySpaceOffset.isOn = _config.IngamePillarOffset;
            _playSpaceOffsetX.CustomOffset = _config.PillarPositionOffset.x;
            _playSpaceOffsetZ.CustomOffset = _config.PillarPositionOffset.z;
            _playSpaceOffsetRotation.CustomOffset = _config.PillarRotationOffsetAngle;

            TMP_Dropdown.OptionData currentRegionOption = _regionDropdown.options
                .FirstOrDefault(option =>
                    option.text.Equals(PhotonRegionHelper.GetRegionNameByCode(PhotonRegionHelper.CurrentRegion)));
            _regionDropdown.value =
                _regionDropdown.options.IndexOf(currentRegionOption);

            // audio input/output
            SetConfigValuesToAudioEngine();
            SetAudioEngineSettingsToUi();
        }

        /// <summary>
        /// Sets the audio settings from the audio mixer and config to the UI
        /// </summary>
        private void SetUiSettingsToConfig() {
            // General
            _profile.PlayerName = _playerNameInputField.text;

            // Audio Tab
            if (Microphone.devices.Length > 0) {
                var captionText = _inputDevicesDropDown.captionText.text;
                if (_shortToFullInputDeviceName.ContainsKey(captionText))
                    _config.TeamVoiceChatMicrophone = _shortToFullInputDeviceName[captionText];
                else
                    Debug.LogWarning("Selected mic not in dictionary!");
            }
            else {
                Debug.LogWarning("Computer has no Microphone attached");
                _config.TeamVoiceChatMicrophone = string.Empty;
            }

            if (SharedControllerType.VR) {
                if (InputControllerVR.Instance == null)
                    Debug.LogError("Cant find Input Controller for VR Player");
                else
                    InputControllerVR.Instance.SetControllerAsPreferred(
                        _preferredHand.captionText.text.Equals("RIGHT"));
            }

            _config.SingleButtonControl = _singleButtonControlToggle.isOn;
            _config.SmallPlayArea = _smallPlayArea.isOn;
            _config.ShowShotsPerSecond = _showShotsPerSecond.isOn;
            _config.InvertSmallPlayArea = _invertSmallPlayArea.isOn;
            _config.IngamePillarOffset = _customPlaySpaceOffset.isOn;
            _config.PillarPositionOffset = new Vector3(
                _playSpaceOffsetX.CustomOffset,
                0,
                _playSpaceOffsetZ.CustomOffset);
            _config.PillarRotationOffsetAngle = _playSpaceOffsetRotation.CustomOffset;
            _config.EnableHapticHitFeedback = _bHaptics.isOn;
            _config.TeamVoiceChatVoiceDetectionThreshold = _sensitivity.value;
            _config.TeamVoiceChatEnableVoiceChat = _enableVoiceChatToggle.isOn;
            _config.MasterVolume = _audioUiController.GetMasterVolume();
            _config.MusicVolume = _audioUiController.GetMusicVolume();
            _config.SoundFxVolume = _audioUiController.GetSoundVolume();
            _config.AnnouncerVolume = _audioUiController.GetAnnouncerVolume();
            _config.TeammatesVolume = _audioUiController.GetTeammatesVolume();
        }

        [UsedImplicitly]
        public void OnThresholdChanged(float value) {
            if (_photonVoice != null)
                _photonVoice.VoiceDetectionThreshold = value;
        }

        [UsedImplicitly]
        public void OnInputDeviceChanged(int i) {
            if (_photonVoice != null) {
                StartCoroutine(SetRecorderMic(_shortToFullInputDeviceName[_inputDevicesDropDown.captionText.text]));
            }
        }

        private IEnumerator SetRecorderMic(string newMic) {
            _photonVoice.UnityMicrophoneDevice = newMic;
            yield return new WaitForSeconds(0.5f);
            _photonVoice.RestartRecording();
        }

        [UsedImplicitly]
        public void OnResetPlaySpacePressed() {
            if (PillarOffsetManager.Instance == null)
                Debug.LogError("Cant find Pillar Offset Manager");
            else
                PillarOffsetManager.Instance.RotatePlaySpaceMovement.ResetRotatePlaySpaceMovement();
        }

        [UsedImplicitly]
        public void OnResetPlaySpaceOffsetPressed() {
            ResetTempCustomPlaySpaceOffset();
        }

        [UsedImplicitly]
        public void OnToggleAdjustPlaySpace() {
            if (ConfigurationManager.Configuration.IngamePillarOffset && _customPlaySpaceOffset.isOn) {
                _playSpaceOffsetX.CustomOffset = ConfigurationManager.Configuration.PillarPositionOffset.x;
                _playSpaceOffsetZ.CustomOffset = ConfigurationManager.Configuration.PillarPositionOffset.z;
                _playSpaceOffsetRotation.CustomOffset = ConfigurationManager.Configuration.PillarRotationOffsetAngle;
                if (PillarOffsetManager.Instance == null)
                    Debug.LogError("Cant find Pillar Offset Manager");
                else {
                    ApplyTempPillarOffset(_playSpaceOffsetX.CustomOffset, _playSpaceOffsetZ.CustomOffset,
                        _playSpaceOffsetRotation.CustomOffset);
                }
            }

            if (ConfigurationManager.Configuration.IngamePillarOffset && !_customPlaySpaceOffset.isOn
                || !ConfigurationManager.Configuration.IngamePillarOffset && _customPlaySpaceOffset.isOn)
                ResetTempCustomPlaySpaceOffset();
        }

        private void ResetTempCustomPlaySpaceOffset() {
            _playSpaceOffsetX.CustomOffset = 0f;
            _playSpaceOffsetZ.CustomOffset = 0f;
            _playSpaceOffsetRotation.CustomOffset = 0f;
            if (PillarOffsetManager.Instance == null)
                Debug.LogError("Cant find Pillar Offset Manager");
            else {
                ApplyTempPillarOffset(_playSpaceOffsetX.CustomOffset, _playSpaceOffsetZ.CustomOffset,
                    _playSpaceOffsetRotation.CustomOffset);
            }
        }

        /// <summary>
        /// close the settings ui panel and activate the main panel
        /// </summary>
        [UsedImplicitly]
        public void OnBackButtonPressed() {
            if (CheckConfigurationChanges())
                MessageQueue.Singleton.AddYesNoMessage(
                    "Do you want to return to main menu without saving your settings?",
                    "Are you sure?",
                    null,
                    null,
                    "Yes",
                    BackWithoutSaving);
            else {
                BackWithoutSaving();
            }
        }

        private void BackWithoutSaving() {
            if (PillarOffsetManager.Instance == null)
                Debug.LogError("Cant find Input Controller for VR Player");
            else {
                ApplyTempPillarOffset(_config.PillarPositionOffset.x, _config.PillarPositionOffset.z,
                    _config.PillarRotationOffsetAngle);
            }

            SetConfigValuesToRecorder();
            SetConfigValuesToAudioEngine();
            CloseSettingsPanel();
        }

        private void SetConfigValuesToRecorder() {
            StartCoroutine(SetRecorderMic(_config.TeamVoiceChatMicrophone));
            _photonVoice.VoiceDetectionThreshold = _config.TeamVoiceChatVoiceDetectionThreshold;
        }

        private void CloseSettingsPanel() {
            UIController.SwitchPanel(HubUIController.PanelType.MainMenu);
        }

        /// <summary>
        /// Saves the settings from ui to config
        /// </summary>
        [UsedImplicitly]
        public void OnSaveButtonPressed() {
            SetUiSettingsToConfig();
            ConfigurationManager.WriteConfigToFile();
            PlayerProfileManager.WriteToFile();
            IngameSettingsSaveButtonPressed?.Invoke(this);
            UIController.PlayAudioSound(_saveSound);
            if (!_regionDropdown.captionText.text.Equals(
                PhotonRegionHelper.GetRegionNameByCode(PhotonRegionHelper.CurrentRegion))) {
                MessageQueue.Singleton.AddYesNoMessage(
                    "You changed your region. This will cause a restart of the application." +
                    "Are you sure you want to save the new region?",
                    "Are you sure?",
                    null,
                    null,
                    "YES",
                    ChangeRegion,
                    "NO",
                    CloseSettingsPanel);
            }
            else {
                CloseSettingsPanel();
            }
        }

        /// <summary>
        /// Change  chaperone size in x & z dimensions and change rotation on Oculus
        /// </summary>
        [UsedImplicitly]
        public void OnPositionXOffsetButtonClick(bool direction)
        {
#if UNITY_ANDROID
            var temp = direction ? -1 : 1;
            _playSpaceOffsetX.CustomOffset += temp * PillarOffsetManager.Instance.OffsetStepsPosition;

            ApplyTempPillarOffset(_playSpaceOffsetX.CustomOffset, _playSpaceOffsetZ.CustomOffset,
                _playSpaceOffsetRotation.CustomOffset);
#endif
        }

        [UsedImplicitly]
        public void OnPositionZOffsetButtonClick(bool direction)
        {
#if UNITY_ANDROID
            var temp = direction ? -1 : 1;
            _playSpaceOffsetZ.CustomOffset += temp * PillarOffsetManager.Instance.OffsetStepsPosition;
            ApplyTempPillarOffset(_playSpaceOffsetX.CustomOffset, _playSpaceOffsetZ.CustomOffset,
                _playSpaceOffsetRotation.CustomOffset);
#endif
        }

        [UsedImplicitly]
        public void OnRotationOffsetButtonClick(bool isLeft)
        {
#if UNITY_ANDROID
            var temp = isLeft ? -1 : 1;
            _playSpaceOffsetRotation.CustomOffset += temp * PillarOffsetManager.Instance.OffsetStepsRotation;

            ApplyTempPillarOffset(_playSpaceOffsetX.CustomOffset, _playSpaceOffsetZ.CustomOffset,
                _playSpaceOffsetRotation.CustomOffset);
#endif
        }

        private void ChangeRegion() {
            ConnectionManagerHome.Instance.ChangeRegion(_regionDropdown.captionText.text, UIController);
        }

        /// <summary>
        /// Sets the audio settings from the config to the audio mixer
        /// </summary>
        private void SetConfigValuesToAudioEngine() {
            if (_config == null) return;
            _audioUiController.SetMasterVolume(Mathf.Clamp01(_config.MasterVolume));
            _audioUiController.SetMusicVolume(Mathf.Clamp01(_config.MusicVolume));
            _audioUiController.SetSoundVolume(Mathf.Clamp01(_config.SoundFxVolume));
            _audioUiController.SetAnnouncerVolume(Mathf.Clamp01(_config.AnnouncerVolume));
            _audioUiController.SetTeammatesVolume(Mathf.Clamp01(_config.TeammatesVolume));
        }

        /// <summary>
        /// Sets the audio settings from the audio mixer and config to the UI
        /// </summary>
        private void SetAudioEngineSettingsToUi() {
            // audio input

            SetAudioInputDevicesDropDown();
            if (_config != null)
                _sensitivity.value = _config.TeamVoiceChatVoiceDetectionThreshold;
            // mic volume?
            if (_config != null)
                _enableVoiceChatToggle.isOn = _config.TeamVoiceChatEnableVoiceChat;

            // audio output

            _masterVolumeSlider.value = _audioUiController.GetMasterVolume();
            _musicVolumeSlider.value = _audioUiController.GetMusicVolume();
            _effectsVolumeSlider.value = _audioUiController.GetSoundVolume();
            _announcerVolumeSlider.value = _audioUiController.GetAnnouncerVolume();
            _teamMatesVolumeSlider.value = _audioUiController.GetTeammatesVolume();
        }

        private void SetAudioInputDevicesDropDown() {
            _inputDevicesDropDown.ClearOptions();
            _shortToFullInputDeviceName = new Dictionary<string, string>();
            var audioInputDevices = new List<TMP_Dropdown.OptionData>();
            foreach (string micName in Microphone.devices) {
                string shortName = micName.Split('(', ')')[1];
                if (string.IsNullOrEmpty(micName) || string.IsNullOrEmpty(shortName)) {
                    Debug.LogWarning("Not a valid mic");
                    continue;
                }

                _shortToFullInputDeviceName.Add(shortName, micName);
                audioInputDevices.Add(new TMP_Dropdown.OptionData(shortName));
            }

            Debug.Log("found " + audioInputDevices.Count + " audio input devices");
            if (audioInputDevices.Count == 0) {
                audioInputDevices.Add(new TMP_Dropdown.OptionData("No input device"));
            }

            _inputDevicesDropDown.options = audioInputDevices;
            int configMicIndex = -1;
            for (var i = 0; i < _inputDevicesDropDown.options.Count; i++) {
                if (_shortToFullInputDeviceName[_inputDevicesDropDown.options[i].text] !=
                    _config.TeamVoiceChatMicrophone)
                    continue;
                configMicIndex = i;
                break;
            }

            if (configMicIndex != -1)
                _inputDevicesDropDown.value = configMicIndex;
        }

        private bool CheckConfigurationChanges() {
            const double floating = 0.001;
            if (_config.SingleButtonControl != _singleButtonControlToggle.isOn) return true;
            if (_config.SmallPlayArea != _smallPlayArea.isOn) return true;
            if (_config.ShowShotsPerSecond != _showShotsPerSecond.isOn) return true;
            if (_config.InvertSmallPlayArea != _invertSmallPlayArea.isOn) return true;
            if (_config.IngamePillarOffset != _customPlaySpaceOffset.isOn) return true;
            if (_config.PillarPositionOffset != new Vector3(
                _playSpaceOffsetX.CustomOffset,
                0,
                _playSpaceOffsetZ.CustomOffset)) return true;
            if (Math.Abs(_config.PillarRotationOffsetAngle - _playSpaceOffsetRotation.CustomOffset) >
                floating) return true;
            if (_config.EnableHapticHitFeedback != _bHaptics.isOn) return true;
            if (Math.Abs(_config.TeamVoiceChatVoiceDetectionThreshold - _sensitivity.value) > floating) return true;
            if (_config.TeamVoiceChatEnableVoiceChat != _enableVoiceChatToggle.isOn) return true;
            if (Math.Abs(_config.MasterVolume - _masterVolumeSlider.value) > floating) return true;
            if (Math.Abs(_config.MusicVolume - _musicVolumeSlider.value) > floating) return true;
            if (Math.Abs(_config.SoundFxVolume - _effectsVolumeSlider.value) > floating) return true;
            if (Math.Abs(_config.AnnouncerVolume - _announcerVolumeSlider.value) > floating) return true;
            if (Math.Abs(_config.TeammatesVolume - _teamMatesVolumeSlider.value) > floating) return true;

            return false;
        }

        private void ToggleAllSettingsUiObjects(bool status, bool playerNameInputFieldSelected = false) {
            // player name input field should not disabled when it is selected
            if (!playerNameInputFieldSelected)
                _playerNameInputField.interactable = status;
            _sensitivity.interactable = status;
            _singleButtonControlToggle.interactable = status;
            _smallPlayArea.interactable = status;
            _showShotsPerSecond.interactable = status;
            _invertSmallPlayArea.interactable = status;
            _customPlaySpaceOffset.interactable = status;
            _playSpaceOffsetX.ToggleButtonInteraction(status);
            _playSpaceOffsetZ.ToggleButtonInteraction(status);
            _enableVoiceChatToggle.interactable = status;
            _masterVolumeSlider.interactable = status;
            _musicVolumeSlider.interactable = status;
            _effectsVolumeSlider.interactable = status;
            _announcerVolumeSlider.interactable = status;
            _teamMatesVolumeSlider.interactable = status;
            _backButton.interactable = status;
            _saveButton.interactable = status;
        }

        [SuppressMessage("ReSharper", "RedundantAssignment")]
        private void ToggleUiElement<T>(bool status, T uiElement) {
            switch (uiElement) {
                case Dropdown dropdown:
                    dropdown = (Dropdown) (object) uiElement;
                    dropdown.interactable = status;
                    break;
                case TMP_InputField tmpInputField:
                    tmpInputField = (TMP_InputField) (object) uiElement;
                    tmpInputField.interactable = status;
                    break;
                case TMP_Dropdown tmpDropdown:
                    tmpDropdown = (TMP_Dropdown) (object) uiElement;
                    tmpDropdown.interactable = status;
                    break;
                case InputField inputField:
                    inputField = (InputField) (object) uiElement;
                    inputField.interactable = status;
                    break;
                case Slider slider:
                    slider = (Slider) (object) uiElement;
                    slider.interactable = status;
                    break;
                case Toggle toggle:
                    toggle = (Toggle) (object) uiElement;
                    toggle.interactable = status;
                    break;
                case Button button:
                    button = (Button) (object) uiElement;
                    button.interactable = status;
                    break;
                case Text text:
                    text = (Text) (object) uiElement;
                    text.color = status ? Color.white : Color.gray;
                    break;
                default:
                    Debug.LogError("unknown type of object");
                    break;
            }
        }
    }
}