using System;
using System.Linq;
using System.Text.RegularExpressions;
using AI;
using JetBrains.Annotations;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class TrainingsUIController : MonoBehaviour {
    public delegate void TrainingsUIButtonPressed
        (TrainingsUIController sender, int playerCount, BotBrain.BotDifficulty botLevel);

    public static event TrainingsUIButtonPressed StartBotMatchButtonPressed;
    private const int MaxEnemyCount = 4;
    [SerializeField] private BotBrain.BotDifficulty[] _botLevels;

    [SerializeField] private Dropdown _botLevelDropdown;
    [SerializeField] private Dropdown _matchUpDropdown;

    // Start is called before the first frame update
    void Start()
    {
        FillMatchUpDropdown();
        FillBotLevelDropdown();
    }

    private void FillMatchUpDropdown() {
        _matchUpDropdown.options.Clear();
        Dropdown.OptionDataList optionList = new Dropdown.OptionDataList();
        for (int i = 1; i <= MaxEnemyCount; i++) {
            optionList.options.Add(new Dropdown.OptionData(i.ToString()));
        }
        _matchUpDropdown.AddOptions(optionList.options);
    }

    private void FillBotLevelDropdown() {
        _botLevelDropdown.options.Clear();
        Dropdown.OptionDataList optionList = new Dropdown.OptionDataList();
        _botLevels.ForEach(botLevel => optionList.options.Add(
            new Dropdown.OptionData(Regex.Replace(botLevel.ToString(), "(\\B[A-Z])", " $1"))));
        _botLevelDropdown.AddOptions(optionList.options);
    }

    [UsedImplicitly]
    public void OnBotMatchButtonPressed() {
        StartBotMatchButtonPressed?.Invoke(this, int.Parse(_matchUpDropdown.captionText.text),
            GetCurrentSelectedBotLevel());
        MessageQueue.Singleton.AddVolatileMessage("Starting Bot Match...",
            "Bot Match",
            null,
            null,
            null,
            5);
    }
    
    [UsedImplicitly]
    public void OnTutorialButtonPressed() {
        GameManager.Instance.StartTutorial(true);
    }

    /// <summary>
    /// Returns the current selected bot difficulty of the drop down
    /// </summary>
    private BotBrain.BotDifficulty GetCurrentSelectedBotLevel() {
        return _botLevels.FirstOrDefault(level => level.ToString().Contains(Regex.Replace(_botLevelDropdown.captionText.text, @"\s+", "")));
    }
}