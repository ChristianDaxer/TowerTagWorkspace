using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SOEventSystem.Shared;
using TowerTag;
using UnityEngine;

/// <summary>
/// Creates and manages shots. The purpose is to instantiate shot prefabs and retrieve them via their id.
/// Does <b>not</b> handle logic associated with shooting.
/// </summary>
/// <author>Ole Jürgensen (ole@vrnerds.de)</author>
[CreateAssetMenu(menuName = "TowerTag/Shot Manager")]
public class ShotManager : ScriptableObjectSingleton<ShotManager> {
    #region serialized fields

    [SerializeField, Tooltip("The prefab that is instantiated for each shot")]
    private Shot _shotPrefab;

    [SerializeField, Tooltip("Another prefab that is instantiated for each shot. " +
                             "Should be moved to a separate component")]
    private MuzzleFlash _muzzleFlashPrefab;

    [SerializeField, Tooltip("Determines the maximum shot count in the scene")]
    private int _poolSize;

    [SerializeField, Tooltip("Serialize BalanceConfiguration SO")]
    private BalancingConfiguration _balancingConfiguration;

    #endregion

    #region events

    public delegate void ShotFiredCallback(ShotManager shotManager, string id, IPlayer player, Vector3 position,
        Quaternion rotation);

    public event ShotFiredCallback ShotFired;

    public delegate void ShotDestroyedCallback(ShotManager shotManager, string id);

    public event ShotDestroyedCallback ShotDestroyed;

    #endregion

    #region cached values

    private readonly Dictionary<string, Shot> _shots = new Dictionary<string, Shot>();
    public List<Shot> Shots => _shots.Values.ToList();
    private BoundedMap<string, ShotData> _shotData;

    private ObjectPool _shotPool;
    private ObjectPool _muzzleFlashPool;

    private float _projectileSpeed;

    #endregion

    #region Init

    private void OnEnable() {
        _shotPool = new ObjectPool(_poolSize, _shotPrefab.gameObject, null, true);
        _muzzleFlashPool = new ObjectPool(_poolSize, _muzzleFlashPrefab.gameObject, null, true);
        _projectileSpeed = _balancingConfiguration.ProjectileSpeed;
        _shots.Clear();
        // _shotData holds more elements than shots, so that the data of destroyed shots can be referenced for some time
        _shotData = new BoundedMap<string, ShotData>(2 * _poolSize);
    }

    private void OnDisable() {
        _shotPool.Dispose();
        _muzzleFlashPool.Dispose();
        _shots.Clear();
        _shotData?.Clear();
    }

    #endregion

    public void CreateShot(string id, [NotNull] IPlayer player, float projectileAge, Vector3 spawnPosition, Quaternion rotation) {
        Vector3 speed = rotation * Vector3.forward;
        speed *= _projectileSpeed;
        Vector3 position = spawnPosition + projectileAge * speed;
        if (_shots.ContainsKey(id) && _shots[id] != null) {
            Shot shot = _shots[id];

            // todo extract update position into Shot. Also raycast to make sure no collider was missed.
            shot.transform.position = position;
        }
        else if (_shotData.ContainsKey(id)) {
            Debug.LogWarning($"Trying to create shot with id {id} that was already destroyed");
        }
        else {
            var shot = _shotPool.CreateGameObject(spawnPosition, rotation)?.GetComponent<Shot>();
            if (shot != null) {
                var shotData = new ShotData(id, spawnPosition, player, speed, false, shot.TactSender);
                _shotData.Add(id, shotData);
                _shots.Add(id, shot);
                shot.Fire(shotData, projectileAge);
                ShotFired?.Invoke(this, id, player, spawnPosition, rotation);
                _muzzleFlashPool.CreateGameObject(position, rotation);
            }
            else {
                Debug.LogError($"Failed to create shot {id}. last active element: {_shotPool.LastActiveElement}");
            }
        }
    }

    public void DestroyShot(string id, bool hitValidated = false) {
        if (hitValidated && _shotData.ContainsKey(id)) {
            _shotData[id] = _shotData[id].ValidateHit();
        }

        if (!_shots.ContainsKey(id)) {
//            Debug.LogWarning($"Cannot destroy projectile with id {id}: not found");
            return;
        }

        if(_shots[id] != null) _shotPool.Destroy(_shots[id].GetComponent<IPoolableObject>().GetID());
        ShotDestroyed?.Invoke(this, id);
        _shots.Remove(id);
    }

    [CanBeNull] public ShotData GetShotData(string shotID) {
        return _shotData.ContainsKey(shotID) ? _shotData[shotID] : null;
    }
}