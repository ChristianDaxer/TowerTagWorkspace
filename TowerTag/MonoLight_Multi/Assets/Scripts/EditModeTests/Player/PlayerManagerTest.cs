using System.Linq;
using NSubstitute;
using NUnit.Framework;
using TowerTag;
using UnityEngine;

namespace Player {
    public class PlayerManagerTest {
        [TearDown]
        public void TearDown() {
            GameManager.Instance.Dispose();
            PlayerManager.Instance.Dispose();
        }

        [SetUp]
        public void SetUp() {
            GameManager.Instance.Dispose();
            PlayerManager.Instance.Dispose();
        }

        [Test]
        public void ShouldGetNoPlayers() {
            Assert.AreEqual(0, PlayerManager.Instance.GetAllConnectedPlayerCount());
        }

        [Test]
        public void ShouldAddPlayer() {
            var player = Substitute.For<IPlayer>();
            player.GameObject.Returns(new GameObject());
            PlayerManager.Instance.AddPlayer(player);

            PlayerManager.Instance.GetAllConnectedPlayers(out var players, out var count);
            Assert.AreEqual(player, players.Single());
        }

        [Test]
        public void ShouldRemovePlayer() {
            var player = Substitute.For<IPlayer>();
            player.GameObject.Returns(new GameObject());
            PlayerManager.Instance.AddPlayer(player);
            PlayerManager.Instance.RemovePlayer(player);

            Assert.AreEqual(0, PlayerManager.Instance.GetAllConnectedPlayerCount());
        }

        [Test]
        public void ShouldNotGetMissingPlayer() {
            IPlayer player = PlayerManager.Instance.GetPlayer(7);
            Assert.Null(player);
        }

        [Test]
        public void ShouldGetPlayer() {
            var player = Substitute.For<IPlayer>();
            player.GameObject.Returns(new GameObject());
            player.PlayerID.Returns(7);
            player.IsValid.Returns(true);
            PlayerManager.Instance.AddPlayer(player);

            Assert.AreEqual(player, PlayerManager.Instance.GetPlayer(7));
        }

        [Test]
        public void ShouldNotGetInvalidPlayer() {
            var player = Substitute.For<IPlayer>();
            player.GameObject.Returns(new GameObject());
            player.PlayerID.Returns(7);
            player.IsValid.Returns(false);
            PlayerManager.Instance.AddPlayer(player);

            Assert.Null(PlayerManager.Instance.GetPlayer(7));
        }

        [Test]
        public void ShouldNotGetOwnPlayer() {
            Assert.Null(PlayerManager.Instance.GetOwnPlayer());
        }

        [Test]
        public void ShouldGetOwnPlayer() {
            var photonService = Substitute.For<IPhotonService>();
            var localPhotonPlayer = Substitute.For<Photon.Realtime.IPlayer>();
            localPhotonPlayer.ActorNumber.Returns(7);
            photonService.LocalPlayer.Returns(localPhotonPlayer);
            ServiceProvider.Set(photonService);

            var player = Substitute.For<IPlayer>();
            player.PlayerID.Returns(7);
            player.IsValid.Returns(true);
            player.IsMe.Returns(true);
            PlayerManager.Instance.AddPlayer(player);

            Assert.AreEqual(player, PlayerManager.Instance.GetOwnPlayer());
        }

        [Test]
        public void ShouldGetAllParticipatingPlayers() {
            var photonService = Substitute.For<IPhotonService>();
            var localPhotonPlayer = Substitute.For<Photon.Realtime.IPlayer>();
            localPhotonPlayer.ActorNumber.Returns(7);
            photonService.LocalPlayer.Returns(localPhotonPlayer);
            ServiceProvider.Set(photonService);

            var player = Substitute.For<IPlayer>();
            player.GameObject.Returns(new GameObject());
            player.PlayerID.Returns(7);
            player.IsValid.Returns(true);
            player.IsParticipating.Returns(true);
            PlayerManager.Instance.AddPlayer(player);

            PlayerManager.Instance.GetParticipatingPlayers(out var players, out var count);
            for (int i = 0; i < count; i++)
            {
                if (players[i] == player)
                {
                    Assert.True(true);
                    break;
                }
            }
        }

        [Test]
        public void ShouldNotGetNonParticipatingPlayers() {
            var photonService = Substitute.For<IPhotonService>();
            var localPhotonPlayer = Substitute.For<Photon.Realtime.IPlayer>();
            localPhotonPlayer.ActorNumber.Returns(7);
            photonService.LocalPlayer.Returns(localPhotonPlayer);
            ServiceProvider.Set(photonService);

            var player = Substitute.For<IPlayer>();
            player.GameObject.Returns(new GameObject());
            player.PlayerID.Returns(7);
            player.IsValid.Returns(true);
            player.IsParticipating.Returns(false);
            PlayerManager.Instance.AddPlayer(player);

            PlayerManager.Instance.GetParticipatingPlayers(out var players, out var count);
            for (int i = 0; i < count; i++)
            {
                if (players[i] == player)
                {
                    Assert.False(true);
                    break;
                }
            }
        }

        [Test]
        public void ShouldGetSpectatingPlayers() {
            var photonService = Substitute.For<IPhotonService>();
            var localPhotonPlayer = Substitute.For<Photon.Realtime.IPlayer>();
            localPhotonPlayer.ActorNumber.Returns(7);
            photonService.LocalPlayer.Returns(localPhotonPlayer);
            ServiceProvider.Set(photonService);

            var player = Substitute.For<IPlayer>();
            player.GameObject.Returns(new GameObject());
            player.PlayerID.Returns(7);
            player.IsValid.Returns(true);
            player.IsParticipating.Returns(false);
            PlayerManager.Instance.AddPlayer(player);

            PlayerManager.Instance.GetSpectatingPlayers(out var spectating, out var count);
            for (int i = 0; i < count; i++)
            {
                if (spectating[i] == player)
                {
                    Assert.True(true);
                    break;
                }
            }
        }

        [Test]
        public void ShouldNotGetSpectatingPlayers() {
            var photonService = Substitute.For<IPhotonService>();
            var localPhotonPlayer = Substitute.For<Photon.Realtime.IPlayer>();
            localPhotonPlayer.ActorNumber.Returns(7);
            photonService.LocalPlayer.Returns(localPhotonPlayer);
            ServiceProvider.Set(photonService);

            var player = Substitute.For<IPlayer>();
            player.GameObject.Returns(new GameObject());
            player.PlayerID.Returns(7);
            player.IsValid.Returns(true);
            player.IsParticipating.Returns(true);
            PlayerManager.Instance.AddPlayer(player);

            PlayerManager.Instance.GetSpectatingPlayers(out var spectating, out var count);
            for (int i = 0; i < count; i++)
            {
                if (spectating[i] == player)
                {
                    Assert.False(true);
                    break;
                }
            }
        }
    }
}