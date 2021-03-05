using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using REST;
using Toornament.Converter;
using Toornament.DataTransferObject;
using Toornament.Store.Model;
using VRNerdsUtilities;

namespace Toornament.Store {
    public class MatchStore : SingletonMonoBehaviour<MatchStore> {
        private const string ServerUrl = "https://api.toornament.com";

        // cached data
        private readonly Dictionary<string, Match> _matches = new Dictionary<string, Match>();

        // events to subscribe on
        public event Action<Match> OnGetMatch;
        public event Action<Match[]> OnGetMatches;
        public event Action<Match> OnPatchMatch;
        public event Action<string, MatchResult> OnGetMatchResult;
        public event Action<string, MatchResult> OnPutMatchResult;
        public event Action<string> OnError;

        private void Start() {
            transform.parent = ToornamentContainer.Instance.transform;
        }

        #region GetMatches

        public void GetMatches(string tournamentId) {
            string route = "/v1/tournaments/" + tournamentId + "/matches";
            var uri = new Uri(new Uri(ServerUrl), route);
            Client.Get(uri.ToString(), AuthStore.Instance.NoAuthHeaders, GetMatchesSuccess, GetMatchesError);

            OnGetMatches?
                .Invoke(_matches
                .Select(idAndMatch => idAndMatch.Value)
                .Where(match => match.tournament_id.Equals(tournamentId))
                .ToArray());
        }

        private void GetMatchesSuccess(long responseCode, string text) {
            var matchesDTO = JsonConvert.DeserializeObject<MatchDTO[]>(text);
            Match[] newMatches = matchesDTO
                .Select(MatchConverter.Convert)
                .ToArray();
            foreach (Match match in newMatches) {
                _matches[match.id] = match;
            }

            OnGetMatches?.Invoke(newMatches);
        }

        private void GetMatchesError(long responseCode, string text) {
            Debug.LogWarning("Failed to get matches. HTTP Response Code " + responseCode + "\n" + text);
            OnError?.Invoke(text);
        }

        #endregion

        #region GetMatch
        [UsedImplicitly]
        public void GetMatch(string tournamentId, string matchId) {
            string route = "/v1/tournaments/" + tournamentId + "/matches/" + matchId;
            var uri = new Uri(new Uri(ServerUrl), route);
            Client.Get(uri.ToString(), AuthStore.Instance.NoAuthHeaders, GetMatchSuccess, GetMatchError);
            OnGetMatch?.Invoke(_matches[matchId]);
        }

        private void GetMatchSuccess(long responseCode, string text) {
            var matchDTO = JsonConvert.DeserializeObject<MatchDTO>(text);
            Match match = MatchConverter.Convert(matchDTO);
            _matches[match.id] = match;
            OnGetMatch?.Invoke(match);
        }

        private void GetMatchError(long responseCode, string text) {
            Debug.LogWarning("Failed to get match. HTTP Response Code " + responseCode + "\n" + text);
            OnError?.Invoke(text);
        }

        #endregion

        #region PatchMatch
        [UsedImplicitly]
        public void PatchMatch(int tournamentId, int matchId, Match match) {
            string route = "/v1/tournaments" + "/" + tournamentId + "/matches" + "/" + matchId;
            var uri = new Uri(new Uri(ServerUrl), route);
            string data = JsonConvert.SerializeObject(MatchConverter.ConvertToPatchMatchDTO(match));
            Client.Patch(uri.ToString(), AuthStore.Instance.AuthHeaders, data, PatchMatchSuccess, PatchMatchError);
        }

        private void PatchMatchSuccess(long responseCode, string text) {
            var matchDTO = JsonConvert.DeserializeObject<MatchDTO>(text);
            Match match = MatchConverter.Convert(matchDTO);
            _matches[match.id] = match;
            OnPatchMatch?.Invoke(match);
        }

        private void PatchMatchError(long responseCode, string text) {
            Debug.LogWarning("Failed to patch match. HTTP Response Code " + responseCode + "\n" + text);
            OnError?.Invoke(text);
        }

        #endregion

        #region GetMatchResult
        [UsedImplicitly]
        public void GetMatchResult(string tournamentId, string matchId) {
            string route = "/v1/tournaments" + "/" + tournamentId + "/matches" + "/" + matchId + "/result";
            var uri = new Uri(new Uri(ServerUrl), route);
            Client.Get(uri.ToString(), AuthStore.Instance.NoAuthHeaders,
                (responseCode, text) => GetMatchResultSuccess(matchId, text), GetMatchResultError);
            OnGetMatchResult?.Invoke(matchId, _matches[matchId].GetResult());
        }

        private void GetMatchResultSuccess(string matchId, string text) {
            var response = JsonConvert.DeserializeObject<MatchResultDTO>(text);
            MatchResult matchResult = MatchConverter.Convert(response);
            _matches[matchId].UpdateResult(matchResult);
            OnGetMatchResult?.Invoke(matchId, matchResult);
        }

        private void GetMatchResultError(long responseCode, string text) {
            Debug.LogWarning("Failed to get match result. HTTP Response Code " + responseCode + "\n" + text);
            OnError?.Invoke(text);
        }

        #endregion

        #region PutMatchResult

        public void PutMatchResult(string tournamentId, string matchId, MatchResult matchResult) {
            string route = "/v1/tournaments" + "/" + tournamentId + "/matches" + "/" + matchId + "/" + "result";
            var uri = new Uri(new Uri(ServerUrl), route);
            string data = JsonConvert.SerializeObject(MatchConverter.Convert(matchResult));
            Client.Put(uri.ToString(), AuthStore.Instance.AuthHeaders, data,
                (responseCode, text) => PutMatchSuccess(matchId, text), PutMatchError);
        }

        private void PutMatchSuccess(string matchId, string text) {
            var response = JsonConvert.DeserializeObject<MatchResultDTO>(text);
            MatchResult matchResult = MatchConverter.Convert(response);
            OnPutMatchResult?.Invoke(matchId, matchResult);
        }

        private void PutMatchError(long responseCode, string text) {
            Debug.LogWarning("failed to put match result. HTTP Response Code " + responseCode + "\n" + text);
            OnError?.Invoke(text);
        }

        #endregion
    }
}