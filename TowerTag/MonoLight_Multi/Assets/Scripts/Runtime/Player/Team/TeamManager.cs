using Photon.Pun;
using SOEventSystem.Shared;
using UnityEngine;

namespace TowerTag
{
    [CreateAssetMenu(menuName = "TowerTag/Team Manager", fileName = "TeamManager")]
    public class TeamManager : ScriptableObjectSingleton<TeamManager>, ITeamManager
    {
        [SerializeField] private Team _teamNeutral;
        [SerializeField] private Team _teamIce;
        [SerializeField] private Team _teamFire;
        [SerializeField] private TeamMaterialManager _teamMaterialManager;
        [SerializeField] private TeamColorManager _teamColorManager;

        public ITeam TeamNeutral => _teamNeutral;
        public ITeam TeamFire => _teamFire;
        public ITeam TeamIce => _teamIce;

        private void OnValidate()
        {
            Init();
        }

        private void OnEnable()
        {
            Init();
        }

        public void Init()
        {
            if (_teamNeutral != null) TeamNeutral.ID = TeamID.Neutral;
            if (_teamFire != null) TeamFire.ID = TeamID.Fire;
            if (_teamIce != null) TeamIce.ID = TeamID.Ice;
            _teamMaterialManager.Init();
            _teamColorManager.Init();
        }

        public ITeam Get(TeamID id)
        {
            switch (id)
            {
                case TeamID.Neutral:
                    return _teamNeutral;
                case TeamID.Fire:
                    return _teamFire;
                case TeamID.Ice:
                    return _teamIce;
                default:
                    Debug.LogError($"Invalid TeamID {id}");
                    return null;
            }
        }

        /// <summary>
        /// Returns the team ID of the smaller team and returns Fire if the teams are even
        /// </summary>
        /// <param name="isTeamFull"></param>
        /// <returns></returns>
        public TeamID GetIdOfSmallerTeam(out bool isTeamFull) {
            var fireTeam = Get(TeamID.Fire);
            var iceTeam = Get(TeamID.Ice);

            fireTeam.GetPlayers(out var firePlayers, out var firePlayerCount);
            iceTeam.GetPlayers(out var icePlayers, out var icePlayerCount);

            TeamID smallerTeam = firePlayerCount <= icePlayerCount ? TeamID.Fire : TeamID.Ice;
            isTeamFull = IsTeamFull(smallerTeam);
            return smallerTeam;
        }

        public TeamID GetEnemyTeamIDOfPlayer(IPlayer player)
        {
            return player.TeamID == TeamID.Fire ? TeamID.Ice : TeamID.Fire;
        }

        public bool IsTeamFull(TeamID teamID)
        {
            Team team = teamID == TeamID.Fire ? _teamFire : _teamIce;
            if (PhotonNetwork.CurrentRoom != null
                && PhotonNetwork.CurrentRoom.CustomProperties != null
                && team != null)
            {
                if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(RoomPropertyKeys.MaxPlayers)) {
                    return team.GetPlayerCount() >=
                           (byte) PhotonNetwork.CurrentRoom.CustomProperties[RoomPropertyKeys.MaxPlayers] / 2;
                }
                else return team.GetPlayerCount() >= PhotonNetwork.CurrentRoom.MaxPlayers;
            }

            Debug.LogError(
                "TeamManager:IsTeamFull -> Photon Network Current Room or Custom Room Props or current team null. This should not have happened.");
            return true;
        }
    }
}