using TowerTag;
using UnityEngine;

public class ScoreBoardSoundsPlayer : TTSingleton<ScoreBoardSoundsPlayer> {
    // Audio
    [SerializeField] private bool _playAudio = true;
    [SerializeField] private AudioSource[] _sources;

    // countdowns
    [Header("Countdown Messages")]
    private string _countdown5SSoundName = "46_5s_Countdown";

    private string _countdown3SSoundName = "47_3s_Countdown";
    private bool _countdownAudioPlayed;

    // remainingMatchTime
    [Header("Remaining MatchTime Message")]
    private string _remainingMatchTimeReminderSoundName = "48_OneMinuteLeft";

    private bool _remainingMatchTimeReminderPlayed;

    [Header("Score Messages")]
    private string _team1ScoresSoundName = "45_TeamFireScores";

    private string _team2ScoresSoundName = "44_TeamIceScores";

    [Header("Win Messages")]
    private string _tieNoTeamWinsSoundName = "49_Tie";

    private string _team1WinByKillsSoundName = "41_orangeWinByKills";
    private string _team2WinByKillsSoundName = "42_blueWinByKills";

    [Header("Player died Messages")]
    private string _youDiedSoundName = "50_YouDied";

    private string _team1LostOnePlayer = "51_TeamFireLostOnePlayer";
    private string _team2LostOnePlayer = "52_TeamIceLostOnePlayer";

    [Header("Current Match Messages")]
    private string _introGoalPillarModeSoundName = "53_Intro_GoalMode";

    private string _introTeamDeathMatchModeSoundName = "54_IntroTDMMode";
    private string _intro2SoundName = "55_Intro_2_Match";
    private bool _matchIntroSoundsPlayed;

    [Header("Abort Match Voting")]
    private string _abortMatchVotePlusAnnouncerSoundName = "16_VoteMatchAbortPlusAnnouncer";
    private string _abortMatchVotePlingSoundName = "17_VoteMatchAbortPling";
    private bool _isFirstTimeAbortMatchVoteAnnouncing;


    public void InitForNewMatch() {
        _remainingMatchTimeReminderPlayed = false;
        _matchIntroSoundsPlayed = false;
        _countdownAudioPlayed = false;
        _isFirstTimeAbortMatchVoteAnnouncing = true;
    }

    public void InitCountdownTimer() {
        _countdownAudioPlayed = false;
    }

    // Trigger Audio
    public void PlayCountdownAudioMessage(int countdownTime) {
        if (!_playAudio || _countdownAudioPlayed)
            return;

        string countdownSoundName;
        switch (countdownTime) {
            case 3:
                countdownSoundName = _countdown3SSoundName;
                break;
            case 5:
                countdownSoundName = _countdown5SSoundName;
                break;
            default:
                countdownSoundName = null;
                break;
        }

        _countdownAudioPlayed = true;

        if (SoundDatabase.Instance == null) {
            Debug.LogError("Cannot play countdown audio message: sound database not found");
            return;
        }

        SoundDatabase.Instance.PlaySound(_sources, countdownSoundName);
    }

    public void PlayRemainingTimeReminderAudioMessage() {
        if (!_playAudio || _remainingMatchTimeReminderPlayed)
            return;

        _remainingMatchTimeReminderPlayed = true;

        if (SoundDatabase.Instance == null) {
            Debug.LogError("Cannot play remaining time reminder audio message: sound database not found");
            return;
        }

        SoundDatabase.Instance.PlaySound(_sources, _remainingMatchTimeReminderSoundName);
    }

    public void PlayWinSound(TeamID winningTeam) {
        if (!_playAudio)
            return;

        if (SoundDatabase.Instance == null) {
            Debug.LogError("Cannot play sound: SoundDatabase not found");
            return;
        }

        switch (winningTeam) {
            case TeamID.Neutral:
                SoundDatabase.Instance.PlayRandomSoundWithName(_sources, _tieNoTeamWinsSoundName);
                break;
            case TeamID.Fire:
                SoundDatabase.Instance.PlayRandomSoundWithName(_sources, _team1WinByKillsSoundName);
                break;
            case TeamID.Ice:
                SoundDatabase.Instance.PlayRandomSoundWithName(_sources, _team2WinByKillsSoundName);
                break;
        }
    }

