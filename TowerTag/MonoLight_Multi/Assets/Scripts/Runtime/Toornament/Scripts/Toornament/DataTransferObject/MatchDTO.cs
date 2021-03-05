// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming
namespace Toornament.DataTransferObject {
    public class MatchDTO {
        public string id { get; set; }
        public string type { get; set; }
        public string discipline { get; set; }
        public string status { get; set; }
        public string tournament_id { get; set; }
        public int? number { get; set; }
        public int? stage_number { get; set; }
        public int? group_number { get; set; }
        public int? round_number { get; set; }
        public string date { get; set; }
        public string timezone { get; set; }
        public string match_format { get; set; }
        public string note { get; set; }
        public OpponentDTO[] opponents { get; set; }
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
    }
}