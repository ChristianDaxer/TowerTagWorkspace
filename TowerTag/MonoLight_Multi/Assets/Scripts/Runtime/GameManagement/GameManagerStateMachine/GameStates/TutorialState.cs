using TowerTag;
using UnityEngine.SceneManagement;

partial class GameManager {
    private class TutorialState : GameManagerState {
        public override GameManagerStateMachine.State StateIdentifier => GameManagerStateMachine.State.Tutorial;
        private const string CommendationsSceneName = "Tutorial";


        public override void EnterState() {
            base.EnterState();
            LoadTutorialScene();
        }

        private void LoadTutorialScene() {
            StateMachine.BlockIncomingSerialization(true);
            SceneManager.sceneLoaded += OnSceneLoaded;
            TTSceneManager.Instance.LoadTutorialScene();
        }

        private void OnSceneLoaded(Scene newScene, LoadSceneMode mode) {
            IPlayer ownPlayer = PlayerManager.Instance.GetOwnPlayer();
            Pillar pillar = PillarManager.Instance.GetDefaultPillar();
            if (pillar != null)
                ownPlayer.SetTeam(pillar.OwningTeam.ID);
            else Debug.LogErrorFormat("No default pillar available."); 

            if (!newScene.name.Equals(CommendationsSceneName)) {
                Debug.LogWarning($"Loaded scene {newScene.name} while waiting for scene {CommendationsSceneName}");
                return;
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
            PlayerManager.Instance.GetAllConnectedPlayers(out var players, out var count);
            for (int i = 0; i < count; i++)
                players[i].InitPlayerFromPlayerProperties();

            StateMachine.BlockIncomingSerialization(false);
        }
    }
}