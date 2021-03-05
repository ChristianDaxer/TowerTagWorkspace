using Bhaptics.Tact.Unity;
using TowerTag;
using UnityEngine;

public class ShotData {
    public string ID { get; }
    public Vector3 SpawnPosition { get; }
    public IPlayer Player { get; }
    public Vector3 Speed { get; }
    public TactSender TactSender { get; }
    public bool HasHit { get; }

    public ShotData(string id, Vector3 spawnPosition, IPlayer player, Vector3 speed, bool hasHit = false, TactSender tactSender = null) {
        ID = id;
        SpawnPosition = spawnPosition;
        Player = player;
        Speed = speed;
        HasHit = hasHit;
        TactSender = tactSender;
    }

    public ShotData ValidateHit() {
        return new ShotData(ID, SpawnPosition, Player, Speed, true, TactSender);
    }
}