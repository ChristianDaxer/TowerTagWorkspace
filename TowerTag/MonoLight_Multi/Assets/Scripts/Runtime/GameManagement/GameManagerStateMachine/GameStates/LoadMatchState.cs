using System.Linq;
using Photon.Pun;
using TowerTag;

public partial class GameManager {
    /// <summary>
    /// State to sync, load and Init the new Match.
    /// </summary>
    private class LoadMatchState : GameManagerMatchState {
        /// <summary>
        /// Identifies the current state (returns GameManagerStateMachine.States.LoadMatch).
        /// </summary>
        public override GameManagerStateMachine.State StateIdentifier => GameManagerStateMachine.State.LoadMatch;

        /// <summary>
        /// Sync Barrier to ensure all registered have received, loaded and initialized the new Match.
        /// </summary>
        private PlayerSyncBarrier _matchInitializedBarrier;

        #region called by StateMachine

        /// <summary>
        /// Init state.
        /// </summary>
        public override void EnterState() {
            base.EnterState();

            // create barrier to wait till all clients Initialized Match
            if (PhotonNetwork.IsMasterClient) {
                PlayerManager.Instance.GetAllHumanPlayers(out var players, out var count);
                _matchInitializedBarrier = new PlayerSyncBarrier(players.Take(count).ToArray(),
                    GameManagerInstance.CurrentMatch.MatchID, OnPlayersInSync);
                if(count == 0) OnPlayersInSync(_matchInitializedBarrier);
            }
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public override void ExitState() {
            base.ExitState();
            _matchInitializedBarrier = null;
        }

        #endregion

        #region PlayerSyncBarrier

        /// <summary>
        /// Pass the player's sync message to the SyncBarrier
        /// </summary>
        /// <param name="matchID">The match ID for which the player reportedly loaded the match scene</param>
        /// <param name="player">Player who send the message.</param>
        public override void OnReceivedPlayerSyncInfo(int matchID, IPlayer player) {
            _matchInitializedBarrier?.CheckPlayerMessage(matchID, player);
        }

        /// <summary>
        /// This callback is called when sync messages from all registered players were received on Master.
        /// </summary>
        /// <param name="sync"></param>
        private void OnPlayersInSync(PlayerSyncBarrier sync) {
            if (PhotonNetwork.IsMasterClient) {
                _matchInitializedBarrier = null;
                StateMachine.ChangeState(GameManagerStateMachine.State.Countdown);
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
            s += "SyncBarrier: " + (_matchInitializedBarrier == null
                     ? "-"
                     : "unsynced Players: " + _matchInitializedBarrier.UnSyncedPlayerCount) + "\n";

            s += "Match: ";
            if (GameManagerInstance.CurrentMatch != null) {
                s += " ID: " + GameManagerInstance.CurrentMatch.MatchID + " active:" +
                     GameManagerInstance.CurrentMatch.IsActive + "\n";

                s += "MatchStats: ";
                if (GameManagerInstance.CurrentMatch.Stats is MatchStats stats) {
                    int teamCount = stats.GetTeamStats() != null ? stats.GetTeamStats().Count : 0;
                    int playerCount = stats.GetPlayerStats() != null ? stats.GetPlayerStats().Count : 0;
                    s += " Team count: " + teamCount + " Player count: " + playerCount;
                }
            }

            return s;
        }

        #endregion
    }
}