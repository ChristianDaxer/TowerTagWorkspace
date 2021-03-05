using Network;
using Photon.Pun;
using TowerTag;

public partial class GameManager {
    /// <summary>
    /// State to handle Pause.
    /// </summary>
    private class PauseState : GameManagerMatchState {
        /// <summary>
        /// Identifies the current state (returns GameManagerStateMachine.States.Paused).
        /// </summary>
        public override GameManagerStateMachine.State StateIdentifier => GameManagerStateMachine.State.Paused;

        /// <summary>
        /// easy access to GameManagers MatchTimer - >just for convenience
        /// </summary>
        private MatchTimer Timer => GameManagerInstance.MatchTimer;

        #region synced Values

        /// <summary>
        /// Did we receive a Resume command yet (by sync or SetPause call)?
        /// </summary>
        private bool _resumeWasTriggered;

        /// <summary>
        /// StartTimestamp when the Match will resume (needed for MatchTimer).
        /// </summary>
        private int _matchStartTimestamp;

        /// <summary>
        /// New EndTimestamp when the Match will end (needed for MatchTimer).
        /// </summary>
        private int _matchEndsTimestamp;

        #endregion

        #region called by StateMachine

        /// <summary>
        /// Init state.
        /// </summary>
        public override void EnterState() {
            base.EnterState();
            // Send the Analytics Event if we are the Master Client
            if (PhotonNetwork.IsMasterClient) {
                AnalyticsController.PauseMatch(
                        ConfigurationManager.Configuration.Room,
                        GameManagerInstance.CurrentMatch.GetRegisteredPlayerCount(),
                        GameManagerInstance.CurrentMatch.RoundsStarted,
                        BalancingConfiguration.Singleton.MatchTimeInSeconds,
                        GameManagerInstance.CurrentMatch.Scene
                    );
            }

            // reset flag
            _resumeWasTriggered = false;

            // InitVoiceChat for Pause
            VoiceChatPlayer voicePlayer = GameManagerInstance.VoiceChatPlayer;
            if (voicePlayer != null && voicePlayer.IsInitialized)
                voicePlayer.Pause(true);

            // pause the local Timer
            Timer.PauseTimer();

            // trigger local "pause event"
            GameManagerInstance.OnPauseReceived(true);
        }

        /// <summary>
        /// Update state (wait until we receive a resume event and the MatchTimer finished the countdown).
        /// </summary>
        public override void UpdateState() {
            if (_resumeWasTriggered && Timer.IsMatchTimer)
                StateMachine.ChangeState(GameManagerStateMachine.State.PlayMatch);
        }

        /// <summary>
        /// Cleanup: send local event that we leave Pause state.
        /// </summary>
        public override void ExitState() {
            base.ExitState();
            // Send the Analytics Event if we are the Master Client
            if (PhotonNetwork.IsMasterClient) {
                AnalyticsController.ResumeMatch(
                        ConfigurationManager.Configuration.Room,
                        GameManagerInstance.CurrentMatch.GetRegisteredPlayerCount(),
                        GameManagerInstance.CurrentMatch.RoundsStarted,
                        BalancingConfiguration.Singleton.MatchTimeInSeconds,
                        GameManagerInstance.CurrentMatch.Scene
                    );
            }

            // trigger local "resume" event
            GameManagerInstance.OnPauseReceived(false);
        }

        #endregion

        #region Serialize

        public override bool Serialize(BitSerializer stream) {
            bool success = base.Serialize(stream);

            success = success && stream.SerializeUncompressed(ref _matchStartTimestamp);
            success = success && stream.SerializeUncompressed(ref _matchEndsTimestamp);
            success = success && stream.Serialize(ref _resumeWasTriggered);

            if (stream.IsReading && _resumeWasTriggered) {
                ResumeWasTriggered();
            }

            return success;
        }

        #endregion

        #region Pause

        /// <summary>
        /// Handle SetPause(false) called on GameManager (Pause calls (SetPause(true) will be ignored).
        /// Please check GameManager.MatchTimer.IsResumingAllowed before calling SetPause because it will be ignored if timer.IsResumingAllowed is false.
        /// This function should only get called on Master client (is ignored on remote clients).
        /// </summary>
        /// <param name="pause">Has to be false to resume the Match (call will be ignored otherwise).</param>
        public override void SetPauseMatch(bool pause) {
            if (!PhotonNetwork.IsMasterClient) {
                Debug.LogWarning("Tried to pause match as client. Only the master client can pause.");
                return;
            }

            if (Timer.BlockPauseFunction) {
                Debug.LogWarning("Pause is currently blocked.");
                return;
            }

            if (!pause && !_resumeWasTriggered) {
                if (Timer.IsResumingAllowed) {
                    // timespan till end of Match (before paused)
                    float remainingTimeInSeconds = Timer.GetRemainingMatchTimeInSeconds();

                    // offset we have to apply to compensate network latency so the timestamp arrives at remote Players before the timestamp is reached.
                    float offsetToMatchStart = GameManagerInstance.ResumeFromPauseCountdownTimeInSec +
                                               GameManagerInstance.CountdownDelay;

                    // calculate new match- start & end times
                    _matchStartTimestamp =
                        HelperFunctions.AddTimeInSecondsToPhotonServerTimestamp(PhotonNetwork.ServerTimestamp,
                            offsetToMatchStart);
                    _matchEndsTimestamp =
                        HelperFunctions.AddTimeInSecondsToPhotonServerTimestamp(_matchStartTimestamp,
                            remainingTimeInSeconds);

                    // trigger resume locally
                    ResumeWasTriggered();

                    // trigger sync
                    StateMachine.IsDirty = true;
                }
            }
        }

        /// <summary>
        /// Handle resume locally (on Master called by SetPause call, on clients called by sync).
        /// </summary>
        private void ResumeWasTriggered() {
            // use this flag to ensure that ResumeWasTriggered() is only called on clients when we received an Resume call on Master
            _resumeWasTriggered = true;

            // Init voice chat for Match
            VoiceChatPlayer voicePlayer = GameManagerInstance.VoiceChatPlayer;
            if (voicePlayer != null && voicePlayer.IsInitialized)
                voicePlayer.Pause(false);

            // trigger Resume on matchTimer so it starts waitForCountdown/countdown
            Timer.ResumeTimerAt(_matchStartTimestamp, _matchEndsTimestamp,
                GameManagerInstance.ResumeFromPauseCountdownTimeInSec);
            PlayerManager.Instance.GetParticipatingPlayers(out var players, out var count);
            for (int i = 0; i < count; i++)
                players[i].PlayerNetworkEventHandler.SendTimerActivation(_matchStartTimestamp,
                    Match.CountdownType.ResumeMatch);
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
            s += "MatchStartTimestamp: " + _matchStartTimestamp + "\n";
            s += "MatchEndsTimestamp: " + _matchEndsTimestamp + "\n";
            s += "ResumeWasTriggered: " + _resumeWasTriggered + "\n";
            return s;
        }

        #endregion
    }
}