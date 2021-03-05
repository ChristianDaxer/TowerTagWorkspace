// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming
namespace Toornament.Store.Model {
    public class Match {
        public string id { get; set; }
        [ShowInUI] public string type { get; set; }
        public string discipline { get; set; }
        [ShowInUI] public string status { get; set; }
        public string tournament_id { get; set; }
        [ShowInUI] public int? number { get; set; }
        [ShowInUI] public int? stage_number { get; set; }
        [ShowInUI] public int? group_number { get; set; }
        [ShowInUI] public int? round_number { get; set; }
        [ShowInUI] public string date { get; set; }
        public string timezone { get; set; }
        [ShowInUI] public string match_format { get; set; }
        [ShowInUI] public string note { get; set; }
        public Opponent[] opponents { get; set; }
        public Stream[] streams { get; set; }
        public VOD[] vods { get; set; }
        public string private_note { get; set; }

        public class Stream {
            public string id { get; set; }
            public string name { get; set; }
            public string url { get; set; }
            public string language { get; set; }
        }

        public class VOD {
            public string name { get; set; }
            public string url { get; set; }
            public string language { get; set; }
            public string category { get; set; }
        }

        public void UpdateResult(MatchResult matchResult) {
            status = matchResult.status;
            opponents = matchResult.opponents;
        }

        public MatchResult GetResult() {
            return new MatchResult {
                status = status,
                opponents = opponents
            };
        }
    }
}