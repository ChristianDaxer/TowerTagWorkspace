using Photon.Pun;
using TowerTag;
using UnityEngine.SceneManagement;

public partial class GameManager {
    private class OffboardingState : GameManagerState {
        public override GameManagerStateMachine.State StateIdentifier => GameManagerStateMachine.State.Offboarding;

        private const string OffboardingSceneName = "Offboarding";


        /// <summary>
        /// Init state.
        /// </summary>
        public override void EnterState() {
            base.EnterState();

            // deactivate Gun and make Player immortal in commendations scene
            GameManagerInstance.ActivateAllPlayersOnMaster(false, true);
            LoadOffboardingScene();
        }

        private void LoadOffboardingScene() {
            Debug.Log("Loading offboarding scene");
            StateMachine.BlockIncomingSerialization(true);
            SceneManager.sceneLoaded += OnSceneLoaded;
            TTSceneManager.Instance.LoadOffboardingScene();
        }

        private void OnSceneLoaded(Scene newScene, LoadSceneMode mode) {

            if (!newScene.name.Equals(OffboardingSceneName)) {
                Debug.LogWarning($"Loaded scene {newScene.name} while waiting for scene {OffboardingSceneName}");
                return;
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
            PlayerManager.Instance.GetAllConnectedPlayers(out var players, out var count);
            for (int i = 0; i < count; i++)
            {
                players[i].InitPlayerFromPlayerProperties();
                if (PhotonNetwork.IsMasterClient)
                    TeleportHelper.TeleportPlayerOnSpawnPillar(players[i], TeleportHelper.TeleportDurationType.Immediate);
            }

            StateMachine.BlockIncomingSerialization(false);
        }
    }
}