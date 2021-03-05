using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using REST;
using Toornament.Converter;
using Toornament.DataTransferObject;
using Toornament.Store.Model;
using VRNerdsUtilities;

// ReSharper disable CollectionNeverQueried.Local

namespace Toornament.Store {
    public class ParticipantStore : SingletonMonoBehaviour<ParticipantStore> {
        private const string ServerUrl = "https://api.toornament.com";

        // cached data
        private readonly Dictionary<string, Participant> _participants = new Dictionary<string, Participant>();

        // events to subscribe on
        public event Action<Participant> OnUpdateParticipant;
        public event Action<Participant> OnRegisterParticipant;
        public event Action<Participant[]> OnGetParticipants;
        public event Action<string> OnError;

        private void Start() {
            transform.parent = ToornamentContainer.Instance.transform;
        }

        #region RegisterParticipant

        public void RegisterParticipant(RegisterParticipantDTO registerParticipantDTO) {
            const string route = "/participant/v2/me/registrations";
            var uri = new Uri(new Uri(ServerUrl), route);
            string data = JsonConvert.SerializeObject(registerParticipantDTO);
            Client.Patch(uri.ToString(), AuthStore.Instance.AuthHeaders, data, RegisterParticipantsSuccess,
                RegisterParticipantError);
        }

        private void RegisterParticipantsSuccess(long responseCode, string text) {
            var result = JsonConvert.DeserializeObject<ParticipantDTO>(text);
            Participant participant = ParticipantConverter.Convert(result);
            _participants[participant.id] = participant;
            OnRegisterParticipant?.Invoke(participant);
        }

        private void RegisterParticipantError(long responseCode, string text) {
            Debug.LogWarning("Failed to register participant " + responseCode + "\n" + text);
            OnError?.Invoke(text);
        }

        #endregion

        #region GetParticipants

        public void GetParticipants(string tournamentId) {
            string route = "/v1/tournaments" + "/" + tournamentId + "/" + "/participants?with_lineup=1";
            var uri = new Uri(new Uri(ServerUrl), route);
            Client.Get(uri.ToString(), AuthStore.Instance.NoAuthHeaders, GetParticipantsSuccess, GetParticipantsError);
            // TODO call event eagerly, need to cache participants by tournament id...
        }

        private void GetParticipantsSuccess(long responseCode, string text) {
            Participant[] participants = JsonConvert.DeserializeObject<ParticipantDTO[]>(text)
                .Select(ParticipantConverter.Convert)
                .ToArray();
            foreach (Participant participant in participants) {
                _participants[participant.id] = participant;
            }

            OnGetParticipants?.Invoke(participants);
        }

        private void GetParticipantsError(long responseCode, string text) {
            Debug.LogWarning("Failed to get participant " + responseCode + "\n" + text);
            OnError?.Invoke(text);
        }

        #endregion

        #region PatchParticipant

        public void PatchParticipant(string tournamentId, string participantId, CreateParticipantDTO participantDTO) {
            string route = "/v1/tournaments" + "/" + tournamentId + "/" + "participants" + "/" + participantId;
            var uri = new Uri(new Uri(ServerUrl), route);
            string data = JsonConvert.SerializeObject(participantDTO);
            Client.Patch(uri.ToString(), AuthStore.Instance.AuthHeaders, data, PatchParticipantSuccess,
                PatchParticipantError);
        }

        private void PatchParticipantSuccess(long responseCode, string text) {
            var result = JsonConvert.DeserializeObject<ParticipantDTO>(text);
            Participant participant = ParticipantConverter.Convert(result);
            _participants[result.id] = participant;
            OnUpdateParticipant?.Invoke(participant);
        }

        private void PatchParticipantError(long responseCode, string text) {
            Debug.LogWarning("Failed to patch participant. HTTP Response Code " + responseCode + "\n" + text);
            OnError?.Invoke(text);
        }

        #endregion
    }
}