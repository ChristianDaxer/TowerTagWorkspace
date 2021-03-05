using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using Network;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using TowerTag;
using UnityEngine;
using UnityEngine.UI;

namespace Home.UI {
    public class CreateMatchUI : HomeMenuPanel {
        [SerializeField] private TMP_Dropdown _gameModeDropdown;
        [SerializeField] private TMP_Dropdown _mapDropdown;
        [SerializeField] private TMP_Dropdown _maxPlayersDropdown;
        [SerializeField] private Slider _matchTimeSlider;
        [SerializeField] private TMP_Text _timerText;
        [SerializeField] private Image _previewImage;
        [SerializeField] private Sprite _randomMapImage;
        [SerializeField] private Toggle _pinToggle;
        [SerializeField] private TMP_InputField _pin;
        [SerializeField] private Toggle _autostartToggle;
        [SerializeField] private Toggle _fillWithBotsToggle;
        [SerializeField] private GameMode _defaultMode;

        private new void Awake() {
            base.Awake();
            OnGameModeSelected(0);
            // OnMapSelected(0);
            // OnMaxPlayersSelected(0);
        }

        public override void OnEnable() {
            base.OnEnable();
            BalancingConfiguration.Singleton.MatchTimeInSeconds = BalancingConfiguration.Singleton.InitialMatchTimeInSeconds;
            _matchTimeSlider.value = BalancingConfiguration.Singleton.InitialMatchTimeInSeconds / 60;
        }

        [UsedImplicitly]
        public void OnBackButtonPressed() {
            UIController.SwitchPanel(HubUIController.PanelType.FindMatch);
        }

        [UsedImplicitly]
        public void OnStartMatchButtonPressed() {
            ConfigurationManager.Configuration.Room = PlayerProfileManager.CurrentPlayerProfile.PlayerGUID;
            Enum.TryParse(_gameModeDropdown.options[_gameModeDropdown.value].text, out GameMode gameMode);
            string map = gameMode != GameMode.UserVote ? _mapDropdown.options[_mapDropdown.value].text : null;
            byte maxPlayers = byte.Parse(_maxPlayersDropdown.options[_maxPlayersDropdown.value].text);

            RoomOptions options = _pinToggle.isOn
                ? RoomConfiguration.GetCustomRoomOptions(maxPlayers, gameMode, map, _matchTimeSlider.value, _autostartToggle.isOn, _pin.text, _fillWithBotsToggle.isOn)
                : RoomConfiguration.GetCustomRoomOptions(maxPlayers, gameMode, map, _matchTimeSlider.value, _autostartToggle.isOn, fillWithBots: _fillWithBotsToggle.isOn);

            PhotonNetwork.CreateRoom(ConfigurationManager.Configuration.Room, options);
        }

        [UsedImplicitly]
        public void OnGameModeSelected(int index) {
            Enum.TryParse(_gameModeDropdown.options[index].text, out GameMode gameMode);
            if (gameMode != GameMode.UserVote) {
                _mapDropdown.options = MatchDescriptionCollection.Singleton._matchDescriptions
                    .Where(desc => desc.GameMode.HasFlag(gameMode))
                    .Select(desc => new TMP_Dropdown.OptionData(desc.MapName))
                    .ToList();
                _mapDropdown.value = 0;
                OnMapSelected(0);
            }
            else {
                SetUserVoteSettings();
            }

            _previewImage.color = _previewImage.sprite == _randomMapImage
                ? TeamManager.Singleton.Get(TeamID.Ice).Colors.UI
                : Color.white;
        }

        [UsedImplicitly]
        public void OnMatchTimerSliderValueChanged(float value) {
            BalancingConfiguration.Singleton.MatchTimeInSeconds = (int) value * 60;
            _timerText.text = value.ToString(CultureInfo.CurrentCulture);
        }

        private void SetUserVoteSettings() {
            _mapDropdown.ClearOptions();
            _mapDropdown.options = new List<TMP_Dropdown.OptionData> {
                new TMP_Dropdown.OptionData("Random")
            };
            _mapDropdown.value = 0;
            _maxPlayersDropdown.ClearOptions();
            _maxPlayersDropdown.options = new List<TMP_Dropdown.OptionData>() {
                new TMP_Dropdown.OptionData("8"),
                new TMP_Dropdown.OptionData("6"),
                new TMP_Dropdown.OptionData("4"),
                new TMP_Dropdown.OptionData("2")
            };
            _maxPlayersDropdown.value = 0;
            _previewImage.sprite = _randomMapImage;
        }

        [UsedImplicitly]
        public void OnMapSelected(int index) {
            string map = _mapDropdown.options[index].text;
            Enum.TryParse(_gameModeDropdown.options[_gameModeDropdown.value].text, out GameMode gameMode);
            MatchDescription matchDescription = MatchDescriptionCollection.Singleton._matchDescriptions
                .First(desc => desc.GameMode.HasFlag(gameMode) && desc.MapName.Equals(map));
            _maxPlayersDropdown.options = new[] {8, 6, 4, 2}
                .Where(n => n <= matchDescription.MatchUp.MaxPlayers)
                .Select(n => new TMP_Dropdown.OptionData(n.ToString()))
                .ToList();
            _maxPlayersDropdown.value = 0;
            OnMaxPlayersSelected(0);
            UpdatePreviewImage(matchDescription);
        }

        [UsedImplicitly]
        public void OnMaxPlayersSelected(int index) {
        }

        private void UpdatePreviewImage(MatchDescription matchDescription) {
            if (matchDescription.MapScreenshot != null)
                _previewImage.sprite = matchDescription.MapScreenshot;
            else {
                _previewImage.sprite = null;
                Debug.LogWarning("No image for match description found! Please insert in dictionary");
            }
        }
    }
}