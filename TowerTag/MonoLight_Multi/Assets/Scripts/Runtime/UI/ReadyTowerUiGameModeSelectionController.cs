using System;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using TowerTag;
using UnityEngine.UI;

public class ReadyTowerUiGameModeSelectionController : MonoBehaviour {
    private const string VotedText = "VOTED ENTERED";
    private const string VoteNextGame = "VOTE NEXT GAME";

    [SerializeField] private ReadyTowerUiGameModeButton[] _gameModeButtons;
    [SerializeField] private Text _voteInformation;
    private IPlayer _localPlayer;
    private bool _currentUserVoteToggle;

    public ReadyTowerUiGameModeButton[] GameModeButtons => _gameModeButtons;

    public event Action NewVoteEntered;

    private GameMode _previousModeVote = GameMode.UserVote;

    private void Awake() {
        _localPlayer = PlayerManager.Instance.GetOwnPlayer();
        if (_localPlayer == null) {
            Debug.LogError("No local Player found. Disabling mode selection");
            enabled = false;
        }
    }

    private void OnEnable() {
        if (GameModeButtons != null) {
            foreach (var button in GameModeButtons) {
                button.GameModeSelected += OnPlayerSelectedGameMode;
            }
        }

        if (_localPlayer.VoteGameMode != GameMode.UserVote) {
            TogglePlayerVoteStatus(_localPlayer.VoteGameMode, true);
        }

        PlayerManager.Instance.GetAllConnectedPlayers(out var players, out var count);
        for (int i = 0; i < count; i++)
        {
            players[i].GameModeVoted += OnGameModeVoted;
            OnGameModeVoted(players[i], (players[i].VoteGameMode, GameMode.UserVote));
        }

        PlayerManager.Instance.PlayerAdded += OnPlayerAdded;
        PlayerManager.Instance.PlayerRemoved += OnPlayerRemoved;
		GameManager.Instance.BasicCountdownStarted += OnBasicCountdownStarted;
        GameManager.Instance.BasicCountdownAborted += OnBasicCountdownAborted;
        GameManager.Instance.MissionBriefingStarted += OnMissionBriefingStarted;
    }
	
	private void OnBasicCountdownStarted(float countdownTime)
    {
        if (GameModeButtons != null) {
            foreach (var button in GameModeButtons) {
                button.ToggleGameModeButton(false, true);
            }
        }
    }

    private void OnBasicCountdownAborted()
    {
        if (GameModeButtons != null) {
            foreach (var button in GameModeButtons) {
                button.ToggleGameModeButton(true, true);
            }
        }
    }

    private void OnMissionBriefingStarted(MatchDescription arg1, GameMode arg2)
    {
        if (GameModeButtons != null) {
            foreach (var button in GameModeButtons) {
                button.ToggleGameModeButton(false, true);
            }
        }
    }

    private void OnDisable() {
        if (GameModeButtons != null) {
            foreach (var button in GameModeButtons) {
                button.GameModeSelected -= OnPlayerSelectedGameMode;
            }
        }

        _gameModeButtons.ForEach(mode => mode.RemoveAllVoteIcons());

        PlayerManager.Instance.GetAllConnectedPlayers(out var players, out var count);
        for (int i = 0; i < count; i++)
            players[i].GameModeVoted -= OnGameModeVoted;

        PlayerManager.Instance.PlayerAdded -= OnPlayerAdded;
        PlayerManager.Instance.PlayerRemoved -= OnPlayerRemoved;
    }

    private void OnPlayerSelectedGameMode(ReadyTowerUiGameModeButton sender, GameMode selectedMode)
    {
        _previousModeVote = _localPlayer.VoteGameMode;
        _localPlayer.VoteGameMode = selectedMode;
        
        foreach (var button in GameModeButtons) {
            if (button.Mode.Equals(selectedMode))
            {
                button.ToggleGameModeButton(false);
            }
            else
            {
                button.Init(true);
            }

            if (button.Mode.Equals(_previousModeVote))
            {
                button.RemoveVoteIcon();
            }
        }

        _voteInformation.text = VotedText;
    }

