using System;
using System.Collections;
using NUnit.Framework;
using TowerTag;
using UnityEngine;
using UnityEngine.TestTools;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Object = UnityEngine.Object;

namespace Tests {
    public class PlayerIntegrationTest : TowerTagTest {
        [UnityTest, Timeout(15000)]
        public IEnumerator ShouldStartFPSPlayer() {
            yield return StartFPSPlayer(NSubstitute.Substitute.For<IPhotonService>());
            yield return new WaitUntil(() => TTSceneManager.Instance.IsInHubScene);
        }

        [UnityTest, Timeout(15000)]
        public IEnumerator ShouldSpawnPlayer() {
            yield return StartFPSPlayer(NSubstitute.Substitute.For<IPhotonService>());
            yield return new WaitUntil(() => TTSceneManager.Instance.IsInHubScene);

            var player = Object.FindObjectOfType<Player>();
            Pillar spawnPillar = PillarManager.Instance.FindSpawnPillarForPlayer(player);
            var stream = new BitSerializer(new BitWriterNoAlloc(new byte[1024]));
            stream.WriteInt(spawnPillar.ID, BitCompressionConstants.MinPillarID, BitCompressionConstants.MaxPillarID);
            int teleportTypeCount = Enum.GetNames(typeof(TeleportHelper.TeleportDurationType)).Length;
            stream.WriteInt((int) TeleportHelper.TeleportDurationType.Immediate, 0, teleportTypeCount);
            player.UpdateValuesFromPlayerProperties(new Hashtable {
                {$"{player.PlayerID}_P", stream.GetData()}
            });

            Assert.AreEqual(spawnPillar, player.CurrentPillar);
        }
    }
}