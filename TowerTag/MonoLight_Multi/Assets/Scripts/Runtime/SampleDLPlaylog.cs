using Membership.Model.v2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SampleDLPlaylog : MonoBehaviour {

    public void OnClickPost()
    {
        var data = new MatchResult
        {
            RegionName = "Japan",
            RoomName = "Photon Room Name",
            StartTime = System.DateTime.Now,
            PlaySeconds = 600.0f,
            MapName = "HighTower",
            WinTeamNo = 1,
            Version = "2018.1.6",

            // Round result (Ex. 3 round data)
            RoundDatas = new List<RoundData> {
                new RoundData { WinTeamNo =1, PlaySeconds = 200 },
                new RoundData { WinTeamNo =0, PlaySeconds = 180 },
                new RoundData { WinTeamNo =1, PlaySeconds = 220 },
            },

            // Player result (Ex. 4 players)
            PlayDatas = new List<PlayData> {
                new PlayData
                {
                    PlayerId = "0",
                    PlayerName = "Player1",
                    TeamNo = 0,
                    KillNum = 1,
                    DeadNum = 1,
                    GetTowerNum = 3,
                    AssistNum = 4,
                    DamagePnt = 10,
                    DamageNum = 1,
                    HitPnt = 11,
                    HitNum = 1,
                    PlaySeconds = 400,
                    MoveTowerNum = 20,
                },
                new PlayData
                {
                    PlayerId = "0",
                    PlayerName = "Player2",
                    TeamNo = 0,
                    KillNum = 1,
                    DeadNum = 1,
                    GetTowerNum = 3,
                    AssistNum = 4,
                    DamagePnt = 10,
                    DamageNum = 1,
                    HitPnt = 11,
                    HitNum = 1,
                    PlaySeconds = 400,
                    MoveTowerNum = 20,
                },
                new PlayData
                {
                    PlayerId = "0",
                    PlayerName = "Player3",
                    TeamNo = 0,
                    KillNum = 1,
                    DeadNum = 1,
                    GetTowerNum = 3,
                    AssistNum = 4,
                    DamagePnt = 10,
                    DamageNum = 1,
                    HitPnt = 11,
                    HitNum = 1,
                    PlaySeconds = 400,
                    MoveTowerNum = 20,
                },
                new PlayData
                {
                    PlayerId = "0",
                    PlayerName = "Player4",
                    TeamNo = 0,
                    KillNum = 1,
                    DeadNum = 1,
                    GetTowerNum = 3,
                    AssistNum = 4,
                    DamagePnt = 10,
                    DamageNum = 1,
                    HitPnt = 11,
                    HitNum = 1,
                    PlaySeconds = 400,
                    MoveTowerNum = 20,
                },
            },
        };

        // Send play log.
        Membership.Client playDataLog = new Membership.Client(this);
        //playDataLog.Request(data);

        // Case Get result
        playDataLog.PostData(data, CallbackPost);
    }

    void CallbackPost(int rc, string text)
    {
        GameObject.Find("TextResult").GetComponent<Text>().text = text;
        Debug.Log(text);
    }
}
