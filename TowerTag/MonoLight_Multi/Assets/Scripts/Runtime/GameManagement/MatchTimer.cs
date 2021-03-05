using System;
using BehaviorDesigner.Runtime.Tasks;
using Photon.Pun;
using UnityEngine;
using Action = System.Action;


/// <summary>
/// Class to make central MatchTimer available for all classes who need the Match/Countdown-Timer.
/// </summary>
public class MatchTimer : MonoBehaviour {
    /// <summary>
    /// TimerState: States the MatchTimer can be in.
    /// </summary>
    public enum TimerState {
        /// We are not in a Match (before MatchStartsAt and after StopMatch is called).
        Undefined,

        /// Match is running.
        MatchTimer,

        /// Countdown after MatchStartsAt or ResumeMatch was started (when WaitingForCountdown phase is over).
        CountdownTimer,

        /// When Match is paused (after PauseMatch until ResumeMatch is called).
        TimerIsPaused,

        /// Time before the countdown begins (MatchStartsAt or ResumeMatch was called and before the countdown is started).
        WaitForCountdown
    }

    /// <summary>
    /// Internal state of the Timer.
    /// </summary>
    public TimerState CurrentTimerState {
        get => _currentTimerState;
        private set {
            if (value != _currentTimerState) {
                _currentTimerState = value;
                try {
                    CurrentTimerStateChanged?.Invoke(this, _currentTimerState);
                }
                catch (ArgumentOutOfRangeException e) {
                    throw new ArgumentException(e.Message, e.ParamName);
                }
            }
        }
    }

    /// <summary>
    /// Type of countdown (was the countdown triggered by StartTimerAt or by ResumeMatchTimer)
    /// </summary>
    private enum CountdownType {
        /// No countdown was triggered or triggered countdown finished.
        Undefined,

        /// Current countdown was triggered by StartTimerAt.
        StartTimer,

        /// Current countdown was triggered by ResumeTimer.
        ResumeFromPause
    }

    /// <summary>
    /// Private member to hold type of current Countdown.
    /// </summary>
    private CountdownType _currentCountdownType;

    /// <summary>
    /// Is the Timer paused (currentTimerState == TimerState.TimerIsPaused)?
    /// </summary>
    public bool IsPaused => TimerState.TimerIsPaused == CurrentTimerState;

    /// <summary>
    /// Is the Timer resuming from Pause (Time between ResumeTimerAt() is called and the Timer goes to MatchTimerState)
    /// </summary>
    public bool IsResumingFromPause => _currentCountdownType == CountdownType.ResumeFromPause;

    /// <summary>
    /// Is Timer in CountdownTimer state?
    /// </summary>
    public bool IsCountdownTimer => TimerState.CountdownTimer == CurrentTimerState;

    /// <summary>
    /// Is Timer in CountdownTimer state?
    /// </summary>
    public bool IsMatchTimer => TimerState.MatchTimer == CurrentTimerState;

    /// <summary>
    /// Is Timer in WaitingForCountdown state?
    /// </summary>
    public bool IsWaitingForCountdown => TimerState.WaitForCountdown == CurrentTimerState;

    /// <summary>
    /// Should the Pause function get blocked?
    /// </summary>
    public bool BlockPauseFunction { get; }

    /// <summary>
    /// Is pausing the timer allowed right now? If this returns false  any call of PauseTimer() will be ignored (and an error message is printed to console).
    /// </summary>
    public bool IsPausingAllowed =>
        TimerState.TimerIsPaused != CurrentTimerState
        && TimerState.Undefined != CurrentTimerState
        && !BlockPauseFunction;

    /// <summary>
    /// Is resuming the timer allowed right now? If this returns false  any call of ResumeTimer() will be ignored (and an error message is printed to console).
    /// </summary>
    public bool IsResumingAllowed => TimerState.TimerIsPaused == CurrentTimerState;

    /// <summary>
    /// Timestamp the match should start or resume at, used only for countdown calculations
    /// </summary>
    [SerializeField, ReadOnly] private int _matchStartAtServerTimestamp;

    /// <summary>
    /// Timestamp the match should end at, used for matchTime calculations.
    /// </summary>
    [SerializeField, ReadOnly] private int _matchEndAtServerTimestamp;

    /// <summary>
    /// Timespan for countdown, used to start countdown (independent of network delay).
    /// </summary>
    public int CountdownTimeInSeconds { get; private set; }


