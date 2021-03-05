using Newtonsoft.Json;
using NUnit.Framework;
using Toornament;
using Toornament.DataTransferObject;

namespace Test {
    public class CreateTournamentDTOTest {
        [Test]
        public void ShouldSerialize() {
            // GIVEN
            CreateTournamentDTO createTournament = CreateTournamentDTO.Dummy();
            
            // WHEN
            string serialized = JsonConvert.SerializeObject(createTournament);
            var deserialized = JsonConvert.DeserializeObject<CreateTournamentDTO>(serialized);
            
            // THEN
            Assert.AreEqual(createTournament.full_name, deserialized.full_name);
        }
    }
}