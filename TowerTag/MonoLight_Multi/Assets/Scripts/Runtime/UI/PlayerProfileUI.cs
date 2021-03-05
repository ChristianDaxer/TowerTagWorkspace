using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameManagement;
using Home.UI;
using JetBrains.Annotations;
using TowerTag;
using TowerTagSOES;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
#if !UNITY_ANDROID
using Valve.VR;
#endif

namespace UI {
    public class PlayerProfileUI : Logger {
#region Behavior

        [Space, Header("Behavior")]
        [SerializeField, Tooltip("Should the name be saved to the disk when the connection is established?")]
        private bool _saveNameOnConnect = true;

        [SerializeField, Tooltip("Should the team be saved to the disk when the connection is established?")]
        private bool _saveTeamOnConnect = true;

        [SerializeField, Tooltip("The default name that is set if no Name was entered")]
        private string _defaultPlayerName = "VR-Nerd";

#endregion

#region Controller Objects

        [Space, Header("Controller Objects")]
        [SerializeField, Tooltip("The gameObject which holds the UiController Script Component")]
        private AudioUiController _audioUiController;

        [SerializeField, Tooltip("The message queue for the message overlay")]
        private MessageQueue _overlayMessageQueue;

#endregion

#region UI Objects

        [Space, Header("UI Objects")]
        [Header("Title Screen")]
        [SerializeField, Tooltip("The Animator of the Main Menu Canvas")]
        private Animator _mainMenuAnimator;

        [SerializeField, Tooltip("The game object which holds the Text Component for the title screen Connect Button")]
        private Text _connectButtonText;

        [Header("General")]
        [SerializeField, Tooltip("The game object which holds the InputField Component for the players name")]
        private InputField _nameTextField;

        [SerializeField, Tooltip("The Text field to display the location name")]
        private InputField _locationNameText;

        [SerializeField, Tooltip("The game object which holds the Dropdown menu for choosing the team")]
        private Dropdown _teamDropdown;

        [SerializeField, Tooltip("The Level of Quality of the Screenshots")]
        private Slider _screenShotResolution;

        [SerializeField, Tooltip("The game object which toggles the ingame Pillar Offset calibration.")]
        private Toggle _ingamePillarOffsetToggle;

        [SerializeField, Tooltip("Toggle Single Button Control")]
        private Toggle _singleButtonControl;

        [Header("Audio")]
        [SerializeField, Tooltip("The game object which holds the dropdown menu for choosing the audio input device")]
        private Dropdown _audioInputDevicesDropdown;

        [FormerlySerializedAs("_microphoneSensivitySlider")]
        [SerializeField,
         Tooltip(
             "The game object which holds the slider which defines the microphone sensitivity for the voice activation")]
        private Slider _microphoneSensitivitySlider;

        [SerializeField, Tooltip("The game object which holds the Toggle to control Photon voice chat")]
        private Toggle _voiceChatToggle;

        [SerializeField, Tooltip("The game object which holds the slider which defines the master audio volume")]
        private Slider _masterVolumeSlider;

        [SerializeField, Tooltip("The game object which holds the slider which defines the music audio volume")]
        private Slider _musicVolumeSlider;

        [SerializeField, Tooltip("The game object which holds the slider which defines the sound audio volume")]
        private Slider _soundVolumeSlider;

        [SerializeField, Tooltip("The game object which holds the slider which defines the announcer audio volume")]
        private Slider _announcerVolumeSlider;

        [SerializeField, Tooltip("The game object which holds the slider which defines the teammates audio volume")]
        private Slider _teammatesVolumeSlider;


        [Header("Network")] [SerializeField, Tooltip("The selectable photon region codes")]
        private string[] _regionCodes =
            {"best", "asia", "au", "cae", "cn", "eu", "in", "jp", "ru", "rue", "sa", "kr", "us", "usw"};

        [SerializeField, Tooltip("The game object which holds the InputField for the room name")]
        private InputField _roomNameTextField;

        [SerializeField, Tooltip("The gameObject which holds the Dropdown for the photon region")]
        private Dropdown _regionDropdown;

        [SerializeField,
         Tooltip(
             "The gameObject which holds the Toggle checkbox if the game should by played on a LAN server instead of the cloud")]
        private Toggle _playOnLanServerToggle;

        [SerializeField, Tooltip("The gameObject which holds the InputField for the LAN IP")]
        private InputField _lanIpTextField;

