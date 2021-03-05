using System;
using System.Collections.Generic;
using System.Linq;
using Toornament.Store;
using Toornament.Store.Model;
using VRNerdsUtilities;

namespace Toornament {
    public class ToornamentManager : SingletonMonoBehaviour<ToornamentManager> {
        // cached data
        private Match _match;
        private Participant[] _participants;

        // events
        public event Action<Match> OnSelectMatch;

        private void Start() {
            transform.parent = ToornamentContainer.Instance.transform;
            ToornamentImgui.Instance.OnSelectMatch += SelectMatch;
            ParticipantStore.Instance.OnGetParticipants += GetParticipants;
        }

        private void GetParticipants(Participant[] obj) {
            _participants = obj;
        }

        private void SelectMatch(Match match) {
            _match = match;
            // enrich with lineups
            _match.opponents = match.opponents
                .Select(opponent => {
                    opponent.Participant =
                        _participants.First(participant => participant.id == opponent.Participant.id);
                    return opponent;
                }).ToArray();
            OnSelectMatch?.Invoke(match);
        }

        public void ReportResult(string winningParticipantName) {
            if (_match == null) {
                Debug.LogWarning("Failed to report match result: no match selected");
                return;
            }

            Opponent[] opponents = _match.opponents;
            foreach (Opponent opponent in opponents) {
                bool isDraw = winningParticipantName == null;
                opponent.result = isDraw
                    ? 2 // toornament draw
                    : opponent.ParticipantName.Equals(winningParticipantName)
                        ? 1 // toornament win
                        : 3; // toornament loss
            }

            var matchResult = new MatchResult {
                status = "completed",
                opponents = opponents
            };
            MatchStore.Instance.PutMatchResult(_match.tournament_id, _match.id, matchResult);
        }

        public void ReportScore(Dictionary<string, int> scores) {
            if (_match == null) {
                Debug.LogWarning("Failed to report match result: no match selected");
                return;
            }

            Opponent[] opponents = _match.opponents;
            foreach (Opponent opponent in opponents) {
                if (scores.ContainsKey(opponent.ParticipantName))
                    opponent.score = scores[opponent.ParticipantName];
            }

            var matchResult = new MatchResult {
                status = "running",
                opponents = opponents
            };
            MatchStore.Instance.PutMatchResult(_match.tournament_id, _match.id, matchResult);
        }

        private void OnDestroy() {
            ToornamentImgui.Instance.OnSelectMatch -= SelectMatch;
            ParticipantStore.Instance.OnGetParticipants -= GetParticipants;
        }
    }
}