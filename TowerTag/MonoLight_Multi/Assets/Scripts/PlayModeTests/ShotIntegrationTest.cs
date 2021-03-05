using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using NSubstitute;
using NUnit.Framework;
using TowerTag;
using UnityEngine;
using UnityEngine.TestTools;

[TestFixture, Category("Tower Tag")]
public class ShotIntegrationTest : TowerTagTest {
    [UnityTest, Timeout(15000)]
    public IEnumerator ShouldShootOnOperator() {
        // Given operator and one player
        yield return null;
        var mockPhotonService = Substitute.For<IPhotonService>();
        yield return StartOperator(mockPhotonService);
        var photonPlayer = Substitute.For<Photon.Realtime.IPlayer>();
        photonPlayer.ActorNumber.Returns(7);
        Player player = InstantiateFakePlayer(7001, photonPlayer, TeamID.Ice);
        yield return null;

        // When triggering shot game action
        ShotGameAction shotGameAction = Resources.FindObjectsOfTypeAll<ShotGameAction>()[0];
        var shotParameter = new ShotParameter {
            TriggeredBy = 7,
            PlayerID = 7001,
            Position = Vector3.zero,
            Rotation = Quaternion.LookRotation(Vector3.up),
            TimeStamp = 0,
            ShotID = "shotID_ShouldSpawnPlayerAndShoot"
        };

        shotGameAction.OnEvent(new EventData {
            Code = 0,
            Parameters = new Dictionary<byte, object> {
                {245, shotParameter.Serialize()},
                {254, 7}
            }
        });

        // Then a shot should appear
        Shot shot = null;
        yield return new WaitUntil(() => (shot = Object.FindObjectOfType<Shot>()) != null);
        Assert.AreEqual(player, shot.Player);
    }

    [UnityTest, Timeout(15000)]
    public IEnumerator ShouldShootAsFPSPlayer() {
        // Given an FPS player
        yield return StartFPSPlayer(Substitute.For<IPhotonService>());

        // When pressing the trigger
        var gunController = Object.FindObjectOfType<GunController>();
        gunController.OnTriggerPressed(GunController.GunControllerState.TriggerAction.Shoot);

        // Then a shot should appear
        yield return new WaitUntil(() => Object.FindObjectOfType<Shot>() != null);
    }

    [UnityTest, Timeout(15000)]
    public IEnumerator ShouldHitOtherPlayer() {
        yield return null;
        var mockPhotonService = Substitute.For<IPhotonService>();
        yield return StartOperator(mockPhotonService);

        var photonPlayerIce = Substitute.For<Photon.Realtime.IPlayer>();
        photonPlayerIce.ActorNumber.Returns(7);
        Player playerIce = InstantiateFakePlayer(7001, photonPlayerIce, TeamID.Ice);
        var photonPlayerFire = Substitute.For<Photon.Realtime.IPlayer>();
        photonPlayerFire.ActorNumber.Returns(6);
        Player playerFire = InstantiateFakePlayer(6001, photonPlayerFire, TeamID.Fire);

        yield return null;
        ShotGameAction shotGameAction = Resources.FindObjectsOfTypeAll<ShotGameAction>()[0];
        var shotParameter = new ShotParameter {
            TriggeredBy = playerIce.PlayerID,
            PlayerID = playerIce.PlayerID,
            Position = Vector3.zero,
            Rotation = Quaternion.LookRotation(Vector3.up),
            TimeStamp = 0,
            ShotID = "shotID_ShouldHitOtherPlayer"
        };
        shotGameAction.OnEvent(new EventData {
            Code = 0,
            Parameters = new Dictionary<byte, object> {
                {245, shotParameter.Serialize()},
                {254, 7}
            }
        });
        HitGameAction hitGameAction = Resources.FindObjectsOfTypeAll<HitGameAction>()[0];
        var hitParameter = new HitParameter {
            ColliderType = DamageDetectorBase.ColliderType.Head,
            HitNormal = Vector3.forward,
            HitPoint = Vector3.zero,
            ShotID = "shotID_ShouldHitOtherPlayer",
            TargetPlayerID = playerFire.PlayerID,
            TriggeredBy = playerIce.PlayerID
        };
        hitGameAction.OnEvent(new EventData {
            Code = 2,
            Parameters = new Dictionary<byte, object> {
                {245, hitParameter.Serialize()},
                {254, playerIce.OwnerID}
            }
        });
        hitGameAction.OnEvent(new EventData {
            Code = 2,
            Parameters = new Dictionary<byte, object> {
                {245, hitParameter.Serialize()},
                {254, playerIce.OwnerID}
            }
        }); // second hit event will be ignored, because shot has already hit
        Assert.AreEqual(20, playerFire.PlayerHealth.CurrentHealth);
    }
}