        [SerializeField, Tooltip("The gameObject which holds the InputField for the LAN Port")]
        private InputField _lanPortTextField;


        [Header("Haptics")]
        [SerializeField, Tooltip("The gameObject which holds the Toggle checkbox if the GunController should be used")]
        private Toggle _gunEnabledToggle;

        [SerializeField,
         Tooltip("The gameObject which holds the Text to display at which port the GunController is connected")]
        private Text _connectedGunText;

        [SerializeField, Tooltip("The gameObject which holds the Toggle checkbox if haptic hit feedback is enabled")]
        private Toggle _hapticHitFeedbackToggle;

        [SerializeField, Tooltip("The gameObject which holds the Toggle checkbox if haptic shot feedback is enabled")]
        private Toggle _hapticShotFeedbackToggle;

        [Header("Licensing")]
        [SerializeField, Tooltip("The gameObject which holds the Text to display the product key which is used")]
        private Text _productKeyText;

        [FormerlySerializedAs("_emailAdressText")] [SerializeField, Tooltip("The gameObject which holds the Text to display the email address which is used")]
        private Text _emailAddressText;

#endregion

        private const int LicenseCharsShown = 5;
        private const int EmailCharsShown = 2;
        private LicenseManager _licenseManager;
        private PlayerProfile _profile;
        private Configuration _config;
        private OverlayUiController _overlay;
        private bool _queryPending;

        private GameInitialization gameInitialization;

        /// <summary>
        /// Take name and team out of the config and use them
        /// </summary>
        private void Start() {
            if (gameInitialization == null)
            {
                if (!GameInitialization.GetInstance(out gameInitialization))
                    return;
            }

            _licenseManager = gameInitialization.GetComponent<LicenseManager>();

            if (PlayerProfileManager.CurrentPlayerProfile != null) {
                _profile = PlayerProfileManager.CurrentPlayerProfile;
                AnalyticsController.SetAnalyticsUserId(_profile.PlayerGUID);
            }
            else {
                LogError("currentPlayerProfile is null!");
            }

            _config = ConfigurationManager.Configuration;
            SetConfigValuesToUI();

            if (SharedControllerType.IsAdmin) {
                SetConnectButtonText("Open Room");
            }
            else if (SharedControllerType.Spectator) {
                SetConnectButtonText("Spectate");
            }
            else {
                SetConnectButtonText("Connect");
            }

            _overlay = gameInitialization.GetComponent<OverlayUiController>();
            if (!_overlay) {
                Debug.LogError("Couldn't find the " + _overlay.GetType().Name + " Component on the GameInit Prefab");
            }

            // Add a second before calling LoadingComplete to ensure that the animation will be smooth
            Invoke(nameof(LoadingComplete), 1.0f);
        }

        /// <summary>
        /// Gets called when the loading of the menu complete is
        /// </summary>
        private void LoadingComplete() {
            GetComponent<ScreenManager>().OpenPanel(_mainMenuAnimator);
        }

        /// <summary>
        /// Save name and team to disk
        /// </summary>
        [UsedImplicitly]
        public void OnConnectButtonPressed()
        {
            if (_saveNameOnConnect)
            {
                PlayerProfileManager.CurrentPlayerProfile.PlayerName = _nameTextField.text;
                PlayerProfileManager.WriteToFile();
            }

            if (PlayerProfileManager.CurrentPlayerProfile.PlayerName.Length < 1)
            {
                PlayerProfileManager.CurrentPlayerProfile.PlayerName = _defaultPlayerName;
            }

            if (_saveTeamOnConnect)
            {
                ConfigurationManager.Configuration.TeamID = _teamDropdown.value;
                ConfigurationManager.WriteConfigToFile();
            }

            CheckConfigMic();
#if !UNITY_ANDROID
            if (SharedControllerType.VR && OpenVR.Input == null)
            {
                StartCoroutine(StartOpenVRAndTryToConnect());
                return;
            }
#else
            //TODO might need to add another check here
#endif

            ConnectionManager.Instance.ConnectionManagerState = ConnectionManager.ConnectionState.MatchMaking;
            Connect();
        }

#if !UNITY_ANDROID
        private IEnumerator StartOpenVRAndTryToConnect() {
            VRController.ActivateOpenVR();
            int tryDuration = 3;
            float time = 0;
            while (OpenVR.Input == null || SteamVR.initializing) {
                time += Time.deltaTime;
                if (time >= tryDuration) {
                    _overlayMessageQueue.AddErrorMessage(
                        "CAN NOT START BECAUSE THE VR DEVICE IS NOT CONNECTED CORRECTLY");
                    yield break;
                }

                yield return null;
            }

            Connect();
        }
#else
        //TODO might need to add another check here
#endif

