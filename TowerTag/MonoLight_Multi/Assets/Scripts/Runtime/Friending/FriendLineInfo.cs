using Home.UI;
using TowerTagAPIClient.Model;

namespace Runtime.Friending
{
    public class FriendLineInfo
    {
        public ulong UserId { get; }
        public string Name { get; set; }
        public bool IsInGame { get; set; }
        public bool IsInRoom { get; set; }
        public string RoomName { get; set; }
        public RoomLine.RoomLineData RoomInfo{get; set; }
        public PlayerStatistics FriendsPlayerStatistics { get; set; }

        public FriendLineInfo(ulong userId)
        {
            UserId = userId;
        }

        public FriendLineInfo(ulong userId, string name, bool isInGame, bool isInRoom, string roomName, PlayerStatistics statistics)
        {
            UserId = userId;
            Name = name;
            IsInGame = isInGame;
            IsInRoom = isInRoom;
            RoomName = roomName;
            FriendsPlayerStatistics = statistics;
        }
    }
}