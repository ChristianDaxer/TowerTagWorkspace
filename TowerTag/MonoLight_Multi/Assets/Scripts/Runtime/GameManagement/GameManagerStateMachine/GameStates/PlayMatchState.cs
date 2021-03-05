using Photon.Pun;
using TowerTag;

public partial class GameManager {
    /// <summary>
    /// State to connect Listener (MatchFinished, RoundFinished, StatsChanged) and play the Match.
    /// </summary>
    private class PlayMatchState : GameManagerMatchState {
        /// <summary>
        /// Identifies the current state (returns GameManagerStateMachine.States.PlayMatch).
        /// </summary>
        public override GameManagerStateMachine.State StateIdentifier => GameManagerStateMachine.State.PlayMatch;

        /// <summary>
        /// easy access to GameManagers MatchTimer - >just for convenience
        /// </summary>
        private MatchTimer Timer => GameManagerInstance.MatchTimer;

        #region called by StateMachine

        /// <summary>
        /// Init state (Register Listener to Match events  (MatchFinished, RoundFinished, StatsChanged))
        /// </summary>
        public override void EnterState() {
            base.EnterState();
            GameManagerInstance.ResumeMatch();
            GameManagerInstance.CurrentMatch.RoundFinished += OnRoundFinished;
        }

        /// <summary>
        /// Update state -> Update the Match.
        /// </summary>
        public override void UpdateState() {
            if (!PhotonNetwork.IsMasterClient || GameManagerInstance.CurrentMatch == null) return;
            if (Timer.GetRemainingMatchTimeInSeconds() <= 0) {
                StateMachine.ChangeState(GameManagerStateMachine.State.MatchFinished);
            }
        }

        public override void ExitState() {
            GameManagerInstance.CurrentMatch.RoundFinished -= OnRoundFinished;
        }

        private void OnRoundFinished(IMatch match, TeamID teamID) {
            if (PhotonNetwork.IsMasterClient)
                StateMachine.ChangeState(GameManagerStateMachine.State.RoundFinished);
        }

        #endregion

        #region Match EventHandler

        #endregion

        #region Pause

        /// <summary>
        /// Handle SetPause(true) called on GameManager (Resume calls (SetPause(false) will be ignored).
        /// Please check GameManager.MatchTimer.IsPausingAllowed before calling SetPause because it will be ignored if timer.IsPausingAllowed is false.
        /// This function should only get called on Master client (is ignored on remote clients).
        /// </summary>
        /// <param name="pause">Has to be true to pause the Match (call will be ignored otherwise).</param>
        public override void SetPauseMatch(bool pause) {
            if (!PhotonNetwork.IsMasterClient) {
                Debug.LogWarning("Tried to pause match as client. Only the master client can pause.");
                return;
            }

            if (Timer.BlockPauseFunction) {
                Debug.LogWarning("Pause is currently blocked.");
                return;
            }

            if (pause) {
                if (Timer.IsPausingAllowed) {
                    StateMachine.ChangeState(GameManagerStateMachine.State.Paused);
                }
            }
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
            s += "TimerState: " + Timer.CurrentTimerState + "\n";
            s += "Time: " + Timer.GetCurrentTimerInSeconds() + "\n";
            s += "MatchTimespan: " + Timer.MatchTimespanInSeconds + "\n";
            return s;
        }

        #endregion
    }
}