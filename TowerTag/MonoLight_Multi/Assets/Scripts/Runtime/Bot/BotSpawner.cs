using System;
using System.Collections.Generic;
using AI;
using TowerTag;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;

public class BotSpawner : MonoBehaviour
{
    // team evening is disabled for any player with the default name of DefaultDebugBotName.
    // this can be a potential threat.
    // maybe set to new guid on every runtime entry to avoid exploitation 
    public static string DefaultDebugBotName = "Debug Bot";
    private bool _showIMGUI;

    private static readonly string[] _teams = {"Fire", "Ice"};
    private static readonly string[] _difficulties;
    private int _teamsIndex;
    private int _difficultyIndex = 1;
    private bool _enabled;

    private static readonly GUIStyle BotIceStyleCenteredBold = new GUIStyle();

    private static readonly GUIStyle BotIceStyleBold = new GUIStyle();
    private static readonly GUIStyle BotFireStyleBold = new GUIStyle();
    private static readonly GUIStyle BotIceStyle = new GUIStyle();
    private static readonly GUIStyle BotFireStyle = new GUIStyle();
    private static readonly GUIStyle KDAStyle = new GUIStyle();

    private bool _spawnSettingsOpen;
    private bool _teamViewOpen;
    private bool _diffEventLogOpen;

    private Vector2 _scrollViewPos = Vector2.zero;
    private List<string> _logBuffer = new List<string>();
    private Dictionary<IPlayer, float> _kdaCache = new Dictionary<IPlayer, float>();
    private float _averageKDA = 0f;

    static BotSpawner()
    {
        _difficulties = Enum.GetValues(typeof(BotBrain.BotDifficulty))
            .Cast<BotBrain.BotDifficulty>()
            .Select(x => x.ToString())
            .ToArray();
    }

    private void Start()
    {
        _enabled = Debug.isDebugBuild;

        BotIceStyle.normal.textColor = TeamManager.Singleton.TeamIce.Colors.UI;
        BotIceStyle.alignment = TextAnchor.MiddleLeft;

        BotFireStyle.normal.textColor = TeamManager.Singleton.TeamFire.Colors.UI;
        BotFireStyle.alignment = TextAnchor.MiddleLeft;

        BotIceStyleBold.fontStyle = FontStyle.Bold;
        BotIceStyleBold.normal.textColor = TeamManager.Singleton.TeamIce.Colors.UI;

        BotFireStyleBold.fontStyle = FontStyle.Bold;
        BotFireStyleBold.normal.textColor = TeamManager.Singleton.TeamFire.Colors.UI;

        BotIceStyleCenteredBold.fontStyle = FontStyle.Bold;
        BotIceStyleCenteredBold.normal.textColor = TeamManager.Singleton.TeamIce.Colors.UI;
        BotIceStyleCenteredBold.alignment = TextAnchor.MiddleCenter;

        KDAStyle.normal.textColor = new Color(255, 255, 255, 255);
        KDAStyle.alignment = TextAnchor.MiddleLeft;

        Application.logMessageReceived += OnLogMessageReceived;
        BotBrain.OnCalculatedAverageKDA += OnReceivingAverageKDA;
        BotBrain.OnCalculatedKDA += OnReceivingPlayerKDA;
        TTSceneManager.Instance.CommendationSceneLoaded += OnCommendationSceneLoaded;
    }

