using Photon.Pun;

public partial class GameManager {
    /// <summary>
    /// State to show countdown before the start of a match or a new round.
    /// </summary>
    private class CountdownState : GameManagerMatchState {
        /// <summary>
        /// Identifies the current state (returns GameManagerStateMachine.States.Countdown).
        /// </summary>
        public override GameManagerStateMachine.State StateIdentifier => GameManagerStateMachine.State.Countdown;

        /// <summary>
        /// easy access to GameManagers MatchTimer - >just for convenience
        /// </summary>
        private MatchTimer Timer => GameManagerInstance.MatchTimer;

        #region synced Values

        /// <summary>
        /// StartTime: Photon server timestamp to define when the next Match/Round will start at.
        /// </summary>
        private int _startAtTimestamp;

        /// <summary>
        /// StopTime: Photon server timestamp to define when the next Match/Round will end at (when the Timer does not get interrupted (by Pause or something else).
        /// </summary>
        private int _stopAtTimestamp;

        /// <summary>
        /// Countdown we have to set in MatchTimer in EnterState().
        /// </summary>
        private int _countDownTimeInSec;

        #endregion

        #region called by StateMachine

        /// <summary>
        /// Init state.
        /// </summary>
        public override void EnterState() {
            base.EnterState();

            if (PhotonNetwork.IsMasterClient)
                CalculateTimestampsOnMaster();

            // When the CountdownDuration is higher than the remaining time -> Match finished prematurely!
            if (GameManagerInstance.CurrentMatch.RoundsStarted > 0
                && _countDownTimeInSec >= Timer.GetRemainingMatchTimeInSeconds()
                && Timer.CurrentTimerState != MatchTimer.TimerState.Undefined) {
                StateMachine.ChangeState(GameManagerStateMachine.State.MatchFinished);
            }
            else {
                // todo: for late joiners, countdown must be 0. Need way to identifies late joiners here.
                int countdownSeconds = _countDownTimeInSec;
                GameManagerInstance.StartNewRoundAt(_startAtTimestamp, _stopAtTimestamp, countdownSeconds);
            }
        }

        /// <summary>
        /// Update state (wait until the Match timer finished the countdown).
        /// </summary>
        public override void UpdateState() {
            if (!PhotonNetwork.IsMasterClient)
                return;

            if (Timer.IsMatchTimer) {
                StateMachine.ChangeState(GameManagerStateMachine.State.PlayMatch);
            }
            else if (Timer.GetRemainingMatchTimeInSeconds() <= 0) {
                StateMachine.ChangeState(GameManagerStateMachine.State.MatchFinished);
            }
        }

        #endregion

        #region local Helper

        /// <summary>
        /// Calculate needed values in EnterState() on Master client and sync it to remote clients so they can use them in EnterState() to start a new Match or Round.
        /// This function should only get called on Master client (is ignored on remote clients).
        /// </summary>
        private void CalculateTimestampsOnMaster() {
            if (!PhotonNetwork.IsMasterClient)
                return;

            if (GameManagerInstance.CurrentMatch == null)
                return;

            // offset we have to apply to compensate network latency so the timestamp arrives at remote Players before the timestamp is reached.
            int offsetToMatchStart = GameManagerInstance.CountdownDelay;

            _countDownTimeInSec = GameManagerInstance.CurrentMatch.RoundsStarted == 0
                ? GameManagerInstance.MatchStartCountdownTimeInSec
                : GameManagerInstance.RoundStartCountdownTimeInSec;
            _startAtTimestamp = HelperFunctions.AddTimeInSecondsToPhotonServerTimestamp(PhotonNetwork.ServerTimestamp,
                offsetToMatchStart + _countDownTimeInSec);
            _stopAtTimestamp = GameManagerInstance.CurrentMatch.RoundsStarted == 0
                ? HelperFunctions.AddTimeInSecondsToPhotonServerTimestamp(_startAtTimestamp,
                    GameManagerInstance.CurrentMatch.MatchTimeInSeconds)
                : HelperFunctions.AddTimeInSecondsToPhotonServerTimestamp(PhotonNetwork.ServerTimestamp,
                    Timer.GetRemainingMatchTimeInSeconds());
        }

        #endregion

        #region Serialize

        /// <summary>
        /// Implement synchronisation of internal state here:
        /// - sync Start/EndTimestamps, isFirstRound, countdownTime (send from Master client to all other clients)
        /// </summary>
        /// <param name="stream">Stream to read from or write your data to.</param>
        /// <returns>True if succeeded read/write, false otherwise.</returns>
        public override bool Serialize(BitSerializer stream) {
            bool success = base.Serialize(stream);
            success = success && stream.SerializeUncompressed(ref _startAtTimestamp);
            success = success && stream.SerializeUncompressed(ref _stopAtTimestamp);
            success = success && stream.Serialize(ref _countDownTimeInSec,
                          BitCompressionConstants.MinCountdownTimeInSec,
                          BitCompressionConstants.MaxCountdownTimeInSec);
            return success;
        }

        #endregion

        #region Debug

        /// <summary>
        /// Print State internals to string (and to console if printToLog is true).
        /// </summary>
        /// <param name="printToLog">Should the returned string also printed to console/logFile?</param>
        /// <returns>String with internal members to view in DebugUI.</returns>
        public override string PrintState(bool printToLog) {
            string s = base.PrintState(printToLog) + "\n";
            s += "StartAtTimestamp: " + _startAtTimestamp + "\n";
            s += "StopAtTimestamp: " + _stopAtTimestamp + "\n";
            s += "CountDownTimeInSec: " + _countDownTimeInSec + "\n";
            s += "CountDownTimeInSec: " + Timer.GetCountdownTime() + "\n";
            return s;
        }

        #endregion
    }
}