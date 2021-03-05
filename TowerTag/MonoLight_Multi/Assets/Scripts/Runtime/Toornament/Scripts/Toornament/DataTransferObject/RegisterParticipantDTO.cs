using System.Collections.Generic;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming

namespace Toornament.DataTransferObject {
    public class RegisterParticipantDTO {
        public string tournament_id { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public Dictionary<string, string> custom_fields { get; set; }
        public TeamMember[] lineup { get; set; }

        public static RegisterParticipantDTO Dummy() {
            return new RegisterParticipantDTO {
                name = "Evil Geniuses",
                lineup = new[] {
                    new TeamMember {
                        name = "Storm Spirit",
                        email = "contact@oxent.net"
                    }
                }
            };
        }

        public class TeamMember {
            public string name { get; set; }
            public string email { get; set; }
            public Dictionary<string, string> custom_fields { get; set; }
        }
    }
}