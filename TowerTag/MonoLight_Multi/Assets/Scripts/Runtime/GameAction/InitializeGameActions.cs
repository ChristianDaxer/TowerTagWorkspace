using UnityEngine;

public class InitializeGameActions : MonoBehaviour {
    [SerializeField] private GameActionManager _gameActionManager;

    private void Start() {
        _gameActionManager.Init();
    }
}