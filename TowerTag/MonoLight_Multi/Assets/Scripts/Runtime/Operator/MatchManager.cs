using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TMPro;
using TowerTag;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the editable MatchSettings of the OperatorUI
/// </summary>
public class MatchManager : MonoBehaviour {
    [Header("General Options")] [SerializeField, Tooltip("Drag all the possible MatchUps for this Build here")]
    private MatchUp[] _eligibleMatchUps;

    [SerializeField] private MatchDescriptionCollection _matchDescriptionCollection;

    [SerializeField,
     Tooltip(
         "The time steps at which the match time can be configured in seconds (e.g. 30s for 4:00, 4:30, 5:00 etc.)")]
    private int _matchTimeTimeSteps = 60;

    [Header("UI Components")] [SerializeField, Tooltip("Drag the dropdown menu for the match up choice here")]
    private TMP_Dropdown _matchUpDropdown;

    [SerializeField, Tooltip("Drag the dropdown menu for the match up choice here")]
    private TMP_Dropdown _gameModeDropdown;

    [SerializeField, Tooltip("Drag the dropdown menu for the game mode choice here")]
    private TMP_Dropdown _mapDropdown;

    [SerializeField, Tooltip("Drag the Slider for the Match Time here")]
    private Slider _matchTimeSlider;

    [SerializeField, Tooltip("Drag the text for the Match Time here")]
    private TMP_Text _matchTimeText;

    [SerializeField, Tooltip("Enable if you want to allow the random Map Option.")]
    private bool _allowRandomMap;

    private List<GameMode> _modesInDescription = new List<GameMode>();
    private int _currentMatchTime = 240;

    //The descriptions with the chosen game mode
    private List<MatchDescription> _currentPossibleMatches;

    //All MatchDescriptions
    private MatchDescription[] _matchDescriptions;
    private MatchDescription _currentMatchDescription;

    private const string UserVoteDropdownText = "- User Vote -";
    private const string RandomDropDownItem = "- random -";
    public int CurrentMatchTime => _currentMatchTime;

    private void Start() {
        _matchDescriptions = _matchDescriptionCollection._matchDescriptions;
        _modesInDescription = GetGameModesFromCollection();
        _matchTimeSlider.value = BalancingConfiguration.Singleton.MatchTimeInSeconds;

        FeedMatchUpDropdown();

        FeedGameModeDropDown();

        FeedMapDropdown();
    }

    private List<GameMode> GetGameModesFromCollection() {
        List<GameMode> modes = new List<GameMode>();
        foreach (MatchDescription match in _matchDescriptions) {
            if (!modes.Contains(match.GameMode))
                modes.Add(match.GameMode);
        }

        return modes;
    }

    private void FeedGameModeDropDown() {
        var newDropdown = new List<TMP_Dropdown.OptionData>();
        _modesInDescription.ForEach(mode => newDropdown.Add(new TMP_Dropdown.OptionData(mode.ToString())));
        newDropdown.Add(new TMP_Dropdown.OptionData(UserVoteDropdownText));
        _gameModeDropdown.options = newDropdown;
    }

    private GameMode GetCurrentGameMode() {
        return _modesInDescription[_gameModeDropdown.value];
    }

    /// <summary>
    /// Call this to init the Dropdown menu for the game modes
    /// </summary>
    private void FeedMapDropdown() {
        var newDropdown = new List<TMP_Dropdown.OptionData>();
        _currentPossibleMatches = _matchDescriptions.Where(mode => mode.GameMode == GetCurrentGameMode()).ToList();

        // Add random drop down data when Random Maps is true
        if (_allowRandomMap)
            newDropdown.Add(new TMP_Dropdown.OptionData(RandomDropDownItem));

        _currentPossibleMatches.ForEach(map => newDropdown.Add(new TMP_Dropdown.OptionData(map.MapName)));
        _mapDropdown.options = newDropdown;
        _mapDropdown.value = 0;
        _currentMatchDescription = _allowRandomMap ? null : _currentPossibleMatches[_mapDropdown.value];
    }