    /// <summary>
    /// Remaining time the match should run.
    /// </summary>
    [SerializeField, ReadOnly] private float _remainingMatchTime;

    /// <summary>
    /// When the timer becomes MatchTimer for the first time it will be true
    /// </summary>
    private bool _started;

    private TimerState _currentTimerState;
    
    public bool IsLateJoiner { get; private set; }

    public MatchTimer(bool blockPauseFunction) {
        BlockPauseFunction = blockPauseFunction;
    }

    // 
    /// <summary>
    /// Timespan a match should run (Start..End) excluding pauses.
    /// </summary>
    public int MatchTimespanInSeconds { get; private set; }

    /// <summary>
    /// Timestamp the match should end at, used for matchTime calculations.
    /// </summary>
    public int MatchEndAtServerTimestamp => _matchEndAtServerTimestamp;

    /// <summary>
    /// Timestamp the match should start or resume at, used only for countdown calculations
    /// </summary>
    public int MatchStartAtServerTimestamp => _matchStartAtServerTimestamp;


    private void Start() {
        CurrentTimerState = TimerState.Undefined;
    }
    private void Update() {
        // update remaining match time if running (not paused or waiting)
        if (TimerState.TimerIsPaused != CurrentTimerState && _started &&
            _currentCountdownType != CountdownType.ResumeFromPause)
        {
            _remainingMatchTime = HelperFunctions.GetTimeDifferenceInSecFromPhotonServerTimestamps(
                PhotonNetwork.ServerTimestamp, MatchEndAtServerTimestamp);
        }


        // check if we have to change internal state and switch if needed
        if (TimerState.Undefined != CurrentTimerState && TimerState.TimerIsPaused != CurrentTimerState)
            SetCurrentState();
    }

    /// <summary>
    /// Convenience function to calculate Timer values dependent of the internal state.
    /// Use this in conjunction with isTimerValid or currentTimerState to check if the returned values are valid and what they mean.
    /// </summary>
    /// <returns>
    /// Timespan to the end of the match when in MatchTimerState (same as GetRemainingMatchTimeInSeconds()).
    /// Timespan to the start of the match when in CountdownTimerState (same as GetCountdownTime()).
    /// CountdownTime in seconds when in WaitForCountdownState (same as countdownTimeInSeconds).
    /// -1 when in another TimerState.
    /// </returns>
    public float GetCurrentTimerInSeconds() {
        switch (CurrentTimerState) {
            // while math is running return time until match ends
            // while countdown return countdownTime (time until math starts/resumes ...)
            case TimerState.MatchTimer:
                return GetRemainingMatchTimeInSeconds();
            // while we wait for the countdown to begin
            case TimerState.CountdownTimer:
                return GetCountdownTime();
            // while paused or state is not valid (we are not in MatchState) return default error value
            case TimerState.WaitForCountdown:
                return CountdownTimeInSeconds;
            default:
                return -1f;
        }
    }

    /// <summary>
    /// Timespan the match is running yet (excluding pause times).
    /// </summary>
    /// <returns>Timespan the match is running (excluding pause times) if the internal state is MatchTimer.  </returns>
    public float GetMatchTime() {
        return MatchTimespanInSeconds - _remainingMatchTime;
    }

    /// <summary>
    /// Calculates and returns the current countdown timespan (timespan from current timestamp to the start of the match (if set appropriate by StartTimerAt(..) or ResumeTimerAt(..))).
    /// If Timer is not in countdown state (TimerState.CountdownTimer != currentTimerState) it returns the countdown time set by last StartTimerAt(..) or ResumeTimerAt() call.
    /// </summary>
    /// <returns>If Timer is in CountdownTimer state (TimerState.CountdownTimer == currentTimerState) it returns Timespan from current timestamp to MatchEndsAt timestamp in seconds [countdownTime..0], otherwise the last set countdown.</returns>
    public int GetCountdownTime() {
        return (TimerState.CountdownTimer == CurrentTimerState)
            ? Mathf.CeilToInt(HelperFunctions.GetTimeDifferenceInSecFromPhotonServerTimestamps(PhotonNetwork.ServerTimestamp,
                MatchStartAtServerTimestamp))
            : CountdownTimeInSeconds;
    }

