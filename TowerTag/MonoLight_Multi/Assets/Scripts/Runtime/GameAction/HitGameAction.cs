using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#if !UNITY_ANDROID
using System.Windows.Forms.VisualStyles;
#endif
using JetBrains.Annotations;
using TowerTag;
using UnityEngine;
using ColliderType = DamageDetectorBase.ColliderType;

/// <summary>
/// Game action that is triggered when something or someone is hit by a shot.
/// </summary>
/// <author>Ole Jürgensen (ole@vrnerds.de)</author>
[CreateAssetMenu(menuName = "Game Action/Hit")]
public class HitGameAction : GameAction<HitParameter> {
    [SerializeField, Tooltip("Defines which party determines whether a hit is valid")]
    private HitAuthority _hitAuthority;

    [SerializeField, Tooltip("Manager to create and destroy shots")]
    private ShotManager _shotManager;

    [SerializeField, Tooltip("Manager to retrieve pillar walls from their id")]
    private PillarWallManager _pillarWallManager;

    [SerializeField] private WallDamageGameAction _wallDamageGameAction;

    public static HitGameAction Instance => (HitGameAction)Singleton;

    public delegate void PlayerHit(ShotData shotData, IPlayer targetPlayer, Vector3 hitPoint, ColliderType targetType);

    public event PlayerHit PlayerGotHit;

    protected override byte EventCode => 2;
    protected override byte DenyEventCode => 3;

#region cached

    private Dictionary<ColliderType, int> _damageTable;
    [SerializeField] private List<DamageTableEntry> _damageTableEntries;

#endregion

    [Serializable]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public struct DamageTableEntry {
        public ColliderType ColliderType;
        public int Damage;
    }

    private enum HitAuthority {
        ShootingPlayer,
        TargetPlayer,
        Operator,
        Any
    }

    private new void OnEnable() {
        base.OnEnable();
        _damageTable = _damageTableEntries.ToDictionary(entry => entry.ColliderType, entry => entry.Damage);
    }

    protected override bool IsValid(int senderId, HitParameter parameters) {
        ShotData shotData = _shotManager.GetShotData(parameters.ShotID);

        if (shotData?.Player == null)
            return false; // shooting player disconnected

        IPlayer targetPlayer = PlayerManager.GetPlayer(parameters.TargetPlayerID);
        if (targetPlayer == null)
            return false; // target player disconnected

        if (parameters.ColliderType == ColliderType.Undefined)
            return false; // unidentified hit zone

        if (!targetPlayer.PlayerHealth.IsAlive)
            return false; // target is already dead

        if (GameManagerInstance.CurrentState != GameManager.GameManagerStateMachine.State.Configure
            && GameManagerInstance.CurrentMatch != null && !GameManagerInstance.CurrentMatch.IsActive)
            return false; // neither configure nor running match

        if (shotData.HasHit)
            return false; // hit already validated

        switch (_hitAuthority) {
            case HitAuthority.ShootingPlayer:
                return shotData.Player.OwnerID == senderId;
            case HitAuthority.TargetPlayer:
                return targetPlayer.OwnerID == senderId;
            case HitAuthority.Operator:
                return PhotonService.MasterClient.ActorNumber == senderId;
            case HitAuthority.Any:
                return true;
            default:
                Debug.LogWarning($"Unknown hit authority setting {_hitAuthority}");
                return false;
        }
    }

    protected override void Execute(int senderId, HitParameter parameters) {
        ShotData shotData = _shotManager.GetShotData(parameters.ShotID);
        Vector3 hitPoint = parameters.HitPoint;
        ColliderType colliderType = parameters.ColliderType;
        IPlayer targetPlayer = PlayerManager.GetPlayer(parameters.TargetPlayerID);
        if (targetPlayer == null) {
            Debug.LogWarning($"Cannot execute HitGameAction {parameters}: target player cannot be found");
            return;
        }

        if (shotData == null) {
            Debug.LogWarning($"Cannot execute HitGameAction {parameters}: shot cannot be found");
            return;
        }

        HitPlayer(shotData, hitPoint, colliderType, targetPlayer);
    }

    protected override void Deny(int senderId, HitParameter parameters) {
        // this is normal, because only one player has authority while all of them can report
//        Debug.LogWarning("Denied requested hit");
    }

    protected override void Rollback(int senderId, HitParameter parameters) {
        // recreate shot? Might look awkward if a shot is respawned in midair. On the other hand players might get hit by invisible shots
        Debug.LogWarning("Cannot rollback hit");
    }

