// ReSharper disable InconsistentNaming
namespace Toornament.Store.Model {
    public class Participant {
        public string id { get; set; }
        [ShowInUI] public string name { get; set; }
        public Logo logo { get; set; }
        public string country { get; set; }
        [ShowInUI] public Lineup[] lineup { get; set; }
        [ShowInUI] public CustomField[] custom_fields { get; set; }
        [ShowInUI] public string email { get; set; }
        public bool? check_in { get; set; }
        public CustomField[] custom_fields_private { get; set; }

        public class Logo {
            public string icon_large_square { get; set; }
            public string extra_small_square { get; set; }
            public string medium_small_square { get; set; }
            public string medium_large_square { get; set; }
        }

        public class CustomField {
            [ShowInUI] public string type { get; set; }
            [ShowInUI] public string label { get; set; }
            [ShowInUI] public string value { get; set; }
        }

        public class Lineup {
            [ShowInUI] public string name { get; set; }
            public string country { get; set; }
            [ShowInUI] public CustomField[] custom_fields { get; set; }
            [ShowInUI] public string email { get; set; }
            public CustomField[] custom_fields_private { get; set; }
        }
    }
}