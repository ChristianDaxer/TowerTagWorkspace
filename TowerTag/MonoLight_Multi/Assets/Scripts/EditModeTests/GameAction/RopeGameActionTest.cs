using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExitGames.Client.Photon;
using GameManagement;
using NSubstitute;
using NUnit.Framework;
using Photon.Realtime;
using TowerTag;
using UnityEngine;
using IPlayer = TowerTag.IPlayer;

namespace Tests {
    public class RopeGameActionTest {
        [Test]
        public void ShouldThrowWhenConnectingWithoutPlayer() {
            var ropeGameAction = ScriptableObject.CreateInstance<RopeGameAction>();
            var target = new GameObject().AddComponent<OptionSelector>();
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentException>(() => ropeGameAction.ConnectRope(target, null));
        }

        [Test]
        public void ShouldThrowWhenConnectingWithoutTarget() {
            var ropeGameAction = ScriptableObject.CreateInstance<RopeGameAction>();
            var player = Substitute.For<IPlayer>();
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentException>(() => ropeGameAction.ConnectRope(null, player));
        }

        [Test]
        public void ShouldDenyConnectForMissingPlayer() {
            // GIVEN: a mock Photon environment
            var ropeGameAction = ScriptableObject.CreateInstance<RopeGameAction>();
            var photonService = Substitute.For<IPhotonService>();
            var photonPlayer = Substitute.For<Photon.Realtime.IPlayer>();
            photonPlayer.ActorNumber.Returns(7);
            photonService.LocalPlayer.Returns(photonPlayer);

            // a mock game manager
            var gameManager = Substitute.For<IGameManager>();
            var matchTimer = new GameObject().AddComponent<MatchTimer>();
            gameManager.MatchTimer.Returns(matchTimer);

            // a mock player manager
            var playerManager = Substitute.For<IPlayerManager>();
            var player = Substitute.For<IPlayer>();
            var target = new GameObject().AddComponent<OptionSelector>();

            // scene service
            var sceneService = Substitute.For<ISceneService>();

            // a rope game action asset
            ropeGameAction.Init(photonService, gameManager, playerManager, sceneService);

            // WHEN trying to connect rope
            ropeGameAction.Denied += (sender, id, parameter) => Assert.Pass();
            ropeGameAction.ConnectRope(target, player);

            // THEN fail if denied callback was not triggered
            Assert.Fail("Should have passed by deny event");
        }

        [Test]
        public void ShouldSendConnectEvent() {
            // GIVEN: a mock Photon environment
            var ropeGameAction = ScriptableObject.CreateInstance<RopeGameAction>();
            var photonService = Substitute.For<IPhotonService>();
            var photonPlayer = Substitute.For<Photon.Realtime.IPlayer>();
            photonPlayer.ActorNumber.Returns(7);
            photonService.LocalPlayer.Returns(photonPlayer);

            // a mock game manager
            var gameManager = Substitute.For<IGameManager>();
            var matchTimer = new GameObject().AddComponent<MatchTimer>();
            gameManager.MatchTimer.Returns(matchTimer);

            // a mock player manager with a registered player
            var playerManager = Substitute.For<IPlayerManager>();
            var player = Substitute.For<IPlayer>();
            var stayLoggedInTrigger = new GameObject().AddComponent<StayLoggedInTrigger>();
            player.LoggedInTrigger.Returns(stayLoggedInTrigger);
            player.PlayerID.Returns(7001);
            player.OwnerID.Returns(7);
            player.IsAlive.Returns(true);
            var target = new GameObject().AddComponent<OptionSelector>();
            var trigger = new GameObject("LoggedInTrigger").AddComponent<StayLoggedInTrigger>();
            trigger.GetType().GetField("_options", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(trigger, new[] {target});
            player.LoggedInTrigger.Returns(trigger);
            playerManager.GetPlayer(7001).Returns(player);

            // scene service
            var sceneService = Substitute.For<ISceneService>();

            // an optimistic rope game action asset
            ropeGameAction.Init(photonService, gameManager, playerManager, sceneService);

            // WHEN trying to connect rope
            ropeGameAction.ConnectRope(target, player);

            // THEN should raise the game action network event
            photonService.Received().RaiseEvent(
                8,
                Arg.Is<object[]>(objects => objects.Contains(7)), // contains player ID
                Arg.Is<RaiseEventOptions>(options => options.Receivers == ReceiverGroup.MasterClient),
                SendOptions.SendReliable);
        }

        [Test]
        public void ShouldConnectViaEvent() {
            // GIVEN: a mock Photon environment
            var ropeGameAction = ScriptableObject.CreateInstance<RopeGameAction>();
            var photonService = Substitute.For<IPhotonService>();
            var photonPlayer = Substitute.For<Photon.Realtime.IPlayer>();
            photonPlayer.ActorNumber.Returns(7);
            photonService.LocalPlayer.Returns(photonPlayer);

            // a mock game manager
            var gameManager = Substitute.For<IGameManager>();
            var matchTimer = new GameObject().AddComponent<MatchTimer>();
            gameManager.MatchTimer.Returns(matchTimer);

            // a mock chargeable and stay-logged-in trigger
            var stayLoggedInTrigger = new GameObject().AddComponent<StayLoggedInTrigger>();
            var target = new GameObject().AddComponent<OptionSelector>();
            target.ID = 9;
            stayLoggedInTrigger.GetType()
                .GetField("_options", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(stayLoggedInTrigger, new[] {target});

            // a mock player manager with a registered player
            var playerManager = Substitute.For<IPlayerManager>();
            var player = Substitute.For<IPlayer>();
            player.LoggedInTrigger.Returns(stayLoggedInTrigger);
            player.PlayerID.Returns(7);
            player.IsAlive.Returns(true);
            playerManager.GetPlayer(7).Returns(player);

            // scene service
            var sceneService = Substitute.For<ISceneService>();

            // a rope game action asset
            ropeGameAction.Init(photonService, gameManager, playerManager, sceneService);

            // a connection network event
            object[] eventContent = new RopeGameAction.Parameter {
                ActionType = RopeGameAction.Parameter.RopeActionType.Connect,
                PlayerId = 7,
                TargetId = 9,
                TargetType = RopeGameAction.Parameter.RopeTargetType.Option,
                TriggeredBy = 7
            }.Serialize();

            var eventData = new EventData {
                Code = 8,
                Parameters = new Dictionary<byte, object> {
                    {ParameterCode.Data, eventContent},
                    {ParameterCode.ActorNr, 7}
                }
            };

            // WHEN
            ropeGameAction.RopeConnectedToChargeable += (sender, player1, chargeable) => {
                Assert.AreEqual(ropeGameAction, sender);
                Assert.AreEqual(player1, player);
                Assert.AreEqual(chargeable, target);

                // THEN
                Assert.Pass();
            };
            ropeGameAction.OnEvent(eventData);

            // ELSE
            Assert.Fail("Should have passed via connection event");
        }
    }
}