        private void Connect() {
            ConnectionManager.Instance.Connect();
            AnalyticsController.Connect(
                    gameInitialization.GlobalGameVersion,
                    _config.Room,
                    _profile.PlayerName,
                    TeamManager.Singleton.Get((TeamID) _config.TeamID).Name,
                    _config.PreferredRegion,
                    SharedControllerType.Singleton.Value.ToString(),
                    _config.PlayInLocalNetwork
                );
        }

        /// <summary>
        /// Save all configs to disk
        /// </summary>
        [UsedImplicitly]
        public void OnSaveButtonPressed()
        {
            SetUiValuesToConfig();
            ConfigurationManager.WriteConfigToFile();
            PlayerProfileManager.WriteToFile();
        }

        /// <summary>
        /// Reset all settings to the config values
        /// </summary>
        [UsedImplicitly]
        public void OnBackButtonPressed()
        {
            SetConfigValuesToAudioEngine();
        }

        /// <summary>
        /// To avoid having a wrong mic chosen
        /// </summary>
        private void CheckConfigMic() {
            if (!Microphone.devices.Contains(_config.TeamVoiceChatMicrophone)) {
                //Check for vive or rift microphone
                string mic = Microphone.devices.FirstOrDefault(dev => dev.ToLower().Contains("usb audio device"));
                mic = string.IsNullOrEmpty(mic)
                    ? Microphone.devices.FirstOrDefault(dev => dev.ToLower().Contains("rift"))
                    : mic;
                if (!string.IsNullOrEmpty(mic)) {
                    _config.TeamVoiceChatMicrophone = mic;
                    ConfigurationManager.WriteConfigToFile();
                    Debug.LogWarning("Mic set to " + mic);
                }
                else {
                    //Check if there is any other device
                    if (Microphone.devices.Length > 0) {
                        _config.TeamVoiceChatMicrophone = Microphone.devices[0];
                        ConfigurationManager.WriteConfigToFile();
                        Debug.LogWarning("Mic set to " + Microphone.devices[0]);
                    }

                    Debug.LogWarning("There is no active microphone!");
                }
            }
        }

        /// <summary>
        /// Takes the values from the UI and applies them to the config
        /// </summary>
        private void SetUiValuesToConfig()
        {
            // General Tab
            PlayerProfileManager.CurrentPlayerProfile.PlayerName = _nameTextField.text;
            _config.LocationName = _locationNameText ? _locationNameText.text : "";
            _config.TeamID = _teamDropdown.value;
            _config.ScreenShotResolution = (int) _screenShotResolution.value;
            _config.IngamePillarOffset = _ingamePillarOffsetToggle.isOn;
            _config.SingleButtonControl = _singleButtonControl.isOn;


            // Audio Tab
            if (Microphone.devices.Length > 0)
            {
                _config.TeamVoiceChatMicrophone = _audioInputDevicesDropdown.captionText.text;
            }
            else
            {
                Debug.Log("Computer has no Microphone attached");
                _config.TeamVoiceChatMicrophone = string.Empty;
            }

            _config.TeamVoiceChatVoiceDetectionThreshold = _microphoneSensitivitySlider.value;
            _config.TeamVoiceChatEnableVoiceChat = _voiceChatToggle.isOn;
            _config.MasterVolume = _audioUiController.GetMasterVolume();
            _config.MusicVolume = _audioUiController.GetMusicVolume();
            _config.SoundFxVolume = _audioUiController.GetSoundVolume();
            _config.AnnouncerVolume = _audioUiController.GetAnnouncerVolume();
            _config.TeammatesVolume = _audioUiController.GetTeammatesVolume();


            // Network Tab
            _config.Room = _roomNameTextField.text;
            _config.PreferredRegion = _regionCodes[_regionDropdown.value];
            _config.PlayInLocalNetwork = _playOnLanServerToggle.isOn;
            _config.ServerIp = _lanIpTextField.text;
            if (int.TryParse(_lanPortTextField.text, out int lanPort))
            {
                _config.ServerPort = lanPort;
            }
            else
            {
                Log("Couldn't convert the lanPort \"" + _lanPortTextField.text + "\" to an int");
            }


            // Gun Tab
            //config.arduinoCOMPortName
            _config.EnableRumbleController = _gunEnabledToggle.isOn;
            _config.EnableHapticHitFeedback = _hapticHitFeedbackToggle.isOn;
            _config.EnableHapticShootFeedback = _hapticShotFeedbackToggle.isOn;
        }