    public void TriggerPlayerHit(Shot shot, IPlayer targetPlayer, Vector3 hitPoint, ColliderType colliderType) {
        Trigger(new HitParameter {
            ShotID = shot.ID,
            HitPoint = hitPoint,
            HitNormal = -shot.transform.forward,
            ColliderType = colliderType,
            TargetPlayerID = targetPlayer.PlayerID
        });
        _shotManager.DestroyShot(shot.ID);
    }

    public void TriggerWallHit(Shot shot, string wallID, Vector3 hitPoint, Vector3 hitNormal, string colliderTag) {
        PillarWall wall = _pillarWallManager.GetPillarWall(wallID);
        if (wall != null)
            HitWall(shot, hitPoint, hitNormal, colliderTag, wall);
    }

    public void TriggerEnvironmentHit(Shot shot, Vector3 hitPoint, Vector3 hitNormal, string colliderTag) {
        HitEnvironment(shot, hitPoint, hitNormal, colliderTag);
    }

    private void HitPlayer([NotNull] ShotData shotData, Vector3 hitPoint, ColliderType colliderType,
        [NotNull] IPlayer targetPlayer) {
        if (shotData.Player?.GameObject == null)
            return;
        if (shotData.Player.TeamID != targetPlayer.TeamID) {
            if (PhotonService.IsMasterClient) {
                if (GameManagerInstance.CurrentMatch != null && GameManagerInstance.CurrentMatch.IsActive) {
                    GameManagerInstance.CurrentMatch.Stats.AddHit(targetPlayer, shotData.Player,
                        _damageTable[colliderType]);
                }

                targetPlayer.PlayerHealth.TakeDamage(_damageTable[colliderType], colliderType, shotData.Player);
            }

            if (!targetPlayer.IsMe) {
                EffectDatabase.PlaceDecal("Dec_Player", hitPoint, Quaternion.LookRotation(-shotData.Speed),
                    shotData.Player, true);
            }

            PlayerGotHit?.Invoke(shotData, targetPlayer, hitPoint, colliderType);
            _shotManager.DestroyShot(shotData.ID, true);
        }
        else {
            EffectDatabase.PlaceDecal("Dec_FriendlyFire", hitPoint, Quaternion.LookRotation(-shotData.Speed),
                shotData.Player, true);
        }
    }

    private void HitEnvironment(Shot shot, Vector3 hitPoint, Vector3 hitNormal, string colliderTag) {
        EffectDatabase.PlaceDecal(colliderTag, hitPoint, Quaternion.LookRotation(hitNormal), shot.Player, true);
        _shotManager.DestroyShot(shot.ID, true);
    }

    private void HitWall(Shot shot, Vector3 hitPoint, Vector3 hitNormal, string colliderTag,
        [NotNull] PillarWall wall) {
        EffectDatabase.PlaceDecal(colliderTag, hitPoint, Quaternion.LookRotation(hitNormal), shot.Player, true);
        wall.AddForce(hitPoint, shot.transform.forward);
        if (PhotonService.IsMasterClient) {
            wall.AddDamageOnMasterClient(1);
            _wallDamageGameAction.TriggerWallDamage(wall.ID, wall.Damage);
        }

        _shotManager.DestroyShot(shot.ID, true);
    }

    public void InvokePlayerGotHitEvent(Player targetPlayer)
    {
        var transform = targetPlayer.transform;
        ShotData shotData = new ShotData("autoHit", transform.forward * 100, targetPlayer as IPlayer, -transform.forward);
        PlayerGotHit?.Invoke(shotData, targetPlayer as IPlayer, transform.position + Vector3.forward, ColliderType.Body);
    }
}

public class HitParameter : GameActionParameter {
    public string ShotID;
    public Vector3 HitPoint;
    public Vector3 HitNormal;
    public ColliderType ColliderType;
    public int TargetPlayerID;

    protected override object[] SerializeParameters() {
        return new object[] {ShotID, HitPoint, HitNormal, (byte) ColliderType, TargetPlayerID};
    }

    protected override void DeserializeParameters(object[] objects) {
        ShotID = (string) objects[0];
        HitPoint = (Vector3) objects[1];
        HitNormal = (Vector3) objects[2];
        ColliderType = (ColliderType) (byte) objects[3];
        TargetPlayerID = (int) objects[4];
    }
}