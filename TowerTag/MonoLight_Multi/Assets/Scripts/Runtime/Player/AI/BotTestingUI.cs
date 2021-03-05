using System.Linq;
using AI;
using TowerTag;
using TowerTagSOES;
using UnityEngine;
using static AI.BotBrain;

public class BotTestingUI : MonoBehaviour {
    [SerializeField] private bool _visible;

    [SerializeField] private KeyCode _keyCode = KeyCode.B;

    //[SerializeField] private List<string> _botNames = new List<string>();
    [SerializeField] private Rect _viewRect = new Rect(1410, 220, 500, 500);
    [SerializeField] private Color _fireColor = new Color(255, 162, 0, 255);
    [SerializeField] private Color _iceColor = new Color(0, 255, 255, 255);

    private static PlayerManager PlayerManager => PlayerManager.Instance;
    private IPlayer[] _aiPlayers;
    private int _numberOfBots;
    private PlayerNetworkEventHandler _playerNetworkEventHandler;


    private void Start() {
        if (!Debug.isDebugBuild)
            enabled = false;
        if (!SharedControllerType.IsAdmin)
            enabled = false;

        SetAIPlayers();
    }


    private void SetAIPlayers() {
        PlayerManager.Instance.GetAllAIPlayers(out var players, out var count);
        _aiPlayers = players.Take(count).ToArray();
        _numberOfBots = _aiPlayers.Length;
    }

    private void Update() {
        if (Input.GetKeyDown(_keyCode))
            _visible = !_visible;
        if (_aiPlayers.Length != PlayerManager.GetAIPlayerCount()) //number of bots changed
        {
            SetAIPlayers();
        }
    }

    private void OnGUI() {
        if (!_visible)
            return;

        #region GUISTYLES

        var botIceStyle = new GUIStyle();
        var botFireStyle = new GUIStyle();
        botIceStyle.richText = true;
        botIceStyle.fontStyle = FontStyle.Bold;
        botIceStyle.normal.textColor = _iceColor;
        botFireStyle.richText = true;
        botFireStyle.fontStyle = FontStyle.Bold;
        botFireStyle.normal.textColor = _fireColor;

        #endregion

        GUI.Box(_viewRect, "");
        GUILayout.BeginArea(_viewRect);
        GUILayout.Label("****** AI Bot TestUI (toggle with " + _keyCode + ") ******\n\n" + "Number of Bots: " +
                        _numberOfBots + "\n");

        if (SharedControllerType.IsAdmin) //only if operator
        {
            GUILayout.BeginVertical();
            if (_aiPlayers.Length > 0) {
                //if (GUILayout.Button("Set Bot Names", GUILayout.Width(110))) {

                //    for (int i = 0; i < _AIPlayers.Length; i++)
                //    {
                //        AdminController.Instance.SetPlayerName(_AIPlayers[i], "[BOT] " + _botNames[i]);
                //    }

                //}

                GUILayout.Label("\n-------------- TEAM FIRE --------------\n", botFireStyle);


                foreach (IPlayer bot in _aiPlayers) {
                    GUILayout.BeginHorizontal();
                    if (bot.TeamID == 0) {
                        DrawBotControls(bot, botFireStyle);
                        GUILayout.Label("> " + bot?.GameObject.transform.GetChild(1).GetComponent<BotBrain>().Difficulty + " \n");
                    }

                    GUILayout.EndHorizontal();
                }

                GUILayout.Label("-------------- TEAM ICE --------------\n", botIceStyle);

                foreach (IPlayer bot in _aiPlayers) {
                    GUILayout.BeginHorizontal();
                    if (bot.TeamID == TeamID.Ice) {
                        DrawBotControls(bot, botIceStyle);
                        GUILayout.Label("> " + bot?.GameObject.transform.GetChild(1).GetComponent<BotBrain>()?.Difficulty + " \n");
                    }

                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndVertical();
        }

        GUILayout.EndArea();
    }


    private void DrawBotControls(IPlayer bot, GUIStyle nameStyle) {
        GUILayout.Label("> " + bot.PlayerName, nameStyle);
        GUILayout.Label("> " + bot.SelectedAIParameters + "\n");
        if (bot.GameObject != null) _playerNetworkEventHandler = bot.GameObject.GetComponent<PlayerNetworkEventHandler>();
        if (GUILayout.Button("Easy")) {
            _playerNetworkEventHandler.UpdateAIParameters(BotDifficulty.Easy);
            bot.SelectedAIParameters = "Easy";
        }

        if (GUILayout.Button("Medium")) {
            _playerNetworkEventHandler.UpdateAIParameters(BotDifficulty.Medium);
            bot.SelectedAIParameters = "Medium";
        }

        if (GUILayout.Button("Hard")) {
            _playerNetworkEventHandler.UpdateAIParameters(BotDifficulty.Hard);
            bot.SelectedAIParameters = "Hard";
        }
    }
}