    public void ToggleGoalTowerGameMode(bool status) {
        // GT should be always playable in Home Version always
        if (TowerTagSettings.Home) return;

        ReadyTowerUiGameModeButton gtButton = GetButtonByGameMode(GameMode.GoalTower);

        if (_localPlayer.VoteGameMode != GameMode.UserVote) {
            if (_localPlayer.VoteGameMode == GameMode.GoalTower && !gtButton.IsGameModePlayable()) {
                _localPlayer.VoteGameMode = GameMode.UserVote;
                gtButton.RemoveAllVoteIcons();
                _gameModeButtons.ForEach(button => button.ToggleGameModeButton(button.IsGameModePlayable()));
            }

            gtButton.ToggleGameModeButton(false);
            gtButton.ToggleModeInfoOverlayText(false, "> NOT ENOUGH PLAYERS <");
        }
        else {
            if (GameManager.Instance.CurrentHomeMatchType != GameManager.HomeMatchType.Custom)
                _gameModeButtons.FirstOrDefault(mode => mode.Mode == GameMode.GoalTower)?.ToggleGameModeButton(status);
            _gameModeButtons.FirstOrDefault(mode => mode.Mode == GameMode.GoalTower)?
                .ToggleModeInfoOverlayText(!status, "> NOT ENOUGH PLAYERS <");
        }
    }

    private void OnPlayerAdded(IPlayer player) {
        player.GameModeVoted += OnGameModeVoted;
    }

    private void OnPlayerRemoved(IPlayer player) {
        player.GameModeVoted -= OnGameModeVoted;

        if (player.VoteGameMode != GameMode.UserVote) {
            ReadyTowerUiGameModeButton readyTowerUiGameModeButton =
                GameModeButtons.First(mode => mode.Mode == player.VoteGameMode);
            readyTowerUiGameModeButton.RemoveVoteIcon();
        }
    }

    public void OnUserVotingToggled(bool value) {
        if (_currentUserVoteToggle == value) return;
        _voteInformation.text = value ? VoteNextGame : "CURRENT MODE".ToUpperInvariant();
        _currentUserVoteToggle = value;
    }

    private void OnGameModeVoted(IPlayer sender, (GameMode currentGameMode, GameMode previousGameMode) gameModeData) {
        if (gameModeData.currentGameMode != GameMode.UserVote) {

            if (gameModeData.previousGameMode != GameMode.UserVote)
            {
                ReadyTowerUiGameModeButton previousReadyTowerUiGameModeButton =
                    GameModeButtons.First(mode => mode.Mode == gameModeData.previousGameMode);
                previousReadyTowerUiGameModeButton.RemoveVoteIcon();
            }
           
            ReadyTowerUiGameModeButton readyTowerUiGameModeButton =
                GameModeButtons.First(mode => mode.Mode == gameModeData.currentGameMode);
            readyTowerUiGameModeButton.AddVoteIcon();
            
            if (PhotonNetwork.IsMasterClient) {
                NewVoteEntered?.Invoke();
            }
        }
    }

    /// <summary>
    /// Toggles the visualization for the mode selection
    /// </summary>
    /// <param name="votedMode">If setActive is true, use the new voted mode, if false use the voted mode to unselect</param>
    /// <param name="setActive"></param>
    public void TogglePlayerVoteStatus(GameMode votedMode, bool setActive) {
        foreach (var button in GameModeButtons) {
            if (button.Mode.Equals(votedMode))
            {
                button.ToggleGameModeButton(false);
            }
            else
            {
                button.Init(true);
            }
        }

        _voteInformation.text = setActive ? VotedText : VoteNextGame;
    }

    public void SetGameModeByOperator(GameMode currentSelectedGameMode) {
        // Remove all vote Icons
        GameModeButtons.ForEach(button => button.RemoveAllVoteIcons());

        if (currentSelectedGameMode == GameMode.UserVote) {
            // Reset User Mode Votes, activate mode buttons
            foreach (var button in GameModeButtons) {
                button.ToggleGameModeButton(true);
            }

            GameModeButtons.ForEach(button => button.SelectButtonManually(false));
            GameModeButtons.ForEach(button => button.ToggleModeButtonImageColor(true));
        }
        else {
            // Deactivate mode buttons
            foreach (var button in GameModeButtons) {
                button.ToggleGameModeButton(false);
            }

            //GameModeButtons.ForEach(button => button.SelectButtonManually(false));

            ReadyTowerUiGameModeButton readyTowerUiGameModeButton =
                GameModeButtons.First(mode => mode.Mode == currentSelectedGameMode);

            _voteInformation.text = "CURRENT MODE";
            readyTowerUiGameModeButton.SelectButtonManually(true);
            if (readyTowerUiGameModeButton.Mode == GameMode.GoalTower && !TowerTagSettings.Home)
                readyTowerUiGameModeButton.ToggleModeInfoOverlayText(readyTowerUiGameModeButton.IsGameModePlayable());
        }
    }

    public ReadyTowerUiGameModeButton GetButtonByGameMode(GameMode mode) {
        return _gameModeButtons.FirstOrDefault(button => button.Mode == mode);
    }
}