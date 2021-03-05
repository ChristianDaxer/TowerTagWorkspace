using Newtonsoft.Json;
using REST;
using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Toornament.Converter;
using Toornament.DataTransferObject;
using Toornament.Store.Model;
using VRNerdsUtilities;

namespace Toornament.Store {
    public class TournamentStore : SingletonMonoBehaviour<TournamentStore> {
        private const string ServerUrl = "https://api.toornament.com";

        // cached data
        private readonly Dictionary<string, Tournament> _tournaments = new Dictionary<string, Tournament>();
        private readonly Dictionary<string, Tournament> _myTournaments = new Dictionary<string, Tournament>();

        // events to subscribe on
        public event Action<Tournament[]> OnGetTournaments;
        public event Action<Tournament> OnGetTournament;
        public event Action<Tournament> OnCreateTournament;
        public event Action<Tournament> OnUpdateTournament;
        public event Action<string> OnDeleteTournament;
        public event Action<string> OnError;

        private void Start() {
            transform.parent = ToornamentContainer.Instance.transform;
        }

        #region GetTournaments

        // ReSharper disable once InconsistentNaming
        public void GetTournaments(string discipline, string afterISO8601) {
            OnGetTournaments?.Invoke(_tournaments.Values
                .Where(tournament => tournament.discipline.Equals(discipline))
                // TODO filter by date
                .ToArray());
            string route = "/v1/tournaments"
                           + "?discipline=" + discipline
                           + "&after_end=" + afterISO8601;
            var uri = new Uri(new Uri(ServerUrl), route);
            Client.Get(uri.ToString(), AuthStore.Instance.NoAuthHeaders, GetTournamentsSuccess, GetTournamentsError);
        }

        public void GetTournaments() {
            OnGetTournaments?.Invoke(_tournaments.Values.ToArray());
            const string route = "/v1/tournaments";
            var uri = new Uri(new Uri(ServerUrl), route);
            Client.Get(uri.ToString(), AuthStore.Instance.NoAuthHeaders, GetTournamentsSuccess, GetTournamentsError);
        }

        private void GetTournamentsSuccess(long responseCode, string text) {
            var tournamentsDTO = JsonConvert.DeserializeObject<TournamentDTO[]>(text);
            Tournament[] newTournaments = tournamentsDTO
                .Select(TournamentConverter.Convert)
                .ToArray();
            foreach (Tournament tournament in newTournaments) {
                _tournaments[tournament.id] = tournament;
            }

            OnGetTournaments?.Invoke(newTournaments);
        }

        private void GetTournamentsError(long responseCode, string text) {
            Debug.LogWarning("Failed to get tournaments: " + responseCode + "\n" + text);
            OnError?.Invoke(text);
        }

        #endregion

        #region GetMyTournaments

        public void GetMyTournaments() {
            OnGetTournaments?.Invoke(_myTournaments.Values.ToArray());
            const string route = "/v1/me/tournaments";
            var uri = new Uri(new Uri(ServerUrl), route);
            Client.Get(uri.ToString(), AuthStore.Instance.AuthHeaders, GetMyTournamentsSuccess, GetMyTournamentsError);
        }

        private void GetMyTournamentsSuccess(long responseCode, string text) {
            var tournamentsDTO = JsonConvert.DeserializeObject<TournamentDTO[]>(text);
            Tournament[] newMyTournaments = tournamentsDTO
                .Select(TournamentConverter.Convert)
                .ToArray();
            foreach (Tournament tournament in newMyTournaments) {
                _myTournaments[tournament.id] = tournament;
            }

            OnGetTournaments?.Invoke(newMyTournaments);
        }

        private void GetMyTournamentsError(long responseCode, string text) {
            Debug.LogWarning("Failed to get my tournaments: " + responseCode + "\n" + text);
            OnError?.Invoke(text);
        }

        #endregion

        #region GetTournament

        public void GetTournament(string id) {
            if (OnGetTournaments != null && _tournaments.ContainsKey(id))
                OnGetTournaments(new[] { _tournaments[id] });
            string route = "/v1/tournaments/" + id;
            var uri = new Uri(new Uri(ServerUrl), route);
            Client.Get(uri.ToString(), AuthStore.Instance.NoAuthHeaders, GetTournamentSuccess, GetTournamentError);
        }

        private void GetTournamentSuccess(long responseCode, string text) {
            var tournamentDTO = JsonConvert.DeserializeObject<TournamentDTO>(text);
            Tournament tournament = TournamentConverter.Convert(tournamentDTO);
            _tournaments[tournament.id] = tournament;
            OnGetTournament?.Invoke(tournament);
        }

        private void GetTournamentError(long responseCode, string text) {
            Debug.LogWarning("Failed to get tournament: " + responseCode + "\n" + text);
            OnError?.Invoke(text);
        }

        #endregion

        #region CreateTournament

        public void CreateTournament(CreateTournamentDTO tournament) {
            const string route = "/v1/tournaments";
            string data = JsonConvert.SerializeObject(tournament);
            var uri = new Uri(new Uri(ServerUrl), route);
            Client.Post(uri.ToString(), AuthStore.Instance.AuthHeaders, data,
                CreateTournamentSuccess, CreateTournamentError);
        }

        private void CreateTournamentSuccess(long responseCode, string text) {
            var tournamentDTO = JsonConvert.DeserializeObject<TournamentDTO>(text);
            Tournament tournament = TournamentConverter.Convert(tournamentDTO);
            _tournaments[tournament.id] = tournament;
            OnCreateTournament?.Invoke(tournament);
        }

        private void CreateTournamentError(long responseCode, string text) {
            Debug.LogWarning("Failed to create tournament: " + responseCode + "\n" + text);
            OnError?.Invoke(text);
        }

        #endregion

        #region PatchTournament

        public void PatchTournament(string tournamentId, Tournament tournament) {
            string route = "/v1/tournaments" + "/" + tournamentId;
            string data = JsonConvert.SerializeObject(TournamentConverter.ConvertToCreationDTO(tournament));
            var uri = new Uri(new Uri(ServerUrl), route);
            Client.Patch(uri.ToString(), AuthStore.Instance.AuthHeaders, data,
                PatchTournamentSuccess, PatchTournamentError);
        }

        private void PatchTournamentSuccess(long responseCode, string text) {
            var tournamentDTO = JsonConvert.DeserializeObject<TournamentDTO>(text);
            Tournament tournament = TournamentConverter.Convert(tournamentDTO);
            _tournaments[tournament.id] = tournament;
            OnUpdateTournament?.Invoke(tournament);
        }

        private void PatchTournamentError(long responseCode, string text) {
            Debug.LogWarning("Failed to update tournament: " + responseCode + "\n" + text);
            OnError?.Invoke(text);
        }

        #endregion

        #region DeleteTournament
        [UsedImplicitly]
        public void DeleteTournament(string tournamentId) {
            string route = "/v1/tournaments/" + tournamentId;
            var uri = new Uri(new Uri(ServerUrl), route);
            Client.Delete(uri.ToString(), AuthStore.Instance.AuthHeaders,
                (responseCode, text) => DeleteTournamentSuccess(tournamentId, responseCode, text),
                DeleteTournamentError);
        }

        // ReSharper disable twice UnusedParameter.Local
        private void DeleteTournamentSuccess(string tournamentId, long responseCode, string text) {
            OnDeleteTournament?.Invoke(tournamentId);
        }

        private void DeleteTournamentError(long responseCode, string text) {
            Debug.LogWarning("Failed to delete tournament: " + responseCode + "\n" + text);
            OnError?.Invoke(text);
        }

        #endregion
    }
}