    public void PlayScoreSound(TeamID scoringTeam) {
        if (!_playAudio)
            return;

        if (SoundDatabase.Instance == null) {
            Debug.LogError("Cannot play sound: SoundDatabase not found");
            return;
        }

        switch (scoringTeam) {
            case TeamID.Fire:
                SoundDatabase.Instance.PlayRandomSoundWithName(_sources, _team1ScoresSoundName);
                break;
            case TeamID.Ice:
                SoundDatabase.Instance.PlayRandomSoundWithName(_sources, _team2ScoresSoundName);
                break;
            default:
                Debug.LogWarning($"There is no score sound for {scoringTeam}");
                break;
        }
    }

    public void PlayPlayerDiedSound(int playerID, TeamID teamID, int killingPlayerID, byte killedByColliderType) {
        // 1) if we are the player who died -> play: you died, wait for next round
        // 2) if we are the player who killed -> play: you killed a good man or HEADSHOT (if it was a headshot)
        // 3) else if team is 0 -> play: team 0 lost one player
        // 4) else if team is 1 -> play: team 1 lost one player

        if (!_playAudio)
            return;

        string soundName = null;

        var player = PlayerManager.Instance.GetOwnPlayer();

        if (player == null) return;

        // 1) if we are the player who died -> play: you died, wait for next round
        if (player.PlayerID.Equals(playerID) && (GameManager.Instance.CurrentMatch.GameMode.Equals(GameMode.Elimination))) {
            soundName = _youDiedSoundName;
        }
        else if (player.PlayerID.Equals(killingPlayerID)) {
            // Hey you, if you are looking for the reward sounds here, peek into Reward.cs and
            // RewardController::TriggerDetectedAnimation() and you've got what you are looking for!
            return;
        }
        else {
            switch (teamID) {
                case TeamID.Fire:
                    soundName = _team1LostOnePlayer;
                    break;
                case TeamID.Ice:
                    soundName = _team2LostOnePlayer;
                    break;
            }
        }


        if (SoundDatabase.Instance == null) {
            Debug.LogError($"Cannot play sound {soundName}: SoundDatabase not found");
            return;
        }

        SoundDatabase.Instance.PlaySound(_sources, soundName);
    }


    // play introSound (for goal or team death match mode), wait and then play introSound_2
    private Coroutine _playIntro2Coroutine;

    public void PlayMatchIntroSound(IMatch match) {
        if (!_playAudio || _matchIntroSoundsPlayed)
            return;

        if (match == null) {
            Debug.LogError("Cannot player match intro sound: match is null.");
            return;
        }

        if (SoundDatabase.Instance == null) {
            Debug.LogError("Cannot play match intro sound: SoundDatabase not found");
            return;
        }

        // time between start of intro and intro_2
        const float waitingTimeBeforeStartIntro2SoundInSec = 5f;
        _matchIntroSoundsPlayed = true;
        switch (match) {
            case GoalPillarMatch _:
                SoundDatabase.Instance.PlaySound(_sources, _introGoalPillarModeSoundName);
                break;
            case EliminationMatch _:
                SoundDatabase.Instance.PlaySound(_sources, _introTeamDeathMatchModeSoundName);
                break;
        }

        if (_playIntro2Coroutine != null)
            StopCoroutine(_playIntro2Coroutine);

        _playIntro2Coroutine = StartCoroutine(HelperFunctions.Wait(waitingTimeBeforeStartIntro2SoundInSec,
            () => { SoundDatabase.Instance.PlaySound(_sources, _intro2SoundName); }));
    }

    protected override void Init() {
        InitForNewMatch();
    }

    protected void OnDestroy() {
        if (_playIntro2Coroutine != null) {
            StopCoroutine(_playIntro2Coroutine);
            _playIntro2Coroutine = null;
        }

        _sources = null;
    }

    public void PlayAbortMatchVotedSound()
    {
        if (_isFirstTimeAbortMatchVoteAnnouncing)
        {
            SoundDatabase.Instance.PlaySound(_sources, _abortMatchVotePlusAnnouncerSoundName);
            _isFirstTimeAbortMatchVoteAnnouncing = false;
        }
        else
        {
            SoundDatabase.Instance.PlaySound(_sources, _abortMatchVotePlingSoundName);
        }

    }
}