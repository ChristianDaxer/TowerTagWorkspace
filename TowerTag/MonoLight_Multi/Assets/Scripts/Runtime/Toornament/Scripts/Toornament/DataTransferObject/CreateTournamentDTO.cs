using Newtonsoft.Json;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming

namespace Toornament.DataTransferObject {
    public class CreateTournamentDTO {
        public string discipline { get; set; }
        public string name { get; set; }
        public int size { get; set; }
        public string participant_type { get; set; }
        public string full_name { get; set; }
        public string organization { get; set; }
        public string website { get; set; }
        public string date_start { get; set; }
        public string date_end { get; set; }
        public string timezone { get; set; }
        public bool online { get; set; }

        [JsonProperty("public")]
        public bool _public { get; set; }

        public string location { get; set; }
        public string country { get; set; }
        public string description { get; set; }
        public string rules { get; set; }
        public string prize { get; set; }
        public bool check_in { get; set; }
        public bool participant_nationality { get; set; }
        public string match_format { get; set; }
        public string[] platforms { get; set; }

        public static CreateTournamentDTO Dummy() {
            return new CreateTournamentDTO {
                discipline = "tower_tag",
                name = "Dummy Tournament",
                size = 16,
                participant_type = "team"
            };
        }
    }
}