    /// <summary>
    /// Returns remaining time the match should run (excluding pauses).
    /// </summary>
    /// <returns>Returns remaining time the match should run (excluding pauses).</returns>
    public float GetRemainingMatchTimeInSeconds() {
        return _remainingMatchTime;
    }

    /// <summary>
    /// Calculate the timespan till the Match starts or resumes (dependent of matchStartsAt timestamp set by StartMatchAt() or ResumeMatchAt()).
    /// </summary>
    /// <returns>Timespan till the Match starts or resumes, is negative if matchStartsAt timestamp set by StartMatchAt() or ResumeMatchAt() is in the past.</returns>
    public float GetTimeTillMatchStarts() {
        return HelperFunctions.GetTimeDifferenceInSecFromPhotonServerTimestamps(PhotonNetwork.ServerTimestamp,
            MatchStartAtServerTimestamp);
    }

    /// <summary>
    /// Starts timer and sets timestamps needed for timer calculations.
    /// Please calculate appropriate timestamps beforehand:
    ///     - startTimestamp   = current timestamp + countdownTime + offsetToCompensateNWLatency
    ///     - endTimestamp     = startTimestamp + matchTime
    /// </summary>
    /// <param name="startTimestamp">Timestamp the timer switches to MatchTimer (when the match should start).</param>
    /// <param name="endTimestamp">Timestamp the timer switches to undefined state (when the match should end).</param>
    /// <param name="countdownTimeInSeconds">Time before StartTimestamp the Timer counts down to start of the match.</param>
    public void StartTimerAt(int startTimestamp, int endTimestamp, int countdownTimeInSeconds, bool lateJoin = false) {
        // set member
        _matchStartAtServerTimestamp = startTimestamp;
        _matchEndAtServerTimestamp = endTimestamp;
        CountdownTimeInSeconds = countdownTimeInSeconds;
        IsLateJoiner = lateJoin;
        // calculate timespans
        int now = PhotonNetwork.ServerTimestamp;
        bool alreadyStarted = HelperFunctions.GetTimeDifferenceInSecFromPhotonServerTimestamps(
                                  now, MatchStartAtServerTimestamp) < 0;
        _remainingMatchTime = HelperFunctions.GetTimeDifferenceInSecFromPhotonServerTimestamps(
            alreadyStarted ? now : MatchStartAtServerTimestamp, MatchEndAtServerTimestamp);

        MatchTimespanInSeconds = (int) _remainingMatchTime;
        _currentCountdownType = CountdownType.StartTimer;
        SetCurrentState();
    }

    /// <summary>
    /// Stops the timer and set internal state to undefined.
    /// </summary>
    public void StopTimer() {
        // set internal state
        CurrentTimerState = TimerState.Undefined;
        _currentCountdownType = CountdownType.Undefined;
        _remainingMatchTime = 0;
        _matchEndAtServerTimestamp = 0;
        _matchStartAtServerTimestamp = 0;
        IsLateJoiner = false;
        _started = false;
        // throw stopMatch event
        Stopped?.Invoke();
    }

    /// <summary>
    /// Pauses the timer and saves the current timestamp.
    /// If you want to resume the match and need the remaining match time call GetRemainingMatchTimeInSeconds() when timer is still paused (see ResumeTimer(..)).
    /// </summary>
    public void PauseTimer() {
        // check if it is valid to pause the timer

        if (!IsPausingAllowed) {
            Debug.LogWarning($"MatchTimer.PauseTimer: Can't pause timer in current TimerState: {CurrentTimerState}");
            return;
        }

        // set internal state
        CurrentTimerState = TimerState.TimerIsPaused;
        _currentCountdownType = CountdownType.Undefined;

        // throw pause event
        Paused?.Invoke();
    }

    /// <summary>
    /// Resume Timer from Pause.
    /// To calculate new EndTimestamp call GetTimeTillEndOfMatch() before resuming the Timer:
    ///     - startTimestamp    = current timestamp + countdownTime + offsetToCompensateNWLatency
    ///     - endTimestamp      = startTimestamp + GetRemainingMatchTimeInSeconds()
    /// </summary>
    /// <param name="resumeTimestamp">Timestamp the match should resume from pause.</param>
    /// <param name="newEndTimestamp">Timestamp the match should end.</param>
    /// <param name="countdownTimeInSeconds">Countdown to show before resuming from pause (if it is enough time till resumeTimestamp).</param>
    public void ResumeTimerAt(int resumeTimestamp, int newEndTimestamp, int countdownTimeInSeconds) {
        // check if it is valid to unpause the timer
        if (!IsResumingAllowed) {
            Debug.LogWarning("MatchTimer.PauseTimer: " +
                             $"Can't resume timer if we are not paused! current TimerState: {CurrentTimerState}");
            return;
        }

        // set new member
        _matchStartAtServerTimestamp = resumeTimestamp;
        _matchEndAtServerTimestamp = newEndTimestamp;
        CountdownTimeInSeconds = countdownTimeInSeconds;

        // set & calculate internal state
        _currentCountdownType = CountdownType.ResumeFromPause;
        SetCurrentState();
    }

