using System;
using System.Collections.Generic;
using System.Linq;
using Home.UI;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomListFilter : MonoBehaviour {
    [SerializeField] private TMP_Dropdown _gameModeDropdown;
    [SerializeField] private TMP_Dropdown _mapDropdown;
    [SerializeField] private TMP_Dropdown _maxPlayerDropdown;
    [SerializeField] private Toggle _filterToggle;
    [SerializeField] private TMP_InputField _roomName;

    private GameMode _currentGameMode;
    private GameMode CurrentGameMode => _currentGameMode;
    private bool GameModeFilterSelected => !IsDefaultValueOfDropdownSelected(_gameModeDropdown);
    private bool MapFilterSelected => !IsDefaultValueOfDropdownSelected(_mapDropdown);
    private bool MaxPlayerFilterSelected => !IsDefaultValueOfDropdownSelected(_maxPlayerDropdown);
    private bool RoomNameEntered => !string.IsNullOrEmpty(_roomName.text);
    private string Map { get; set; }

    private int _maxPlayer;
    private int MaxPlayer => _maxPlayer;

    private const string DefaultDropdownValue = "ALL";

    private void Awake()
    {
        if(_filterToggle.isOn)
        {
            _filterToggle.onValueChanged.Invoke(false);
            _filterToggle.isOn = false;
        }

        FillModeDropdown();
        InitMapDropdown();
    }

    private void InitMapDropdown() {
        ResetDropdown(_mapDropdown);
    }

    private void FillModeDropdown() {
        ResetDropdown(_gameModeDropdown);
        var gameModes = Enum.GetValues(typeof(GameMode)).Cast<GameMode>();
        foreach (GameMode gameMode in gameModes) {
            if (gameMode == GameMode.UserVote) continue;
            _gameModeDropdown.options.Add(new TMP_Dropdown.OptionData(gameMode.ToString().ToUpper()));
        }
    }

    private void ResetDropdown(TMP_Dropdown dropdown) {
        dropdown.options = new List<TMP_Dropdown.OptionData> {new TMP_Dropdown.OptionData(DefaultDropdownValue)};
        dropdown.SetValueWithoutNotify(dropdown.value+1);
        dropdown.SetValueWithoutNotify(0);
        dropdown.onValueChanged.Invoke(0);
    }

    [UsedImplicitly]
    public void OnGameModeSelected(int index) {
        if(!IsDefaultValueOfDropdownSelected(_gameModeDropdown)) {
            Enum.TryParse(_gameModeDropdown.options[index].text, true, out _currentGameMode);
            ResetDropdown(_mapDropdown);
            _mapDropdown.options.AddRange(MatchDescriptionCollection.Singleton._matchDescriptions
                .Where(desc => desc.GameMode == _currentGameMode)
                .Select(desc => new TMP_Dropdown.OptionData(desc.MapName.ToUpper()))
                .ToList());
            OnMapSelected(0);
        }
        else {
            _currentGameMode = GameMode.UserVote;
            ResetDropdown(_mapDropdown);
        }
    }

    [UsedImplicitly]
    public void OnMapSelected(int index) {
        if(!IsDefaultValueOfDropdownSelected(_mapDropdown)) {
            Map = _mapDropdown.options[index].text;
            MatchDescription matchDescription = MatchDescriptionCollection.Singleton._matchDescriptions
                .First(desc => desc.GameMode == _currentGameMode
                                            && desc.MapName.Equals(Map,StringComparison.OrdinalIgnoreCase));
            ResetDropdown(_maxPlayerDropdown);
            _maxPlayerDropdown.options.AddRange(new[] {8, 6, 4, 2}
                .Where(n => n <= matchDescription.MatchUp.MaxPlayers)
                .Select(n => new TMP_Dropdown.OptionData(n.ToString()))
                .ToList());
            _maxPlayerDropdown.value = 0;
        }
        else {
            ResetDropdown(_maxPlayerDropdown);
            _maxPlayerDropdown.options.AddRange(new[] {8, 6, 4, 2}
                .Select(n => new TMP_Dropdown.OptionData(n.ToString()))
                .ToList());
            _maxPlayerDropdown.value = 0;
        }
        OnMaxPlayerSelected(0);
    }

    [UsedImplicitly]
    public void OnMaxPlayerSelected(int value) {
        int.TryParse(_maxPlayerDropdown.options[value].text, out _maxPlayer);
    }

    private bool IsDefaultValueOfDropdownSelected(TMP_Dropdown dropdown) {
        return dropdown.options[dropdown.value].text.Equals(DefaultDropdownValue);
    }

    public bool IsRoomLineValidForFilterSettings(RoomLine.RoomLineData data) {
        if (!_filterToggle.isOn) return true;
        bool valid = true;
        if (GameModeFilterSelected)
            valid &= data.GameMode.Equals(CurrentGameMode.ToString(), StringComparison.OrdinalIgnoreCase);
        if(MapFilterSelected)
            valid &= data.Map.Equals(Map, StringComparison.OrdinalIgnoreCase);
        if(MaxPlayerFilterSelected)
            valid &= data.MaxPlayers == MaxPlayer;
        if (RoomNameEntered)
            valid &= data.HostName.ToLower().Contains(_roomName.text.ToLower());
        return valid;
    }
}