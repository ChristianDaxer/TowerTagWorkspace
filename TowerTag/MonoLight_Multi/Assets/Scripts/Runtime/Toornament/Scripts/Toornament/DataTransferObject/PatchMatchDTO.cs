// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming
namespace Toornament.DataTransferObject {
    public class PatchMatchDTO {
        public string date { get; set; }
        public string timezone { get; set; }
        public string match_format { get; set; }
        public string note { get; set; }
        public string private_note { get; set; }
        public string[] streams { get; set; }
        public VOD[] vods { get; set; }

        public class VOD {
            public string name { get; set; }
            public string url { get; set; }
            public string language { get; set; }
            public string category { get; set; }
        }
    }
}