using Photon.Pun;

public partial class GameManager {
    /// <summary>
    /// State to handle ShowRoundStats timeout (show round stats meanwhile).
    /// </summary>
    private class RoundFinishedState : GameManagerMatchState {
        /// <summary>
        /// Identifies the current state (returns GameManagerStateMachine.States.RoundFinished).
        /// </summary>
        public override GameManagerStateMachine.State StateIdentifier => GameManagerStateMachine.State.RoundFinished;

        #region synced Values

        private int _winningTeamID;

        #endregion

        #region called by StateMachine

        /// <summary>
        /// Init state.
        /// </summary>
        public override void EnterState() {
            base.EnterState();

            if (!PhotonNetwork.IsMasterClient) {
                GameManagerInstance.FinishRoundOnClients();
            }
        }

        /// <summary>
        /// Update state (wait for showRoundStats timeout).
        /// </summary>
        public override void UpdateState() {
            if (!PhotonNetwork.IsMasterClient) return;

            if (GameManagerInstance.MatchTimer.GetRemainingMatchTimeInSeconds() <= 0) {
                StateMachine.ChangeState(GameManagerStateMachine.State.MatchFinished);
            }

            // if timeout finished -> switch to Countdown state to start new round
            if (HelperFunctions.GetTimeDifferenceInSecFromPhotonServerTimestamps(
                    EnteredTimestamp, PhotonNetwork.ServerTimestamp) >= GameManagerInstance.ShowRoundStatsTimeoutInSec)
                StateMachine.ChangeState(GameManagerStateMachine.State.Countdown);
        }

        #endregion

        #region Serialize

        public override bool Serialize(BitSerializer stream) {
            bool success = base.Serialize(stream);
            success = success && stream.Serialize(ref _winningTeamID,
                          BitCompressionConstants.MinTeamID,
                          BitCompressionConstants.MaxTeamID);
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
            s += "EnteredTimestamp: " + EnteredTimestamp;
            return s;
        }

        #endregion
    }
}