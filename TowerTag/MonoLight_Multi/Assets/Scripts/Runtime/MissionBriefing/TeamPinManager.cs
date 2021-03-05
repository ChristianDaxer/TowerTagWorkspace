using System.Collections.Generic;
using TowerTag;
using TowerTagSOES;
using UnityEngine;
using UnityEngine.UI;

public class TeamPinManager : MonoBehaviour {
    [Header("Player Info")]
    [SerializeField] private TeamID _teamID;

    [Header("Pin Text")]
    [SerializeField] private Text[] _fireTexts;
    [SerializeField] private Text[] _iceTexts;

    [Header("Pin Animations")]
    [SerializeField] private Animator[] _firePinAnimators;
    [SerializeField] private Animator[] _icePinAnimators;

    private readonly Dictionary<GameMode, (string ownTeamText, string enemyTeamText, string iceText, string fireText)> _gameModeTexts =
        new Dictionary<GameMode, (string, string, string, string)> {
        { GameMode.Elimination, ("YOUR TEAM","ENEMY TEAM", "TEAM ICE", "TEAM FIRE")},
        { GameMode.GoalTower, ("YOUR GOALTOWER", "ENEMY GOALTOWER", "GOALTOWER ICE", "GOALTOWER FIRE")},
        { GameMode.DeathMatch, ("YOUR TEAM", "ENEMY TEAM", "TEAM ICE", "TEAM FIRE")}
    };

    private MissionBriefingController _briefingController;
    public Animator[] OwnTeamPinAnimators => _teamID == TeamID.Fire ? _firePinAnimators : _icePinAnimators;
    public Animator[] EnemyTeamPinAnimators => _teamID == TeamID.Fire ? _icePinAnimators : _firePinAnimators;

    private void Awake() {
        IPlayer ownPlayer = PlayerManager.Instance.GetOwnPlayer();
        if (SharedControllerType.IsAdmin || ownPlayer == null) {
            _teamID = TeamID.Neutral;
        }
        else {
            _teamID = ownPlayer.TeamID;
        }

        _briefingController = GetComponentInParent<MissionBriefingController>();

        _fireTexts.ForEach(SetFireTexts);
        _iceTexts.ForEach(SetIceTexts);
    }

    private void SetIceTexts(Text text) {
        if (_teamID == TeamID.Neutral) {
            text.text = GetTeamTextByTeamID(TeamID.Ice);
            return;
        }
        text.text = _teamID == TeamID.Ice ? GetOwnTeamText() : GetEnemyTeamText();
    }

    private void SetFireTexts(Text text) {
        if (_teamID == TeamID.Neutral) {
            text.text = GetTeamTextByTeamID(TeamID.Fire);
            return;
        }
        text.text = _teamID == TeamID.Fire ? GetOwnTeamText() : GetEnemyTeamText();
    }

    private string GetOwnTeamText() {
        return _gameModeTexts[GameManager.Instance.CurrentMatch.GameMode].ownTeamText;
    }

    private string GetEnemyTeamText() {
        return _gameModeTexts[GameManager.Instance.CurrentMatch.GameMode].enemyTeamText;
    }

    private string GetTeamTextByTeamID(TeamID teamID) {
        return teamID == TeamID.Fire ? _gameModeTexts[GameManager.Instance.CurrentMatch.GameMode].fireText
                                     : _gameModeTexts[GameManager.Instance.CurrentMatch.GameMode].iceText;
    }
}
