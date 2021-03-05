// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming
namespace Toornament.DataTransferObject {
    public class CreateParticipantDTO {
        public string name { get; set; }
        public string email { get; set; }
        public TeamMember[] lineup { get; set; }

        public static CreateParticipantDTO Dummy() {
            return new CreateParticipantDTO {
                name = "Evil Geniuses",
                email = "e@mail.de",
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
        }
    }
}