using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using TowerTag;
using UnityEngine;

namespace Commendations {
    /// <summary>
    /// The commendation controller evaluates match results and assigns <see cref="Commendation"/>, accordingly.
    /// The awarded <see cref="IPlayer"/> is teleported onto the appropriate <see cref="Pillar"/>.
    /// The mirrored pillars are set to display the awarded commendation.
    ///
    /// </summary>
    /// <author>Ole Jürgensen (ole@vrnerds.de)</author>
    public class CommendationsController : MonoBehaviourPunCallbacks {
        [Header("Scene Objects")]
        [SerializeField, Tooltip("Pillars where players are put when awarded. " +
                                 "The order determines how players are placed." +
                                 "The most important award is associated with the first pillar in this list.")]
        private Pillar[] _pillars;

        [Header("Commendations")] [SerializeField, Tooltip("The commendations that can be awarded. The order determines the priority.")]
        private PerformanceBasedCommendation[] _commendations;

        [SerializeField, Tooltip("Commendation awarded to players that win no other commendation. Optional.")]
        private Commendation _defaultCommendation;

        [Header("Visualization")]
        [SerializeField, Tooltip("Prefab for the visual representation of the commendations. " +
                                 "Will be instantiated at the mirrored pillars.")]
        private CommendationView _commendationViewPrefab;

        [Header("Settings")] [SerializeField, Tooltip("Pillars without players or with default commendations will have this height.")]
        private float _pillarDefaultHeight;

        [SerializeField, Tooltip("Pillars are lifted upwards by this value for each value point of the commendation.")]
        private float _pillarHeightStep = 0.4f;

        private ICommendationService _commendationService;

        public delegate void CommendationsDelegate(
            CommendationsController sender, Dictionary<IPlayer, (ICommendation, int)> commendations);

        public event CommendationsDelegate CommendationsAwarded;
        public static event Action<Commendation> LocalPlayerCommendationAwarded;

        private const string CommendationsPropertyName = "awardedCommendations";
        private const string PillarHeightsPropertyName = "pillarHeights";
        private bool _alreadyRewarded;

        private void Awake() {
            _commendationService = ServiceProvider.Get<ICommendationService>();
            _commendationService.SetPlayerManager(PlayerManager.Instance);
        }

        private void Start() {
            // as master, award commendations and update photon room properties accordingly
            if (PhotonNetwork.IsMasterClient) {
                Dictionary<IPlayer, (ICommendation commendation, int place)> awardedCommendations =
                    AwardCommendationsOnMaster();
                var awardedCommendationsHashtable = new Hashtable {
                    {
                        CommendationsPropertyName,
                        awardedCommendations.ToDictionary(kv => kv.Key.PlayerID, kv => kv.Value.commendation.name)
                    }, {
                        PillarHeightsPropertyName,
                        _pillars
                            .Where(pillar => pillar.Owner != null)
                            .ToDictionary(pillar => pillar.ID, pillar => pillar.gameObject.transform.position.y)
                    }
                };
                PhotonNetwork.CurrentRoom.SetCustomProperties(awardedCommendationsHashtable);
            }

            // visualize commendations
            ShowCommendations();
            UpdatePillars();
        }

        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged) {
            if (propertiesThatChanged.ContainsKey(CommendationsPropertyName)) ShowCommendations();
            if (propertiesThatChanged.ContainsKey(PillarHeightsPropertyName)) UpdatePillars();
        }

        /// <summary>
        /// Deserialize and visualize commendations from photon room properties.
        /// </summary>
        private void ShowCommendations() {
            // clear existing commendation visualizations
            FindObjectsOfType<CommendationView>().ForEach(commendationView => Destroy(commendationView.gameObject));

            // deserialize
            var roomPropertyCommendations =
                PhotonNetwork.CurrentRoom.CustomProperties[CommendationsPropertyName] as Dictionary<int, string>;
            if (roomPropertyCommendations == null) return;
            Dictionary<IPlayer, Commendation> awardedCommendations = roomPropertyCommendations
                .Select(kv => (
                    player: PlayerManager.Instance.GetPlayer(kv.Key),
                    commendation: GetCommendation(kv.Value)))
                .Where(tuple => tuple.player != null && tuple.commendation != null)
                .ToDictionary(tuple => tuple.player, tuple => tuple.commendation);

            // instantiate visualizations
            awardedCommendations.ForEach(kv => {
                CommendationView commendationView = InstantiateWrapper.InstantiateWithMessage(_commendationViewPrefab, transform);
                commendationView.Set(kv.Value, kv.Key);
            });

            if (awardedCommendations.Any(pair => pair.Key.IsMe)) {
                if (!_alreadyRewarded) {
                    var ownPlayer = PlayerManager.Instance.GetOwnPlayer();
                    if (ownPlayer != null) LocalPlayerCommendationAwarded?.Invoke(awardedCommendations[ownPlayer]);
                    _alreadyRewarded = true;
                }
            }
        }

