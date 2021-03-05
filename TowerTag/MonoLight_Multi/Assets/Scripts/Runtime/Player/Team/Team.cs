using System;
using System.Linq;
using UnityEngine;

namespace TowerTag {
    [CreateAssetMenu(menuName = "TowerTag/Team", fileName = "New Team")]
    public class Team : ScriptableObject, ITeam {
        [SerializeField] private TeamColors _colors;
        [SerializeField] private string _defaultName;
        [SerializeField] private string _ragDollDecalName;
        public string Name { get; private set; } = "";
        public string DefaultName => _defaultName;
        public string RagDollDecalName => _ragDollDecalName;
        public TeamID ID { get; set; }

        public int GetPlayerCount()
        {
            if (ID == TeamID.Fire)
                return PlayerManager.Instance.GetParticipatingFirePlayerCount();
            return PlayerManager.Instance.GetParticipatingIcePlayerCount();
        }

        public void GetPlayers(out IPlayer[] players, out int count)
        {
            if (ID == TeamID.Fire)
                PlayerManager.Instance.GetParticipatingFirePlayers(out players, out count);
            else PlayerManager.Instance.GetParticipatingIcePlayers(out players, out count);
        }

        public void PlayersWithoutAI(out IPlayer[] nonAIPlayers, out int playerCount)
        {
            if (ID == TeamID.Fire)
               PlayerManager.Instance.GetParticipatingHumanFirePlayers(out nonAIPlayers, out playerCount);
            else PlayerManager.Instance.GetParticipatingHumanIcePlayers(out nonAIPlayers, out playerCount);
        }

        public int PlayerCountWithoutAI()
        {
            IPlayer[] nonAIPlayers = null;
            int playerCount = 0;
            if (ID == TeamID.Fire)
               PlayerManager.Instance.GetParticipatingHumanFirePlayers(out nonAIPlayers, out playerCount);
            else PlayerManager.Instance.GetParticipatingHumanIcePlayers(out nonAIPlayers, out playerCount);
            return playerCount;
        }

        public TeamColors Colors {
            get => _colors;
            set => _colors = value;
        }

        public event TeamNameChangeDelegate NameChanged;

        private void OnEnable() {
            Name = _defaultName;
        }

        public void SetName(string newName) {
            Name = newName ?? throw new ArgumentNullException(nameof(newName));
            NameChanged?.Invoke(this, newName);
        }
    }
}