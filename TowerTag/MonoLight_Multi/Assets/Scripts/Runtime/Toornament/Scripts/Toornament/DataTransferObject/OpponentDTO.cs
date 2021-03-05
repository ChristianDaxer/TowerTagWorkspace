// ReSharper disable InconsistentNaming
namespace Toornament.DataTransferObject {
    public class OpponentDTO {
        public int? number { get; set; }
        public ParticipantDTO participant { get; set; }
        public int? result { get; set; }
        public int? score { get; set; }
        public bool? forfeit { get; set; }
    }
}