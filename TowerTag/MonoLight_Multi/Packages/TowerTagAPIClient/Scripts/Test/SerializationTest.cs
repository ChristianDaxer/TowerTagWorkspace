using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using TowerTagAPIClient.Model;

namespace TowerTagAPIClient {
    public class SerializationTest {
        [Test]
        public void ShouldSerializeMatch() {
            Match dummyMatch = Match.DummyMatch(new[] {
                new Match.Player {id = "playerId1", isBot = false, isMember = true, teamId = 0},
                new Match.Player {id = "playerId2", isBot = false, isMember = true, teamId = 1}
            });
            string serialized = JsonConvert.SerializeObject(dummyMatch);
            var deserialized = JsonConvert.DeserializeObject<Match>(serialized);

            Assert.AreEqual(dummyMatch.winningTeam, deserialized.winningTeam);
            Assert.AreEqual(dummyMatch.players.Select(p => p.id), deserialized.players.Select(p => p.id));
        }

        [Test]
        public void ShouldSerializeMatchPlayerPerformance() {
            Match.PlayerPerformance playerPerformance = Match.DummyPerformance(
                new Match.Player {id = "playerId1", isBot = false, isMember = true, teamId = 0});
            string serialized = JsonConvert.SerializeObject(playerPerformance);
            var deserialized = JsonConvert.DeserializeObject<Match.PlayerPerformance>(serialized);

            Assert.AreEqual(playerPerformance.damageDealt, deserialized.damageDealt);
            Assert.AreEqual(playerPerformance.healthHealed, deserialized.healthHealed);
        }

        [Test]
        public void ShouldSerializeDictionary() {
            var dictionary = new Dictionary<string, int> {{"test", 3}, {"test2", 7}};
            string serialized = JsonConvert.SerializeObject(dictionary);
            var deserialized = JsonConvert.DeserializeObject<Dictionary<string, int>>(serialized);

            Assert.AreEqual(dictionary, deserialized);
        }
    }
}