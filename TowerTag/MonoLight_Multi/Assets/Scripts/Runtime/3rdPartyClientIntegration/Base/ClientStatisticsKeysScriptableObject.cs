using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ClientStatisticsKeys.asset", menuName = "ScriptableObjects/Create Client Statistics Keys", order = 0)]
public class ClientStatisticsKeysScriptableObject : ScriptableObject
{
    public string Matches = "MATCHES";
    public string LoginRow = "LOGIN";
    public string Win = "WIN";
    public string WinStreak = "WINSTREAK";
    public string Snipe = "SNIPE";
    public string HealedLowPlayer = "LOWHEAL";
    public string HealthHealed = "PARA";
    public string Claims = "TOWER";
    public string Tele = "TELE";
    public string Fire = "FIRE";
    public string Ice = "ICE";
    public string HeadShotsTaken = "HEAD_R";
    public string MVP = "MVP";
    public string Lvl = "LVL";
}
