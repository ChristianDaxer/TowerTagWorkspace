using Newtonsoft.Json;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming

namespace Toornament.Store.Model {
    public class Tournament {
        public string id { get; set; }
        [ShowInUI] public string discipline { get; set; }
        [ShowInUI] public string name { get; set; }
        [ShowInUI] public string full_name { get; set; }
        public string status { get; set; }
        public string date_start { get; set; }
        public string date_end { get; set; }
        public string timezone { get; set; }
        public bool online { get; set; }

        [JsonProperty("public")]
        public bool _public { get; set; }

        public bool archived { get; set; }
        public string location { get; set; }
        public string country { get; set; }
        public int size { get; set; }
        public string participant_type { get; set; }
        public string match_type { get; set; }
        public string organization { get; set; }
        public string website { get; set; }
        [ShowInUI] public string description { get; set; }
        public string rules { get; set; }
        public string prize { get; set; }
        public TournamentStream[] streams { get; set; }
        public string[] platforms { get; set; }
        public Logo logo { get; set; }
        public bool check_in { get; set; }
        public bool participant_nationality { get; set; }
        public string match_format { get; set; }

        public class Logo {
            public string logo_small { get; set; }
            public string logo_medium { get; set; }
            public string logo_large { get; set; }
            public string original { get; set; }
        }

        public class TournamentStream {
            public string id { get; set; }
            public string name { get; set; }
            public string url { get; set; }
            public string language { get; set; }
        }
    }
}