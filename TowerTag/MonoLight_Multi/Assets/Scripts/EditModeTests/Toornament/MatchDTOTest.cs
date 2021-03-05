using Newtonsoft.Json;
using NUnit.Framework;
using Toornament.Store.Model;

namespace Test {
    public class MatchDTOTest {
        [Test]
        public void shouldSerialize() {
            // GIVEN
            string serialized = TestJSON();
            
            // WHEN
            var deserialized = JsonConvert.DeserializeObject<Match>(serialized);
            
            // THEN
            Assert.AreEqual("1266881481410232320", deserialized.tournament_id);
        }

        private static string TestJSON() {
            return
                "{\r\n    \"type\": \"duel\",\r\n    \"discipline\": \"streetfighter5\",\r\n    \"tournament_id\": \"1266881481410232320\",\r\n    \"opponents\": [\r\n      {\r\n        \"participant\": null,\r\n        \"forfeit\": false,\r\n        \"number\": 1,\r\n        \"result\": null,\r\n        \"score\": null\r\n      },\r\n      {\r\n        \"participant\": null,\r\n        \"forfeit\": false,\r\n        \"number\": 2,\r\n        \"result\": null,\r\n        \"score\": null\r\n      }\r\n    ],\r\n    \"id\": \"1266884641801134174\",\r\n    \"status\": \"pending\",\r\n    \"number\": 1,\r\n    \"stage_number\": 1,\r\n    \"group_number\": 1,\r\n    \"round_number\": 1,\r\n    \"date\": null,\r\n    \"timezone\": null,\r\n    \"match_format\": null\r\n  }";
        }
    }
}