    private void Update()
    {
        if (!_enabled) return;

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.B))
        {
            _showIMGUI = !_showIMGUI;
        }
    }

    private void OnGUI()
    {
        if (!_enabled) return;
        if (!_showIMGUI) return;

        GUILayout.BeginVertical("Debug Bot Management", "window");
        GUILayout.Space(3);

        if (GUILayout.Button(_spawnSettingsOpen ? "v Spawn Settings" : "> Spawn Settings"))
        {
            _spawnSettingsOpen = !_spawnSettingsOpen;
        }

        if (_spawnSettingsOpen) DrawSpawnBots();

        if (GUILayout.Button(_teamViewOpen ? "v Team Difficulty Settings" : "> Team Difficulty Settings"))
        {
            _teamViewOpen = !_teamViewOpen;
        }

        if (_teamViewOpen) DrawTeamDifficulties();

        if (GUILayout.Button(_diffEventLogOpen ? "v Difficulty Change Log" : "> Difficulty Change Log"))
        {
            _diffEventLogOpen = !_diffEventLogOpen;
        }

        if (_diffEventLogOpen) DrawDifficultyEventLog();
    }

    private void DrawDifficultyEventLog()
    {
        GUILayout.BeginVertical("box");
        _scrollViewPos = GUILayout.BeginScrollView(_scrollViewPos);

        foreach (var log in _logBuffer)
        {
            GUILayout.Label(log);
        }

        GUILayout.EndScrollView();
        if (GUILayout.Button("Clear"))
        {
            _logBuffer.Clear();
        }

        GUILayout.EndVertical();
    }

    private void DrawSpawnBots()
    {
        GUILayout.BeginVertical("box");
        _teamsIndex = DrawSelection(_teams, _teamsIndex);
        _difficultyIndex = DrawSelection(_difficulties, _difficultyIndex);
        if (GUILayout.Button("Spawn Bot"))
        {
            IPlayer bot = BotManager.Instance.AddBot((TeamID) _teamsIndex, (BotBrain.BotDifficulty) _difficultyIndex);
            bot.IsBot = true;
            bot.DefaultName = DefaultDebugBotName;
        }

        GUILayout.EndVertical();
    }

    private void DrawTeamDifficulties()
    {
        GUILayout.BeginVertical("box");

        PlayerManager.Instance.GetParticipatingPlayers(out var players, out var count);

        if (players.Any())
        {
            GUILayout.BeginVertical("box");
            GUILayout.Label("Team Ice", BotIceStyleBold);
            GUILayout.Space(3f);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Player Name");
            GUILayout.FlexibleSpace();
            GUILayout.Label("Difficulty");
            GUILayout.FlexibleSpace();
            GUILayout.Label("KDA");

            GUILayout.EndHorizontal();

            PlayerManager.Instance.GetParticipatingFirePlayers(out var firePlayers, out var fireCount);
            for (int i = 0; i < fireCount; i++)
                DrawPlayerLabel(firePlayers[i], BotIceStyle, BotFireStyle);

            GUILayout.Space(10f);
            GUILayout.Label("Team Fire", BotFireStyleBold);
            GUILayout.Space(3f);

            PlayerManager.Instance.GetParticipatingIcePlayers(out var icePlayers, out var iceCount);
            for (int i = 0; i < iceCount; i++)
                DrawPlayerLabel(icePlayers[i], BotIceStyle, BotFireStyle);

            GUILayout.EndVertical();
        }

        GUILayout.Label($"Average KDA: {_averageKDA}", BotIceStyleCenteredBold);

        GUILayout.EndVertical();
    }

    private void DrawPlayerLabel(IPlayer player, GUIStyle style, GUIStyle buttonStyle)
    {
       

        GUILayout.BeginHorizontal();
        GUILayout.Label($"{player.PlayerName}", style);

        GUILayout.FlexibleSpace();

        if (player.IsBot)
        {
            BotBrain brain = player.GameObject.transform.GetChild(1).GetComponent<BotBrain>();
            BotBrain.BotDifficulty diff = brain
                .Difficulty;

            
            if (GUILayout.Button("<", buttonStyle, GUILayout.Width(20f), GUILayout.Height(18f)))
            {
                if ((int) (diff - 1) <= 4 && (diff - 1) >= 0)
                {
                    diff--;
                    brain.SetAIParameters(diff);
                }
            }

            GUILayout.Label($"{diff}", style);
            GUILayout.Space(10f);
            if (GUILayout.Button(">", buttonStyle, GUILayout.Width(20f), GUILayout.Height(18f)))
            {
                if ((int) (diff + 1) <= 4 && (diff + 1) >= 0)
                {
                    diff++;
                    brain.SetAIParameters(diff);
                }
            }
        }
        else
        {
            GUILayout.Label($"Player", style);
        }

        GUILayout.FlexibleSpace();
        GUILayout.Label($"{(_kdaCache.ContainsKey(player) ? _kdaCache[player] : 0f):F2}", KDAStyle,
            GUILayout.Width(30f));

        GUILayout.EndHorizontal();
    }

    private int DrawSelection(string[] selectors, int selected)
    {
        GUILayout.BeginVertical("box");
        GUILayout.BeginHorizontal();

        for (int i = 0; i < selectors.Length; i++)
        {
            GUI.enabled = i != selected;
            if (GUILayout.Button(selectors[i]))
            {
                return i;
            }

            GUI.enabled = true;
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        return selected;
    }

    private void OnLogMessageReceived(string condition, string stacktrace, LogType type)
    {
        // we do not log and check for substring when not visible, because it is quite costly.
        if (_showIMGUI)
        {
            if (condition.Contains("Autoskill:"))
            {
                // we must skip so much because of the time logging extension
                _logBuffer.Add(condition);
            }
        }
    }


    private void OnReceivingPlayerKDA(IPlayer player, float kda)
    {
        _kdaCache[player] = kda;
    }

    private void OnReceivingAverageKDA(float kda)
    {
        _averageKDA = kda;
    }

    private void OnCommendationSceneLoaded()
    {
        _kdaCache.Clear();
        _averageKDA = 0f;
    }
}