        /// <summary>
        /// Deserialize and apply the pillar heights from photon room properties.
        /// </summary>
        private void UpdatePillars() {
            // deserialize
            var pillarHeights =
                PhotonNetwork.CurrentRoom.CustomProperties[PillarHeightsPropertyName] as Dictionary<int, float>;
            if (pillarHeights == null) return;

            // reset pillar heights and update visuals.
            _pillars.ForEach(pillar => {
                float height = pillarHeights.ContainsKey(pillar.ID) ? pillarHeights[pillar.ID] : _pillarDefaultHeight;
                Vector3 pillarPosition = pillar.transform.position;
                pillarPosition = new Vector3(pillarPosition.x, height, pillarPosition.z);
                pillar.gameObject.transform.position = pillarPosition;
                pillar.PillarVisuals.GetComponent<PillarVisualsExtended>()
                    .SetPillarVisualsActive(pillarHeights.ContainsKey(pillar.ID));
            });

            // reset players position, in case the pillar position changed
            PlayerManager.Instance.GetParticipatingPlayers(out var players, out var count);
            for (int i = 0; i < count; i++)
            {
                if (players[i].CurrentPillar == null || players[i].PlayerAvatar == null) continue;
                players[i].PlayerAvatar.TeleportTransform.position = players[i].CurrentPillar.TeleportTransform.position;
            }
        }

        /// <summary>
        /// Among the available commendations, finds the one with the given name.
        /// Returns null if none is found.
        /// </summary>
        /// <param name="commendationName">The name of the commendations</param>
        /// <returns>The commendation with the given name or null</returns>
        private Commendation GetCommendation(string commendationName) {
            if (commendationName == _defaultCommendation.name) return _defaultCommendation;
            return _commendations.FirstOrDefault(commendation => commendation.name == commendationName);
        }

        /// <summary>
        /// Distributes the available commendations among the players based on the match statistics.
        /// Teleports the players to the appropriate pillars.
        /// 
        /// </summary>
        /// <returns></returns>
        private Dictionary<IPlayer, (ICommendation, int)> AwardCommendationsOnMaster() {
            if (!PhotonNetwork.IsMasterClient) {
                return null;
            }

            PlayerManager.Instance.GetParticipatingPlayers(out var players, out var count);
            if (_pillars.Length < count) {
                Debug.LogError("Not enough pillars to place players");
                return null;
            }

            IPerformanceBasedCommendation[] performanceBasedCommendations =
                _commendations.Select(c => (IPerformanceBasedCommendation) c).ToArray();
            Dictionary<IPlayer, (ICommendation, int)> awardedCommendations =
                _commendationService.AwardCommendations(performanceBasedCommendations,
                    players, count, GameManager.Instance.CurrentMatch.Stats,
                    _defaultCommendation);

            foreach (IPlayer player in awardedCommendations.Keys) {
                (ICommendation commendation, int place) = awardedCommendations[player];
                AwardCommendationOnMaster(player, commendation, place);
            }

            CommendationsAwarded?.Invoke(this, awardedCommendations);
            return awardedCommendations;
        }

        /// <summary>
        /// Teleports the player onto the appropriate pillar and reports to Unity analytics.
        /// </summary>
        private void AwardCommendationOnMaster(IPlayer player, ICommendation commendation, int place) {
            if (!PhotonNetwork.IsMasterClient)
                return;

            if (_pillars == null || _pillars.Length == 0) {
                Debug.LogException(new System.Exception("Unable to teleport player to appropriate pillar. References to a set of pillars is invalid."));
                goto earlyout;
            }

            if (place < 0) { 
                Debug.LogException(new System.Exception(string.Format("Unable to teleport player to appropriate pillar. Invalid pillar place index: {0}", place)));
                goto earlyout;
            }

            if (place > _pillars.Length) { 
                Debug.LogException(new System.Exception(string.Format("Unable to teleport player to appropriate pillar. The target place index: {0} is larger then the number of pillars available: {1}", place, _pillars.Length)));
                goto earlyout;
            }

            else { 
                Pillar pillar = _pillars[place];
                pillar.OwningTeamID = player.TeamID;

                Transform pillarTransform = pillar.transform;
                Vector3 pillarPosition = pillarTransform.position;

                float pillarHeight = _pillarDefaultHeight + _pillarHeightStep * commendation.Value;

                pillarTransform.position = new Vector3(pillarPosition.x, pillarHeight, pillarPosition.z);

                TeleportHelper.TeleportPlayerRequestedByGame(
                    player, pillar, TeleportHelper.TeleportDurationType.Immediate);
            }

            earlyout:

            if (PhotonNetwork.IsMasterClient && GameManager.Instance.CurrentMatch != null)
                GameManager.Instance.CurrentMatch.Stats.AddCommendation(player, commendation); // add to stats

            SendAnalyticsEvent(player, commendation);
        }

        /// <summary>
        /// Reports the awarded commendation to unity analytics.
        /// </summary>
        /// <param name="player">The player that was awarded the commendations</param>
        /// <param name="commendation">The awarded commendations</param>
        private static void SendAnalyticsEvent(IPlayer player, ICommendation commendation) {
            MatchStats gameModeStats = GameManager.Instance.CurrentMatch.Stats;
            if (!gameModeStats.GetPlayerStats().ContainsKey(player.PlayerID)) return;
            var playerStats = gameModeStats.GetPlayerStats()[player.PlayerID];
            string commendationName = commendation.DisplayName.Length > 0 ? commendation.DisplayName : "None";

            AnalyticsController.Commendation(
                ConfigurationManager.Configuration.Room,
                player.PlayerName,
                TeamManager.Singleton.Get(player.TeamID).Name,
                commendationName,
                playerStats.Kills,
                playerStats.Deaths,
                playerStats.Assists
            );
        }
    }
}