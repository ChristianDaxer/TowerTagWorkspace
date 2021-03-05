using GameManagement;
using UnityEngine;

[RequireComponent(typeof(GameManagerNetworkEventHandler))]
public class GameManagerPhotonView : TTSingleton<GameManagerPhotonView> {
    public GameManagerNetworkEventHandler NetworkEventHandler { get; private set; }
    protected override void Init() {
        if (GameInitialization.GetInstance(out var gameInitialization))
            gameInitialization.OnPhotonSceneViewInstantiated(gameObject);
        NetworkEventHandler = GetComponent<GameManagerNetworkEventHandler>();
    }
}