        /// <summary>
        /// Takes the values from the config and applies them to the UI
        /// </summary>
        public void SetConfigValuesToUI()
        {
            // General Tab
            _nameTextField.text = _profile.PlayerName;
            _locationNameText.text = _config.LocationName;
            _nameTextField.characterLimit = BitCompressionConstants.PlayerNameMaxLength;
            _teamDropdown.value = _config.TeamID;
            _screenShotResolution.value = _config.ScreenShotResolution;
            _ingamePillarOffsetToggle.isOn = _config.IngamePillarOffset;
            _singleButtonControl.isOn = _config.SingleButtonControl;

            // Audio Tab
            SetConfigValuesToAudioEngine();
            SetAudioEngineSettingsToUi();

            // Network Tab
            _roomNameTextField.text = _config.Room;
            FeedRegionDropdown();
            _regionDropdown.value = _regionCodes.Contains(_config.PreferredRegion)
                ? Array.IndexOf(_regionCodes, _config.PreferredRegion)
                : 0;
            _playOnLanServerToggle.isOn = _config.PlayInLocalNetwork;
            _lanIpTextField.text = _config.ServerIp;
            _lanPortTextField.text = _config.ServerPort.ToString();

            // Haptics Tab
            _connectedGunText.text = _config.ArduinoCOMPortName;
            _gunEnabledToggle.isOn = _config.EnableRumbleController;
            _hapticHitFeedbackToggle.isOn = _config.EnableHapticHitFeedback;
            _hapticShotFeedbackToggle.isOn = _config.EnableHapticShootFeedback;

            // License Tab
            SetLicenseInformationToUI();
        }

        /// <summary>
        /// Set the License Information from the LicenseManager/CryptLex to the UI
        /// </summary>
        private void SetLicenseInformationToUI()
        {
            if (TowerTagSettings.BasicMode || TowerTagSettings.Home) return; // no license in basic
#if !UNITY_ANDROID
            if (_licenseManager)
            {
                if (!SharedControllerType.IsAdmin && !SharedControllerType.Spectator)
                {
                    string productKey = _licenseManager.GetProductKey();
                    if (productKey == null)
                    {
                        Debug.LogWarning("Cannot load license information to display in UI: product key not found");
                        return;
                    }

                    _productKeyText.text = CensorString(productKey, LicenseCharsShown);

                    string[] splitEmail =
                        _licenseManager.GetEmailAddress(LicenseManager.CryptlexVersion.V3)?.Split('@');
                    if (splitEmail != null && splitEmail.Length == 2)
                    {
                        splitEmail[0] = CensorString(splitEmail[0], EmailCharsShown);
                        _emailAddressText.text = splitEmail[0] + "@" + splitEmail[1];
                    }
                    else
                    {
                        _emailAddressText.text = "invalid email";
                    }
                }
            }
            else
            {
                Debug.LogError("Couldn't find a " + _licenseManager.GetType().Name +
                               " Component on the GameInitialization object!");
            }
#endif
        }

        private static string CensorString([NotNull] string stringToHide, int lastCharsToShow = 0) {
            if (stringToHide.Length <= lastCharsToShow) return stringToHide;
            if (lastCharsToShow <= 0) return new string('*', stringToHide.Length);
            return new string('*', stringToHide.Length - lastCharsToShow)
                   + stringToHide.Substring(stringToHide.Length - lastCharsToShow);
        }

