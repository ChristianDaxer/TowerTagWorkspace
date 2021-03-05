using System;
using Photon.Pun;
using TowerTag;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Action/Shot")]
public class ShotGameAction : GameAction<ShotParameter> {
    [SerializeField] private ShotManager _shotManager;

    protected override byte EventCode => 0;
    protected override byte DenyEventCode => 1;

    protected override bool IsValid(int senderId, ShotParameter parameters) {
        // todo validate: is in chaperone, is not in tower, ...
        IPlayer player = PlayerManager.GetPlayer(parameters.PlayerID);
        if (player == null) {
            Debug.LogWarning("Shot action denied: player is null");
            return false;
        }

        if (senderId != player.OwnerID) {
            if(!player.IsBot) Debug.LogWarning("Shot action denied: senderId != OwnerId -> no authority");
            return false;
        }

        if (player.IsInIngameMenu) {
            if(!player.IsBot) Debug.LogWarning("Shot action denied: Player is in ingame menu");
            return false;
        }
        if (!player.PlayerHealth.IsAlive) {
            if(!player.IsBot) Debug.LogWarning("Shot action denied: player is not alive, should not be able to shoot!");
            return false;
        }

        if (!GameManagerInstance.MatchTimer.IsMatchTimer && GameManagerInstance.CurrentState !=
            GameManager.GameManagerStateMachine.State.Configure
            && GameManagerInstance.CurrentState != GameManager.GameManagerStateMachine.State.Tutorial) {
            if(!player.IsBot) Debug.LogWarning("Shot action denied: Neither a match is running nor game is in hub");
            return false;
        }

        return true;
//
//        return player != null // player still connected
//               && senderId == player.OwnerID // has authority
//               && !player.IsInIngameMenu // player is not in ingame vr menu
//               && (player.PlayerHealth.IsAlive || GameManagerInstance.CurrentState ==
//                   GameManager.GameManagerStateMachine.State.Configure)
//               && (GameManagerInstance.MatchTimer.IsMatchTimer || GameManagerInstance.CurrentState ==
//                   GameManager.GameManagerStateMachine.State.Configure);
    }

    protected override void Execute(int senderId, ShotParameter parameters) {
        var age = (float) (PhotonNetwork.Time - parameters.TimeStamp);
        if (age < 0) {
            //Debug.LogError($"Negative projectile age ${age}. Network Time Stamps have diverged. Need to Resynchronize!");
            age = 0; // otherwise shots will be instantiated behind the gun
        }

        IPlayer player = PlayerManager.GetPlayer(parameters.PlayerID);
        if (player == null) {
            Debug.LogWarning("Cannot create shot, because the shooting player " +
                             $"with id {parameters.PlayerID} could not be found");
            return;
        }

        _shotManager.CreateShot(parameters.ShotID, player, age, parameters.Position, parameters.Rotation);
        // todo handle energy here
        // todo fire event that can cause muzzle flash and rumble (if local)
        if (PhotonNetwork.IsMasterClient && GameManagerInstance.CurrentMatch != null
                                         && GameManagerInstance.CurrentMatch.IsActive)
            GameManagerInstance.CurrentMatch.Stats.AddShot(player);
    }

    protected override void Deny(int senderId, ShotParameter parameters) {
        IPlayer player = PlayerManager.GetPlayer(parameters.PlayerID);
        if(player.IsMe)
            player?.GunController.ShotDeniedSound.Play();
        //        Debug.LogWarning("Shot action denied:" +
        //                         $" match timer: {GameManagerInstance.MatchTimer.IsMatchTimer}"
        //                         + $" sender: {senderId}, playerID: {parameters.PlayerID}"
        //                         + $" alive: {player != null && player.PlayerHealth.IsAlive}"
        //                         + $" hub scene: {MySceneManager.Instance.IsInHubScene}");
        // todo fire event that is caught by Components to give audio/visual feedback
    }

    protected override void Rollback(int senderId, ShotParameter parameters) {
        _shotManager.DestroyShot(parameters.ShotID);
    }

    public void Shoot(IPlayer player, Vector3 position, Quaternion rotation) {
        Trigger(new ShotParameter {
            ShotID = Guid.NewGuid().ToString(),
            PlayerID = player.PlayerID,
            Position = position,
            Rotation = rotation,
            TimeStamp = PhotonNetwork.Time
        });
    }
}

public class ShotParameter : GameActionParameter {
    public string ShotID;
    public int PlayerID;
    public Vector3 Position;
    public Quaternion Rotation;
    public double TimeStamp;

    protected override object[] SerializeParameters() {
        return new object[] {ShotID, PlayerID, Position, Rotation, TimeStamp};
    }

    protected override void DeserializeParameters(object[] objects) {
        ShotID = (string) objects[0];
        PlayerID = (int) objects[1];
        Position = (Vector3) objects[2];
        Rotation = (Quaternion) objects[3];
        TimeStamp = (double) objects[4];
    }
}