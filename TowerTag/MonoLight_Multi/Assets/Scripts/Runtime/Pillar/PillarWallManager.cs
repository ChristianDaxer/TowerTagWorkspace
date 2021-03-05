using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SOEventSystem.Shared;
using UnityEngine;

/// <summary>
/// Manager that keeps track of all enabled <see cref="PillarWall"/>.
/// </summary>
/// <author>Ole Jürgensen (ole@vrnerds.de)</author>
[CreateAssetMenu(menuName = "Pillar Wall Manager")]
public class PillarWallManager : ScriptableObjectSingleton<PillarWallManager> {
    private Dictionary<string, PillarWall> _pillarWalls = new Dictionary<string, PillarWall>();

    private void OnEnable() {
        Clear();
    }

    /// <summary>
    /// Register PillarWall so it can be retrieved by its ID.
    /// </summary>
    /// <param name="pillarWall">PillarWall that be registered</param>
    public void Register(PillarWall pillarWall) {
        //Registered Wall Object is destroyed -> Overwrite
        if (_pillarWalls.ContainsKey(pillarWall.ID) && _pillarWalls[pillarWall.ID] == null) {
            _pillarWalls[pillarWall.ID] = pillarWall;
            return;
        }

        //No registered Wall
        if (!_pillarWalls.ContainsKey(pillarWall.ID)) {
            _pillarWalls.Add(pillarWall.ID, pillarWall);
            return;
        }

        //Duplicate Wall
        if (pillarWall != _pillarWalls[pillarWall.ID])
            Debug.LogWarning($"Duplicate pillar wall id {pillarWall.ID}");
    }

    public void Unregister(PillarWall pillarWall) {
        Unregister(pillarWall.ID);
    }

    private void Unregister(string pillarWallId) {
        if (_pillarWalls.ContainsKey(pillarWallId))
            _pillarWalls.Remove(pillarWallId);
    }

    public List<PillarWall> GetAllWalls() {
        return _pillarWalls.Values.Where(wall => wall != null).ToList();
    }

    [CanBeNull]
    public PillarWall GetPillarWall(string id) {
        return id != null && _pillarWalls.ContainsKey(id) ? _pillarWalls[id] : null;
    }

    [ContextMenu("Clear")]
    public void Clear() {
        _pillarWalls = _pillarWalls
            .Where(keyValuePair => keyValuePair.Value != null)
            .ToDictionary(keyValuePair => keyValuePair.Key, kv => kv.Value);
    }
}