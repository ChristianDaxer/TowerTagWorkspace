using System;
using System.Collections.Generic;
using AI;
using TMPro;
using TowerTag;
using UnityEngine;
using UnityEngine.UI;

public class BotLevel : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown _skillLevelDropdown;
    [SerializeField] private Image[] _dropdownImages;
    [SerializeField] private TMP_Text[] _texts;

    private string[] _botLevel;
    private PlayerLineController _playerLineController;
    private PlayerNetworkEventHandler _playerNetworkEventHandler;
    private IPlayer _player;

    private void OnEnable() {
        _skillLevelDropdown.onValueChanged.AddListener(OnValueChanged);
        RegisterPlayerEvents();
    }

    private void OnDisable() {
        _skillLevelDropdown.onValueChanged.RemoveListener(OnValueChanged);
        UnregisterPlayerEvents();
    }

    private void Start() {
        _playerLineController = GetComponent<PlayerLineController>();
        _player = _playerLineController.Player;
        _playerNetworkEventHandler = _player.PlayerNetworkEventHandler;
        RegisterPlayerEvents();

        _botLevel = Enum.GetNames(typeof(BotBrain.BotDifficulty));
        var skillLevel = new List<string>();
        _botLevel.ForEach(level => skillLevel.Add(level));
        _skillLevelDropdown.ClearOptions();
        _skillLevelDropdown.AddOptions(skillLevel);

        _dropdownImages = _skillLevelDropdown.gameObject.GetComponentsInChildren<Image>(true);
        _texts = _skillLevelDropdown.gameObject.GetComponentsInChildren<TMP_Text>(true);
        ChangeDropdownColor(_player, _player.TeamID);
    }

    private void OnValueChanged(int value) {
        _player.BotDifficulty = (BotBrain.BotDifficulty) value;
        _playerNetworkEventHandler.UpdateAIParameters((BotBrain.BotDifficulty)value);
    }

    private void RegisterPlayerEvents() {
        if (_player != null)
            _player.PlayerTeamChanged += ChangeDropdownColor;
    }

    private void UnregisterPlayerEvents() {
        if (_player != null)
            _player.PlayerTeamChanged -= ChangeDropdownColor;
    }

    private void ChangeDropdownColor(IPlayer player, TeamID teamID) {
        if (_dropdownImages.Length <= 0 || _texts.Length <= 0) {
            Debug.LogWarning("Can't colorize skill level dropdown! No images or texts to colorize found!");
        }
    }
}
