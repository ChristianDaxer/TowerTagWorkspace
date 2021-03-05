using System.Linq;
using ExitGames.Client.Photon;
using Photon.Realtime;
using TowerTagSOES;
using UI;
using UnityEngine;
using IPlayer = TowerTag.IPlayer;

namespace GameManagement
{
    /// <summary>
    /// Controller for spectator related tasks. The spectator is a <see cref="ControllerType"/> for users that want to
    /// observe a game but neither control it nor participate as a player. Do not mistake for a player that is not
    /// participating and counts as a spectator temporarily.
    /// There is a limit to the number of spectators that can join a room simultaneously. This is to limit the network
    /// traffic required for each user. When joining a room, this controller will check against that limit and trigger
    /// a disconnect if necessary.
    /// </summary>
    [RequireComponent(typeof(ConnectionManager))]
    public class SpectatorController : MonoBehaviour
    {
        private ConnectionManager _connectionManager;
        private IPhotonService _photonService;
        private const string SpectatorKey = "spectator";

        private void Awake()
        {
            _photonService = ServiceProvider.Get<IPhotonService>();
            _connectionManager = GetComponent<ConnectionManager>();
        }

        private void OnEnable()
        {
            _connectionManager.JoinedRoom += OnJoinedRoom;
            PlayerManager.Instance.PlayerRemoved += OnPlayerRemoved;
        }

        private void OnDisable()
        {
            _connectionManager.JoinedRoom -= OnJoinedRoom;
            PlayerManager.Instance.PlayerRemoved -= OnPlayerRemoved;
        }

        private void OnPlayerRemoved(IPlayer player)
        {
            if (!SharedControllerType.Spectator) return;
            if (PlayerManager.Instance.GetParticipatingPlayersCount() <= 0)
            {
                ConnectionManager.Instance.LeaveRoom();
            }
        }

        private void OnJoinedRoom(ConnectionManager connectionManager, IRoom room)
        {
            if (!SharedControllerType.Spectator) return;
            Hashtable playerProperties = _photonService.LocalPlayer.CustomProperties;
            playerProperties[SpectatorKey] = true;
            _photonService.LocalPlayer.SetCustomProperties(playerProperties);
            int spectatorCount = room.Players.Count(kv =>
                kv.Value.CustomProperties.ContainsKey(SpectatorKey) && (bool) kv.Value.CustomProperties[SpectatorKey]);
            UnityEngine.Debug.Log(
                $"Joined room as a spectator with {spectatorCount - 1} other spectators.");
            if (spectatorCount > TowerTagSettings.MaxSpectatorCount)
            {
                ConnectionManager.Instance.Disconnect();
                MessageQueue.Singleton.AddErrorMessage(
                    "You have been kicked, because there are too many spectators in the room.");
            }
        }
    }
}