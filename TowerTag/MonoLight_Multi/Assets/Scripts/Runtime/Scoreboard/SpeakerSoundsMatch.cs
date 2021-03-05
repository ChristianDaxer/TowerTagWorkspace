using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon.Pun;
using TowerTag;
using UnityEngine.Serialization;

/// <summary>
/// I "merged" old scripts for the sound management to this one.
/// </summary>
public class SpeakerSoundsMatch : MonoBehaviour {
    private IMatch _currentMatch;

    /// <summary>
    /// How much time before the countdown should the countdown audio start (so the countdown sound is in sync with shown countdown text)
    /// </summary>
    private float _offsetTimeToPlayAudio;

    /// <summary>
    /// How much time before the countdown should the 5s countdown audio start (so the countdown sound is in sync with shown countdown text)
    /// </summary>
    [FormerlySerializedAs("_offsetTimeFor5sCountdownAudio")]
    [SerializeField,
     Tooltip(
         "How much time before the countdown should the 5s countdown audio start(so the countdown sound is in sync with shown countdown text.")]
    private float _offsetTimeFor5SCountdownAudio = 1;

    /// <summary>
    /// How much time before the countdown should the 3s countdown audio start (so the countdown sound is in sync with shown countdown text)
    /// </summary>
    [FormerlySerializedAs("_offsetTimeFor3sCountdownAudio")]
    [SerializeField,
     Tooltip(
         "How much time before the countdown should the 3s countdown audio start(so the countdown sound is in sync with shown countdown text.")]
    private float _offsetTimeFor3SCountdownAudio;

    /// <summary>
    /// How much time the Countdown audio should start before the countdown, so the countdown sound is in sync with shown countdown.
    /// </summary>
    [Tooltip("How much time the Countdown audio should start before the countdown, so the countdown sound is in sync with shown countdown.")]
    private const int TimeAfterTheStartOfTheMatchToPlayIntroSounds = 3;

    /// <summary>
    /// Cached value to remember if we played the countdown audio file already?
    /// </summary>
    private bool _countdownSoundPlayed;

    // Audio
    /// <summary>
    /// Cached value to choose countdown audio (3 or 5 second countdown)
    /// </summary>
    private int _currentCountdownTime;

    /// <summary>
    /// Convenience property to grab the MatchTimer
    /// </summary>
    private MatchTimer _timer;

    private MatchTimer MatchTimer {
        get {
            if (_timer == null) {
                _timer = GameManager.Instance.MatchTimer;
            }

            return _timer;
        }
    }

    private static ScoreBoardSoundsPlayer scoreBoardSoundsPlayer;
    private static ScoreBoardSoundsPlayer ScoreBoardSoundsPlayerInstance
    {
        get
        {
            if (scoreBoardSoundsPlayer == null)
            {
                if (!ScoreBoardSoundsPlayer.GetInstance(out scoreBoardSoundsPlayer))
                    return null;
            }
            return scoreBoardSoundsPlayer;
        }
    }

    private void Awake() {
        Init();
    }

    private void Init() {
        GameManager gameManager = GameManager.Instance;
        InitMatchCallbacks(gameManager.CurrentMatch);
    }

    private void InitMatchCallbacks(IMatch match) {
        if (match == null)
            return;

        PlayerManager.Instance.GetParticipatingPlayers(out var players, out var count);
        // cleanup callbacks for old Match
        if (_currentMatch != null) {
            _currentMatch.StartingAt -= StartCountdown;
            _currentMatch.RoundStartingAt -= StartCountdown;
            _currentMatch.RoundFinished -= TriggerPlayScoredSound;
            _currentMatch.Finished -= TriggerWinningTeamSound;
            for (int i = 0; i < count; i++)
                players[i].PlayerHealth.PlayerDied -= PlayerDiedSound;
        }

        // init new Callbacks
        _currentMatch = match;
        _currentMatch.StartingAt += StartCountdown;
        _currentMatch.Finished += TriggerWinningTeamSound;
        _currentMatch.RoundStartingAt += StartCountdown;
        _currentMatch.RoundFinished += TriggerPlayScoredSound;

        for (int i = 0; i < count; i++)
            players[i].PlayerHealth.PlayerDied += PlayerDiedSound;

        ScoreBoardSoundsPlayerInstance.InitForNewMatch();
    }

