// ReSharper disable InconsistentNaming

using JetBrains.Annotations;

namespace Toornament.Store.Model {
    public class MatchResult {
        [ShowInUI] public string status { get; set; }
        [ShowInUI] public Opponent[] opponents { get; set; }
        [UsedImplicitly]
        public static MatchResult Dummy() {
            var matchResult = new MatchResult {
                status = "running",
                opponents = new Opponent[] { }
            };
            return matchResult;
        }
    }
}