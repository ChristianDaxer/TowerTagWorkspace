using TowerTag;
using UnityEngine;


/// <summary>
/// Base class to visualize (and react) <see cref="PillarWall"/> events (OnDamageChanged, OnReachedMaxDamage, OnForceWasApplied).
/// This is just an abstract base class to ease registration of event handler functions (in Awake).
/// Real Views should be implemented in derived classes (overriding the abstract methods in this class).
/// </summary>
[RequireComponent(typeof(PillarWall))]
public abstract class PillarWallView : MonoBehaviour {
    protected PillarWall PillarWall { get; private set; }
    private Pillar _pillar;



    /// <summary>
    /// Fetch WallDamageHandler_Base and register event Handler.
    /// </summary>
    protected virtual void Awake() {
        _pillar = GetComponentInParent<Pillar>();
        PillarWall = GetComponent<PillarWall>();

        RegisterEvents();
        OnOwningTeamChanged(_pillar, TeamID.Neutral, _pillar.OwningTeamID, new IPlayer[0]);
    }

    /// <summary>
    /// Cleanup: register event Handler.
    /// </summary>
    protected void OnDestroy() {
        UnregisterEvents();
    }

    /// <summary>
    /// Register event handler on WallDamageHandler_Base object.
    /// </summary>
    private void RegisterEvents() {
        if (PillarWall == null) {
            Debug.LogError("Cannot register pillar wall events: damage handler is null");
            return;
        }

        GameManager.Instance.MissionBriefingStarted += OnMissionBriefingStarted;
        PillarWall.WasReset += Reset;
        _pillar.OwningTeamChanged += OnOwningTeamChanged;
        PillarWall.DamageChanged += OnWallDamageChanged;
        PillarWall.ReachedMaxDamage += OnWallReachedMaxDamage;
        PillarWall.ForceWasApplied += OnForceWasApplied;
    }

    /// <summary>
    /// Unregister event handler on WallDamageHandler_Base object.
    /// </summary>
    private void UnregisterEvents() {
        if (PillarWall == null) {
            Debug.LogError("Cannot unregister pillar wall events: damage handler is null");
            return;
        }
        GameManager.Instance.MissionBriefingStarted -= OnMissionBriefingStarted;
        _pillar.OwningTeamChanged -= OnOwningTeamChanged;
        PillarWall.WasReset -= Reset;
        PillarWall.DamageChanged -= OnWallDamageChanged;
        PillarWall.ReachedMaxDamage -= OnWallReachedMaxDamage;
        PillarWall.ForceWasApplied -= OnForceWasApplied;
    }

    private void OnMissionBriefingStarted(MatchDescription obj, GameMode gameMode) {
        OnWallReachedMaxDamage();
    }

    #region Abstract Event Handler Functions to override in derived classes

    /// <summary>
    /// React on WallDamageHandler's OnWallDamageChanged event.
    /// </summary>
    /// <param name="oldDamage">The old damage of the wall before change.</param>
    /// <param name="newDamage">The current damage of the wall (now).</param>
    protected abstract void OnWallDamageChanged(float oldDamage, float newDamage);

    /// <summary>
    /// React on WallDamageHandler's OnReachedMaxDamage event.
    /// -> Start fallingDown Animation
    /// </summary>
    protected abstract void OnWallReachedMaxDamage();

    /// <summary>
    /// Resets the visualization to the current damage
    /// </summary>
    public abstract void Reset();

    /// <summary>
    /// reacts to the OwningTeamChanged event from the Pillar
    /// </summary>
    protected abstract void OnOwningTeamChanged(Claimable claimable, TeamID oldTeam, TeamID newTeam, IPlayer[] newOwner);

    /// <summary>
    /// React on WallDamageHandler's OnForceWasApplied event.
    /// -> Add force to RigidBody so it wobbles a bit.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="force"></param>
    protected abstract void OnForceWasApplied(Vector3 position, Vector3 force);

    #endregion
}