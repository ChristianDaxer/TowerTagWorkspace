using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

namespace TowerTagAPIClient.Model {
    public class MatchOverview {
        public string id = "";
        public string date = "";
        public string location;
        public Dictionary<int, int> teamScores;
    }
}