    private void StartCountdown(IMatch match, int startTimeStamp) {
        // StartCountdownTimer
        _countdownSoundPlayed = false;

        // choose countdownAudio (5 or 3 second countdown)
        float time = Mathf.Min(MatchTimer.CountdownTimeInSeconds,
            HelperFunctions.GetTimeDifferenceInSecFromPhotonServerTimestamps(PhotonNetwork.ServerTimestamp,
                startTimeStamp));

        if (time >= 5) {
            _currentCountdownTime = 5;
            _offsetTimeToPlayAudio = _offsetTimeFor5SCountdownAudio;
        }
        else if (time >= 3) {
            _currentCountdownTime = 3;
            _offsetTimeToPlayAudio = _offsetTimeFor3SCountdownAudio;
        }

        ScoreBoardSoundsPlayerInstance.InitCountdownTimer();
    }

    private bool printedErrror;
    private void Update() {
        if (MatchTimer == null) {
            if (!printedErrror)
            {
                Debug.LogError("MatchTimer is null!");
                printedErrror = true;
            }
            return;
        }

        printedErrror = false;

        int time = Mathf.CeilToInt(MatchTimer.GetCurrentTimerInSeconds());

        // prepare for countdown -> show Loading & play Countdown sound (needs to start before countdown starts)
        if (MatchTimer.IsWaitingForCountdown || MatchTimer.IsCountdownTimer) {
            if (MatchTimer.GetTimeTillMatchStarts() <= _currentCountdownTime + _offsetTimeToPlayAudio &&
                !_countdownSoundPlayed) {
                _countdownSoundPlayed = true;
                ScoreBoardSoundsPlayerInstance.PlayCountdownAudioMessage(_currentCountdownTime);
            }
        }

        // show match time!
        else if (MatchTimer.IsMatchTimer) {
            if (_currentMatch != null &&
                (int) MatchTimer.GetMatchTime() == TimeAfterTheStartOfTheMatchToPlayIntroSounds) {
                ScoreBoardSoundsPlayerInstance.PlayMatchIntroSound(_currentMatch);
            }

            if (time == 60) {
                 ScoreBoardSoundsPlayerInstance.PlayRemainingTimeReminderAudioMessage();
            }
        }
    }

    private static void TriggerWinningTeamSound(IMatch match) {
        WinningTeamSound(match.Stats);
    }

    private static void WinningTeamSound(MatchStats stats) {
        if (stats == null) return;

        ScoreBoardSoundsPlayerInstance.PlayWinSound(stats.WinningTeamID);
    }

    private void PlayerDiedSound(PlayerHealth playerHealth, IPlayer other, byte colliderType) {
        if (ScoreBoardSoundsPlayerInstance == null) {
            Debug.LogError("ScoreBoardSoundPlayer is null, can't play player died sound!");
            return;
        }

        PlayerManager.Instance.GetParticipatingPlayers(out var players, out var count);

        int activeCount = 0;
        for (int i = 0; i < count; i++)
            if (players[i].PlayerHealth.IsActive)
                activeCount++;

        if (_currentMatch != null && activeCount >= 2) {
            MatchStats gameStats = _currentMatch.Stats;
            Dictionary<int, PlayerStats> playerStats = gameStats.GetPlayerStats();
            if (playerHealth != null && other != null && playerStats.ContainsKey(playerHealth.Player.PlayerID)) {
                ScoreBoardSoundsPlayerInstance.PlayPlayerDiedSound(
                    playerHealth.Player.PlayerID,
                    playerHealth.Player.TeamID,
                    other.PlayerID,
                    colliderType);
            }
        }
    }

    private static void TriggerPlayScoredSound(IMatch match, TeamID roundWinningTeamID) {
        PlayScoredSound(roundWinningTeamID);
    }

    private static void PlayScoredSound(TeamID winningTeamID) {
        ScoreBoardSoundsPlayerInstance.PlayScoreSound(winningTeamID);
    }

    private void OnDestroy() {
        if (_currentMatch != null) {
            _currentMatch.StartingAt -= StartCountdown;
            _currentMatch.Finished -= TriggerWinningTeamSound;
            _currentMatch.RoundStartingAt -= StartCountdown;
            _currentMatch.RoundFinished -= TriggerPlayScoredSound;

            PlayerManager.Instance.GetParticipatingPlayers(out var players, out var count);
            for (int i = 0; i < count; i++)
                players[i].PlayerHealth.PlayerDied -= PlayerDiedSound;

            _currentMatch = null;
        }
    }
}