        /// <summary>
        /// Sets the audio settings from the config to the audio mixer
        /// </summary>
        private void SetConfigValuesToAudioEngine() {
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
            List<Dropdown.OptionData> audioInputDevices = new List<Dropdown.OptionData>();
            string audioDevices = string.Empty;
            for (int i = 0; i < Microphone.devices.Length; i++) {
                audioInputDevices.Add(new Dropdown.OptionData(Microphone.devices[i]));
                audioDevices += Microphone.devices[i];
                if (i < Microphone.devices.Length - 1) {
                    audioDevices += ", ";
                }
            }

            Log("found " + audioInputDevices.Count + " audio input devices: " + audioDevices);
            if (audioInputDevices.Count == 0) {
                audioInputDevices.Add(new Dropdown.OptionData("No input device"));
            }

            _audioInputDevicesDropdown.options = audioInputDevices;
            int configMicIndex = -1;
            for (int i = 0; i < _audioInputDevicesDropdown.options.Count; i++) {
                if (_audioInputDevicesDropdown.options[i].text == _config.TeamVoiceChatMicrophone) {
                    configMicIndex = i;
                    break;
                }
            }

            if (configMicIndex != -1)
                _audioInputDevicesDropdown.value = configMicIndex;

            _masterVolumeSlider.value = _audioUiController.GetMasterVolume();
            _musicVolumeSlider.value = _audioUiController.GetMusicVolume();
            _soundVolumeSlider.value = _audioUiController.GetSoundVolume();
            _announcerVolumeSlider.value = _audioUiController.GetAnnouncerVolume();
            _teammatesVolumeSlider.value = _audioUiController.GetTeammatesVolume();
            _voiceChatToggle.isOn = _config.TeamVoiceChatEnableVoiceChat;
        }

        /// <summary>
        /// Deactivate the Product Activation
        /// </summary>
        [UsedImplicitly]
        public void OnDeactivateButtonPressed()
        {
#if !UNITY_ANDROID
            if (!_queryPending)
            {
                _overlayMessageQueue.AddInputFieldMessage(
                    "",
                    "Enter license key..",
                    "",
                    "Enter current key without dashes (\"-\")",
                    InputFieldHelper.InputFieldType.PlayerName,
                    () => { _queryPending = true; },
                    () => { _queryPending = false; },
                    "Confirm",
                    DeactivateLicense,
                    "Cancel");
            }

            _queryPending = true;
#endif
        }

#if !UNITY_ANDROID
        private void DeactivateLicense(string enteredLicense)
        {
            string productKeyWithoutDashes = _licenseManager.GetProductKey()?.Replace("-", "");
            if (productKeyWithoutDashes != null && productKeyWithoutDashes.Equals(enteredLicense))
            {
                _queryPending = false;
                if (_licenseManager)
                {
                    if (!SharedControllerType.IsAdmin && !SharedControllerType.Spectator)
                    {
                        if (_licenseManager.DeactivateProduct())
                        {
                            TTSceneManager.Instance.LoadLicensingScene();
                            _overlayMessageQueue.AddVolatileMessage("You deactivated your license!", "Success");
                        }
                        else
                        {
                            Debug.LogError("Deactivation unsuccessful!");
                        }
                    }
                    else
                    {
                        Debug.Log("Skip deactivation because we are admin or spectator and not licensed");
                    }
                }
                else
                {
                    Debug.LogError("Couldn't find a " + _licenseManager.GetType().Name +
                                   " Component on the GameInitialization object!");
                }
            }
            else
            {
                _overlayMessageQueue.AddVolatileMessage("Your entered license key is not correct!", "Failed");
            }
        }
#endif

        /// <summary>
        /// Initially fills the Region Dropdown menu with the Photon Region Options
        /// </summary>
        private void FeedRegionDropdown() {
            _regionDropdown.options = _regionCodes.Select(code => new Dropdown.OptionData(code)).ToList();
        }

        /// <summary>
        /// Returns the currently chosen photon region
        /// </summary>
        /// <returns></returns>
        // ReSharper disable once UnusedMember.Local -> why not used?
        private string GetCloudRegionCodeFromRegionDropdown() {
            return _regionDropdown.options[_regionDropdown.value].text;
        }

        private void SetConnectButtonText(string newText) {
            _connectButtonText.text = newText;
        }

        [UsedImplicitly]
        public void OnLoadPillarOffsetToolButtonPressed()
        {
            SharedControllerType.Singleton.Set(this, ControllerType.PillarOffsetController);
            TTSceneManager.Instance.LoadScene("PillarOffsetTool");
        }

        /// <summary>
        /// Exit the application.
        /// </summary>
        [UsedImplicitly]
        public void OnExitButtonPressed()
        {
            if (SharedControllerType.Spectator)
            {
                MessageQueue.Singleton.AddYesNoMessage(
                    "Do you really want to quit...",
                    "Quit Game",
                    null,
                    null,
                    "YES",
                    Application.Quit);
            }
            else
            {
                _overlayMessageQueue.AddYesNoMessage(
                    "QUIT APPLICATION?", "CONFIRM APPLICATION QUIT", null, null, "YES", () =>
                    {
                        Debug.LogWarning("User quit application via PlayerProfileUI");
                        Application.Quit();
                    });
            }
        }
    }
}