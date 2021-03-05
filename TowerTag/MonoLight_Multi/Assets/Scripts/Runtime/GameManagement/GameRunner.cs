using UnityEngine;

namespace GameManagement {
    /// <summary>
    /// MonoBehaviour for running the game loop of the active <see cref="IGameManager"/>.
    /// </summary>
    public class GameRunner : MonoBehaviour {
        //setter visible for testing
        public IGameManager ActiveGameManager { get; set; } = GameManager.Instance;

        private void LateUpdate() {
            ActiveGameManager.Tick();
        }

        private void OnApplicationQuit() {
            ActiveGameManager.OnApplicationQuit();
        }
    }
}