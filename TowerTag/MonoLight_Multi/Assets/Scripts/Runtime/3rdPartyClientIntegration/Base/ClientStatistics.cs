using System;
using System.Collections.Generic;
using UnityEngine;

public class ClientStatistics : TTSingleton<ClientStatistics>
{
    public event Action Stored;
    public bool StoringDone { get; protected set; }

    public ClientStatisticsKeysScriptableObject keys;

    private Dictionary<string, int> _statistics;
    protected void SetStatistics(Dictionary<string, int> statistics) => _statistics = statistics;

    public Dictionary<string, int> Statistics
    {
        get
        {
            if (_statistics == null)
            {
                _statistics = new Dictionary<string, int>()
                {
                    {keys.Matches, 0},
                    {keys.LoginRow, 0},
                    {keys.Win, 0},
                    {keys.WinStreak, 0},
                    {keys.Snipe, 0},
                    {keys.HealedLowPlayer, 0},
                    {keys.HealthHealed, 0},
                    {keys.Claims, 0},
                    {keys.Tele, 0},
                    {keys.Fire, 0},
                    {keys.Ice, 0},
                    {keys.HeadShotsTaken, 0},
                    {keys.MVP, 0},
                    {keys.Lvl, 0}
                };
            }

            return _statistics;
        }
    }

    public virtual void StoreStatisticsInDictionary () 
    {
        StoringDone = true;
        Stored?.Invoke();
    }

    protected override void Init() {}

    protected virtual void Save () {}
    private void OnDestroy() => Save();
}