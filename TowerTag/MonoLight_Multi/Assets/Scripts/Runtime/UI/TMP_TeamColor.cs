using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TowerTag;
using UnityEngine;

public class TMP_TeamColor : MonoBehaviour
{
    [SerializeField] private TeamID _team = TeamID.Ice;
    [SerializeField] private bool _isDark;
    
    private void Start()
    {
        Color col;

        if (_team.Equals(TeamID.Ice))
        {
            col = _isDark ? TeamManager.Singleton.TeamIce.Colors.DarkUI : TeamManager.Singleton.TeamIce.Colors.UI;
        } else if (_team.Equals(TeamID.Fire))
        {
            col = _isDark ? TeamManager.Singleton.TeamFire.Colors.DarkUI : TeamManager.Singleton.TeamFire.Colors.UI;
        }
        else
        {
            col = _isDark ? TeamManager.Singleton.TeamNeutral.Colors.DarkUI : TeamManager.Singleton.TeamNeutral.Colors.UI;
        }
        
        GetComponent<TMP_Text>().color = col;
    }
}
