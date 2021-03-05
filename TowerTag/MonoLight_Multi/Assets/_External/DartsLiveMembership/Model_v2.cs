using System;
using System.Collections.Generic;

#pragma warning disable 0414

namespace Membership.Model.v2
{
    /// <summary> data for game </summary>
    [Serializable]
    public class PlayData_v0 : PlayDataBase_v0
    {
        /// <summary> data for rounds (number for rounds) </summary>
        public RoundData_v0[] round_inf = new RoundData_v0[0];

        /// <summary> data for players (number for players) </summary>
        public PlayerData_v0[] player_inf = new PlayerData_v0[0];

        public PlayDataBase_v0 CreatePlayDataBase()
        {
            return new PlayDataBase_v0
            {
                playgame_id = playgame_id,
                CreatedTimestamp = CreatedTimestamp,

                room_name = room_name,
                start_datetime = start_datetime,
                region_name = region_name,
                play_seconds = play_seconds,
                map_name = map_name,
                win_team_no = win_team_no,
                version = version
            };
        }
    }


    public class PlayGameId_v0
    {
        public string playgame_id { get; set; }
        public DateTime CreatedTimestamp { get; set; }
    }


    [Serializable]
    public class PlayDataBase_v0 : PlayGameId_v0
    {
        /// <summary> name of Photon Room </summary>
        public string room_name = "";

        /// <summary> start time(yyyy-MM-dd HH:mm:ss) </summary>
        private string start_time = "";

        public DateTime start_datetime = System.DateTime.Now;

        /// <summary> name of region </summary>
        public string region_name = "";

        /// <summary> play time(seconds) </summary>
        public float play_seconds = 0;

        /// <summary> name of Map </summary>
        public string map_name = "HighTower";

        /// <summary> number of the winning team(0 or 1, -1=Draw) </summary>
        public int win_team_no = 0;

        /// <summary> towertag application version </summary>
        public string version = "";

    }

    /// <summary> data for round </summary>
    [Serializable]
    public class RoundData_v0 : PlayGameId_v0
    {
        public int round_id = 0;

        /// <summary> number of the winning team (0 or 1, -1=Draw) </summary>
        public int win_team_no = 0;

        /// <summary> play time of this round(seconds) </summary>
        public float play_seconds = 0;
    }

    /// <summary> data for player </summary>
    [Serializable]
    public class PlayerData_v0 : PlayGameId_v0
    {
        /// <summary> name of this player </summary>
        public string player_name = "";

        /// <summary> number of this team(0 or 1) </summary>
        public int team_no = 0;

        /// <summary> number of the membership uuid for this player (0 = no data) </summary>
        public long player_id = 0;

        /// <summary> total number of kill </summary>
        public int kill_num = 0;

        /// <summary> total number of dead </summary>
        public int dead_num = 0;

        /// <summary> total number of assist </summary>
        public int assist_num = 0;

        /// <summary> total point damaged </summary>
        public float damage_pnt = 0.0f;

        /// <summary> total times damaged </summary>
        public int damage_num = 0;

        /// <summary> total points to hit enemies </summary>
        public float hit_pnt = 0.0f;

        /// <summary> total times to hit enemies </summary>
        public int hit_num = 0;

        /// <summary> total lifetime(seconds) </summary>
        public float play_seconds = 0.0f;

        /// <summary> total times to change the color of towers</summary>
        public int get_tower_num = 0;

        /// <summary> move tower count </summary>
        public int move_tower_num = 0;
    }


    [Serializable]
    public class MatchResult : MatchData
    {
        //        [DynamoDBProperty("Id")]
        public string Id = "0";

        //        [DynamoDBProperty("Kind")]
        public string Kind { get; set; }

        public string PlayerId { get; set; }

        public DateTime Dated = DateTime.Now;
    }

    [Serializable]
    public class MatchData
    {
        /// <summary> name of region </summary>
        public string RegionName = "";

        /// <summary> start time(yyyy-MM-dd HH:mm:ss) </summary>
        public DateTime StartTime = DateTime.Now;

        /// <summary> name of Photon Room </summary>
        public string RoomName = "";

        /// <summary> name of Map </summary>
        public string MapName = "HighTower";

        /// <summary> play time(seconds) </summary>
        public float PlaySeconds = 0;

        /// <summary> number of the winning team(0 or 1, -1=Draw) </summary>
        public int WinTeamNo = 0;

        /// <summary> towertag application version </summary>
        public string Version = "";

        /// <summary> data for rounds (number for rounds) </summary>
        public List<RoundData> RoundDatas = new List<RoundData>();

        /// <summary> data for players (number for players) </summary>
        public List<PlayData> PlayDatas = new List<PlayData>();
    }

    /// <summary> data for round </summary>
    [Serializable]
    public class RoundData
    {
        /// <summary> number of the winning team (0 or 1, -1=Draw) </summary>
        public int WinTeamNo = 0;

        /// <summary> play time of this round(seconds) </summary>
        public float PlaySeconds = 0;
    }

    /// <summary> data for player </summary>
    [Serializable]
    public class PlayData
    {
        /// <summary> number of the membership uuid for this player (0 = no data) </summary>
        public string PlayerId = "0";

        /// <summary> name of this player </summary>
        public string PlayerName = "";

        /// <summary> number of this team(0 or 1) </summary>
        public int TeamNo = 0;

        /// <summary> total number of kill </summary>
        public int KillNum = 0;

        /// <summary> total number of dead </summary>
        public int DeadNum = 0;

        /// <summary> total number of assist </summary>
        public int AssistNum = 0;

        /// <summary> total point damaged </summary>
        public float DamagePnt = 0.0f;

        /// <summary> total times damaged </summary>
        public int DamageNum = 0;

        /// <summary> total points to hit enemies </summary>
        public float HitPnt = 0.0f;

        /// <summary> total times to hit enemies </summary>
        public int HitNum = 0;

        /// <summary> total lifetime(seconds) </summary>
        public float PlaySeconds = 0.0f;

        /// <summary> total times to change the color of towers</summary>
        public int GetTowerNum = 0;

        /// <summary> move tower count </summary>
        public int MoveTowerNum = 0;

        // added by VR-Nerds
        public int ShotsFired = 0;

        public int Headshots = 0;
        
        public int Snipershots = 0;
        
        public int DoubleKills = 0;

        public string Commendation = "";

        public int GoalPillarsClaimed;

        public int HealingDone;

        public int HealingReceived;
    }

    [Serializable]
    public class UserProfile
    {
        public string UserId = "0";

        /// <summary> name of Photon Room </summary>
        public string Name = "UserName";

        public DateTime CreatedTimestamp { get; internal set; }
    }
}