    /// <summary>
    /// Sets the internal timer state dependent of how much time to the start of the countdown or match remains.
    /// <br/><br/>
    /// currentTime &lt; startMatchTimestamp - countdownTime                     : waitForCountdown <br/>
    /// startMatchTime - countdownTime &lt;= currentTime &lt; startMatchTime     : countdownTimer<br/>
    /// startMatchTime &lt;= currentTime &lt; endMatchTimestamp                    : matchTimer<br/>
    /// </summary>
    private void SetCurrentState() {
        int currentTimeStamp = PhotonNetwork.ServerTimestamp;

        // calculate timespan till start timestamp
        var timeSpanToStart = HelperFunctions.GetTimeDifferenceInSecFromPhotonServerTimestamps(
            currentTimeStamp, MatchStartAtServerTimestamp);

        // currentTime < (startMatchTime - countdownTime)                       : waitForCountdown
        if (timeSpanToStart > CountdownTimeInSeconds)
            CurrentTimerState = TimerState.WaitForCountdown;

        // (startMatchTime - countdownTime) <= currentTime < startMatchTime     : countdownTimer
        else if (timeSpanToStart > 0)
            CurrentTimerState = TimerState.CountdownTimer;

        // startMatchTime <= currentTime < endMatchTime                         : matchTimer
        else if (GetRemainingMatchTimeInSeconds() >= 0) {
            TimerState old = CurrentTimerState;
            CurrentTimerState = TimerState.MatchTimer;

            // we enter MatchTime state throw Start/Resume event
            if (old != TimerState.MatchTimer) {
                TriggerMatchTimerStateEvent();
            }
        }
        // else                                                                 : undefined
        else {
            // set undefined state & trigger stop match event
            if (CurrentTimerState == TimerState.MatchTimer) {
                StopTimer();
            }
        }
    }

    /// <summary>
    /// Triggers startTimer or ResumeTimer event dependent of StartTimerAt or ResumeTimerAt was called before.
    /// </summary>
    private void TriggerMatchTimerStateEvent() {
        // if we came from StartTimer countdown
        if (_currentCountdownType == CountdownType.StartTimer) {
            // reset value before event (if someone asks for our state)
            if ((int) GetRemainingMatchTimeInSeconds() == MatchTimespanInSeconds)
                _started = true;
            _currentCountdownType = CountdownType.Undefined;

            // throw StartTimer event
            Started?.Invoke();
        }

        // if we came from ResumeTimer countdown
        else if (_currentCountdownType == CountdownType.ResumeFromPause) {
            // reset value before event (if someone asks for our state)
            _currentCountdownType = CountdownType.Undefined;

            // throw Resume event
            Resumed?.Invoke();
        }
        else {
            Debug.LogError("Can't Trigger StartMatch or ResumeMatchEvent because currentCountdownType is undefined!");
        }
    }

    #region Events

    /// <summary>
    /// The State of the MatchTimer has changed
    /// </summary>
    public static event Action<MatchTimer, TimerState> CurrentTimerStateChanged;

    /// <summary>
    /// ResumeTimer event (when the countdown finished and the Timer goes to MatchTimer state  (after ResumeTimerAt() call)).
    /// </summary>
    public event Action Started;

    /// <summary>
    /// StopTimer event (when StopTimer() is called)).
    /// </summary>
    public event Action Stopped;

    /// <summary>
    /// PauseTimer event (when PauseTimer() is called)).
    /// </summary>
    public event Action Paused;

    /// <summary>
    /// ResumeTimer event (when the countdown finished and the Timer goes to MatchTimer state  (after ResumeTimerAt() call)).
    /// </summary>
    public event Action Resumed;

    #endregion
}