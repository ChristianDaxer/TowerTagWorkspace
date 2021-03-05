using System;
using Photon.Pun;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;

/// <summary>
/// Class to handle damage of Walls. Does only handling and serializing of damage value.
/// Animations, physical behaviours and visualization should get implemented in other classes which can register for the local events.
/// </summary>
public class PillarWall : MonoBehaviour {
    #region Member

    /// <summary>
    /// Damage value of this wall (in range [0..1], 0 means no damage, 1 means full damage).
    /// </summary>
    public float Damage { get; private set; }

    private const float FloatTolerance = 0.001f;

    /// <summary>
    /// Gets multiplied with base damage made by Bullets to calculate damage per hit (collision with Bullet).
    /// </summary>
    [SerializeField, ReadOnly] private float _damagePerHitMultiplier = 0.1f;

    #endregion

    #region Events

    /// <summary>
    /// Event gets triggered when damage value has changed (Event(float oldDamage, float newDamage)).
    /// </summary>
    public event Action<float, float> DamageChanged;

    public event Action WasReset;

    /// <summary>
    /// Event gets triggered when AddForce was called (Event(Vector3 collisionPosition, Vector3 collisionForce)).
    /// </summary>
    public event Action<Vector3, Vector3> ForceWasApplied;

    /// <summary>
    /// Event gets triggered when max damage was reached.
    /// </summary>
    public event Action ReachedMaxDamage;

    [SerializeField] private PillarWallManager _pillarWallManager;

    [SerializeField] private string _id;

    public string ID => _id;

    #endregion

    #region Core (called on every client)

#if UNITY_EDITOR
    private void OnValidate() {
        if (gameObject.scene.buildIndex == -1) return;
        if (string.IsNullOrEmpty(_id)) {
            GenerateID();
        }

        if (_pillarWallManager == null) {
            Debug.LogWarning($"Cannot validate {this}: PillarWallManager not assigned");
            return;
        }

        PillarWall registeredWall = _pillarWallManager.GetPillarWall(_id);
        if (registeredWall == null) {
            _pillarWallManager.Register(this);
        }
        else if (registeredWall != this) {
            GenerateID();
            _pillarWallManager.Register(this);
        }
    }

    private void GenerateID() {
        _id = Guid.NewGuid().ToString();
        EditorUtility.SetDirty(this);
        if (!EditorApplication.isPlaying)
            EditorSceneManager.MarkSceneDirty(gameObject.scene);
    }
#endif

    private void OnEnable() {
        if (_pillarWallManager != null) _pillarWallManager.Register(this);
    }

    private void OnDisable() {
        if (_pillarWallManager != null) _pillarWallManager.Unregister(this);
    }

    /// <summary>
    /// Initializes WallDamageHandler and sets damage to 0.
    /// </summary>
    public void Init() {
        _damagePerHitMultiplier = BalancingConfiguration.Singleton.WallDamagePerHitMultiplier;
        OnDamageValueHasChanged(0);
    }

    /// <summary>
    /// Resets WallDamageHandler and sets damage to 0.
    /// </summary>
    public void Reset() {
        OnDamageValueHasChanged(0);
        WasReset?.Invoke();
    }


    /// <summary>
    /// Helper Func to call to change damage member value (triggers events & sets damage value).
    /// </summary>
    /// <param name="newDamage"></param>
    private void OnDamageValueHasChanged(float newDamage) {
        // trigger damageHasChanged event
        DamageChanged?.Invoke(Damage, newDamage);

        // trigger reachedMaxDamage event
        if (newDamage >= 1 && Math.Abs(Damage - newDamage) > FloatTolerance) {
            ReachedMaxDamage?.Invoke();
        }

        Damage = newDamage;
    }

    /// <summary>
    /// Function to apply collision force.
    /// </summary>
    /// <param name="position">Position in WorldSpace where collision occured.</param>
    /// <param name="force">Force of the collision to apply.</param>
    public void AddForce(Vector3 position, Vector3 force) {
        // trigger onForceWasApplied event
        ForceWasApplied?.Invoke(position, force);
    }

    #endregion

    #region Should only be called on Master!

    public void SetDamage(float damage) {
        if (Math.Abs(damage - Damage) > FloatTolerance)
            OnDamageValueHasChanged(damage);
    }


    /// <summary>
    /// Adds damage to wall (baseDamage is multiplied by damagePerHitMultiplier and added to current damage).
    /// </summary>
    /// <param name="baseDamage">BaseDamage applied by bullets or other projectiles or collisions.</param>
    public void AddDamageOnMasterClient(float baseDamage) {
        if (!PhotonNetwork.IsMasterClient) {
            Debug.LogError("Cannot add wall damage on client");
            return;
        }

        float tmpDamage = Mathf.Clamp01(Damage + baseDamage * _damagePerHitMultiplier);

        if (Math.Abs(tmpDamage - Damage) > FloatTolerance) {
            OnDamageValueHasChanged(tmpDamage);
        }
    }

    #endregion
}