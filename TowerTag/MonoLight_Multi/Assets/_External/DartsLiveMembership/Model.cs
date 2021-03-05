using System;
using UnityEngine;

#pragma warning disable 0414

namespace Membership.Model
{
    /// <summary> data for game </summary>
    [Obsolete("please user Membership.Model.v2")]
    [Serializable]
    public class PlayData : ISerializationCallbackReceiver
    {
        /// <summary> constructor </summary>
        public PlayData()
        {
        }

        /// <summary> name of Photon Room </summary>
        public string room_name = "";

        /// <summary> start time(yyyy-MM-dd HH:mm:ss) </summary>
        [SerializeField]
        private string start_time = "";

        public DateTime start_datetime = System.DateTime.Now;

        /// <summary> name of region </summary>
        public string region = "";

        /// <summary> play time(seconds) </summary>
        public float play_seconds = 0;

        /// <summary> name of Map </summary>
        public string map_name = "HighTower";

        /// <summary> number of the winning team(0 or 1, -1=Draw) </summary>
        public int win_team_no = 0;

        /// <summary> data for rounds (number for rounds) </summary>
        public RoundData[] round_inf;

        /// <summary> data for players (number for players) </summary>
        public PlayerData[] player_inf;

        public void OnAfterDeserialize()
        {
        }
        public void OnBeforeSerialize()
        {
            start_time = start_datetime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary> towertag application version </summary>
        public string version = "";

    }

    [Obsolete("please user Membership.Model.v2")]
    /// <summary> data for round </summary>
    [Serializable]
    public class RoundData
    {
        /// <summary> number of the winning team (0 or 1, -1=Draw) </summary>
        public int win_team_no = 0;

        /// <summary> play time of this round(seconds) </summary>
        public float play_seconds = 0;
    }

    [Obsolete("please user Membership.Model.v2")]
    /// <summary> data for player </summary>
    [Serializable]
    public class PlayerData
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
}
