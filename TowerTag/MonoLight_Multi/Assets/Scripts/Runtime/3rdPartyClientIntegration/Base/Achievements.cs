using System.Collections.Generic;

public class Achievements : TTSingleton<Achievements>
{
    public AchievementsKeysScriptableObject Keys;
    private List<string> _list = new List<string>();
    public List<string> List
    {
        get
        {
            if (_list == null)
            {
                _list = new List<string>()
                {
                    Keys.TutorialDone,
                    Keys.PlayTime,
                    Keys.LogInRow,
                    Keys.WonSingleMatch,
                    Keys.WinStreak5,
                    Keys.WinStreak10,
                    Keys.Sniper,
                    Keys.HealLowPlayers,
                    Keys.Parademic,
                    Keys.ControlFreak,
                    Keys.VRLegs,
                    Keys.HottestFire,
                    Keys.ColdestIce,
                    Keys.HeadShotsTaken,
                    Keys.MVP,
                    Keys.Play50,
                    Keys.Play100,
                    Keys.Play200,
                    Keys.Lvl10,
                    Keys.Lvl20,
                    Keys.Lvl30,
                };
            }

            return _list;
        }
    }

    protected override void Init() {}
}
