using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Commendations {
    public class CommendationsTest {
        private const string MVPAssetPath = "Assets/ScriptableObjects/Commendations/MVP.asset";

        [Test]
        public void ShouldLoadAsset() {
            var mvp = AssetDatabase.LoadAssetAtPath<StatsBasedCommendation>(MVPAssetPath);
            Assert.NotNull(mvp);
        }

        [Test]
        public void ShouldEvaluateMVP() {
            var mvp = AssetDatabase.LoadAssetAtPath<StatsBasedCommendation>(MVPAssetPath);

            var matchStats = Substitute.For<IMatchStats>();
            matchStats.GetPlayerStats().Returns(new Dictionary<int, PlayerStats>() {
                {0, new PlayerStats {PlayerID = 0}},
                {1, new PlayerStats {PlayerID = 1, Kills = 1}},
                {2, new PlayerStats {PlayerID = 2}},
            });
            int bestPlayer = mvp.GetBestPlayer(matchStats);

            Assert.AreEqual(1, bestPlayer);
        }

        [Test]
        public void ShouldNotAwardMVPWhenStandoff() {
            var mvp = AssetDatabase.LoadAssetAtPath<StatsBasedCommendation>(MVPAssetPath);

            var matchStats = Substitute.For<IMatchStats>();
            matchStats.GetPlayerStats().Returns(new Dictionary<int, PlayerStats>() {
                {0, new PlayerStats {PlayerID = 0}},
                {1, new PlayerStats {PlayerID = 1, Kills = 1}},
                {2, new PlayerStats {PlayerID = 2, Kills = 1}},
            });
            int bestPlayer = mvp.GetBestPlayer(matchStats);

            Assert.AreEqual(-1, bestPlayer);
        }
    }
}