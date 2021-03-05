// ReSharper disable InconsistentNaming
namespace Toornament.DataTransferObject {
    public class ParticipantDTO {
        public string id { get; set; }
        public string name { get; set; }
        public Logo logo { get; set; }
        public string country { get; set; }
        public Lineup[] lineup { get; set; }
        public CustomField[] custom_fields { get; set; }
        public string email { get; set; }
        public bool? check_in { get; set; }
        public CustomField[] custom_fields_private { get; set; }

        public class Logo {
            public string icon_large_square { get; set; }
            public string extra_small_square { get; set; }
            public string medium_small_square { get; set; }
            public string medium_large_square { get; set; }
        }

        public class CustomField {
            public string type { get; set; }
            public string label { get; set; }
            public string value { get; set; }
        }

        public class Lineup {
            public string name { get; set; }
            public string country { get; set; }
            public CustomField[] custom_fields { get; set; }
            public string email { get; set; }
            public CustomField[] custom_fields_private { get; set; }
        }
    }
}