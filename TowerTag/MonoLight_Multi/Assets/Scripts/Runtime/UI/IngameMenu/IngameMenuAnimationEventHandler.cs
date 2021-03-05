using JetBrains.Annotations;
using UnityEngine;

public class IngameMenuAnimationEventHandler : MonoBehaviour {
    public delegate void IngameMenuAnimationAction(object sender, bool animationFinished);

    public event IngameMenuAnimationAction IngameMenuSpawn;
    public event IngameMenuAnimationAction IngameMenuDeSpawn;

    [UsedImplicitly]
    public void IngameMainMenuSpawnStarted() {
        IngameMenuSpawn?.Invoke(this, false);
    }

    [UsedImplicitly]
    public void IngameMainMenuSpawnFinished() {
        IngameMenuSpawn?.Invoke(this, true);
    }

    [UsedImplicitly]
    public void IngameMainMenuDeSpawnStarted() {
        IngameMenuDeSpawn?.Invoke(this, false);
    }

    [UsedImplicitly]
    public void IngameMainMenuDeSpawnFinished() {
        IngameMenuDeSpawn?.Invoke(this, true);
    }
}