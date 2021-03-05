using Network;
using Photon.Pun;
using TowerTag;

public partial class GameManager {
    /// <summary>
    /// State to trigger EmergencyStop.
    /// </summary>
    private class EmergencyState : GameManagerState {
        /// <summary>
        /// Identifies the current state (returns GameManagerStateMachine.States.Emergency).
        /// </summary>
        public override GameManagerStateMachine.State StateIdentifier => GameManagerStateMachine.State.Emergency;

        #region called by StateMachine

        /// <summary>
        /// Init state.
        /// </summary>
        public override void EnterState() {
            base.EnterState();
            // Send the Analytics Event
            if (GameManagerInstance.CurrentMatch != null) // possible after late join
            {
                AnalyticsController.Emergency(
                    ConfigurationManager.Configuration.PreferredRegion,
                    ConfigurationManager.Configuration.PlayInLocalNetwork,
                    ConfigurationManager.Configuration.Room,
                    GameManagerInstance.CurrentMatch.GetRegisteredPlayerCount()
                );
            }

            // disable Player (disable Gun & Damage handling)
            if (PhotonNetwork.IsMasterClient) {
                PlayerManager.Instance.GetAllConnectedPlayers(out var players, out var count);
                for (int i = 0; i < count; i++)
                    players[i].PlayerStateHandler.SetPlayerStateOnMaster(PlayerState.DeadButNoLimbo);
            }

            // switch PhotonVoiceChannel so the Admin is talking and everybody is listening to him
            VoiceChatPlayer voicePlayer = GameManagerInstance.VoiceChatPlayer;
            if (voicePlayer != null && voicePlayer.IsInitialized)
                voicePlayer.SetEmergency(true);

            // stop the MatchTimer
            GameManagerInstance.MatchTimer.StopTimer();

            // trigger local EmergencyStop event
            GameManagerInstance.OnEmergencyReceived();
        }

        public override void LoadHub() {
            Debug.LogWarning("Cannot return to hub from emergency state");
        }

        #endregion
    }
}