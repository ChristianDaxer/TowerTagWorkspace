using System;
using SOEventSystem.Shared;
using TowerTag;
using UnityEngine;

/// <summary>
/// <see cref="SharedVariable{T}"/> wrapper for a player teleport event.
/// </summary>
/// <author>Ole Jürgensen (ole@vrnerds.de)</author>
[CreateAssetMenu(menuName = "Shared/TowerTag/Player Teleport")]
public class SharedPlayerTeleport : SharedVariable<PlayerTeleport> { }

/// <summary>
/// Event Arguments for a player teleport event.
/// </summary>
/// <author>Ole Jürgensen (ole@vrnerds.de)</author>
[Serializable]
public class PlayerTeleport {
    [SerializeField] private IPlayer _player;
    [SerializeField] private Pillar _origin;
    [SerializeField] private Pillar _target;

    /// <summary>
    /// The player that teleports.
    /// </summary>
    public IPlayer Player => _player;

    /// <summary>
    /// The <see cref="Pillar"/> the player teleports from.
    /// </summary>
    public Pillar Origin => _origin;

    /// <summary>
    /// The <see cref="Pillar"/> the player teleports to.
    /// </summary>
    public Pillar Target => _target;

    public PlayerTeleport(IPlayer player, Pillar origin, Pillar target) {
        _player = player;
        _origin = origin;
        _target = target;
    }
}