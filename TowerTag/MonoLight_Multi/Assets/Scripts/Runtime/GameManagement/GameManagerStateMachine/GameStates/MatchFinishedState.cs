using Photon.Pun;

public partial class GameManager {
    /// <summary>
    /// State to handle ShowMatchStats timeout (show match stats meanwhile).
    /// </summary>
    private class MatchFinishedState : GameManagerMatchState {
        /// <summary>
        /// Identifies the current state (returns GameManagerStateMachine.States.MatchFinished).
        /// </summary>
        public override GameManagerStateMachine.State StateIdentifier => GameManagerStateMachine.State.MatchFinished;

        #region synced Values

        #endregion

        #region called by StateMachine

        /// <summary>
        /// Init state.
        /// </summary>
        public override void EnterState() {
            base.EnterState();
            GameManagerInstance.FinishMatch();
        }

        /// <summary>
        /// Update state (wait for showMatchStats timeout).
        /// </summary>
        public override void UpdateState() {
            if (PhotonNetwork.IsMasterClient) {
                if (HelperFunctions.GetTimeDifferenceInSecFromPhotonServerTimestamps(EnteredTimestamp,
                        PhotonNetwork.ServerTimestamp) >= GameManagerInstance.ShowMatchStatsTimeoutInSec) {
                    StateMachine.ChangeState(GameManagerStateMachine.State.Commendations);
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
            s += "EnteredTimestamp: " + EnteredTimestamp;
            return s;
        }

        #endregion
    }
}