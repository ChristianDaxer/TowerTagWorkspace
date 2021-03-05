using System;
using JetBrains.Annotations;
using TowerTag;

public delegate void TeamNameChangeDelegate([NotNull] ITeam team, [NotNull] string newName);

namespace TowerTag {
    public interface ITeam {
        [NotNull]
        string Name { get; }
        TeamID ID { get; set; }

        int GetPlayerCount();
        void GetPlayers(out IPlayer[] players, out int count);

        void PlayersWithoutAI(out IPlayer[] nonAIPlayers, out int playerCount);
        int PlayerCountWithoutAI();

        TeamColors Colors { get; set; }

        event TeamNameChangeDelegate NameChanged;

        void SetName([NotNull] string newName);
    }

    [Serializable]
    public enum TeamID {
        Neutral = -1,
        Fire = 0,
        Ice = 1
    }

    public static class StreamExtension {
        public static bool Serialize(this BitSerializer serializer, ref TeamID teamID) {
            var temp = (int) teamID;
            if (!serializer.Serialize(ref temp, BitCompressionConstants.MinTeamID, BitCompressionConstants.MaxTeamID))
                return false;
            if (serializer.IsReading) teamID = (TeamID) temp;
            return true;
        }
    }
}