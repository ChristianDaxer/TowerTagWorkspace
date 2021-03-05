// ReSharper disable InconsistentNaming
namespace Toornament.DataTransferObject {
    public class MatchResultDTO {
        public string status { get; set; }
        public OpponentDTO[] opponents { get; set; }

        public static MatchResultDTO Dummy() {
            var matchResult = new MatchResultDTO {
                status = "running",
                opponents = new OpponentDTO[] { }
            };
            return matchResult;
        }
    }
}