    /// <summary>
    /// Call this to init the Dropdown menu for the match ups
    /// </summary>
    private void FeedMatchUpDropdown() {
        var matchUps = new List<TMP_Dropdown.OptionData>();
        if (_eligibleMatchUps.Length > 0) {
            foreach (MatchUp matchUp in _eligibleMatchUps) {
                matchUps.Add(new TMP_Dropdown.OptionData(matchUp.Name));
            }

            _matchUpDropdown.options = matchUps;
        }
        else {
            Debug.LogError(
                "No eligible matchUps registered in the Admin Controller. Did you forget to drag them in the inspector?");
        }
    }

    /// <summary>
    /// Get the current chosen matchUp from the matchUp Dropdown Menu
    /// </summary>
    /// <returns>the currently chosen MatchUp</returns>
    public MatchUp GetMatchUpFromDropdown() {
        MatchUp chosenMatchUp = _eligibleMatchUps[_matchUpDropdown.value];
        return chosenMatchUp;
    }

    /// <summary>
    /// Check if the currently selected map has enough spawn towers for every player.
    /// If a team has more players than the map has spawn towers, it's not startable
    /// </summary>
    /// <returns>True if the currently selected map has enough spawn towers. True, if 'random' selected</returns>
    public bool IsSelectedMapStartable() {
        string selectedOptionText = _mapDropdown.options[_mapDropdown.value].text;
        return IsMapSelectable(selectedOptionText);
    }

    /// <summary>
    /// Check if the map has enough spawn tower for every player.
    /// If a team has more players than the map has spawn towers, it's not selectable
    /// </summary>
    /// <param name="mapName">The name of the map</param>
    /// <returns>True if the map has enough spawn towers</returns>
    public bool IsMapSelectable(string mapName) {
        if (mapName == RandomDropDownItem) return true;

        MatchDescription match = _matchDescriptions.FirstOrDefault(desc => desc.MapName == mapName);
        if (match == null) return false;

        return TeamManager.Singleton.TeamIce.GetPlayerCount() <= match.MatchUp.MaxPlayers / 2
               && TeamManager.Singleton.TeamFire.GetPlayerCount() <= match.MatchUp.MaxPlayers / 2;
    }

    /// <summary>
    /// Snaps the Slider to the correct values and changes the text field
    /// </summary>
    public void SnapMatchTimeSlider(float currentValue) {
        int correctedValue = Mathf.FloorToInt(currentValue / _matchTimeTimeSteps) * _matchTimeTimeSteps;
        _matchTimeSlider.value = correctedValue;

        int minutes = Mathf.FloorToInt(correctedValue / 60f);
        int seconds = correctedValue - minutes * 60;

        string matchTimeString = $"{minutes,2}:{seconds,2:D2}";
        _matchTimeText.text = matchTimeString;

        _currentMatchTime = correctedValue;
    }

    [UsedImplicitly]
    public void OnGameModeChanged(int value) {
        if(value < _modesInDescription.Count)
            FeedMapDropdown();
        //Toggling UserVote for each selection
        AdminController.Instance.UserVote = value >= _modesInDescription.Count
                                            && _gameModeDropdown.captionText.text.Equals(UserVoteDropdownText);
        AdminController.Instance.CurrentGameMode = (GameMode) value;
    }

    [UsedImplicitly]
    public void OnMapValueChanged(int value) {
        _currentMatchDescription = _currentPossibleMatches
            .FirstOrDefault(md => md.MapName == _mapDropdown.options[value].text);
    }

    public void SetCurrentMatchDescription(MatchDescription matchDescription) {
        if (matchDescription != null)
            _currentMatchDescription = matchDescription;
    }
    public MatchDescription GetCurrentMatchDescription() {
        return _currentMatchDescription != null
            ? _currentMatchDescription
            : GameManager.Instance.ChooseRandomMatch(desc => desc.GameMode == GetCurrentGameMode());
    }

    public GameMode GetCurrentSelectedGameMode() {
        switch ((GameMode) _gameModeDropdown.value) {
            case GameMode.Elimination:
                return GameMode.Elimination;
            case GameMode.DeathMatch:
                return GameMode.DeathMatch;
            case GameMode.GoalTower:
                return GameMode.GoalTower;
            default:
                return GameMode.UserVote;
        }
    }

    public bool IsCurrentSelectedGameModeUserVote() {
        return _gameModeDropdown.options[_gameModeDropdown.value].text == UserVoteDropdownText;
    }
}