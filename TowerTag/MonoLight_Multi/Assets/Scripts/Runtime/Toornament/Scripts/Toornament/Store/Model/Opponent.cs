// ReSharper disable InconsistentNaming
namespace Toornament.Store.Model {
    public class Opponent {
        [ShowInUI]
        public int? Number { get; set; }

        [ShowInUI]
        public string ParticipantName => Participant?.name;

        public Participant Participant { get; set; }

        [ShowInUI]
        public int? result { get; set; }

        [ShowInUI]
        public int? score { get; set; }

        [ShowInUI]
        public bool? forfeit { get; set; }
    }
}