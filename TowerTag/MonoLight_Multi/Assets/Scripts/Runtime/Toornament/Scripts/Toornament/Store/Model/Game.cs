// ReSharper disable InconsistentNaming
namespace Toornament.Store.Model {
    public class Game {
        public int number { get; set; }
        public string status { get; set; }
        public Opponent[] opponents { get; set; }
    }
}