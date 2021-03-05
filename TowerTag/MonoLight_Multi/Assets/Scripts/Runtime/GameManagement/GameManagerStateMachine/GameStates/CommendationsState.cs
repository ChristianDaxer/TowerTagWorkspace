using Network;
using Photon.Pun;
using TowerTag;
using TowerTagSOES;
using UnityEngine.SceneManagement;

public partial class GameManager {
    /// <summary>
    /// State that corresponds to the commendations scene after a match has finished.
    /// <author>Ole Jürgensen (ole@vrnerds.de)</author>
    /// </summary>
    private class CommendationsState : GameManagerMatchState {
        public override GameManagerStateMachine.State StateIdentifier => GameManagerStateMachine.State.Commendations;

        private const string CommendationsSceneName = "Commendations";
        private int _showCommendationsPeriod = 30;

        #region called by StateMachine
        /// <summary>
        /// Init state.
        /// </summary>
        public override void EnterState() {
            base.EnterState();
            if (GameManagerInstance.CurrentMatch != null) // late join -> no match
            {
                AnalyticsController.LoadCommendations(
                    ConfigurationManager.Configuration.Room,
                    GameManagerInstance.CurrentMatch.GetRegisteredPlayerCount(),
                    CommendationsSceneName,
                    SharedControllerType.Singleton.Value.ToString()
                );
            }

            // deactivate Gun and make Player immortal in commendations scene
            GameManagerInstance.ActivateAllPlayersOnMaster(false, true);
            LoadCommendationsScene();
        }

        private void LoadCommendationsScene() {
            Debug.Log("Loading commendations scene");
            StateMachine.BlockIncomingSerialization(true);
            SceneManager.sceneLoaded += OnSceneLoaded;
            TTSceneManager.Instance.LoadScene(TTSceneManager.Instance.CommendationsScene);
        }

        private void OnSceneLoaded(Scene newScene, LoadSceneMode mode) {

            if (!newScene.name.Equals(CommendationsSceneName)) {
                Debug.LogWarning($"Loaded scene {newScene.name} while waiting for scene {CommendationsSceneName}");
                return;
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
            if(GameManagerInstance.VoiceChatPlayer.IsInitialized)
                GameManagerInstance.VoiceChatPlayer.ChangeConversationGroups(VoiceChatPlayer.ChatType.TalkToAll);
            PlayerManager.Instance.GetAllConnectedPlayers(out var players, out var count);
            for (int i = 0; i < count; i++)
                players[i].InitPlayerFromPlayerProperties();

            StateMachine.BlockIncomingSerialization(false);

            if (PhotonNetwork.IsMasterClient) {

                PlayerManager.Instance.GetSpectatingPlayers(out var spectatingPlayers, out var spectatingCount);

                for (int i = 0; i < spectatingCount; i++)
                {
                    Debug.LogError($"Loading Commendation, {spectatingPlayers[i]} is participating {spectatingPlayers[i].IsParticipating}");
                    TeleportHelper.TeleportPlayerToFreeSpectatorPillar(spectatingPlayers[i],
                        TeleportHelper.TeleportDurationType.Immediate);
                }
            }
        }

        /// <summary>
        /// Update state (wait for showMatchStats timeout).
        /// This is called from Update() and is executed every frame.
        /// </summary>
        public override void UpdateState() {
            if (!PhotonNetwork.IsMasterClient) {
                return;
            }

            // transition to hub scene after a period of time.
            if (HelperFunctions.GetTimeDifferenceInSecFromPhotonServerTimestamps(EnteredTimestamp,
                    PhotonNetwork.ServerTimestamp) >= _showCommendationsPeriod) {
                if(GameManagerInstance.TrainingVsAI)
                    ConnectionManager.Instance.LeaveRoom();
                else if(TTSceneManager.Instance.IsInCommendationsScene)
                    StateMachine.ChangeState(GameManagerStateMachine.State.Configure);
            }
        }

        /// <summary>
        /// Cleanup state.
        /// </summary>
        public override void ExitState() {
            base.ExitState();
            GameManagerInstance.CurrentMatch?.StopMatch();
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